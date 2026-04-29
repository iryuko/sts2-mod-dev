# T5 Bug 修复计划

日期：2026-04-26

## 一 本轮范围

本文件记录测试版发布后第一批 T5 反馈的定位结果与修复计划。

本轮先不扩新功能，只处理以下反馈：

1. `Symbol III` 打出后本局永久格挡增加，但 SL 后增加值清零。
2. `Symbol III` 疑似还有 combat 后清零问题。
3. `Shadow of the Past I / II / III` 卡面写 2 场战斗后生效，实机像是 1 场战斗后就生效。
4. ancient 奖励 `Touch of Orobas / 欧罗巴斯之触` 处理 Sakiko starter relic 时变成无效果的头戴。
5. Act 2 boss `Kaiser Crab / 帝皇蟹` 的玩家朝向机制疑似对 Sakiko 不生效。
6. 联机模式下 Sakiko 可能导致队友打不出牌与多人数据不同步。
7. `Perk Up / 抖擞精神` 偶发卡在屏幕中间不能完成打出，且随机无色牌池混入状态、诅咒、多人牌等不应出现的对象。

## 二 直接测试入口：帝皇蟹

原版 `FightConsoleCmd` 已反编译确认：

- 命令名：`fight`
- 参数：`EncounterModel.Id.Entry`
- 处理方式：把输入转成大写后查 `ModelDb.GetById<EncounterModel>()`
- 命令是 networked console command

帝皇蟹 encounter 的内部 Entry 已由本地化与模型确认：

```text
fight KAISER_CRAB_BOSS
```

使用条件：

- 必须在 run 进行中。
- 命令会直接 `EnterRoomDebug(RoomType.Monster, MapPointType.Unassigned, encounterModel)` 跳转到该 encounter。

## 三 Bug 1 / 4：`Symbol III` 永久格挡 SL 或战斗后清零

### 现象

- `Symbol III` 设计为每打出一次，本局游戏格挡永久 `+1`。
- 实机反馈：SL 后增加的格挡清零。
- 另有疑似：战斗结束后也会清零。

### 当前代码

相关文件：

- `src/Cards/TogawasakikoSongCards.cs`
- 类：`SymbolIii`

当前实现要点：

- `CurrentBlock` 与 `IncreasedBlock` 已加 `[SavedProperty]`。
- `OnPlay(...)` 中：
  - 先 `CreatureCmd.GainBlock(...)`
  - 再 `BuffFromPlay(...)`
  - 再给予 `SymbolIIIPower`
- `BuffFromPlay(...)` 只修改当前被打出的这张牌实例。

### 原版最近邻

已反编译原版：

- `MegaCrit.Sts2.Core.Models.Cards.GeneticAlgorithm`

原版关键链路：

- `CurrentBlock` 与 `IncreasedBlock` 都是 `[SavedProperty]`。
- `CurrentBlock` setter 会同步刷新 `DynamicVars.Block.BaseValue`。
- `OnPlay(...)` 中既调用：
  - `BuffFromPlay(intValue)`
  - 也调用 `(DeckVersion as GeneticAlgorithm)?.BuffFromPlay(intValue)`

### 初判根因

当前 `Symbol III` 少了原版 `GeneticAlgorithm` 的关键一步：

- 没有同步 buff `DeckVersion`。

因此打出时只改了战斗内卡牌实例，牌组中的持久版本没有被更新。SL 或战斗结束后，游戏重新从 deck / save 恢复时，永久增值自然丢失。

另一个低风险差异：

- 当前 `CurrentBlock` setter 没有像原版一样同步 `DynamicVars.Block.BaseValue`。
- 虽然 `UpdateBlock()` 后面手动写了 `DynamicVars.Block.BaseValue`，但存档反序列化走 setter 时可能不会触发 `UpdateBlock()`。

### 拟修复

按原版 `GeneticAlgorithm` 派生，不另起链路：

1. 在 `CurrentBlock` setter 中同步：
   - `DynamicVars.Block.BaseValue = _currentBlock`
2. `OnPlay(...)` 中 `BuffFromPlay(...)` 后追加：
   - `(DeckVersion as SymbolIii)?.BuffFromPlay(extra)`
3. 保持 `IncreasedBlock` 为 `[SavedProperty]`。
4. 不在 `AfterCombatEnd(...)` 手动清理或补写，避免再次碰战斗收尾时序。

### 本轮修改

- `CurrentBlock` setter 现已同步刷新 `DynamicVars.Block.BaseValue`。
- `OnPlay(...)` 现已对齐原版 `GeneticAlgorithm`：
  - 先 buff 当前战斗实例
  - 再同步 buff `DeckVersion as SymbolIii`
- 不新增 combat-end 清理逻辑。

### 2026-04-26 二次定位

实机反馈 `Symbol III` 仍不受 `Frail / 脆弱` 影响，且 `Dexterity / 敏捷` 下叠甲量异常。

重新对照原版 `GeneticAlgorithm` 与 `BlockVar` 后确认：

- `GeneticAlgorithm.CurrentBlock` setter 只写 `DynamicVars.Block.BaseValue`。
- `BlockVar.UpdateCardPreview(...)` 会通过 `Hook.ModifyBlock(...)` 统一处理 Frail / Dexterity / power / enchantment 预览。
- 我们上一轮额外写 `PreviewValue / EnchantedValue`，会把这条原版预览链短路。

二次修正：

- 移除 `RefreshDisplayedBlock()`。
- 不再手动写 `PreviewValue / EnchantedValue`。
- `Symbol III` 只维护 `BaseValue`，其余修正交回原版 hook。

### 2026-04-26 三次定位

实机继续反馈 SL 后仍清零。继续对照原版保存链后确认新的高概率根因：

