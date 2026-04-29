# T5 卡牌库 / 百科大全接入方案

日期：2026-04-26

## 一 目标

在原版 `Compendium / 百科大全 -> Card Library / 卡牌库` 里，为丰川祥子建立独立卡牌库入口。

目标不是自建一个平行 UI，而是尽量沿用原版卡牌库：

- 原版卡牌 grid
- 原版筛选 / 排序
- 原版已发现 / 未发现 / 未解锁可见性
- 原版升级预览
- 原版卡图、标题、描述、本地化、统计展示

## 二 原版结构

### 1. 入口 scene

百科菜单入口：

- `scenes/screens/compendium_submenu.tscn`
- `NCompendiumSubmenu`

卡牌库主界面：

- `scenes/screens/card_library/card_library.tscn`
- `NCardLibrary`

卡牌库 grid：

- `NCardLibraryGrid`

池筛选按钮：

- `scenes/screens/card_library/library_pool_toggle.tscn`
- `NCardPoolFilter`

本地化表：

- `localization/<lang>/card_library.json`

### 2. 卡牌数据来源

原版 `NCardLibraryGrid._Ready()` 直接读：

```csharp
List<CardPoolModel> cardPoolModels = ModelDb.AllCardPools.ToList();
foreach (CardModel allCard in ModelDb.AllCards)
{
    if (allCard.ShouldShowInCardLibrary)
    {
        _allCards.Add(allCard);
    }
}
```

结论：

- 只要 Sakiko 的 `TogawasakikoCardPool` 能进入 `ModelDb.AllCardPools`
- 且 Sakiko 卡牌进入 `ModelDb.AllCards`
- 且 `ShouldShowInCardLibrary == true`
- 卡牌 grid 理论上就能显示这些牌

卡图不需要单独给卡牌库写一份。卡牌节点仍然走 `CardModel` 自身的 portrait / frame / energy icon / localization。

### 3. 可见性来源

原版 `NCardLibraryGrid.RefreshVisibility()` 使用：

```csharp
_seenCards = SaveManager.Instance.Progress.DiscoveredCards.ToHashSet();
UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
_unlockedCards = ModelDb.AllCardPools
    .Select(p => p.GetUnlockedCards(unlockState, CardMultiplayerConstraint.None))
    .SelectMany(c => c)
    .ToHashSet();
```

结论：

- 是否在库里出现：取决于 `ModelDb.AllCards` 和 `ShouldShowInCardLibrary`
- 是否完整显示：取决于 `Progress.DiscoveredCards`
- 是否锁定：取决于 `CardPoolModel.GetUnlockedCards(...)`

当前 Sakiko 卡池未自定义 epoch 过滤，因此进入池的牌默认是 unlocked；但未见过的牌仍会按原版显示为 unknown / locked 状态。

### 4. 左侧角色池筛选是硬编码

`NCardLibrary._Ready()` 里写死了五个原版角色：

```csharp
_ironcladFilter = GetNode<NCardPoolFilter>("%IroncladPool");
_silentFilter = GetNode<NCardPoolFilter>("%SilentPool");
_defectFilter = GetNode<NCardPoolFilter>("%DefectPool");
_regentFilter = GetNode<NCardPoolFilter>("%RegentPool");
_necrobinderFilter = GetNode<NCardPoolFilter>("%NecrobinderPool");
```

然后写死过滤逻辑：

```csharp
_poolFilters.Add(_ironcladFilter, c => c.Pool is IroncladCardPool);
_poolFilters.Add(_silentFilter, c => c.Pool is SilentCardPool);
_poolFilters.Add(_defectFilter, c => c.Pool is DefectCardPool);
_poolFilters.Add(_regentFilter, c => c.Pool is RegentCardPool);
_poolFilters.Add(_necrobinderFilter, c => c.Pool is NecrobinderCardPool);
```

并写死角色到按钮的映射：

```csharp
_cardPoolFilters.Add(ModelDb.Character<Ironclad>(), _ironcladFilter);
_cardPoolFilters.Add(ModelDb.Character<Silent>(), _silentFilter);
_cardPoolFilters.Add(ModelDb.Character<Defect>(), _defectFilter);
_cardPoolFilters.Add(ModelDb.Character<Necrobinder>(), _necrobinderFilter);
_cardPoolFilters.Add(ModelDb.Character<Regent>(), _regentFilter);
```

