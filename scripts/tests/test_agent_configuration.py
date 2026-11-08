"""Behavior tests for policy references, metadata, and hook configuration."""
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location(
    "agent_configuration", Path(__file__).resolve().parents[1] / "validate_agent_configuration.py")
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class AgentConfigurationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.write("AGENTS.md", "| Trigger | Required skill |\n| Review | pr-review |\n")
        for name in ["pr-review", "security-review", "unit-tests"]:
            self.write(f".agents/skills/{name}/SKILL.md",
                       f"---\nname: {name}\ndescription: Review relevant behavior.\n---\n# Workflow\n")
        self.write(".codex/hooks.json", json.dumps({"hooks": {"SessionStart": [{"hooks": [{
            "type": "command", "command": "python scripts/validate_agent_configuration.py --hook",
            "timeout": 15}]}]}}))

    def write(self, relative, content):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8")

    def errors(self):
        return module.validate(self.root)[0]

    def test_valid_configuration_passes(self):
        self.assertEqual([], self.errors())

    def test_missing_reference_fails(self):
        with (self.root / "AGENTS.md").open("a", encoding="utf-8") as file:
            file.write("[Required policy](.agents/skills/missing/SKILL.md)\n")
        self.assertTrue(any("Missing reference" in error for error in self.errors()))

    def test_trigger_to_missing_skill_fails(self):
        self.write("AGENTS.md", "| Review | missing-skill |\n")
        self.assertIn("Trigger references missing skill: missing-skill", self.errors())

    def test_duplicate_frontmatter_field_fails(self):
        self.write(".agents/skills/pr-review/SKILL.md",
                   "---\nname: pr-review\nname: other\ndescription: Review behavior.\n---\n")
        self.assertTrue(any("duplicate frontmatter" in error for error in self.errors()))

    def test_retired_directive_reference_fails(self):
        self.write("AGENTS.md", "Read .agents/directives/unit-tests.md\n")
        self.assertTrue(any("Retired directive reference" in error for error in self.errors()))

    def test_malformed_hook_fails(self):
        self.write(".codex/hooks.json", "{invalid")
        self.assertIn("Invalid/missing hook configuration", self.errors())

    def test_nonobject_hook_handler_fails_without_crashing(self):
        self.write(".codex/hooks.json", json.dumps({"hooks": {"SessionStart": [{"hooks": ["invalid"]}]}}))
        self.assertIn("Invalid/missing hook configuration", self.errors())

    def test_unbounded_hook_fails(self):
        self.write(".codex/hooks.json", json.dumps({"hooks": {"SessionStart": [{"hooks": [{
            "type": "command", "command": "python scripts/validate_agent_configuration.py",
            "timeout": 600}]}]}}))
        self.assertIn("Session-start check needs a bounded timeout", self.errors())

    def test_reference_cannot_escape_repository(self):
        self.write("AGENTS.md", "[Outside](../outside-policy.md)\n")
        self.assertIn("Reference escapes repository: AGENTS.md", self.errors())


if __name__ == "__main__":
    unittest.main()
