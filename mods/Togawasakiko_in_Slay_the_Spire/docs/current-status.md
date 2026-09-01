# Togawasakiko 当前状态

记录日期：2026-09-01

## 事实源

本文件是角色目录内的当前实现事实源。优先级：

1. 当前源码、manifest 和 scene。
2. 当前 release 与安装哈希。
3. 本文件。
4. 专项审计。
5. `docs/archive/phase-logs/` 中的历史过程记录。

## 版本与成品

- manifest id：`Togawasakiko_in_Slay_the_Spire`
- mod version：`0.2.1`
- `min_game_version`：`0.107.1`
- STS2 reference：`v0.107.1` / `59260271`
- 兼容性分支 active manifest 仍为 `0.2.1`；`v0.2.2` 在独立发布工作树统一升级。
- 本分支未安装、打包或发布；同一份源码对 Mac/Windows 参考程序集均为 0 warning、0 error。
- release 目录：
  - `exports/release/Togawasakiko_in_Slay_the_Spire/`
- 2026-07-26 本机安装与 release 三件套一致；以下哈希不包含 2026-09-01 兼容性分支源码修复。

SHA-256：

- DLL：`bb26e7872d3560d35fcafecbf436181e158072ec54ac8b9fc584c2eb3fa7f500`
- PCK：`4b92969d63a405f7c12c6aa1065cf206fe84bc8aac5373ae5b63a9fcb7d2ebac`
- manifest：`7f1166000fb9ce0e0a74197989e151d7cfbf9c582988168d557759076f4c9cd7`

## 角色底盘

- 起始生命：65。
- 起始金币：99。
- 起始牌组：
  - 4 `StrikeTogawasakiko`
  - 4 `DefendTogawasakiko`
  - 1 `Slander`
  - 1 `Unendurable`
- 起始 relic：`DollMask`。

当前源码数值：

- `Slander`
  - 1 费，4 基础伤害。
  - 目标每层压力额外 +2 伤害。
  - 升级后 0 费。
- `Unendurable`
  - 2 费，8 格挡，施加 3 压力。
  - 升级后 11 格挡，施加 4 压力。

旧设计文档中的 `Slander +3/层`、`Unendurable 5 格挡/2 压力` 已被当前代码和本地化取代。

## 卡牌与池

`TogawasakikoCardPool` 当前共 50 张：

| Rarity | 数量 |
| --- | ---: |
| Basic | 4 |
| Common | 14 |
| Uncommon | 23 |
| Rare | 8 |
| Ancient | 1 |

另外注册：

- 压力衍生牌 4 张，位于原版 `TokenCardPool`。
- Shadow 事件牌 3 张，位于自定义 event-granted pool。
- relic-granted 卡 2 张。

普通奖励资格明确排除：

- Basic。
- Ancient。
- starter。
- 缺本地化或不应显示于图鉴的牌。

历史 T2 的 `12 Common / 22 Uncommon / 16 Rare = 50` 是扩展规划，不是当前实现分布。

## 压力与 Curseslander

- 压力是目标身上的共享 counter。
- 多个祥子对同一目标施加的压力合并，不按玩家分账。
- 4 张压力衍生牌均为 Token、Ethereal、Exhaust，不能被原版 modifier 随机生成。

`Curseslander`：

- Ancient Attack，1 费，升级后 0 费。
- 继承 `Slander` 的 4 基础伤害与每层压力 +2。
- 从压力衍生池有放回抽 2 张加入手牌。
- 只把本次新创建的两个实例改为本场 0 费。
- 不改变 canonical，也不影响其他来源生成的同名牌。

## 事件与 Ancient

### `UnattendedPiano`

- 普通 `EventModel`，不是 Ancient。
- 仅在所有玩家均为 Togawasakiko 时允许进入事件池。
- Shadow 发放走原版 `CardPileCmd.AddCursesToDeck(...)`。
- 2026-07-23 已修 canonical/mutable event 共享 `_remainingShadows` 列表。
- visited-event 交回原版 `ActModel.PullNextEvent` / `RoomSet`。
- `OnEventFinished()` 负责音乐 cleanup。
- 状态：旧 release 的代码、build、install、hash 已完成；本分支没有新增实机证据，同进程 SL 仍待验证。

### `TogawaTeiji`

