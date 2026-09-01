# Togawasakiko Bug Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 恢复已验证但未进入远端的联机修复，让 Darv 回归原版流程，并增加双端 API 与发布身份门禁，同时完全不碰 Spine、动作和 PCK。

**Architecture:** 只在基于 `8b5e17ee` 的隔离分支工作。先写仅依赖 Python 标准库的源码契约测试，再逐项对照 STS2 `v0.107.1` 原版反编译结果移植 2026-07-29 候选修改。可选 UI 反射失败时保留原版流程；联机 gameplay 不变量失败时显式报错。同一份 C# 源码分别对 Mac 和 Windows 参考程序集构建，最终版本升级、PCK、安装、tag 和 GitHub Release 留给独立的 `v0.2.2` 发布工作树。

**Tech Stack:** C# / .NET 9, Harmony, GodotSharp, Python 3 standard library `unittest`, deterministic `dotnet build`, SHA-256, ZIP validation.

**Spec:** `docs/superpowers/specs/2026-09-01-togawasakiko-bug-hardening-design.md`

## Global Constraints

- Git 基线固定为 PR #4 远端头 `8b5e17ee`，不得整体复制脏主工作区中的 mod 目录。
- 游戏/API 基线固定为 STS2 `v0.107.1`，commit `59260271`。
- 不修改 `assets/`、`incoming_assets/`、视觉资源、动作、Spine、Godot scene、PCK、release zip。
- 不增加平台专属 gameplay 分支；Mac/Windows 构建使用同一份源码。
- 本分支 active manifest 保持 `0.2.1`；`0.2.2` 只由独立发布工作树统一升级一次。
- 新 Python 测试和脚本只使用标准库，不依赖 Pillow。
- 基线构建为 0 error、4 个 nullable warning；最终不得增加 warning。
- 自动化通过只证明源码契约和 API 编译兼容，不得写成 Mac、Windows 或真实联机实机通过。
- 本计划不安装游戏、不覆盖本地 mod、不 push PR #4、不创建 tag 或 GitHub Release。

## File Map

