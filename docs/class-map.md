# 类映射记录

当前已经有可分析程序集，但本文件仍不填写任何未经验证的具体类名、接口或命名空间。

## 当前可分析来源

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 原始路径：`.../SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll`
  - 当前状态：已复制，待反编译或提取元数据
  - 是否已验证：仅验证文件存在与可复制，未验证内部类型

- 来源文件：`references/game-dlls/sts2/x86_64/sts2.dll`
  - 原始路径：`.../SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll`
  - 当前状态：已复制，待与 arm64 版本比对
  - 是否已验证：仅验证文件存在与可复制，未验证内部类型

- 来源文件：`references/game-dlls/shared/GodotSharp.dll`
  - 原始路径：`.../SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/GodotSharp.dll`
  - 当前状态：已复制，作为运行环境参考
  - 是否已验证：仅验证文件存在与哈希

- 来源文件：`references/game-dlls/shared/0Harmony.dll`
  - 原始路径：`.../SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/0Harmony.dll`
  - 当前状态：已复制，作为运行环境参考
  - 是否已验证：仅验证文件存在与哈希

## 已确认的命名空间字符串

以下内容仅表示在字符串层面看到了命名空间名字，不表示已经确认公开 API 或用途：

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 命名空间：`MegaCrit.Sts2.Core.Modding`
  - 是否已验证：已确认名称存在
  - 备注：与 modding 相关，具体职责待验证

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 命名空间：`MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen`
  - 是否已验证：已确认名称存在
  - 备注：看起来是 UI 层命名空间，具体行为待验证

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 命名空间：`MegaCrit.Sts2.Core.Nodes.Screens.Settings`
  - 是否已验证：已确认名称存在
  - 备注：其中命中了 `NOpenModdingScreenButton` 相关字符串

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 命名空间：`MegaCrit.Sts2.Core.Nodes.GodotExtensions`
  - 是否已验证：已确认名称存在
  - 备注：说明程序集内部存在与 Godot 相关的扩展层

## 已确认的类型名或成员名线索

以下内容仍然只是名称层证据，不代表功能已经确认：

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Modding.ModManager`
  - 可能用途：名称暗示为 mod 管理相关类型
  - 是否已验证：仅确认名称
  - 备注：未确认公开性与调用方式

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Modding.ModManifest`
  - 可能用途：名称暗示为 mod 清单相关类型
  - 是否已验证：仅确认名称
  - 备注：未确认字段结构

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Modding.ModSource`
  - 可能用途：名称暗示为 mod 来源枚举或数据类型
  - 是否已验证：仅确认名称
  - 备注：未确认取值范围

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModdingScreen`
  - 可能用途：名称暗示为 modding UI 屏幕
  - 是否已验证：仅确认名称
  - 备注：未确认在游戏内如何进入

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NConfirmModLoadingPopup`
  - 可能用途：名称暗示为确认加载弹窗
  - 是否已验证：仅确认名称
  - 备注：不能据此确认真实加载规则

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer`
  - 可能用途：名称暗示为 mod 信息容器
  - 是否已验证：仅确认名称
  - 备注：未确认显示字段

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModMenuButton`
  - 可能用途：名称暗示为 mod 菜单按钮
  - 是否已验证：仅确认名称
  - 备注：未确认入口位置

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`NOpenModdingScreenButton`
  - 可能用途：名称暗示为打开 modding 界面的按钮
  - 是否已验证：仅确认名称
  - 备注：与 `res://src/Core/Nodes/Screens/Settings/NOpenModdingScreenButton.cs` 字符串同时出现

## 已确认的方法名或字段名线索

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`TryLoadModFromPck`
  - 可能用途：名称暗示尝试从 `.pck` 加载 mod
  - 是否已验证：仅确认名称
  - 备注：尚未确认其是否为对外入口、内部工具方法或测试代码路径

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`OpenModdingScreen`
  - 可能用途：名称暗示打开 modding UI
  - 是否已验证：仅确认名称
  - 备注：未确认调用位置

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`GetModdedLocTables`
  - 可能用途：名称暗示处理被 mod 修改的本地化表
  - 是否已验证：仅确认名称
  - 备注：不能据此确认实际本地化覆盖机制

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`ConcatModelsFromMods`
  - 可能用途：名称暗示拼接来自 mods 的模型
  - 是否已验证：仅确认名称
  - 备注：未确认模型范围

- 来源文件：`references/game-dlls/sts2/arm64/sts2.dll`
  - 类型名：`pck_name` / `mods_enabled` / `disabled_mods` / `mod_settings` / `schema_version`
  - 可能用途：名称暗示某类 manifest、设置或序列化字段
  - 是否已验证：仅确认名称
  - 备注：这些字段属于哪种文件格式，当前仍待验证

## 当前限制

- 当前主要依据 `strings`、`file`、`objdump -x` 与 json 元数据，仍未获得完整类型签名。
- 还不能确认哪些类型属于公开扩展点，哪些只是游戏内部实现。
- 还不能确认 manifest、loader 或入口接口的真实命名。
- 还不能确认 `ModInitializerAttribute` 是否与 STS2 mod 加载流程有关。

## 建议后续记录格式

- 来源文件：
- 命名空间：
- 类型名：
- 可能用途：
- 是否已验证：
- 备注：
