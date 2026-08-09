---
type: "query"
date: "2026-08-09T12:16:53.066525+00:00"
question: "Profile page only renders fetched data after a manual click"
contributor: "graphify"
outcome: "useful"
source_nodes: ["ProfilePage", ".loadProfile()", "ChangeDetectorRef"]
---

# Q: Profile page only renders fetched data after a manual click

## Answer

Expanded from original query via graph vocabulary: profile, page, users, fetch, results, detect, change, update, component, service, async, view. Angular 21 bootstraps zoneless change detection by default, so manual RxJS subscription callbacks that mutate ordinary component fields need to notify Angular. ProfilePage now calls ChangeDetectorRef.markForCheck() from each HTTP finalize callback, including loadProfile, so fetched form data and loading, success, and error state render without another user event. The Angular development build passes.

## Outcome

- Signal: useful

## Source Nodes

- ProfilePage
- .loadProfile()
- ChangeDetectorRef