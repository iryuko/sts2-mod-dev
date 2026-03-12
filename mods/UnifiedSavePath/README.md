# UnifiedSavePath

这是针对当前 macOS STS2 环境做的本地修正版。

目标：

- 复现并绕开公开版 `UnifiedSavePath` 在 `get_IsRunningModded()` 上的 Harmony patch 失败。
- 在当前 macOS 主机上先不依赖 Harmony patch。
- 保持安装产物仍为最小模型：`UnifiedSavePath.dll` + `UnifiedSavePath.pck`。

当前实现说明：

- 使用 `ModInitializerAttribute("Initialize")` 作为游戏内确认过的初始化入口。
- 不再调用 `PatchAll()` 扫描整程序集。
- 不再 patch `get_IsRunningModded()` / `set_IsRunningModded(bool)`。
- 不再依赖 Harmony patch `GetProfileDir(int)`。
- 改为在初始化后启动后台线程，持续把：
  - `MegaCrit.Sts2.Core.Saves.UserDataPathProvider.IsRunningModded`
  - 压回 `false`

注意：

- 这是一种为当前 macOS 运行环境准备的保守 workaround。
- 它依赖 `GetProfileDir(int32)` 在运行时读取 `IsRunningModded` 的当前值。
- 还不能宣称已经覆盖所有平台目录、账号目录或云存档分支差异。
