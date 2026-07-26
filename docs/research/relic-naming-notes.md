# Relic 命名笔记

记录日期：2026-03-23

## 已确认规律

### 1. relic 类名直接生成 `Entry`

代表关系：

| 类名 | `Entry` | 显示名 | 文件名 |
| --- | --- | --- | --- |
| `Akabeko` | `AKABEKO` | `Akabeko` | `akabeko.png` |
| `BurningBlood` | `BURNING_BLOOD` | `Burning Blood` | `burning_blood.png` |
| `StrikeDummy` | `STRIKE_DUMMY` | `Strike Dummy` | `strike_dummy.png` |
| `PaperKrane` | `PAPER_KRANE` | `Paper Krane` | `paper_krane.png` |
| `PaperPhrog` | `PAPER_PHROG` | `Paper Phrog` | `paper_phrog.png` |

### 2. 显示文本来自 `relics.json`

`RelicModel` 已确认：

- `Title => relics/<Entry>.title`
- `Description => relics/<Entry>.description`
- `Flavor => relics/<Entry>.flavor`

这说明 relic 相比卡牌，多了一层常见的 `flavor` 文本，但 key 主干仍然是同一个 `Entry`。

### 3. 图标资源以 `entry-lower` 为中心

`RelicModel` 已确认：

- 大图：`images/relics/<entry-lower>.png`
- 打包图集：`atlases/relic_atlas.sprites/<entry-lower>.tres`
- 轮廓图集：`atlases/relic_outline_atlas.sprites/<entry-lower>.tres`

### 4. 稀有度不进 ID

示例可见：

- `BurningBlood.Rarity => Starter`
- `PaperKrane.Rarity => Rare`
- `PaperPhrog.Rarity => Uncommon`

但它们的 `Entry` 都没有把 `STARTER`、`RARE`、`UNCOMMON` 编进名字。

## console 结论

`RelicConsoleCmd` 与卡牌不同，不是纯精确匹配。

它的匹配顺序已确认是：

1. `Id.Entry == 输入`
2. `Id.Entry.StartsWith(输入)`
3. `Id.Entry.Contains(输入)`

因此：

- `relic STRIKE_DUMMY` 最稳定
- `relic strike` 有较高碰撞风险
- `relic paper` 可能命中多个对象

结论：relic console 虽然“更宽松”，但做稳定测试时仍应坚持完整 `Entry`。

## 高可能

- 自定义 relic 若想兼容 console 与长期维护，仍应优先用完整唯一的全大写蛇形 `Entry`，不要依赖 contains 级别的模糊匹配。

## 待验证

- 是否存在少量 relic 覆盖 `IconBaseName`，导致图标文件名不等于 `entry-lower`。当前样本中未见。
