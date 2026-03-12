# 当前状态

记录日期：2026-03-10

## 当前阶段

已从“猜测安装目录和 manifest 位置”进入“已成功触发 SmokeMod 加载提示，转向确认内置 mod 界面与存档影响”阶段。

## 当前目标

确认游戏内是否存在内置 mod 列表 / 管理界面入口，继续定位当前存档问题，并评估 `UnifiedSavePath` 在 macOS 上是否具备低风险试用条件。

## 已确认

- 本仓库是工作区，不是游戏安装目录；真实游戏路径以 `local/game-path.txt` 为准。
- macOS 本机实际扫描到的 mod 目录是：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 当前游戏目录中已存在：
  - `.../Contents/MacOS/mods/SmokeMod/SmokeMod.dll`
  - `.../Contents/MacOS/mods/SmokeMod/SmokeMod.pck`
- 本机最近一次启动中，游戏已经明确提示加载了我们的 `SmokeMod`。
- 已安装版与工作区发布版的核心产物一致：
  - `SmokeMod.dll` 文件名、大小、SHA-256 一致
  - `SmokeMod.pck` 文件名、大小、SHA-256 一致
- 工作区内两个 `mod_manifest.json` 一致：
  - `mods/SmokeMod/manifest/mod_manifest.json`
  - `mods/SmokeMod/pack/mod_manifest.json`
- `local/tmp/sts2-arm64.il` 显示游戏会读取 `res://mod_manifest.json`，说明 manifest 关键位置在 PCK 内。
- `mods/SmokeMod/exports/release/SmokeMod/` 当前只保留 `SmokeMod.dll` 与 `SmokeMod.pck`，已符合当前最小安装产物模型。
- `local/tmp/sts2-arm64.il` 与现有结论文件中已能确认一组内置 modding UI 线索：
  - `NOpenModdingScreenButton`
  - `OpenModdingScreen`
  - `NModdingScreen`
  - `NModMenuRow`
  - `NModInfoContainer`
  - `NConfirmModLoadingPopup`
- `NSettingsScreen::_Ready()` 中存在 `%ModdingButton`，并把该按钮绑定到 `OpenModdingScreen(...)`。
- `NMainMenuSubmenuStack` 中可实例化 `NModdingScreen`，说明该屏幕不是孤立的命名残留。
- `NModdingScreen` 中存在：
  - `OnGetModsPressed`，会打开 Steam Workshop 页面
  - `OnMakeModsPressed`，会打开官方 example-mod wiki
  - `OnRowSelected` / `OnModEnabledOrDisabled`
  - `NModInfoContainer::Fill(...)`，会展示 mod 名称、作者、版本、描述与 `mod_image.png`
- 工作区已存在 `UnifiedSavePath` 下载产物：
  - `mods/UnifiedSavePaths-6-1-0-1-1773016995/UnifiedSavePath.dll`
  - `mods/UnifiedSavePaths-6-1-0-1-1773016995/UnifiedSavePath.pck`
- `UnifiedSavePath.dll` 已确认引用：
  - `sts2` `0.1.0.0`
  - `0Harmony` `2.4.2.0`
- 本机 macOS `sts2.dll` 中已确认存在：
  - `MegaCrit.Sts2.Core.Saves.UserDataPathProvider`
  - `get_IsRunningModded()`
  - `set_IsRunningModded(bool)`
  - `GetProfileDir(int32)`
- 本机 macOS 游戏运行时已确认自带：
  - `.../data_sts2_macos_arm64/0Harmony.dll`
- 已新增专题结论：
  - `docs/unified-save-path-macos-check.md`
- 2026-03-11 实机日志已确认：
  - `UnifiedSavePath.UnifiedSavePathMod` 初始化时抛出 Harmony patch 异常
  - 失败点是 `UserDataPathProvider::get_IsRunningModded()`
  - 内层异常为 `System.NotImplementedException`
