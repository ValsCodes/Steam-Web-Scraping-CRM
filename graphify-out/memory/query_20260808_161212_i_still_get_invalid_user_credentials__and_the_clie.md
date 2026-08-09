---
type: "query"
date: "2026-08-08T16:12:12.385153+00:00"
question: "I still get Invalid user credentials. and the client doesn't open upon clicking the bat"
contributor: "graphify"
outcome: "useful"
source_nodes: ["browser", "local-release", ".Login()", "AuthController", "ApplicationUser"]
---

# Q: I still get Invalid user credentials. and the client doesn't open upon clicking the bat

## Answer

Expanded from original query via vocab: [authentication, browser, client, credentials, database, identity, launcher, local, login, password, server, user]. Reproduction found the LF-only batch could not resolve labels and exited before the client/browser flow. Converting it to CRLF fixed label dispatch; escaping the (localdb) status text fixed a second cmd parser error; the launcher then exited 0 and invoked the browser. db_steam_app2 contains one unlocked Identity V3 account with normalized identifiers, so remaining Invalid user credentials means the supplied identifier/password does not match that existing record, not a client/server communication failure.

## Outcome

- Signal: useful

## Source Nodes

- browser
- local-release
- .Login()
- AuthController
- ApplicationUser