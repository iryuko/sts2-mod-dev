# Gameplay Regression

Uses the actual mod and STS2 v0.107.1 assemblies with the original `TestMode`.
No test framework packages or copied gameplay implementations are required.

From the workspace root on macOS:

```bash
bash shared/scripts/test-togawasakiko-gameplay.sh
```

Requires a .NET SDK supporting net9.0 and a .NET 9 runtime. The bundled Harmony
does not support the machine's .NET 10 runtime. The script prefers the isolated
runtime at `local/tools/dotnet-runtime-9/dotnet`, then falls back to system dotnet.
To prepare the isolated runtime with the existing workspace installer:

```bash
bash local/tools/dotnet-install.sh --runtime dotnet --channel 9.0 --install-dir local/tools/dotnet-runtime-9 --no-path
```

The script verifies the installed game DLL matches the reference and copies four
managed dependencies into `references/game-dlls/test-support/`. It does not start
Steam, Godot or STS2, write game saves, install a mod, or rebuild the PCK.

The fixture uses real card effects, PowerCmd, Hook, card piles, history and combat
card cloning. Native OS queries and log output are replaced because the original
logger calls Godot even in TestMode. Combat startup bypasses scene/network setup;
the tests do not prove reward timing, live multiplayer, save/load, or visual UI.
The actual Harmony combat-watcher patch is installed. The full mod initializer
is not run; the relic-pool test supplies Entry's two registrations explicitly.

The 2026-09-04 suite covers Inferiority redemption, Artifact blocking, ownership,
legacy debuff branches, Two Moons costs and special relic registration.

The approved bridge-card batch adds 23 checks (41 total): filtered native pile
selection/movement, empty candidates, owner/turn adjacency, removing all Face,
limited token-play rewards, stacking/autoplay/expiry, dual owners, native
upgrade/downgrade and bilingual descriptions, Dexterity previews, and Imprisoned
XII's real on-entry draw. `TestLocalization` loads reference localization tables
plus current mod dictionaries into the real formatter without writing user data.
Reference tables are the older extracted baseline; this validates the new mod
strings with the current formatter, not every updated vanilla translation.
`CardSelectCmd.UseSelector` supplies test choices through the original test
interface; the live selection screen and multiplayer transport are not mocked
as passing. Most effects are invoked directly. The Choir nesting and Burst replay
tests run actual `CardCmd.AutoPlay` / `OnPlayWrapper`; only the native Godot clock
is replaced by the managed clock. They reproduce/fix completion-history order
and confirm replays spend the same two-trigger budget. This is still not live
UI/action scheduling or multiplayer transport verification.

The second, pressure/exhaust batch adds 32 checks (73 total). It covers four
non-Song reward registrations, filtered own-hand selection (zero/empty/too few),
per-exhaust Block and Dexterity previews, the native Dark Embrace draw during
an exhaust batch, combat-only keyword mutation, and all four tokens' actual
English/Chinese keyword text before and after losing Ethereal and gaining Retain.
It calls native `DoTurnEnd` and `FlushPlayerHand` for expiry/retention, and actual
`CardCmd.AutoPlay` for Choir exhaustion. These tests bypass the surrounding
network/action scheduler, not the native pile/keyword/exhaust logic.
Other cases cover all-enemy Pressure, single-target consumption including zero,
two owners' independent limits, stacking, next-turn and next-combat reset.
Review added a real `PlayerCmd.AddPet<Osty>` ally-exclusion regression and the
Until Next Act -> self-exhaust -> Choir replay sequence through native autoplay.
The new power uses native `CombatState.HittableEnemies`, like Noxious Fumes,
not the older mod helper that includes friendly Monster-based pets.
The multiplayer fixture does not supply a local network identity, so Imprisoned
XII's hand-entry filter case is a separate single-player test; two-owner selection
uses Ether instead. No production fallback was weakened for the test host.

The 2026-09-05 Song batch adds 33 checks (106 total). It exercises native
upgrade/downgrade and bilingual text, DIVINE payment on every repeat, Blue World
combat generation and relative discounts, full-hand overflow, free-X capture,
actual energy/star payment and ResourceInfo, clone and next-combat isolation.
Blue Eyes uses the original instances and native replay-count hooks; tests cover
Symbol III growth and Dexterity, snapshots, native result piles, and nested Blue Eyes.
Octagram tests install the actual payment/autoplay/replay-loop Harmony patches,
including an incompatible-IL rejection check, real type-order violations,
per-player readiness, teammate repeats, and native wrapper cleanup. A Host-type
network service stub is used only for readiness; it does not simulate transport,
singleplayer/last-player transitions, enemy phases, death or victory rewards.
For scripted generation, the host bridges TestRngInjector into GetForCombat
(only GetDistinctForCombat supports it natively). Unscripted calls execute the
real RNG path, covered separately. Model effects and pile operations are not mocked.
The shuffle regression replaces only the native Godot scene-tree frame-duration
read with a managed 1/60s clock; the shuffle RNG, card moves, draws and hooks stay
native. It checks DIVINE reshuffling Blue Eyes' already-selected discard Song
into hand, which must still receive both plays.
The dedicated album-portrait regression brings the full suite to 107 checks.
