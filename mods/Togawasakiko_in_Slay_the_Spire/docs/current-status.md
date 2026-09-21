# Togawasakiko 当前状态

## 0.2.5：施法接入与从容应对 8／12（2026-09-21）

用户先反馈「动画合理」，再确认数值调整「测试合格」。技能、能力和四张压力衍生牌的 Cast 接入，以及从容应对 8／12 格挡均已实测认可。发布 DLL 与实测安装一致，PCK 只更新内嵌版本号；117 项回归、28 项 Python 检查、双端 API 构建和最终 PCK 原生验证通过。
版本资料、资源对照和三件套校验见[0.2.5 发布说明](releases/2026-09-21-0.2.5.md)。后续以用户新需求为准，下方安装和待测描述为历史记录。

## 从容应对 8／12 格挡已安装（2026-09-21）

用户认可 cast 动画后要求调整「从容应对」：基础 8 格挡、升级后 12 格挡。源码、实际效果和中英文动态显示已核对，117 项回归通过；已通过共享脚本安装并验证三件套哈希。PCK/manifest 与上轮相同，当前仍为 0.2.4 本地候选，新数值待 Steam 反馈，尚未推送或发布。
变更及安装身份见[从容应对调整审计](../../../docs/audits/composed-response-balance-2026-09-21.md)。

## Cast 已通过用户实测（2026-09-21）

按用户要求，祥子的技能牌、能力牌和四张压力衍生牌已接入 Cast。攻击型压力牌「满脑子都想着自己」用 Cast 替换 Attack，保持 0.31 秒荆棘命中与原伤害/眩晕。当前是公开 0.2.4 基础上的本地候选：manifest 显示 0.2.4，DLL 更新，PCK 与公开 0.2.4 完全相同；本机三件套已安装并校验。
117 项原生玩法/动作命令回归、28 项 Python 检查、Mac/Windows 引用构建通过，独立复核无待修问题。用户反馈「动画合理」，cast 实测已认可；其后提出从容应对数值调整，见上节。
隔离工作树：`/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev`；分支 `codex/cast-card-integration-20260921`。安装审计与三件套身份见 [cast-card-integration-2026-09-21.md](../../../docs/audits/cast-card-integration-2026-09-21.md)。

## 0.2.4 战斗动画与暗影荆棘（2026-09-21）

用户 Steam 实测反馈「测试没有问题，允许推送」，本轮采用的整套动作、死亡修边与光环滚落、暗影荆棘已获确认。[安装审计](../../../docs/audits/combat-animation-install-2026-09-21.md)保留实际通过测试的三件套身份。
0.2.4 的内容与验证见[发布说明](releases/2026-09-21-0.2.4.md)。发布前修复荆棘亚像素退化多边形，原生 3,543 组边界扫描通过；角色数据、贴图与玩法保持用户实测内容。
后续以用户新需求为准，不从下方旧状态自动恢复历史任务。共享主工作区的其他研究保持独立。


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
- mod version：`0.2.3`
- `min_game_version`：`0.107.1`
- STS2 reference：`v0.107.1` / `59260271`
- release 目录：
  - `exports/release/Togawasakiko_in_Slay_the_Spire/`
- 当前本地安装与 `0.2.3` release 三件套一致。

SHA-256：

- DLL：`42f223cad65812c3138b5bfb3b38a6ff7f32d1d92e2a00b491800fcc825aaad7`
- PCK：`c820289c7da963d42d9e4e66d100c94cee2c1a40cd6f911401e44b0f2251e193`
- manifest：`fc0c4410f295c5c749b86dcc03f7c6ac8679249b50543f207a9561feee9a0a26`

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
- 当前 `0.2.3` 分支继承战斗 Spine runtime；本次修复没有修改其 scene、动画或拆件。
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
