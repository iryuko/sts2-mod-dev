# UnifiedSavePath macOS 检查

记录日期：2026-03-11

## 结论

- 最新结论档位：暂时不建议直接试当前发布版。
- 核心判断：
  - 从静态对照看，它依赖的关键 patch 点在本机版本中存在，命名与基础签名可对上。
  - 但 2026-03-11 20:11 这次 macOS 本机实机运行已经明确出现 Harmony patch 异常，`UnifiedSavePathMod.Initialize()` 失败，关键 patch 没有成功挂上。
  - Steam Cloud 记录也证明游戏仍然写入并上传了 `modded/profile1/...`，说明当前发布版没有完成“modded 与 vanilla 共用同一路径”的目标。

## 已确认

### 公开说明

来源：Nexus Mods `UnifiedSavePath`

- 作用：让 modded 和 vanilla 共用同一套存档目录。
- 说明明确写到：
  - 只改 save path routing，不改 gameplay content。
  - 核心做法是把 `UserDataPathProvider.IsRunningModded` 固定为 `false`。
  - 安装方式是原生 mod 安装，不需要外部 mod loader。
  - 文件产物是 `UnifiedSavePath.dll` 与 `UnifiedSavePath.pck`。
  - 作者备注：`Only tested in windows + single player.`

### 本地 UnifiedSavePath 产物

工作区已存在下载产物：

- `mods/UnifiedSavePaths-6-1-0-1-1773016995/UnifiedSavePath.dll`
- `mods/UnifiedSavePaths-6-1-0-1-1773016995/UnifiedSavePath.pck`

已确认：

- `UnifiedSavePath.pck` 内含 `res://mod_manifest.json`。
- manifest 关键信息可见：
  - `pck_name = "UnifiedSavePath"`
  - `name = "Unified Save Path"`
  - `author = "JiesiLuo"`
  - `version = "1.0.0"`
- `UnifiedSavePath.dll` 是标准 .NET/Mono 程序集，引用：
  - `sts2` `0.1.0.0`
  - `0Harmony` `2.4.2.0`
  - `System.Runtime` `9.0.0.0`
- DLL 中可见：
  - `ModInitializerAttribute`
  - `HarmonyPatch`
  - `PatchGetIsRunningModded`
  - `PatchSetIsRunningModded`
  - `PatchGetProfileDir`

### 本地 macOS STS2 对应证据

在 `local/tmp/sts2-arm64.il` 中已确认：

- 存在类型：
  - `MegaCrit.Sts2.Core.Saves.UserDataPathProvider`
- 存在字段：
  - `'<IsRunningModded>k__BackingField'`
- 存在属性：
  - `IsRunningModded`
- 存在方法：
  - `get_IsRunningModded()`
  - `set_IsRunningModded(bool)`
  - `GetProfileDir(int32)`
- `GetProfileDir(int32)` 的当前逻辑是：
  - 若 `IsRunningModded == false`，组合空前缀和 `profile{n}`
  - 若 `IsRunningModded == true`，组合 `modded/` 和 `profile{n}`
- `OneTimeInitialization::ExecuteEssential()` 中会在 `ModManager::get_LoadedMods()` 之后调用 `UserDataPathProvider::set_IsRunningModded(bool)`。

### 本机运行时条件

当前 macOS 游戏包中已存在：

- `SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll`
- `SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/0Harmony.dll`

因此，至少从程序集依赖看：

- `UnifiedSavePath.dll` 所需的 `sts2` 存在
- `UnifiedSavePath.dll` 所需的 `0Harmony` 存在
- 这类 Harmony patch mod 的基础运行条件成立

## 2026-03-11 实机日志结果

### 已确认

来自 macOS unified logging，进程 `Slay the Spire 2`：

- 2026-03-11 20:11:15，游戏记录到：
  - `Exception thrown when calling mod initializer of type UnifiedSavePath.UnifiedSavePathMod`
  - 内层异常是：
    - `HarmonyLib.HarmonyException: Patching exception in method static System.Boolean MegaCrit.Sts2.Core.Saves.UserDataPathProvider::get_IsRunningModded()`
    - `System.NotImplementedException: The method or operation is not implemented.`
- 这说明问题不是“mod 被忽略”，而是：
  - mod 已被发现并尝试初始化
  - 但在 patch `get_IsRunningModded()` 时失败
  - 因此关键逻辑没有成功生效

同一次运行中，2026-03-11 20:11:46 还记录到：

