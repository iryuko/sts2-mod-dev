# 实验记录

---

## 日期

2026-03-10

## 实验目标

巡检 macOS 版游戏目录结构，找出适合复制到工作区 `references/` 的关键参考文件。

## 操作

列出执行的脚本、复制的文件、查看的路径。

1. 只读检查游戏根目录 `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2`。
2. 进入 `SlayTheSpire2.app/Contents/`，检查 `MacOS/`、`Resources/`、`Frameworks/`。
3. 检索 `.dll`、`.json`、`.pck`、可执行文件，以及命名接近 `mod`、`manifest`、`plugin`、`loader` 的文件。
4. 更新 `shared/scripts/sync-game-files.sh` 为白名单复制模式，并执行该脚本。
5. 将少量关键程序集和元数据复制到 `references/`。

## 观察结果

只写实际看到的结果。

- 游戏根目录下存在 `SlayTheSpire2.app` 与 `controller_config/`。
- `.app` 内存在 `Contents/MacOS/Slay the Spire 2`，经 `file` 检查为 `arm64` 与 `x86_64` 通用二进制。
- `Contents/Resources/Slay the Spire 2.pck` 存在，体积约 `1.5G`。
- `Contents/Resources/release_info.json` 内容显示：
  - `version`: `v0.98.2`
  - `branch`: `v0.98.2`
  - `commit`: `f4eeecc6`
  - `date`: `2026-03-06T15:52:37-08:00`
- `Contents/Info.plist` 显示：
  - `CFBundleIdentifier`: `com.megacrit.SlayTheSpire2`
  - `LSArchitecturePriority`: `arm64`, `x86_64`
- `Resources/data_sts2_macos_arm64/` 与 `Resources/data_sts2_macos_x86_64/` 中都存在：
  - `sts2.dll`
  - `sts2.deps.json`
  - `sts2.runtimeconfig.json`
  - `0Harmony.dll`
  - `GodotSharp.dll`
- `sts2.runtimeconfig.json` 显示目标框架为 `.NET 9.0`，包含 `Microsoft.NETCore.App 9.0.7`。
- `sts2.deps.json` 显示 `sts2/0.1.0` 依赖至少包括：
  - `GodotSharp 4.5.1`
  - `0Harmony 2.4.2.0`
  - `Steamworks.NET 1.0.0.0`
  - `Sentry 5.0.0`
  - `SmartFormat 3.3.0`
- `GodotSharp.dll` 与 `0Harmony.dll` 在 `arm64` 和 `x86_64` 目录中的 SHA-256 相同。
- `sts2.dll` 在 `arm64` 和 `x86_64` 目录中的 SHA-256 不同，说明两套程序集不是完全相同的二进制副本。
- 在本次巡检范围内，未看到独立命名为 `manifest`、`plugin`、`mod loader` 的明确文件；看到的 `MonoMod.*` 与 `System.Runtime.Loader.dll` 只能视为运行时依赖，不能据此断定为 STS2 的 mod 加载机制。

## 暂时结论

允许写假设，但要明确标记为“待验证”。

- 目前可以确认：macOS 版 STS2 使用 `.app` bundle，且游戏逻辑程序集位于 `Contents/Resources/data_sts2_macos_*`。
- 目前可以确认：`sts2.dll` 是值得优先分析的主程序集候选，`GodotSharp.dll` 与 `0Harmony.dll` 是后续理解运行环境时的重要依赖。
- 待验证：游戏是否存在官方或事实上的 mod 安装目录。
- 待验证：真实 mod 加载入口、manifest 结构、以及应优先跟踪 `arm64` 还是同时跟踪两套架构程序集。

## 后续动作

下一步要补充什么验证。

1. 对 `references/game-dlls/sts2/arm64/sts2.dll` 进行只读反编译或元数据提取，确认命名空间与关键类型。
2. 比较 `arm64` 与 `x86_64` 两份 `sts2.dll` 的公开类型差异，确认是否只存在架构差异还是还包含行为差异。
3. 继续只读检查游戏外部的日志、配置、用户数据位置，确认 mod 成品最终应安装到哪里。

