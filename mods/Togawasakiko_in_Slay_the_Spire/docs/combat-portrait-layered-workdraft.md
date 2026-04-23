# 战斗立绘分层工作版

状态：已取消，不再作为当前正式实现方向。

当前正式口径已恢复为静态战斗立绘，并统一使用：
[character_in_combat_portrait_togawasakiko1.png](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/incoming_assets/character_in_combat_portrait/character_in_combat_portrait_togawasakiko1.png)

当前新增的是一个独立的 workdraft scene，用于验证祥子战斗立绘的最小拆层方案是否足够支撑：

- 起手：左手置于胸前
- 中段：左臂快速外展甩出
- 收尾：回到起手附近

## 路径

- scene：
  [togawasakiko_layered_workdraft.tscn](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/pack/scenes/creature_visuals/workdraft/togawasakiko_layered_workdraft.tscn)
- workdraft 资源：
  [workdraft](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/assets/character/in_combat_portrait_layers/workdraft)
- runtime staging：
  [workdraft](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/pack/mod_assets/character/in_combat_portrait_layers/workdraft)

## 当前结构

- `BodyBase`
- `LeftUpperArmPivot/LeftUpperArm`
- `LeftForearmPivot/LeftForearmHand`
- `FrontCover`
- `TorsoBridge`

其中 `TorsoBridge` 只用于 workdraft 外展预览阶段，目的是在切到外展参考层时补上胸口和肩口连接，避免底图挖空后中间发黑。

## 当前动画

- `idle_subtle`
- `cast_gesture_v1`

当前 `autoplay` 设为 `cast_gesture_v1`，方便直接看循环工作版动作。

## 当前限制

- 这不是原版 Spine 链路，而是 Godot 分层 workdraft。
- 当前外展姿态依赖 `extend_ref` 贴图切换，不是纯 transform 动画。
- 起手图源没有原生透明通道，所以边缘和肩口接缝仍然只是工作级别，不是正式成品。
- 当前已经临时接入正式战斗路径：
  [togawasakiko.tscn](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/pack/scenes/creature_visuals/togawasakiko.tscn)
- 原先静态版已备份到：
  [togawasakiko_static_backup.tscn](/Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/pack/scenes/creature_visuals/workdraft/togawasakiko_static_backup.tscn)

## 下一步建议

1. 先在 Godot 内看 `cast_gesture_v1` 的肩口、胸口和手臂轨迹是否成立。
2. 如果轨迹方向正确，再决定是否继续把 `extend_ref` 收敛成真正可动层，而不是贴图切换。
3. 确认 workdraft 成立后，再讨论如何把触发接到 `Attack` / `Cast`。