- Create: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Create: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_release_validation.py`
- Create: `mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py`
- Create: `mods/Togawasakiko_in_Slay_the_Spire/scripts/validate-release-staging.py`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj`
- Delete: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/DarvPatches.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Entry.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/ModSupport.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Cards/TogawasakikoSongCards.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoSongPowers.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CombatWatcherPatches.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoPowers.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Relics/BestCompanion.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CardLibraryPatches.cs`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/TogawaEventRoomPatches.cs`
- Create: `docs/audits/multiplayer-compatibility-fixes-2026-07-29.md`
- Modify: `docs/current-status.md`, `docs/next-task.md`, `docs/thread-handoff.md`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`

---

### Task 1: Contract Harness and Dual-Reference Build

**Files:**
- Create: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Create: `mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj:2-21`

**Interfaces:**
- Consumes: Mac reference directory with `sts2.dll` and `GodotSharp.dll`; Windows evidence directory with `sts2.dll`, `GodotSharp.dll`, `0Harmony.dll` and `release_info.json`.
- Produces: MSBuild properties `Sts2ReferenceDir: string` and `HarmonyReferencePath: string`; CLI `verify-api-compatibility.py --mac-reference-dir PATH --windows-reference-dir PATH [--output-root PATH]`.

- [ ] **Step 1: Write failing project-reference tests**

Create the test helpers and first contract class:

~~~python
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET

MOD_ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = Path(__file__).resolve().parents[3]
SRC_ROOT = MOD_ROOT / "src"


def read_source(relative_path: str) -> str:
    return (MOD_ROOT / relative_path).read_text(encoding="utf-8")


class ReferenceConfigurationContracts(unittest.TestCase):
    def test_csproj_exposes_reference_overrides(self) -> None:
        root = ET.parse(SRC_ROOT / "Togawasakiko_in_Slay_the_Spire.csproj").getroot()
        values = {node.tag: (node.text or "").strip() for node in root.iter()}
        self.assertEqual(
            "../../../references/game-dlls/sts2/arm64",
            values["Sts2ReferenceDir"],
        )
        self.assertEqual(
            "../../../references/game-dlls/shared/0Harmony.dll",
            values["HarmonyReferencePath"],
        )
        hints = [node.text.strip() for node in root.iter("HintPath") if node.text]
        self.assertIn("$(Sts2ReferenceDir)/sts2.dll", hints)
        self.assertIn("$(Sts2ReferenceDir)/GodotSharp.dll", hints)
        self.assertIn("$(HarmonyReferencePath)", hints)

    def test_dual_reference_verifier_exists(self) -> None:
        script = MOD_ROOT / "scripts" / "verify-api-compatibility.py"
        self.assertTrue(script.is_file())
        text = script.read_text(encoding="utf-8")
        self.assertIn("--mac-reference-dir", text)
        self.assertIn("--windows-reference-dir", text)
        self.assertIn("--no-incremental", text)
~~~

- [ ] **Step 2: Run the tests and verify baseline failure**

Run:

~~~bash
python3 -m unittest mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py -v
~~~

Expected: one error for missing `Sts2ReferenceDir` and one failure because the verifier does not exist.

- [ ] **Step 3: Add explicit reference properties**

Add conditional defaults:

~~~xml
<Sts2ReferenceDir Condition="'$(Sts2ReferenceDir)' == ''">../../../references/game-dlls/sts2/arm64</Sts2ReferenceDir>
<HarmonyReferencePath Condition="'$(HarmonyReferencePath)' == ''">../../../references/game-dlls/shared/0Harmony.dll</HarmonyReferencePath>
~~~

Change the three references to:

~~~xml
<HintPath>$(Sts2ReferenceDir)/sts2.dll</HintPath>
<HintPath>$(HarmonyReferencePath)</HintPath>
<HintPath>$(Sts2ReferenceDir)/GodotSharp.dll</HintPath>
~~~

- [ ] **Step 4: Implement and run the dual-reference verifier**

The script must:

1. Parse `--mac-reference-dir`, `--windows-reference-dir` and optional `--output-root` with `argparse`.
2. Require `sts2.dll` and `GodotSharp.dll` in both reference directories.
3. Use repository `references/game-dlls/shared/0Harmony.dll` for Mac and Windows directory `0Harmony.dll` for Windows.
4. Parse Windows `release_info.json` and require version `v0.107.1` plus commit `59260271`.
5. Print SHA-256 for all six selected reference files.
6. Run two sequential commands with `subprocess.run(check=True)`:

~~~python
command = [
    "dotnet",
    "build",
    str(project),
    "-c",
    "Release",
    "--no-incremental",
    f"-p:Sts2ReferenceDir={reference_dir}",
    f"-p:HarmonyReferencePath={harmony_path}",
    f"-p:OutputPath={output_root / label}",
]
~~~

Run:

~~~bash
python3 -m unittest mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py -v
python3 mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py \
  --mac-reference-dir "$PWD/references/game-dlls/sts2/arm64" \
  --windows-reference-dir "/Users/user/Desktop/sts2-mod-dev/references/windows-sts2/2026-07-03/data_sts2_windows_x86_64" \
  --output-root /tmp/togawasakiko-api-compat
~~~

Expected: tests pass; both builds return 0 errors and no more than 4 warnings.

- [ ] **Step 5: Commit**

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py \
  mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py \
  mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj
git commit -m "test: add cross-platform API compatibility gate"
~~~

### Task 2: Darv Original-Flow Recovery

**Files:**
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Delete: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/DarvPatches.cs`
- Verify: `mods/Togawasakiko_in_Slay_the_Spire/src/Cards/TogawasakikoCards.cs:147-204`

**Interfaces:**
- Consumes: original `Darv.GenerateInitialOptions()` and `DustyTome.SetupForPlayer(Player)` from `v0.107.1`.
- Produces: no mod-owned Darv override; `Curseslander` remains an Ancient card in the Togawasakiko model namespace.

- [ ] **Step 1: Add failing Darv contracts**

