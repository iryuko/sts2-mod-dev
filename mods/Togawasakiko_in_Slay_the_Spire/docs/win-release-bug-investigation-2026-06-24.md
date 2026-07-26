# Win release bug investigation - 2026-06-24

> Status (2026-07-26): historical-to-current investigation record. The opening "latest release" means `v0.2.1-20260615` at the time of the first report. API synchronization to STS2 v0.107.1 has since been completed; same-package Windows card-play and jukebox room-transition tests are still open. Current status is tracked in [current-status.md](current-status.md).

## Scope

Player reports after the latest `v0.2.1-20260615` release:

- On Windows Steam builds, some cards can remain stuck in the middle of the screen and fail to play.
- On Windows Steam builds, `jukebox` custom music can stop after entering a new room.

macOS Steam testing has not reproduced either issue yet. Treat this as a cross-platform / version-sensitive bug hunt, not as user error.

## Required evidence before a final fix

- Windows `godot.log` from a run where the issue occurred.
- STS2 version and commit from `release_info.json`.
- Exact mod zip filename and zip sha256.
- For the stuck-card issue:
  - card name
  - combat room / event combat / normal combat
  - whether the card targets an enemy, self, all enemies, or no target
  - whether `jukebox` is open or playing
- For the `jukebox` issue:
  - room type before and after transition
  - selected track
  - whether the UI still displays a playing status after the audio stops
  - whether base-game BGM returns, is muted, or all music is silent

## 2026-07-03 Windows reference files

Copied from the user-provided Windows runtime files into:

- `references/windows-sts2/2026-07-03/data_sts2_windows_x86_64/`

Observed Windows runtime:

- `release_info.json`: `v0.107.1`, commit `59260271`, date `2026-06-18T15:43:56-07:00`
- `sts2.dll` sha256: `a1f9e653f1e28e4076558fee1e60d218619cb7e057b887c6417f62c62c6d7a52`
- `0Harmony.dll` sha256: `ef1898322c9f5c86dc1b0758b272a9c440823b4a41ca9a0b82a3aa6b3d206387`
- `GodotSharp.dll` sha256: `0e4897ecdfb31456a97c7d8028dfb8d7dbdc632e2f73fc9b438d7b266a139289`

Compared references:

- local macOS API reference: `v0.103.3`, commit `460a0ece`
- old PCK extract reference: `v0.98.3`, commit `cb602cef`

Important conclusion:

- The reported Windows environment is not merely "same game on Windows"; it is a newer STS2 runtime than the local macOS reference used by the mod project.
- `0Harmony.dll` and `GodotSharp.dll` match current local references; the relevant compatibility delta is mainly `sts2.dll`.

## 2026-07-03 confirmed code-level bug

`v0.107.1` changed generated-card APIs in `CardPileCmd`.

Old local reference (`v0.103.3`) has:

- `AddGeneratedCardToCombat(CardModel card, PileType newPileType, bool addedByPlayer, CardPilePosition position = Bottom)`

Windows `v0.107.1` has:

- `AddGeneratedCardToCombat(CardModel card, PileType newPileType, Player? creator, CardPilePosition position = Bottom)`

The released mod was compiled against the old `bool addedByPlayer` signature and had direct calls in `ModSupport` generated-card helpers. On Windows `v0.107.1`, those calls can throw `MissingMethodException` at runtime. If this happens inside a card `OnPlay` async flow, the visible symptom is expected to be exactly "the card moved toward play position / screen center, then the play flow did not finish."

Fixed locally:

- `ModSupport` now routes generated-card insertion through a reflection compatibility wrapper.
- The wrapper first detects the new `Player? creator` signature and passes the recipient player.
- If the old `bool addedByPlayer` signature exists, it falls back to `creator != null`.
- Direct old-signature calls have been removed from source and verified by decompiling the rebuilt mod DLL.

This is currently the strongest explanation for the Windows stuck-card report, especially for cards that generate pressure / song / colorless cards during play.

## 2026-07-03 jukebox guard

Windows `v0.107.1` also changed `NRunMusicController` behavior:

- `_currentTrack` is now nullable.
- `StopMusic()` clears `_currentTrack`.
- `UpdateMusic()` can no-op when the selected act track matches `_currentTrack`.