---

## 日期

2026-03-10

## 实验目标

仅基于已复制到 `references/` 的真实文件，继续提取 `sts2.dll` 与相关程序集的元数据线索，缩小 mod 加载相关未知范围。

## 操作

列出执行的脚本、复制的文件、查看的路径。

1. 使用 `file` 与 `objdump -x` 对 `references/game-dlls/sts2/arm64/sts2.dll` 做头信息检查。
2. 使用 `strings` 检索 `sts2.dll`、`0Harmony.dll`、`GodotSharp.dll` 中与 `mod`、`manifest`、`loader`、`pck`、`godot` 等关键词相关的字符串。
3. 对 `references/api-notes/sts2/sts2.deps.arm64.json`、`references/api-notes/sts2/sts2.deps.x86_64.json`、`references/api-notes/sts2/sts2.runtimeconfig.arm64.json` 做关键词检查。
4. 对 `arm64` 与 `x86_64` 两份 `sts2.dll` 的关键词命中做抽样比对。

## 观察结果

只写实际看到的结果。

- `references/game-dlls/sts2/arm64/sts2.dll` 经 `file` 识别为 `PE32+ executable (DLL) ... Aarch64 Mono/.Net assembly`。
- `objdump -x` 显示该文件存在 `CLR Runtime Header`，且 `AddressOfEntryPoint` 为 `0x0`。
- `sts2.dll` 字符串中能看到：
  - `TargetFrameworkAttribute`
  - `AssemblyFileVersionAttribute`
  - `AssemblyInformationalVersionAttribute`
  - `.NETCoreApp,Version=v9.0`
- `sts2.deps.json` 与 `sts2.runtimeconfig.json` 共同支持以下已确认信息：
  - 目标框架为 `.NET 9.0`
  - 运行时包含 `Microsoft.NETCore.App 9.0.7`
  - `sts2/0.1.0` 依赖 `GodotSharp 4.5.1`、`0Harmony 2.4.2.0`、`Steamworks.NET 1.0.0.0`
- `0Harmony.dll` 字符串中可见：
  - `0Harmony, Version=2.4.2.0`
  - `.NETCoreApp,Version=v9.0`
  - `HarmonyLib`
- `GodotSharp.dll` 字符串中可见：
  - `GodotSharp`
  - `.NETCoreApp,Version=v8.0`
  - 指向 Godot 仓库的 `RepositoryUrl`
- `sts2.dll` 中可见一批完整命名空间字符串，其中包含：
  - `MegaCrit.Sts2.Core.Modding`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen`
  - `MegaCrit.Sts2.Core.Nodes.Screens.Settings`
  - `MegaCrit.Sts2.Core.Nodes.GodotExtensions`
  - `MegaCrit.Sts2.Core.Assets`
- `sts2.dll` 中可见的 modding 相关类型或名称包括：
  - `MegaCrit.Sts2.Core.Modding.ModManager`
  - `MegaCrit.Sts2.Core.Modding.ModManifest`
  - `MegaCrit.Sts2.Core.Modding.ModSource`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NConfirmModLoadingPopup`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModMenuButton`
  - `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModdingScreen`
  - `NOpenModdingScreenButton`
- `sts2.dll` 中可见的相关方法或字段名包括：
  - `TryLoadModFromPck`
  - `OpenModdingScreen`
  - `GetModdedLocTables`
  - `GetSubtypesInMods`
  - `ConcatModelsFromMods`
  - `IsModDisabled`
  - `get_ModTypes`
  - `pck_name`
  - `pckFilename`
  - `mods_enabled`
  - `disabled_mods`
  - `mod_settings`
  - `schema_version`
  - `author`
  - `version`
- `sts2.dll` 中还可见一组 `res://` 风格路径字符串：
  - `res://src/Core/Nodes/Screens/ModdingScreen/NConfirmModLoadingPopup.cs`
  - `res://src/Core/Nodes/Screens/ModdingScreen/NModInfoContainer.cs`
  - `res://src/Core/Nodes/Screens/ModdingScreen/NModMenuButton.cs`
  - `res://src/Core/Nodes/Screens/ModdingScreen/NModMenuRow.cs`
  - `res://src/Core/Nodes/Screens/ModdingScreen/NModdingScreen.cs`
  - `res://src/Core/Nodes/Screens/Settings/NOpenModdingScreenButton.cs`
