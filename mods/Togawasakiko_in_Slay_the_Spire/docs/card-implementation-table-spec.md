# 卡牌实现表格字段规范

日期：2026-03-27

## 一 目的

本文件用于规定“卡牌实现表格”应包含哪些列，以及每一列应如何填写。

目标不是做视觉排版模板，而是让后续表格能直接支撑：

- 卡牌模型落库
- 本地化文本录入
- 稀有度与奖励池接入
- `Pressure` / song / token 等特殊逻辑实现
- 卡图资产对位

如果后续你把卡牌需求交给我，优先按本文件的列来给。

## 二 使用原则

- 一张牌占表格中的一行。
- 内部字段优先写稳定英文主键，不直接拿中文显示名当主键。
- 未冻结的内容可以写“待定”，但不要混写多个版本。
- 数值写当前拟定值，不要写“差不多”“可能”“参考”。
- 升级若有分支，必须写清楚“升级后变什么”，不要只写“加强”。

## 三 推荐列顺序

建议表格至少按以下顺序建列：

1. `内部类名`
2. `Entry`
3. `资源文件名`
4. `中文名`
5. `英文名`
6. `卡牌类型`
7. `稀有度`
8. `费用`
9. `目标`
10. `是否初始卡组`
11. `是否正常奖励池`
12. `是否衍生牌`
13. `基础效果`
14. `升级效果`
15. `关键词`
16. `变量数值`
17. `与 Pressure 的关系`
18. `与 song 的关系`
19. `实现备注`
20. `卡图状态`

## 四 各列写法规范

### 1. `内部类名`

用途：

- 直接映射 C# 类名

写法：

- `PascalCase`
- 不带空格
- 不带中文

示例：

- `StrikeTogawasakiko`
- `Slander`
- `PersonaDissociation`

### 2. `Entry`

用途：

- 直接映射模型 `Entry`
- 对应本地化 key 与资源主键

写法：

- 全大写蛇形

示例：

- `STRIKE_TOGAWASAKIKO`
- `SLANDER`
- `PERSONA_DISSOCIATION`

### 3. `资源文件名`

用途：

- 对应卡图文件 base name

写法：

- 全小写蛇形
- 不带扩展名

示例：

- `strike_togawasakiko`
- `slander`
- `persona_dissociation`

### 4. `中文名`

用途：

- 对应显示名

写法：

- 直接填最终或暂定中文名

示例：

- `打击`
- `中伤`

### 5. `英文名`

用途：

- 对应英文显示名

写法：

- 正常标题写法

示例：

- `Strike`
- `Slander`

### 6. `卡牌类型`

用途：

- 映射 `CardType`

允许值：

- `Attack`
- `Skill`
- `Power`
- `Status`
- `Curse`

不要写：

- `攻击`
- `白卡`
- `技能牌`

这些属于解释，不属于内部枚举值。

### 7. `稀有度`

用途：

- 映射 `CardRarity`
- 决定是否进入正常奖励池

允许值：

- `Basic`
- `Common`
- `Uncommon`
- `Rare`
- `Colorless`
- `Token`
- `Status`
- `Curse`
- `Event`
- `Ancient`

当前项目建议：

- 初始打击 / 防御：`Basic`
- 中文品质对照：
  - `Basic -> 基础牌 / 初始牌`
  - `Common -> 普通`
  - `Uncommon -> 非凡 / 罕见`
  - `Rare -> 稀有`
  - `Event -> 事件牌`
  - `Ancient -> Ancient 牌 / 古代牌`
  - `Token -> Token / 衍生牌`
  - `Status -> 状态牌`
  - `Curse -> 诅咒牌`
- 正常可奖励牌：优先 `Common / Uncommon / Rare`
- 压力兑换衍生牌：`Token`

### 8. `费用`

用途：

- 映射基础能量费用

写法：

- 直接写整数
- `X` 费用写 `X`
- 无费用状态牌写 `0`

示例：

- `0`
- `1`
- `2`
- `X`

### 9. `目标`

