# Strike 判定分析

> 档案状态：PrimalForceStrike 旁支研究，不属于当前主线。

## 结论先行

当前本轮直接审计到的核心联动里，`Strike` 的识别主要不是按牌名字符串，而是按：

- `CardTag.Strike`

但还存在一条更窄的体系：

- `IsBasicStrikeOrDefend`

这条更窄体系要求的不只是 `Strike` tag，还要求牌本身是 `Basic` rarity。

## 已确认的通用 `Strike tag` 消费方

### `PerfectedStrike`

已确认其额外伤害统计逻辑会遍历 `Owner.PlayerCombatState.AllCards`，并按：

- `card.Tags.Contains(CardTag.Strike)`

进行计数。

结论：

- 这类交互只要给目标牌补上 `Strike` tag，就足够进入统计。

### `HellraiserPower`

已确认其抽牌触发逻辑会在 `AfterCardDrawnEarly(...)` 中按：

- `card.Tags.Contains(CardTag.Strike)`

决定是否自动打出。

结论：

- 这类交互只看 `Strike` tag。
- 当前本地化写着 “card containing Strike”，但代码实现并不是按牌名字符串搜索。

### `StrikeDummy`

已确认其 `ModifyDamageAdditive(...)` 会检查：

- `cardSource.Tags.Contains(CardTag.Strike)`

结论：

- `StrikeDummy` 按 tag 提供额外伤害。

### `FakeStrikeDummy`

已确认其实现与 `StrikeDummy` 同型：

- `cardSource.Tags.Contains(CardTag.Strike)`

结论：

- 同样只看 `Strike` tag。

### `SoldiersStew`

已确认其 `OnUse(...)` 会遍历 `AllCards`，再按：

- `card.Tags.Contains(CardTag.Strike)`

筛选施加效果对象。

结论：

- 这类交互也只看 `Strike` tag。

## 已确认的更窄体系：`Basic Strike/Defend`

### `CardModel.IsBasicStrikeOrDefend`

已确认其判断条件为：

1. `Rarity == Basic`
2. 且 `Tags` 包含 `Strike` 或 `Defend`

因此它不是通用 `Strike` 判定，而是：

- 基础打击 / 防御牌专用判定

### `PandorasBox`

已确认它使用：

- `card.IsBasicStrikeOrDefend()`

结论：

- 如果某张牌只是补了 `Strike` tag，但 rarity 不是 `Basic`，它仍然不会被 `PandorasBox` 当成目标。

### `GhostSeed`

已确认 `GhostSeed.CanAffect(...)` 要求：

- `Rarity == Basic`
- 且 `Tags` 包含 `Strike` 或 `Defend`

结论：

- 它不是通用 Strike 体系，而是基础 Strike/Defend 体系。

### `NutritiousSoup`

已确认 `AfterObtained()` 会筛：

- `Rarity == Basic`
- `Tags.Contains(CardTag.Strike)`

结论：

- 只补 `Strike` tag 还不够。

### `LeafyPoultice`

已确认其流程会先筛：

- `Rarity == Basic`

再分别找：

- `Tags.Contains(CardTag.Strike)`
- `Tags.Contains(CardTag.Defend)`

结论：

- 同样不是通用 Strike 体系。

## 对 `GiantRock` 的含义

当前已确认：

- `GiantRock` 默认不带 `Strike` tag
- `GiantRock` 的 rarity 不是 `Basic`

因此：

### 只补 `Strike` tag 就够的交互

- `PerfectedStrike`
- `Hellraiser`
- `StrikeDummy`
- `FakeStrikeDummy`
- `SoldiersStew`

### 只补 `Strike` tag 还不够的交互

- `PandorasBox`
- `GhostSeed`
- `NutritiousSoup`
- `LeafyPoultice`

## 当前边界

本轮没有穷举所有游戏里所有提到 “Strike” 的对象。

当前结论仅覆盖本轮直接审计到的这些对象。若后续要扩展到更广义的 “Strike 体系”，需要继续区分：

- 通用 `Strike tag`
- 基础 `Strike/Defend`
- 以及是否还有未审计到的其他特殊判定
