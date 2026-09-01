# 下一轮任务

记录日期：2026-09-01

## 当前目标

继续稳定：

- `mods/Togawasakiko_in_Slay_the_Spire`

不要扩新卡、新机制或新的 UI 框架。

## 优先级

1. 在独立发布工作树集成兼容性分支
   - 当前 active manifest 保持 `0.2.1`，不要在机制分支零散改版本。
   - 发布工作树统一升级 `v0.2.2`，重新生成 DLL、PCK、manifest、ZIP 和 SHA-256。
   - 运行 `scripts/validate-release-staging.py`，确认三份 manifest、ZIP 字节和四条哈希完全一致。
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
6. 做真实联机回归
   - 双祥子共同施加 Weak、Vulnerable 和负 Strength，确认每次 Pressure 兑换只触发一次。
   - `ImprisonedXii` 额外抽牌触发洗牌和选牌。
   - Shadow/Two Moons 存档迁移、两件 Teiji 遗物入牌组。
7. 检查人物动作 runtime
   - PR #4 已包含动作更新，但本轮没有新增 Mac/Windows 实机证据。
   - Spine 仍由独立工作继续，不能混入兼容性修复提交。

## 已完成的静态收口

- Darv 已删除模组补丁并回归原版流程。
- 联机迁移、hook task、Pressure watcher 所有权与 mutable replay 状态已收口。
- Card Library 与 Teiji 的可选私有反射已改为惰性、feature-local 失败。
- Mac/Windows `v0.107.1` 参考程序集构建均为 0 warning、0 error；真实双端和联机仍待验证。

## 证据要求

每项必须区分：

- 源码已改。
- build 通过。
- 已安装且哈希一致。
- Mac 实机通过。
- Win 实机通过。

只有最后两项才能支持相应平台“已修复”的结论。

## 完成标准

- `v0.2.2` 发布包通过 release staging identity gate。
- `UnattendedPiano` SL 路线实机闭环。
- Win 卡牌与 jukebox 两项都有同版本日志和明确结论。
- 联机关键路径有双玩家同版本日志和明确结论。
- 当前状态、角色状态和时间线同步更新。
