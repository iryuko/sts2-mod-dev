# 卡牌命名笔记

记录日期：2026-03-23

## 已确认规律

### 1. 卡牌类名直接生成卡牌 ID

证据链：

- `ModelDb.GetEntry(Type)` 返回 `StringHelper.Slugify(type.Name)`
- `CardModel.TitleLocString => new LocString("cards", Id.Entry + ".title")`
- `CardModel.Description => new LocString("cards", Id.Entry + ".description")`

因此卡牌的主命名链是：

- 类名 `StrikeIronclad`
- `Id.Entry = STRIKE_IRONCLAD`
- 本地化 key：
  - `STRIKE_IRONCLAD.title`
  - `STRIKE_IRONCLAD.description`

### 2. 卡图文件名跟 `Entry` 小写版走

`CardModel` 已确认：

- 图集路径：`atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/{entry-lower}.tres`
- PNG 路径：`packed/card_portraits/{Pool.Title.ToLowerInvariant()}/{entry-lower}.png`

代表例子：

| 类名 | `Entry` | 资源文件 | 显示名 |
| --- | --- | --- | --- |
| `StrikeIronclad` | `STRIKE_IRONCLAD` | `ironclad/strike_ironclad.png` | `Strike` |
| `DefendIronclad` | `DEFEND_IRONCLAD` | `ironclad/defend_ironclad.png` | `Defend` |
| `PrimalForce` | `PRIMAL_FORCE` | `ironclad/primal_force.png` | `Primal Force` |
| `GiantRock` | `GIANT_ROCK` | `token/giant_rock.png` | `Giant Rock` |
| `Hellraiser` | `HELLRAISER` | `ironclad/hellraiser.png` | `Hellraiser` |

### 3. 卡池名决定资源子目录

`CardModel` 用的是 `Pool.Title.ToLowerInvariant()`，不是角色 `Id.Entry` 本身。

已确认示例：

- `IroncladCardPool.Title => "ironclad"`
- 因此 `StrikeIronclad`、`PrimalForce`、`Hellraiser` 的卡图都在 `packed/card_portraits/ironclad/`
- `GiantRock` 属于 `TokenCardPool`，因此卡图在 `packed/card_portraits/token/`

### 4. 显示名会复用，内部 ID 不会

最重要的实际规律：

- `STRIKE_IRONCLAD.title = Strike`
- 其他角色也会有自己的 `STRIKE_<CHAR>` 风格

所以：

- 显示名是面向玩家的
- `Entry` 才是面向系统和调试的

### 5. 基础牌常用角色后缀做消歧

当前已确认基础牌至少如此：

- `StrikeIronclad -> STRIKE_IRONCLAD`
- `DefendIronclad -> DEFEND_IRONCLAD`
- `DefendSilent -> DEFEND_SILENT`
- `DefendDefect -> DEFEND_DEFECT`

这说明原版不会拿一个全局 `STRIKE` / `DEFEND` 去区分所有角色版本。

## console 结论

`CardConsoleCmd` 与 `RemoveCardConsoleCmd` 已确认：

- 都会把参数做 `ToUpperInvariant()`
- 都按 `c.Id.Entry == cardName` 精确匹配
- 命令说明文字明确写了：
  - `Screaming snake case ('BODY_SLAM', not 'Body Slam')`

结论：

- console 对卡牌更像“吃 `Entry`”
- 不是吃显示名
- 也不是吃类名原文

## 高可能

- 自定义角色的基础牌最好继续使用 `Strike<CharName>` / `Defend<CharName>` 这种消歧风格。
- token 卡若会跨系统暴露，最好也让类名和 `Entry` 具备清晰语义，而不是只叫 `Rock`、`Token` 这种过宽名称。

## 待验证

- 是否存在少数卡牌覆盖 `AllPortraitPaths` 或 `VisualCardPool`，从而让卡图目录与默认池目录不同。
- 是否存在原版卡牌使用与类名不一致的人工 `Entry` 覆盖。当前样本中未见。
