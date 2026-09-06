# Togawasakiko 当前状态

记录日期：2026-09-06

## 事实源

本文件是角色目录内的当前实现事实源。优先级：

1. 当前源码、manifest 和 scene。
2. 当前 release 与安装哈希。
3. 本文件。
4. 专项审计。
5. `docs/archive/phase-logs/` 中的历史过程记录。

## 版本与成品

- manifest id：`Togawasakiko_in_Slay_the_Spire`
- mod version：`0.2.2`
- `min_game_version`：`0.107.1`
- STS2 reference：`v0.107.1` / `59260271`
- release 目录：
  - `exports/release/Togawasakiko_in_Slay_the_Spire/`
- 当前本地 post-release 热修安装与 release 三件套一致。

SHA-256：

- DLL：`53c6bce1e932df648b63ebfb65c2000f11bc47547ac1914c1f3630ffb48da709`
- PCK：`829e3b68f10acf37b184aceb99382399efcc780c92eaf828b25fe79addbd4047`
- manifest：`3d06dd0b6f26e0ecdf36196a275d8f8f6f969cb18174ab8c0fecef6fbf38f624`

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
- 状态：代码、build、install、hash 完成；同进程 SL 实机待验证。

### `TogawaTeiji`

- 自定义 `AncientEventModel`。
- `ModelDb.AllAncients` 全局注册；仅在 run 中存在祥子时加入对应 act 的 Ancient 池。
- 支持 agnostic dialogue，混合多人队伍可参与奖励。
- 奖励包括 `BestCompanion`、`BlackLimousine` 和继续演出分支。
- 地图节点、对话头像、主图和背景 scene 已接入 runtime。

### `Aroma of Chaos`

- 不 patch 原版事件。
- 已补 `TOGAWASAKIKO.aromaPrinciple` 及其他按角色拼接的本地化 key。
- 状态：代码/build/install 已完成，升级后离场仍应纳入回归测试。

### Darv

- `Curseslander` 已使 `DustyTome.SetupForPlayer()` 有合法 Ancient 候选。
- 当前仍存在 `DarvPatches.cs`，会对祥子整体接管 `GenerateInitialOptions()`。
- 该 patch 是空池时期的过渡防护，现与原版优先准则冲突。
- 状态：应对照 v0.107.1 原版后删除或证明仍不可替代。

## jukebox

当前源码行为目标：

- 选曲后跨所有 room 保持播放，包括 combat。
- 共享 `AudioStreamPlayer` 挂在 scene tree root，不依赖 overlay 生命周期。
- `RoomEntered` 与 `NRunMusicController.UpdateMusic/UpdateTrack/UpdateAmbience` 后重新确认播放。
- 自定义曲目播放时压住原版 FMOD/Godot BGM。
- 选择 `Off (null)` 或离开 run 时停止，并恢复用户 BGM。

早期“进入 combat 自动 Off”的文档已失效。当前仍需 Mac/Win 换房生命周期实机回归。

## 视觉与资产

- 五张 bridge 卡已在 2026-09-06 从共用占位图切换为独立 portrait：
  `unfinished_score.png`、`following_phrase.png`、`unmask.png`、`rehearsal_order.png`、`backstage_support.png`。
- 五张图均为 `1000x760`，源码、`assets/`、`pack/`、Godot import、PCK 和安装路径已经闭环。
- 当前 `0.2.2` 分支继承战斗 Spine runtime；本次热修没有修改其 scene、动画或拆件。
- merchant/rest site 使用自定义兼容 scene 和静态 portrait。
- merchant scene 内 Silent skeleton 仅用于满足原版节点契约，当前隐藏。
- 能量计数器缺 `EnergyVfxBack` / `EnergyVfxFront` 是明确接受的视觉缺陷。
- 全部当前卡牌 model 均有对应源 portrait；详见 `asset-status.md`。
- 五张 bridge 卡尚缺游戏内卡框裁切和升级态视觉确认，不能仅凭 PCK 检查写成实机通过。

## 当前风险

1. `UnattendedPiano` SL 修复未做实机闭环。
2. v0.107.1 Win 卡牌卡中间与 jukebox 换房仍缺同包实机结果。
3. `DarvPatches.cs` 复制原版流程，应该回收。
4. `MagneticForceHellWargodPower._cardsQueuedForReplay` 可能在 mutable power 间共享。
5. `CardLibraryPatches` 等静态 private `FieldRefAccess` 在版本漂移时可能拖死整个 feature 初始化。
6. `KillKiss` 最后一击、奖励、商店和 `Compose` 仍需成体系回归。

## 当前不做

- 不扩新卡。
- 不添加新机制。
- 不重写角色框架。
- 不修已接受的能量计数器 VFX 缺口。
- 不把构建通过写成实机通过。
