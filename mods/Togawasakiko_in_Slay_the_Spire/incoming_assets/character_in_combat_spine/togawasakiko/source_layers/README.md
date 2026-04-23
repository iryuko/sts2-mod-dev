# Togawasakiko Spine 源包

这不是运行时导出包。

当前目录只包含可进入 `Spine` 工程的源切图，全部保持原画布对齐：

- `togawasakiko_full_reference.png`
- `togawasakiko_body_base.png`
- `thorn_left_base.png`
- `thorn_left_mid.png`
- `thorn_left_tip.png`
- `thorn_right_base.png`
- `thorn_right_mid.png`
- `thorn_right_tip.png`

当前建议骨骼 / slot 对应关系：

- `body`
  - `togawasakiko_body_base`
- `thorn_left_base`
  - `thorn_left_base`
- `thorn_left_mid`
  - `thorn_left_mid`
- `thorn_left_tip`
  - `thorn_left_tip`
- `thorn_right_base`
  - `thorn_right_base`
- `thorn_right_mid`
  - `thorn_right_mid`
- `thorn_right_tip`
  - `thorn_right_tip`

推荐最少动画：

- `idle_loop`
- `attack`
- `cast`

version 1 动作目标：

- 人物本体不动
- 两根荆棘向右快速甩出
- 再沿逆路径回位