- 对 `x86_64` 版本 `sts2.dll` 的同类关键词抽样，已看到与 `arm64` 版本相同的一批核心命名命中，例如 `TryLoadModFromPck`、`ModManifest`、`ModSettings`、`DisabledMod`。
- `ModInitializerAttribute` 字符串也存在，但当前只能确认名称存在，不能将其直接当作 STS2 自定义 mod 入口证据。

## 暂时结论

允许写假设，但要明确标记为“待验证”。

- 已确认：游戏程序集里确实存在 `MegaCrit.Sts2.Core.Modding` 相关命名空间和多组带 `Mod*` 命名的类型。
- 已确认：游戏程序集里确实存在与 `Pck`、`ModdingScreen`、`OpenModdingScreen` 相关的名称线索。
- 高可能：游戏内部至少包含“读取某种 mod 包/manifest 并在界面中展示”的实现痕迹。
- 待验证：`TryLoadModFromPck` 的真实参数、调用时机、扫描目录、文件格式要求。
- 待验证：`pck_name`、`mods_enabled`、`disabled_mods`、`mod_settings` 等字段究竟属于 manifest、设置文件、存档还是运行时数据模型。
- 待验证：`ModInitializerAttribute` 是否只是标准 .NET 名称，还是被 STS2 某处用于加载逻辑。

## 后续动作

下一步要补充什么验证。

1. 如果本机可补充只读 .NET 元数据工具，优先枚举 `sts2.dll` 的公开类型、方法签名与自定义特性。
2. 对 `sts2.dll` 中的 `MegaCrit.Sts2.Core.Modding.*` 类型做更窄范围的元数据提取，只记录签名，不推断业务逻辑。
3. 在游戏目录之外继续只读寻找日志、配置或用户数据目录，验证 `mods_enabled`、`disabled_mods`、`mod_settings` 是否对应实际落盘文件。

---

## 日期

2026-03-10

## 实验目标

把当前安装路径判断从“完全未知”收敛到“可做首轮闭环验证的首要候选”，并升级安装脚本到最小可验证状态。

## 操作

列出执行的脚本、复制的文件、查看的路径。

1. 接受新的外部前提：当前抢先体验期的首要外部安装候选路径按 `<GameRoot>/mods/` 处理。
2. 只读检查本机游戏目录下是否已存在 `<GameRoot>/mods/`。
3. 更新 `local/install-notes.md`、`shared/scripts/install-mod.sh`、最小成品规范与首轮验证 checklist。

## 观察结果

只写实际看到的结果。

- 本机游戏目录当前不存在：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods`
- 因此当前状态不是“本机已证实 `<GameRoot>/mods/` 有效”，而是：
  - 首要候选：`<GameRoot>/mods/`
  - 证据等级：高可能
  - 状态：待本机验证
- 安装脚本已升级为：
  - 默认目标：`<GameRoot>/mods/<ModName>/`
  - 默认模式：dry-run
  - 支持 `--apply`
  - 支持 `--create-mods-dir`
  - 支持 `--allow-overwrite`
  - 默认不覆盖已有同名 mod 目录
- 这套行为用于建立“最小可验证闭环”，不是宣称最终规则已经完全确认。

## 暂时结论

允许写假设，但要明确标记为“待验证”。

- 已确认：`<GameRoot>/mods/` 已被提升为首要外部候选安装路径。
- 已确认：本机当前尚无该目录，因此首轮验证需要显式创建或先观察游戏是否自行生成。
- 高可能：如果社区实测与当前 DLL 线索指向同一路径，则 `<GameRoot>/mods/<ModName>/` 是最值得优先验证的安装目标。
- 待验证：游戏是否真正扫描该目录、目录内是否要求特定文件命名、以及 manifest 工作模板是否足以触发识别。

## 后续动作

下一步要补充什么验证。

1. 准备一个最小 mod 成品目录。
2. 用 dry-run 检查安装路径和目录层级。
3. 在明确成品最小集合后，执行第一次真实复制验证。

---

## 日期

2026-03-10

## 实验目标

记录第一次 SmokeMod 本机复测结果，确认“没有任何反馈”究竟更接近“静默跳过”还是“尚未实际安装”。

## 操作

1. 读取本机日志：
   - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log`
   - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T00.48.43.log`
2. 用关键词检索 `SmokeMod`、`mod`、`manifest`、`pck`、`mods/`、`error`、`warn`。
3. 检查游戏根目录下是否实际存在：
   - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/`
