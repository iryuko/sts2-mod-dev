# DLL Metadata Notes

记录日期：2026-03-10

本文件只记录从已复制 DLL 与元数据文件中直接提取到的证据，不记录未经验证的业务推断。

## 证据来源

- `references/game-dlls/sts2/arm64/sts2.dll`
- `references/game-dlls/sts2/x86_64/sts2.dll`
- `references/game-dlls/shared/0Harmony.dll`
- `references/game-dlls/shared/GodotSharp.dll`
- `references/api-notes/sts2/sts2.deps.arm64.json`
- `references/api-notes/sts2/sts2.deps.x86_64.json`
- `references/api-notes/sts2/sts2.runtimeconfig.arm64.json`

## 已确认

- `sts2.dll` 是 .NET/CLR 程序集：
  - `file` 识别为 `Mono/.Net assembly`
  - `objdump -x` 可见 `CLR Runtime Header`
- `arm64` 版 `sts2.dll` 目标框架字符串中出现 `.NETCoreApp,Version=v9.0`。
- `sts2.runtimeconfig.arm64.json` 显示：
  - `tfm`: `net9.0`
  - `Microsoft.NETCore.App`: `9.0.7`
- `sts2.deps.json` 显示主程序集项为 `sts2/0.1.0`。
- `sts2.deps.json` 明确列出依赖：
  - `GodotSharp 4.5.1`
  - `0Harmony 2.4.2.0`
  - `Steamworks.NET 1.0.0.0`
  - `Sentry 5.0.0`
- `0Harmony.dll` 字符串中可见：
  - `0Harmony, Version=2.4.2.0`
  - `.NETCoreApp,Version=v9.0`
- `GodotSharp.dll` 字符串中可见：
  - `GodotSharp`
  - `.NETCoreApp,Version=v8.0`
- `sts2.dll` 中可见的完整命名空间字符串至少包括：
  - `MegaCrit.Sts2.Core.Modding`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen`
  - `MegaCrit.Sts2.Core.Nodes.Screens.Settings`
  - `MegaCrit.Sts2.Core.Nodes.GodotExtensions`
  - `MegaCrit.Sts2.Core.Assets`

## 高可能

- `MegaCrit.Sts2.Core.Modding.ModManager`、`ModManifest`、`ModSource` 这些名称一起出现，高可能说明程序集内部存在一组与 mod 数据或 mod 管理相关的类型。
- `NModdingScreen`、`NConfirmModLoadingPopup`、`NModInfoContainer`、`NModMenuButton`、`NModMenuRow`、`NOpenModdingScreenButton` 这些名字一起出现，高可能说明游戏内部有一个与 modding 相关的 UI 流程。
- `TryLoadModFromPck`、`pck_name`、`pckFilename` 同时出现，高可能说明某处逻辑会把 `.pck` 作为候选输入之一。
- `GetModdedLocTables`、`ConcatModelsFromMods`、`GetSubtypesInMods` 这些名称一起出现，高可能说明游戏内部考虑过来自 mods 的模型或本地化扩展。

## 待验证

- `ModManager`、`ModManifest`、`ModSource` 的实际类型签名。
- `TryLoadModFromPck` 的调用位置、参数和返回类型。
- `pck_name`、`mods_enabled`、`disabled_mods`、`mod_settings` 到底属于 manifest、设置文件还是运行时数据结构。
- 上述名称是公开扩展点、内部实现细节，还是测试路径保留的符号。

## 备注

- `ModInitializerAttribute` 字符串存在，但它本身不足以证明 STS2 提供了自定义 mod 入口 API。
- 当前尚未使用反编译器枚举方法签名，因此本文件应被视为“字符串与头信息层”的证据笔记。