- `CardModel.ToSerializable()` 通过 `SavedProperties.From(this)` 写 `[SavedProperty]`。
- `CardModel.FromSerializable(...)` 通过 `save.Props?.Fill(cardModel)` 回填。
- `SavedPropertiesTypeCache` 默认只在静态初始化时缓存原版 `AbstractModelSubtypes`。
- mod 自定义卡牌类型即使字段标了 `[SavedProperty]`，如果未显式注入 `SavedPropertiesTypeCache`，保存链也可能拿不到这些字段。

三次修正：

- 在 `TogawasakikoMod.Initialize()` 中调用 `SavedPropertiesTypeCache.InjectTypeIntoCache(...)`。
- 显式注入：
  - `SymbolIii`
  - `ShadowOfThePastI`
  - `ShadowOfThePastII`
  - `ShadowOfThePastIII`
- 这一步同时覆盖 `Symbol III` 的 `CurrentBlock / IncreasedBlock` 与 `Shadow` 的 `CombatsSeen`。

### 复测项

- 战斗中打出 `Symbol III` 一次，当前牌与牌组中该牌显示都应 `+1`。
- 保存退出重进后，增值不应丢失。
- 结束战斗进入下一场后，增值不应丢失。
- 升级版基础值仍应为 `7 + IncreasedBlock`。
- 角色有 `Frail / 脆弱` 时，实际获得格挡与预览都应按原版降低。
- 角色有正/负 `Dexterity / 敏捷` 时，实际获得格挡与预览都应按原版修正，永久 `+1` 增长不应被敏捷污染。

复测状态：

- 2026-04-27 实机复测通过，`Symbol III` 标记完成。

## 四 Bug 2：`Shadow of the Past` 一场战斗后就触发

### 现象

- 卡面写：完成 2 场战斗后触发。
- 实机反馈：实际 1 场战斗后就触发。

### 当前代码

相关文件：

- `src/Cards/TogawasakikoCards.cs`
- 抽象类：`ShadowOfThePastCard`

当前实现要点：

- `MaxCombats = 2`
- `CombatsSeen` 是普通 public property，没有 `[SavedProperty]`。
- `AfterCombatEnd(...)` 会调用 `ResolveTrackedDeckCard(Owner)` 找 deck 中同 ID 的卡，然后给 tracked deck card `CombatsSeen++`。

### 原版最近邻

已反编译原版：

- `MegaCrit.Sts2.Core.Models.Cards.Guilty`

原版关键链路：

- `CombatsSeen` 是 `[SavedProperty]`。
- `CombatsSeen` setter 会同步描述变量：
  - `DynamicVars["Combats"].BaseValue = 5 - CombatsSeen`
- `AfterCombatEnd(...)` 只在当前实例满足以下条件时计数：
  - `base.Pile != null`
  - `base.Pile.Type == PileType.Deck`
- 原版不会让战斗内复制件去回写 deck 中同 ID 的另一张牌。

### 初判根因

当前实现偏离 `Guilty` 的点会导致双重风险：

1. 没有 `[SavedProperty]`，跨 SL / 存档状态不可靠。
2. `ResolveTrackedDeckCard(...)` 可能让多个实例在同一次 `AfterCombatEnd` 中都回写同一张 deck card。

第二点能解释“一场战斗后就触发”：

- 如果 deck 实例和 combat/clone 实例都收到 `AfterCombatEnd(...)`
- 它们都会 resolve 到同一张 deck card
- 同一场战斗内 `CombatsSeen` 被加两次
- `MaxCombats = 2` 于是第一场战斗后直接触发

### 拟修复

严格回到原版 `Guilty` 路线：

1. 给 `CombatsSeen` 加 `[SavedProperty]`。
2. 加 `DynamicVar("Combats", MaxCombats)`，并在 setter 里维护剩余场次显示。
3. `AfterCombatEnd(...)` 只允许 `PileType.Deck` 的当前实例计数。
4. 删除或停用 `ResolveTrackedDeckCard(...)` 的跨实例回写逻辑。
5. 保持三个 reward 分支不变：
   - I：最大生命 `+7`
   - II：移除 Sakiko starter strike / defend
   - III：升级 starter relic

### 本轮修改

- `CombatsSeen` 现已加 `[SavedProperty]`。
- 新增 `Combats` 动态变量，setter 会按 `MaxCombats - CombatsSeen` 刷新剩余场次数。
- `AfterCombatEnd(...)` 现只允许 `PileType.Deck` 的当前实例计数。
- 已移除跨实例 `ResolveTrackedDeckCard(...)` 回写逻辑，避免同一场战斗被多个实例重复计数。
- 三个 `Shadow` 的实际奖励分支保持不变。

### 复测项

- 获得任意 `Shadow` 后打一场战斗：不应触发，只更新剩余场次。
- 第二场战斗后才触发。
- SL 后剩余场次不应重置。
- 同时持有两张同类 `Shadow` 时，不应互相错绑或同 ID 串计数。

## 五 Bug 3：`Touch of Orobas` 把 Sakiko starter relic 变成无效果头戴

### 现象

- ancient 奖励 `Touch of Orobas / 欧罗巴斯之触` 会升级 starter relic。
- Sakiko 的 starter relic 是 `DollMask`。
- 实机反馈：选择后变成无效果的头戴。

### 当前代码

相关文件：

- `src/Relics/DollMask.cs`
- `src/Relics/PianoOfMom.cs`
- `src/Characters/Togawasakiko.cs`
- `src/ModSupport.cs`

当前 Sakiko 自己的升级链：

- `ShadowOfThePastIII.ResolveShadowReward(...)`
- 调 `ModSupport.TryUpgradeDollMask(...)`
- 用 `RelicCmd.Replace(DollMask, UpgradedDollMask)`