4. 只读查看本机 `settings.save` 中与 mod 相关的字段。

## 观察结果

- 两份日志中都未发现 `SmokeMod`、`manifest`、`pck`、`mods/` 相关命中。
- 当前日志内容主要是正常启动、资源预载、存档同步和退出信息，没有看到“扫描 mod 目录”或“解析 manifest”的直接记录。
- 当前游戏目录中未发现：
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/`
- 这说明截至本次检查时，游戏目标路径里没有实际安装好的 SmokeMod 包。
- 本机设置文件：
  - `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save`
  中可见字段：
  - `"mod_settings": null`
- 该字段只能作为“设置结构中存在 mod 相关槽位”的证据，不能据此确认已扫描外部 mod。

## 暂时结论

- 已确认：本次“游戏似乎什么都没有报告”并没有在日志里留下与 SmokeMod 相关的痕迹。
- 已确认：当前 `<GameRoot>/mods/SmokeMod/` 不存在，因此这次结果不能直接拿来判断扫描规则是否正确。
- 高可能：当前问题更接近“尚未完成真实安装”或“安装目标路径未实际落地”，而不是“游戏已扫描到包但完全静默”。
- 待验证：在真实安装落地后，游戏是否会在日志或界面中留下最小识别痕迹。

## 后续动作

1. 执行一次真实安装：
   - `./shared/scripts/install-mod.sh SmokeMod --apply --create-mods-dir`
2. 安装后立刻复核 `<GameRoot>/mods/SmokeMod/` 是否存在且内容完整。
3. 再次启动游戏，并重新对 `godot.log` 做关键词检索。
4. 如果仍然没有痕迹，再把重心切到“manifest 字段名”和“真实可运行最小 dll/pck”。

---

## 日期

2026-03-10

## 实验目标

不再优先猜测安装目录，而是只读查找游戏是否在日志、用户数据、配置或缓存中留下任何 mod 加载、忽略或配置落盘痕迹。

## 操作

1. 巡检 macOS 常见用户数据与缓存位置：
   - `/Users/user/Library/Application Support/SlayTheSpire2`
   - `/Users/user/Library/Application Support/Steam/userdata/355081496/2868840`
   - `/Users/user/Library/Caches/com.megacrit.SlayTheSpire2`
   - `/Users/user/Library/Caches/SentryCrash/Slay the Spire 2`
2. 检索关键词：
   - `SmokeMod`
   - `mods_enabled`
   - `disabled_mods`
   - `mod_settings`
   - `schema_version`
   - `pck_name`
   - `manifest`
3. 检查当前 `SmokeMod` 包内部命名一致性。

## 观察结果

- 已确认的高优先级路径包括：
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T02.21.00.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T02.27.25.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save`
  - `/Users/user/Library/Application Support/Steam/userdata/355081496/2868840/remote/settings.save`
  - `/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log`
  - `/Users/user/Library/Caches/SentryCrash/Slay the Spire 2/Data/CrashState.json`
- 未发现 `~/Library/Preferences` 或 `~/Library/Logs` 下的 STS2 专用候选路径。
- 本机 `settings.save` 与 Steam remote `settings.save` 中都可见：
  - `"mod_settings": null`
  - `"schema_version": 4`