结论：

- Sakiko 卡牌可能已经进入 grid 数据源
- 但 UI 没有 Sakiko 的角色筛选按钮
- 更重要：如果在 Sakiko run 中打开卡牌库，`OnSubmenuOpened()` 会尝试 `_cardPoolFilters[characterModel]`
- 如果没有给 `_cardPoolFilters` 加 Sakiko 映射，存在 `KeyNotFoundException` 风险

## 三 推荐接口设计

新增一个专门的百科 / 卡牌库接入 patch 文件：

- `src/Patches/CardLibraryPatches.cs`

职责分三层。

### 1. 保证数据层进入原版库

建议补强 `ModelDb.AllCardPools` 与 `ModelDb.AllCards`：

- `ModelDb.AllCardPools` postfix：追加 `ModelDb.CardPool<TogawasakikoCardPool>()`
- `ModelDb.AllCards` postfix：追加 `TogawasakikoCardPool.AllCards`

原因：

- 当前已有 `ModelDb.AllCharacters` postfix 注入 Sakiko
- 理论上 `AllCharacterCardPools` 会间接包含 Sakiko 卡池
- 但 `ModelDb` 有 `_allCardPools / _allCards / _allCharacterCardPools` 缓存
- 如果缓存初始化早于角色注入，单靠 `AllCharacters` patch 不够稳

建议用去重 append，不破坏原版池顺序：

```csharp
if (!__result.Any(pool => pool.Id == togawaPool.Id))
{
    __result = __result.Concat(new[] { togawaPool });
}
```

卡牌 append 也按 `Id` 去重。

### 2. 注入 Sakiko 池筛选按钮

在 `NCardLibrary._Ready()` postfix 里：

1. 取得原版 `PoolFilters` GridContainer。
2. 实例化原版按钮 scene：
   - `scenes/screens/card_library/library_pool_toggle.tscn`
3. 给按钮设置：
   - node name：`TogawasakikoPool`
   - 图标：优先用 Sakiko 顶栏头像或 character icon
   - shadow texture 同步同一图
   - `Loc = new LocString("card_library", "POOL_TOGAWASAKIKO_TIP")`
4. 把按钮加入原版 `_poolFilters`：
   - `filter => card.Pool is TogawasakikoCardPool`
5. 把按钮加入原版 `_cardPoolFilters`：
   - `ModelDb.Character<Togawasakiko>() -> filter`
6. 连接按钮 `Toggled` 到原版 `UpdateCardPoolFilter(...)`。
   - 该方法是 private，需要 reflection / AccessTools 调用。
7. 连接 `FocusEntered`，更新原版 `_lastHoveredControl`。

这样做的好处：

- 不复制整个 `card_library.tscn`
- 不重写 `NCardLibrary`
- 原版筛选、排序、搜索、升级预览继续工作
- 只把 Sakiko 当作一个额外角色池接入

### 3. 本地化补表

当前 `ModSupport.EnsureLocalizationOverrides()` 已写：

- `characters`
- `cards`
- `powers`
- `relics`
- `ancients`
- `events`

要新增：

- `card_library`

建议新增：

```text
POOL_TOGAWASAKIKO_TIP
```

英文：

```text
Togawa Sakiko cards.
```

中文：

```text
丰川祥子的卡牌。
```

如果后续希望搜索栏关键词支持 `sakiko / togawa / song / pressure`，不要第一版硬扩；先只做池按钮和标准搜索。

## 四 资产接入

卡牌库不需要新增卡图资产接口。它使用 `CardModel` 自带信息：

- portrait path
- title / description localization
- rarity
- type
- energy cost
- frame material
- energy icon

本轮唯一可能新增的 UI 资产是左侧池筛选按钮图标。

推荐第一版不新增资源，直接复用：

- `ModelDb.Character<Togawasakiko>().IconTexture`

即顶栏头像。

如果后续要换专属卡牌库池图标，再补：

- `assets/icons/ui/card_library_pool_togawasakiko.png`
- runtime：`pack/images/packed/card_library/pool_filter_togawasakiko.png`

