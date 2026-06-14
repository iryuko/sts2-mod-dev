# 战斗 Spine 分层拆分计划

日期：2026-06-13

## 一 当前输入

运动责任分类以 `combat-spine-motion-classification.md` 为准。

本文件只记录图层候选、遮挡顺序和切分要求；不要把这里出现的每个可见小物件都理解为必须独立运动。

当前使用两张图共同确定拆分方案：

- `incoming_assets/character_in_combat_spine/togawasakiko/previews/spine_redesign_idle_concept_v3.png`
  - 战斗 idle 姿态目标
  - 用来判断最终站姿、面向、视线和动作气质
- `incoming_assets/character_in_combat_spine/togawasakiko/previews/spine_redesign_rig_parts_body_arms_v2.png`
  - 拆件参考底稿
  - 用来判断胸衣、头发、裙摆、手臂分离后的源图结构
- `incoming_assets/character_in_combat_spine/togawasakiko/previews/spine_redesign_rig_parts_body_arms_v2_annotated.png`
  - 第一版编号标注图
  - 只表示部件范围与命名，不表示最终矩形裁切边界

重要原则：

- 标注框不是裁切框
- 不能按矩形抠图
- 不能只按色差自动拆
- 最终源图应按不规则 alpha 轮廓、遮挡顺序和运动关系分层

## 二 拆分评判准则

优先级从高到低如下。

1. 运动关系

如果一个部分需要相对其他部分独立旋转、摆动、缩放或变形，就拆。

典型对象：

- 头相对脖子转动
- 前发相对脸部摆动
- 双马尾相对头部摆动
- 上臂绕肩转动
- 前臂绕肘转动
- 手绕腕转动
- 裙摆层相对腰部摆动
- 刺环相对头部轻微漂浮或独立晃动

2. 遮挡层级

如果两个区域在画面里有明确前后压盖关系，就拆。

典型对象：

- 前发压在脸前
- 后发在身体后
- 胸前蝴蝶结压在红色胸衣前
- 束腰压在胸衣和裙腰前
- 黑色外裙压在奶油色内裙前
- 手套口压在前臂末端
- 袖口压在上臂或前臂连接处

3. 旋转中心

如果两个区域的 pivot 不同，就拆。

典型对象：

- 上臂 pivot 在肩
- 前臂 pivot 在肘
- 手 pivot 在腕
- 头 pivot 在颈
- 双马尾根部 pivot 在发带附近
- 裙摆 pivot 在腰封下缘或裙腰

4. 材质与形变方式

材质硬度和变形方式明显不同的对象，应拆开。

典型对象：

- 黑色束腰偏硬，适合刚体或轻微整体变形
- 红色袖子偏软，需要轻微布料形变
- 长发需要链式摆动或 mesh deformation
- 奶油色内裙需要柔性摆动
- 靴子偏硬，通常跟随小腿刚体移动

反例：

- 束腰本身通常不需要独立运动方程。它应默认跟随躯干骨骼，只在遮挡层级、接缝隐藏、材质替换或技术验证需要时，才作为 `torso` bone 下的独立 slot / overlay 保留。

5. 接缝隐藏能力

切口必须尽量藏在自然遮挡边界下。

优先使用：

- 发带藏双马尾根部
- 领口藏脖子下缘
- 袖口藏上臂 / 前臂接缝
- 手套口藏腕部接缝
- 腰封下缘藏裙摆根部
- 外裙层压住内裙根部

## 三 命名规则

建议正式源图图层使用英文 snake_case，按大类排序。

格式：

- `body_*`
- `head_*`
- `hair_*`
- `arm_l_*`
- `arm_r_*`
- `hand_l_*`
- `hand_r_*`
- `skirt_*`
- `leg_l_*`
- `leg_r_*`
- `fx_*`

示例：

- `head_face`
- `hair_front_bangs`
- `hair_back_mass`
- `hair_twin_tail_l_base`
- `body_torso_red_bodice`
- `body_corset`
- `arm_l_upper_sleeve`
- `arm_l_forearm`
- `hand_l_glove_idle`
- `hand_l_glove_point`
- `skirt_outer_front`
- `skirt_inner_cream_front`
- `fx_thorn_halo`

## 四 第一版候选图层

注意：

- 本表是候选源图 / slot 清单，不等于全部都有独立运动。
- 是否需要独立驱动，以 `combat-spine-motion-classification.md` 的运动分类为准。
- D 类 overlay 和 E 类合并对象可以在正式制作时合并进父级主体层。

