# STS2 Dev Console 指令参照

记录日期：2026-05-04

依据：

- `references/game-dlls/sts2/arm64/sts2.dll`
- `MegaCrit.Sts2.Core.DevConsole`
- `MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.*ConsoleCmd`
- `ModelDb` / `ModelId` / `StringHelper.Slugify`
- `references/pck-extract/sts2-main/localization/eng/*.json` 本地化 key 抽样

本文确认的是原版代码里的命令注册、参数解析、Entry 来源与主要效果；没有逐条进游戏实测。

## 总规则

- 原版内置命令共 38 个；`help` 是内置特殊命令，不是 `AbstractConsoleCmd` 子类。
- `help` 列出当前已注册命令；`help <cmd>` 显示单条命令的参数与说明。
- 输入按空格切分，空参数会被过滤；没有引号或转义语义。
- 命令名查找会转小写；参数是否大小写敏感由各命令自己决定。
- 大多数命令默认 `DebugOnly = true`。明确非 debug-only 的命令只有 `cloud`、`getlogs`、`log`、`open`。
- `IsNetworked = true` 的命令在多人非 fake multiplayer 环境会包装成 `ConsoleCmdGameAction` 同步执行。
- `DevConsole` 也会注册 mod 中的 `AbstractConsoleCmd` 子类；本文只覆盖原版内置命令。

## Entry 规则

console 里的对象参数通常吃 `ModelId.Entry`，不是显示名、文件名、动态变量名或本地化标题。

`ModelDb.GetEntry(type)` 的规则是：

```text
Entry = StringHelper.Slugify(type.Name)
```

也就是类名转全大写蛇形。如果类名本身带后缀，后缀通常也会进入 Entry。

| 类型 | 类名/对象 | Entry | console 示例 |
| --- | --- | --- | --- |
| Card | `StrikeIronclad` | `STRIKE_IRONCLAD` | `card STRIKE_IRONCLAD` |
| Potion | `StrengthPotion` | `STRENGTH_POTION` | `potion STRENGTH_POTION` |
| Power | `StrengthPower` | `STRENGTH_POWER` | `power STRENGTH_POWER 3 0` |
| Power | `WeakPower` | `WEAK_POWER` | `power WEAK_POWER 2 1` |
| Relic | `Akabeko` | `AKABEKO` | `relic AKABEKO` |
| Affliction | `Bound` | `BOUND` | `afflict BOUND 1 0` |
| Enchantment | `Adroit` | `ADROIT` | `enchant ADROIT 1 0` |
| Encounter | `AxebotsNormal` | `AXEBOTS_NORMAL` | `fight AXEBOTS_NORMAL` |
| Monster | `Axebot` | `AXEBOT` | `unlock monsters AXEBOT` |
| Event | `AbyssalBaths` | `ABYSSAL_BATHS` | `event ABYSSAL_BATHS` |
| Ancient | `Neow` | `NEOW` | `ancient NEOW` |
| Act | `Glory` | `GLORY` | `act GLORY` |

## Entry 参数矩阵