### 原版最近邻

已反编译原版：

- `MegaCrit.Sts2.Core.Models.Relics.TouchOfOrobas`

原版关键链路：

- 内置表 `RefinementUpgrades` 只包含 5 个 starter relic：
  - `BurningBlood -> BlackBlood`
  - `RingOfTheSnake -> RingOfTheDrake`
  - `DivineRight -> DivineDestiny`
  - `BoundPhylactery -> PhylacteryUnbound`
  - `CrackedCore -> InfusedCore`
- `GetUpgradedStarterRelic(starterRelic)` 如果查不到映射，会返回：
  - `ModelDb.Relic<Circlet>().ToMutable()`

### 初判根因

用户怀疑基本成立。

`DollMask` 不在原版 `TouchOfOrobas.RefinementUpgrades` 字典中，所以原版 fallback 到 `Circlet`。这不是 Sakiko 自己 `TryUpgradeDollMask(...)` 的问题，而是原版 ancient relic 的内置角色 starter relic 映射缺口。

### 拟修复

对齐原版入口，不替换整条流程：

1. 新增 Harmony patch 到 `TouchOfOrobas.GetUpgradedStarterRelic(RelicModel starterRelic)`。
2. 当 `starterRelic is DollMask` 时，postfix 返回 Sakiko 专属 Orobas relic。
3. 让原版 `SetupForPlayer(...)`、hover tip、`AfterObtained(...)` 继续走原版链路。

### 本轮修改

- 新增 `src/Patches/TouchOfOrobasPatches.cs`。
- 仅 patch `TouchOfOrobas.GetUpgradedStarterRelic(...)` 的 Sakiko starter relic 分支。
- 初版曾把返回 relic 改为 `UpgradedDollMask`。
- 二次设计修正后，当前返回 `PianoOfMom / 妈妈的钢琴`，对齐 Orobas 的 starter refinement 定位。
- `PianoOfMom` 每场战斗开始时，将 1 张随机升级歌曲牌加入手牌，不写入 deck。
- 不改原版 `RefinementUpgrades` 字典，不替换 `SetupForPlayer(...) / AfterObtained(...)`，避免影响五个原版角色。

### 复测项

- 获得 `Touch of Orobas` 时 hover / 描述应显示 `DollMask -> Piano of Mom / 妈妈的钢琴`。
- 选择后 relic 栏中 `DollMask` 被替换为 `PianoOfMom`。
- 不应再出现 `Circlet`。
- 每场战斗开始时，手牌获得 1 张随机升级歌曲牌。
- 获得的歌曲牌不进入 deck，战斗结束后不保留。
- 原版 5 个角色 starter relic 映射不受影响。

复测状态：

- 2026-04-27 实机复测通过，`Touch of Orobas -> PianoOfMom` 标记完成。

## 六 Bug 5：帝皇蟹朝向机制疑似对 Sakiko 不生效

### 现象

- Act 2 boss 帝皇蟹有玩家面向左右蟹钳的机制。
- 反馈称 Sakiko 可能不能改变朝向。

### 当前代码 / 资源

相关文件：

- `pack/scenes/creature_visuals/togawasakiko.tscn`

当前 combat visual：

- 根节点挂 `NCreatureVisuals`
- 有 `%Visuals` 节点
- `Sprite2D` 在 `Visuals` 下

### 原版最近邻

已反编译：

- `KaiserCrabBoss`
- `Crusher`
- `Rocket`
- `SurroundedPower`
- `NCreature`
- `NCreatureVisuals`

原版机制：

- `Crusher.AfterAddedToRoom()` 给自己加 `BackAttackLeftPower`。
- `Rocket.AfterAddedToRoom()`：
  - 给玩家侧施加 `SurroundedPower`
  - 给自己加 `BackAttackRightPower`
- `SurroundedPower.BeforeCardPlayed(...)`：
  - 玩家打出牌且有 target 时，根据 target 是否有 `BackAttackLeftPower / BackAttackRightPower` 调整 `Facing`
  - 然后通过 `NCombatRoom.Instance.GetCreatureNode(c)?.Body` 取玩家 body
  - 直接翻转 `body.Scale.X`

### 初判

目前看 Sakiko scene 具备 `%Visuals`，理论上 `NCreatureVisuals.GetCurrentBody()` 能返回该节点，`SurroundedPower` 应该可以翻转它的 scale。

因此这里先列为待实机确认，不提前写成已定位。

高可能排查方向：

1. 实机观察打左钳 / 右钳时角色 sprite 是否翻转。
2. 如果不翻转，查 `NCombatRoom.Instance.GetCreatureNode(playerCreature)?.Body` 是否为空。
3. 如果 Body 不为空但视觉不变，查 static sprite 的 nested scale / parent scale 是否被其它代码或 scene 初始值抵消。
4. 如果视觉翻转但机制没变，查 `SurroundedPower.Facing` 是否正确改变，以及伤害乘区是否命中。

### 复测入口

```text
fight KAISER_CRAB_BOSS
```

建议复测：

- 打左侧 `Crusher` 后观察角色朝向。
- 打右侧 `Rocket` 后观察角色朝向。
- 分别吃背击，确认 `SurroundedPower` 伤害乘区是否按朝向变化。

### 2026-04-27 复查结论

本轮重新对照原版 `SurroundedPower` 与 `NCreatureVisuals` 后确认：

- `NCreatureVisuals._Ready()` 会通过 `%Visuals` 设置 `Body`。
- Sakiko 当前 scene 中存在唯一名 `%Visuals`，因此 `Body` 应能被原版正常解析。
- `SurroundedPower.FaceDirection(...)` 会先更新自身 `Facing`，再调用 `FlipScale(Body)` 做视觉翻转。
- `SurroundedPower.ModifyDamageMultiplicative(...)` 使用的是 `Facing` 字段判断背击乘区，不依赖视觉 scale。
- `FlipScale(...)` 只在需要时把 `Body.Scale.X` 乘以 `-1`。Sakiko `%Visuals` 初始 scale 为 `Vector2(0.22, 0.22)`，理论上会在 `0.22 / -0.22` 之间切换。

