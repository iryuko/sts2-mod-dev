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

## 6. 联机混合队伍遇到 `UnattendedPiano` 导致非祥子队友卡住

### 现象

- 联机队伍中有祥子时，可能遇到祥子专属普通问号房事件 `UnattendedPiano / 无人钢琴`。
- 祥子玩家自己流程正常。
- 非祥子队友进入该事件后会卡住。

### 根因

`UnattendedPiano.IsAllowed(...)` 旧实现只要求 run 中存在任意 `Togawasakiko` 玩家，因此“祥子 + 非祥子”的混合多人队伍也会把该事件加入候选池。

事件效果直接作用于 `Owner`，并会发放 `ShadowOfThePastI / II / III` 这类祥子专属长期事件牌。该事件没有为非祥子角色设计 fallback 流程，因此混合队伍中队友进入后存在卡住风险。

### 修复

- `UnattendedPiano.IsAllowed(...)` 已改为要求 run 内所有玩家都是 `Togawasakiko`。
- 单人祥子仍允许。
- 多人全祥子仍允许。
- 多人混合队伍不再让该事件进池。
- 不改事件内容，不引入 shared event 分叉，先用最小角色限制保证稳定。

### 验证状态

- `dotnet build src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- 待实机复测多人混合队伍是否不再遇到该事件。

## 7. 混合联机队伍中非祥子队友可参与 `TogawaTeiji` ancient 奖励

### 设计口径

- `TogawaTeiji / 丰川定治` 是 ancient 奖励事件，不同于祥子专属普通事件 `UnattendedPiano`。
- 只要队伍中存在丰川祥子，就可以遇到 `TogawaTeiji`。
- 非祥子队友可以正常参与奖励选择，并通过 `BestCompanion / BlackLimousine` 获得对应卡牌奖励。

### 根因

`TogawaAncientAvailabilityPatch` 旧实现按当前选择奖励的 `player.Character is Togawasakiko` 限制 `TogawaTeiji`。

这会让混合联机队伍中非祥子队友被挡在定治奖励外，和当前设计口径不一致。同时，`TogawaTeiji` 只注册了 `TOGAWASAKIKO` 角色对白，非祥子 owner 进入时也缺少通用 dialogue fallback。

### 修复

- `TogawaAncientAvailabilityPatch` 已改为按 run 判断：
  - `runState.Players.Any(player => player.Character is Togawasakiko)`
- 只要本局队伍里有祥子，`TogawaTeiji` 对该 run 中所有玩家都允许。
- `TogawaTeiji.DefineDialogues()` 新增 `AgnosticDialogues` 通用重复对白。
- 补充 `TOGAWA_TEIJI.talk.ANY.*` 英中本地化，供非祥子 owner 使用。
- `BestCompanion / BlackLimousine` 的原有发卡链保持不变。

### 验证状态

- `dotnet build src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- 待实机复测混合队伍中非祥子队友选择 relic 后是否正常获得并使用 `BarkingBarkingBarking / PullmanCrash`。

## 8. `TogawaTeiji` 地图节点显示为 Neow 且无法交互

### 现象

- 第三层本应为 `TogawaTeiji` 的 ancient 节点显示为 Neow。
- 节点无法正常交互。

### 根因

反编译原版确认，`AncientEventModel.MapIconPath` 并不会读取此前文档记录的 `pack/images/packed/ancients/map_nodes/togawa_teiji_map_node.png`。

原版硬编码路径是：

- `res://images/packed/map/ancients/ancient_node_togawa_teiji.png`
- `res://images/packed/map/ancients/ancient_node_togawa_teiji_outline.png`

而 `scenes/ui/ancient_map_point.tscn` 默认贴图就是 Neow。Teiji 图标资源缺失时，`NAncientMapPoint._Ready()` 的运行时替换失败，表现上就可能保留 Neow 默认图，并因初始化异常导致不可交互。

同时，`TogawaAncientAvailabilityPatch` 之前直接访问 `runState.Players.Any(...)`，缺少初始化阶段防御；`Glory.AllAncients / GetUnlockedAncients` 之前也总是追加 Teiji，和“有祥子才能遇到 Teiji”的设计口径不完全一致。

### 修复

- 补回原版硬编码路径下的地图节点资源：
  - `pack/images/packed/map/ancients/ancient_node_togawa_teiji.png`
  - `pack/images/packed/map/ancients/ancient_node_togawa_teiji_outline.png`
- 运行 `build-mod.sh` 重新生成 `.import` 与 `runtime_imports/*.ctex`，并重新打包 PCK。
- `TogawaAncientAvailabilityPatch` 改为调用空值安全的 `HasTogawasakiko(IRunState?)`。
- `Glory.AllAncients / GetUnlockedAncients` 的 Teiji 追加改为当前 run 中有 `Togawasakiko` 才追加。
- `ModelDb.AllAncients` 仍保持全局注册 Teiji，避免破坏 console / 模型查找。

### 验证状态

