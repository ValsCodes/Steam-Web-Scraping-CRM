---
name: codebase-discovery
description: Discover SteamApp architecture, symbols, callers, routes, dependencies, and change impact through scoped source searches and project files. Use for repository discovery; skip broad discovery for an already identified, locally understandable target.
---

# SteamApp Codebase Discovery

## Choose the smallest inspection

Inspect source directly when the user names the exact file, class, method, endpoint, or component and the task is locally understandable.

Codebase-memory is temporarily disabled for this project. Do not call its tools, start its server, or create/update its index unless the user explicitly re-enables it.

## Procedure

1. Start from the named file, feature, error, or changed files.
2. Use scoped rg or rg --files searches to locate symbols, callers, routes, and configuration. If unavailable, use Get-ChildItem and Select-String within the relevant directories.
3. Read affected source and project files. Expand to callers, dependencies, or neighboring implementations only when correctness requires it.
4. For current-diff analysis, inspect git diff, git diff --cached, and relevant untracked files. Compare against an existing base ref only when the requested scope needs it.
5. Verify implementation-sensitive, negative, exhaustive, and security-sensitive conclusions against source and project files. Do not claim exhaustive coverage from a narrow search.

## SteamApp orientation

Client sources are under SteamApp.Client/src/app; server projects and .NET tests are under SteamApp.Server. For architectural ownership or conventions conclusions, read the relevant [C# conventions](../csharp-conventions/SKILL.md) or [UI conventions](../ui-conventions/SKILL.md). Treat orientation as navigation, not proof; verify claims against affected project files, source, configuration, and tests.