- 自定义 `AncientEventModel`。
- `ModelDb.AllAncients` 全局注册；仅在 run 中存在祥子时加入对应 act 的 Ancient 池。
- 支持 agnostic dialogue，混合多人队伍可参与奖励。
- 奖励包括 `BestCompanion`、`BlackLimousine` 和继续演出分支。
- 两件遗物已按原版 `RunState.CreateCard -> CardPileCmd.Add -> PreviewCardPileAdd` 添加奖励牌。
- UI 描述补丁不再写回 `Player.RunState`；`_event` 字段改为功能首次使用时惰性解析，失败则继续原版流程。
- 地图节点、对话头像、主图和背景 scene 已接入 runtime。
- 状态：源码契约与双参考程序集构建通过，实机奖励/描述仍待验证。

### `Aroma of Chaos`

- 不 patch 原版事件。
- 已补 `TOGAWASAKIKO.aromaPrinciple` 及其他按角色拼接的本地化 key。
- 状态：代码/build/install 已完成，升级后离场仍应纳入回归测试。

### Darv

- `Curseslander` 已使 `DustyTome.SetupForPlayer()` 有合法 Ancient 候选。
- 已删除空池时期的 `DarvPatches.cs`，由原版 `GenerateInitialOptions()` 与 `DustyTome` 生成奖励。
- 状态：源码契约与 Mac/Windows 参考程序集构建通过，双端实机仍待验证。

## 联机与初始化收口

- Shadow 与 Two Moons 开局迁移按 `RunState.Players` 顺序覆盖所有玩家。
- `ImprisonedXii` hook 抽牌任务通过 `AssignTaskAndWaitForPauseOrCompletion` 进入原版同步队列。
- 多祥子共享 Pressure 时，卡牌 owner、施加者、玩家顺序共同决定唯一兑换 watcher。
- `MagneticForceHellWargodPower` replay 集合只在 mutable power 实例上懒初始化。
- Card Library 私有字段改为惰性解析；字段缺失只禁用祥子筛选按钮，不阻断模组初始化。
- 当前自动化：21 项标准库契约/发布测试，Mac/Windows `v0.107.1` 双参考构建 0 warning、0 error。
- 未证明：Mac 实机、Windows 实机及真实双玩家同步。

## jukebox

当前源码行为目标：

- 选曲后跨所有 room 保持播放，包括 combat。
- 共享 `AudioStreamPlayer` 挂在 scene tree root，不依赖 overlay 生命周期。
- `RoomEntered` 与 `NRunMusicController.UpdateMusic/UpdateTrack/UpdateAmbience` 后重新确认播放。
- 自定义曲目播放时压住原版 FMOD/Godot BGM。
- 选择 `Off (null)` 或离开 run 时停止，并恢复用户 BGM。

早期“进入 combat 自动 Off”的文档已失效。当前仍需 Mac/Win 换房生命周期实机回归。

## 视觉与资产

- 当前提交中的战斗角色 scene 继续使用静态 portrait。
- 新 Spine runtime 正在独立制作，场景、动画与导入资产将在完成后单独提交。
- merchant/rest site 使用自定义兼容 scene 和静态 portrait。
- merchant scene 内 Silent skeleton 仅用于满足原版节点契约，当前隐藏。
- 能量计数器缺 `EnergyVfxBack` / `EnergyVfxFront` 是明确接受的视觉缺陷。
- 全部当前卡牌 model 均有对应源 portrait；详见 `asset-status.md`。

## 当前风险

1. `UnattendedPiano` SL 修复未做实机闭环。
2. v0.107.1 Win 卡牌卡中间与 jukebox 换房仍缺同包实机结果。
3. watcher 仍在 `CombatManager.SetUpCombat` 后使用 `ApplyInternal`；真实联机时序待验证。
4. Teiji、Aroma、`KillKiss` 最后一击、奖励、商店和 `Compose` 仍需成体系回归。
5. PR #4 人物动作更新尚无本轮 Mac/Windows runtime 证据；Spine 继续由独立工作处理。

## 当前不做

- 不扩新卡。
- 不添加新机制。
- 不重写角色框架。
- 不修已接受的能量计数器 VFX 缺口。
- 不把构建通过写成实机通过。
- 不在兼容性分支改版本、PCK、动作、Spine 或其他视觉资产。