- `dotnet build src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `../../shared/scripts/build-mod.sh . --configuration Release` 通过，并生成新的 PCK。
- 已确认 `pack/images/packed/map/ancients/ancient_node_togawa_teiji*.png.import` 与对应 `pack/runtime_imports/ancient_node_togawa_teiji*.ctex` 存在。
- 待实机复测第三层 Teiji 节点是否不再显示 Neow，且进入后可正常交互。

## 9. 祥子第二层遇到 `Darv / 达弗` 卡死

### 现象

- 单人选择 `Togawasakiko` 后，第二层遇到先古之民 `Darv / 达弗` 时会卡住。
- 当前日志没有直接捕捉到 Darv 异常堆栈，但原版 Darv 代码给出可复现的高风险入口。

### 原版参考

已对照原版：

- `MegaCrit.Sts2.Core.Models.Events.Darv.GenerateInitialOptions()`
- `MegaCrit.Sts2.Core.Models.Relics.DustyTome.SetupForPlayer(Player)`

Darv 生成初始选项时有一半概率加入 `DustyTome`。

`DustyTome.SetupForPlayer(...)` 会从当前角色卡池中筛选：

- `CardRarity.Ancient`
- 且不属于 `ArchaicTooth.TranscendenceCards`

然后直接 `NextItem(...)` 取一张作为将来发放的 ancient card。

### 根因

`TogawasakikoCardPool` 当前没有任何 `CardRarity.Ancient` 卡。

因此 Darv 在给祥子生成 `DustyTome` 选项时，候选 ancient card 集合为空。原版 `DustyTome.SetupForPlayer(...)` 没有为空集合做防守，会在事件初始选项生成阶段中断，表现为进入 Darv 后流程卡住。

这不是 Darv 的对白缺失问题：

- Darv 原版有 `AgnosticDialogues`
- 自定义角色在非首次访问时理论上可以走 `ANY` 重复对白

### 修复

新增 `src/Patches/DarvPatches.cs`：

- 只在 `Darv.Owner.Character is Togawasakiko` 时接管 `Darv.GenerateInitialOptions()`。
- 复刻原版 Darv 的 boss relic 选项池与 Act 2 / Act 3 能量 relic 分支。
- 在准备加入 `DustyTome` 前，先检查祥子卡池是否存在可供 `DustyTome` 使用的 `CardRarity.Ancient` 卡。
- 若没有可用 ancient card，则跳过 `DustyTome`，改为返回普通 Darv relic 选项，避免空池 `NextItem(...)`。
- 其他角色仍走原版 Darv 流程。

### 验证状态

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过。
- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已覆盖安装。
- release / installed 哈希已核对一致：
  - dll：`cd44f51378538491c7b5c87692797fdee11070e355b4282b678e81cc4b0f6780`
  - pck：`70c64d189f1911e04cd91f6f779e3a515ae8e9c5949708083dbe3f5fec8263b6`
  - manifest：`1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`
- 待实机复测：祥子第二层进入 Darv 是否不再卡住，且选项数量正常。

### 后续设计修正：加入祥子 Ancient 卡

为让 `DustyTome` 走原版正常流程，而不是长期依赖跳过选项，已新增独立 Ancient 卡 `Curseslander`：

- 模型类：`Curseslander`
- 本地化 id：`CURSESLANDER`
- 中文名：`诅咒`
- 英文名：`Curseslander`
- 稀有度：`CardRarity.Ancient`
- 类型：`Attack`
- 费用：`1`，升级后 `0`
- 效果：保留 `Slander / 中伤` 的伤害逻辑；打出后从压力衍生牌池有放回地随机生成 `2` 张牌，加入手牌，并只将这 `2` 张新生成实例的本场战斗费用改为 `0`。

实现要点：

- `Curseslander` 加入 `TogawasakikoCardPool`，因此 `DustyTome.SetupForPlayer(...)` 可以抽到合法 ancient card。
- `Togawasakiko.IsRewardEligibleCard(...)` 显式排除 `CardRarity.Ancient`，避免该卡进入普通奖励修复池。
- `Curseslander` 使用专用的 `GiveRandomZeroCostPressureGeneratedCardsToPlayer(...)` 生成压力衍生牌；该 helper 走 `CardFactory.GetForCombat(..., count, ...)`，原版实现为循环 `rng.NextItem(options)`，因此是有放回抽取，允许两张同名。
- `0` 费只作用于 `Curseslander` 本次创建并加入手牌的卡实例，不改变压力衍生牌 canonical，也不影响同名牌从其他途径生成时的费用。
- `CardModel` 原版会按 `Rarity == CardRarity.Ancient` 自动使用 ancient frame / text background / banner；mod 侧只需要提供 portrait。
- 当前 `assets/cards/ancient/curseslander.png` 与 `pack/mod_assets/cards/ancient/curseslander.png` 已替换为正式 Ancient 风格卡图；尺寸 `606x852`，已重新 build / install 生成 runtime import。
- 为排查 console 连续触发 Darv 时看不到 `DustyTome` 的情况，`DarvPatches.cs` 已加入诊断日志：记录 `dustyTomeRoll`、Darv relic 选项数、DustyTome ancient 候选数量与 id，以及最终是加入 `DustyTome` 还是走 relic-only fallback。当前逻辑仍保留原版 50% `DustyTome` 掷骰，不强制出现。
- 最新诊断版已重新 build / install，release 与 installed hash 一致：dll `ee7bc54383e099917c99572bac59544de912870e47fe22eb3bcc1ae8078ee8de`，pck `e3bd5dac11e0136000759192bbf10ea0887311a3f00cd5aeba4fc6565b72182a`，manifest `1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`。

为制作正式图，已从原版 `.ctex` 导出 8 张 Ancient portrait 参考图到：

- `assets/cards/ancient/reference_original/apotheosis.png`
- `assets/cards/ancient/reference_original/break.png`
- `assets/cards/ancient/reference_original/corruption.png`
- `assets/cards/ancient/reference_original/forbidden_grimoire.png`
- `assets/cards/ancient/reference_original/neows_fury.png`
- `assets/cards/ancient/reference_original/quadcast.png`
- `assets/cards/ancient/reference_original/suppress.png`
- `assets/cards/ancient/reference_original/wraith_form.png`

补充验证：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- 使用系统 Python 路径重跑 `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过，并生成新的 PCK。
- 已确认 `pack/mod_assets/cards/ancient/curseslander.png.import` 与 `pack/runtime_imports/curseslander*.ctex` 存在。