用途：

- 映射 `TargetType`

允许值：

- `None`
- `Self`
- `AnyEnemy`
- `AnyAlly`
- `AllEnemies`
- `AllAllies`

如果你不确定原版枚举名，至少也按上面这组写，不要自由发挥。

### 10. `是否初始卡组`

用途：

- 标记是否属于角色开局 `StartingDeck`

写法：

- `是`
- `否`

### 11. `是否正常奖励池`

用途：

- 标记该牌是否应出现在战斗奖励、商店或其他正常生成来源中

写法：

- `是`
- `否`

说明：

- `Token` / 衍生牌通常写 `否`
- 起始牌若后续也能被正常奖励拿到，可以写 `是`

### 12. `是否衍生牌`

用途：

- 标记是否属于 `Pressure` 兑换牌、临时生成牌、token

写法：

- `是`
- `否`

### 13. `基础效果`

用途：

- 作为主要实现说明

写法：

- 用短句写清楚打出后发生什么
- 多段效果用分号或编号拆开

推荐写法：

- `造成 8 点伤害`
- `获得 7 点格挡；给予 1 个目标 2 层压力`
- `消耗目标 2 层压力；将 1 张“XXX”加入手牌`

不要只写：

- `压制`
- `伤害`
- `起甲`

这种粒度太粗，无法直接实现。

### 14. `升级效果`

用途：

- 明确升级后变更项

写法：

- 必须写“改了什么”
- 如果不变，也明确写 `无变化`

推荐写法：

- `费用 1 -> 0`
- `伤害 8 -> 11`
- `格挡 5 -> 9`
- `改为消耗 3 层压力`

### 15. `关键词`

用途：

- 映射卡牌关键字

允许用逗号分隔多个值。

当前推荐写法：

- `无`
- `Exhaust`
- `Ethereal`
- `Retain`
- `Innate`

如果是项目原创标签，可写：

- `Song`

但若写了原创标签，最好同时在 `实现备注` 里写清规则。

### 16. `变量数值`

用途：

- 记录需要落到 `DynamicVar` 的数值

写法：

- 用 `键=值` 形式
- 多个值用分号分开

示例：

- `Damage=6`
- `Block=5; Pressure=2`
- `Damage=4; BonusPerPressure=3`

### 17. `与 Pressure 的关系`

用途：

- 单独标记这张牌与核心机制的交互方式

建议从以下几类里选：

- `无`
- `施加压力`
- `消耗压力`
- `按压力层数结算`
- `需要目标有压力`
- `把压力兑换为衍生牌`

如果关系复杂，可以组合写：

- `施加压力；按压力层数结算`

### 18. `与 song 的关系`

用途：

- 标记是否属于 song 子集或与 song 机制联动

写法：

- `无`
- `Song`
- `消费 Song`
- `生成 Song`
- `受 Song 数量影响`

### 19. `实现备注`

用途：

- 放无法塞进基础效果列的技术说明

适合写：

- `需要自选目标`
- `升级走减费而不是加数值`
- `会生成 1 张 Token`
- `本回合内生效`
- `需要新增 Power`

不适合写：

- 大段剧情文案
- 美术意见

### 20. `卡图状态`

用途：

- 给 T3 / 资源接线时快速判断当前是否缺图

推荐写法：

- `未开始`
- `占位已接`
- `正式图待上传`
- `正式图已接`

## 五 最低可实现列

如果你这次先不给完整大表，至少保证这几列有值：

- `内部类名`
- `Entry`
- `中文名`
- `卡牌类型`
- `稀有度`
- `费用`
- `目标`
- `基础效果`
- `升级效果`
- `是否正常奖励池`

没有这些字段，我就只能先做骨架，不能稳定实现。

## 六 推荐补充列

如果你后面准备一次性交比较完整的设计，建议再加这些列：

- `所属子体系`
- `预计卡池阶段`
- `是否需要新 Power`
- `是否需要新 Relic/状态联动`
- `AI/敌人可用性`
- `是否需要动画/特效`
- `文案是否冻结`

