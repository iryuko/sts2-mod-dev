# 游戏目录布局记录

## 已记录的游戏根目录

`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2`

- 这是《Slay the Spire 2》的游戏本体根目录。
- 这是 macOS 下的 Steam 安装目录，不是开发工作区。
- 2026-03-10 实测到该目录下存在 `SlayTheSpire2.app`、`controller_config/` 和根目录级 `.DS_Store`。

## 2026-03-10 实测结构概览

游戏根目录下：

- `SlayTheSpire2.app`
- `controller_config/`
- `.DS_Store`

`.app` bundle 内的 `Contents/` 下：

- `Info.plist`
- `MacOS/Slay the Spire 2`
- `PkgInfo`
- `Resources/`
- `Frameworks/`
- `_CodeSignature/`

`Contents/Resources/` 下已观察到：

- `Slay the Spire 2.pck`
- `release_info.json`
- `PrivacyInfo.xcprivacy`
- `icon.icns`
- `data_sts2_macos_arm64/`
- `data_sts2_macos_x86_64/`

`Contents/Resources/data_sts2_macos_arm64/` 与 `data_sts2_macos_x86_64/` 下已观察到：

- `sts2.dll`
- `sts2.deps.json`
- `sts2.runtimeconfig.json`
- `0Harmony.dll`
- `GodotSharp.dll`
- 大量 .NET 运行时依赖 DLL

`Contents/Frameworks/` 下已观察到：

- `Sentry.framework`
- `libsentry.macos.release.framework`
- `libGodotFmod.macos.template_release.framework`
- `libspine_godot.macos.template_release.framework`
- `libfmod.dylib`
- `libfmodstudio.dylib`

## 与分析相关的当前结论

- 当前 macOS 发行包是 `.app` bundle 结构，不是平铺的 Windows 风格目录。
- 游戏主可执行文件位于 `Contents/MacOS/Slay the Spire 2`，经 `file` 检查为包含 `arm64` 与 `x86_64` 的通用二进制。
- 游戏主资源包 `Contents/Resources/Slay the Spire 2.pck` 存在，但体积约 `1.5G`，当前只记录路径，不复制到工作区。
- 目前未在已巡检路径中看到命名明确为 `mod`、`manifest`、`plugin` 或自定义 `loader` 的独立文件。
- 目前也未看到名为 `Managed/` 或 `Assemblies/` 的目录；macOS 包内实际承载程序集的位置是 `Resources/data_sts2_macos_arm64/` 与 `Resources/data_sts2_macos_x86_64/`。

## 从已复制程序集反向看到的资源路径线索

以下内容来自 `references/game-dlls/sts2/arm64/sts2.dll` 的字符串检查，不代表这些源码路径在 Steam 安装目录中原样存在：

- `res://src/Core/Nodes/Screens/ModdingScreen/NConfirmModLoadingPopup.cs`
- `res://src/Core/Nodes/Screens/ModdingScreen/NModInfoContainer.cs`
- `res://src/Core/Nodes/Screens/ModdingScreen/NModMenuButton.cs`
- `res://src/Core/Nodes/Screens/ModdingScreen/NModMenuRow.cs`
- `res://src/Core/Nodes/Screens/ModdingScreen/NModdingScreen.cs`
- `res://src/Core/Nodes/Screens/Settings/NOpenModdingScreenButton.cs`

当前只能确认：

- 编译后的程序集里保留了 Godot 风格的 `res://` 源路径字符串。
- 其中出现了 `ModdingScreen` 和 `OpenModdingScreenButton` 相关路径。

当前仍不能确认：

- 这些路径对应的是内部开发源码路径、导出时保留的资源标识，还是运行时直接可访问的资源路径。
- 它们与外部 mod 安装目录或用户自定义加载方式之间的真实关系。

## 后续需要检查的路径类型

这些位置值得在确认 mod 实际加载方式时进一步核实：

- 是否存在官方或约定的 mod 安装子目录。
- 两个 `data_sts2_macos_*` 目录中，后续 mod 开发应优先参考哪套程序集，还是需要同时保留两套。
- `Resources/` 下是否有与 mod 加载、清单或资源覆盖有关的文件。
- `.app` bundle 内是否存在需要保持签名或结构完整性的目录。
- 是否有日志、配置或用户数据目录位于游戏根目录之外。

## 如何区分开发工作区与游戏安装区

开发工作区：

- 用途：写源码、放模板、导出构建产物、记录分析结果。
- 位置：当前仓库目录，目标命名为 `sts2-mod-dev`。
- 可以安全清理 `exports/`、`bin/`、`obj/` 等开发产物。

游戏安装区：

- 用途：由 Steam 管理，负责启动与运行游戏。
- 位置：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2`
- 不应直接放源码，不应直接进行反编译、实验性修改或杂项文件堆放。
- 只有确认后的成品安装步骤，才应通过脚本把 `release` 产物复制进去。