- `res://UnifiedSavePath/mod_image.png` 加载失败

这会在 modding UI 里显示该 mod 时触发资源错误，但它只是次要问题，不是存档未统一的根因。

### Steam Cloud / 存档侧证据

来自 `Steam/logs/cloud_log.txt` 与 `userdata/.../2868840/remotecache.vdf`：

- 2026-03-11 20:12:00 退出上传时，Steam 仍然上传了：
  - `modded/profile1/saves/progress.save`
  - `modded/profile1/saves/prefs.save`
- 本次运行后：
  - `remote/modded/profile1/saves/progress.save` 变成了一个新的小文件
  - `remote/profile1/saves/progress.save` 保持旧的 vanilla 进度

这说明当前发布版 `UnifiedSavePath` 在本机这次测试中没有把写盘路径统一回 vanilla profile。

## 高可能

- 该 mod 的核心不是 UI / 资源修改，而是运行逻辑层的 save path routing patch。
- `mod_image.png` 缺失只是 UI 资源问题，不是主故障。
- 当前主故障更像是 Harmony 在 macOS / 当前运行时上无法完成对 `get_IsRunningModded()` 的 patch。

## 风险点

### 1. 作者公开测试范围只到 Windows + 单人

- 这是最直接的外部风险提示。
- 当前没有公开证据证明它在 macOS 或多人模式下已被验证。

### 1.5. 当前发布版已经在本机 macOS 实机失败

- 失败点不是存档格式，而是 mod initializer 中的 Harmony patch。
- 因此这已经不再是“理论风险”，而是当前发布版的已发生问题。

### 2. 它只改到 `IsRunningModded` / `GetProfileDir` 这一层

- 本机 `UserDataPathProvider` 还存在：
  - `GetProfileScopedPath`
  - `GetProfileScopedBasePath`
  - `GetAccountScopedBasePath`
  - `GetPlatformDirectoryName`
- 其中 account / platform 作用域仍可能让路径落到不同的 `steam/<id>/...` 或 `default/<id>/...`。
- 因此即使这个 mod 成功取消了 `modded/` 前缀，也不自动等于“所有存档分流问题都被解决”。

### 3. 当前 macOS 已验证的原生安装模型与作者示例路径不完全同形

- 作者页面的示例是把两个文件放到 `<GameRoot>/mods/`。
- 我们当前本机已验证有效的安装根目录是：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 且已成功加载的 `SmokeMod` 安装形式是：
  - `.../Contents/MacOS/mods/<ModName>/<ModName>.dll`
  - `.../Contents/MacOS/mods/<ModName>/<ModName>.pck`
- 因此在 macOS 上直接照抄 Windows 示例路径，不是最稳妥方案。

### 4. 这是直接影响主存档路由的 mod

- 一旦它起效，modded 与 vanilla 进度会合并。
- 这正是它的目的，但也意味着测试前必须先备份原始存档。

## 安装层面建议

如果要在本机 macOS 上试：

- 最合理的目标目录：
  - `SlayTheSpire2.app/Contents/MacOS/mods/UnifiedSavePath/`
- 目录内放置：
  - `UnifiedSavePath.dll`
  - `UnifiedSavePath.pck`
- 当前没有证据显示它还需要额外的第三方外置依赖。
- 不建议在第一轮测试时直接照 Nexus 页面把文件裸放到 `mods/` 根目录；优先沿用我们已经验证过的“按 mod 名单独建目录”的本机模型。

## 最小试用方案

仅当后续拿到修正版构建或决定做二次验证时再执行。当前发布版不建议继续重复试装。

前提：先做只读备份，不覆盖原始存档。

1. 备份当前真实用户数据目录下的相关 `settings.save`、`profile.save`、`runs`、`history` 等文件。
2. 记录未安装 UnifiedSavePath 时：
   - 目标旧档是否能看到
   - 新开一局后数据写到了哪条路径
3. 将 `UnifiedSavePath.dll` 与 `UnifiedSavePath.pck` 安装到：
   - `.../SlayTheSpire2.app/Contents/MacOS/mods/UnifiedSavePath/`
4. 启动游戏，允许加载 mod。
5. 优先验证旧档是否恢复可见 / 可继续。
6. 再新开一个最小进度，退出后比对：
   - 是否落到与 vanilla 相同的 profile 路径
   - 是否仍然被写到另一路径
7. 如结果异常，先移除该 mod，再用备份回滚，不要在异常状态下继续推进主存档。
