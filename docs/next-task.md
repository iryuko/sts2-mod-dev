# 下一轮任务

记录日期：2026-07-26

## 当前目标

继续稳定：

- `mods/Togawasakiko_in_Slay_the_Spire`

不要扩新卡、新机制或新的 UI 框架。

## 优先级

1. 让 Darv 回归原版流程
   - 当前已经有合法 Ancient 卡 `Curseslander`。
   - 对照 v0.107.1 原版 `Darv.GenerateInitialOptions()` 与 `DustyTome.SetupForPlayer()`。
   - 若原版能够自然处理祥子卡池，应删除 `DarvPatches.cs`，不要继续复制原版选项表。
2. 实机闭环 `UnattendedPiano` 同进程 SL
   - 看完三张 Shadow。
   - SL 后再次选择弹琴。
   - 确认选项、发牌、事件结束和音乐 cleanup 全部正常。
3. 用同一 release 做 Windows v0.107.1 回归
   - 牌是否仍卡在屏幕中央。
   - 压力兑换是否触发。
   - 获取完整 `godot.log`、触发卡名和包 sha256。
4. 验证 jukebox 当前生命周期
   - 非战斗房选曲后依次进入 event、merchant、fire、combat。
   - 自定义曲目应持续，原版 BGM 不叠声。
   - 选择 `Off (null)` 和离开 run 后应恢复原版音乐。
5. 做角色主流程回归
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
- `UnattendedPiano` SL 路线实机闭环。
- Win 卡牌与 jukebox 两项都有同版本日志和明确结论。
- 当前状态、角色状态和时间线同步更新。