因此当前判断：

- 不支持“Windows 系统差异导致原版朝向机制整体失效”这一强结论。
- 更可能的情况是：
  1. Windows 玩家只观察到静态立绘视觉不明显或未刷新，但 `Facing` 与背击伤害其实正常。
  2. Windows/Godot 对静态 `Sprite2D` 父节点负 scale 的显示存在局部表现差异，但这只会影响视觉，不一定影响战斗机制。
  3. 玩家使用旧包或 scene 未更新，导致实际 `%Visuals` 结构与当前包不同。

下一步 Windows 复测必须区分两件事：

- 视觉是否翻转。
- 背击伤害是否按 `SurroundedPower` 的 `1.5x` 乘区变化。

如果视觉不翻但伤害正常，优先按 visual 层问题处理；如果伤害也不变，再查 `SurroundedPower` 是否施加、`Facing` 是否更新、目标是否带 `BackAttackLeftPower / BackAttackRightPower`。

2026-04-27 实机反馈：

- 背击伤害乘区正常。
- 因此帝皇蟹朝向机制层判定正常，不作为战斗逻辑 bug 继续处理。
- 若后续仍有反馈，只按视觉翻转不明显 / 平台视觉差异 / 旧包差异继续排查。

## 七 Bug 6：联机模式卡队友出牌 / 多人数据不同步

### 现象

- 联机模式选用 Sakiko 时，可能导致队友打不出牌。
- 也可能出现多人数据不同步。

### 当前已查风险点

相关文件：

- `src/Entry.cs`
- `src/ModSupport.cs`
- `src/Cards/*.cs`
- `src/Powers/*.cs`
- `src/Events/UnattendedPiano.cs`
- `src/Jukebox/*`

已看到的风险类型：

1. `Entry.OnRoomEntered()` 使用 `_ = ApplyCombatWatcherAsync(...)` 异步挂 watcher。
   - 这不是通过玩家选择 action 显式排队。
   - 在联机里可能造成不同客户端应用 hook 的时点不同。
2. 多处 helper 在非战斗 / hook 场景构造 `PlayerChoiceContext`。
   - 已有 `CreateHookChoiceContext(...)` 对多人场景做防守。
   - 但仍需逐个检查哪些地方 fallback 到 detached context。
3. 随机目标 / 随机生成：
   - `GetRandomEnemy(...)` 已在多人且 RNG 不可解析时拒绝 fallback。
   - 仍需检查所有随机抽卡、随机 song、随机 pressure token 是否全部走 run/player RNG，而不是本地 LINQ 或本地随机。
4. UI 注入与 jukebox：
   - `Jukebox` 是本地 UI / AudioStreamPlayer。
   - 只要不改 combat model，理论上不应同步。
   - 但必须确认它没有在联机房间里改 run/combat state。
5. 自动打牌 / 自动加牌 / hook 内伤害：
   - `CardCmd.AutoPlay`
   - `CardPileCmd.AddGeneratedCardToCombat`
   - `CreatureCmd.Damage`
   - `PowerCmd.Apply / Remove / ModifyAmount`
   这些在联机中必须由正确的 networked action / hook context 驱动。

### 原版最近邻

已反编译：

- `NetFullCombatState`
- `NetPlayCardAction`
- `FightConsoleCmd`
- `SurroundedPower`

关键结论：

- 联机同步会比对 creature、power、combat piles、relic、rng、choice id、last executed action/hook。
- 自定义字段如果只存在于卡牌 `[SavedProperty]` 中，通常会随 `SerializableCard` 进入同步。
- 但脱离 action queue 的异步 command、不同客户端各自执行的本地回调、或者本地随机，会高概率导致 state diverged。

### 初判优先排查对象

优先查这些高风险点：

1. `ApplyCombatWatcherAsync(...)`
   - 是否应改为更贴近原版的 start-of-combat hook / relic/power 入口，而不是 `_ =` fire-and-forget。
2. `TogawasakikoCombatWatcherPower`
   - 所有 hook 是否会对非本地玩家、队友、敌人执行非确定性操作。
3. 生成牌 helper：
   - `GiveRandomSongCardToPlayer`
   - `GiveRandomPressureGeneratedCardToPlayer`
   - `GiveGeneratedCardToPlayer`
4. 自动打牌对象：
   - `AveMujicaPower`
   - `MagneticForceHellWargodPower`
   - `IHaveAscended` 等自动生成 / 自动打出路径
5. 事件房：
   - `UnattendedPiano` 在多人下是否应限制 owner / local player，或使用原版 multiplayer event context。

### 拟处理顺序

联机问题先不直接大改，先分三步：

1. 增加或整理日志定位点：
   - action/hook source
   - player NetId
   - combat Players.Count
   - 是否使用 detached context
   - 是否拒绝多人 fallback
2. 找一张最小复现牌：
   - 优先从“生成牌入手”和“回合 hook 自动动作”两类切。
3. 每修一个联机点，都必须做单人回归，避免为了联机改坏单人闭环。

### 2026-04-27 定位与首轮修正

本轮继续按文档中列出的联机风险点排查，确认了两条代码级高风险偏离：

