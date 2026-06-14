# 战斗 Spine 重绘设计 Brief

日期：2026-06-11

## 一 当前决策

当前战斗 `Spine` 方向改为重新设计，不再把既有静态战斗立绘拆成骨骼素材。

当前蓝图基准：

- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/redesign/blueprints/idle_right_arm_cross_left_arm_down_blueprint_v1.png`
- 该图只作为角色比例、服装、头发、站姿和画风蓝图，不作为可直接抠图的正式母版。
- 新 idle 方案：右臂抱胸，左臂自然下垂。
- 新 attack 方案：处于镜头外侧的左臂上抬，整条手臂伸直，食指斜向上约 45 度指向怪物侧。
- 旧的“右臂外展指向”方案废弃为历史参考，不再作为生产主线。

原因：

- 现有战斗立绘原本是单张完成图，不是按 `Spine` 骨骼动画生产的源文件
- 强行拆现有图会导致关节、遮挡、头发、裙摆、手臂和荆棘都缺少干净的覆盖区
- 角色动作会被原始构图限制，后续 `attack / cast / hurt / die` 的表现空间不足
- 从头按骨骼、slot 和动作需求画，更接近原版角色资源生产方式

旧目录中的分层图只作为历史参考，不作为正式制作基底：

- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/`
- `incoming_assets/character_in_combat_spine/togawasakiko/togawasakiko_spine_source_package_20260409.zip`

正式路径和运行时接口保持不变：

- `assets/animations/characters/togawasakiko/`
- `pack/animations/characters/togawasakiko/`
- `pack/scenes/creature_visuals/togawasakiko.tscn`

## 二 设计目标

目标不是把一张立绘“动起来”，而是做一套战斗中可长期使用的角色模型。

这套模型需要满足：

- 站场时读得出是丰川祥子
- 局内缩放后轮廓清楚
- 能支撑基础战斗触发动画
- 服装、头发、手臂和可攻击部件可独立运动
- 不依赖一次性构图遮挡来成立
- 后续能扩展更复杂动画，而不是只能做简单抖动

优先级：

1. 战斗待机可用
2. `attack` 和 `cast` 有明确差异
3. `hurt` 与 `die` 不缺动作
4. 后续再考虑商店、火堆、角色选择背景的动态化

## 三 角色视觉方向

建议整体维持当前项目已经建立的方向：

- Ave Mujica / Oblivionis 风格舞台感
- 红黑礼服、黑色束腰、层叠裙摆
- 银蓝长发、双马尾或左右垂坠发束
- 金色眼睛、冷淡表情
- 黑色蝴蝶结、胸前饰物、手套
- 暗红、黑、金色细线作为主要识别色

战斗模型不建议画成大幅度斜角插画。更适合：

- 近似三分之二侧身或正面偏侧
- 身体重心稳定
- 左手预留攻击动作空间；右臂主要承担抱胸 idle 角色读法
- 头发、裙摆、袖口、披片、荆棘有可动余量
- 角色整体不要太高瘦，避免在游戏内缩放后身体细节被压没

## 四 画布与构图

源图建议使用统一大画布，便于 Spine 对齐。

建议：

- 源工作画布：`2048x2048` 或 `3072x3072`
- 角色主体站姿占画布高度约 `70%` 到 `82%`
- 脚底或裙摆下缘靠近画布下方，但保留透明安全区
- 攻击部件向怪物侧或右上预留空间，当前主动作来自镜头外侧左臂上抬指向
- 不要把关键头部贴近画布顶边

游戏内 `creature_visuals` 会通过 scene 的 `position / scale / Bounds / IntentPos / CenterPos` 调整，所以源图重点是干净、完整、可切件，不需要在源图里模拟最终场景遮挡。

## 五 推荐拆件

最低推荐拆件不是按画面元素随便切，而是按运动关系切。

核心骨架：

- `root`
- `hips`
- `spine`
- `chest`
- `neck`
- `head`
- `upper_arm_l`
- `forearm_l`
- `hand_l`
- `upper_arm_r`
- `forearm_r`
- `hand_r`

头发：

- `bangs_front`
- `side_hair_l`
- `side_hair_r`
- `twin_tail_l_base`
- `twin_tail_l_mid`
- `twin_tail_l_tip`
- `twin_tail_r_base`
- `twin_tail_r_mid`
- `twin_tail_r_tip`
- `back_hair`

