---
type: "query"
date: "2026-08-15T06:31:17.146324+00:00"
question: "Inside Automated Check presets remain on Loading presets, failed product checks have no readable trace in Automated Check History, preset creation may return 429, and ManualChecksController should not use HandleAsync."
contributor: "graphify"
outcome: "useful"
source_nodes: ["ManualCheckSetupDialogComponent", "ManualCheckHistoryDialogComponent", "ManualChecksController", "ManualCheckDataService", "ManualCheckExecutionService"]
---

# Q: Inside Automated Check presets remain on Loading presets, failed product checks have no readable trace in Automated Check History, preset creation may return 429, and ManualChecksController should not use HandleAsync.

## Answer

Expanded from the original request via graph vocabulary: [automated, check, preset, history, product, error, failed, logs, endpoint, handler, result, rate]. The graph led to ManualCheckSetupDialogComponent, ManualCheckHistoryDialogComponent, ManualChecksController, ManualCheckDataService, and ManualCheckExecutionService. Implemented deterministic preset loading with retry, clearer save-and-start UX, durable run and product failure trace metadata exposed in history, readable history panels, explicit 429 ProblemDetails and logging, and endpoint-local ManualCheckRequestException handling with HandleAsync removed. Angular build, 19 focused client tests, and 193 server tests pass.

## Outcome

- Signal: useful

## Source Nodes

- ManualCheckSetupDialogComponent
- ManualCheckHistoryDialogComponent
- ManualChecksController
- ManualCheckDataService
- ManualCheckExecutionService