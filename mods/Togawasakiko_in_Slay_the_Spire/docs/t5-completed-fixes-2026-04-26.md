# T5 已完成修复记录

日期：2026-04-26

本文件用于后续 git / release 公示，记录已经完成代码修复并通过本地构建的 bug。

## 1. `Symbol III` 永久格挡 SL / 跨战斗丢失

### 现象

- `Symbol III` 每打出一次，本局游戏格挡应永久 `+1`。
- 实机反馈：SL 后增加值清零。
- 另有疑似：战斗结束后也会清零。

### 原版参考

- 原版 Defect `GeneticAlgorithm`。
- 原版打出后不只 buff 当前战斗实例，还会同步 buff `DeckVersion`。

### 修复

- `SymbolIii.CurrentBlock` setter 现在同步刷新 `DynamicVars.Block.BaseValue`。
- `SymbolIii.OnPlay(...)` 现在对齐 `GeneticAlgorithm`：
  - buff 当前战斗实例
  - 同步 buff `DeckVersion as SymbolIii`
- 不新增 combat-end 清理逻辑，避免污染战斗收尾时序。

### 2026-04-26 二次修正

实机继续反馈：

- `Symbol III` 不受 `Frail / 脆弱` 影响。
- 身上有 `Dexterity / 敏捷` 时，显示或结算出的叠甲量仍异常。

二次对照原版 `GeneticAlgorithm` 后确认，前一轮仍有一处偏离：

- 原版 `CurrentBlock` setter 只写 `DynamicVars.Block.BaseValue`。
- 我们额外手动写了 `PreviewValue / EnchantedValue`。
- 这会绕开 `BlockVar.UpdateCardPreview(...)` 中由原版 hook 统一计算的预览链，导致 Frail / Dexterity 相关显示和实际结算出现偏离。

本次已移除 `RefreshDisplayedBlock()` 以及所有手动写 `PreviewValue / EnchantedValue` 的逻辑。`Symbol III` 现在只像原版 `GeneticAlgorithm` 一样维护 `BaseValue`，把 Frail / Dexterity / enchantment / power 修正交回原版 `BlockVar` 和 `Hook.ModifyBlock(...)`。

### 2026-04-26 三次修正

实机继续反馈：

- `Symbol III` 在 SL 后仍会丢失本局永久增加的格挡。

进一步反编译原版保存链确认：

- `CardModel.ToSerializable()` 通过 `SavedProperties.From(this)` 写入 `[SavedProperty]`。
- `CardModel.FromSerializable(...)` 通过 `save.Props?.Fill(cardModel)` 回填属性。
- `SavedPropertiesTypeCache` 默认只缓存原版 `AbstractModelSubtypes` 中的类型。
- mod 自定义卡牌类型如果没有显式注入 `SavedPropertiesTypeCache`，即使字段标了 `[SavedProperty]`，也可能不会被 `SavedProperties.From(...)` 收集。

本次已在 `TogawasakikoMod.Initialize()` 中显式注册带保存字段的 mod 卡牌类型：

- `SymbolIii`
- `ShadowOfThePastI`
- `ShadowOfThePastII`
- `ShadowOfThePastIII`

这次修复的目标是让 `SymbolIii.CurrentBlock / IncreasedBlock` 以及 `ShadowOfThePastCard.CombatsSeen` 真正进入原版 SL / 存档序列化链。旧存档中已经丢失的增量无法恢复，但新打出的增量应从本版本开始保留。

### 验证状态

- `dotnet build -c Release` 通过，`0 warning / 0 error`。
- 已执行 release build 与安装。
- release/install 的 `dll / pck / mod_manifest.json` 哈希已核对一致。
- 2026-04-27 实机复测通过，标记完成。
- 已确认 `Symbol III` 的 SL / 跨战斗保留问题已解决。

## 2. `Shadow of the Past I / II / III` 一场战斗后提前触发

### 现象

- 卡面写完成 `2` 场战斗后触发。
- 实机反馈：实际像是 `1` 场战斗后就触发。

### 原版参考

- 原版 curse `Guilty`。
- 原版只允许牌组中的当前实例在 `AfterCombatEnd(...)` 计数。
- 原版 `CombatsSeen` 是 `[SavedProperty]`，并同步剩余场次数动态变量。

### 修复

- `ShadowOfThePastCard.CombatsSeen` 加 `[SavedProperty]`。
- 新增 `Combats` 动态变量，按 `MaxCombats - CombatsSeen` 显示剩余场次。
- `AfterCombatEnd(...)` 现在只在 `PileType.Deck` 的当前实例上计数。
- 移除跨实例 `ResolveTrackedDeckCard(...)` 回写逻辑，避免同一场战斗被多个实例重复计数。
- 三张 `Shadow` 的奖励效果保持不变。

