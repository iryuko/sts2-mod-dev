# 战斗 Spine 第一批分层生产清单

日期：2026-06-13

## 一 目的

本文件把运动责任规范转成实际生产顺序。

当前阶段只做正式分层生产准备，不直接把临时抠图放进 runtime。

当前执行口径以 `combat-spine-blueprint-block-plan.md` 为准：先做三张大块同画布源层，再根据预览决定是否进入细拆。

原则：

- 优先做会影响动画质量的 A / B 类部件。
- C 类只在父骨骼不同、必须跟随不同大块时拆。
- D 类 overlay 先不抢生产位，除非它是接缝遮挡或特效挂点的必要条件。
- 每组部件必须有同画布透明层和组合预览，不能只交单独透明 PNG。

## 二 当前第一批范围

第一批先覆盖三张决定整体可行性的蓝图块：

1. `body_core_no_hair_no_left_arm`
2. `hair_all`
3. `arm_l_full`

旧的左臂三段、头发链条、裙摆层、腿靴等细拆方案暂时作为后续细化清单保留，不作为当前第一步。

束腰、胸口宝石、胸前领结、抱胸右臂、裙子、双腿和靴子当前并入 `body_core_no_hair_no_left_arm`，只随主体整体微晃。

## 三 生产批次

### Batch 01 主体块

目标：

- 生成没有头发、没有左臂的完整主体。
- 主体包含头脸、躯干、服装、抱胸右手、裙子、双腿和靴子。
- 主体只承担整体 idle 微晃，不做局部主动动作。

图层：

| 图层 | 运动分类 | 父骨骼 | 是否独立运动 | 必须补画 / 重叠 | 验收重点 |
| --- | --- | --- | --- | --- | --- |
| `body_core_no_hair_no_left_arm` | C | `body_root` / `torso` | 否，只整体微晃 | 左肩和左侧身体补齐；后脑、肩背、衣领边缘在移除头发后补完整 | 单独看不能有左臂洞、头发洞或脏遮罩 |

验收预览：

- `body_core_no_hair_no_left_arm_preview`
- `body_core_edge_check_preview`

说明：

- 抱胸右手并入主体块。
- 束腰、胸前饰物、裙子、腿和靴子也先并入主体块。
- 当前不是从蓝图抠图，而是以蓝图为参考生成干净主体层。

### Batch 02 头发块

目标：

- 生成单独头发层，用于 idle 微飘和 attack 后的二级动态。
- 第一版先作为整体头发块，不立即拆刘海、后发和双马尾。

图层：

| 图层 | 运动分类 | 父骨骼 | 是否独立运动 | 必须补画 / 重叠 | 验收重点 |
| --- | --- | --- | --- | --- | --- |
| `hair_all` | B | `hair_root` / `head` | 次级动态 | 发根、后脑、肩背交界处保留重叠 | 组合回主体时不露头皮洞、不压坏脸部轮廓 |

验收预览：

- `hair_all_layer_preview`
- `hair_body_assembly_preview`
- `idle_hair_sway_preview`

### Batch 03 左臂运动块

目标：

- 生成独立的整条左臂层，idle 时角度与蓝图一致。
- attack 时左臂作为主动运动件快速上抬，形成直臂斜上约 45 度指向。
- `arm_l_full` 只用于整体对位和补画检查。
- 左臂正式运动一定拆骨骼，不采用整臂刚体旋转。

图层：

| 图层 | 运动分类 | 父骨骼 | 是否独立运动 | 必须补画 / 重叠 | 验收重点 |
| --- | --- | --- | --- | --- | --- |
| `arm_l_full` | A / reference | `upper_arm_l` / `arm_l` | 对位参考；最终细拆 | 肩根藏进主体左肩；袖口和手套口预留未来细拆重叠 | idle 能无缝放回蓝图位置；后续拆成上臂、前臂和手型附件 |

验收预览：

- `arm_l_full_layer_preview`
- `idle_arm_l_body_assembly_preview`
- `attack_left_arm_45deg_timing_preview`

## 四 后续细拆清单

以下内容暂时不作为当前第一步，但在三大块验证通过后再拆：

- 左臂内部：`arm_l_upper_sleeve`、`arm_l_forearm`、`hand_l_glove_idle`、`hand_l_glove_point`
- 头发内部：`hair_front_bangs`、`hair_back_mass`、`hair_side_lock_*`、`hair_twin_tail_*`
- 裙摆：`skirt_outer_*`、`skirt_inner_cream_*`、`skirt_back_long_black`
- 腿与靴：`leg_*`
- overlay：`body_chest_bow`、`body_blue_jewel`、`hair_ribbon_*`、`fx_thorn_halo`

## 五 通用验收规则

每个图层必须满足：

- 与最终角色源图相同画布尺寸，除非 Spine 项目显式记录 offset。
- 不使用矩形裁切边界。
- alpha 边缘干净，但不能为了干净牺牲连接处补画。
- 关节和根部至少保留 `20-60px` 隐藏重叠区。
- 单层预览和组合预览都必须保留。

每个批次完成后必须输出：

- `*_layer_contact_sheet.png`
- `*_assembly_preview.png`
- `*_overlap_preview.png`
- 如涉及动作，输出 `idle` 与 `attack` 两种姿态预览。

## 六 下一步执行

下一步从 Batch 01 主体块开始。

先产出 `body_core_no_hair_no_left_arm` 的同画布透明层，然后做两个组合预览：

- 主体单独预览
- 主体与当前蓝图的对位检查

如果主体边缘和补画通过，再生成 `hair_all` 与 `arm_l_full`。