- 本机 `profile.save` 与 `prefs.save` 中能看到 `schema_version`，但未见 mod 相关字段。
- `async.log` 当前为空。
- `CrashState.json` 只记录崩溃统计，没有 mod 相关字段。
- 未在已检查日志和配置中发现以下关键词：
  - `SmokeMod`
  - `mods_enabled`
  - `disabled_mods`
  - `pck_name`
  - `manifest`
- 当前 `SmokeMod` 包内部命名关系一致：
  - 目录名：`SmokeMod`
  - manifest 的 `name`：`SmokeMod`
  - manifest 的 `dll`：`SmokeMod.dll`
  - manifest 的 `pck`：`SmokeMod.pck`
- 当前 `SmokeMod.dll` 已升级为真实 CLR 程序集。
- 当前该 DLL 由 `shared/scripts/build-mod.sh` 通过 Mono 编译器生成，并已成功安装到游戏目录中。
- 当前 `SmokeMod.pck` 已升级为真实 Godot PCK，文件头可见 `GDPC`。
- manifest 的 `id` 为 `smoke-mod`，与目录名不完全同形；目前只能记为“待验证是否敏感”。

## 暂时结论

- 已确认：游戏会把 `mod_settings` 这个字段落盘到 `settings.save`，但当前值仍为 `null`。
- 已确认：截至本轮检查，没有发现任何与 `SmokeMod` 直接相关的日志、配置或忽略记录。
- 高可能：当前更值得优先怀疑“包无效或被静默忽略”，而不是继续单独怀疑路径。
- 这个判断的依据是：
  - 用户已手动测试过两个候选安装位置但都没有明显反馈
  - 虽然 `dll` 与 `pck` 都已经是真实产物，但它们仍是“不绑定已确认 STS2 API / 内容结构的最小验证产物”
- 待验证：当 `dll` 与 `pck` 都是真实文件后，游戏是否会开始在日志或配置中留下痕迹。

## 后续动作

1. 采用“启动前 / 启动后”最小比对方案，优先盯：
   - `godot.log`
   - 新生成的 `godot*.log`
   - 本机 `settings.save`
   - Steam remote `settings.save`
   - `async.log`
2. 如果仍无任何新痕迹，优先把 `SmokeMod.dll` 或 `SmokeMod.pck` 中至少一项替换为真实最小产物。
3. 在真实产物出现前，不再把“无反馈”直接解释为路径错误。

---

## 日期

2026-03-10

## 实验目标

在 `SmokeMod.dll` 已升级为真实程序集之后，复查游戏是否终于留下任何 mod 相关日志、配置或缓存痕迹。

## 操作

1. 重新检查 `godot.log` 与新增的 `godot*.log`。
2. 重新检查本地 `settings.save` 和 Steam remote `settings.save`。
3. 重新检查 `async.log`。
4. 检索关键词：
   - `SmokeMod`
   - `mods_enabled`
   - `disabled_mods`
   - `manifest`
   - `pck`
   - `mods/`

## 观察结果