| 命令参数 | Entry 来源 | 匹配方式 | 示例 |
| --- | --- | --- | --- |
| `card <card_entry>` | `ModelDb.AllCards[].Id.Entry` | 转大写后精确匹配 | `card STRIKE_IRONCLAD` |
| `remove_card <card_entry>` | `ModelDb.AllCards[].Id.Entry` | 转大写后精确匹配 | `remove_card STRIKE_IRONCLAD Hand` |
| `potion <potion_entry>` | `ModelDb.AllPotions[].Id.Entry` | 转大写后精确匹配 | `potion STRENGTH_POTION` |
| `power <power_entry>` | 所有 `PowerModel` 子类的 `Id.Entry` | 转大写后精确匹配 | `power STRENGTH_POWER 3 0` |
| `relic <relic_entry>` | `ModelDb.AllRelics[].Id.Entry` | 包含匹配，全等优先 | `relic AKABEKO` |
| `afflict <affliction_entry>` | `AfflictionModel.Id.Entry` | 构造 `AFFLICTION.<ENTRY>` 精确查询 | `afflict BOUND 1 0` |
| `enchant <enchantment_entry>` | `EnchantmentModel.Id.Entry` | 构造 `ENCHANTMENT.<ENTRY>` 精确查询 | `enchant ADROIT 1 0` |
| `event <event_entry>` | `ModelDb.AllEvents + ModelDb.AllAncients` | 转大写后精确匹配 | `event ABYSSAL_BATHS` |
| `ancient <ancient_entry>` | `ModelDb.AllAncients[].Id.Entry` | 构造 `EVENT.<ENTRY>`，且必须是 ancient | `ancient NEOW` |
| `fight <encounter_entry>` | `EncounterModel.Id.Entry` | 构造 `ENCOUNTER.<ENTRY>` 精确查询 | `fight AXEBOTS_NORMAL` |
| `act <act_entry>` | `ModelDb.Acts[].Id.Entry` | 非数字时转大写后精确匹配 | `act GLORY` |
| `unlock cards <entries...>` | `ModelDb.AllCards[].Id.Entry` | 每个 entry 转大写后精确验证 | `unlock cards STRIKE_IRONCLAD` |
| `unlock potions <entries...>` | `ModelDb.AllPotions[].Id.Entry` | 每个 entry 转大写后精确验证 | `unlock potions STRENGTH_POTION` |
| `unlock relics <entries...>` | `ModelDb.AllRelics[].Id.Entry` | 精确验证，不走 `relic` 的模糊匹配 | `unlock relics AKABEKO` |
| `unlock monsters <entries...>` | `ModelDb.Monsters[].Id.Entry` | 每个 entry 转大写后精确验证 | `unlock monsters AXEBOT` |
| `unlock events <entries...>` | `ModelDb.AllEvents[].Id.Entry`，不含 ancients | 每个 entry 转大写后精确验证 | `unlock events ABYSSAL_BATHS` |
| `unlock epochs <epoch_ids...>` | `EpochModel.AllEpochIds` | 每个 epoch id 转大写后精确验证；不是 `ModelId` | `unlock epochs ACT2_B_EPOCH` |

## 非 Entry 参数

| 参数 | 来源 | 示例 |
| --- | --- | --- |
| `achievement <operation> <id>` | `Achievement` 枚举名转 lower snake_case | `achievement check all_cards_upgraded` |
| `room <RoomType>` | `RoomType` 枚举 | `room Shop` |
| `card ... [PileType]` | `PileType` 枚举 | `card STRIKE_IRONCLAD Deck` |
| `remove_card ... [Hand\|Deck]` | `PileType` 枚举，但实际只支持 `Hand`/`Deck` | `remove_card STRIKE_IRONCLAD Deck` |
| `damage` / `block` / `power` target index | `CombatState.Creatures[index]`，`0` 是玩家 | `power STRENGTH_POWER 3 0` |
| `kill` target index | `CombatState.Enemies[index]`，`0` 是第一个敌人 | `kill 0` |
| `heal` optional index | `CombatState.Allies[index]`；省略时治疗玩家 | `heal 10` |
| `leaderboard [name|-]` | leaderboard 名称；`-` 表示默认榜 | `leaderboard upload - 1000` |
| `ancient ... [choice]` | `EventOption.TextKey` 包含匹配 | `ancient NEOW BLESS` |

## 命令总表

