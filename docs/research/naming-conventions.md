# 原版命名总规则

记录日期：2026-03-23

## 结论先行

当前已能把 STS2 原版大多数“逻辑名 / 本地化 key / 资源名”关系总结为一条主干规则：

- 类名：`PascalCase`
- 内部 `ModelId.Entry`：由类名 `Slugify` 得到的全大写蛇形，如 `StrikeIronclad -> STRIKE_IRONCLAD`
- 显示文本 key：通常直接复用 `Entry`，再拼 `.title`、`.description`
- 资源文件名：通常把 `Entry` 转为全小写蛇形，如 `strike_ironclad`

这条规则在 `CardModel`、`PowerModel`、`RelicModel`、`CharacterModel` 上都成立，只是资源目录模板不同。

## 已确认

### 1. `ModelId` 是两段式：`Category.Entry`

- `AbstractModel` 持有 `Id`
- `ModelDb.GetId(Type)` 会生成：
  - `Category = ModelId.SlugifyCategory(GetCategoryType(type).Name)`
  - `Entry = StringHelper.Slugify(type.Name)`
- `ModelId.ToString()` 输出为 `Category + "." + Entry`

这意味着原版不是手写一堆散乱字符串，而是强依赖“类型名 -> slug”的统一生成。

### 2. `Slugify` 的结果是“全大写蛇形”

`StringHelper.Slugify(...)` 的行为已确认：

- 先把 `CamelCase` 拆成带 `_` 的形式
- 再整体转成大写
- 再移除非法字符

代表例子：

- `StrikeIronclad -> STRIKE_IRONCLAD`
- `GiantRock -> GIANT_ROCK`
- `WeakPower -> WEAK_POWER`
- `BurningBlood -> BURNING_BLOOD`
- `RandomCharacter -> RANDOM_CHARACTER`

### 3. 资源名普遍是 `entry.ToLowerInvariant()`

已确认的代码模板：

- 卡牌：`packed/card_portraits/<pool>/<entry-lower>.png`
- power：`images/powers/<entry-lower>.png`
- relic：`images/relics/<entry-lower>.png`
- 角色选择图：`packed/character_select/char_select_<entry-lower>.png`
- 角色顶栏图标：`images/ui/top_panel/character_icon_<entry-lower>.png`

因此资源层通常不是再从显示名推导，而是直接从内部 `Entry` 的小写版推导。

### 4. 显示名不是唯一键

最典型例子是基础牌：

- `StrikeIronclad -> STRIKE_IRONCLAD -> "Strike"`
- `StrikeDefect -> STRIKE_DEFECT -> "Strike"`
- `DefendIronclad -> DEFEND_IRONCLAD -> "Defend"`
- `DefendSilent -> DEFEND_SILENT -> "Defend"`

结论：

- 显示名对玩家友好
- 但显示名不足以唯一标识对象
- 真正稳定的是 `Entry`

### 5. power 与 debuff 没有分裂出两套 ID 风格

当前看到的状态类统一都以 `...Power` 结尾，ID 也统一以 `_POWER` 结尾：

- `WeakPower -> WEAK_POWER`
- `VulnerablePower -> VULNERABLE_POWER`
- `StrengthPower -> STRENGTH_POWER`
- `DexterityPower -> DEXTERITY_POWER`

Buff / Debuff 的区别不体现在命名层，而体现在 `PowerType`：

- `WeakPower.Type => Debuff`
- `VulnerablePower.Type => Debuff`
- `StrengthPower.Type => Buff`
- `DexterityPower.Type => Buff`

### 6. relic 命名不携带稀有度信息

当前未发现 relic 的 `Entry` 会编码 `COMMON`、`RARE` 之类前后缀。

- `BurningBlood -> BURNING_BLOOD`
- `PaperKrane -> PAPER_KRANE`
- `PaperPhrog -> PAPER_PHROG`
- `StrikeDummy -> STRIKE_DUMMY`

稀有度由 `RelicRarity` 属性单独表达，不写进 ID。

## 高可能

- 这套“类名单一真源 -> 自动派生 ID / key / 文件名”的设计，是原版内容命名的主轴，而不是少数对象的偶然一致。
- 自定义内容如果偏离这条主轴，最先受影响的通常不是编译，而是 console、资源定位、本地化查找和长期维护。

## 待验证

- 是否存在少量历史遗留类型，在 `Entry`、资源名或本地化 key 上有人工特例。
- `AfflictionModel`、`EnchantmentModel`、`PotionModel` 是否在全部对象上也完全遵循同一模板。

## 对后续角色 mod 的直接启示

- 先定类名，再让类名稳定地产生 `Entry`
- 不要把显示名当内部主键
- 不要把稀有度、数值、语言信息塞进内部 ID
- 资源命名尽量直接复用 `entry.ToLowerInvariant()`
- 中英双语都应围绕同一个 `Entry` 挂接，而不是各写一套内部名