1. `TogawasakikoCombatWatcherPower` 旧安装路径不是只存在“异步 fire-and-forget 时序”问题，还存在更直接的多人状态分歧风险。
   - 旧实现位于 `Entry.OnRoomEntered()`：
     - 先通过 `GetLocalPlayer(runState)` 解析本地玩家。
     - 只在本地玩家是 `Togawasakiko` 时执行 `_ = ApplyCombatWatcherAsync(...)`。
   - 这意味着多人局中：
     - Sakiko 玩家客户端会给 Sakiko 挂 watcher。
     - 队友客户端的 `LocalContext.GetMe()` 是队友，不会给 Sakiko 挂 watcher。
   - 结果是不同客户端从战斗开始就可能拥有不同的 creature power 列表，符合 `NetFullCombatState` 对 power / combat state divergence 的风险模型。
2. 共享随机 combat 生牌 helper 旧实现使用 `recipient.PlayerRng.Rewards`。
   - 受影响对象包括：
     - `GiveRandomSongCardToPlayer(...)`
     - `GiveRandomPressureGeneratedCardToPlayer(...)`
     - `GiveGeneratedCardToPlayer<T>()` 的上层随机入口
     - `PianoOfMom` 使用的随机升级 song 入口
   - 已对照原版 `ManifestAuthority` / `SpectrumShiftPower`：
     - 原版 combat 内随机生成卡牌使用 `CardFactory.GetDistinctForCombat(...)`
     - RNG 使用 `player.RunState.Rng.CombatCardGeneration`
     - 并通过 `CardFactory` 过滤多人约束。
   - 因此继续用 reward RNG 生成战斗内随机牌，属于“能跑但偏离原版 combat generation 链”的同步风险。

本轮修改：

- 移除 `Entry.OnRoomEntered()` 中旧的本地玩家异步 watcher 安装逻辑。
- 新增 `TogawasakikoCombatWatcherInstaller : AbstractModel`。
- 在 `TogawasakikoMod.Initialize()` 中通过 `ModHelper.SubscribeForCombatStateHooks(...)` 注册 combat hook subscriber。
- 新 installer 在原版 `BeforeCombatStart` hook 链中：
  - 遍历 `combatState.Players`
  - 对每个 `player.Character is Togawasakiko` 的玩家同步检查 / 安装 `TogawasakikoCombatWatcherPower`
  - 对已存在 watcher 执行 `ResetCombatState()`
  - 同步执行 `ClearPersistedTwoMoonsCostModifiers(player)`
- 这样 watcher 安装不再依赖本地玩家身份，也不再是脱离 hook 链的 `_ =` 异步任务。
- `CreateRandomCardFromCanonicalPool(...)` 改为使用：
  - `CardFactory.GetDistinctForCombat(...)`
  - `recipient.RunState.Rng.CombatCardGeneration`
  - 原版 `CardFactory` 的 combat / multiplayer constraint 过滤链。

当前验证状态：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- 尚未做多人实机复测。

### 2026-04-28 启动崩溃修正

封装给测试者后，启动游戏直接崩溃。最新 `godot.log` 明确报错：

- `System.MissingMethodException: Cannot dynamically create an instance of type 'Togawasakiko_in_Slay_the_Spire.TogawasakikoCombatWatcherInstaller'. Reason: No parameterless constructor defined.`
- 抛点位于 `MegaCrit.Sts2.Core.Models.ModelDb.Init()`。

根因：

- `TogawasakikoCombatWatcherInstaller` 继承了 `AbstractModel`。
- 原版 `ModelDb.Init()` 会扫描并动态创建所有 `AbstractModel` 子类。
- 该 helper 初版只有 `CombatState` 构造参数，没有无参构造，因此启动期模型初始化直接失败。

修正：

- 为 `TogawasakikoCombatWatcherInstaller` 补无参构造。
- `_combatState` 改为可空字段。
- `BeforeCombatStart()` 在 `_combatState == null` 时直接返回，保证被 `ModelDb` 创建的 canonical 空实例不会执行逻辑。
- combat hook subscriber 仍返回带 `CombatState` 的实例，保留联机 watcher 安装修复。

验证：

- `dotnet build ... -c Release` 通过，`0 warning / 0 error`。
- 已重新 release build、安装并覆盖测试 zip。
- 本次问题属于启动期动态模型扫描约束，后续新增任何 `AbstractModel` 子类都必须保留无参构造，或避免把纯 helper 设计成 `AbstractModel` 子类。

下一步复测建议：

- 双人局中由 Sakiko 玩家和队友分别进同一场战斗，确认双方日志都出现同一个 Sakiko player NetId 的 watcher 安装记录。
- Sakiko 使用能触发压力兑换的牌，确认队友客户端也能看到同样的生成牌 / power 变化。
- 重点复测：
  - `Compose`
  - `AveMujica`
  - `RestorationOfPower`
  - `PianoOfMom`

### 2026-04-28 压力 ownership 语义冻结

本轮补充确认“双 Sakiko 联机时压力如何计算”的设计语义。

当前实现不是每个 Sakiko 单独维护压力池，而是沿用原版 power 叠层模型：

- `PressurePower` 是目标身上的 `Counter` debuff。
- `ModSupport.ApplyPressure(...)` 直接对目标 `PowerCmd.Apply<PressurePower>(...)`。
- `ModSupport.GetPressure(...)` 与 `TryConsumePressure(...)` 都只读取 / 修改目标当前唯一压力总量。
- 代码没有保存每层压力的施加者，也没有 per-player pressure map。

因此冻结为：

- 多个 Sakiko 对同一个敌人施加压力时，叠加到同一个共享压力池。
- 任意 Sakiko 的牌都按目标当前总压力读取与消耗。
- 一个 Sakiko 可以消耗另一个 Sakiko 叠出的压力。
- 这是当前首版的正式设计，不作为 bug 修。

后续注意：

