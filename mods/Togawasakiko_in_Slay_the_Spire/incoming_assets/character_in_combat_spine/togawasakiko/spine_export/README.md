# Togawasakiko Spine 导出包

当前目录用于放真正的 `Spine` 运行时导出文件。

只有以下文件进入这里时，才算“可接入原版战斗动画链”：

- `togawasakiko.atlas`
- `togawasakiko.skel`
- atlas page `png`
- 如有需要，对应 `json` 或工程预览文件

注意：

- 当前仓库里现有的 `PNG` 分层图不能直接等价替代 `.skel/.atlas`
- `.skel` 和 `.atlas` 必须由 `Spine` 工程真实导出
- 项目侧后续会基于这些文件补：
  - `togawasakiko_skel_data.tres`
  - `pack/scenes/creature_visuals/togawasakiko.tscn` 的 `SpineSprite` 接线