| 命令 | 格式 | 场景 | 说明 |
| --- | --- | --- | --- |
| `achievement` | `achievement <unlock\|revoke\|check> [achievement_enum_snake_case]` | 本地 | 成就操作；`check` 必须给 ID。 |
| `act` | `act <act_index\|act_entry>` | 跑团中 | 数字按 act 下标跳转；字符串按 `ActModel.Id.Entry` 替换当前 act。 |
| `afflict` | `afflict <affliction_entry> [amount] [hand_index]` | 战斗中 | 给手牌加 affliction；`amount` 默认 `0`，`hand_index` 默认 `0`。 |
| `ancient` | `ancient <ancient_entry> [choice_key_part]` | 跑团中 | 打开指定 ancient event，可强制选项。 |
| `art` | `art <affliction\|card\|enchantment\|power\|relic>` | 本地 | 列出指定类型缺失美术资源的内容。 |
| `block` | `block <amount> [creature_index]` | 战斗中 | 给玩家或 `Creatures[index]` 加格挡；不支持负数。 |
| `card` | `card <card_entry> [PileType]` | 跑团中 | 创建卡牌到 pile，默认 `Hand`；手牌满 10 张会失败。 |
| `cloud` | `cloud delete` | 本地 | 删除 Steam Cloud 保存文件；第一次确认，第二次执行并退出游戏。 |
| `damage` | `damage <amount> [creature_index]` | 战斗中 | 不给 index 时伤害所有敌人；给 index 时伤害 `Creatures[index]`。 |
| `die` | `die` | 跑团中 | 杀死玩家。 |
| `draw` | `draw [count]` | 跑团中 | 抽牌；省略时默认 `1`。 |
| `dump` | `dump` | 本地 | 把 Model ID 序列化缓存输出到 console 与日志。 |
| `enchant` | `enchant <enchantment_entry> [amount] [hand_index]` | 战斗中 | 给手牌加 enchantment；`amount` 默认 `1`，`hand_index` 默认 `0`。 |
| `energy` | `energy <amount>` | 战斗中 | 给玩家加能量；不支持负数。 |
| `event` | `event <event_entry>` | 跑团中 | 跳到普通 event 或 ancient event。 |
| `fight` | `fight <encounter_entry>` | 跑团中 | 跳到指定 encounter；不是 monster entry。 |
| `getlogs` | `getlogs [test-feedback\|name]` | 本地 | 打包日志、截图和诊断文件到 `BugReport...zip`。 |
| `godmode` | `godmode` | 跑团中 | 开关无敌模式；应用/移除 `Strength`、`Buffer`、`Regen` 各 9999。 |
| `gold` | `gold <amount>` | 跑团中 | 调用 `GainGold(amount)`；代码未禁止负数。 |
| `heal` | `heal <amount> [ally_index]` | 跑团中 | 省略 index 时治疗玩家；给 index 时治疗 `Allies[index]`。 |
| `instant` | `instant` | 本地 | 在 `Fast` 与 `Instant` 快速模式之间切换。 |
| `kill` | `kill [enemy_index\|all]` | 战斗中 | 省略时杀第一个敌人；`all` 杀全部敌人。 |
| `leaderboard` | `leaderboard upload [name\|-] <score>` / `leaderboard random [name\|-] [count]` | 本地 | 上传分数或生成随机榜单项；`random` 默认 100 条。 |
| `log` | `log [log_type] <log_level>` | 本地 | 设置日志级别；非 debug-only。 |
| `multiplayer` | `multiplayer [test]` | 本地 | 打开 multiplayer submenu；`test` 打开 debug test scene。 |
| `open` | `open <logs\|saves\|root\|build-logs\|loc-override>` | 本地 | 用系统文件管理器打开常用目录；非 debug-only。 |
| `potion` | `potion <potion_entry>` | 跑团中 | 添加药水到 belt。 |
| `power` | `power <power_entry> <amount> <creature_index>` | 战斗中 | 给 `Creatures[index]` 添加或修改 power。 |
| `relic` | `relic [add\|remove] <relic_entry>` | 跑团中 | 默认 `add`；按 relic entry 包含匹配。 |
| `remove_card` | `remove_card <card_entry> [Hand\|Deck]` | 跑团中 | 从手牌或牌组移除第一张匹配卡；默认 `Hand`。 |
| `room` | `room <RoomType>` | 跑团中 | 跳到指定房间类型；不是 Entry。 |
| `sentry` | `sentry <status\|test\|message\|exception\|crash> [text\|confirm]` | 本地 | Sentry 测试；`crash confirm` 会触发原生崩溃。 |
| `stars` | `stars <amount>` | 战斗中 | 给玩家加 stars；不支持负数。 |
| `trailer` | `trailer` | 本地 | 开关 trailer mode。 |
| `travel` | `travel` | 跑团中 | 开关地图 debug travel。 |
| `unlock` | `unlock <cards\|potions\|relics\|monsters\|events\|epochs\|ascensions\|all> [entries...]` | 本地 | 标记内容发现/解锁并保存 progress。 |
| `upgrade` | `upgrade [hand_index]` | 跑团中 | 升级手牌指定位置；省略时默认 `0`。 |
| `win` | `win` | 战斗中 | 移除敌人 power 后杀死所有敌人并检查胜利。 |

## Entry 命令细节

### `card` / `remove_card`

```text
card <CardModel.Id.Entry> [PileType]
remove_card <CardModel.Id.Entry> [Hand|Deck]
```

- `card` 可写任意 `PileType`，默认 `Hand`。
- `remove_card` 实际只支持 `Hand` 与 `Deck`，默认 `Hand`。
- 示例：`card STRIKE_IRONCLAD`、`remove_card STRIKE_IRONCLAD Deck`

### `potion`

```text
potion <PotionModel.Id.Entry>
```

