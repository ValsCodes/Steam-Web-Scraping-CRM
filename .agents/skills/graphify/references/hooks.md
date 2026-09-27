# graphify reference: commit hook and native CLAUDE.md integration

PowerShell command bodies follow [runtime.md](runtime.md); execute them through `pwsh -NoProfile` as required by `AGENTS.md`.

Load this when the user asked to install the post-commit hook or wire graphify into a project's CLAUDE.md.

## For git commit hook

Install a post-commit hook that auto-rebuilds the graph after every commit. No background process needed - triggers once per commit, works with any editor.

```powershell
graphify hook install    # install
graphify hook uninstall  # remove
graphify hook status     # check
```

After every `git commit`, the hook detects which code files changed (via `git diff HEAD~1`), re-runs AST extraction on those files, and rebuilds `graph.json` and `GRAPH_REPORT.md`. Doc/image changes are ignored by the hook - run `/graphify --update` manually for those.

If a post-commit hook already exists, graphify appends to it rather than replacing it.

---

## For native CLAUDE.md integration

Run once per project to make graphify always-on in Claude Code sessions:

```powershell
graphify claude install
```

This writes a `## graphify` section to the local `CLAUDE.md` that instructs Claude to check the graph before answering codebase questions and rebuild it after code changes. No manual `/graphify` needed in future sessions.

```powershell
graphify claude uninstall  # remove the section
```

The CLI may generate POSIX shell content inside a Git hook because Git owns its execution. Invoke installation/status commands from PowerShell; do not execute that hook body directly in PowerShell. Install hooks or change CLAUDE.md only when explicitly requested. Preserve SteamApp codebase-discovery-first routing rather than enabling Graphify for every ordinary question.
