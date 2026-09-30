---
name: steamapp-database-operations
description: Connect to and safely inspect or modify SteamApp SQL Server or LocalDB data for preset creation, catalog inserts, relation changes, corrections, and verification. Use for direct database queries or mutations; do not use for EF migrations or application-code-only changes.
---

# SteamApp database operations

Use the repository's SQL Server schema and `sqlcmd` for direct local database work. Keep source files and the live schema authoritative; old SQL comments and examples may lag current application validation.

## Authority and scope

- Read-only inspection is allowed when it supports the user's request.
- Execute inserts, updates, deletes, or stored procedure calls only when the user explicitly asks to change the database. Asking to create or install a preset directly authorizes the required preset rows, but not unrelated cleanup or catalog changes.
- Confirm the target environment. Default to the explicitly supplied local development database; never infer authorization for a remote, shared, staging, or production database.
- Preserve unrelated rows. If the exact database, owner, game, URL, preset, or affected-row scope cannot be resolved uniquely, stop before mutation and ask for the missing choice.

## Protect local configuration and identity

- Never print or repeat connection strings, credentials, tokens, user IDs, or other private configuration in commentary, reports, logs, or tracked documentation.
- Do not hardcode live connection strings or user IDs into tracked SQL. Use placeholders, validated `sqlcmd` variables, or values already stored in approved local configuration.
- Prefer Windows integrated authentication when the selected connection uses `Trusted_Connection=True` or `Integrated Security=True`.
- Use task-specific PowerShell variables such as `$steamDbServer` and `$steamDbName`; never repurpose `$HOME`, `$home`, or `$CODEX_HOME`.

## Connect with the installed tools

This workspace normally uses SQL Server LocalDB and has `sqlcmd` available. Verify rather than assuming:

```powershell
Get-Command sqlcmd -ErrorAction Stop
sqllocaldb info
```

Resolve the server and database from the connection information authorized for the task or from the application's local configuration. Keep the full connection string out of output. Test the exact target before doing any write:

```powershell
$steamDbServer = '<resolved-server>'
$steamDbName = '<resolved-database>'
sqlcmd -S $steamDbServer -d $steamDbName -E -b -Q "SET NOCOUNT ON; SELECT connection_ok = 1;"
```

Use `-b` for every execution so SQL errors produce a failing exit code. Add `-r 1` when error-stream separation helps diagnostics. Do not connect without `-d`; an accidental default-database write is unacceptable.

For multi-statement work, create or adapt a reviewable `.sql` file and execute it with:

```powershell
sqlcmd -S $steamDbServer -d $steamDbName -E -b -i '<script-path>'
```

Use `-v Name="value"` only for values whose expected shape has first been validated. Never interpolate untrusted text into SQL or a shell command. SQL string literals must be Unicode (`N'...'`) and escape apostrophes by doubling them.

## Inspect before mutation

Before writing:

1. Verify the connection points to the intended database.
2. Inspect the affected table columns, keys, foreign keys, defaults, check constraints, and triggers from the live database.
3. Resolve target rows by ownership plus stable natural keys. Do not select a row by display name alone if another user or game can have the same name.
4. Query the exact rows that will change and determine the expected affected-row count.
5. Check current server-side validation constants and database constraints. Do not assume a limit copied into an older SQL template is current.

For catalog and preset discovery, prefer the read-only [inspection script](../../../docs/inspect-game-catalog-and-presets.sql). Set its in-script filters in a private working copy when filtering would expose a user ID in a tracked file.

## Write transactionally and idempotently

- Start scripts with `SET NOCOUNT ON; SET XACT_ABORT ON;`.
- Wrap related changes in `BEGIN TRY`, `BEGIN TRANSACTION`, `COMMIT`, and a `CATCH` block that rolls back whenever `XACT_STATE() <> 0`, then `THROW`.
- Validate ownership and all foreign-key targets again inside the transaction.
- Use `UPDLOCK, HOLDLOCK` when resolving rows for an idempotent insert that could race another writer.
- Prefer insert-if-missing behavior. On rerun, compare the complete existing definition and succeed only if it is equivalent; fail rather than silently replacing a different definition.
- For requested updates or deletes, use exact predicates and explicit expected-row-count guards. Capture the before-state needed to explain or reverse the change.
- Never use broad `DELETE`, `UPDATE`, `TRUNCATE`, database drops, or schema changes unless the user explicitly requests that exact destructive scope.
- Do not use an EF migration for ordinary data insertion. Use migrations only when the user requests a schema/model change and the applicable C# workflow is followed.

## Verify and report

After a successful write, query the committed rows through a fresh statement or connection. Verify row counts, ownership, foreign keys, ordered child rows, and any domain invariants. For a failed script, verify that the transaction rolled back before retrying.

Report the operation type, logical target, affected row counts, verification result, and whether rollback/recovery is available. Do not include the connection string, database credentials, raw user ID, or unrelated row contents.

For manual-check preset creation or modification, read [preset-creation.md](references/preset-creation.md) before inspecting or writing preset rows.
