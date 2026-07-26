# 当前开放问题

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
