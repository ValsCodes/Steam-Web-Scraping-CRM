"""Validate the repository's restricted agent metadata format without executing content."""
import argparse
import json
import re
import sys
from pathlib import Path


def validate(root):
    root = Path(root).resolve()
    errors = []
    skills = root / ".agents/skills"

    def fail(message):
        errors.append(message)

    files = sorted(skills.glob("*/SKILL.md"))
    names = set()
    for path in files:
        label = path.relative_to(root).as_posix()
        if not path.resolve().is_relative_to(root):
            fail("Skill escapes repository: " + label)
            continue
        text = path.read_text(encoding="utf-8")
        header = re.match(r"\A---\n(.*?)\n---\n", text, re.S)
        fields = {}
        if not header:
            fail("Invalid frontmatter: " + label)
            continue
        # Local skills deliberately support only plain name/description scalars.
        for line in header[1].splitlines():
            key, separator, value = line.partition(":")
            if not separator or key not in {"name", "description"} or key in fields:
                fail("Invalid/duplicate frontmatter field: " + label)
            fields[key] = value.strip()
        name = fields.get("name", "")
        description = fields.get("description", "")
        if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", name) or len(name) > 64:
            fail("Invalid skill name: " + label)
        if name != path.parent.name or name in names:
            fail("Duplicate/mismatched skill name: " + label)
        names.add(name)
        if not description or len(description) > 1024 or any(c in description for c in "<>"):
            fail("Invalid skill description: " + label)
        if description.startswith(("[", "{", "|", ">")) or ": " in description or " #" in description:
            fail("Description must be a plain single-line scalar: " + label)
        metadata = path.parent / "agents/openai.yaml"
        if metadata.exists():
            if not metadata.resolve().is_relative_to(root):
                fail("Metadata escapes repository: " + label)
                continue
            lines = metadata.read_text(encoding="utf-8").splitlines()
            values = {}
            if not lines or lines[0] != "interface:":
                fail("Invalid UI metadata root: " + label)
            for line in lines[1:]:
                match = re.fullmatch(r'  (display_name|short_description|default_prompt): (".*")', line)
                if not match or match[1] in values:
                    fail("Invalid/duplicate UI metadata field: " + label)
                    continue
                try:
                    values[match[1]] = json.loads(match[2])
                except ValueError:
                    fail("Invalid UI metadata string: " + label)
            if set(values) != {"display_name", "short_description", "default_prompt"}:
                fail("Missing UI metadata field: " + label)
            if "$" + name not in values.get("default_prompt", ""):
                fail("UI prompt does not name its skill: " + label)

    for name in {"pr-review", "security-review", "unit-tests"} - names:
        fail("Missing workflow: " + name)
    agents = root / "AGENTS.md"
    if not agents.exists():
        fail("Missing AGENTS.md")
    else:
        for line in agents.read_text(encoding="utf-8").splitlines():
            if line.startswith("|"):
                columns = [column.strip().strip(chr(96)) for column in line.strip("|").split("|")]
                if len(columns) == 2 and re.fullmatch(r"[a-z]+(?:-[a-z]+)+", columns[1]):
                    if columns[1] not in names:
                        fail("Trigger references missing skill: " + columns[1])

    documents = [agents, *skills.rglob("*.md")]
    for path in documents:
        if not path.exists():
            continue
        label = path.relative_to(root).as_posix()
        if not path.resolve().is_relative_to(root):
            fail("Policy escapes repository: " + label)
            continue
        text = path.read_text(encoding="utf-8")
        if ".agents/directives/" in text:
            fail("Retired directive reference: " + label)
        for target in re.findall(r"\[[^\]]+\]\(([^)]+)\)", text):
            target = target.split("#", 1)[0]
            if not target or re.match(r"https?://|mailto:", target):
                continue
            resolved = (path.parent / target).resolve()
            if not resolved.is_relative_to(root):
                fail("Reference escapes repository: " + label)
            elif not resolved.exists():
                fail("Missing reference: " + label + " -> " + target)
        for target in re.findall(chr(96) + r"(\.agents/[^" + chr(96) + r"]+\.md)" + chr(96), text):
            resolved = (root / target).resolve()
            if not resolved.is_relative_to(root) or not resolved.is_file():
                fail("Missing/unsafe explicit reference: " + label)

    if list((root / ".agents/directives").glob("*.md")):
        fail("Retired directive files remain")
    if (skills / "pr-review-refactor/SKILL.md").exists():
        fail("Separate refactor workflow remains")
    hook_path = root / ".codex/hooks.json"
    try:
        hooks = json.loads(hook_path.read_text(encoding="utf-8"))["hooks"]
        if not isinstance(hooks, dict):
            raise ValueError()
        handlers = hooks["SessionStart"]
        if not isinstance(handlers, list) or not handlers:
            raise ValueError()
        if any(not isinstance(group, dict) or not isinstance(group.get("hooks"), list) for group in handlers):
            raise ValueError()
        commands = [handler for group in handlers for handler in group["hooks"]]
        if any(not isinstance(handler, dict) for handler in commands):
            raise ValueError()
        if not any(handler.get("type") == "command"
                   and "scripts/validate_agent_configuration.py" in handler.get("command", "")
                   for handler in commands):
            fail("Missing session-start configuration check")
        for handler in commands:
            if handler.get("type") != "command" or not isinstance(handler.get("command"), str):
                fail("Unsupported session-start handler")
            timeout = handler.get("timeout", 0)
            if isinstance(timeout, bool) or not isinstance(timeout, (int, float)) or not 0 < timeout <= 30:
                fail("Session-start check needs a bounded timeout")
    except (OSError, ValueError, KeyError, TypeError):
        fail("Invalid/missing hook configuration")
    return errors, len(files)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--hook", action="store_true")
    args = parser.parse_args()
    try:
        errors, count = validate(args.root)
    except (OSError, UnicodeError):
        errors, count = ["Could not read agent configuration"], 0
    if args.hook:
        message = ("Agent configuration check failed: " + "; ".join(errors)) if errors else (
            "Agent configuration passed. Follow AGENTS.md, select applicable skills, "
            "and use validation proportionate to the changed behavior. Reading a policy does not prove compliance.")
        print(json.dumps({"systemMessage": message} if errors else {
            "hookSpecificOutput": {"hookEventName": "SessionStart", "additionalContext": message}}))
    else:
        print(f"Agent configuration: {count} skills; {len(errors)} errors.")
        for error in errors:
            print("FAIL: " + error)
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