- 新增日志：
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T04.04.27.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T05.46.18.log`
- `godot.log`、`settings.save`、`remote/settings.save` 都发生了正常启动后的时间戳更新。
- 但所有已检查日志中仍未发现：
  - `SmokeMod`
  - `mods_enabled`
  - `disabled_mods`
  - `manifest`
  - `pck`
  - `mods/`
- `settings.save` 与 `remote/settings.save` 中仍然只有：
  - `"mod_settings": null`
  - `"schema_version": 4`
- `async.log` 仍为空。
- 最新两份启动日志内容仍然只是正常启动、资源加载、主菜单进入和退出，没有新增 mod 相关记录。

## 暂时结论

- 已确认：把 `SmokeMod.dll` 从占位文本换成真实 CLR 程序集，仍然没有让游戏留下任何新的 mod 痕迹。
- 高可能：当前瓶颈已经不再是“DLL 是否为真实文件”，而更可能是：
  - `SmokeMod.pck` 仍是占位文本
  - 或 manifest / DLL / PCK 的组合没有满足游戏进入加载路径的最低条件
- 待验证：如果再把 `SmokeMod.pck` 换成真实资源包，或者进一步收紧 manifest 字段，是否会开始出现痕迹。

## 后续动作

1. 现在可以基于“真实 DLL + 真实 PCK + 工作模板 manifest”的组合再启动一轮游戏。
2. 如果这轮仍无痕迹，再收紧到 manifest 字段、包内内容布局和目标目录污染这三个方向。

---

## 日期

2026-03-10

## 实验目标

把开源示例项目 `lamali292/sts2_example_mod` 作为“真实可安装 STS2 mod”的首要参考对象，只学习其项目组织、构建产物和安装链路，再反推当前 `SmokeMod` 的结构缺口。

## 操作

1. 检查示例项目的：
   - `README.md`
   - `ExampleMod.csproj`
   - `local.props.example`
   - `pack/mod_manifest.json`
   - `pack/project.godot`
2. 记录该项目如何：
   - 指定游戏路径
   - 引用游戏 DLL
   - 生成 PCK
   - 把构建产物复制到游戏目录
3. 对照当前工作区中的：
   - `mods/SmokeMod/`
   - `shared/scripts/build-mod.sh`
   - `shared/scripts/install-mod.sh`
   - `local/tmp/sts2-arm64.il`

## 观察结果

- 示例项目源码根目录直接包含：
  - `ExampleMod.csproj`
  - `ModEntry.cs`
  - `Patches/`
  - `Relics/`
  - `pack/`
- 示例项目使用 `local.props` 注入本机路径：
  - `STS2GamePath`
  - `GodotExePath`
- 示例项目在 `ExampleMod.csproj` 中直接派生：
  - `ModsOutputDir = $(STS2GamePath)\\mods\\$(ModName)`
  - `GameDataDir = $(STS2GamePath)\\data_sts2_windows_x86_64`
- 示例项目构建前会自动生成：
  - `pack/project.godot`
  - `pack/mod_manifest.json`
- 示例项目构建后会自动执行：
  - 复制 `ExampleMod.dll` 到 `$(STS2GamePath)\\mods\\ExampleMod\\`
  - 调用 Godot `--export-pack` 生成 `ExampleMod.pck` 到同一目录
- 示例项目安装后的目标目录结构等价于：
  - `<GameRoot>/mods/ExampleMod/ExampleMod.dll`
  - `<GameRoot>/mods/ExampleMod/ExampleMod.pck`
- 示例项目的安装链路里，没有把外部 `mod_manifest.json` 作为最终安装文件单独复制。
- 示例项目当前 `pack/mod_manifest.json` 字段只有：
  - `pck_name`
  - `name`
  - `author`
  - `version`
- 这与本机 `sts2.dll` IL 中已看到的 `ModManifest` 结构高度一致；当前 IL 中可见字段为：
  - `pck_name`
  - `name`
  - `author`
  - `description`
  - `version`
- 本机 IL 同时确认：游戏读取的是 `res://mod_manifest.json`，也就是 PCK 内的 manifest，不是外部同目录 JSON。
- 当前 `SmokeMod` 的 PCK 打包源目录只有：
  - `project.godot`
  - `pack_pck.gd`
  - `smoke_marker.txt`
- 当前 `SmokeMod.pck` 没有把 `mod_manifest.json` 打进包内。
- 当前 `SmokeMod` 仍在额外导出并安装外部 `mod_manifest.json`，这与示例项目的实际安装产物结构不一致。

## 暂时结论

- 已确认：示例项目证明了一个“真实可安装 mod”的构建链路可以是：
  - 生成 manifest
  - 把 manifest 放入 Godot 打包源目录
  - 输出外部安装产物只有 DLL 和 PCK