| 组 | 图层名 | 对应标注 | 切分边界 | 重叠 / 补画要求 | 建议 bone | 形变 |
| --- | --- | --- | --- | --- | --- | --- |
| head | `head_face` | 1 / 25 | 沿脸、耳、颈部轮廓切 | 颈部下方多留到领口内 | `head` | 刚体 |
| head | `head_neck` | 1 | 领口内侧到下颌 | 上下都要被头和衣领盖住 | `neck` | 刚体 |
| hair | `hair_front_bangs` | 2 | 刘海外轮廓，压脸前层 | 发根多留到头顶内侧 | `hair_front` | 轻 mesh |
| hair | `hair_side_lock_l` | 2 / 3 | 左侧垂发自然轮廓 | 上端藏进头发主体 | `hair_side_l` | 链式 / mesh |
| hair | `hair_side_lock_r` | 2 / 5 | 右侧垂发自然轮廓 | 上端藏进头发主体 | `hair_side_r` | 链式 / mesh |
| hair | `hair_back_mass` | 3 | 后发大块外轮廓 | 上端藏在头后，下端完整保留 | `hair_back` | mesh |
| hair | `hair_twin_tail_l_base` | 4 / 24 | 左发带下方第一段 | 根部多留在发带后 | `hair_tail_l_1` | 链式 |
| hair | `hair_twin_tail_l_mid` | 4 / 24 | 左马尾中段 | 与 base/tip 各留重叠 | `hair_tail_l_2` | 链式 / mesh |
| hair | `hair_twin_tail_l_tip` | 4 / 24 | 左马尾末端卷曲 | 上端留重叠 | `hair_tail_l_3` | mesh |
| hair | `hair_twin_tail_r_base` | 5 | 右发带下方第一段 | 根部多留在发带后 | `hair_tail_r_1` | 链式 |
| hair | `hair_twin_tail_r_mid` | 5 | 右马尾中段 | 与 base/tip 各留重叠 | `hair_tail_r_2` | 链式 / mesh |
| hair | `hair_twin_tail_r_tip` | 5 | 右马尾末端卷曲 | 上端留重叠 | `hair_tail_r_3` | mesh |
| hair | `hair_ribbon_l` | 6 / 24 | 左发带蝴蝶结轮廓 | 压住马尾根部 | `head` | 刚体 |
| hair | `hair_ribbon_r` | 6 / 25 | 右发带蝴蝶结轮廓 | 压住马尾根部 | `head` | 刚体 |
| body | `body_torso_red_bodice` | 9 | 红色胸衣主体 | 肩、腰都多留到束腰/袖下 | `chest` | 轻 mesh |
| body | `body_chest_bow` | 8 | 黑色胸前蝴蝶结 | 后方补完整胸衣，不靠它遮洞 | `chest` | 轻摆动 |
| body | `body_blue_jewel` | 8 | 蓝宝石单独切 | 可作为胸口高光件 | `chest` | 刚体 |
| body | `body_corset` | 10 | 黑色束腰和扣带 | 默认可并入躯干；若单独保留，只作为跟随 `torso` 的遮挡 overlay，上下多留压住胸衣/裙腰 | `torso` | 无独立运动 / 可轻 mesh |
| body | `body_waist_bow` | 10 / 12 | 后腰黑蝴蝶结 | 根部藏在束腰后 | `hips` | 轻摆动 |
| arms | `arm_l_upper_sleeve` | 18 | 红袖上臂 | 上端多留到肩袖下，末端藏进袖口 | `upper_arm_l` | 轻 mesh |
| arms | `arm_l_forearm` | 19 | 前臂皮肤段 | 上端藏袖口，下端藏手套口 | `forearm_l` | 刚体 |
| arms | `hand_l_glove_idle` | 20 | 下垂 idle 手套和手 | 腕部多留到手套口内 | `hand_l` | 刚体 |
| arms | `hand_l_glove_point` | 20 | attack 指向手套和手 | 腕部多留到手套口内，食指斜上 45 度可读 | `hand_l` | 刚体 |
| arms | `arm_r_upper_sleeve` | 21 | 右臂抱胸红袖上臂 | 上端多留到肩袖下，末端藏进袖口 | `upper_arm_r` | 轻 mesh |
| arms | `arm_r_forearm` | 22 | 右臂抱胸前臂 | 上端藏袖口，下端藏手套口 | `forearm_r` | 刚体 |
| arms | `hand_r_glove_idle` | 23 | 右臂抱胸手套和手 | 腕部多留到手套口内 | `hand_r` | 刚体 |
| skirt | `skirt_outer_front` | 12 | 前层黑裙摆 | 上端藏进束腰下，左右保留重叠 | `skirt_front` | mesh |
| skirt | `skirt_outer_l` | 12 / 14 | 左外层黑裙摆 | 根部藏腰封，内侧压内裙 | `skirt_l` | mesh |
| skirt | `skirt_outer_r` | 12 / 14 | 右外层黑裙摆 | 根部藏腰封，内侧压内裙 | `skirt_r` | mesh |
| skirt | `skirt_inner_cream_front` | 13 | 奶油色内裙前层 | 根部被黑裙摆压住 | `skirt_inner_front` | mesh |
| skirt | `skirt_inner_cream_l` | 13 | 左侧内裙 | 根部被外裙压住 | `skirt_inner_l` | mesh |
| skirt | `skirt_inner_cream_r` | 13 | 右侧内裙 | 根部被外裙压住 | `skirt_inner_r` | mesh |
| skirt | `skirt_back_long_black` | 14 | 后层长黑裙 | 根部藏腰后，作为后景 | `hips` | mesh |
| legs | `leg_l_thigh` | 15 | 左大腿袜段 | 上端藏裙下，下端藏靴口 | `thigh_l` | 刚体 |
| legs | `leg_l_boot` | 17 | 左靴 | 靴口覆盖小腿下端 | `shin_l` | 刚体 |
| legs | `leg_r_thigh` | 16 | 右大腿袜段 | 上端藏裙下，下端藏靴口 | `thigh_r` | 刚体 |
| legs | `leg_r_boot` | 17 | 右靴 | 靴口覆盖小腿下端 | `shin_r` | 刚体 |
| fx | `fx_thorn_halo` | 7 / 26 | 金色刺环完整轮廓 | 与头部保持独立，可轻微漂浮 | `halo` | 刚体 |