~~~python
class DarvOriginalFlowContracts(unittest.TestCase):
    def test_mod_does_not_patch_darv(self) -> None:
        self.assertFalse((SRC_ROOT / "Patches" / "DarvPatches.cs").exists())

    def test_curseslander_is_ancient(self) -> None:
        cards = read_source("src/Cards/TogawasakikoCards.cs")
        character = read_source("src/Characters/Togawasakiko.cs")
        start = cards.index("internal sealed class Curseslander")
        end = cards.index("internal sealed class Unendurable", start)
        block = cards[start:end]
        self.assertIn("CardRarity.Ancient", block)
        self.assertIn('GetAncientPortraitPath("curseslander.png")', block)
        self.assertIn("ModelDb.Card<Curseslander>()", character)
~~~

- [ ] **Step 2: Verify the intended failure**

Run:

~~~bash
python3 -m unittest mods.Togawasakiko_in_Slay_the_Spire.tests.test_compatibility_contracts.DarvOriginalFlowContracts -v
~~~

Expected: the no-patch test fails because `DarvPatches.cs` exists; the Curseslander test passes.

- [ ] **Step 3: Delete only the copied Darv patch**

Re-read these `v0.107.1` evidence files immediately before editing:

- `local/decompiled-sts2-core-target/MegaCrit.Sts2.Core.Models.Events/Darv.cs`
- `local/decompiled-sts2-core-target/MegaCrit.Sts2.Core.Models.Relics/DustyTome.cs`

Delete `src/Patches/DarvPatches.cs` with `apply_patch`. Do not add a replacement prefix, candidate filter or fallback option generator.

- [ ] **Step 4: Run tests and both builds**

Run the full compatibility test file and the verifier command from Task 1 with output root `/tmp/togawasakiko-api-compat-darv`.

Expected: all tests pass and both builds pass.

- [ ] **Step 5: Commit**

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/DarvPatches.cs
git commit -m "fix: return Darv rewards to original flow"
~~~

### Task 3: Run-Wide Migration and Managed Choice Contexts

**Files:**
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Entry.cs:105-175`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/ModSupport.cs:878-1074,1290-1382,1594-1610,1673-1690`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Cards/TogawasakikoSongCards.cs:330-355,837-854,1065-1085`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoSongPowers.cs:230-260`

**Interfaces:**
- Consumes: `RunState.Players`, `HookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(Task)`, `ThrowingPlayerChoiceContext`, and the public Mac/Windows `v0.107.1` APIs verified directly from both retained assemblies.
- Produces: `RunCombatHookTask(AbstractModel, ICombatState, Func<PlayerChoiceContext, Task>, Player?): Task`; gameplay Pressure and Power helpers require an upstream `PlayerChoiceContext`; old-version reflection shims are removed where both current assemblies expose the same public signature.

- [ ] **Step 1: Add failing synchronization contracts**

~~~python
class MultiplayerContextContracts(unittest.TestCase):
    def test_run_migrations_cover_all_players(self) -> None:
        entry = read_source("src/Entry.cs")
        block = entry[
            entry.index("private static void OnRunStarted"):
            entry.index("private static ShadowOfThePastCard")
        ]
        self.assertIn("foreach (Player player in runState.Players)", block)
        self.assertNotIn("GetLocalPlayer(runState)", block)

    def test_hook_task_is_registered(self) -> None:
        support = read_source("src/ModSupport.cs")
        cards = read_source("src/Cards/TogawasakikoSongCards.cs")
        self.assertIn("public static async Task RunCombatHookTask(", support)
        self.assertIn("AssignTaskAndWaitForPauseOrCompletion(task)", support)
        self.assertIn("await ModSupport.RunCombatHookTask(", cards)

    def test_detached_gameplay_context_is_absent(self) -> None:
        support = read_source("src/ModSupport.cs")
        self.assertNotIn("CreateDetachedChoiceContext", support)
        self.assertNotIn("CreateBestEffortCombatChoiceContext", support)
        self.assertNotIn("public static async Task ApplyPressure(Creature target", support)

    def test_kill_kiss_uses_throwing_context(self) -> None:
        powers = read_source("src/Powers/TogawasakikoSongPowers.cs")
        self.assertIn(
            "PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();",
            powers,
        )

    def test_current_v0107_public_apis_are_called_directly(self) -> None:
        support = read_source("src/ModSupport.cs")
        for old_resolver in (
            "ResolveCreatureCombatStateProperty",
            "ResolvePowerApplyCreatureMethod",
            "ResolvePowerModifyAmountMethod",
            "ResolveAddGeneratedCardToCombatMethod",
            "ResolveAttackTargetingAllOpponentsMethod",
            "ResolveHookPlayerChoiceContextModelConstructor",
        ):
            self.assertNotIn(old_resolver, support)
        self.assertIn("creature?.CombatState as CombatState", support)
        self.assertIn("PowerCmd.Apply<T>(choiceContext", support)
        self.assertIn("PowerCmd.ModifyAmount(choiceContext", support)
        self.assertIn("CardPileCmd.AddGeneratedCardToCombat(", support)
        self.assertIn("command.TargetingAllOpponents(combatState)", support)