The original jukebox patch treated `NRunMusicController._ExitTree` as a run-exit signal and cleared `_activeTrackPath`. That was too broad. If `NRunMusicController` exits/re-enters the scene tree during a Windows room transition, the mod would stop the custom stream and erase the information needed to resume it on `RoomEntered`.

Fixed locally:

- `JukeboxRunMusicControllerExitPatch` now checks `RunManager.Instance.IsInProgress`.
- If the run is still in progress, it logs and preserves jukebox state instead of calling `StopPlaybackForRunExit()`.

This is a targeted guard for the reported "entering a new room stops jukebox music" symptom. A Windows `godot.log` is still needed to confirm whether `_ExitTree` is actually firing during room transition.

## Current observations from code

### `jukebox` has a concrete lifecycle risk

`src/Patches/JukeboxRunPatches.cs` patches `NRunMusicController._ExitTree` and calls:

- `JukeboxRunInjector.StopPlaybackForRunExit()`
- which clears `_activeTrackPath`
- and stops the shared `AudioStreamPlayer`

Original `NRunMusicController._ExitTree()` itself only calls `StopMusic()`.

This patch assumes `NRunMusicController._ExitTree` only means "run is exiting". That is probably true in the macOS path tested so far, but it is not a safe semantic contract. If the Windows build rebuilds or detaches the run music controller during a room transition, the mod will interpret a normal room transition as run exit and permanently stop custom playback. Once `_activeTrackPath` is cleared, `RoomEntered` cannot resume the track.

More robust direction:

- Do not use `NRunMusicController._ExitTree` as the sole run-exit signal.
- Stop custom playback only when the whole `NRun` exits, the run is abandoned, or combat-specific logic explicitly resets to `Off`.
- Add diagnostic logs around:
  - `NRunMusicController._ExitTree`
  - `NRun._ExitTree` if available
  - `RoomEntered`
  - `_activeTrackPath`
  - `_sharedPlayer` validity / parent path / `IsPlaying()`

### `jukebox` also has a resource-enumeration risk

The track list is discovered by:

- `DirAccess.Open("res://audio/music/tracks")`
- filtering original `.mp3/.ogg/.wav` filenames
- then loading the selected path through `GD.Load<AudioStream>(resourcePath)`

The pack project currently contains both original `.mp3` files and `.mp3.import` files. The import files remap playback to `res://runtime_imports/*.mp3str`.

Potential Windows-sensitive failure modes:

- `DirAccess` may see only different files from a mounted PCK than it does in the macOS editor/build setup.
- `GD.Load("res://audio/music/tracks/foo.mp3")` may resolve through `.import` on one platform but fail or return a stream with different lifecycle behavior on another.
- If stream reload fails after room transition, `EnsureCustomTrackPlaying()` silently returns after logging only "failed to load track".

More robust direction:

- Generate a deterministic track registry from known tracks instead of relying on `DirAccess` at runtime.
- Prefer loading explicit known resource paths and log both `ResourceLoader.Exists(path)` and `GD.Load` result.
- Consider setting loop at the stream level when possible, instead of relying only on `Finished += OnTrackFinished`.

### Stuck-card issue is most likely an interrupted async play flow

The reported visual symptom, "card stuck in the middle of the screen", usually matches a card play action that starts but never completes. In this codebase, the highest-risk class of causes is an exception inside:

- card `OnPlay(...)`
- card / power hook after card play
- autoplay paths
- generated-card paths

Many custom cards intentionally use `ArgumentNullException.ThrowIfNull(cardPlay.Target, ...)` for targeted cards. That should be fine if the base game correctly supplies a target, but if a Windows-specific input / targeting edge case produces a null target, the exception would interrupt the async play chain.

Current suspicious paths to verify with logs:

- targeted cards that begin with `ArgumentNullException.ThrowIfNull(cardPlay.Target, ...)`
- `CardCmd.AutoPlay(...)` in `ChoirSChoir` and replay powers
- `CardPileCmd.AutoPlayFromDrawPile(...)` in `AveMujicaPower`
- generated card helpers in `ModSupport`
- any hook that constructs a best-effort `PlayerChoiceContext`

Do not fix this by broadly swallowing all `OnPlay` exceptions. That would hide state corruption and could leave combat in a worse state. The correct fix is to identify the exact thrown exception and align that card / hook with the original game's safe command path.

### UI overlay is a lower-probability but testable input risk

