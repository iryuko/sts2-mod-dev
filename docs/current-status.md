# 当前状态

## 卡牌悬空修复已推送（2026-09-24）

本轮清单中的九类已识别问题已完成代码修复：第一轮卡牌悬空、资源缓存、磁力重放、奥斯提误选与 Ave Mujica 费用判断，以及第二轮弹琴离场音乐、联机展示隔离、jukebox 音量叠声、Shadow 获得楼层。147 项玩法回归、11 项原生资源/音频检查、28 项 Python 检查及 Mac/Win v0.107.1 引用构建通过，原生宿主无 ERROR 或资源泄漏警告。
修复位于 `/Users/user/.codex/worktrees/card-stall-fix-20260922/sts2-mod-dev`，分支 `codex/card-stall-fix-20260922`；代码提交 `f66d1c38` 已推送并创建 [PR #10](https://github.com/iryuko/sts2-mod-dev/pull/10)，尚未合并、安装或发布。保留已发布 0.2.5，且已同步 main 的 PR #9 卡牌设计工作台；不能从共享旧源码覆盖这个基线。立绘、Spine、PCK 与另一任务的百科卡牌显示改动未动。
[审计与验收清单](audits/card-stall-and-risk-audit-2026-09-22.md)已更新推送前复核与测试夹具清理修正：147 项玩法、28 项 Python、双端 API 构建通过，11 项原生检查连续复跑 5 次无错误/泄漏告警。下一步是候选安装与同进程 SL、Windows 和双机实机回归，不能把编译/宿主测试通过当作完整实机通过；推送前构建身份见审计。


## 0.2.5：施法接入与从容应对 8／12（2026-09-21）

用户先反馈「动画合理」，再确认数值调整「测试合格」。技能、能力和四张压力衍生牌的 Cast 接入，以及从容应对 8／12 格挡均已实测认可。发布 DLL 与实测安装一致，PCK 只更新内嵌版本号；117 项回归、28 项 Python 检查、双端 API 构建和最终 PCK 原生验证通过。
版本资料、资源对照和三件套校验见[0.2.5 发布说明](../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-21-0.2.5.md)。后续以用户新需求为准，下方安装和待测描述为历史记录。

## 从容应对 8／12 格挡已安装（2026-09-21）

用户认可 cast 动画后要求调整「从容应对」：基础 8 格挡、升级后 12 格挡。源码、实际效果和中英文动态显示已核对，117 项回归通过；已通过共享脚本安装并验证三件套哈希。PCK/manifest 与上轮相同，当前仍为 0.2.4 本地候选，新数值待 Steam 反馈，尚未推送或发布。
变更及安装身份见[从容应对调整审计](audits/composed-response-balance-2026-09-21.md)。

## Cast 已通过用户实测（2026-09-21）

按用户要求，祥子的技能牌、能力牌和四张压力衍生牌已接入 Cast。攻击型压力牌「满脑子都想着自己」用 Cast 替换 Attack，保持 0.31 秒荆棘命中与原伤害/眩晕。当前是公开 0.2.4 基础上的本地候选：manifest 显示 0.2.4，DLL 更新，PCK 与公开 0.2.4 完全相同；本机三件套已安装并校验。
117 项原生玩法/动作命令回归、28 项 Python 检查、Mac/Windows 引用构建通过，独立复核无待修问题。用户反馈「动画合理」，cast 实测已认可；其后提出从容应对数值调整，见上节。
隔离工作树：`/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev`；分支 `codex/cast-card-integration-20260921`。安装审计与三件套身份见 [cast-card-integration-2026-09-21.md](audits/cast-card-integration-2026-09-21.md)。

## 0.2.4 战斗动画与暗影荆棘（2026-09-21）

用户 Steam 实测反馈「测试没有问题，允许推送」，本轮采用的整套动作、死亡修边与光环滚落、暗影荆棘已获确认。[安装审计](audits/combat-animation-install-2026-09-21.md)保留实际通过测试的三件套身份。
0.2.4 的内容与验证见[发布说明](../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-21-0.2.4.md)。发布前修复荆棘亚像素退化多边形，原生 3,543 组边界扫描通过；角色数据、贴图与玩法保持用户实测内容。
后续以用户新需求为准，不从下方旧状态自动恢复历史任务。共享主工作区的其他研究保持独立。


记录日期：2026-09-06

## 权威范围

本文件只记录当前仍有效的项目级结论。角色实现细节以：

- `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`

为准。历史 T3/T4/T5 日志已移入角色目录的 `docs/archive/phase-logs/`。

## 当前主线

当前唯一持续服务对象：

- `mods/Togawasakiko_in_Slay_the_Spire`

当前阶段是稳定性回归与兼容性收口，不是加载链研究，也不是继续扩卡。

当前游戏/API 基线：

- STS2 `v0.107.1`
- commit `59260271`
- macOS arm64/x86_64 与 Windows x86_64 参考文件均已保留

## 当前构建与安装

- manifest 版本：`0.2.3`
- `min_game_version`：`0.107.1`
- 标准成品：DLL、PCK、外部 `mod_manifest.json`
- 当前 `0.2.3` release 展开目录与本机安装目录三件套逐字节一致
- 当前 SHA-256：
  - DLL：`42f223cad65812c3138b5bfb3b38a6ff7f32d1d92e2a00b491800fcc825aaad7`
  - PCK：`c820289c7da963d42d9e4e66d100c94cee2c1a40cd6f911401e44b0f2251e193`
  - manifest：`fc0c4410f295c5c749b86dcc03f7c6ac8679249b50543f207a9561feee9a0a26`

2026-09-06 `0.2.3` bridge 卡图修复：

- `Unfinished Score`、`Following Phrase`、`Unmask`、`Rehearsal Order`、`Backstage Support`
  已从共用 `basic/unendurable.png` 改为五个独立 portrait。
- 五张图均为 `1000x760`，已完成 headless Godot 导入、PCK 构建、标准脚本安装和哈希核对。
- gameplay regression 为 `107 passed, 0 failed`；主 Python 测试 `25` 项、死亡资源测试 `28` 项通过。
- Mac/Windows `v0.107.1` 双参考构建、PCK 实际加载和三件套 ZIP 结构校验通过。
- 未启动 Steam 或游戏，实机卡框裁切与图鉴显示待确认。
- 既有 GitHub `0.2.2` Release 保持不变；本次使用新的 `0.2.3` tag 与发布资产。

2026-07-26 Steam 启动验证：

- Togawasakiko DLL、PCK 和 initializer 正常完成。
- 退出前没有 mod-loader 或 mod-initialization `ERROR`。
- 主菜单模组提示恢复非错误颜色。
- 退出进程后 Godot 仍会打印通用 renderer/resource leak 诊断；它们不属于模组加载失败。

## 当前实现规模

当前源码实际包含：

- 1 个可玩角色。
- 50 张角色卡池牌：
  - Basic 4
  - Common 14
  - Uncommon 23
  - Rare 8
  - Ancient 1
- 4 张压力衍生牌。
- 3 张事件获得 Shadow 卡。
- 2 张 relic-granted 卡。
- 1 个普通事件 `UnattendedPiano`。
- 1 个 Ancient 事件 `TogawaTeiji`。
- 5 个 relic model。
- 角色选择、静态战斗立绘、能量计数器、商店、火堆、Ancient 与音频资源。
- `0.2.3` 继承当前战斗 Spine 场景、动画与导入资产；本次卡图修复不修改该部分。
- 23 首 jukebox runtime 曲目和 1 首事件音乐。

历史 T2 的“50 张 Common/Uncommon/Rare 正常卡”是规划目标，不是当前代码事实。当前奖励可用的 Common/Uncommon/Rare 合计 45 张。

## 已收口问题

### 主菜单模组状态见红

- 根因是禁用 Watcher 的旧式 `"dependencies": ["BaseLib"]`。
- 原版迁移时登记 `MOD_ERROR.MIGRATION_REQUIRED`，全局状态又不排除禁用模组。
- Watcher manifest 已迁移为 dependency object。
- Togawasakiko manifest 已补 `min_game_version`，构建脚本会传递该字段。
- 详见 `docs/audits/workspace-cleanup-and-mod-error-2026-07-26.md`。

### v0.107.1 API 漂移

代码侧已处理：

- generated-card 和 `PowerCmd` context 参数变化。
- `Creature.CombatState -> ICombatState`。
- AOE targeting 参数变化。
- power/relic hook 虚方法签名变化。
- `NRunMusicController.UpdateTrack` 重载歧义。

构建、安装和 member-reference 静态检查已完成；Win 端完整实机回归尚未完成。

### 事件与角色本地化缺 key

- `Aroma of Chaos` 的 `TOGAWASAKIKO.aromaPrinciple` 已补。
- 同类 `goldMonologue`、`eventDeathPrevention`、多人 banter 和 `SeaGlass` 角色 title 已补。
- 修法保留原版事件流程，没有 patch 事件本体。

### Darv 空 Ancient 卡池

- 已加入 Ancient 卡 `Curseslander`，`DustyTome` 现在有合法候选。
- 卡牌效果和只影响本次生成实例的 0 费规则已落地。
- 但 `DarvPatches.cs` 仍然整体接管祥子的 `GenerateInitialOptions()`；这与“有原版流程就回归原版”的当前准则不一致，不能记为彻底收口。

## 当前必须继续验证

### `UnattendedPiano` 同进程 SL

- 根因已确认是 canonical/mutable event 浅拷贝共享 `_remainingShadows`。
- 已改为 mutable event lazy 初始化独立列表。
- 已移除重复 visited-event 写入与私有字段反射。
- 已补 `OnEventFinished()` 音乐清理。
- build/install/hash 已完成，仍缺同进程 SL 实机闭环。

### Win 卡牌卡在屏幕中央

- 已确认最初 release 绑定了多个 v0.107.1 已变化的 API。
- 当前 DLL 已移除已知高风险旧 member reference。
- 仍需要 Windows `v0.107.1` 用同一包实机确认，不能用 Mac 构建成功代替。

### jukebox 换房停歌

当前源码真实意图是：

- 选曲后跨所有 room 持续播放，包括 combat。
- 房间音乐控制器更新后重新确保自定义流播放。
- 选择 `Off (null)` 或离开 run 时才停止并恢复原版 BGM。

早期“进入 combat 自动 Off”的文档口径已失效。当前仍需 Win/Mac 实机确认换房、merchant、火堆、combat 和离开 run 的完整生命周期。

### 仍存在的代码风险

- `DarvPatches.cs` 仍复制原版选项生成逻辑，应优先评估删除。
- `MagneticForceHellWargodPower` 的运行时 `HashSet<CardModel>` 可能在 mutable power 间共享。
- `CardLibraryPatches` 等位置存在静态 private `FieldRefAccess`，版本漂移可导致整类初始化失败。
- merchant 是自定义兼容 scene：隐藏原版 Silent skeleton，显示祥子静态 portrait；它不是正式专属 Spine scene。
- 能量计数器缺 `EnergyVfxBack` / `EnergyVfxFront` 是用户明确接受的视觉缺陷，不列入本轮修复。

## 文档状态

- 项目时间线已重建：`docs/project-timeline.md`。
- 当前事实、研究、模板、旁支项目和历史日志已分层。
- 2026-03 至 2026-06 的旧阶段日志保留原文，但不再作为当前状态源。
