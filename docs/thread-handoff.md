# 线程接班摘要

## 0.2.6 已发布（2026-09-24）

用户明确要求发布后，已发布 [0.2.6 Release](https://github.com/iryuko/sts2-mod-dev/releases/tag/togawasakiko-v0.2.6-20260924)，标记为最新正式版。Tag 指向 `d8c1e7c6`；ZIP 和校验文件的 GitHub SHA-256 均与本地一致，旧 Release 未覆盖。
包含本轮九类稳定性修复，保留 0.2.5 的立绘、Spine、施法动作和数值。571 项 PCK 内容只有版本号变化；147 项玩法、11 项原生资源/音频、28 项 Python、双端 API 构建及最终 PCK/荆棘渲染验证通过。下载与三件套身份见[发布说明](../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-24-0.2.6.md)。
本轮没有更新本机安装，PR #10 仍未合并 main；另一任务的百科卡牌全显示候选保持原状，不在本次发布中。Windows 实际运行、双机联机和完整游戏 SL/UI 仍待验收，不把构建通过当作实机通过。

## 卡牌悬空修复已推送（2026-09-24）

本轮清单中的九类已识别问题已完成代码修复：第一轮卡牌悬空、资源缓存、磁力重放、奥斯提误选与 Ave Mujica 费用判断，以及第二轮弹琴离场音乐、联机展示隔离、jukebox 音量叠声、Shadow 获得楼层。147 项玩法回归、11 项原生资源/音频检查、28 项 Python 检查及 Mac/Win v0.107.1 引用构建通过，原生宿主无 ERROR 或资源泄漏警告。
修复位于 `/Users/user/.codex/worktrees/card-stall-fix-20260922/sts2-mod-dev`，分支 `codex/card-stall-fix-20260922`；代码提交 `f66d1c38` 已推送并创建 [PR #10](https://github.com/iryuko/sts2-mod-dev/pull/10)，随后已发布 0.2.6，尚未合并 main 或安装到游戏。保留 0.2.5 内容与 PR #9 卡牌设计工作台；立绘和 Spine 不变，PCK 只更新版本号，另一任务的百科卡牌显示候选未纳入。
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

## 一句话状态

当前唯一主项目是 Togawasakiko。它已经是完整可构建、可安装、可进局的角色 mod；当前工作是 v0.107.1 双端兼容与高风险流程回归，不是从零实现角色。

## 先读

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/decisions.md`
5. `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`
6. `mods/Togawasakiko_in_Slay_the_Spire/docs/index.md`

不要先读 T4/T5 长日志。它们已归档，只在追溯具体 bug 时下钻。

## 当前版本

- STS2：`v0.107.1` / `59260271`
- mod manifest：`0.2.3`
- 当前本地安装与 `0.2.3` release 三件套哈希一致
- DLL：`42f223cad65812c3138b5bfb3b38a6ff7f32d1d92e2a00b491800fcc825aaad7`
- PCK：`c820289c7da963d42d9e4e66d100c94cee2c1a40cd6f911401e44b0f2251e193`
- manifest：`fc0c4410f295c5c749b86dcc03f7c6ac8679249b50543f207a9561feee9a0a26`
- 2026-07-26 Steam 启动：initializer 完成，主菜单无模组错误状态

## 最重要的校正

- jukebox 当前不是“进 combat 自动 Off”。
  - 源码意图是跨所有 room 持续播放。
  - 只在 `Off (null)` 或离开 run 时停止。
- merchant 不是纯原版 Silent fallback，也不是正式祥子 Spine。
  - 它是自定义兼容 scene，隐藏 Silent skeleton，显示静态祥子 portrait。
- `0.2.3` 继承当前战斗 Spine runtime；2026-09-06 的 bridge 卡图修复没有修改该部分。
- 五张 bridge 卡此前全部共用 `basic/unendurable.png`，不是只有“接续小节”错图。
  2026-09-06 已在 `0.2.3` 中改为五个独立 `1000x760` portrait，并进入安装 PCK；尚未启动游戏做视觉确认。
- Darv 目前还没有真正回归原版。
  - `Curseslander` 已解决 Ancient 空池。
  - 但 `DarvPatches.cs` 仍整体接管祥子选项生成，下一步应优先删除或证明必要性。
- T2 的“50 张正常卡”是历史规划。
  - 当前角色池共 50 张，其中 45 张是 Common/Uncommon/Rare。
- 退出 Godot 时的资源泄漏 `ERROR` 不等于 mod loader 见红。

## 当前断点

1. 五张 bridge 卡图已安装，待 Card Library 实机确认裁切和升级态。
2. `UnattendedPiano` SL 共享列表 bug 已改代码，待实机。
3. Win 卡牌卡中间的已知 API 漂移已修，待同包 Win 回归。
4. jukebox 换房保护已加，待 Win/Mac 生命周期回归。
5. Darv patch 应回归原版。
6. `MagneticForceHellWargodPower` 共享集合与 private `FieldRefAccess` 仍是静态风险。

## 必须遵守

- 先反编译当前版本原版对象，再改同类机制。
- 原版能完成的流程不另造一套。
- 不把 starter、token、event、Ancient 卡混进普通奖励。
- `Slugify(type.Name)`、本地化 key、资源文件名必须一致。
- 静态初始化不绑定高风险 private 反射。
- build、install、hash 校验必须串行。
- 不把“构建通过”写成“实机修复”。

## 标准命令

```bash
./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
```

完整历史见：

- `docs/project-timeline.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/development-timeline.md`