`JukeboxOverlay` is a `TopLevel` `Control`. The root control uses `MouseFilter = Ignore`, and the intended hit area is only:

- closed: `164 x 52`
- expanded: `344 x 228`

That makes a full-screen input blocker less likely. Still, Windows DPI / viewport behavior could enlarge the effective control rect. If the stuck-card report correlates with `jukebox` being open, inspect:

- overlay position and size logs
- whether closing `jukebox` prevents the issue
- whether the stuck-card issue happens without ever opening `jukebox`

## Current working hypotheses

1. Highest-confidence `jukebox` hypothesis:
   - the `NRunMusicController._ExitTree` patch is too broad and stops playback during a Windows-only controller lifecycle event.

2. Secondary `jukebox` hypothesis:
   - track discovery / load through `DirAccess` and imported `.mp3` remaps behaves differently from macOS after PCK mounting or room transition.

3. Highest-confidence stuck-card hypothesis:
   - an exception in a specific custom card or hook interrupts the async play chain. Need Windows `godot.log` to identify the exact card and stack.

4. Lower-confidence stuck-card hypothesis:
   - `jukebox` overlay or another custom UI node consumes targeting / release input under a Windows viewport / DPI condition.

5. Version mismatch risk:
   - local reference data is not uniformly from the same game release (`references/pck-extract` still reports `v0.98.3`, while API notes report `v0.103.3`). Player reports must include game version so we do not chase a platform bug that is actually an STS2 version delta.

## Next implementation direction

Before changing card behavior:

1. Add targeted diagnostics around play-flow exceptions and `jukebox` lifecycle.
2. Ask Windows testers for logs using the diagnostic build.
3. For `jukebox`, replace the broad `NRunMusicController._ExitTree` stop condition with a true run-exit signal or a guarded check that does not clear `_activeTrackPath` during ordinary room transitions.
4. For track loading, move toward an explicit registry or generated manifest instead of runtime directory enumeration.
5. For stuck cards, fix the exact card / hook stack once identified.

## 2026-07-04 follow-up after first wincompat build still stuck

Windows tester still reported cards stuck after the `20260703-wincompat` build. The first compatibility patch only removed direct calls to the changed `CardPileCmd.AddGeneratedCardToCombat(CardModel, PileType, bool, CardPilePosition)` signature.

Further comparison against Windows `v0.107.1` found a second concrete API break:

- mac/local compile target has `PowerCmd.Apply<T>(Creature, decimal, Creature?, CardModel?, bool)` and `PowerCmd.ModifyAmount(PowerModel, decimal, Creature?, CardModel?, bool)`.
- Windows `v0.107.1` only has context-aware versions:
  - `PowerCmd.Apply<T>(PlayerChoiceContext, Creature, decimal, Creature?, CardModel?, bool)`
  - `PowerCmd.ModifyAmount(PlayerChoiceContext, PowerModel, decimal, Creature?, CardModel?, bool)`

The released mod still had many direct old-signature `PowerCmd.Apply` / `ModifyAmount` calls in cards, powers, and pressure helpers. On Windows these can throw `MissingMethodException` during `OnPlay`, which still matches the visual symptom: the card starts playing, reaches the screen-center play position, then the async play flow is interrupted before cleanup.

Implemented fix:

- Added `ModSupport.ApplyPower<T>` and `ModSupport.ModifyPowerAmount` reflection compatibility wrappers.
- Card `OnPlay` paths now pass the current `PlayerChoiceContext` into these wrappers.
- Pressure helpers now route through the compatibility wrappers.
- Relic/power hooks that already receive a `PlayerChoiceContext` pass it through.
- Added a reflection-compatible `HookPlayerChoiceContext` constructor path because Windows changed the model-source constructor from `CombatState` to `ICombatState`.

Verification:

- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` succeeded.
- `monodis --memberref` on the rebuilt DLL shows no direct `PowerCmd.Apply`, `PowerCmd.ModifyAmount`, `CardPileCmd.AddGeneratedCardToCombat`, or old `HookPlayerChoiceContext(AbstractModel, ulong, CombatState, GameActionType)` memberrefs.
- A managed runtime load test against the Windows DLL could not be completed on this mac because the Windows `sts2.dll` is x86_64 and the local process is arm64; .NET reports architecture incompatibility before type loading.

New test package:

- `exports/release/Togawasakiko_in_Slay_the_Spire-v0.2.1-20260704-win-powercmd-compat.zip`
- zip sha256: `6fc5bfd2925da48782866e5c482d45d382fd648f73e48e1c4c29b7f670726dfd`
- dll sha256: `e8073a9ca9a859b3464d9660b50b4fc5e80826b9efc9507d1b5d5cded50a9812`
- pck sha256: `34c5b3d1f418c9d77cf9878e650a24222adfaab6d4ed3b957f168a233f371b49`
- manifest sha256: `1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`

Remaining Windows API drift to address separately:

- `AbstractModel.BeforeHandDraw`, `AfterSideTurnStart`, and `AfterPowerAmountChanged` changed signatures on Windows `v0.107.1` by using `ICombatState` and/or passing extra context.
- Current affected custom overrides:
  - `PianoOfMom.BeforeHandDraw(Player, PlayerChoiceContext, CombatState)`
  - `FaceReactionPower.AfterSideTurnStart(CombatSide, CombatState)`
  - `InferiorityPower.AfterPowerAmountChanged(PowerModel, decimal, Creature?, CardModel?)`
- These are likely to cause missing custom effects or type-load risk on Windows, but they are less directly tied to the "card stuck mid-screen" symptom than the confirmed command-method breaks above.

## 2026-07-04 jukebox room-transition fix

Windows tester also reported that custom jukebox music stops after changing rooms.

Relevant vanilla flow in Windows `v0.107.1`:

- `RunManager.EnterRoomInternal(...)` calls:
  - `NRunMusicController.Instance?.UpdateTrack()`
  - `NRunMusicController.Instance?.UpdateAmbience()`
  - then fires `RoomEntered`
- Act changes and some restore flows can also call `NRunMusicController.UpdateMusic()` or `UpdateTrack()` outside the normal room-enter callback.
- `NRunMusicController._ExitTree()` calls `StopMusic()`, and this can happen during node lifecycle changes.

Existing jukebox logic only tried to restore the custom `AudioStreamPlayer` once in `RoomEntered`, and only reloaded the track when `_player.Stream == null`. That can miss Windows timing cases where vanilla FMOD/music state is changed during or just after room setup, or where the stream object remains non-null but playback is no longer audible.

Implemented fix:

- `JukeboxOverlay.HandleRoomEntered(...)` now force-reloads/rebinds the active track immediately.
- It also schedules delayed restore checks over the next three process frames.
- `EnsureCustomTrackPlaying(...)` can now force reload, replay, reapply volume, and re-enable progress/volume controls.
- Added postfix patches for:
  - `NRunMusicController.UpdateMusic`
  - `NRunMusicController.UpdateTrack`
  - `NRunMusicController.UpdateAmbience`
- Those postfixes call `JukeboxRunInjector.PreserveCustomPlaybackAfterRunMusicUpdate(...)`, which preserves the custom jukebox track after vanilla music updates.

New combined Windows test package:

- `exports/release/Togawasakiko_in_Slay_the_Spire-v0.2.1-20260704-win-powercmd-jukebox-compat.zip`
- zip sha256: `d48c4680ca87164bb29b263f931bc1f06bb6e7806771888716a5c37c6f6634e4`
- dll sha256: `ecd7346e78c98876dfafe40afff6fba2d1eb5012bbfd1d281d0048a10e635fc8`
- pck sha256: `34c5b3d1f418c9d77cf9878e650a24222adfaab6d4ed3b957f168a233f371b49`
- manifest sha256: `1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`

## 2026-07-04 combat-state compatibility fix after mac regression

After installing the Windows compatibility build locally, mac also failed in combat. The current installed mac game had the same relevant API drift as Windows `v0.107.1`, even though the checked-in mac reference DLLs were older.

Confirmed runtime errors from `~/Library/Application Support/SlayTheSpire2/logs/godot.log`:

- `MissingMethodException: Method not found: 'CombatState Creature.get_CombatState()'`
- First stack: `CombatWatcherPatches.InstallForPlayer(...)` during `CombatManager.SetUpCombat(...)`.
- Second stack: `ModSupport.GetEnemyCreatures(...)` during `DollMask.AfterPlayerTurnStart(...)`.

Cause:

- The mod was still compiled with direct member references to the old `Creature.CombatState : CombatState` property.
- Newer STS2 exposes that property as `ICombatState`.
- Return type is part of the managed method signature, so the old member reference fails even if the runtime object is still a concrete `CombatState`.
- Windows `v0.107.1` also changed `AttackCommand.TargetingAllOpponents(CombatState)` to `TargetingAllOpponents(ICombatState)`, so all custom all-enemy attack cards were another likely stuck-card path.

Implemented fix:

- Added `ModSupport.GetCombatState(Creature?)`, using reflection to read `Creature.CombatState` without binding to the old return type.
- Replaced all direct source-level `.CombatState` reads in the mod with the compatibility helper.
- Added `ModSupport.TargetingAllOpponentsCompat(...)`, using reflection to call either the old `CombatState` or new `ICombatState` `AttackCommand.TargetingAllOpponents` signature.
- Replaced all custom all-enemy attack calls with the compatibility method.
- Added connection-state tracking in `JukeboxOverlay` so `Finished -= OnTrackFinished` is only called after a matching connection, avoiding a non-fatal Godot red error during jukebox player setup.

Verification:

- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire` succeeded.
- `monodis --memberref` on the rebuilt DLL shows no direct `Creature.get_CombatState` or `AttackCommand.TargetingAllOpponents(CombatState)` memberrefs.
- Installed locally via `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target`.

