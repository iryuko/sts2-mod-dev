# Togawasakiko 联机兼容修复

记录日期：2026-07-29

## 范围

本轮只处理会让完整 `RunState` 在客户端之间分叉、让玩家选择脱离原版同步队列，
或让 mutable model 共享运行时状态的问题。原版事实源为 STS2
`v0.107.1` / `59260271` 的当前程序集和 Windows 反编译结果。

## 已修

1. 开局牌库迁移
   - 旧实现只处理 `LocalContext` 对应玩家。
   - 现按 `RunState.Players` 顺序处理所有玩家的 Shadow 卡实例与 Two Moons
     遗留费用修正。
   - 每个客户端现在对同一份完整状态执行同一组迁移。
2. `ImprisonedXii` 进入手牌后的抽牌
   - 旧实现创建 `HookPlayerChoiceContext` 后直接下传，没有把任务交给 context。
   - 若额外抽牌触发洗牌选牌，`SignalPlayerChoiceBegun` 会等待一个永远不会登记的
     Task。
   - 现复制原版 `Hook` 的生命周期：
     `HookPlayerChoiceContext -> AssignTaskAndWaitForPauseOrCompletion`。
3. Pressure / Power context
   - 已有 `OnPlay` context 的两处 Inferiority 兑换现在继续透传该 context。
   - 删除无 context 的 Pressure/Power 修改重载和会吞掉选择同步的 detached
     context。
   - `KillKiss` 回合开始伤害改用原版同类 Power 使用的
     `ThrowingPlayerChoiceContext`。
4. 战斗 watcher
   - 安装按 `CombatState.Players` 顺序执行。
   - creature 未接入当前战斗、watcher 重复或安装后数量不是 1 时直接报错，不再
     捕获后继续运行不完整战斗状态。
   - 多个祥子共用 Pressure 时，一次兑换只归属一个 watcher：祥子卡牌来源优先，
     其次祥子施加者，否则取玩家列表中的第一个存活祥子。
5. Teiji 与远古遗物
   - Teiji UI 补丁不再通过反射写 `Player.RunState`。
   - `BestCompanion` 与 `BlackLimousine` 按原版 `DustyTome.AfterObtained` 使用
     `RunState.CreateCard -> CardPileCmd.Add -> PreviewCardPileAdd`，不再直接
     `Deck.AddInternal`。
6. replay Power clone 状态
   - `MagneticForceHellWargodPower` 的 replay 集合改为原版
     `HellraiserPower` 同型的 nullable backing field 和 mutable 实例懒初始化。
   - 不同玩家和不同战斗的 Power clone 不再共享 `HashSet<CardModel>`。

## 受控例外

- watcher 仍在 `CombatManager.SetUpCombat` 后通过 `ApplyInternal` 安装。
- 原因是当前 API 没有已验证的公开“角色全局 combat hook listener”入口；此前用
  `ModHelper.SubscribeForCombatStateHooks` 构造 model 会触发
  `DuplicateModelException`。
- 本轮没有重新引入那条失败路径，而是把直接安装收紧为确定性执行和严格数量验证。

## 静态验证

- 无 context 的 `ApplyPressure`、`TryConsumePressure`、`ApplyPower` 和
  `ModifyPowerAmount` 调用已清零。
- 遗物直接写 deck 和 Teiji UI 写 `Player.RunState` 已清零。
- gameplay 随机选择继续使用 `RunState.Rng` / `PlayerRng`。
- `LocalContext` 剩余用途是本地 Jukebox/UI 玩家解析和原版 Hook action 的
  local net id，不再决定完整 RunState 的牌库迁移范围。

## 尚未证明

- 本轮只完成源码、静态审计和 Release 构建，不等于 Win/Mac 联机实机通过。
- 至少需要两名玩家覆盖：
  - 双祥子对同一目标施加 Weak/Vulnerable/负 Strength。
  - `ImprisonedXii` 额外抽牌时触发洗牌和 `StratagemPower` 选牌。
  - Shadow 存档继续与 Two Moons 遗留费用迁移。
  - Teiji 两件遗物奖励入牌组。

## 2026-09-01 远端收口

- 基线：PR #4 `8b5e17ee`。
- 六组修改已逐项对照 STS2 `v0.107.1` / `59260271` 后移植。
- Darv 已删除模组覆盖，回归原版 `GenerateInitialOptions()` 与 `DustyTome`。
- Mac 与 Windows 参考程序集构建已通过；这不等于双端实机或联机通过。
- 人物动作、Spine、Godot scene 和 PCK 未在本分支修改。