- 同一次运行中还出现：
  - `res://UnifiedSavePath/mod_image.png` 缺失
- Steam Cloud 记录显示本次退出仍然上传了：
  - `modded/profile1/saves/progress.save`
  - `modded/profile1/saves/prefs.save`
  说明当前发布版 `UnifiedSavePath` 没有把写盘路径统一回 vanilla profile

## 高可能

- 游戏内存在一个内置的 modding screen，并且入口很可能挂在设置界面中。
- 该 modding screen 不只是静态列表，还包含启用/禁用 mod、查看 mod 详情、显示 pending changes warning 的管理逻辑。
- 当前更值得优先验证的是“已加载 SmokeMod 对存档造成了什么影响”，而不是继续怀疑安装目录或最小产物模型。
- `UnifiedSavePath` 的静态依赖与本机程序集能对上，但当前发布版在 macOS 上的 Harmony patch 实际失败。

## 待验证

- 游戏内实际可见的 mod 列表入口位置：
  - 是否稳定出现在设置页
  - 是否还存在主菜单或其他入口
- 当前运行中的 modding screen 是否能直接列出 `SmokeMod` 并展示详情。
- mod 启用后导致的“存档问题”具体表现是什么：
  - 存档无法继续
  - 存档被标记 modded
  - 联机 / 校验不通过
  - 其他运行时副作用
- `UnifiedSavePath` 是否真的能在 macOS 上让旧档恢复到同一路径，而不只是消除 `modded/` 前缀。
- 如果试装 `UnifiedSavePath`，其实际生效路径究竟落到：
  - `steam/<id>/...`
  - `default/<id>/...`
  - 或其他候选路径
- 是否存在一个经过 macOS 修正的 `UnifiedSavePath` 变体，或需要本地重做更保守的 patch 方案。

## 当前阻塞

- 当前缺的是一次对游戏内 mod 列表 / 管理界面的直接可见性验证，而不是文件层面的继续猜测。
- 当前还没有完成“移除安装版 SmokeMod 后重新进游戏，观察存档是否恢复”的人工回归验证。
- 工作区结论文档仍然保留一部分以 consent gate 为中心的旧叙事，需要切换到新主线。
- `UnifiedSavePath` 作者只公开确认了 Windows + 单人测试范围，而当前公开发布版已经在本机 macOS 上触发 Harmony patch 异常。

## 最近完成

- 已确认安装目录应为 `Contents/MacOS/mods/`，并已体现在安装脚本中。
- 已把 `mod_manifest.json` 打进 SmokeMod 的 PCK 打包源。
- 已将 SmokeMod release 收敛到 DLL + PCK 两个核心产物。
- 已确认游戏最近一次启动已明确提示加载 `SmokeMod`。
- 已确认安装版 `SmokeMod.dll` / `SmokeMod.pck` 与工作区发布版完全一致。
- 已从程序集与现有参考文件中进一步确认游戏内部存在 modding screen、mod 行项、mod 信息面板、确认弹窗和外链按钮。
- 已完成 `UnifiedSavePath` 的本地二进制与 macOS 程序集对照，确认其 patch 目标在本机版本中存在。
- 已通过 macOS unified logging 确认 `UnifiedSavePath` 当前发布版初始化失败，根因落在 Harmony patch `get_IsRunningModded()`。

## 下一步依赖

- 在游戏内直接确认 mod 列表 / 管理界面的可见入口与实际显示效果。
- 如需隔离存档问题，可先手动删除游戏目录中的已安装 `SmokeMod`，再重新进游戏检查存档是否恢复。
- 如果要继续沿这条路线推进，下一步不该重复安装当前发布版，而应先分析 `get_IsRunningModded()` 的 Harmony patch 为什么在本机抛 `NotImplementedException`。
- 继续统一接班文件，避免新线程再回到“是否识别 mod”这一已跨过的问题。