- 牌面文本避免写成“你的压力”，应写成“目标的压力 / 敌人的压力”。
- 联机复测仍需确认压力兑换生成牌在双方客户端同步一致。
- 若未来设计要求“每名 Sakiko 独立压力”，需要重构 `ApplyPressure / GetPressure / TryConsumePressure` 以及所有压力读取、消耗和 UI 展示路径。

### 2026-04-28 联机首战黑屏修正

封装启动崩溃修正后继续双人联机实测，进入第一个 combat 时黑屏，不能完成战斗房间加载。

日志：

- 地图投票和进房 action 已正常同步。
- `CombatRoom.StartCombat(...)` 中已进入 `CombatManager.SetUpCombat(...)`。
- 在初始洗牌链路 `Hook.ModifyShuffleOrder(...)` 中抛出：
  - `DuplicateModelException: Trying to create a duplicate canonical model of type Togawasakiko_in_Slay_the_Spire.TogawasakikoCombatWatcherInstaller. Don't call constructors on models! Use ModelDb instead.`

根因：

- 上一版 `TogawasakikoCombatWatcherInstaller : AbstractModel` 虽然补了无参构造，解决了启动期 `ModelDb.Init()` 崩溃，但仍然存在更根本的问题：
  - `AbstractModel` 子类会被 `ModelDb` 视为 canonical model 管理。
  - combat hook subscriber 每次 `new TogawasakikoCombatWatcherInstaller(combatState)` 都是在直接构造一个新的 canonical model。
  - 原版在初始洗牌时就会枚举 combat hook listeners，不是等到 `BeforeCombatStart` 才创建 subscriber。
- 因此该 helper 不能设计为 `AbstractModel` 子类，也不能通过每场战斗 `new` 的方式进入 hook listener 链。

修正：

- 移除 `TogawasakikoCombatWatcherInstaller : AbstractModel`。
- 移除 `ModHelper.SubscribeForCombatStateHooks(...)` 安装路径。
- 保留上一轮已经确认必要的两点联机方向：
  - 不再在 `Entry.OnRoomEntered()` 里按本地玩家身份异步安装 watcher。
  - 战斗内随机生成牌继续走 `CardFactory.GetDistinctForCombat(...)` 与 `RunState.Rng.CombatCardGeneration`。
- 新增 `CombatWatcherPatches`：
  - Harmony postfix 原版 `CombatManager.SetUpCombat(CombatState state)`。
  - 在原版完成 `Player.ResetCombatState()`、`Player.PopulateCombatState(...)`、`NetCombatCardDb.StartCombat(...)`、`AddCreature(...)` 之后，遍历 `state.Players`。
  - 对所有 `player.Character is Togawasakiko` 的玩家同步安装 / 重置隐藏 `TogawasakikoCombatWatcherPower`。
  - 安装使用 `ModelDb.Power<TogawasakikoCombatWatcherPower>().ToMutable(1)` 加 `ApplyInternal(..., silent: true)`，避免在同步的 `SetUpCombat` postfix 内启动异步 command。

当前验证状态：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过。
- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已覆盖安装。
- 已重新封装：
  - `local/releases/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire-0.2.0-test-t5-mpfix-2026-04-28.zip`
  - zip sha256：`726a40897421d71c02d518c40accc6c4c7d0e580ab0ba6802107b7fba158f24f`
  - release / installed dll sha256：`0b46b1ae0b40dbcafadccb81fd4a6d2ba5b5a977d90c91f73f132e0d6fb736d8`

待复测：

- 双人联机进入第一场小怪，不应再黑屏卡在 combat loading。
- 双方日志都应出现同一个 Sakiko player NetId 的 `Installed combat watcher during SetUpCombat` 记录。
- 复测 `Symbol` 回合结束随机伤害，确认直接 `ApplyInternal` 安装 watcher 后仍能进入原有 hook。
- 若仍出现“队友出牌被卡住”，下一轮从 hook 执行范围继续查 `TogawasakikoCombatWatcherPower` 的出牌后逻辑，而不是再改 watcher 安装生命周期。
- 若仍出现卡队友出牌，再继续排查 `CardCmd.AutoPlay` 和 hook 内 `PlayerChoiceContext`。

### 2026-04-28 压力兑换不触发修正

继续联机实测后确认：

- 战斗可以正常进入并完成流程。
- 但 Sakiko 自己或队友造成的 debuff 都不会消耗目标压力生成压力衍生牌。
- `Speak / 说话！` 仍能正常生成人格解离。

定位：

- `Speak / 说话！` 是卡牌 `OnPlay(...)` 里直接调用 `AddSpecificCardToCombatPile<PersonaDissociation>(...)`，不经过隐藏 watcher。
- 压力兑换依赖 `TogawasakikoCombatWatcherPower.AfterPowerAmountChanged(...)`：
  - 目标获得 `Weak` 且压力足够时，消耗 2 压力，给 Sakiko 生成 `PersonaDissociation`。
  - 目标获得 `Vulnerable` 且压力足够时，消耗 3 压力，给 Sakiko 生成 `AllYouThinkAboutIsYourself`。
  - 目标失去 `Strength / Dexterity` 且压力足够时，消耗 1 压力，给 Sakiko 生成 `SocialWithdrawal`。
- 最新日志显示每场战斗都有：
  - `Failed while installing combat watcher after combat setup: System.NullReferenceException`
  - 抛点在 `CombatWatcherPatches.InstallForPlayer(...)`。

根因：

- 新安装路径中使用了 `ModelDb.Power<TogawasakikoCombatWatcherPower>().ToMutable(1)`。
- 对照原版 `PowerCmd.Apply(...)`，原版是：
  - `powerModel.ToMutable()`
  - 再 `ApplyInternal(target, amount, silent)`
