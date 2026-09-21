# 下一轮任务

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
