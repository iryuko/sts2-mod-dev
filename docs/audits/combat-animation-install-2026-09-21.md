# 认可动画整套安装 · 2026-09-21

用户明确授权：安装全部已认可动画，由用户自己打开 Steam 实测并反馈；**只有反馈成功后，才继续 Git 推送与 release。** 本安装记录形成时尚待实测；用户随后反馈「测试没有问题，允许推送」，已解除推送与发布前置条件。具体发布内容见 [0.2.4 说明](../../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-21-0.2.4.md)。

## 安装内容

- 待机、放松、攻击、施法、受击：整套自然运动候选。
- 死亡：修边后的 0.21 秒快速向前趴倒；金色光环独立脱落、前滚并轻弹两次，1.72 秒消失。
- 目标攻击特效：黑红暗影荆棘，错峰显形、细枝缠绕、碎裂消散，0.31 秒命中、0.80 秒清理。

角色资源精确来自 `local/combat-spine-death-halo-roll-20260921/candidate/`，已包含其前序认可动作；荆棘来自 `local/combat-thorn-shadow-study-20260921/candidate/`。来源候选保留，未重复覆盖旧实验，V50/V87 未参与。

整合工作树：`/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev`。
分支：`codex/combat-animation-polish-20260921`，基于 `origin/main` 的本地已知提交 `d1df1c45`（tree 与正式 0.2.3 相同）。共享主工作区的卡牌研究与历史脏状态保留；发布 0.2.3 工作树未改。
本地 manifest 仍为 **0.2.3**，本轮通过下列哈希识别候选；实测成功后再处理新版本与公开分发。

## 构建与验证

1. 核对两套候选 manifest，复制认可的六项资源到隔离工作树 pack，新增光环 import 由 Godot 在正式资源路径生成；未沿用预览中的 `res://study_animation/` 路径。
2. 使用现有 `shared/scripts/build-mod.sh` 构建 .NET 与 PCK。C# 源码未改，游戏参考仍为 `v0.107.1 / 59260271`；程序集由本机工具重新编译。
3. 使用独立 Godot + Spine QA 工程加载**实际导出的 PCK**：六项候选资源哈希一致；13 张卡图、3 项图标、六种动作可加载；死亡光环在 0.6 秒存在、1.8 秒消失；新版荆棘材质、命中信号与自动清理通过。原生 OpenGL 渲染无错误。
4. 玩法回归 **107 passed, 0 failed**。首次测试宿主仅找到 .NET 10，改用已有 .NET 9 运行器后全部通过，没有下载新运行时。
5. 安装前确认游戏进程未运行，备份已安装的完整三件套，再通过 `shared/scripts/install-mod.sh --apply --allow-overwrite` 安装。
6. 安装目录恰好三件套，逐文件 SHA-256 与 staging 完全一致。未把源码、候选目录或实验脚本复制进游戏。

[本轮构建、验证与安装证据](/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev/local/combat-animation-install-20260921)包含 `build.log`、`pck-verification.log`、`gameplay-regression-runtime9.log`、`install.log`、`installation-verification.json`。

## 安装身份

时间：2026-09-21T20:06:19.181097+08:00。

目录：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire`。

| 文件 | 安装 SHA-256 |
| --- | --- |
| `Togawasakiko_in_Slay_the_Spire.dll` | `41b9092c9551c3397158be8dde81f843b408c339081d2f2dfab96fab1a623422` |
| `mod_manifest.json` | `fc0c4410f295c5c749b86dcc03f7c6ac8679249b50543f207a9561feee9a0a26` |
| `Togawasakiko_in_Slay_the_Spire.pck` | `141207bb35666b1c31eb93f7bf18b408f3d07b5231fa544b9f96ab3bf4b90228` |

覆盖前备份：`/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev/local/combat-animation-install-20260921/installed-backup`。备份为发布 0.2.3 三件套；原 PCK SHA-256 为 `c820289c7da963d42d9e4e66d100c94cee2c1a40cd6f911401e44b0f2251e193`。

## 用户实测结果

用户已从 Steam 实测并明确反馈没有问题。下面三件套哈希保留为实际通过用户测试的安装身份；0.2.4 另含发布前发现的亚像素退化荆棘绘制保护，原生边界扫描通过。

- 返回失败/瑕疵：在当前隔离分支修正，重新构建、验证和安装。
- 明确返回成功：已有授权继续整理本轮资源和验证记录、推 Git、准备新版本与 release；不要把本地大型预览、备份或其他任务内容一并提交。发布前重新核对远端与版本，保留旧 0.2.3 release。