- `PowerModel.ToMutable(initialAmount)` 会在 power 尚未拥有 `Owner` 时设置 `Amount`，从而触发 `Owner.InvokePowerModified(...)` 空引用。
- 因此 watcher 实际没有安装成功，后续所有 `AfterPowerAmountChanged(...)` 都不会触发。

修正：

- `CombatWatcherPatches` 改为：
  - `ModelDb.Power<TogawasakikoCombatWatcherPower>().ToMutable()`
  - `watcher.ApplyInternal(creature, 1m, silent: true)`
- 这回到原版 `PowerCmd.Apply(...)` 的模型创建顺序，同时保留同步安装，不回退到 fire-and-forget async command。

当前验证状态：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过。
- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已覆盖安装。
- 已重新封装：
  - `local/releases/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire-0.2.0-test-t5-mpfix-2026-04-28.zip`
  - zip sha256：`de3a72ff197ec19f312ec04bbb6890cff3ebee997734709377a8db74868ab3b3`
  - release / installed dll sha256：`0680eb3b00d9ab640f430abdcfeb689f78c2aba02ec0a325b1417166cae3c19e`

待复测：

- 进第一场 combat 后日志应出现 `Installed combat watcher during SetUpCombat for player=...`，不应再出现 watcher 安装 NRE。
- Sakiko 对有压力的敌人施加 `Weak`，应消耗 2 压力并把 `PersonaDissociation` 放入 Sakiko 手牌。
- 队友对有压力的敌人施加 `Weak`，也应消耗 2 压力并把 `PersonaDissociation` 放入 Sakiko 手牌，而不是队友手牌。
- 同理复测 `Vulnerable` 与降低 `Strength / Dexterity` 的兑换分支。

### 2026-04-28 联机相关原版对齐扫查

压力兑换修复后，按“偏离原版写法优先排查”的准则继续扫查 combat / multiplayer 相关代码。

本轮重点扫查：

- `PlayerChoiceContext` / `HookPlayerChoiceContext` 构造。
- combat 内随机生成牌。
- combat 内生成牌加入手牌 / 弃牌堆。
- 自损与伤害命令。
- 直接 `ApplyInternal / AddInternal / RemoveInternal / ToMutable(initialAmount)` 等绕过原版命令链的用法。

发现与修正：

1. combat 内生成牌的加入链路偏离原版。
   - 原版最近邻：
     - `Discovery`
     - `WhiteNoise`
     - `InfernalBlade`
     - `JackOfAllTrades`
     - `ManifestAuthority`
     - `SpectrumShiftPower`
   - 原版做法：
     - `CardFactory.GetDistinctForCombat(...)`
     - `CardPileCmd.AddGeneratedCardToCombat(..., addedByPlayer: true)`
   - 修正：
     - `GiveGeneratedCardToPlayer<T>()`
     - `AddSpecificCardToCombatPile<T>()`
     - `GiveRandomSongCardToPlayer(...)`
     - `GiveRandomUpgradedSongCardToPlayer(...)`
     - `GiveRandomPressureGeneratedCardToPlayer(...)`
     全部统一走 `AddGeneratedCardToCombat(..., true, ...)`。
   - 目的：
     - 让 combat history、`AfterCardGeneratedForCombat` hook、多人 combat card state 更接近原版生成牌路径。

2. 手动构造 hook choice context 的 action type 过窄。
   - 旧实现：
     - `CreateHookChoiceContext(...)` 固定用 `GameActionType.CombatPlayPhaseOnly`。
   - 风险：
     - 该 helper 会被 `KillKissPower.AfterSideTurnStart(...)` 等非玩家出牌阶段 hook 使用。
     - `CombatPlayPhaseOnly` 可能在敌方回合 / NotPlayPhase 被 action queue 暂停，和原版 hook action 的常见用法不一致。
   - 修正：
     - `CreateHookChoiceContext(...)` 默认改为 `GameActionType.Combat`。
     - 保留参数入口，后续如果某个调用点必须限制到 play phase，可显式传入。

3. 自损卡偏离原版伤害命令链。
   - 旧实现：
     - `WeightliftingChampion` 通过 `ModSupport.LoseHp(...)` 反射调用 `Creature.LoseHpInternal(...)`。
   - 原版最近邻：
     - `Bloodletting`
     - `Offering`
     - `Hemokinesis`
   - 原版做法：
     - `CreatureCmd.Damage(choiceContext, Owner.Creature, HpLoss, DamageProps.cardHpLoss, this)`。
   - 修正：
     - `WeightliftingChampion` 改为走 `CreatureCmd.Damage(...)`。
     - 删除 combat 内不再使用的 `ModSupport.LoseHp(...)` 与 `LoseHpInternal` 反射 helper。

保留但标记为低优先级：

- `UnattendedPiano` 使用 `Owner.PlayerRng.Rewards` 随机 Shadow 奖励，属于事件奖励场景，不是 combat 内随机生成牌，本轮不改。
- `ApplyEventHpLoss(...)` 使用 `ThrowingPlayerChoiceContext` 处理事件房非战斗扣血，当前问题集中在 combat 联机同步，本轮不扩到事件系统。

当前验证状态：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过。
- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已覆盖安装。
- 已重新封装：
  - `local/releases/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire-0.2.0-test-t5-mpfix-2026-04-28.zip`
  - zip sha256：`4dd2ed67278c7025998f66cbd463152415a3fb587019a8fc350d75e0f256b618`
  - release / installed dll sha256：`958726c96564727d96662482c970d899425ba2e9c5b8ba0dc50672109064fc8d`

待复测：

- 压力兑换三分支：
  - `Weak -> PersonaDissociation`
  - `Vulnerable -> AllYouThinkAboutIsYourself`
  - `Strength / Dexterity` 下降 -> `SocialWithdrawal`