## 七 推荐表格示例

下面是推荐行格式示例：

| 内部类名 | Entry | 资源文件名 | 中文名 | 英文名 | 卡牌类型 | 稀有度 | 费用 | 目标 | 是否初始卡组 | 是否正常奖励池 | 是否衍生牌 | 基础效果 | 升级效果 | 关键词 | 变量数值 | 与 Pressure 的关系 | 与 song 的关系 | 实现备注 | 卡图状态 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Slander | SLANDER | slander | 中伤 | Slander | Attack | Basic | 1 | AnyEnemy | 是 | 否 | 否 | 造成 4 点伤害；目标每有 1 层压力，额外造成 3 点伤害 | 费用 1 -> 0 | 无 | Damage=4; BonusPerPressure=3 | 按压力层数结算 | 无 | 起始牌，奖励池应排除 | 正式图待上传 |
| Unendurable | UNENDURABLE | unendurable | 难熬 | Unendurable | Skill | Basic | 2 | AnyEnemy | 是 | 否 | 否 | 获得 8 点格挡；给予 1 个目标 3 层压力 | 格挡 8 -> 11；压力 3 -> 4 | 无 | Block=8; Pressure=3 | 施加压力 | 无 | 起始牌，奖励池应排除 | 正式图待上传 |
| PerkUp | PERK_UP | perk_up | 抖擞精神 | Perk Up | Skill | Common | 0 | None | 否 | 是 | 否 | 随机将 `1` 张无色牌加入手牌 | 额外抽 `1` 张牌 | 无 | Draw=1 (upgrade only) | 无 | 无 | 需从无色池随机生成 | 正式图已接 |
| Speak | SPEAK | speak | 说话！ | Speak! | Skill | Common | 0 | None | 否 | 是 | 否 | 将 `1` 张本场战斗费用为 `0` 的【人格解离】加入手牌 | 获得 `Innate` | Exhaust | GeneratedPersonaDissociationCost=0 | 无直接消耗压力 | 无 | 生成的是临时费用修改版人格解离，不改原衍生牌定义 | 正式图已接 |
| RestorationOfPower | RESTORATION_OF_POWER | restoration_of_power | 复权 | Restoration of Power | Skill | Common | 0 | None | 否 | 是 | 否 | 随机将 `1` 张压力衍生牌加入手牌 | 改为独立随机加入 `2` 张 | Exhaust | GeneratedPressureCards=1 | 直接从冻结压力衍生牌池随机生成 | 无 | 当前允许升级后两张重复 | 正式图已接 |
| PutOnYourMask | PUT_ON_YOUR_MASK | put_on_your_mask | 面具戴好 | Put On Your Mask | Skill | Common | 1 | AnyEnemy | 否 | 是 | 否 | 施加 `2` 层 Weak；若目标在打出前已拥有 Weak，你获得 `1` 层【颜】Power | Weak `2 -> 3` | 无 | Weak=2; FaceStacks=1 | 无直接消耗压力 | 无 | “已拥有 Weak”按打牌前状态检查 | 正式图已接 |
| SeverThePast | SEVER_THE_PAST | sever_the_past | 斩断过去 | Sever the Past | Attack | Common | 1 | AnyEnemy | 否 | 是 | 否 | 造成 `8` 点伤害；将弃牌堆洗回抽牌堆 | 伤害 `8 -> 12` | 无 | Damage=8 | 无 | 无 | 若弃牌堆为空，仅结算伤害 | 正式图已接 |
| SoManyMaggots | SO_MANY_MAGGOTS | so_many_maggots | 好多蛆 | So Many Maggots | Skill | Common | 1 | AnyPlayer | 否 | 是 | 否 | 选择 `1` 名玩家；若你手中有牌则丢弃 `1` 张；然后移除其身上的所有负面 Power | 费用 `1 -> 0` | 无 | Discard=1; RemoveAllDebuffs=Yes | 无直接消耗压力 | 无 | 当前“负面 Power”冻结为 `PowerType.Debuff`；若无牌可弃仅跳过弃牌；无显式目标时保守回退到自身 | 正式图已接 |
| AnswerMe | ANSWER_ME | answer_me | 作出回答 | Answer Me | Skill | Common | 2 | AllEnemies | 否 | 是 | 否 | 压力 `< 5` 的敌人获得 `7` 层压力；压力 `>= 5` 的敌人失去 `1` 点力量 | 费用 `2 -> 1` | 无 | PressureThreshold=5; PressureGain=7; StrengthLoss=1 | 依当前压力分支结算 | 无 | 全体逐目标独立判定 | 正式图已接 |
| WeightliftingChampion | WEIGHTLIFTING_CHAMPION | weightlifting_champion | 举重冠军 | Weightlifting Champion | Skill | Common | 1 | Self | 否 | 是 | 否 | 失去 `4` 点生命；获得 `1` 点力量和 `1` 点敏捷 | 生命损失 `4 -> 2` | Exhaust | HpLoss=4; Strength=1; Dexterity=1 | 无 | 无 | 当前按 `Lose HP` 实现，不视为受到伤害 | 正式图已接 |
| PersonaDissociation | PERSONA_DISSOCIATION | persona_dissociation | 人格解离 | Persona Dissociation | Skill | Token | 1 | AnyEnemy | 否 | 否 | 是 | 给予目标 `1` 层“人格解离”；该 debuff 按每次独立受伤事件逐层翻倍并逐层消耗 | 无变化 | Exhaust, Ethereal | Amount=1 | 把压力兑换为衍生牌 | 无 | 需对应可叠层 Power | 正式图已接 |
| TreasurePleasure | TREASURE_PLEASURE | treasure_pleasure | Treasure Pleasure | Treasure Pleasure | Skill | Uncommon | 0 | None | 否 | 是 | 否 | 自己获得 `1` 层人格解离，并获得【磁场力量-地狱战神】直到回合结束 | 移除 Exhaust | Exhaust, Song | PersonaDissociation=1 | 无直接消耗压力 | song 子集 | 需对应一回合持续 replay power；当前卡图已接 | 正式图已接 |
| DawnOfDespair | DAWN_OF_DESPAIR | dawn_of_despair | 绝望伊始 | Dawn of Despair | Attack | Uncommon | 1 | AnyEnemy | 否 | 是 | 否 | 造成 `2` 点伤害 `6` 次；然后使目标本回合获得【绝望回响】 | 伤害次数 `6 -> 7` | 无 | Damage=2; HitCount=6; DespairEcho=1 | 通过 `SakikoDespairEchoPower` 让后续每个伤害事件追加施压 | 无 | 当前按每个独立伤害事件触发 `Despair Echo`；避免与原版 `DESPAIR_POWER` 撞 key | 正式图已接 |
| GeorgetteMeGeorgetteYou | GEORGETTE_ME_GEORGETTE_YOU | georgette_me_georgette_you | Georgette Me, Georgette You | Georgette Me, Georgette You | Skill | Uncommon | 1 | AnyEnemy | 否 | 是 | 否 | 指定 1 名敌人；若你的当前生命值大于等于其当前生命值，则施加 `7` 层压力，否则造成 `7` 点伤害 | 压力 / 伤害均 `7 -> 9` | Song | Damage=7; Pressure=7 | 与压力体系直接联动 | song 子集 | 比较的是当前生命值，不是最大生命值 | 正式图已接 |
| SymbolI | SYMBOL_I | symbol_i | Symbol I | Symbol I | Attack | Common | 1 | RandomEnemy | 否 | 是 | 否 | 造成 `3` 点伤害 `3` 次，每次随机选择目标。你在本回合获得【Symbol I】 | 每段伤害 `3 -> 4` | Song | Damage=3; HitCount=3 | 无直接消耗压力 | song 子集 | `3` 次伤害分别独立随机选敌；`Symbol I` 本身只作为本回合标志物 | 占位图已接 |
| SymbolIi | SYMBOL_II | symbol_ii | Symbol II | Symbol II | Attack | Uncommon | 2 | AllEnemies | 否 | 是 | 否 | 对所有敌人造成 `6` 点伤害，并使其各获得 `1` 层【自卑】。你在本回合获得【Symbol II】 | 伤害 `6 -> 9` | Song | Damage=6; Inferiority=1 | 施加【自卑】时仍会按冻结规则尝试兑换【过劳焦虑】 | song 子集 | `Symbol II` 只作为本回合标志物；自卑结算沿用当前冻结实现 | 正式图已接 |
| SymbolIii | SYMBOL_III | symbol_iii | Symbol III | Symbol III | Skill | Uncommon | 0 | Self | 否 | 是 | 否 | 获得 `3` 点格挡。本场战斗中，每次打出这张牌时，其获得的格挡增加 `3` 点。你在本回合获得【Symbol III】 | 初始格挡 `3 -> 7` | Song | Block=3; IncrementPerPlay=3 | 无直接消耗压力 | song 子集 | 当前按卡实例的 battle counter 实现：第一次 `3/7`，之后每次额外 `+3` | 正式图已接 |
| SymbolIv | SYMBOL_IV | symbol_iv | Symbol IV | Symbol IV | Power | Rare | 2 | Self | 否 | 是 | 否 | 获得【Symbol IV】；其效果为每回合额外抽 `1` 张牌 | 费用 `2 -> 1` | Song | DrawPerTurn=1 | 无直接消耗压力 | song 子集 | `Symbol IV` 可叠层，抽牌按层数结算；但回合结束统计 Symbol 时无论几层都只算 `1` 种 | 正式图已接 |
| LeaveItToMe | LEAVE_IT_TO_ME | leave_it_to_me | 交给我吧 | Leave It to Me | Attack | Uncommon | 2 | AnyEnemy | 否 | 是 | 否 | 造成 `11` 点伤害；移除目标至多 `7` 层压力；回复 `5` 点生命；若仍有压力，施加 `1` 层 Weak | 伤害 `11 -> 15` | 无 | Damage=11; PressureRemove=7; Heal=5; Weak=1 | 消耗目标压力但不要求足额 | 无 | 回复生命无条件触发，Weak 按移除后剩余压力判定 | 正式图已接 |
| BailMoney | BAIL_MONEY | bail_money | 保释金 | Bail Money | Attack | Uncommon | 0 | AnyEnemy | 否 | 是 | 否 | 造成 `8` 点伤害；使目标失去 `1` 点敏捷；然后你失去 `10` Gold | 伤害 `8 -> 12` | 无 | Damage=8; DexterityLoss=1; GoldLoss=10 | 无直接消耗压力 | 无 | 当前 Gold 不足时仍可打出，扣款下限交由原版 `LoseGold` 链处理 | 正式图已接 |
| Innocence | INNOCENCE | innocence | 天真 | Innocence | Power | Uncommon | 1 | Self | 否 | 是 | 否 | 每回合开始时，使所有敌人获得 `2` 层自闭 | 自闭 `2 -> 3` | 无 | SocialWithdrawalPerTurn=2 | 与自闭体系直接联动 | 无 | 通过独立 `InnocencePower` 持续实现 | 正式图已接 |
| Housewarming | HOUSEWARMING | housewarming | 乔迁 | Housewarming | Skill | Uncommon | 0 | None | 否 | 是 | 否 | 将 `1` 张带有 `Ethereal + Exhaust` 的【大狗大狗叫叫叫】加入手牌 | 获得 `Innate` | 无 | GeneratedBarking=1 | 无 | 无 | 生成的是临时修饰版本，不改原始 `Barking Barking Barking` 定义 | 正式图已接 |
| FinalCurtain | FINAL_CURTAIN | final_curtain | 谢幕 | Final Curtain | Attack | Rare | 1 | AllEnemies | 否 | 是 | 否 | 对所有敌人造成 `5` 点伤害，重复次数等于当前敌人总数 | 每段伤害 `5 -> 7` | 无 | Damage=5; HitCount=LivingEnemyCount | 无 | 无 | 按 AOE 多次结算，不折算成单次乘区伤害 | 正式图已接 |
| BladeThroughTheHeart | BLADE_THROUGH_THE_HEART | final_curtain (runtime placeholder path) | 利刃穿心 | Blade Through the Heart | Attack | Rare | 2 | AllEnemies | 否 | 是 | 否 | 先使所有敌人获得 `2` 层 Vulnerable、`2` 层 Weak、`-1 Dexterity`，再造成 `12` 点伤害 | 伤害 `12 -> 20` | 无 | Damage=12; Vulnerable=2; Weak=2; DexterityLoss=1 | 无直接消耗压力 | 无 | debuff 全部先施加，再统一结算 AOE 伤害 | 运行时先复用现有图片路径 |
| Fragility | FRAGILITY | face (runtime placeholder path) | 脆弱 | Fragility | Skill | Uncommon | 3 | Self | 否 | 是 | 否 | 获得 `17` 点格挡，获得 `1` 层【颜】，回复 `8` 点生命 | 格挡 `17 -> 21`；回复 `8 -> 11` | Exhaust | Block=17; Heal=8; FaceStacks=1 | 通过【颜】与压力体系形成反击联动 | 无 | 本轮按当前修正版【颜】Power 接线 | 运行时先复用现有图片路径 |
| ShadowOfThePastI | SHADOW_OF_THE_PAST_I | i_have_ascended (runtime placeholder path) | 往日之影1 | Shadow of the Past I | Curse | Event | 0 | None | 否 | 否 | 否 | `Unplayable`；经历 `2` 场战斗后，从牌组移除并获得 `+7 Max HP` | 不可升级 | Unplayable | TriggerAfterCombats=2; MaxHpGain=7 | 无 | 无 | event-only curse-like 长期牌；当前不进正常卡池 | 运行时先复用现有图片路径 |
| ShadowOfThePastII | SHADOW_OF_THE_PAST_II | housewarming (runtime placeholder path) | 往日之影2 | Shadow of the Past II | Curse | Event | 0 | None | 否 | 否 | 否 | `Unplayable`；经历 `2` 场战斗后，从牌组移除并移除 `1` 张 starter strike 与 `1` 张 starter defend | 不可升级 | Unplayable | TriggerAfterCombats=2 | 无 | 无 | 当前只精确命中 `StrikeTogawasakiko / DefendTogawasakiko` | 运行时先复用现有图片路径 |
| ShadowOfThePastIII | SHADOW_OF_THE_PAST_III | treasure_pleasure (runtime placeholder path) | 往日之影3 | Shadow of the Past III | Curse | Event | 0 | None | 否 | 否 | 否 | `Unplayable`；经历 `2` 场战斗后，从牌组移除并升级 starter relic | 不可升级 | Unplayable | TriggerAfterCombats=2 | 无 | 无 | 当前通过 `RelicCmd.Replace(DollMask, UpgradedDollMask)` 落地 | 运行时先复用现有图片路径 |

## 八 填表时的禁忌

- 不要把“设计思路”写进 `费用`、`目标`、`稀有度` 列。
- 不要把多个候选内部名混在同一格。
- 不要把“升级后更强”当作升级描述。
- 不要把 `Token` 写成 `Common`，也不要把临时生成牌误写成正常奖励池牌。
- 不要把中文名直接拿来替代 `内部类名` 或 `Entry`。

## 九 建议交付格式

你后续可以任选以下格式给我：

- Markdown 表格
- Excel / Numbers 导出的列式文本
- CSV

但无论哪种格式，列名都尽量对齐本文件，能减少我二次整理的时间。

## 十 当前建议

如果你准备开始逐批交卡，建议先分三张表：

1. `Starter / Basic`
2. `正常奖励池`
3. `Pressure 衍生牌 / Token`

这样实现顺序会最稳，不会把正常卡池和临时生成牌混在一起。