~~~

- [ ] **Step 2: Run and capture the five expected failures**

Run the `MultiplayerContextContracts` class. Expected: local-player migration, unmanaged hook context, detached fallback, Kill Kiss context and old-version API shim tests all fail.

- [ ] **Step 3: Port the verified July implementation**

Change `OnRunStarted` and `SanitizeShadowCards` so the outer player source is always `runState.Players`. Set the run-level sanitized flag only after every player deck has been inspected.

Replace best-effort context creation with:

~~~csharp
public static async Task RunCombatHookTask(
    AbstractModel source,
    ICombatState combatState,
    Func<PlayerChoiceContext, Task> taskFactory,
    Player? preferredPlayer = null)
{
    HookPlayerChoiceContext? choiceContext =
        CreateHookChoiceContext(source, combatState, preferredPlayer);
    if (choiceContext == null)
    {
        throw new InvalidOperationException(
            $"Could not create a synchronized combat hook context for {source.GetType().Name}.");
    }

    Task task = taskFactory(choiceContext);
    await choiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
}
~~~

Make `CreateHookChoiceContext` return `HookPlayerChoiceContext` and directly call the public current constructor:

~~~csharp
return new HookPlayerChoiceContext(
    source,
    localPlayerId.Value,
    combatState,
    gameActionType);
~~~

Remove the cached constructor `ConstructorInfo` and its resolver. Also remove:

- no-context `ApplyPressure` and `TryConsumePressure` overloads;
- no-context `ApplyPower` and `ModifyPowerAmount` overloads;
- `ResolvePowerChoiceContext`;
- `CreateBestEffortCombatChoiceContext`;
- `CreateDetachedChoiceContext` and `DetachedPlayerChoiceContext`.

Replace the old-version reflection shims with the public signatures confirmed to be identical in the retained Mac and Windows `v0.107.1` assemblies:

~~~csharp
public static CombatState? GetCombatState(Creature? creature)
{
    return creature?.CombatState as CombatState;
}

public static Task<T?> ApplyPower<T>(
    PlayerChoiceContext choiceContext,
    Creature target,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false)
    where T : PowerModel
{
    return PowerCmd.Apply<T>(choiceContext, target, amount, applier, cardSource, silent);
}
~~~

Use the same direct pattern for `PowerCmd.ModifyAmount`, `CardPileCmd.AddGeneratedCardToCombat`, and `AttackCommand.TargetingAllOpponents(ICombatState)`. Remove their cached `MethodInfo`, signature flags and resolver methods. The Hook context constructor is also direct because both retained current assemblies expose the same public constructor.

Update both Inferiority generation call sites to pass `choiceContext`. Update `ImprisonedXii` to use `RunCombatHookTask`. Use `new ThrowingPlayerChoiceContext()` in Kill Kiss.

Do not port thorn VFX additions from the dirty workspace.

- [ ] **Step 4: Verify source and both APIs**

Run:

