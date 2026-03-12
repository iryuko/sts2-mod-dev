# 已定决策

本文件只记录已经定下来的规则与方向，避免后续重复讨论。

## 仓库与路径

- 本仓库是 STS2 mod 研究与开发工作区，不是游戏安装目录。
- 游戏根路径以 `local/game-path.txt` 为准。
- 不把 Steam 安装结构并入仓库。
- 不直接在游戏目录里开发源码、实验文件或临时脚本。

## 与游戏目录交互

- 对游戏目录的读写优先通过 `shared/scripts/` 中的脚本完成。
- 如需分析游戏文件，先复制到 `references/`，再在工作区中分析。
- 安装目标只应是最小成品，不应把整个 mod 开发目录复制进游戏目录。

## 文档与接班

- 新线程默认先读：
  - `AGENTS.md`
  - `docs/current-status.md`
  - `docs/next-task.md`
  - `docs/thread-handoff.md`
  - `docs/decisions.md`
- 历史日志和旧研究记录默认不是新线程必读内容。
- 只有当结论文档不足以支撑当前任务时，才去下钻旧文档或原始日志。

## 当前研究主线

- 当前主线是：
  - manifest 实际位置
  - mod 安装位置
  - 加载 warning / consent 规则
  - SmokeMod 最小闭环验证
- 当前不把项目重心放在复杂 mod 功能开发上。

## 已收敛的技术方向

- 当前 macOS 本机应优先按 `SlayTheSpire2.app/Contents/MacOS/mods/` 作为安装根目录处理。
- 当前最小安装产物模型以 `<ModName>.dll` 与 `<ModName>.pck` 为主。
- `mod_manifest.json` 当前应视为 PCK 内资源，而不是默认外部安装产物。

## 当前不值得继续空转的旧思路

- 不再优先空转讨论 `<GameRoot>/mods/` 与其他候选目录谁更像官方路径。
- 不再把“外部 `mod_manifest.json` 也必须安装”当作默认前提。
- 不再在缺少证据时反复猜测 STS2 官方 API、入口类或 manifest 完整 schema。