## 五 层级顺序草案

从后到前建议如下：

1. `fx_thorn_halo`
2. `hair_back_mass`
3. `hair_twin_tail_l_*`
4. `hair_twin_tail_r_*`
5. `skirt_back_long_black`
6. `leg_l_thigh`
7. `leg_r_thigh`
8. `leg_l_boot`
9. `leg_r_boot`
10. `skirt_inner_cream_*`
11. `skirt_outer_*`
12. `body_torso_red_bodice`
13. `body_corset`
14. `body_waist_bow`
15. `arm_l_upper_sleeve`
16. `arm_l_forearm`
17. `hand_l_glove_idle` / `hand_l_glove_point`
18. `arm_r_upper_sleeve`
19. `arm_r_forearm`
20. `hand_r_glove_idle`
21. `head_neck`
22. `head_face`
23. `hair_side_lock_*`
24. `hair_front_bangs`
25. `hair_ribbon_*`
26. `body_chest_bow`
27. `body_blue_jewel`

具体左右手层级会随动作变化调整。当前方案中 idle 右臂抱胸，右前臂 / 右手可能要压在胸前；attack 时镜头外侧左臂斜上 45 度指向，左前臂 / 左手需要压在躯干前。Spine 内可以通过 slot 顺序或额外附件解决。

## 六 需要补画的区域

当前生成图仍是概念参考，不是最终源图。正式分层时需要补画：

- 肩袖下方被手臂盖住的躯干边缘
- 上臂进入袖口的隐藏段
- 前臂进入袖口和手套口的隐藏段
- 头发被肩膀和身体压住的部分
- 双马尾根部被发带压住的部分
- 裙摆被束腰压住的根部
- 腿部被裙摆遮住的上端
- 靴口压住腿部的重叠段

最低要求：

- 所有关节连接处都要有 `20-60px` 级别的隐藏重叠区，具体取决于源画布尺寸
- 大幅摆动部件，如双马尾和裙摆，建议保留更大的根部重叠
- 不能刚好贴着可见轮廓裁切

## 七 当前束腰对齐试验结论

`body_corset` 当前用于验证“生成干净层如何贴回同画布”的流程，不代表束腰必须成为独立可动部件。

已确认的准则：

- 生成件的透明边缘只代表材质干净度，不代表最终可见尺寸。
- 最终可见轮廓必须以源图服装结构为准，尤其是胸下弧线、身体侧边和裙腰接缝。
- 束腰不能为了保留完整生成件而压住胸部体积，也不能向下硬盖裙腰导致裙层无法遮缝。
- 束腰默认跟随躯干运动；如果拆成单层，目的应是遮挡/接缝/材质管理，而不是单独驱动。
- 对齐时优先检查三个锚点：上缘是否停在胸下、扣带中心线是否贴合躯干轴线、下缘是否能被裙摆根部自然压住。
- 单纯矩形缩放不够时，可以做轻透视变换，但仍要用源图轮廓 envelope 裁回。
- 每个部件必须输出组合预览；只看透明单层不能判断是否可用。

当前保留的参考试验文件：

- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/redesign/same_canvas_generated_layers/body_corset_repaint_v1_refined_v2.png`
- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/redesign/body_corset_repaint_v1_refined_v2_preview.png`

该文件仍是准则样张，不是最终正式 Spine 源图。

## 八 下一步执行

下一步不应直接切现有 PNG 当正式资产。

建议顺序：

1. 按本表确认最终必需图层
2. 让美术或生成流程输出“同画布透明分层源图”
3. 每个图层保留完整 alpha 和隐藏重叠区
4. 再进入 Spine 建骨骼、slot 和 draw order
5. 用 `v3` 战斗站姿作为 idle pose 目标

如果要先做技术验证，可以用当前图粗切一个临时 prototype，但必须标记为 `workdraft`，不得进入正式 `assets/animations/characters/togawasakiko/`。
