# 原版角色攻击 Spine 资产对照

日期：2026-06-13

## 一 目的

本文件用于回答：

- 攻击动作里切换手型 / 手臂附件会不会突兀。
- 原版角色是更依赖骨骼变形，还是会准备 attack 专用附件。
- 祥子右手 `idle` / `point` 双附件策略是否合理。

## 二 检查对象

参考路径：

- `references/pck-extract/sts2-main/animations/characters/`
- `references/pck-extract/sts2-main/.godot/imported/`

检查对象：

- `silent`
- `ironclad`
- `defect`
- `necrobinder`
- `regent`

说明：

- 原版 skeleton 是 Godot Spine 导入后的二进制 `.spskel`，当前没有直接反编译完整动画曲线。
- 本次结论来自 `.tres` 动画 mix 设置、`.spatlas` 区域名，以及 `.spskel` 的字符串证据。
- 因此本文件记录的是“高可信资产结构结论”，不是逐帧曲线复原。

## 三 动画 mix 事实

从 `*_skel_data.tres` 读取到的混合时间：

| 角色 | `default_mix` | 关键 mix |
| --- | --- | --- |
| `silent` | `0.05` | `idle_loop -> hurt = 0.02`，`cast -> idle_loop = 0.02` |
| `ironclad` | `0.05` | `idle_loop -> attack = 0.1`，`hurt -> idle_loop = 0.1` |
| `defect` | 未显式设置 | 未见 mix 表 |
| `necrobinder` | `0.02` | `idle_loop -> attack = 0.02`，`hurt -> die = 0.02` |
| `regent` | `0.05` | `idle_loop -> hurt = 0.02`，`hurt -> die = 0.05` |

结论：

- 原版允许攻击动画快速切入。
- Ironclad 的 `idle_loop -> attack` 有 `0.1s` 过渡，较明显地避免硬切。
- Necrobinder 的攻击切入只有 `0.02s`，说明如果动作设计够清晰，原版也接受非常短的切入。

## 四 atlas 区域证据

### Silent

相关区域：

- `top hand`
- `top hand 2`
- `back hand`
- `back hand 2`
- `shiv_hand`
- `cape_back_attack`
- `cape_front_attack`

判断：

- Silent 不只靠一个手型变形。
- 存在多个手型 / 武器 / attack cape 相关区域。

### Ironclad

相关区域：

- `attack/front hand attack`
- `attack/front arm attack`
- `attack/front bracer attack`
- `attack/bottom hand attack`
- `attack/bottom lower arm attack`
- `attack/bottom upper arm attack`
- `attack/bod attack`
- `attack/l foot attack`
- `attack/r lower leg attack`

判断：

- Ironclad 明确有整套 `attack/` 专用身体与肢体附件。
- 这不是“idle 部件靠骨骼硬掰成攻击姿态”，而是攻击姿态专用素材 + 骨骼动画 + mix。

### Necrobinder

相关区域：

- `r hand`
- `r_chill_hand`
- `open_hand`
- `r index`
- `r middle finger`
- `r ring finger`
- `r lower arm`
- `r upper arm`

判断：

- Necrobinder 存在手型变体和手指级附件。
- 这支持“复杂手势不应强行由单张手图变形”的策略。

### Regent

相关区域：

- `bottom hand`
- `bottom hand freaked`
- `top hand 1`
- `hand`
- `hand copy`
- `top arm 1`
- `top arm 1 flipped`
- `top arm 2`
- `top arm 2 flipped`

判断：

- Regent 也存在手型 / 手臂变体。
- `flipped` 和 `freaked` 说明原版会为姿态变化准备替代附件。

## 五 对祥子手型附件策略的结论

`idle` / `point` 双附件策略是合理的。当前生产主线已从旧的右手指向改为左手指向，因此本结论应用到 `hand_l_glove_idle` / `hand_l_glove_point`。

必须满足以下条件：

- 两个附件绑定同一个 `hand_l` slot，不新建一套手骨骼。
- `attack` 动画中切换到 `hand_l_glove_point`。
- 切换点前后用左前臂快速运动、袖口遮挡、身体前倾和特效节奏掩盖视觉变化。
- 切换不能发生在静止帧；应发生在手臂开始甩出或速度最高的区间。
- 需要设置 `idle_loop -> attack` 的短 mix。建议初始值 `0.05s`，后续可在 `0.02-0.1s` 范围内试。

## 六 不建议的做法

不建议只做一个 `hand_l_glove`，再靠 mesh 或骨骼把放松手拉成指向手。

原因：

- 指节、手套褶皱、食指可读性会变形。
- 攻击手势是高注意力区域，瑕疵会很明显。
- 原版 Ironclad / Necrobinder / Regent 都提供了“攻击或特殊姿态专用附件”的证据。

## 七 推荐实现口径

祥子当前 Batch 01 左臂建议：

- `arm_l_upper_sleeve`
- `arm_l_forearm`
- `hand_l_glove_idle`
- `hand_l_glove_point`

动画建议：

1. `idle_loop` 使用 `hand_l_glove_idle`，左臂自然下垂。
2. `attack` 开始时左肩和前臂先启动。
3. 手臂进入快速上抬 / 外展阶段时切换 `hand_l_glove_point`。
4. 指向姿态停留短暂高可读帧。
5. 回收时可切回 `hand_l_glove_idle`，或在回收末端切回，具体以预览为准。

验收标准：

- 正常速度播放时不能感到一帧闪烁。
- 停帧检查允许看到 attachment 切换，但连接处必须被袖口 / 手套口自然遮住。
- 若切换明显，优先调整切换时机和 mix，而不是取消双手型策略。
