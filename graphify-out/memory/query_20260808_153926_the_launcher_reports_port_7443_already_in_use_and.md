---
type: "query"
date: "2026-08-08T15:39:26.549524+00:00"
question: "The launcher reports port 7443 already in use and it didn't open the browser automatically"
contributor: "graphify"
outcome: "useful"
source_nodes: ["local-release", "start", "browser"]
---

# Q: The launcher reports port 7443 already in use and it didn't open the browser automatically

## Answer

Expanded from graph vocab: [angular, api, browser, client, configuration, environment, launch, launcher, open, port, server, start]. Updated Start-SteamApp-Local-Release.bat to reuse healthy services on ports 7443/4200, reject unrelated listeners, wait for both endpoints, and explicitly open http://localhost:4200 in the default browser from the main launcher. Verified repeated-click reuse and the normal browser-opening path.

## Outcome

- Signal: useful

## Source Nodes

- local-release
- start
- browser