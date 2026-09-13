---
name: bulk-relations
description: Implement SteamApp join-table selection or multi-record relation mutations using one bulk API operation and atomic EF persistence. Use for relation forms and batch relation writes; not for unrelated single-record edits.
---

# SteamApp bulk relation writes

- A multi-record relation selection must use one batch HTTP request, not one create/delete request per record or a client-side forkJoin of individual writes.
- Extend the existing relation endpoint and client service. Make clear whether the request is a full desired selection or explicit additions/removals; an empty full selection removes the scoped relations.
- Validate the entire batch before writing: IDs, authenticated ownership of both sides, game/domain compatibility, deduplication, and a bounded request size. Never trust IDs just because the client filtered them.
- Use the existing EFCore.BulkExtensions provider packages and BulkInsertAsync/BulkDeleteAsync for relation collections. Preserve unchanged rows and their stock, metadata, and history rather than deleting/recreating every relation.
- Put all related inserts and deletes in one explicit database transaction. Commit only when the entire operation succeeds; roll back on failure or cancellation. Bulk operations alone do not make a multi-operation workflow atomic.
- Run the transaction within EF's execution strategy when retries are enabled. Keep one context per operation and await its I/O sequentially. Request-bound Minimal APIs may use their DI-supplied scoped ApplicationDbContext; reusable services use IDbContextFactory.
- Preserve constraints and trigger behavior when configuring SQL Server bulk copy. Do not disable API rate limits to accommodate request fan-out.
- Retain single-record methods only for genuine single-record callers or compatibility; selection forms must use the bulk method.
- Verify large batches, duplicates, invalid/foreign IDs, clearing a selection, preserved unchanged-row data, and rollback after a failure occurring after earlier writes. Use relational tests; InMemory cannot prove transaction rollback or bulk-provider behavior.
- State the transaction boundary explicitly. Separate HTTP saves of a parent, products, or pixels are not one transaction. Make retries safe if a parent was already created.

Current example: PUT /api/game-url-products/{gameUrlId}/bulk accepts the full ProductIds selection and atomically reconciles that Game URL's product relations.
