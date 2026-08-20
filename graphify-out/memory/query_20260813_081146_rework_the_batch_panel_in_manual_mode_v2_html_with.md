---
type: "query"
date: "2026-08-13T08:11:46.494168+00:00"
question: "Rework the Batch Panel in manual-mode-v2.html with previous and next batch buttons, defaults of 1, and no Run Batch increment"
contributor: "graphify"
outcome: "useful"
source_nodes: [".startBatchButtonClicked()", "ManualModeV2"]
---

# Q: Rework the Batch Panel in manual-mode-v2.html with previous and next batch buttons, defaults of 1, and no Run Batch increment

## Answer

Expanded from the original request via graph vocabulary: [manual, mode, batch, run, previous, next, size, current, item, execute]. ManualModeV2.startBatchButtonClicked now runs from a fixed normalized current item without writing back the loop counter; previous and next handlers move by the normalized batch size and then run; reset and source changes restore current item and batch size to 1. The Batch Panel template exposes Run Previous Batch and Run Next Batch controls.

## Outcome

- Signal: useful

## Source Nodes

- .startBatchButtonClicked()
- ManualModeV2