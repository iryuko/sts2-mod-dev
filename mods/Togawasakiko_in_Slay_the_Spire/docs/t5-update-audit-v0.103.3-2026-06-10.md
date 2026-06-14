# `v0.103.3` 本体更新兼容审计

日期：2026-06-10

## 一 本体更新事实

当前本机游戏版本已从旧引用基线 `v0.103.2` 更新到：

- `version`: `v0.103.3`
- `commit`: `460a0ece`
- `date`: `2026-05-29T13:36:05-07:00`
- `main_assembly_hash`: `418053415`

来源：

- `SlayTheSpire2.app/Contents/Resources/release_info.json`

已同步工作区引用：

- `references/api-notes/app/release_info.json`
- `references/game-dlls/sts2/arm64/sts2.dll`
- `references/game-dlls/sts2/x86_64/sts2.dll`

当前 hash：

- `references/game-dlls/sts2/arm64/sts2.dll`
  - `348523fa6a3dccbc0635d1068d306ca9549785c398eefbbda0c9b5219a6548a0`
- `references/game-dlls/sts2/x86_64/sts2.dll`
  - `4d2993d1dcab53f80a308e9707dace38c73e3410cc365f079413efb2f613adfb`

`GodotSharp.dll` 与 `0Harmony.dll` 本轮未见 hash 变化。

## 二 已验证未断的内容

### 1. loader / manifest / PCK 加载

最新 `godot.log` 显示：

- 游戏识别到 `Togawasakiko_in_Slay_the_Spire/mod_manifest.json`
- 成功加载 `Togawasakiko_in_Slay_the_Spire.dll`
- 成功加载 `Togawasakiko_in_Slay_the_Spire.pck`
- 成功调用 `TogawasakikoMod.Initialize()`
- 主菜单加载完成

当前未见启动期：

- `MissingMethodException`
- `Undefined target method`
- `HarmonyException`
- mod 初始化异常

### 2. 新引用编译

使用同步后的 `v0.103.3` `sts2.dll` 重新编译：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release`
- 结果：`0 warning / 0 error`

完整构建：

- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release`
- 结果：通过
- PCK 导出链仍可用
- 本轮构建中 `Normalized 156 import files into visible runtime_imports/`

### 3. 当前关键 patch / 私有反射目标

用临时反射探针核对后，以下本 mod 依赖的高风险目标在 `v0.103.3` 仍存在，签名未见直接断裂：

- `NCardLibrary._Ready`
- `NCardLibrary.UpdateCardPoolFilter`
- `NCardLibrary._poolFilters`
- `NCardLibrary._cardPoolFilters`
- `NCardLibrary._lastHoveredControl`
- `NEventRoom.SetupLayout`
- `NEventRoom.SetDescription`
- `NEventRoom._event`
- `NEventRoom._runState`
- `CombatManager.SetUpCombat(CombatState)`
- `NRun._Ready`
- `NRunMusicController._ExitTree`
- `NGlobalUi._Ready`
- `ModelDb.AllCardPools / AllCards / AllRelicPools / AllCharacters / AllEvents / AllAncients`
- `CharacterModel.MerchantAnimPath / RestSiteAnimPath / EnergyCounterPath`
- `CardPoolModel.EnergyIconPath`
- `EnergyIconHelper.GetPath(string)`
- `CardFactory.CreateForMerchant(...)`
- `CardFactory.GetDistinctForCombat(...)`
- `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch(...)`
- `ProgressSaveManager.CheckFifteenBossesDefeatedEpoch(...)`
- `ProgressSaveManager.ObtainCharUnlockEpoch(...)`
- `TouchOfOrobas.GetUpgradedStarterRelic(...)`
- `Glory / Hive / Overgrowth / Underdocks` 的事件池 getter
- `AncientEventModel.GenerateInitialOptionsWrapper`
- `AncientEventModel._generatedOptions`
- `NAncientDialogueLine.Create(...)`
- `NAncientDialogueLine._line / _ancient / _character`
- `NCharacterSelectScreen.SelectCharacter(...)`
- `NCharacterSelectButton._isLocked / _icon / _lock`
- `RunManager.State`
- `Player.RunState`
- `CardEnergyCost._localModifiers`
- `RunState._visitedEventIds`

### 4. 关键 scene / 路径

从新主 PCK 字符串中确认以下路径仍存在：

- `res://scenes/screens/card_library/card_library.tscn`
- `res://scenes/screens/card_library/library_pool_toggle.tscn`
- `Sidebar/MarginContainer/TopVBox/PoolFilters`
- `res://scenes/screens/char_select/char_select_button.tscn`
- `res://scenes/rooms/rest_site_room.tscn`
- `res://scenes/rooms/merchant_room.tscn`
- 原版角色 `merchant/characters/*`
- 原版角色 `rest_site/characters/*`
- `res://src/Core/Nodes/Audio/NRunMusicController.cs`

因此当前 `card library` 头像按钮注入、`jukebox` run/global UI 注入、角色 merchant/rest site scene patch 入口没有在本次更新中直接消失。

## 三 观察到的架构变化 / 更新点

### 1. 主程序集变更

`sts2.dll` 大小从旧引用的 `8869888` bytes 增至 `9059840` bytes。

这说明本轮不是单纯资源更新，主程序集确实有代码变化。

### 2. 命名空间整理已经成为事实基线

当前实际类型位置包括：