- 已确认：当前更值得吸收的是示例项目的“安装链路与最小产物结构”，不是其复杂功能代码。
- 已确认：当前 `SmokeMod` 最主要的结构缺口不是“还没再猜到更多 manifest 字段”，而是：
  - manifest 没有进入 PCK
  - 安装链路仍把外部 `mod_manifest.json` 当成核心产物
- 高可能：对当前本机版本来说，最小可识别 mod 包更接近：
  - 一个包含 `res://mod_manifest.json` 的 `.pck`
  - 一个同名 `.dll`（如需程序集逻辑）
  - 两者放在被游戏扫描的 `mods` 目录中
- 待验证：示例项目按 Windows 假设的 `<GameRoot>/mods/`，在当前 macOS 版本机上是否应等价替换为“可执行文件目录下的 `mods/`”。

## 后续动作

1. 先把 `SmokeMod` 的 PCK 构建链路改成包含 `mod_manifest.json`。
2. 把首轮安装候选收紧为：
   - `SmokeMod.dll`
   - `SmokeMod.pck`
3. 再按本机 macOS 的真实扫描路径做一次最小复测。

---

## 日期

2026-03-10

## 实验目标

把已经调整过的 `SmokeMod` 重新构建并安装到本机 macOS 更强证据指向的扫描目录，再做一次真正基于游戏日志的闭环复测。

## 操作

1. 调整 `SmokeMod` 的构建链路：
   - 构建时把 `manifest/mod_manifest.json` 复制到 `pack/mod_manifest.json`
   - 重打 `SmokeMod.pck`
   - release 目录只保留 `SmokeMod.dll` 与 `SmokeMod.pck`
2. 调整安装脚本默认目标：
   - 若检测到 macOS `.app` 可执行文件，则默认安装到 `SlayTheSpire2.app/Contents/MacOS/mods/<ModName>/`
3. 执行：
   - `./shared/scripts/build-mod.sh SmokeMod`
   - `./shared/scripts/install-mod.sh SmokeMod --apply --create-mods-dir --replace-target`
4. 启动游戏一次并退出。
5. 检查：
   - `godot.log`
   - 新生成的 `godot*.log`
   - `settings.save`
   - `async.log`

## 观察结果

- 当前 `SmokeMod` release 目录中只有：
  - `SmokeMod.dll`
  - `SmokeMod.pck`
- 当前 `SmokeMod/pack/` 中已存在：
  - `mod_manifest.json`
  - `project.godot`
  - `pack_pck.gd`
  - `smoke_marker.txt`
- 当前安装目标目录为：
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/SmokeMod/`
- 当前游戏目录中的已安装产物只有：
  - `SmokeMod.dll`
  - `SmokeMod.pck`
- 新增日志：
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T08.52.13.log`
- 最新 `godot.log` 中首次出现明确的 mod 扫描痕迹：
  - `Found mod pck file /Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/SmokeMod/SmokeMod.pck`
  - `Skipping loading mod SmokeMod.pck, user has not yet seen the mods warning`
- 本轮启动后：
  - `godot.log` 时间戳已更新
  - `async.log` 时间戳已更新
  - `settings.save` 与 Steam remote `settings.save` 未更新
- 本机 `settings.save` 中仍只看到：
  - `"mod_settings": null`
  - `"schema_version": 4`

## 暂时结论

- 已确认：当前本机 macOS 版本确实会扫描：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 已确认：当前 `SmokeMod.pck` 已经被游戏识别到。
- 已确认：这次阻止 mod 真正进入加载流程的直接原因，不再是路径错误，而是：
  - `user has not yet seen the mods warning`
- 已确认：把 manifest 放入 PCK 内部后，游戏终于留下了可解释的、与 mod 直接相关的日志。
- 高可能：当前下一步需要优先解决的是“玩家已同意/已看过 mod warning”的状态，而不是继续修改 PCK 最小结构。

## 后续动作

1. 继续只读分析：
   - `mods_enabled`
   - `disabled_mods`
   - `PlayerAgreedToModLoading`
   - modding UI 入口
2. 目标是确认：
   - 如何触发游戏内的 mod warning
   - 或该状态是否会落盘到 `settings.save`
