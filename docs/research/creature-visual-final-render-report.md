# Creature Visual Final Render Report

> 分类：原版战斗角色场景渲染方法与产物记录。

日期：2026-03-26

## 目标

将原版角色的 `scenes/creature_visuals/<character>.tscn` 导出为可直接查看的“游戏内立绘最终渲染参考图”，用于后续自定义角色静态立绘制作参考。

## 结果

已成功导出以下 5 个原版可玩角色的最终渲染参考图：

- Ironclad
- Silent
- Defect
- Necrobinder
- Regent

输出目录：

- `/Users/user/Desktop/sts2-mod-dev/images/战斗立绘成品`

同步副本目录：

- `/Users/user/Desktop/sts2-mod-dev/images/角色参考素材/游戏内立绘/<角色>/`

所有导出图尺寸均为 `1200x1200`。

## 方法

原版 `creature_visuals` 场景依赖 Spine GDExtension 与一批导出后缺失的 C# 脚本，因此不能直接用提取目录原样加载。

本次采用的是“最小渲染工程 + 净化场景副本”方式：

1. 在 `/Users/user/Desktop/sts2-mod-dev/local/creature-render-project` 建立最小 Godot 渲染工程。
2. 从提取目录链接 `animations`、`images`、`shaders`、`themes` 与 `.godot` 导入缓存。
3. 补入 Spine 运行时框架链接，不改动游戏原文件。
4. 为 `ironclad`、`silent`、`defect`、`necrobinder`、`regent` 生成去脚本版 `creature_visuals` 场景副本。
5. 通过 `/Users/user/Desktop/sts2-mod-dev/local/render_creature_visuals.gd` 离屏实例化并导出 PNG。

## 结论

- 原版战斗内角色视觉并不是单张现成 PNG，而是场景运行时组合结果。
- 现在已经可以稳定拿到“接近游戏内最终显示”的完整参考图。
- 这说明自定义角色如果不追动态效果，后续完全可以走“静态整图显示”路线；但实现上应是自定义静态显示方案，而不是往原版 Spine 场景里硬塞一张 PNG。

## 相关文件

- `/Users/user/Desktop/sts2-mod-dev/local/render_creature_visuals.gd`
- `/Users/user/Desktop/sts2-mod-dev/local/creature-visual-render-manifest.json`
- `/Users/user/Desktop/sts2-mod-dev/local/creature-render-project/project.godot`
- `/Users/user/Desktop/sts2-mod-dev/images/战斗立绘成品`
