# 下一轮任务

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

## 当前目标

继续稳定：

- `mods/Togawasakiko_in_Slay_the_Spire`

不要扩新卡、新机制或新的 UI 框架。

## 优先级

1. 实机确认五张 bridge 卡图
   - 在 Card Library 分别检查 `Unfinished Score`、`Following Phrase`、`Unmask`、`Rehearsal Order`、`Backstage Support`。
   - 确认五张不再显示 `Unendurable` 占位图，升级态仍使用同一正确 portrait。
   - 检查卡框裁切后的脸部、关键手势与多人构图是否可读。
2. 让 Darv 回归原版流程
   - 当前已经有合法 Ancient 卡 `Curseslander`。
   - 对照 v0.107.1 原版 `Darv.GenerateInitialOptions()` 与 `DustyTome.SetupForPlayer()`。
   - 若原版能够自然处理祥子卡池，应删除 `DarvPatches.cs`，不要继续复制原版选项表。
3. 实机闭环 `UnattendedPiano` 同进程 SL
   - 看完三张 Shadow。
   - SL 后再次选择弹琴。
   - 确认选项、发牌、事件结束和音乐 cleanup 全部正常。
4. 用同一 release 做 Windows v0.107.1 回归
   - 牌是否仍卡在屏幕中央。
   - 压力兑换是否触发。
   - 获取完整 `godot.log`、触发卡名和包 sha256。
5. 验证 jukebox 当前生命周期
   - 非战斗房选曲后依次进入 event、merchant、fire、combat。
   - 自定义曲目应持续，原版 BGM 不叠声。
   - 选择 `Off (null)` 和离开 run 后应恢复原版音乐。
6. 做角色主流程回归
   - 战斗奖励、商店、`Compose`。
   - `KillKiss` 击杀最后敌人的结算。
   - `Aroma of Chaos` 升级后离开事件。
   - Teiji 与 Touch of Orobas。

## 同轮静态修复候选

只有在上述主流程不被阻塞时处理：

- 把 `MagneticForceHellWargodPower` 的共享 `HashSet<CardModel>` 改为每个 mutable power 独立状态。
- 把卡牌库等私有 `FieldRefAccess` 改为惰性解析和 feature-local fail-closed，或改回原版公开流程。

## 证据要求

每项必须区分：

- 源码已改。
- build 通过。
- 已安装且哈希一致。
- Mac 实机通过。
- Win 实机通过。

只有最后两项才能支持相应平台“已修复”的结论。

## 完成标准

- Darv 不再依赖复制原版流程的补丁。
- 五张 bridge 卡在图鉴和卡牌详情中均显示各自正确卡图。
- `UnattendedPiano` SL 路线实机闭环。
- Win 卡牌与 jukebox 两项都有同版本日志和明确结论。
- 当前状态、角色状态和时间线同步更新。