- 输入转大写后精确匹配 `ModelDb.AllPotions[].Id.Entry`。
- 示例：`potion STRENGTH_POTION`、`potion ENTROPIC_BREW`

### `power`

```text
power <PowerModel.Id.Entry> <amount> <creature_index>
```

- 必须在战斗中，且至少提供 3 个参数。
- `power_entry` 来自所有 `PowerModel` 子类。
- power 的 Entry 通常保留 `_POWER` 后缀：`STRENGTH_POWER`、`DEXTERITY_POWER`、`VULNERABLE_POWER`。
- 已有同类型且非 instanced 时调用 `ModifyAmount(existingPower, amount)`；否则新建并 `Apply(..., amount)`。
- 示例：`power STRENGTH_POWER 3 0`

### `relic`

```text
relic <RelicModel.Id.Entry>
relic add <RelicModel.Id.Entry>
relic remove <RelicModel.Id.Entry>
```

- 省略 `add/remove` 时默认 `add`。
- 输入按 `Id.Entry.Contains(...)` 搜索，再按全等、前缀、第一个包含的顺序择一。
- 建议始终写完整 Entry，避免模糊命中错误对象。
- 示例：`relic AKABEKO`、`relic remove AKABEKO`

### `afflict` / `enchant`

```text
afflict <AfflictionModel.Id.Entry> [amount] [hand_index]
enchant <EnchantmentModel.Id.Entry> [amount] [hand_index]
```

- `afflict` 构造 `AFFLICTION.<ENTRY>`；`amount` 默认 `0`。
- `enchant` 构造 `ENCHANTMENT.<ENTRY>`；`amount` 默认 `1`。
- 两者的 `hand_index` 默认都是 `0`。
- 示例：`afflict BOUND 1 0`、`enchant ADROIT 1 0`

### `event` / `ancient`

```text
event <EventModel.Id.Entry>
ancient <AncientEventModel.Id.Entry> [choice_key_part]
```

- `event` 的候选是 `ModelDb.AllEvents.Concat(ModelDb.AllAncients)`，所以可写 `event NEOW`。
- `ancient` 构造 `EVENT.<ENTRY>` 后还要求模型是 `AncientEventModel`。
- `ancient` 第二个参数不是 Entry，而是匹配 `EventOption.TextKey` 的片段。
- `unlock events` 只验证 `ModelDb.AllEvents`，不包含 ancients。

### `fight`

```text
fight <EncounterModel.Id.Entry>
```

- `fight` 吃 encounter entry，不吃 monster entry。
- 示例：`fight AXEBOTS_NORMAL`
- 对比：`unlock monsters AXEBOT` 吃的是 monster entry。

### `act`

```text
act <act_index>
act <ActModel.Id.Entry>
```

- 数字参数按当前 run 的 act 下标跳转，1 起算。
- 非数字参数转大写后匹配 `ModelDb.Acts[].Id.Entry`。
- 示例：`act 1`、`act GLORY`

### `unlock`

```text
unlock cards [CardModel.Id.Entry...]
unlock potions [PotionModel.Id.Entry...]
unlock relics [RelicModel.Id.Entry...]
unlock monsters [MonsterModel.Id.Entry...]
unlock events [EventModel.Id.Entry...]
unlock epochs [EpochModel.AllEpochIds...]
unlock ascensions
unlock all
```

- `cards/potions/relics/monsters/events/epochs` 后的参数都会先转大写。
- `cards/potions/relics/monsters/events` 会在各自集合的 Entry 中精确验证。
- `unlock relics` 不走 `relic` 命令的模糊匹配。
- `unlock events` 不包含 ancients。
- `ascensions` 不支持指定 ID；带额外参数会抛 `NotImplementedException`。

## 高风险命令

- `cloud delete`：第二次确认后删除 Steam Cloud 文件并退出游戏。
- `sentry crash confirm`：触发原生崩溃。
- `achievement unlock/revoke`：操作成就状态。
- `unlock ...`：写入 progress save。
- `leaderboard upload/random`：触碰 leaderboard。
- `relic/card/potion/gold/energy/stars/power/...`：修改当前 run 或 combat state。

## 待确认

- 本文没有列出 `LogType`、`LogLevel`、`RoomType`、`PileType`、`Achievement` 的完整枚举值。
- arm64 DLL 已核对；x86_64 DLL 理论上应一致，但未逐条对比。