### 验证状态

- `dotnet build -c Release` 通过，`0 warning / 0 error`。
- 已执行 release build 与安装。
- release/install 的 `dll / pck / mod_manifest.json` 哈希已核对一致。
- 仍需实机复测第一场不触发、第二场触发、SL 后剩余场次保留。

## 3. `Perk Up / 抖擞精神` 无色池污染与偶发卡中间

### 现象

- `Perk Up` 偶尔卡在屏幕中间，不能正常完成打出。
- 随机无色牌池会抽到状态、诅咒、多人模式牌等不应进入常规无色生成链的对象。

### 原版参考

- 原版 Regent `SpectrumShiftPower`。
- 原版 `ManifestAuthority`。
- 二者生成无色牌时都使用：
  - `ModelDb.CardPool<ColorlessCardPool>()`
  - `GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)`
  - `CardFactory.GetDistinctForCombat(..., player.RunState.Rng.CombatCardGeneration)`

### 修复

- `ModSupport.GiveRandomColorlessCardToPlayer(...)` 不再扫描全部 `IsColorless` card pool。
- 现在只从原版 `ColorlessCardPool` 的 unlocked cards 中生成，并遵守 `CardMultiplayerConstraint`。
- 生成随机性改走原版 `CombatCardGeneration` RNG。
- 入手命令改为对齐原版无色生成牌：
  - `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true)`

### 验证状态

- `dotnet build -c Release` 通过，`0 warning / 0 error`。
- 已执行 release build 与安装。
- release/install 的 `dll / pck / mod_manifest.json` 哈希已核对一致。
- 2026-04-27 实机复测通过，标记完成。
- 已确认 `Perk Up` 不再从污染池生成状态、诅咒、多人专用牌等非标准无色对象。

## 4. `Touch of Orobas` 把 `DollMask` 升级成无效果头戴

### 现象

- ancient 奖励 `Touch of Orobas / 欧罗巴斯之触` 会升级 starter relic。
- Sakiko 的 starter relic 是 `DollMask`。
- 实机反馈：选择该奖励后变成无效果的 `Circlet / 头戴`。

### 原版参考

已对照原版 `TouchOfOrobas`：

- `SetupForPlayer(...)` 会读取玩家身上的 `RelicRarity.Starter` relic。
- `GetUpgradedStarterRelic(...)` 只查内置 `RefinementUpgrades` 映射。
- 该映射只包含五个原版角色 starter relic。
- 查不到映射时，原版 fallback 到 `Circlet`。

### 根因

`DollMask` 是 mod starter relic，不在原版 `RefinementUpgrades` 映射中，所以 `TouchOfOrobas` 按原版 fallback 逻辑返回了 `Circlet`。

### 初版修复

- 新增 `TouchOfOrobasDollMaskUpgradePatch`。
- 只 patch `TouchOfOrobas.GetUpgradedStarterRelic(...)`。
- 当 `starterRelic is DollMask` 时，把结果改为 `ModelDb.Relic<UpgradedDollMask>().ToMutable()`。
- 保留原版 `SetupForPlayer(...)`、hover tip、`AfterObtained(...)` 与 `RelicCmd.Replace(...)` 链路。

### 2026-04-26 二次设计修正

T5 确认 `TouchOfOrobas` 的原版升级结果是“starter refinement”结果，不应继续把 Sakiko 的结果复用到普通 `UpgradedDollMask`。

本次已新增 Sakiko 专属 Orobas relic：

- 类名：`PianoOfMom`
- Entry：`PIANO_OF_MOM`
- 英文名：`Piano of Mom`
- 中文名：`妈妈的钢琴`
- rarity 对齐原版 `BlackBlood / RingOfTheDrake`，仍为 `RelicRarity.Starter`
- 图标暂时复用稳定的 `doll_mask` relic 路径，避免缺资源导致 runtime 报错

效果实现对照原版 `NinjaScroll.BeforeHandDraw(...)`：

- 仅在第一回合抽牌前触发
- 只对 relic owner 生效
- 从项目维护的 `GetSongPoolCanonicals()` 歌曲池随机创建 combat card
- 对新建 combat card 调用 `CardCmd.Upgrade(...)`
- 通过 `CardPileCmd.AddGeneratedCardToCombat(..., PileType.Hand, ...)` 加入当前战斗手牌
- 不写入 deck，不污染 canonical / deck instance
- 不走 `GiveRandomSongCardToPlayer(..., true)`，因此不会套 Compose 的 0 费 this-combat 逻辑

