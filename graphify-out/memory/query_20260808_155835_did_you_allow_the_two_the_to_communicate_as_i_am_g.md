---
type: "query"
date: "2026-08-08T15:58:35.167223+00:00"
question: "did you allow the two the to communicate as I am getting Invalid user credentials. from the server"
contributor: "graphify"
outcome: "useful"
source_nodes: ["AuthController", ".Login()", "local-release", "Program.cs"]
---

# Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server

## Answer

Expanded from original query via vocab: [auth, authentication, client, credentials, database, identity, local, login, password, register, server, user]. The local-release client targets https://localhost:7443, and AuthController.Login returns Invalid user credentials when the Identity user is absent or its password check fails. LocalDB inspection confirmed db_steam_app2 contains 1 user while the launcher-created SteamAppDb contains 0. The launcher was corrected to use existing db_steam_app2 and fail if it is absent.

## Outcome

- Signal: useful

## Source Nodes

- AuthController
- .Login()
- local-release
- Program.cs