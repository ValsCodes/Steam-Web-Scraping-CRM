---
type: "query"
date: "2026-08-08T16:38:13.206035+00:00"
question: "make it not reuse it should restart the currently active one"
contributor: "graphify"
outcome: "useful"
source_nodes: ["local-release", "Program.cs", "browser"]
---

# Q: make it not reuse it should restart the currently active one

## Answer

Expanded from the authentication investigation via graph vocab: [account, auth, credentials, database, hash, identity, local, login, password, reset, user]. The launcher now stops verified SteamApp listeners on ports 7443 and 4200 before starting fresh Release API/client processes. It validates process path or command markers before termination, keeps visible cmd windows, passes db_steam_app2 explicitly, fixes the Angular conditional so npm start always runs, waits for both URLs, and then opens the browser. Verified API and client HTTP 200 after restart; API startup logs opened db_steam_app2 on (localdb)\\MSSQLLocalDB.

## Outcome

- Signal: useful

## Source Nodes

- local-release
- Program.cs
- browser