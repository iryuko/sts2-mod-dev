# Character Select Final Render Report

> 分类：原版角色选择场景渲染方法与产物记录。

日期：2026-03-22

## 目标

本次目标不是继续导出角色选择背景场景使用的 atlas page，而是直接得到 STS2 角色选择界面中每个角色“背景大立绘”的最终渲染成品图，作为后续 AI 作图参考素材。

## 结论

当前 5 个角色选择背景场景都已成功渲染为成品 PNG：

- Ironclad
- Defect
- Silent
- Necrobinder
- Regent

输出目录：

- `images/立绘成品`

所有成品图尺寸均为 `2560x1200`。

## 渲染方法

采用的是“实际场景渲染导出”，不是人工截图，也不是只提 atlas page。

渲染入口场景：

- `res://scenes/screens/char_select/char_select_bg_ironclad.tscn`
- `res://scenes/screens/char_select/char_select_bg_defect.tscn`
- `res://scenes/screens/char_select/char_select_bg_silent.tscn`
- `res://scenes/screens/char_select/char_select_bg_necrobinder.tscn`
- `res://scenes/screens/char_select/char_select_bg_regent.tscn`

执行方式：

1. 使用提取后的 `references/pck-extract/sts2-main` 作为项目路径启动 STS2 自带 Godot 运行时。
2. 通过 `local/render_char_select_splashes.gd` 脚本离屏创建 `SubViewport`。
3. 将每个 `char_select_bg_<character>.tscn` 实例化到 `2560x1200` 的视口中。
4. 等待若干帧，让 Spine skeleton、贴图拼装和场景内效果稳定。
5. 直接调用 `ViewportTexture.get_image().save_png(...)` 导出成品图。

关键点：

- `--headless` 模式下会落到 dummy renderer，拿不到有效纹理，因此最终导出使用的是正常渲染器，而不是 headless。
- 成品图来自场景运行时最终显示结果，不是从资源目录中直接拷出单张现成 PNG。

## 成功导出项

- `images/立绘成品/Ironclad - character select final render.png`
- `images/立绘成品/Defect - character select final render.png`
- `images/立绘成品/Silent - character select final render.png`
- `images/立绘成品/Necrobinder - character select final render.png`
- `images/立绘成品/Regent - character select final render.png`

## 当前状态

这 5 个角色目前都已得到“最终观感的大图”。

因此，当前没有停留在“只能拿到素材层”的角色，也没有卡在 skeleton 渲染、场景依赖或导出流程的角色。

## 相关文件

- `local/render_char_select_splashes.gd`
- `local/char-select-render-manifest.json`
- `images/立绘成品`
