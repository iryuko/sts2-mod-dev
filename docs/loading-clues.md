# Loading Clues

记录日期：2026-03-10

本文件只整理“与加载相关的线索”，并按证据等级分层。

## 已确认

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`TryLoadModFromPck`
  - 说明：只能确认这个名字存在。

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`OpenModdingScreen`
  - 说明：只能确认这个名字存在。

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`MegaCrit.Sts2.Core.Modding.ModManager`
  - 说明：只能确认这个类型名存在。

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`MegaCrit.Sts2.Core.Modding.ModManifest`
  - 说明：只能确认这个类型名存在。

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`res://src/Core/Nodes/Screens/ModdingScreen/NModdingScreen.cs`
  - 说明：程序集里保留了与 `ModdingScreen` 相关的 `res://` 路径字符串。

- 证据来源：`references/game-dlls/sts2/arm64/sts2.dll`
  - 可见字符串：`pck_name`、`mods_enabled`、`disabled_mods`、`mod_settings`、`schema_version`
  - 说明：只能确认这些字段样式的名字存在。

## 高可能

- 高可能：游戏内部存在某种 “modding 界面”。
  - 依据：`NModdingScreen`、`NConfirmModLoadingPopup`、`NModInfoContainer`、`NModMenuButton`、`NModMenuRow`、`NOpenModdingScreenButton` 同时出现。

- 高可能：游戏内部存在针对 `.pck` 的某种加载或导入路径。
  - 依据：`TryLoadModFromPck`、`pck_name`、`pckFilename` 同时出现。

- 高可能：游戏内部考虑过 mod 对模型或本地化的扩展。
  - 依据：`ConcatModelsFromMods`、`GetSubtypesInMods`、`GetModdedLocTables` 同时出现。

## 待验证

- `.pck` 是否就是最终 mod 成品格式，还是仅用于某类内部资源包。
- `manifest` 相关字段的真实落盘格式、文件名和目录位置。
- `mods_enabled`、`disabled_mods`、`mod_settings` 是否对应用户设置文件。
- 内部 `ModdingScreen` 是否面向正式用户、开发者，或仅测试构建使用。

## 当前不应下的结论

- 不能因为出现 `TryLoadModFromPck` 就断定 “把任意 `.pck` 扔进某目录即可加载”。
- 不能因为出现 `ModManifest` 就断定 manifest 已经验证了字段结构。
- 不能因为出现 `OpenModdingScreen` 就断定已经找到了真实入口或菜单路径。
