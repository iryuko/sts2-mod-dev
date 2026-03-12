# 线程接班摘要

本项目在做 STS2 本地 mod 加载链路验证，不是做功能 mod 开发。

当前已经做到：

- 工作区与游戏目录边界已固定。
- macOS 本机已确认扫描 `SlayTheSpire2.app/Contents/MacOS/mods/`。
- SmokeMod 已安装到该目录，而且最近一次游戏启动已经明确提示加载了我们的 mod。
- 安装版 `SmokeMod.dll` / `SmokeMod.pck` 与工作区发布版 SHA-256 一致，可视为同一份发布产物。
- 程序集与现有参考文件已确认存在内置 modding UI 线索：
  - 设置页中的 `%ModdingButton`
  - `OpenModdingScreen`
  - `NModdingScreen`
  - `NModMenuRow`
  - `NModInfoContainer`
  - `NConfirmModLoadingPopup`
- `UnifiedSavePath` 已做过一轮 macOS 可行性检查：
  - 本地 DLL 依赖 `sts2` 与 `0Harmony`
  - 本机 `sts2.dll` 中存在 `UserDataPathProvider.IsRunningModded` 与 `GetProfileDir(int32)`
  - 本机游戏运行时也自带 `0Harmony.dll`
  - 但 2026-03-11 实机运行已确认当前发布版初始化失败
  - 失败日志指向：Harmony patch `UserDataPathProvider::get_IsRunningModded()` 时抛 `System.NotImplementedException`
  - Steam Cloud 也证明本次运行后仍在上传 `modded/profile1/...`

当前最关键的问题：

- 游戏内到底能否直接看到内置 mod 列表 / mod 管理界面，以及 `UnifiedSavePath` 的 Harmony patch 为什么会在 macOS 上失败。

下一步该做：

- 先在游戏内直接找设置页或相关入口，确认 mod 列表 / mod 管理界面是否实际可见。
- 如需隔离存档问题，可手动删除游戏目录中的 `.../Contents/MacOS/mods/SmokeMod/`，再重新进游戏观察存档是否恢复。
- 如果决定试 `UnifiedSavePath`，先看 `docs/unified-save-path-macos-check.md`，按其中的备份与最小试用方案执行。
- 不要再重复安装当前这个 `UnifiedSavePath` 发布版做“是否能统一存档”的验证；它已经在本机日志里明确失败。
- 不要再把主要时间花在“是否识别到 mod”上；该问题当前已跨过。

接手时先看：

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/decisions.md`

默认不要先通读旧日志；只有当前结论文档不够时，再去看 `docs/findings.md` 和原始日志。