服装：

- `torso_red_top`
- `corset`
- `skirt_front`
- `skirt_back`
- `skirt_l`
- `skirt_r`
- `ribbon_chest`
- `ribbon_hair_l`
- `ribbon_hair_r`
- `sleeve_l`
- `sleeve_r`
- `glove_l`
- `glove_r`

攻击 / 特效部件：

- `thorn_l_base`
- `thorn_l_mid`
- `thorn_l_tip`
- `thorn_r_base`
- `thorn_r_mid`
- `thorn_r_tip`
- `red_slash_fx`
- `black_shadow_fx`
- `gold_thread_fx`

可选：

- `mask` 或 `mask_shadow`
- `stage_fragment_l`
- `stage_fragment_r`
- `dark_aura_back`

## 六 动画设计

原版战斗接口会触发这些动画名：

- `idle_loop`
- `attack`
- `cast`
- `hurt`
- `die`
- `relaxed_loop`

本项目最少也应交这些名字。即使某些动作初版很简单，也不要缺动画名。

建议动作口径：

- `idle_loop`：身体轻微呼吸，头发和裙摆低幅摆动，荆棘或暗影缓慢浮动
- `attack`：镜头外侧左臂上抬成直臂 45 度指向，荆棘 / 暗红斩击向目标方向快速打出，再回收
- `cast`：胸前或手部亮起，金色线条 / 红黑光效展开，动作比 `attack` 更克制
- `hurt`：上身轻微后仰，头发和裙摆滞后摆动，表情保持压抑
- `die`：身体失去支撑下沉或被黑影吞没，动作可短但要有结束姿态
- `relaxed_loop`：可先复用 `idle_loop` 的轻动作，但建议表情和姿态更松

第一版不需要追求复杂网格变形，但需要确保每个动画都能被引擎调用。

## 七 不建议做的事

不要：

- 直接把现有完整静态图切成几块当正式 Spine
- 把所有头发画在一整层里
- 把双臂压在身体后面导致无法做施法 / 攻击
- 把裙摆、长发、荆棘全部合进同一张 attachment
- 让 `attack` 和 `cast` 只有颜色差异，没有动作差异
- 只导出 `idle_loop / attack`，缺 `hurt / die / cast / relaxed_loop`

## 八 交稿要求

源文件阶段：

- 保留完整角色参考图
- 保留每个拆件的透明 PNG
- 所有拆件必须使用同一画布对齐，或提供 Spine 工程内已对齐的导入结果
- 明确哪些层允许 mesh deformation，哪些只做 rigid transform

运行时导出阶段：

- `togawasakiko.skel`
- `togawasakiko.atlas`
- atlas page PNG
- 预览图或预览视频
- 如有 Spine 工程文件，也一并保留到来稿区

正式安装阶段仍由项目侧整理：

- `assets/animations/characters/togawasakiko/`
- `pack/animations/characters/togawasakiko/`
- `pack/scenes/creature_visuals/togawasakiko.tscn`

## 九 接入原则

接入时不改角色主路径。

必须保持：

- `res://scenes/creature_visuals/togawasakiko.tscn`
- scene 根脚本仍为 `NCreatureVisuals`
- `Visuals` 节点仍可被 `%Visuals` 找到
- `Bounds` 节点仍可被 `%Bounds` 找到
- `CenterPos` 节点仍可被 `%CenterPos` 找到
- `IntentPos` 节点仍可被 `%IntentPos` 找到

从静态图切到 Spine 时，替换的是 `Visuals` 的内部实现，而不是改掉 T4 已接入的角色路径。

## 十 下一步

下一步应先冻结美术设计，而不是安装。

建议顺序：

1. 画一张新战斗 Spine 角色正稿草图，优先验证站姿、轮廓和攻击空间
2. 在草图上标注拆件边界与关节位置
3. 确认 `idle / attack / cast / hurt / die` 的动作方向
4. 再进入正式分层绘制
5. 最后进入 Spine 工程绑定和导出

当前最重要的产物是“可绑定的设计图”，不是又一张完成度很高但不适合拆骨骼的单幅立绘。