## 10. `Aroma of Chaos / 混沌芳香` 升级牌后事件卡住

### 现象

- 祥子遭遇普通事件 `Aroma of Chaos / 混沌芳香`。
- 选择 `Maintain Control / 维持理智`，并在牌组中选择一张牌升级后，事件界面无法继续互动。

### 原版参考

反编译原版 `MegaCrit.Sts2.Core.Models.Events.AromaOfChaos.MaintainControl()` 确认流程为：

- `CardSelectCmd.FromDeckForUpgrade(...)` 选择牌。
- 若有选择结果，调用 `CardCmd.Upgrade(cardModel)`。
- 然后读取 `characters` 表中的 `<角色ID>.aromaPrinciple`。
- 将该文本作为 `AromaPrinciple` 变量插入事件结束描述。

### 根因

`TOGAWASAKIKO.aromaPrinciple` 缺失。

因此升级本身已经完成，但原版事件在设置结束描述时读取：

- table：`characters`
- key：`TOGAWASAKIKO.aromaPrinciple`

触发 `LocException`，导致 `EventOption.Chosen()` 的异步任务中断，事件没有进入 finished 状态。

这不是升级牌命令的问题，也不需要 patch `AromaOfChaos`。

### 修复

在 `src/ModSupport.cs` 的角色本地化 override 中补充：

- `eng/characters.json`：`TOGAWASAKIKO.aromaPrinciple`
- `zhs/characters.json`：`TOGAWASAKIKO.aromaPrinciple`

修复保持原版事件流程不变，只补齐自定义角色必须提供的角色本地化字段。

### 同类缺 key 扫描

继续扫描反编译结果后，确认当前原版还有以下按角色 id 拼接 localization key 的路径：

- `SunkenTreasury.SecondChest()`：
  - `characters/<角色ID>.goldMonologue`
  - 选择大宝箱、获得金币并加入 `Greed` 后用于结束描述。
- `NEventOptionButton.OnRelease()`：
  - `CharacterModel.EventDeathPreventionLine`
  - 实际读取 `characters/<角色ID>.eventDeathPrevention`
  - 多人局中事件选项会杀死当前玩家时用于死亡保护气泡。
- `FlavorSynchronizer.CreateEndTurnPingDialogueIfNecessary()`：
  - `characters/<角色ID>.banter.alive.endTurnPing`
  - `characters/<角色ID>.banter.dead.endTurnPing`
  - 多人 end-turn ping 对话气泡使用。
- `SeaGlass.Title`：
  - `relics/SEA_GLASS.<角色ID>.title`
  - 不是普通事件，但属于同一类“原版按角色拼 key”的自定义角色风险。

已补齐：

- `TOGAWASAKIKO.goldMonologue`
- `TOGAWASAKIKO.eventDeathPrevention`
- `TOGAWASAKIKO.banter.alive.endTurnPing`
- `TOGAWASAKIKO.banter.dead.endTurnPing`
- `SEA_GLASS.TOGAWASAKIKO.title`

并用集合比较确认：祥子当前已经覆盖原版内置角色共有的 `characters` key 后缀。

### 验证状态

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj -c Release` 通过，`0 warning / 0 error`。
- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 通过。
- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已覆盖安装。
- release / installed 哈希已核对一致：
  - dll：`72793eaff2e9f942a885653cbd2384e1eb5e63fc558a9e01ed57b597d4054ca6`
  - pck：`34c5b3d1f418c9d77cf9878e650a24222adfaab6d4ed3b957f168a233f371b49`
  - manifest：`1b77b1cc7269ccdd539c30e63cffb1c25613e914d5a957b0a6d9c7971d44a849`
- 待实机复测：重新进入 `Aroma of Chaos`，选择升级牌后应正常显示结束描述并允许离开事件。
