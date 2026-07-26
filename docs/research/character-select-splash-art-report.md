# Character Select Splash Art Report

> 分类：原版角色选择资产结构研究。

日期：2026-03-22

## 结论

“开新 run 前进入角色选择界面时，每个角色在背景中显示的大幅角色立绘”在当前 STS2 资源结构里，属于角色选择背景场景资源，而不是普通 UI 图标。

对应入口场景位于：

- `res://scenes/screens/char_select/char_select_bg_ironclad.tscn`
- `res://scenes/screens/char_select/char_select_bg_defect.tscn`
- `res://scenes/screens/char_select/char_select_bg_silent.tscn`
- `res://scenes/screens/char_select/char_select_bg_necrobinder.tscn`
- `res://scenes/screens/char_select/char_select_bg_regent.tscn`

这些场景主要使用 `res://animations/character_select/<character>/` 下的资源。

## 资源形态判断

这类素材并不统一是“一张现成的大图文件”。

- Ironclad、Defect、Regent 的角色选择大立绘主体，主要由 Spine skeleton + atlas page 在运行时拼装显示。
- Silent、Necrobinder 除了角色主体 atlas page 外，还各自带有一张独立背景底图。

因此，这类素材最可能属于：

- `animations/character_select/*` 下的 Spine atlas 页
- 角色选择背景场景直接引用的背景底图

而不是：

- `images/packed/character_select/char_select_<character>.png` 这类角色选择按钮小图
- `images/ui/top_panel/character_icon_<character>.png` 这类角色头像或图标
- `scenes/creature_visuals/*`、`scenes/rest_site/characters/*`、`scenes/merchant/characters/*` 这类战斗或其他界面人物资源

## 区分依据

本次区分主要依据场景引用关系，而不是只看文件名。

- 角色选择大立绘：被 `char_select_bg_<character>.tscn` 直接引用
- 角色选择按钮小图：被 `char_select_button.tscn` 引用，属于小尺寸 UI 槽位图
- 头像或顶部图标：位于 UI 路径，服务于 top panel / 菜单 / 图鉴界面
- 战斗或其他人物素材：位于其他场景树，不属于角色选择背景场景

## 导出结果

导出目录：

- `images/立绘`

共导出 8 张 PNG：

- `Ironclad - character select atlas page.png`
- `Defect - character select atlas page.png`
- `Silent - character select atlas page.png`
- `Silent - background plate.png`
- `Necrobinder - character select atlas page 1.png`
- `Necrobinder - character select atlas page 2.png`
- `Necrobinder - background plate.png`
- `Regent - character select atlas page.png`

## 当前限制

本次没有提取失败项，但存在“无法直接得到单张成品大立绘”的情况。

- Ironclad、Defect、Regent 没有发现独立的完整合成 PNG
- Silent、Necrobinder 的主体也仍然是 Spine atlas + skeleton 组合

因此，目前提取的是角色选择场景实际使用的底层贴图资源，而不是对运行时效果进行二次重建或重新渲染后的成品图。

## 相关文件

详细机器可读清单与来源映射见：

- `local/char-select-splash-report.json`
- `local/char-select-splash-report.md`
