---
name: graphify
description: Query and maintain SteamApp's persistent Graphify knowledge graph for architecture, communities, god nodes, paths, reports, wiki navigation, or visualization. Use when the user invokes /graphify, explicitly requests Graphify analysis, or another repository workflow requires Graphify.
---

# SteamApp Graphify

## Use the existing graph

When `graphify-out/graph.json` exists:

- Run `graphify query "<question>"` for codebase questions.
- Run `graphify path "<A>" "<B>"` for a relationship.
- Run `graphify explain "<concept>"` for a focused concept.
- Use `graphify-out/wiki/index.md` for broad navigation when present.
- Read `graphify-out/GRAPH_REPORT.md` only for broad architecture/community work or when scoped commands are insufficient.

Follow the installed Graphify skill's vocabulary expansion, source-location, honesty, reflection, and result-saving procedure.

## Boundaries

- Treat source and project files as authoritative.
- Treat `graphify-out/` as generated analysis.
- Do not use Graphify and codebase-memory-mcp for the same routine task unless one is insufficient or cross-validation is requested.
- Do not rebuild the graph for a normal question when the existing graph can answer it.
- Dirty Graphify files are expected and do not invalidate the graph by themselves.

## Updates

After source-code changes, run `graphify update .` as required by the repository `AGENTS.md`. Do not update for read-only review or configuration-only edits unless the user explicitly requests refreshed graph output.