~~~bash
python3 -m unittest mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py -v
rg -n "CreateDetachedChoiceContext|CreateBestEffortCombatChoiceContext|ApplyPressure\\(Creature" \
  mods/Togawasakiko_in_Slay_the_Spire/src
~~~

Expected: tests pass and `rg` returns no matches. Run the dual verifier with output root `/tmp/togawasakiko-api-compat-contexts`; both builds pass.

- [ ] **Step 5: Commit**

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py \
  mods/Togawasakiko_in_Slay_the_Spire/src/Entry.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/ModSupport.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Cards/TogawasakikoSongCards.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoSongPowers.cs
git commit -m "fix: keep combat choices in synchronized contexts"
~~~

### Task 4: Watcher Ownership, Relic Rewards, and Replay State

**Files:**
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CombatWatcherPatches.cs:14-55`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoPowers.cs:103-151,277-335`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Relics/BestCompanion.cs:18-65`

**Interfaces:**
- Consumes: stable `CombatState.Players` order; original `HellraiserPower` lazy mutable collection pattern; original `DustyTome.AfterObtained()` card-add chain.
- Produces: exactly one watcher per Togawasakiko player; `IsPressureRedemptionOwner(Player, Creature?, CardModel?): bool`; mutable-instance replay set; command-based relic deck additions.

- [ ] **Step 1: Add failing gameplay-state contracts**

~~~python
class GameplayStateContracts(unittest.TestCase):
    def test_watcher_invalid_state_is_not_swallowed(self) -> None:
        patch = read_source("src/Patches/CombatWatcherPatches.cs")
        self.assertNotIn("catch (Exception ex)", patch)
        self.assertIn("Expected exactly one combat watcher", patch)

    def test_pressure_redemption_has_one_owner(self) -> None:
        powers = read_source("src/Powers/TogawasakikoPowers.cs")
        self.assertIn("IsPressureRedemptionOwner", powers)
        self.assertIn("cardSource?.Owner?.Character is Togawasakiko", powers)
        self.assertIn("applier?.Player?.Character is Togawasakiko", powers)

    def test_replay_state_is_lazy_per_mutable_power(self) -> None:
        powers = read_source("src/Powers/TogawasakikoPowers.cs")
        self.assertIn("private HashSet<CardModel>? _cardsQueuedForReplay;", powers)
        self.assertIn("_cardsQueuedForReplay ??= new HashSet<CardModel>()", powers)
        self.assertNotIn(
            "private readonly HashSet<CardModel> _cardsQueuedForReplay",
            powers,
        )

    def test_relic_cards_use_original_command_chain(self) -> None:
        relics = read_source("src/Relics/BestCompanion.cs")
        self.assertEqual(2, relics.count("Owner.RunState.CreateCard("))
        self.assertEqual(2, relics.count("CardPileCmd.Add(card, PileType.Deck)"))
        self.assertEqual(2, relics.count("CardCmd.PreviewCardPileAdd"))
        self.assertNotIn("AddSpecificCardToDeck", relics)
~~~

- [ ] **Step 2: Verify all four old patterns fail**

Run the `GameplayStateContracts` class. Expected: four failures against `8b5e17ee`.

- [ ] **Step 3: Apply the original-aligned implementations**

In `CombatWatcherPatches`:

- remove the outer catch;
- install in `state.Players` order;
- throw if a player creature is outside the current combat;
- throw if more than one watcher exists before install;
- perform a second pass and require exactly one watcher after install.

In `MagneticForceHellWargodPower` use:

~~~csharp
private HashSet<CardModel>? _cardsQueuedForReplay;

private HashSet<CardModel> CardsQueuedForReplay
{
    get
    {
        AssertMutable();
        return _cardsQueuedForReplay ??= new HashSet<CardModel>();
    }
}
~~~

Use this property for remove/add/finally-remove.

For Pressure redemption, choose owner in this order: Togawasakiko card owner, Togawasakiko applier player, first living Togawasakiko in combat player order. Only that watcher may consume Pressure.

For each relic, use the concrete card type:

~~~csharp
CardModel card = Owner.RunState.CreateCard(
    ModelDb.Card<BarkingBarkingBarking>(),
    Owner);
CardCmd.PreviewCardPileAdd(
    await CardPileCmd.Add(card, PileType.Deck),
    2f);
Flash();
~~~

Repeat directly with `PullmanCrash`; do not add a generic abstraction.

- [ ] **Step 4: Run all contracts and both builds**

Run the full compatibility test file and dual verifier with output root `/tmp/togawasakiko-api-compat-state`.

Expected: tests pass; both builds pass with no warning increase.

- [ ] **Step 5: Commit**

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CombatWatcherPatches.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Powers/TogawasakikoPowers.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Relics/BestCompanion.cs
git commit -m "fix: isolate multiplayer combat state ownership"
~~~

### Task 5: Feature-Local Private Reflection

**Files:**
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CardLibraryPatches.cs:50-140`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/src/Patches/TogawaEventRoomPatches.cs:1-151`

**Interfaces:**
- Consumes: `AccessTools.Field(Type, string): FieldInfo?`, public `NEventRoom.Layout`, Harmony prefix continuation via `return true`.
- Produces: lazy one-time private field resolution; no static `FieldRefAccess`; no `Player.RunState` mutation; original method preserved when optional UI access is unavailable.

- [ ] **Step 1: Add failing reflection contracts**

~~~python
class PrivateReflectionContracts(unittest.TestCase):
    def test_optional_ui_has_no_static_field_ref(self) -> None:
        for relative in (
            "src/Patches/CardLibraryPatches.cs",
            "src/Patches/TogawaEventRoomPatches.cs",
        ):
            source = read_source(relative)
            self.assertNotIn("FieldRefAccess", source, relative)
            self.assertNotIn("AccessTools.FieldRef<", source, relative)

    def test_teiji_does_not_mutate_run_state(self) -> None:
        source = read_source("src/Patches/TogawaEventRoomPatches.cs")
        self.assertNotIn("PlayerRunStateSetter", source)
        self.assertNotIn("PropertySetter(typeof(Player)", source)
        self.assertNotIn("RunStateRef(__instance) =", source)
        self.assertNotIn("class TogawaEventRoomSetupPatch", source)

    def test_event_fallback_uses_public_layout(self) -> None:
        source = read_source("src/Patches/TogawaEventRoomPatches.cs")
        self.assertIn("Lazy<FieldInfo?>", source)
        self.assertIn("__instance.Layout", source)
        self.assertIn("return true;", source)
~~~

- [ ] **Step 2: Verify the static reflection tests fail**

Run the `PrivateReflectionContracts` class. Expected: failures identify `FieldRefAccess`, the run-state setter/setup patch and reflected `get_Layout`.

- [ ] **Step 3: Make Card Library access lazy and local**

Add `using System.Reflection;` and replace each static FieldRef with `Lazy<FieldInfo?>`. Resolve through:

~~~csharp
private static FieldInfo? ResolveOptionalField(string fieldName)
{
    FieldInfo? field = AccessTools.Field(typeof(NCardLibrary), fieldName);
    if (field == null)
    {
        ModSupport.LogWarn(
            $"Card library integration disabled: missing NCardLibrary.{fieldName}.");
    }

    return field;
}
~~~

At postfix invocation:

- if either required dictionary field/value is absent, return before creating a toggle;
- treat `_lastHoveredControl` as optional and only set it if its field resolves;
- retain the outer catch because original `_Ready()` has already completed and this is optional UI.

- [ ] **Step 4: Reduce Teiji to a local description fallback**

Delete `TogawaEventRoomSetupPatch`. Resolve only `_event` through `Lazy<FieldInfo?>`.

The prefix must:

1. Return `true` if the field is unavailable, event is not `TogawaTeiji`, description is missing, owner is null or public `Layout` is null.
2. Never assign `Player.RunState` or `NEventRoom._runState`.
3. Use `(owner.RunState?.Players.Count ?? 1) > 1` only to compute `IsMultiplayer`; do not access private `RunManager.State`.
4. Add character details and event dynamic vars, call `__instance.Layout.SetDescription`, then return `false`.
5. Catch formatting/injection errors, log, and return `true` without a second reflected fallback.

- [ ] **Step 5: Verify and commit**

Run:

~~~bash
python3 -m unittest mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py -v
rg -n "FieldRefAccess|PropertySetter\\(typeof\\(Player\\)|PlayerRunStateSetter" \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CardLibraryPatches.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/TogawaEventRoomPatches.cs
~~~

Expected: tests pass and `rg` has no matches. Run both API builds with output root `/tmp/togawasakiko-api-compat-reflection`.

Commit:

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_compatibility_contracts.py \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/CardLibraryPatches.cs \
  mods/Togawasakiko_in_Slay_the_Spire/src/Patches/TogawaEventRoomPatches.cs
git commit -m "fix: localize optional UI reflection failures"
~~~

### Task 6: Release Staging Identity Gate

**Files:**
- Create: `mods/Togawasakiko_in_Slay_the_Spire/tests/test_release_validation.py`
- Create: `mods/Togawasakiko_in_Slay_the_Spire/scripts/validate-release-staging.py`

**Interfaces:**
- Consumes: CLI `--expected-version VERSION --staging-root PATH --zip PATH --sha256-file PATH`.
- Produces: exit 0 only when source/pack/staged manifests, allowlist, ZIP bytes and four SHA-256 records agree.

- [ ] **Step 1: Write failing temporary-package tests**

Use `tempfile.TemporaryDirectory` to create:

~~~text
stage/Togawasakiko_in_Slay_the_Spire/
  Togawasakiko_in_Slay_the_Spire.dll
  Togawasakiko_in_Slay_the_Spire.pck
  mod_manifest.json
Togawasakiko_in_Slay_the_Spire-v0.2.1.zip
Togawasakiko_in_Slay_the_Spire-v0.2.1.sha256
~~~

The ZIP contains exactly the same three staged files under one top-level mod directory. The test computes each real SHA-256 value and writes four records in standard two-space format, in this order: ZIP, staged DLL, staged PCK, staged manifest. The three staged records use paths relative to `stage/`, including the top-level mod directory.

Implement these test methods:

- `test_valid_staging_passes`
- `test_expected_version_mismatch_fails`, requiring stderr to contain `expected version 0.2.2`
- `test_tampered_artifact_fails_hash_validation`
- `test_extra_staged_file_fails_allowlist`
- `test_zip_with_wrong_top_level_fails`

- [ ] **Step 2: Verify failure because the validator is absent**

Run:

~~~bash
python3 -m unittest mods/Togawasakiko_in_Slay_the_Spire/tests/test_release_validation.py -v
~~~

Expected: tests fail because `scripts/validate-release-staging.py` does not exist.

- [ ] **Step 3: Implement exact validation rules**

The script defines:

~~~python
INSTALL_FILES = {
    "Togawasakiko_in_Slay_the_Spire.dll",
    "Togawasakiko_in_Slay_the_Spire.pck",
    "mod_manifest.json",
}
~~~

Implement and call these functions in order:

- `load_manifest(path: Path) -> dict[str, object]`
- `sha256(path: Path) -> str`
- `parse_hash_file(path: Path) -> dict[str, str]`
- `validate_manifest_identity(expected, source, pack, staged) -> None`
- `validate_staging_allowlist(mod_dir: Path) -> None`
- `validate_zip(zip_path, mod_name, staged_files) -> None`
- `validate_hashes(records, zip_path, mod_dir) -> None`

Rules:

- `version` must match across source, pack, staged and `--expected-version`.
- `min_game_version` must match across all three and equal `0.107.1`.
- staged mod directory contains exactly `INSTALL_FILES`.
- ZIP contains exactly those files under one `Togawasakiko_in_Slay_the_Spire/` prefix and member bytes equal staged bytes.
- hash file has exactly four relative-name records shown in Step 1 and every digest matches.
- any mismatch raises `ValueError`; `main` prints the message to stderr and returns 1.
- success prints version plus all validated paths and returns 0.

- [ ] **Step 4: Run the full standard-library suite**

Run:

~~~bash
python3 -m unittest discover -s mods/Togawasakiko_in_Slay_the_Spire/tests -p 'test_*.py' -v
~~~

Expected: all compatibility and release validation tests pass without third-party packages.

- [ ] **Step 5: Commit**

~~~bash
git add mods/Togawasakiko_in_Slay_the_Spire/tests/test_release_validation.py \
  mods/Togawasakiko_in_Slay_the_Spire/scripts/validate-release-staging.py
git commit -m "test: gate Togawasakiko release identity"
~~~

### Task 7: Full Verification and Status Documentation

**Files:**
- Create: `docs/audits/multiplayer-compatibility-fixes-2026-07-29.md`
- Modify: `docs/current-status.md`
- Modify: `docs/next-task.md`
- Modify: `docs/thread-handoff.md`
- Modify: `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`

**Interfaces:**
- Consumes: commits and command evidence from Tasks 1-6.
- Produces: tracked audit, synchronized status documents and a clean commit range ready for release-worktree integration.

- [ ] **Step 1: Recover the July audit as tracked evidence**

Read the dirty main workspace copy, then reproduce its text with `apply_patch`. Append:

~~~markdown
## 2026-09-01 远端收口

- 基线：PR #4 `8b5e17ee`。
- 六组修改已逐项对照 STS2 `v0.107.1` / `59260271` 后移植。
- Darv 已删除模组覆盖，回归原版 `GenerateInitialOptions()` 与 `DustyTome`。
- Mac 与 Windows 参考程序集构建已通过；这不等于双端实机或联机通过。
- 人物动作、Spine、Godot scene 和 PCK 未在本分支修改。
~~~

- [ ] **Step 2: Synchronize evidence levels in four status documents**

Record consistently:

- Darv source/build complete; Mac and Windows runtime pending.
- July multiplayer changes tracked and dual-reference build complete; real multiplayer pending.
- optional private reflection is lazy and feature-local; missing card-library fields only remove the Togawasakiko toggle.
- active manifest remains `0.2.1`; `v0.2.2` integration is separate.
- card-stuck, jukebox room transition, Unattended Piano, Teiji and action runtime checks stay pending without new in-game evidence.
- accepted energy-counter visual defect remains outside the bug list.

- [ ] **Step 3: Run final verification**

~~~bash
python3 -m unittest discover -s mods/Togawasakiko_in_Slay_the_Spire/tests -p 'test_*.py' -v
python3 mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py \
  --mac-reference-dir "$PWD/references/game-dlls/sts2/arm64" \
  --windows-reference-dir "/Users/user/Desktop/sts2-mod-dev/references/windows-sts2/2026-07-03/data_sts2_windows_x86_64" \
  --output-root /tmp/togawasakiko-api-compat-final
git diff --check
~~~

Expected: all tests pass; Mac and Windows builds have 0 errors and no more than 4 warnings; `git diff --check` is silent.

- [ ] **Step 4: Enforce the no-visual-change boundary**

Run:

~~~bash
git diff --name-only 8b5e17ee..HEAD
git status --short
~~~

Inspect every path. Reject any change under `assets/`, `incoming_assets/`, `pack/`, `exports/` or any PCK, ZIP, image, audio, Spine, animation, scene or imported resource. Inspect ignored `bin/`, `obj/` and test caches before removing only files generated by this branch.

- [ ] **Step 5: Commit documentation and report handoff evidence**

~~~bash
git add docs/audits/multiplayer-compatibility-fixes-2026-07-29.md \
  docs/current-status.md docs/next-task.md docs/thread-handoff.md \
  mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md
git commit -m "docs: record compatibility hardening evidence"
~~~

Report:

- branch and commit range `8b5e17ee..HEAD`;
- exact unit-test count;
- Mac and Windows build result and warning count;
- all reference hashes printed by the verifier;
- changed path list and confirmation that visual/PCK paths are absent;
- remaining Mac, Windows and real multiplayer runtime checks;
- explicit statement that no install, push, tag or Release occurred.