`TouchOfOrobasDollMaskUpgradePatch` 现已改为：

- 当 `starterRelic is DollMask` 时，返回 `ModelDb.Relic<PianoOfMom>().ToMutable()`
- 原版五个角色映射不受影响

### 验证状态

- `dotnet build -c Release` 通过，`0 warning / 0 error`。
- 已执行 release build 与安装。
- release/install 的 `dll / pck / mod_manifest.json` 哈希已核对一致。
- 2026-04-27 实机复测通过，标记完成。
- 已确认 `Touch of Orobas` 对 Sakiko 不再给 `Circlet`，并正确进入 `PianoOfMom / 妈妈的钢琴` 路线。
- 已确认 `PianoOfMom` 战斗开始生成随机升级歌曲牌的效果正常。

## 5. 帝皇蟹转向问题调查记录

### 当前反馈

- Mac 实机测试：Sakiko 可以正常转向。
- 另有 Windows 玩家反馈：不能转向。
- 当前怀疑方向：平台差异或渲染节点差异导致视觉翻转不一致。

### 原版参考

已反编译：

- `KaiserCrabBoss`
- `Crusher`
- `Rocket`
- `SurroundedPower`
- `NCreature`
- `NCreatureVisuals`

原版机制：

- `Crusher.AfterAddedToRoom()` 给左钳加 `BackAttackLeftPower`。
- `Rocket.AfterAddedToRoom()` 给右钳加 `BackAttackRightPower`，并给玩家侧加 `SurroundedPower`。
- `SurroundedPower.BeforeCardPlayed(...)` 根据目标左右钳更新 `Facing`。
- 视觉翻转通过：
  - `NCombatRoom.Instance.GetCreatureNode(c)?.Body`
  - 对 `Body.Scale.X` 乘 `-1`

### Sakiko 当前 scene

文件：

- `pack/scenes/creature_visuals/togawasakiko.tscn`

当前结构：

- 根节点挂原版 `NCreatureVisuals`
- 存在 `%Visuals` 节点
- `Sprite2D` 位于 `%Visuals` 下

按原版逻辑，`NCreatureVisuals.GetCurrentBody()` 应返回 `%Visuals`，因此 `SurroundedPower` 理论上能翻转 Sakiko 静态立绘。

### 可能原因

当前不能写成已确认，只能列为调查假设：

1. Windows 下 Godot 对静态 `Sprite2D` 父节点负 scale 的表现与 Mac 有差异。
2. 玩家看到的是转向视觉未变，但 `SurroundedPower.Facing` 与伤害乘区其实已生效。
3. 某些战斗视觉刷新在 Windows 上重置了 `%Visuals.Scale`，抵消 `SurroundedPower` 的翻转。
4. Sakiko 静态立绘本身左右对称或中心偏移，让玩家误判没有转向。
5. 若 Windows 玩家使用的不是最新安装包，也可能是旧 scene / 旧 DLL 导致的表现差异。

### 2026-04-27 复查结论

重新对照原版后，当前更细结论如下：

- `NCreatureVisuals._Ready()` 会通过 `%Visuals` 设置 `Body`。
- Sakiko 当前 `togawasakiko.tscn` 存在 `%Visuals`，且 `Sprite2D` 在其下，原版 `Body` 解析链应成立。
- `SurroundedPower` 的战斗乘区由自身 `Facing` 字段决定；视觉翻转只是额外把 `Body.Scale.X` 在正负之间切换。
- 因此，如果 Windows 反馈只是“看起来没转向”，不能直接推断战斗机制失效。
- 系统差异仍可能存在，但目前更可能局限在静态 `Sprite2D` 父节点负 scale 的视觉表现、玩家误判、或旧包差异。
- 2026-04-27 实机反馈确认：背击伤害乘区正常。因此帝皇蟹机制层已判定正常，剩余问题只按视觉反馈或玩家误判记录。

### 下一步调查建议

- 在 Windows 反馈者环境中确认 release/install 哈希，排除旧包。
- 用 `fight KAISER_CRAB_BOSS` 进入帝皇蟹。
- 分别打左钳与右钳，观察：
  - 角色视觉是否翻转。
  - 背击伤害是否按 `SurroundedPower` 的 `1.5x` 变化。
- 若视觉不翻但伤害正常，优先修 scene / visual 层。
- 若伤害也不变，再查 `SurroundedPower` 是否施加、`Facing` 是否更新、目标是否带 `BackAttackLeftPower / BackAttackRightPower`。
