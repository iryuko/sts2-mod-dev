# Context / SL state audit (2026-07-23)

## Scope

This audit covers the reported `UnattendedPiano` save-and-load failure and a source-wide scan for the same class of context-lifetime bug.

The reference game assembly is STS2 `v0.107.1` (`commit 59260271`). The current local `godot.log` predates the installed build, so the conclusions below are code- and decompile-backed; the exact runtime exception, if any, still needs a fresh reproduction log.

## Confirmed root cause: `UnattendedPiano` shares mutable state

`UnattendedPiano` declares `_remainingShadows` as a constructor-initialized `List<CardModel>` and removes entries from it as the player receives the three Shadow cards.

Current STS2 behavior:

- `EventRoom` stores only the canonical event ID when saving a normal event room.
- On load, `EventSynchronizer.BeginEvent(...)` creates a new mutable event from the canonical model.
- `AbstractModel.MutableClone()` starts with `MemberwiseClone()`.
- `EventModel.DeepCloneFields()` clones base dynamic variables but does not clone subclass collections.

Therefore the mutable event and the canonical `UnattendedPiano` share the same `_remainingShadows` list. Finishing the three-memory route empties canonical process-wide state. Saving and loading rebuilds the event UI, but the new mutable event receives the already-empty list, so the piano route cannot restart normally.

This is not primarily caused by the visited-event tag. Base-game `ActModel.PullNextEvent(...)` already calls `RunState.AddVisitedEvent(...)`, and `RoomSet.EnsureNextEventIsValid(...)` already rejects IDs in `RunState.VisitedEventIds`. Reloading the current room bypasses candidate selection and restores the saved event ID directly.

## Implemented correction

The fix follows the base-game mutable-model pattern used by events such as `SlipperyBridge`:

- `_remainingShadows` now starts as `null` on the canonical event.
- `RemainingShadows` calls `AssertMutable()` and lazily creates the three-card list only on the active mutable event.
- The redundant `AfterEventStarted()` visited-event mutation and private `_visitedEventIds` reflection helpers were removed; normal event selection remains responsible for visit tracking.
- Event music now stops from `OnEventFinished()`, covering normal choices and base event cleanup.

Required regression:

1. Enter `UnattendedPiano` and take all three Shadow cards.
2. Save and quit before leaving the event, then continue without restarting the process.
3. Choose the piano route again and confirm all three rewards can be obtained normally.
4. Confirm the restored deck starts from the save state and receives no duplicate Shadow card per step.
5. Repeat after a full process restart and on both Mac and Windows.

## Source-wide findings

### High: private-field reflection is initialized before local error handling

`ModSupport.VisitedEventIdsRef`, `CardLibraryTogawasakikoPoolPatch`, and `TogawaEventRoomPatches` create `AccessTools.FieldRefAccess(...)` delegates in static field initializers. If a game update renames or changes one of those private fields, the type initializer can fail before the method-level `try/catch` runs. This is the same broad failure mode as earlier main-menu mod initialization errors.

The visited-event reflection is already unnecessary on `v0.107.1`; public APIs exist. The card-library and event-room patches should resolve private members lazily and fail closed per feature, or be removed where an original public flow can replace them.

### Medium: `MagneticForceHellWargodPower` owns a shared runtime collection

`MagneticForceHellWargodPower._cardsQueuedForReplay` is another constructor-initialized mutable collection on an `AbstractModel` subclass. `PowerModel.DeepCloneFields()` does not clone subclass fields, so all mutable copies share the canonical `HashSet<CardModel>`.

The current single-player/`PowerStackType.Single` path limits practical exposure, and `finally` normally removes markers. It is still a context leak and becomes risky under duplicate power instances, multiplayer hook ordering, or future behavior changes. Base-game powers store per-instance collections through `InitInternalData()`; this power should follow that pattern.

### Medium: event music cleanup depends on explicit choices

`UnattendedPiano` stops its custom music in `LeaveAndDrinkTea`, `StopPlaying`, and `FinalLeave`, but does not override event cleanup. Save/quit, room teardown, cancellation, or an exception after music starts can leave the root-level static `AudioStreamPlayer` active. This should be closed through the event lifecycle rather than every happy-path button.

### Low: global UI instance tracking is never pruned

`JukeboxRunInjector.PatchedGlobalUis` retains every `NGlobalUi` instance ID for the life of the process. Godot instance IDs normally remain unique enough that this is not the reported room-change bug, but repeated runs grow stale global context. Remove IDs when the associated UI exits or store weak/valid object references.

## Baseline verification

- Current source builds successfully in Release mode with 0 errors. The build reports 4 pre-existing nullable warnings in unrelated compatibility code in `ModSupport`.
- Decompiled output confirms the canonical constructor does not initialize `_remainingShadows`; the lazy allocation occurs inside the mutable-only getter.
- The built assembly no longer references `_visitedEventIds`, `HasVisitedEvent`, or `MarkEventVisited`.
- The rebuilt DLL/PCK/manifest were installed locally on 2026-07-23 and match the release artifacts byte-for-byte; a fresh `godot.log` is still required after reproducing the fixed route to distinguish any secondary exception in the option callback or audio path.
