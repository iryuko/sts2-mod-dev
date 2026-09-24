# 当前开放问题

## 2026-09-22 审计更新

本轮九类已识别问题已完成代码修复与针对性测试：卡牌悬空、弹琴静态音频缓存、磁力嵌套重放、奥斯提误选、Ave Mujica 免费打出，以及第二轮的弹琴异常离场音乐、联机远端展示隔离、jukebox 音量叠声、Shadow 获得楼层元数据。2026-09-24 已推送 [PR #10](https://github.com/iryuko/sts2-mod-dev/pull/10)，尚未合并、安装或发布。
当前剩余的是候选安装后同进程 SL、双端实际音频、双机传输与完整 UI 的实机验收，不再把上述四项列为未实现。证据与回归清单见[本轮审计](../../../docs/audits/card-stall-and-risk-audit-2026-09-22.md)。
下方旧 `_cardsQueuedForReplay` 疑问已由原版次数钩子替代解决，不应再次维护这个已删除的集合。其余旧开放项须按当前源码和实机证据判断。

记录日期：2026-07-26

本文件只保留尚未由当前源码、当前版本反编译或实机测试回答的问题。

## 流程与兼容

1. 删除 `DarvPatches.cs` 后，v0.107.1 原版 Darv 是否能凭 `Curseslander` 完整生成选项并发放 `DustyTome`？
2. `UnattendedPiano` 看完三张 Shadow 后同进程 SL，是否已恢复完整弹琴路线？
3. Windows v0.107.1 上，当前包是否仍有牌停在屏幕中央？
4. jukebox 是否能在 Mac/Win 跨 event、merchant、fire、combat 持续播放，并在离开 run 后正确恢复原版 BGM？
5. `KillKiss` 击杀最后敌人后，胜利与奖励结算是否彻底闭环？

## 状态生命周期

1. `MagneticForceHellWargodPower._cardsQueuedForReplay` 是否会因 mutable clone 共享集合而跨实例污染？
2. 卡牌库等静态 private `FieldRefAccess` 在下一次本体更新时如何改为惰性、局部失败，或回归公开流程？
3. Shadow 事件在多人、保存退出和异常离场下是否都能正确 cleanup 音乐与状态？

## 视觉与资源

1. 独立制作中的战斗 Spine 是否完成 idle/attack/hit/cast/death 实机验收，并可单独提交？
2. merchant 和 rest site 是否继续使用静态兼容 scene，还是进入正式专属 Spine 制作？
3. 锁定图、top panel outline 和部分 UI 资产是否需要最终视觉重做？

## 暂不讨论

- 新卡数量。
- 新机制分支。
- 大规模平衡调整。
- 已明确接受的能量计数器 VFX 节点缺口。