- `MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary.NCardLibrary`
- `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom`
- `MegaCrit.Sts2.Core.Nodes.NRun`
- `MegaCrit.Sts2.Core.Nodes.Audio.NRunMusicController`
- `MegaCrit.Sts2.Core.Nodes.CommonUi.NGlobalUi`
- `MegaCrit.Sts2.Core.Factories.CardFactory`
- `MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager`
- `MegaCrit.Sts2.Core.Runs.RunManager`
- `MegaCrit.Sts2.Core.Models.Acts.Glory / Hive / Overgrowth / Underdocks`

当前源码已经使用这些新命名空间，说明代码侧已跟上这一层架构整理。

### 3. 主 PCK 仍包含旧关键 scene 路径

本轮没有看到 `card_library`、`rest_site_room`、`merchant_room`、`char_select_button` 这些路径被搬迁。

## 四 当前 mod 明显落后的点

### 1. `references/pck-extract/sts2-main` 已严重过期

当前 `references/pck-extract/sts2-main/release_info.json` 仍是：

- `v0.98.3`

这已经不能作为 `v0.103.3` 的 scene / resource 事实源。

后续凡是涉及：

- card library 节点结构
- rest site scene
- merchant scene
- character select scene
- audio scene

都不能再引用这个旧提取目录作为最终依据。需要重新提取或建立只含关键 scene 的 `v0.103.3` 参考目录。

### 2. 角色选择按钮仍是手工 UI 注入

当前 `CharacterSelectInjector` 仍通过：

- 找 `NCharacterSelectScreen`
- 找 `CharacterButtons`
- 实例化 `res://scenes/screens/char_select/char_select_button.tscn`
- 反射写 `NCharacterSelectButton._isLocked / _icon / _lock`

这条链在 `v0.103.3` 仍可反射到字段，但形式上仍落后于稳定接口：

- 它依赖原版 scene 节点名。
- 它依赖私有字段名。
- 它不是一个被验证过的官方 mod 扩展点。

本轮不建议立刻重写，但每次本体更新都必须重新跑字段探针和实机选人测试。

### 3. 卡牌百科入口仍是私有字段注入

`NCardLibrary` 的 Sakiko 卡牌库按钮目前依赖：

- `_poolFilters`
- `_cardPoolFilters`
- `_lastHoveredControl`
- `Sidebar/MarginContainer/TopVBox/PoolFilters`
- `library_pool_toggle.tscn`

`v0.103.3` 中这些目标仍存在，但这仍是高脆弱 UI patch，不是稳定 API。

### 4. merchant / rest site 仍依赖自定义 scene 贴原版契约

当前 `CharacterModel.MerchantAnimPath / RestSiteAnimPath` patch 仍有效，但对应自定义 scene 必须继续满足原版 scene 节点契约。

这不是编译期能发现的问题。每次更新后都要实机进：

- 商店
- 火堆

确认没有因为原版 scene 契约细节变化导致卡房间或交互失效。

### 5. `jukebox` 仍同时依赖 Godot bus 与 FMOD 音量遮罩

`v0.103.3` 的主 PCK 仍能看到 `NRunMusicController` 与 FMOD `Bgm` 相关路径。

当前 `jukebox` 策略没有因本轮更新直接失效，但仍是高风险点：

- 原版房间音乐 / ambience 继续加载
- `jukebox` 播放时压原版音量
- merchant 还涉及 FMOD 参数链

本轮只能确认启动和 patch 入口，不等于确认火堆 / 商店不叠声。

### 6. 资源预加载仍有提示噪音

最新日志出现：

- `Asset not cached: res://images/ui/top_panel/character_icon_togawasakiko.png`

安装 PCK 已确认包含该资源和 runtime import，因此当前更像资源缓存 / 预加载提示，不是资源缺失。

但这说明角色 top panel 图标仍没有完全进入原版预加载缓存体系。若后续出现头像偶发空白或首帧加载抖动，应优先查 `CharacterModel.ExtraAssetPaths` 与新版本 preload 规则。

## 五 当前需要实机复测的点

本轮已确认“能启动到主菜单”，尚未确认“完整可玩闭环”。

优先复测：

1. 选人界面 Sakiko 按钮是否显示、可选、不会挡住原版按钮。
2. 进入新局后 top panel 头像是否正常。
3. 普通战斗开局是否正常。
4. 战斗奖励是否仍无坏牌 / starter 污染。
5. `Compose` 是否仍只从 song 池生成。
6. 商店是否仍可进入、购买、离开。
7. 火堆是否仍可休息 / 升级。
8. `jukebox` 播放状态下进火堆是否仍不叠声、不卡死。
9. `jukebox` 播放状态下进商店是否不叠声。
10. 击杀最后一只怪后奖励结算是否仍不被 `ProgressSaveManager` 卡住。
11. 卡牌百科中 Sakiko 头像按钮是否出现且能过滤。
12. 联机双人进入第一场战斗，确认 watcher / 随机生成牌仍同步。

## 六 当前结论

本轮 `v0.103.3` 更新没有直接打断 Sakiko mod 的：

- 编译
- 构建
- manifest 发现
- DLL / PCK 加载
- 初始化
- 关键 Harmony patch 目标
- 已知私有字段反射目标

但以下仍是落后或高风险面：

- 旧 PCK 提取参考仍停在 `v0.98.3`
- 选人按钮手工注入
- 百科按钮手工注入
- merchant / rest site 自定义 scene 契约
- `jukebox` 与 FMOD / Godot 音频遮罩
- top panel 图标预加载提示

因此下一步不是大改，而是按第五节做实机回归；只有实机复现问题后再对相应链路重构。
