# Manual-check preset database workflow

Use this reference when a request creates, installs, updates, or verifies a manual-check preset directly in SteamApp's database.

## Authoritative inputs

- Use `docs/inspect-game-catalog-and-presets.sql` to inspect the live schema, operators, ownership, catalog mappings, existing presets, and expression integrity.
- Use `docs/game-catalog-and-presets-template.sql` as an implementation starting point, not as an authority for current limits.
- Check `SteamApp.Server/SteamApp.WebAPI/Services/ManualCheckDataService.cs` for the current preset validation and normalization behavior before generating SQL.
- When the criteria originate in an `.xlsx` report, use the spreadsheet skill to read the exact relevant sheet and preserve the user's requested tier/category boundaries.

## Relevant model

A preset belongs to a game and may belong to an item group. It does not directly belong to a Game URL. Resolve the intended Game URL to confirm the correct owned game and catalog context, then insert the preset for that game.

The primary preset tables are:

- `dbo.manual_check_preset`
- `dbo.manual_check_criterion`
- `dbo.manual_check_condition_operator`
- `dbo.game`
- `dbo.game_url`
- `dbo.item_group`

Resolve actual columns and constraints from the live database before writing.

## Expression invariants

- Criteria use contiguous, zero-based `sort_order` values.
- Criterion zero has `condition_operator_id = NULL`.
- Every later criterion resolves its operator ID by the operator's stored name; do not hardcode an assumed numeric operator ID.
- At least one of `name_contains` or `value_contains` is non-empty.
- When both fields are present on one criterion, the application requires both to match the same Steam asset description.
- `open_group_count` and `close_group_count` encode parentheses. Running group depth must never be negative, final depth must be zero, and current application/database limits must be respected.
- For a simple list of acceptable effects, use a flat OR expression: the first criterion has no operator and every later criterion uses `OR`; groups are unnecessary.
- Preserve the spreadsheet's requested tier exactly. Do not silently include lower tiers, alternate spellings, or inferred effects.

## Creation workflow

1. Inspect the target game, Game URL, optional item group, supported operators, and any existing preset with the intended name.
2. Confirm the resolved game and URL are owned by the intended user without exposing the user ID in output.
3. Convert the approved effect/product list into ordered criteria and validate duplicates, empty values, expression balance, and the current criterion/group limits.
4. Adapt the reusable template or create a narrowly scoped script with placeholders for private identifiers.
5. Make the script idempotent:
   - insert the preset only when absent;
   - resolve the inserted/existing preset ID inside the transaction;
   - if criteria already exist, compare the complete ordered definition in both directions;
   - insert criteria only for a new empty preset;
   - throw on any conflicting existing definition.
6. Execute through `sqlcmd` with `-S`, `-d`, `-E`, and `-b` against the explicitly authorized local target.
7. Read back the preset and every criterion ordered by `sort_order`.

## Verification

Verify all of the following after commit:

- exactly one intended preset resolved;
- it belongs to the correct owned game and optional item group;
- listing/cooldown settings match the requested definition;
- criterion count equals the approved source list;
- every expected source value appears exactly once unless duplicates were explicitly intended;
- operators and sort order form the intended expression;
- parentheses are balanced and within current limits;
- no unrelated preset or criterion rows changed.

If verification fails, do not patch the live rows ad hoc. Diagnose the mismatch and use a new explicit transaction whose affected rows and recovery plan are clear.
