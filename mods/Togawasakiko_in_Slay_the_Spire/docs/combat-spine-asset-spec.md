# 战斗 Spine 资产规格

日期：2026-04-09

## 一 目标

本文件用于把“丰川祥子战斗立绘改走原版 `Spine` 路径”这件事固定成可执行规格。

结论先写清：

- 原版战斗立绘是 `SpineSprite + *.skel + *.atlas + *_skel_data.tres`
- 若要沿原版路径做动画，必须交完整 `Spine` 运行时资产
- 只有分层 `PNG` 不足以接进原版 trigger 链

## 二 原版最近邻参考

- scene 参考：
  - `references/pck-extract/sts2-main/scenes/creature_visuals/silent.tscn`
- skeleton data 参考：
  - `references/pck-extract/sts2-main/animations/characters/silent/silent_skel_data.tres`

原版最小结构就是：

1. `creature_visuals/<character>.tscn`
2. `animations/characters/<character>/<character>.atlas`
3. `animations/characters/<character>/<character>.skel`
4. `animations/characters/<character>/<character>_skel_data.tres`
5. atlas page 导出的 `PNG`

## 三 祥子建议正式命名

运行时目标路径：

- `pack/animations/characters/togawasakiko/togawasakiko.atlas`
- `pack/animations/characters/togawasakiko/togawasakiko.skel`
- `pack/animations/characters/togawasakiko/togawasakiko_skel_data.tres`
- `pack/animations/characters/togawasakiko/togawasakiko.png`
  - 如果 atlas 导出多页，则按导出结果保留多页

工作区正式库存：

- `assets/animations/characters/togawasakiko/`

来稿区：

- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/`
- `incoming_assets/character_in_combat_spine/togawasakiko/spine_export/`
- `incoming_assets/character_in_combat_spine/togawasakiko/previews/`

## 四 最少需要哪些动画名

为了兼容原版角色战斗触发，最少建议准备：

- `idle_loop`
- `attack`
- `cast`

后续建议补：

- `hurt`
- `die`

如果你这轮只先做“荆棘抽打”，那也不要只交单独一条动画。至少要把：

- `idle_loop`
- `attack`

一起交出来，否则 scene 虽能进 `Spine`，但战斗默认待机仍不完整。

## 五 最少骨骼 / slot 思路

当前不要求你立刻做全身复杂骨骼。

如果本轮只做“人物静止 + 荆棘抽打”，最小建议：

- `root`
- `body`
- `thorn_left_base`
- `thorn_left_mid`
- `thorn_left_tip`
- `thorn_right_base`
- `thorn_right_mid`
- `thorn_right_tip`

人物本体可以先做成静态 attachment，不强求全身骨骼动画。

## 六 当前这轮最适合的版本 1

version 1 推荐做法：

- 人物本体不动
- 黑影与静态背景元素做成固定 attachment
- 两根目标荆棘各拆 `base / mid / tip`
- `attack` 时让两根荆棘向右甩出，再回位
- `idle_loop` 先允许基本静止

这条路线仍然是原版路径，因为驱动者已经从自定义 Godot scene 变成 `SpineSprite`

## 七 你交稿时我需要看到什么

最理想：

1. `Spine` 导出运行时包
2. 对应 atlas page `PNG`
3. 预览截图或小视频
4. 若有的话，再附原始工程文件

最低可用：

1. `togawasakiko.atlas`
2. `togawasakiko.skel`
3. atlas page `PNG`

我可以在项目里补：

- `togawasakiko_skel_data.tres`
- `creature_visuals/togawasakiko.tscn` 的 `SpineSprite` 接线

## 八 当前已确认与待确认

已确认：

- 原版战斗动画核心走 `Spine`
- 我们项目当前没有任何 `Spine` runtime 资产
- 继续用自定义 Godot Tween 不是这条路线

待确认：

- 祥子这套 `Spine` 最终 atlas page 数量
- 是否需要在 `attack` 与 `cast` 上做完全不同的动画
- 是否在 version 1 就补 `hurt / die`