但第一版不要阻塞在新图上。

## 五 风险点

### 1. `OnSubmenuOpened()` 的 Sakiko run 崩溃风险

原版：

```csharp
key.IsSelected = _cardPoolFilters[characterModel] == key;
```

如果 Sakiko 是当前角色但 `_cardPoolFilters` 没注入 Sakiko，会直接炸。

所以只做数据层不够，必须补 `_cardPoolFilters`。

### 2. 私有字段 patch 风险

需要访问：

- `_poolFilters`
- `_cardPoolFilters`
- `_lastHoveredControl`

这些都是 `NCardLibrary` 的 private 字段。建议集中封装在一个 helper 里，不要散落 reflection。

### 3. `ModelDb` 缓存顺序风险

`ModelDb.AllCards / AllCardPools` 有静态缓存。若缓存早于 mod patch 初始化，数据可能缺 Sakiko。

所以建议对 `AllCardPools` 和 `AllCards` 都做 postfix append，而不是只依赖 `AllCharacters`。

### 4. 发现状态

卡牌库会尊重 `Progress.DiscoveredCards`。

不要为了“看见全部卡面”直接把所有 Sakiko 卡强行写进 discovered。那会偏离原版百科逻辑，也会污染玩家进度。

如果需要开发调试入口，应另做 debug console / 本地开关，不混进正式路径。

## 六 第一版验收

1. 主菜单百科大全 -> 卡牌库能看到 Sakiko 池按钮。
2. Sakiko run 中暂停菜单打开百科大全 -> 卡牌库，不抛 `KeyNotFoundException`。
3. 点击 Sakiko 池按钮，只显示 `TogawasakikoCardPool` 中 `ShouldShowInCardLibrary == true` 的牌。
4. Attack / Skill / Power / rarity / cost / search 过滤仍可工作。
5. `View Upgrades` 能显示升级预览。
6. 未发现卡仍按原版 unknown / locked 规则显示，不强行全解锁。
7. 原版五名角色、无色、先古、misc 池不受影响。

## 七 建议实现顺序

1. 新增 `CardLibraryPatches.cs`，只做数据层 append，构建验证。
2. 新增 `card_library` 本地化 override。
3. 在 `NCardLibrary._Ready()` postfix 注入 Sakiko 按钮与过滤器。
4. 实机验证主菜单卡牌库。
5. 实机验证 Sakiko run 暂停菜单卡牌库。
6. 再考虑是否做专属按钮图标资源。

## 八 第一版实现状态

2026-04-26 已完成第一版代码接入。

新增文件：

- `src/Patches/CardLibraryPatches.cs`

已实现：

- `ModelDb.AllCardPools` postfix append `TogawasakikoCardPool`
- `ModelDb.AllCards` postfix append Sakiko card pool 中的卡牌，并按 `Id` 去重
- `NCardLibrary._Ready()` postfix 注入 `TogawasakikoPool` 按钮
- 按用户指定，按钮图标直接复用 `ModelDb.Character<Togawasakiko>().IconTexture`
- 注入原版 `_poolFilters`：
  - `card => card.Pool is TogawasakikoCardPool`
- 注入原版 `_cardPoolFilters`：
  - `ModelDb.Character<Togawasakiko>() -> TogawasakikoPool`
- 连接按钮 `Toggled` 到原版 `UpdateCardPoolFilter`
- 连接 `FocusEntered`，更新原版 `_lastHoveredControl`
- 新增 `card_library` 本地化 override：
  - `POOL_TOGAWASAKIKO_TIP`

验证：

- `dotnet build -c Release` 通过，`0 warning / 0 error`
- 已执行 release build 与安装
- release/install 的 `dll / pck / mod_manifest.json` 哈希已核对一致

待实机复测：

- 主菜单百科大全 -> 卡牌库是否出现 Sakiko 头像按钮
- Sakiko run 中暂停菜单 -> 百科大全 -> 卡牌库是否不再因为 `_cardPoolFilters[characterModel]` 抛错
- 点击 Sakiko 头像按钮后是否只显示 Sakiko 卡池
- 原版五角色 / 无色 / 先古 / misc 过滤是否不受影响