New test package:

- `exports/release/Togawasakiko_in_Slay_the_Spire-v0.2.1-20260704-combatstate-compat.zip`
- zip sha256: `d8b085f7ccc5446e72d64ad5a5ab470e27f23f8859b478eaf817f0f9d32279a1`
- dll sha256: `cac270489c42b4a3a790bcff1a4b8b20efa45ed0d6321185a187f1f7d24cef65`
- pck sha256: `34c5b3d1f418c9d77cf9878e650a24222adfaab6d4ed3b957f168a233f371b49`
- manifest sha256: `1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`

## 2026-07-04 v0.107.1 full API sync pass

After testing the combat-state compatibility build, the mod still showed an error at the main menu. Combat card play and jukebox playback worked, but pressure-token conversion no longer worked.

Confirmed startup error from `godot.log`:

- `HarmonyException: Ambiguous match for HarmonyMethod[(class=NRunMusicController, methodname=UpdateTrack, ...)]`
- Newer `NRunMusicController` has both `UpdateTrack()` and `UpdateTrack(string, float)`.
- The mod patch used `[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateTrack))]`, which became ambiguous during `PatchAll()`.

Implemented fixes:

- Updated local compile references to the current installed game DLLs:
  - STS2 `v0.107.1`, commit `59260271`
  - `references/game-dlls/sts2/arm64/sts2.dll`
  - `references/game-dlls/sts2/x86_64/sts2.dll`
  - `references/api-notes/app/release_info.json`
- Rebuilt against the current DLL so old virtual-hook signatures fail at compile time instead of silently failing at runtime.
- Fixed `NRunMusicController` Harmony patches by explicitly targeting parameterless methods.
- Updated custom hook overrides to current `v0.107.1` signatures:
  - `AfterPowerAmountChanged(PlayerChoiceContext, PowerModel, decimal, Creature?, CardModel?)`
  - `AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)`
  - `BeforeHandDraw(Player, PlayerChoiceContext, ICombatState)`
  - `AfterSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)`
- Pressure-token conversion now receives the real `PlayerChoiceContext` from `AfterPowerAmountChanged` and passes it into pressure consumption.
- Left the custom energy-counter scene unchanged. Its missing `EnergyVfxBack` / `EnergyVfxFront` nodes are a known acceptable visual-resource defect, not part of this API-sync fix.

Verification:

- `dotnet build ... -c Release` succeeded against current `v0.107.1` references.
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire` succeeded.
- Installed locally via `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target`.
- Installed hashes match release output.

New test package:

- `exports/release/Togawasakiko_in_Slay_the_Spire-v0.2.1-20260704-v0107-api-sync-no-energy-scene.zip`
- zip sha256: `12b98fbd27bfdddb482cae741e92815bef898c575924eaa761743d4ea60611fb`
- dll sha256: `633c9dc7bea36a07932e7e043d42116bca0818e67119fdfc58c18b3259bdda4a`
- pck sha256: `34c5b3d1f418c9d77cf9878e650a24222adfaab6d4ed3b957f168a233f371b49`
- manifest sha256: `1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`