- `Speak / 说话！`、`RestorationOfPower`、`Compose`、`PianoOfMom` 这些生成牌入口是否仍正常。
- `KillKiss` 在敌方回合开始造成伤害时是否不再卡 action queue。
- `WeightliftingChampion` 自损、加力量、加敏捷是否仍按原版自损卡流程结算。

## 八 建议修复顺序

1. `Symbol III`
   - 原版最近邻明确，修复范围小。
2. `Shadow of the Past`
   - 原版 `Guilty` 路径明确，且当前实现偏离明显。
3. `Touch of Orobas`
   - 原版 fallback 已确认，适合加窄 patch。
4. 帝皇蟹朝向
   - 先由实机 `fight KAISER_CRAB_BOSS` 验证；若确有问题，再查 scene / Body / scale。
5. 联机同步
   - 最复杂，必须另开小轮次，先做日志与最小复现，不和前 3 个确定性 bug 混修。

## 九 Bug 7：`Perk Up / 抖擞精神` 卡中间与无色池污染

### 现象

- `Perk Up` 偶尔会卡在屏幕中间，表现为牌没有正常完成打出流程。
- 其随机获得的“无色牌”池子过脏，会抽到状态、诅咒、多人模式牌等不应进入常规无色生成链的对象。

### 当前代码

相关文件：

- `src/Cards/TogawasakikoCards.cs`
- `src/ModSupport.cs`

当前 `PerkUp.OnPlay(...)`：

- 调 `ModSupport.GiveRandomColorlessCardToPlayer(Owner)`
- 升级后再抽 `DynamicVars.Cards.BaseValue` 张牌

当前无色池 helper：

- `GetColorlessPoolCanonicals()` 遍历 `ModelDb.AllCardPools`
- 只筛 `pool.IsColorless`
- 再 `SelectMany(pool.AllCards)`

这条链路会把所有标记为 colorless 的 pool 都揉在一起，不等于原版“可生成无色牌池”。

### 原版最近邻

已反编译原版：

- `MegaCrit.Sts2.Core.Models.Cards.ManifestAuthority`
- `MegaCrit.Sts2.Core.Models.Cards.SpectrumShift`
- `MegaCrit.Sts2.Core.Models.Powers.SpectrumShiftPower`
- `MegaCrit.Sts2.Core.Models.CardPools.ColorlessCardPool`

原版关键链路：

- `ManifestAuthority.OnPlay(...)`：
  - `ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)`
  - `CardFactory.GetDistinctForCombat(owner, ..., 1, owner.RunState.Rng.CombatCardGeneration)`
  - `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true)`
- `SpectrumShiftPower.BeforeHandDraw(...)`：
  - 使用同一条 `ColorlessCardPool.GetUnlockedCards(... CardMultiplayerConstraint)` 链
  - 使用 `CardFactory.GetDistinctForCombat(...)`
  - 使用 `CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true)`
- `ColorlessCardPool.GenerateAllCards()` 是原版明确列出的 64 张无色牌，并通过 epoch / unlock 过滤。

### 初判根因

无色池污染的根因已比较明确：

- 当前 helper 用 `ModelDb.AllCardPools.Where(pool.IsColorless)` 过宽。
- 原版生成无色牌不扫全部 colorless pools，而是指定 `ColorlessCardPool` 并走 unlock / multiplayer constraint。

卡在中间的高可能根因：

- 当前使用自建 helper `CreateRandomCardFromCanonicalPool(...) + CardPileCmd.AddGeneratedCardToCombat(... addedByPlayer: false ...)`。
- 这和原版 `ManifestAuthority / SpectrumShiftPower` 的展示 / 生成链不同。
- 原版该类“生成无色牌到手牌”的动作使用 `CardFactory.GetDistinctForCombat(...)` 与 `addedByPlayer: true`。
- `Perk Up` 的卡中间问题可能来自生成牌展示 / action ownership 与当前 play 流程不匹配；应优先回到原版链路，而不是继续补动画绕路。

### 拟修复

按原版 `ManifestAuthority` / `SpectrumShiftPower` 派生：

1. 将 `GiveRandomColorlessCardToPlayer(...)` 改为只使用：
   - `ModelDb.CardPool<ColorlessCardPool>()`
   - `.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)`
   - `CardFactory.GetDistinctForCombat(player, ..., 1, player.RunState.Rng.CombatCardGeneration)`
2. 生成到手牌时对齐原版：
   - `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true)`
3. 不再使用 `ModelDb.AllCardPools.Where(pool.IsColorless)` 作为 `Perk Up` 的候选池。
4. 保留 `Perk Up` 升级后抽 1 张牌的既有逻辑，先不改卡牌设计。
5. 若修复后仍卡中间，再进一步对比 `CardPileCmd.AddGeneratedCardsToCombat` 批量接口与单张接口的表现差异。

### 本轮修改

- `GiveRandomColorlessCardToPlayer(...)` 已改为只使用原版 `ColorlessCardPool`。
- 候选牌现在走 `GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)`。
- 随机生成现在走 `CardFactory.GetDistinctForCombat(..., player.RunState.Rng.CombatCardGeneration)`。
- 入手命令已对齐原版无色生成牌，使用 `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true)`。

### 复测项

- 连续多次打出 `Perk Up`，生成牌应正常入手，`Perk Up` 不应停在屏幕中间。
- 生成结果应只来自原版 `ColorlessCardPool` 的解锁且符合多人约束的牌。
- 不应再生成状态牌、诅咒牌、多人专用牌或其它非标准无色对象。
- 升级版仍应在生成无色牌后抽 1 张牌。
- 单人和多人都要复测，因为原版链路显式使用 `CardMultiplayerConstraint`。

复测状态：

- 2026-04-27 实机复测通过，`Perk Up` 标记完成。
