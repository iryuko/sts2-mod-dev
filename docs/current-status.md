# 当前状态

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
