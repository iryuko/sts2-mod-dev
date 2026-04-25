# T4 角色实现状态

日期：2026-04-06

## 一 当前实现阶段

当前 `Togawasakiko_in_Slay_the_Spire` 已从“纯文档 / 纯资源目录”推进到：

- 可编译的源码实现阶段
- 已生成最小 release 候选物的阶段
- 已完成第一次安装与游戏内首轮闭环验证的阶段
- 已开始进入首批正常卡池牌实现的阶段

当前主线目标已从“第一次最小闭环”推进到：

1. 扩首批冻结的正常卡池牌
2. 持续做战斗内实机验证
3. 继续把占位资源替换成正式资源

当前状态判断应写成：

- 已打通第一次最小可运行闭环
- 已确认角色可以出现在选人界面并进入战斗
- 但战斗内机制与卡池仍处于持续扩写阶段

## 一点四 原版对齐准则

- “对齐原版”必须先反编译或反射原版相关实现，再改 mod 代码。
- 不要仅凭 API 名称、接口形状或“看起来更合理”的封装来推断原版路径。
- 有用户给出原版参考牌 / 遗物 / power 时，第一步应检查该对象的实际 IL / 方法实现：
  - 例如升级加关键词先看 `CalculatedGamble.OnUpgrade`
  - 升级移除 `Exhaust` 先看 `Hotfix.OnUpgrade`
  - 动态费用若是卡牌自身随本战斗历史减费，先看 `BansheesCry.AfterCardEnteredCombat(...)` / `AfterCardPlayed(...)`
  - 动态费用若来自外部 power / relic，再看对应 `TryModifyEnergyCostInCombat`
- 如果原版实现与直觉不同，以原版实现为准，并在文档中标明参考对象。

## 一点五 本轮 bug 修复状态

## 一点五点二 2026-04-23 `jukebox` 与火堆房音频加载兼容修复

- 已确认当前 `jukebox` 的运行时接入方式是：
  - `NRun._Ready()` 登记当前 run
  - `NGlobalUi._Ready()` 注入右上角独立浮层
  - 面板内 `AudioStreamPlayer` 直接播放 `res://audio/music/tracks/` 中的 runtime 曲目
  - 不经过卡牌、relic、power、休息点选项或 scene patch 触发
- 本轮冲突点不是火堆角色 scene，也不是 `rest_site_character_togawasakiko.png` 资源缺失，而是音乐生命周期：
  - 旧方向会在火堆入场前专门干预 `RestSiteRoom.Enter`
  - 这条路和原版 `RunManager.EnterRoomInternal -> RestSiteRoom.EnterInternal -> NRunMusicController.UpdateTrack/UpdateAmbience -> RoomEntered` 时序不完全一致
  - 火堆房还会走 `update_campfire_ambience`，不能像普通战斗 BGM 那样直接阻断房间音乐加载
- 当前代码已改为统一遮罩策略：
  - 移除 `JukeboxRestSiteRoomEnterPatch`
  - 不再对火堆房做专门的 `RestSiteRoom.Enter` 前缀干预
  - `jukebox` 播放自定义曲目时，只把原版 `Bgm` / `Music` bus 压到静音
  - 原版房间 BGM / ambience 加载链仍照常运行
  - `jukebox` 自己的曲目优先走 `Master` bus 播放
  - 如果找不到可静音的 `Bgm` / `Music` bus，则退化为“双线播放”：不阻止原版音乐，只播放自定义曲目并记录 warning
- 当前仍保留的行为：
  - 进入 `CombatRoom` 时自动回到 `Off (null)`，恢复原版战斗音乐链
  - 非战斗房间中，如果自定义曲目仍在播放，则每次 `RoomEntered` 后重申 BGM 静音遮罩
- 当前验证状态：
  - `dotnet build -c Release` 通过，`0 warning / 0 error`
  - `shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release` 已完成
  - `shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target` 已完成
  - release 与游戏安装目录内的 `dll / pck / mod_manifest.json` 哈希一致
  - 已确认新 DLL 中不再包含 `JukeboxRestSiteRoomEnterPatch`
- 待实机复测：
  - 在非战斗房间选中一首 `jukebox` 曲目后进入火堆，确认不再卡死
  - 确认火堆房画面、休息 / 升级按钮与火堆 ambience 初始化正常
  - 确认自定义曲目仍在火堆房播放，且原版火堆 BGM 不叠声
  - 进入下一场战斗时确认 `jukebox` 自动回到 `Off (null)`

## 一点五点零 2026-04-20 普通问号房 `Shadow` 事件初次接线恢复

- 已确认 `Shadow of the Past / 往日之影` 的当前落地路线应为：
  - 普通问号房 `EventModel`
  - 不走 `AncientEventModel`
  - 不自造房间系统
- 已恢复并修通第一版源码接线：
  - 新增事件模型：
    - `UnattendedPiano : EventModel`
  - 新增事件池注入 patch：
    - `ModelDb.AllEvents`
    - `Glory.AllEvents`
    - `Hive.AllEvents`
    - `Overgrowth.AllEvents`
    - `Underdocks.AllEvents`
  - 当前事件内容已按文档设计接入：
    - 初始页可选回 `12` 血离开
    - 也可进入连续 `3` 次弹奏分支
    - 每次继续失去 `6` 点生命，并从 `Shadow I / II / III` 中不放回随机获得 `1` 张
    - 中途停止会直接结束事件
    - 事件内发放的 `Shadow` 直接加入牌组，不进手牌
  - 当前已通过 `RunState._visitedEventIds` 做“一局只出现一次”控制
- 当前一次关键实现修正：
  - `EventModel.Title` 与 `InitialPortraitPath` 在现版本 `sts2.dll` 中都不是可 override 成员
  - 因此当前实现已改为：
    - 标题沿用 `UNATTENDED_PIANO.title` 本地化键
    - 初始主图改走原版普通事件默认路径：
      - `res://images/events/unattended_piano.png`
    - 弹奏后的切图改显式切到：
      - `res://images/events/unattended_piano_shadow.png`
- 当前构建状态：
  - `dotnet build` 已恢复为 `0 warning / 0 error`
  - `build-mod.sh` 与 `install-mod.sh --replace-target` 已完成
  - 已确认安装后的 DLL 中包含：
    - `UnattendedPiano`
    - `AppendUnattendedPiano`
  - 已确认安装后的 PCK 中包含：
    - `images/events/unattended_piano.png`
    - `images/events/unattended_piano_shadow.png`
- 当前仍未完成的部分：
  - 尚未做实机问号房触发验证
  - 尚未确认：
    - 事件是否按角色限制正常进池
    - 初始进房是否正常显示主图与文本
    - 中途切图是否正常
    - 三张 `Shadow` 是否都能正确入牌组
    - run 内 visited 限制是否实际生效
  - 事件专属音乐仍保持“资源缺失时静默跳过”的保守口径

## 一点五点一 2026-04-20 普通问号房 `Shadow` 事件首轮实机问题修补

- 已按实机反馈修补四个直接问题：
  - 事件描述里手写 `NL` 被直接显示
  - `Shadow` 卡牌 `II / III` 因运行时 `Entry` 派生为：
    - `SHADOW_OF_THE_PAST_I_I`
    - `SHADOW_OF_THE_PAST_II_I`
    导致牌组界面读不到本地化键并报错
  - 事件继续弹奏时的掉血未走原版事件标准命令链，表现生硬
  - 事件音乐文件已进包，但 `PlayCustomMusic` 对 `res://audio/music/events/` 路径未成功播出
- 本轮实现修正：
  - `UNATTENDED_PIANO` 英中描述改为真实换行，不再写死 `NL`
  - 为 `SHADOW_OF_THE_PAST_I_I` / `SHADOW_OF_THE_PAST_II_I` 补兼容本地化键
    - 先止住现有 run / 现有存档里的牌组界面崩溃
    - 暂不直接改类名，避免把已生成进牌组的事件牌存档再打断一次
  - `Continue Playing / 继续` 选项现已对齐原版 `SlipperyBridge`：
    - 选项预览使用 `ThatDoesDamage(6)`
    - 实际结算使用 `ThrowingPlayerChoiceContext + CreatureCmd.Damage(..., DamageProps.nonCardHpLoss, null, null)`
  - 事件音乐路径现改为优先播放：
    - `res://audio/music/tracks/unattended_piano.mp3`
    - 若该路径缺失，才回退到：
      - `res://audio/music/events/unattended_piano.mp3`
- 当前已同步把相同音频资源放入：
  - `assets/audio/music/tracks/unattended_piano.mp3`
  - `pack/audio/music/tracks/unattended_piano.mp3`
- 当前状态：
  - `dotnet build` 仍为 `0 warning / 0 error`
  - `build-mod.sh` 与 `install-mod.sh --replace-target` 已重新执行
  - 新 DLL 已包含：
    - `ApplyEventHpLoss`
    - `ThrowingPlayerChoiceContext`
    - `ThatDoesDamage`
- 后续实现约束补记：
  - 事件内若要“展示并获得诅咒牌 / curse-like 牌”，当前已确认应复用原版：
    - `CardPileCmd.AddCursesToDeck(...)`
  - 不应在 `EventOption` 回调里直接拼接：
    - `CardCmd.PreviewCardPileAdd(...)`
    - 或 reward screen 的 `SpecialCardReward` 流程
  - 这类拼接方式会高概率与事件房交互状态冲突，表现为“牌不出现、事件锁死、后续选项无法点击”
  - 但这条经验当前只适用于 `Curse` 路线
  - 若未来要做“事件里展示并获得普通攻击 / 技能 / 能力牌”，必须重新找原版普通牌事件模板，不能直接套用这条诅咒接口

## 一点五点零 2026-04-20 / 2026-04-25 `Symbol III` 卡面动态格挡值修复

- 已重新按原版 Defect `GeneticAlgorithm` 反编译结果调整 `Symbol III`
- 当前实现改为：
  - `CurrentBlock` / `IncreasedBlock` 使用 `[SavedProperty]`，对齐原版的跨存档永久增值字段
  - `CanonicalVars` 使用 `new BlockVar(CurrentBlock, ValueProp.Move)` 和 `DynamicVar("Increase", 1m)`
  - 打出时通过 `CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false)` 获得格挡，重新走原版格挡 / 敏捷修正路径
  - 打出后调用 `BuffFromPlay(DynamicVars["Increase"].IntValue)`，每次把 `IncreasedBlock` 永久增加 `1`
  - `UpdateBlock()` 将当前基础格挡刷新为 `(IsUpgraded ? 7 : 3) + IncreasedBlock`
- 当前预期：
  - 未升级初始显示 / 获得 `3` 格挡，之后每次打出永久 `+1`，显示 `4 / 5 / ...`
  - 升级初始显示 / 获得 `7` 格挡，之后每次打出仍永久 `+1`，显示 `8 / 9 / ...`
  - 降级后按原版降级路径重算为 `3 + IncreasedBlock`
- 已废弃旧的“战斗内计数、每次 `+3`、战斗结束清零”实现

## 一点五点一 2026-04-25 卡牌升级预览路径统一修正

- 已在 `TogawasakikoCard.AddExtraArgsToDescription(...)` 统一覆盖 `IfUpgraded` 描述变量：
  - 实际已升级卡：按升级态渲染
  - 火堆 / deck upgrade preview：按升级态渲染
  - 普通未升级卡：按基础态渲染
- 修正原因：
  - 只在描述文本里直接使用 `IfUpgraded`，但没有覆盖升级预览路径时，console 调出的卡在火堆升级预览里会出现“效果已按升级结算、文本仍按未升级显示”的错位
  - 部分奖励 / 预览 UI 也可能只切换显示态，不代表实际卡实例已升级
- `Symbol III` 后续已按原版 `GeneticAlgorithm` 风格收敛：
  - 使用 `CurrentBlock` / `IncreasedBlock` 保存永久增值
  - 升级初始格挡 `3 -> 7`
  - 每次打出永久 `+1`，并刷新 `DynamicVars.Block.BaseValue`
  - 格挡获得走 `CreatureCmd.GainBlock(... DynamicVars.Block ...)`
- 当前实现准则：
  - 优先贴近原版升级链路
  - 基础关键词放在 `CanonicalKeywords`
  - 升级新增 / 移除关键词放在 `OnUpgrade()` 中通过 `AddKeyword(...) / RemoveKeyword(...)` 处理
  - 不再用 `CanonicalKeywords => IsUpgraded ? ...` 表达升级关键词变化
- 已按该准则修正：
  - `Speak`：升级后 `AddKeyword(CardKeyword.Innate)`
  - `Completeness`：升级后 `AddKeyword(CardKeyword.Retain)`
  - `Notebook`：升级后 `AddKeyword(CardKeyword.Retain)`
  - `TreasurePleasure`：升级后 `RemoveKeyword(CardKeyword.Exhaust)`
  - `MasqueradeRhapsodyRequest`：升级后 `RemoveKeyword(CardKeyword.Exhaust)`
- 进一步迁移到原版可重放 / 可降级路径的升级：
  - `Unendurable`：压力层数改为 `DynamicVar("PressureAmount") + OnUpgrade().UpgradeValueBy(1)`
  - `Thrilled`：获得能量改为 `EnergyVar + OnUpgrade().UpgradeValueBy(1)`
  - `PerkUp`：升级抽牌改为 `CardsVar(0 -> 1)`
  - `RestorationOfPower`：生成张数改为 `CardsVar(1 -> 2)`
  - `PutOnYourMask`：`Weak` 层数改为 `DynamicVar("WeakAmount")`
  - `DawnOfDespair`：攻击段数改为 `DynamicVar("HitCount")`
  - `BarkingBarkingBarking`：`Regen` 层数改为 `DynamicVar("Regen")`
  - `Innocence`：自闭层数改为 `DynamicVar("SocialWithdrawalAmount")`
  - `AWonderfulWorldYetNowhereToBeFound`：伤害上限改为 `DynamicVar("DamageCap")`
  - `Ether`：段数和压力层数改为 `DynamicVar("HitCount") / DynamicVar("PressureAmount")`
  - `GeorgetteMeGeorgetteYou`：压力层数改为 `DynamicVar("PressureAmount")`
  - `MusicOfTheCelestialSphere`：压力换算除数改为 `DynamicVar("PressureDivisor")`
- 当前仍保留 `IsUpgraded` 的位置只用于二态行为，而不是可量化模型状态：
  - `IHaveAscended`：升级后生成升级版【神化】且进入手牌
  - `Housewarming`：升级后生成升级版【大狗大狗叫叫叫】
  - `KillKiss`：升级后切换为 `KillKissPlusPower`

## 一点五点零 2026-04-20 Symbol 卡 console 调用修复

- 已定位 `Symbol` 卡无法通过 console 正常调取的直接根因：
  - `SymbolI` 的运行时 `Entry` 为 `SYMBOL_I`
  - 但原实现中：
    - `SymbolII` 实际派生为 `SYMBOL_I_I`
    - `SymbolIII` 实际派生为 `SYMBOL_II_I`
    - `SymbolIV` 实际派生为 `SYMBOL_I_V`
  - 这与本地化 key、文档冻结值、console 预期输入：
    - `SYMBOL_II`
    - `SYMBOL_III`
    - `SYMBOL_IV`
    不一致
- 根因不是资源缺失，也不是普通奖励池漏注册，而是类名里的罗马数字在原版 `Entry` 派生规则下被错误拆词
- 本轮修正：
  - 卡牌类名改为：
    - `SymbolIi`
    - `SymbolIii`
    - `SymbolIv`
  - 以便运行时 `Entry` 正确派生为：
    - `SYMBOL_II`
    - `SYMBOL_III`
    - `SYMBOL_IV`
  - `GenerateAllCards()` 同步改为引用新类名
  - `SongCardEntries` 同步补登记：
    - `SYMBOL_I`
    - `SYMBOL_II`
    - `SYMBOL_III`
    - `SYMBOL_IV`
- 当前预期 console 输入：
  - `card SYMBOL_I`
  - `card SYMBOL_II`
  - `card SYMBOL_III`
  - `card SYMBOL_IV`

## 一点五点零 2026-04-20 第七批补充卡牌与 `Shadow` 长期牌骨架

- 已新增并接入正常奖励池的 `2` 张普通牌：
  - `Blade Through the Heart / 利刃穿心`
    - `2` 费 rare 攻击
    - 先对所有敌人施加 `2` 层 `Vulnerable`、`2` 层 `Weak`、`-1 Dexterity`
    - 然后再对所有敌人造成 `12` 点伤害
    - 升级后伤害 `12 -> 20`
  - `Fragility / 脆弱`
    - `3` 费 uncommon 技能
    - 获得 `17` 点格挡
    - 获得 `1` 层【颜】
    - 回复 `8` 点生命
    - 升级后格挡 `17 -> 21`，回复 `8 -> 11`
- 已完成 `Shadow of the Past / 往日之影` 系列的第一轮实现方向落地，但当前仍为 **event-only curse-like cards**：
  - 当前不进入正常奖励池
  - 当前不进入商店常规池
  - 当前通过单独 `TogawasakikoEventGrantedCardPool` 注册，供后续事件奖励直接注入
- `Shadow I / II / III` 的实现口径已对齐原版 `Guilty` 方向：
  - 类型为 `CardType.Curse`
  - 带 `Unplayable`
  - 不可升级
  - 卡本体使用 `AfterCombatEnd(...)` 进行跨战斗计数
  - 达到 `2` 场战斗后触发奖励并 `RemoveFromDeck(...)`
- 当前各张 `Shadow` 的奖励链：
  - `Shadow I`
    - `2` 战后移除自身，并给予 `+7 Max HP`
  - `Shadow II`
    - `2` 战后移除自身，并移除 `1` 张 `StrikeTogawasakiko` 和 `1` 张 `DefendTogawasakiko`
    - 当前明确按 Sakiko 的 starter strike / starter defend 处理，不泛化到所有 `Strike / Defend`
  - `Shadow III`
    - `2` 战后移除自身，并把 starter relic 从 `DollMask` 替换为 `UpgradedDollMask`
- starter relic 升级链当前已按“替换 relic”落地，而不是就地改数字：
  - 新增 `UpgradedDollMask`
  - 触发时通过 `RelicCmd.Replace(DollMask, UpgradedDollMask)` 替换
  - 升级后效果为：每回合开始给予所有敌人 `3` 层压力
  - 原版 `DollMask` 保持每回合开始给予所有敌人 `1` 层压力
- 当前资源口径：
  - 本轮没有新增正式卡图或 relic 图
  - `Blade Through the Heart / Fragility / Shadow I / II / III` 当前先复用稳定运行时图片路径，占位运行
  - `UpgradedDollMask` 当前复用 `DollMask` 图标路径，先保证逻辑链稳定

## 一点五点零 2026-04-20 第六批补充卡牌与 Symbol 系统接线

- 已新增并接入正常奖励池的 `4` 张 `song` 牌：
  - `Symbol I`
    - `1` 费 common 攻击
    - `TargetType.RandomEnemy`
    - 造成 `3` 点伤害 `3` 次，且每次伤害都独立随机选择敌人
    - 打出后本回合获得 `Symbol I`
    - 升级后每段伤害 `3 -> 4`
  - `Symbol II`
    - `2` 费 uncommon 攻击
    - 对所有敌人造成 `6` 点伤害
    - 并使所有敌人各获得 `1` 层【自卑】
    - 打出后本回合获得 `Symbol II`
    - 升级后伤害 `6 -> 9`
  - `Symbol III`
    - `0` 费 uncommon 技能
    - 获得 `3` 点格挡
    - 本场战斗中每次打出这张牌，其获得的格挡增加 `3`
    - 打出后本回合获得 `Symbol III`
    - 升级后第一次打出时改为获得 `7` 点格挡，后续仍每次额外 `+3`
  - `Symbol IV`
    - `2` 费 rare 能力
    - 获得 `Symbol IV`
    - 升级后费用 `2 -> 1`
- 已新增 `4` 个 Symbol power：
  - `SymbolIPower`
  - `SymbolIIPower`
  - `SymbolIIIPower`
  - `SymbolIVPower`
- 当前 Symbol 计数规则已按“不同类型是否存在”实现，不按总层数实现：
  - `Symbol I / II / III / IV` 各自最多只算 `1` 种
  - `Symbol IV` 可以叠层，但只影响它自己的抽牌效果，不增加 Symbol 种类数
- 当前回合结束结算链已接入 `TogawasakikoCombatWatcherPower.AfterTurnEnd(...)`：
  - `1` 种：无事发生
  - `2` 种：获得 `5` 点格挡
  - `3` 种：获得 `5` 点格挡，并对 `1` 名随机敌人造成 `13` 点伤害
  - `4` 种：获得 `5` 点格挡，对 `1` 名随机敌人造成 `13` 点伤害，并使所有敌人获得 `10` 层压力
  - 结算后移除 `Symbol I / II / III`
  - `Symbol IV` 保留
- 当前资源口径：
  - `4` 张 Symbol 卡图已按稳定文件名接入 `assets/ + pack/`，并已替换为正式图
  - `4` 个 Symbol power icon 已按稳定文件名接入 `assets/icons/powers/ + pack/images/powers/`，并已替换为正式图

## 一点五点零 2026-04-19 `v0.103.2` 主线更新兼容复核

- 已确认当前本机游戏 live 版本为 `v0.103.2`：
  - 来源：`SlayTheSpire2.app/Contents/Resources/release_info.json`
  - 时间：`2026-04-16`
- 已刷新当前工作区引用基线：
  - `references/game-dlls/sts2/arm64/sts2.dll`
  - `references/api-notes/app/release_info.json`
- 关键兼容结论：
  - 当前 mod loader / manifest 链未因本次更新失效
  - 当前 Harmony patch 未出现 `Undefined target method` 级别断裂
  - 使用 `v0.103.2` 的最新 `sts2.dll` 重新编译后，源码仍为 `0 warning / 0 error`
- 当前仍观察到的运行时残余项：
  - 角色攻击音效仍会按原版默认规则尝试解析 `event:/sfx/characters/togawasakiko/togawasakiko_attack`
  - `KillKiss` 在新版主线下击杀 `Doormaker` 时，会联动打出原版 `queen_progress` 参数报错日志
  - 现阶段判断：
    - 前者是自定义角色未补战斗 FMOD event 的资源噪音
    - 后者更像 `v0.103.2` 原版音乐参数链的边界问题，而不是 manifest / patch 失效

## 一点五点零 2026-04-19 第五批补充卡牌与 `Despair / 绝望` 接线

- 已新增并接入正常奖励池的第五批补充牌：
  - `Restoration of Power / 复权`
    - `0` 费 common 技能
    - `Exhaust`
    - 随机将 `1` 张压力衍生牌加入手牌
    - 升级后改为独立随机加入 `2` 张
  - `Dawn of Despair / 绝望伊始`
    - `1` 费 uncommon 攻击
    - 对单体造成 `2` 点伤害 `6` 次
    - 然后使目标本回合获得 `Despair Echo / 绝望回响`
    - 升级后伤害次数 `6 -> 7`
  - `So Many Maggots / 好多蛆`
    - `1` 费 common 技能
    - 目标为 `AnyPlayer`
    - 选择 `1` 名玩家；若你手中有牌则丢弃 `1` 张；然后移除其身上的所有负面 Power
    - 当前“负面 Power”规则已明确冻结为：`PowerType.Debuff`
    - 升级后费用 `1 -> 0`
  - `Bail Money / 保释金`
    - `0` 费 uncommon 攻击
    - 造成 `8` 点伤害
    - 使目标失去 `1` 点敏捷
    - 然后你失去 `10` Gold
    - 当前实现口径是：即使当前 Gold 不足 `10` 也可打出，Gold 交由原版 `LoseGold` 链处理
    - 升级后伤害 `8 -> 12`
  - `Weightlifting Champion / 举重冠军`
    - `1` 费 common 技能
    - `Exhaust`
    - 失去 `4` 点生命
    - 获得 `1` 点力量和 `1` 点敏捷
    - 升级后生命损失 `4 -> 2`
    - 当前按 `Lose HP` 实现，不视为受到伤害
  - `Housewarming / 乔迁`
    - `0` 费 uncommon 技能
    - 将 `1` 张带有 `Ethereal + Exhaust` 的【大狗大狗叫叫叫】加入手牌
    - 升级后生成的【大狗大狗叫叫叫】改为升级版
    - 当前实现为生成临时修饰版本；升级版会同步触发 `BarkingBarkingBarking` 自身升级逻辑
- 已新增独立 `SakikoDespairEchoPower / Despair Echo / 绝望回响`：
  - `Type = Debuff`
  - 每层在本回合内都会让目标每次受到伤害时额外获得 `3` 层压力
  - 当前按每个独立伤害事件触发，不按整张牌聚合
  - 在玩家回合结束时自动移除
- 2026-04-19 晚间补充修复：
  - `Despair / 绝望` 已确认与原版 `DESPAIR_POWER` 发生 key 冲突，导致 tooltip 直接显示成原版说明
  - 当前已改为独立 key：
    - `SAKIKO_DESPAIR_ECHO_POWER.title`
    - `SAKIKO_DESPAIR_ECHO_POWER.description`
  - 对外显示名同步改为 `Despair Echo / 绝望回响`
  - `So Many Maggots` 当前维持 `AnyPlayer`，并在无显式目标时保守回退到自身，修复“单人局不能打出 / 打出后卡牌浮空”问题，同时保留多人局可选自己或队友的设计口径
  - `Weightlifting Champion` 当前仍保持技能牌，不改成能力牌；真实根因是 `v0.103.2` 下 `LoseHpInternal` 反射查找口径过旧，现已兼容 `public/non-public` 两种签名来源
- 当前资源口径：
  - 第五批 `6` 张卡图已全部由 `incoming_assets/cards/normal_pool/` 正式入库并接到 `assets/ + pack/`
  - `sakiko_despair_echo_power` 已补稳定文件名占位 icon，避免实机缺图

## 一点五点零 2026-04-19 第四批补充卡牌与 `Face / 颜` 机制修正

- 已把 `FaceReactionPower / 颜` 从单层固定触发改成可叠层计数：
  - `StackType` 改为 `Counter`
  - 持续时间仍为“直到你的下个回合开始前”
  - 每次怪物攻击命中你时，无论是否被格挡，都会按“当前层数 × 3”给予攻击者压力
- `Face / 颜` 当前仍为：
  - `1` 费 common `song` 技能牌
  - 获得 `8` 格挡
  - 施加 `1` 层修正后的 `FaceReactionPower`
  - 升级后格挡 `8 -> 12`
- 已新增并接入正常奖励池的第四批补充牌：
  - `Speak / 说话！`
    - `0` 费 common 技能
    - `Exhaust`
    - 将 `1` 张本场战斗费用变为 `0` 的【人格解离】加入手牌
    - 升级后获得 `Innate`
  - `Put On Your Mask / 面具戴好`
    - `1` 费 common 技能
    - 施加 `2` 层 `Weak`
    - 若目标在打出前已拥有 `Weak`，你获得 `1` 层 `FaceReactionPower`
    - 升级后改为施加 `3` 层 `Weak`
  - `Sever the Past / 斩断过去`
    - `1` 费 common 攻击
    - 造成 `8` 点伤害
    - 将弃牌堆全部洗回抽牌堆
    - 升级后伤害 `8 -> 12`
  - `Answer Me / 作出回答`
    - `2` 费 common 技能
    - 对所有敌人逐个判定：
      - 压力 `< 5`：获得 `7` 层压力
      - 压力 `>= 5`：失去 `1` 点力量
    - 升级后费用 `2 -> 1`
  - `Leave It to Me / 交给我吧`
    - `2` 费 uncommon 攻击
    - 造成 `11` 点伤害
    - 移除目标至多 `7` 层压力
    - 无条件回复 `5` 点生命
    - 若结算后目标仍有压力，施加 `1` 层 `Weak`
    - 升级后伤害 `11 -> 15`
  - `Final Curtain / 谢幕`
    - `1` 费 rare 攻击
    - 对所有敌人造成 `5` 点伤害，重复次数等于当前敌人总数
    - 当前实现口径是原版 `Whirlwind` 风格的 `AOE + hit count`
    - 升级后每段伤害 `5 -> 7`
  - `Georgette Me, Georgette You`
    - `1` 费 uncommon `song` 技能
    - 指定 `1` 名敌人
    - 若你的当前生命值大于等于其当前生命值，则施加 `7` 层压力
    - 否则造成 `7` 点伤害
    - 升级后改为 `9` 层压力 / `9` 点伤害
  - `Innocence / 天真`
    - `1` 费 uncommon 能力
    - 通过新 `InnocencePower` 在每回合开始时对所有敌人施加 `2` 层自闭
    - 升级后改为 `3` 层
- 当前资源口径：
  - 新增卡牌和 `INNOCENCE_POWER` 已按稳定文件名接线
  - 2026-04-19 已完成 `Speak / Put On Your Mask / Sever the Past / Answer Me / Leave It to Me / Final Curtain / Innocence` 正式卡图入库

## 一点五点零 2026-04-14 联机前置加固第一轮

- 已先对当前实现里最容易在联机中产生歧义的上下文兜底做保守修正：
  - `GetLocalPlayer(...)` 不再在多人 run 中直接 `FirstOrDefault()`
  - `CreateHookChoiceContext(...)` 不再在多人战斗中盲选第一个玩家作为 fallback
  - `Imprisoned Xii` 的自动抽牌改为优先使用基于当前战斗与持有者的 hook context
- 当前保留的单机兼容口径：
  - 若明确是单人战斗，仍允许使用旧的最小 fallback，避免把单机功能改坏
- 当前设计取向：
  - 宁可在未解析出本地玩家上下文时保守跳过“歧义动作”，也不在联机中 silently 绑错玩家

## 一点五点零点一 2026-04-14 联机前置加固第二轮

- 已继续收紧两类高风险多人 fallback：
  - `KillKiss` 的回合钩子现在显式把 `Owner.Player` 传入 hook context 构造
  - `GetRandomEnemy(...)` 在多人战斗中若拿不到 `CombatTargets` RNG，不再直接回退到第一个敌人
- 当前保留的单机兼容口径：
  - 只有在明确是单人战斗时，`GetRandomEnemy(...)` 才允许保留 `enemies[0]` 的最终 fallback
- 当前设计取向：
  - 对联机会造成潜在 desync 的“静默确定性回退”一律优先砍掉

## 一点五点零点二 2026-04-14 联机前置加固第三轮

- 已补 `ChoirSChoir` 的自动重放目标保护：
  - 自动打出消耗堆卡牌前，先显式解析 autoplay target
  - 若当前卡需要明确目标但在多人里无法安全解析，则保守跳过，不让引擎自行 fallback
- 当前设计取向：
  - 对自动重放链，优先保证“不会 silently 打错目标”
  - 单机仍保留原本功能；多人则优先避免错误结算与潜在 desync

## 一点五点零 2026-04-14 `Crucifix X / 十字架X` 腐化附魔异常扣除玩家格挡修复

- 已定位当前高概率根因：
  - `Crucifix X` 卡面目标是 `TargetType.AllEnemies`
  - 但旧实现实际使用的是“每一段都手写 `foreach enemy` + `DamageCmd.Attack(...).Targeting(enemy)`”
  - 在 `Corrupted` 附魔参与改写 / 重放时，这种“全体敌人卡面语义 + 单体逐次命令”会产生目标处理冲突
  - 表现上会出现：命中怪物的同时异常结算到玩家已有格挡
- 已改为更贴近原版 `Whirlwind` 的命令链：
  - 使用 `DamageCmd.Attack(...).TargetingAllOpponents(ownerCreature.CombatState).WithHitCount(totalHits)`
  - 不再手写逐敌人逐段攻击
- 当前预期结果：
  - `Crucifix X` 仍保持“对所有敌人造成 `X + 额外段数` 次伤害”的设计
  - `Corrupted` 附魔不应再异常清空玩家身上的格挡

## 一点五点零点一 2026-04-14 `Unendurable / 难熬` 数值调整

- 当前数值已改为：
  - 基础态：`8` 格挡，`3` 层压力
  - 升级后：`11` 格挡，`4` 层压力
- 已同步修改：
  - 卡牌实现
  - 本地化文案
  - 卡牌实现表

## 一点五点一 2026-04-13 `Perk Up / 抖擞精神` 接线

- 已新增正常卡池 `Common` 技能牌：
  - `Perk Up`
  - `0` 费 / `TargetType.None`
- 当前卡牌实现：
  - 随机将 `1` 张无色牌加入手牌
  - 升级后额外抽 `1` 张牌
- 当前实现策略：
  - 不硬编码原版某一个无色池类名
  - 直接从 `ModelDb.AllCardPools` 中筛 `IsColorless == true` 的卡池
  - 再从其中可见卡牌里随机生成
- 当前资源状态：
  - `perk_up.png` 已落稳定文件名并接 runtime
  - 2026-04-13 已由中文来稿 `抖擞精神.png` 替换为正式 common 卡图

## 一点五点二 2026-04-06 `Treasure Pleasure` 与人格解离规则收口

- 已新增正常卡池 `song` 非凡技能牌：
  - `Treasure Pleasure`
  - `0` 费 / `TargetType.None`
  - 基础态带 `Exhaust`
  - 升级后移除 `Exhaust`
- 当前卡牌实现：
  - 对自己施加 `1` 层 `PersonaDissociationPower`
  - 对自己施加 `MagneticForceHellWargodPower`
- 已新增独立 power：
  - `MagneticForceHellWargodPower`
  - 持续到本回合结束
  - 监听玩家本回合打出的每一张攻击牌
  - 每张攻击牌都会额外自动结算 `1` 次
  - 通过内部防重放保护，避免递归无限触发
- `PersonaDissociationPower` 已从“一次性标记”改为“可叠层 / 逐次消耗”：
  - `StackType` 改为 `Counter`
  - 每层都会让下一次受到的伤害在格挡结算前先翻倍
  - 每次独立伤害事件触发后，仅失去 `1` 层
  - 即使最终被格挡至 `0`，该层也会照常消耗
- 当前多段伤害结算口径：
  - 按每次独立 `DamageReceived` 事件判定
  - 不按整张牌聚合
  - 这样与当前 `Inferiority` 的运行时挂点保持一致，也最贴近现有引擎钩子
- 当前资源状态：
  - `Treasure Pleasure` 卡图已由来稿替换为正式图并接入 runtime
  - `MAGNETIC_FORCE_HELL_WARGOD_POWER` icon 已按新稳定名接入 runtime
  - `AVE_MUJICA_POWER / FACE_REACTION_POWER / KILL_KISS_POWER` 三张事件型 power icon 已由来稿替换为正式图并接入 runtime
  - `KillKissPower / KillKissPlusPower` 现共用同一张 `KillKiss.png` 素材；已补 `kill_kiss_power.png / kill_kiss_plus_power.png` 兼容落位，避免升级前后任一态回退占位图
  - 旧的 `NextAttackReplayPower / next_attack_replay_power.*` 代码引用、工作区 runtime 路径与安装目录残留已清扫
  - `incoming_assets/_originals/` 中保留了改名前的原始留痕，仅作为审计备份，不参与运行时接线

## 一点五点三 2026-04-05 Act 3 先古之民池恢复

- 已移除“当前 Sakiko run 的 `Act 3` 先古之民池只保留 `Togawa Teiji`”的测试限制。
- 当前 `Glory.AllAncients / GetUnlockedAncients` 的 patch 只负责把 `Togawa Teiji` 追加进 `Act 3` ancient 候选，不再覆盖原版池。
- `Hook.ShouldAllowAncient(...)` 仍保留角色限制：
  - 只有 `Togawasakiko` 角色可以实际遭遇 `Togawa Teiji`
  - 其余角色不会被这条 ancient 误接线
- 同时补了 `player` 判空保护，避免 ancient 过滤阶段再次因为空对象引发异常。

## 一点五点三 2026-04-05 局内 Jukebox 骨架

- 已新增最小局内 `jukebox` 系统骨架：
  - 代码文件：`src/Jukebox/JukeboxUi.cs`
  - root 注入入口：`src/Entry.cs`
- 当前实现方式不是改原版设置页，而是向 run UI 的 `AboveTopBarVfxContainer` 注入一个可展开 / 可折叠的右上角面板。
- 当前实现方式已经收口为：
  - `NRun._Ready()` 负责登记当前 run
  - `NGlobalUi._Ready()` 负责把 `jukebox` 注入全局 UI
  - 当前不再依赖 `AboveTopBarVfxContainer` 或战斗 HUD 按钮容器
  - 当前面板是独立浮层，内含曲目选择、进度条与音量条
- 当前面板行为：
  - 只在 `Togawasakiko` run 内显示
  - 自动扫描 `res://audio/music/tracks/`
  - 生成 `Off (null)` 选项和所有已入 runtime 曲目选项
  - 选中曲目后不阻断原版房间音乐加载，只把原版 `Bgm` / `Music` bus 作为静音遮罩压低，并用本地 `AudioStreamPlayer` 播放选中的音频文件
  - 下方提供可拖动进度条，直接映射到当前音频文件的真实播放进度
  - 选回 `Off (null)` 后，停止自定义播放并恢复原版 BGM bus 音量
  - 每次进入战斗都会自动重置到 `Off (null)`
- 当前明确限制：
  - 当前只扫描 `pack/audio/music/tracks/` 的真实文件，不会读取文档登记位或 `incoming_assets/`
  - 当前不再要求单独制作 `jukebox` 专属 UI 图标
  - 当前不再要求单独制作 `jukebox_open / confirm / next_track / stop` 这组专属 UI 音效，默认优先复用原版 UI 音效口径
- 当前判断：
  - 这条实现已经能支撑 T3 后续只通过“同名音频文件入库”来扩展局内 BGM 列表
  - 不需要先改卡牌逻辑，也不需要先重做原版音乐控制器结构

## 一点五点四 2026-04-05 jukebox 曲库首批安装

- 已按 `docs/t3-jukebox-audio-install-note.md` 的 runtime 口径完成首批曲库安装：
  - 原始来稿已从错误投递目录 `incoming_assets/audio/sfx/jukebox/` 整理回
    - `incoming_assets/audio/music/full_tracks/`
  - 正式库存已落位到：
    - `assets/audio/music/tracks/`
  - runtime 文件已同步到：
    - `pack/audio/music/tracks/`
- 当前首批已安装 `16` 首 `mp3` 曲目：
  - `ave_mujica`
  - `black_birthday`
  - `choir_s_choir`
  - `crucifix_x`
  - `ether`
  - `face`
  - `georgette_me_georgette_you`
  - `god_you_fool`
  - `imprisoned_xii`
  - `killkiss`
  - `masquerade_rhapsody_request`
  - `music_of_the_celestial_sphere`
  - `s_the_way`
  - `sophie`
  - `treasure_pleasure`
  - `two_moons_deep_into_the_forest`
- 当前判断：
  - 只要 `pack/audio/music/tracks/` 中的这些文件被打入 PCK，`jukebox` 就应能直接枚举并显示它们
  - 当前这批文件主要服务 `jukebox / BGM` 选择，不等于已经对每一张 `song card` 做播放接线

## 一点五点五 2026-04-04 角色选择音效接线

- 已把来稿 `oblivionis.mp3` 规范化转存为：
  - `assets/audio/sfx/character_select/togawasakiko_select.ogg`
  - `pack/audio/sfx/character_select/togawasakiko_select.ogg`
- 当前未继续依赖 `CharacterModel.CharacterSelectSfx` 的原版 FMOD event 口径：
  - `Togawasakiko.cs` 里该属性仍是原版 `silent_select`
  - 但已新增 Harmony patch，直接在选人界面 `SelectCharacter(...)` 时对 `Togawasakiko` 播放本地 `ogg`
- 当前实现策略：
  - 使用 `NCharacterSelectScreen.SelectCharacter` 的 postfix
  - 通过 `GD.Load<AudioStream>(res://audio/sfx/character_select/togawasakiko_select.ogg)` 读取本地音频
  - 使用 `AudioStreamPlayer` 挂到 root 播放
- 当前判断：
  - 这条接线绕开了“`CharacterSelectSfx` 可能只接受 FMOD event”这一不确定点
  - 资产、代码、release 包和游戏安装目录现在都已同步
  - 尚未做实机耳听验证

## 一点六 2026-04-03 丰川定治 relic/card 接线

- 已新增并接线两组“先古之民事件专用、但不并入角色常规 relic / card 池”的对象：

- relic：
  - `BestCompanion`
  - 当前效果为“获得时，将 `Barking Barking Barking` 加入牌组”
  - `BlackLimousine`
  - 当前效果为“获得时，将 `Pullman Crash` 加入牌组”
- relic granted card：
  - `BarkingBarkingBarking`
  - `Rare / 1` 费 / 攻击
  - 造成 `8` 点伤害并获得原版 `Regen` `3` 层
  - 升级后改为 `11` 伤害与 `4` 层 `Regen`
  - `PullmanCrash`
  - `Rare / 3` 费 / 攻击 / 消耗
  - 造成 `49` 点伤害；若目标压力大于 `8`，追加 `1` 层 `Vulnerable`
  - 升级后伤害改为 `61`

当前实现策略：

- 新增 `TogawasakikoRelicGrantedCardPool`
  - 用于注册 relic 附带卡牌
  - 不并入 `TogawasakikoCardPool`
- 新增 `TogawasakikoAncientRelicPool`
  - 用于注册未来先古之民事件 relic
  - 不并入当前角色常规 relic 池
- 新增 `ModSupport.AddSpecificCardToDeck<T>(...)`
  - 使用原版 `CardModel.ToMutable()` 与 `CardPile.AddInternal(...)`
  - 避免把“事件 / relic 送牌”错误接成“战斗内生成牌”
- 已新增 `TogawaTeiji : AncientEventModel`
  - 当前先按原版 `AncientEventModel` 抽象面实现：
    - `AllPossibleOptions`
    - `GenerateInitialOptions()`
    - `DefineDialogues()`
  - 当前 `3` 个选项已经直接接入：
    - `BestCompanion`
    - `BlackLimousine`
    - `继续演出吧 / Keep Performing`：获得 `1000 gold`
- 已新增 ancient 注入 patch：
  - `ModelDb.AllAncients`
  - `Glory` 的 `AllAncients / GetUnlockedAncients`
  - `Hook.ShouldAllowAncient(...)`
  - 当前口径是：丰川定治只进入 `Act 3` ancient 候选，并且只允许 `Togawasakiko` 角色实际遭遇

当前资源状态：

- `best_companion.png` 已安装到：
  - `assets/relics/ancient/`
  - `pack/images/relics/`
- `barking_barking_barking.png` 已安装到：
  - `assets/cards/relic_granted/`
  - `pack/mod_assets/cards/relic_granted/`
- `black_limousine.png` 已安装到：
  - `assets/relics/ancient/`
  - `pack/images/relics/`
- `pullman_crash.png` 已安装到：
  - `assets/cards/relic_granted/`
  - `pack/mod_assets/cards/relic_granted/`
- 丰川定治地图节点当前已用 prototype 复制出 runtime 占位：
  - `pack/images/packed/ancients/map_nodes/togawa_teiji_map_node.png`
  - `pack/images/packed/ancients/map_nodes/togawa_teiji_map_node_outline.png`

当前明确未做：

- 未对丰川定治 ancient 做实机验证
- 对外显示名应为 `Togawa Teiji`，不是 `Togawa Teiji`

2026-04-04 补充定位：

- 当前已根据实机日志把“地图可显示、点进去卡住”的问题收敛到事件房间打开阶段，而不是 ancient 进池阶段。
- 为避免再次打断事件 ID 与自动资源路径，本轮先只修正显示名与文档口径；
  - 内部稳定键 `TOGAWA_TEIJI` 与现有 `togawa_teiji` 资源文件名暂不整体重命名。
- `~/Library/Application Support/SlayTheSpire2/logs/godot.log` 中的真实异常为：
  - `System.NullReferenceException`
  - `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom.SetupLayout()`
- 当前确认 `TogawaTeiji : AncientEventModel` 的事件模型会按原版自动解析：
  - `InitialPortraitPath -> res://images/events/togawa_teiji.png`
  - `BackgroundScenePath -> res://scenes/events/background_scenes/togawa_teiji.tscn`
- 其中背景 scene 已存在，但此前 `pack/images/events/togawa_teiji.png` 缺失。
- 本轮已把正式来稿同步到：
  - `assets/ancients/event_main/togawa_teiji.png`
  - `pack/images/events/togawa_teiji.png`
- 当前判断：
  - 本次卡死的直接高概率根因，不是地图节点注入，也不是 relic / card 奖励链本身；
  - 而是事件房间布局阶段读取 `InitialPortraitPath` 时拿到空对象，最终在 `NEventRoom.SetupLayout()` 内部空引用。
- 本轮已重新 `build / install`，等待实机复测确认“点击节点后能正常进入对白”。

当前资产闭环判断：

- 当前“能让事件进池并显示基础节点/奖励”的最小 runtime 资产已具备：
  - `best_companion / black_limousine` relic 图与 atlas
  - `barking_barking_barking / pullman_crash` 卡图
  - `togawa_teiji_map_node / outline` 地图节点占位图
  - `pack/images/events/togawa_teiji.png` 事件立绘 runtime
  - `pack/scenes/events/background_scenes/togawa_teiji.tscn` 事件背景 scene
  - `pack/images/ui/run_history/togawa_teiji.png`
  - `pack/images/ui/run_history/togawa_teiji_outline.png`
- 当前仍未完成的是：
  - 丰川定治背景 scene 内部当前仍沿用 `pack/images/ancients/togawa_teiji_placeholder.png` 作为背景贴图来源，尚未把 scene 内部引用也完全收口到正式命名
  - 丰川定治地图节点仍是从 `prototype` 资源派生出的 runtime 占位版本
  - 丰川定治 ancient 尚未做实机验证

## 一点七 2026-04-01 发包前完整性检查

已对当前 mod 做一轮“源码声明资源 / `pack/` 实际资源 / `exports/release/` 导出产物”三层核对，当前结论如下：

- 当前导出给测试者的最小安装包仍是：
  - `Togawasakiko_in_Slay_the_Spire.dll`
  - `Togawasakiko_in_Slay_the_Spire.pck`
  - `mod_manifest.json`
- 当前 `exports/release/Togawasakiko_in_Slay_the_Spire/` 目录内未发现多余运行时文件：
  - 无旧目录残留
  - 无源码 / 文档被误打进 release 包
- 已逐项核对角色模型声明的关键 runtime 资源：
  - 角色选择 icon / splash
  - top panel / multiplayer hand
  - text energy icon / card energy icon
  - merchant / rest site portrait
  - character icon scene
  - char select background scene
  - energy counter scene
  - creature visual scene
  - merchant / rest site scene
  - 当前都已在 `pack/` 中存在
- 已核对当前代码中声明的全部卡图路径：
  - 基础牌
  - 普通池 common / uncommon / rare
  - 压力衍生牌
  - 当前都已在 `pack/mod_assets/cards/` 中存在
- 当前发现的发包前问题不在于“漏打包资源”，而在于 release 元数据仍停留在最早骨架阶段：
  - `manifest/mod_manifest.json` 的 `description` 过时
  - `version` 原先仍为 `0.1.0`
- 已把 manifest 描述与版本同步到当前可测试状态：
  - `version -> 0.2.0`
  - 描述改为当前实际内容口径

说明：

- `incoming_assets/` 下仍有原始投喂素材与 `.DS_Store`，但它们不属于 release 安装包内容
- `pack/scenes/merchant/characters/togawasakiko_merchant.tscn` 当前仍随包保留，且现行运行时 merchant 路径已重新接回该自定义 scene
- 若对外发测试包，应优先直接分发 `exports/release/Togawasakiko_in_Slay_the_Spire/` 目录，而不是整个 mod 源码目录

本轮已新增独立 bug 修复记录：

- `docs/t4-bugfix-round-2026-03-27.md`

当前这 `4` 类 bug 的代码侧状态为：

- Bug 4：角色牌未进入原版伤害 / 格挡修正链
  - 已定位
  - 已修代码
  - 待实机复测
- Bug 1：`CrucifixX` 不是 AOE `X` 费
  - 已定位
  - 已修代码
  - 已按复测反馈做二次修正
  - 待实机复测
- Bug 2：`SakiMovePlz` 条件判定与高亮失败
  - 已定位
  - 已修代码
  - 已按复测反馈做二次修正
  - 待实机复测
- Bug 3：压力衍生牌无法通过 console 调用
  - 已定位
  - 已修代码与安装包
  - 待实机复测
- 运行时集成补充：火堆 / 商店角色场景缺失
  - 已定位
  - 已补资源并重装
  - 待实机复测
- 新增：商店黑屏但主线程未完全冻结
  - 已定位
  - 已先后尝试自定义兼容 merchant scene
  - 当前运行时已重新使用 Sakiko 自己的 merchant scene
  - 待实机复测
- 新增：`Face` 触发条件与效果定义不符
  - 已定位
  - 已修代码
  - 待实机复测
- 新增：`Persona Dissociation` 倍伤乘区异常
  - 已定位
  - 已修代码
  - 待实机复测
- 新增：原创 debuff token 卡缺少 hover tip
  - 已定位
  - 已修代码
  - 待实机复测
- 新增：`Compose` / 共享自动生牌入手流程可能卡死
  - 已定位
  - 已修代码
  - 待实机复测
- 新增：第二批 `song` 牌接入后 reward / shop / `Compose` 坏牌污染
  - 已进一步确认更底层根因：
    - `ModSupport` 静态初始化曾因 `LoseHpInternal` 反射失败而拖死整套 localization override
  - 已定位到至少一处真实 `Entry` / 本地化 key 错位：
    - `KillKiss -> KILL_KISS`
  - 已把 `LoseHpInternal` 改为运行时惰性解析，避免继续阻断本地化写入
  - 已补 `KILL_KISS / KILL_KISS_POWER` 本地化 key
  - 已保留旧 `KILLKISS*` alias 兼容本地 override 残留
  - 已把 reward / shop / `Compose` / 压力衍生显式生成统一过滤为“已本地化且可见的牌”
  - `starter / song` 判定已从“按类型”收口为“按稳定 `Id.Entry` 集合”
  - 已把战斗奖励补齐逻辑收口回原版 `3` 张
  - 已重新构建并安装
  - 待实机复测
- 新增：`KillKiss` 在敌方回合开始时可能打断回合时序
  - 已定位为触发时机过早
  - 已从 `BeforeSideTurnStart(...)` 挪到 `AfterSideTurnStart(...)`
  - 已重新构建并安装
  - 待实机复测
- 新增：`KillKiss` 击杀导致战斗胜利后，进度统计对自定义角色抛异常
  - 已定位到原版 `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch / CheckFifteenBossesDefeatedEpoch`
  - 原版这两段只识别内置角色，遇到 `Togawasakiko` 会抛 `ArgumentOutOfRangeException`
  - 已新增 Harmony patch，对自定义角色跳过这两段 base-game-only epoch 统计
  - 已重新构建并安装
  - 待实机复测
- 新增：`Black Birthday` 描述格式错误
  - 已定位为错误的能量 selector 文本，不是卡图资产缺失
  - 已按原版方式改回 `EnergyVar + energyIcons()` 小图标方案
  - 已重新构建并安装
  - 待实机复测
- 新增：全局文本能量 icon 被错误映射到卡牌左上角大图
  - 已定位到 `EnergyIconHelper.GetPath("togawasakiko")` 误指向 `res://images/ui/card/energy_togawasakiko.png`
  - 已改为文本专用的小尺寸 sprite-font 资源 `res://images/packed/sprite_fonts/togawasakiko_energy_icon.png`
  - 卡牌左上角费用图仍单独保留 `CardPoolModel.EnergyIconPath`
  - 已重新构建并安装
  - 待实机复测
- 新增：boss 战胜利后，原版角色解锁 epoch 查询对自定义角色抛错
  - 已定位到 `ProgressSaveManager.ObtainCharUnlockEpoch(...)`
  - 原版会硬查 `角色 Entry + 2_EPOCH / 3_EPOCH / 4_EPOCH`
  - 对 `Togawasakiko` 会命中不存在的 `TOGAWASAKIKO2_EPOCH`
  - 已新增 Harmony patch，对自定义角色跳过该 base-game-only 解锁分支
  - 已重新构建并安装
  - 待实机复测
- 新增：`Two Moons Deep Into The Forest` 的“本战斗减费”状态跨战斗残留
  - 2026-04-25 复查后确认：旧修法把 `Two Moons` 费用刷新覆盖到 `player.Deck + combat piles`，并对 deck 实例写入 `SetThisCombat(...)`，会造成费用修正跨战斗 / 跨 run 残留
  - 2026-04-25 二次更正：用户指出 `Two Moons` 更接近原版 `BansheesCry`，已反编译确认原版路线：
    - `BansheesCry.AfterCardEnteredCombat(...)` 根据 `CombatManager.Instance.History.CardPlaysFinished` 回放本战斗历史，并对卡本体 `EnergyCost.AddThisCombat(...)`
    - `BansheesCry.AfterCardPlayed(...)` 在后续满足条件的牌打出后继续对该卡实例 `EnergyCost.AddThisCombat(...)`
  - 当前已改为 `TwoMoonsDeepIntoTheForest` 卡本体负责费用变化：
    - 2026-04-25 三次更正：已取消“不同名称 / Entry 去重”限制，避免同一张持久牌实例跨战斗保留已见过的 `Song` 名称
    - `AfterCardEnteredCombat(...)` 先清掉旧版本残留在该实例上的 local cost modifier，再按本战斗历史中已打出的 `Song` 张数补减费
    - `AfterCardPlayed(...)` 只要同一 owner 打出 `Song`，就对当前 `Two Moons` 实例追加 `EnergyCost.AddThisCombat(-1, false)`
    - `TogawasakikoCombatWatcherPower` 不再实现 `TryModifyEnergyCostInCombat(...)`，避免全局 hook 继续影响后续战斗 / run
    - `ClearPersistedTwoMoonsCostModifiers(...)` 只清理 deck 中旧版本残留的 local modifier；战斗内实例由 `AfterCardEnteredCombat(...)` 自己清理和重建
    - 不再通过 `AfterCombatEnd(...)` 清理费用，避免卡住战斗结算 / 奖励流程
  - `dotnet build src/Togawasakiko_in_Slay_the_Spire.csproj` 已通过
  - 待实机复测：一场战斗内随 `Song` 打出次数减费，新战斗 / 下一 run 回到 `7` 费，结算奖励不被阻塞
- 新增：升级关键词与 `Symbol III` 格挡显示 / 敏捷路径不一致
  - 已反编译原版：
    - `CalculatedGamble.OnUpgrade` 直接调用 `AddKeyword(CardKeyword.Retain)`
    - `Hotfix.OnUpgrade` 直接调用 `RemoveKeyword(CardKeyword.Exhaust)`
  - `Completeness` 的升级加关键词已对齐为 `AddKeyword(CardKeyword.Retain)`
  - `MasqueradeRhapsodyRequest` 的升级移除 `Exhaust` 已对齐为 `RemoveKeyword(CardKeyword.Exhaust)`
  - `Completeness` / `MasqueradeRhapsodyRequest` 描述已恢复 `{IfUpgraded}` 条件文本，用于卡面可见兜底
  - 已确认安装目录 DLL 与 release DLL 哈希一致，并已手动更新 `user://localization_override/*/cards.json` 中这两张牌的实际文本
  - `Symbol III` 改为用 `CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false)` 获得格挡，重新走原版敏捷 / 格挡修正路径
  - `Symbol III` 已按原版 `GeneticAlgorithm` 风格改为 `CurrentBlock` / `IncreasedBlock` 永久增值：未升级初始 `3`，升级初始 `7`，每次使用后当前实例基础格挡永久 `+1`
  - `I Have Ascended+` 改为创建【神化】后先升级再入手牌，避免入堆后升级状态未同步
  - `dotnet build src/Togawasakiko_in_Slay_the_Spire.csproj` 已通过
- 新增：`Inferiority` 的【过劳焦虑】触发时点写错
  - 已重新核对冻结文档：
    - 【过劳焦虑】应在“施加 `Inferiority` 时”尝试兑换
    - 后续减力量触发【自闭】仍属正常规则
  - 已移除上一轮误加的抑制逻辑
  - 已改为在 `Sophie` 成功施加 `Inferiority` 后立刻尝试消耗 `1` 层压力生成【过劳焦虑】
  - 已重新构建并安装
  - 待实机复测
- 新增：文本能量 icon 分流后仍偏大
  - 已确认不是路径错误，而是 `sprite_fonts` 里的临时 icon 主体仍过大
  - 已基于现有大费用图再次缩小文本专用 icon 主体
  - 后续又按实机表现把整张 icon 画布收紧，避免透明边被文本排版当成额外字宽
  - 已重新构建并安装
  - 待实机复测
- 新增：火堆角色立绘过小
  - 已定位为 `rest site` scene 缩放问题，不是图片分辨率问题
  - 已上调 portrait 的 `scale / position`
  - 已重新构建并安装
  - 待实机复测

## 二 当前已接入的对象

### 1. 角色骨架

- 已新增 .NET mod 项目：
  - `src/Togawasakiko_in_Slay_the_Spire.csproj`
- 已新增角色模型：
  - `Togawasakiko`
- 已新增最小卡池 / 药水池 / relic 池骨架：
  - `TogawasakikoCardPool`
  - `TogawasakikoPotionPool`
  - `TogawasakikoRelicPool`

### 2. 角色接入方式

当前已切到模型层 patch 方案：

- 角色模型由当前 mod 正常提供
- 通过 Harmony patch 把 `Togawasakiko` 追加进 `ModelDb.AllCharacters()`
- 选人界面沿用原版 `NCharacterSelectScreen::InitCharacterButtons()` 的原生按钮生成流程

当前未把这条方案写成：

- 原版已有正式自定义角色扩展点

### 3. starter relic

- 已实现：
  - `DollMask`
- 当前逻辑：
  - 每个玩家回合开始时，给予所有敌人 `1` 层压力

### 4. 核心状态与原创 debuff

- 已实现：
  - `PressurePower`
  - `PersonaDissociationPower`
  - `SocialWithdrawalPower`
  - `InferiorityPower`

当前实现口径：

- `PressurePower`
  - 不衰减
  - 只作为累计标记和消耗来源
- `PersonaDissociationPower`
  - 可叠层
  - 每层都会让下一次受到的伤害在格挡结算前先翻倍
  - 按每次独立伤害事件逐层消耗
- `SocialWithdrawalPower`
  - 目标回合结束时受到 `7` 点伤害并减少 `1` 层
- `InferiorityPower`
  - 当前按“层数型持续时间 debuff”处理
  - 每层代表 `1` 个持续回合
  - 每次受到伤害时失去 `1` 点力量
  - 按每一段实际命中单独触发
  - 被格挡至 `0` 的伤害不触发
  - 回合结束减少 `1` 层
  - 可叠加
  - 可续层
  - 在施加该 debuff 时尝试把 `1` 层压力兑换为【过劳焦虑】

说明：

- `Inferiority` 的持续口径已按 2026-03-29 设计更新冻结
- 当前代码仍应视为最小可运行实现，仍需继续实机验证与相关卡牌接线

### 5. 起始牌组

- 已实现：
  - `StrikeTogawasakiko`
  - `DefendTogawasakiko`
  - `Slander`
  - `Unendurable`
- 已接入 starter deck：
  - `4` 张打击
  - `4` 张防御
  - `1` 张 `Slander`
  - `1` 张 `Unendurable`

### 6. 压力衍生牌

- 已实现：
  - `PersonaDissociation`
  - `SocialWithdrawal`
  - `AllYouThinkAboutIsYourself`
  - `OverworkAnxiety`

当前说明：

- `PersonaDissociation` 与 `SocialWithdrawal` 当前已直接对齐冻结 `Entry`
- `AllYouThinkAboutIsYourself` 已对齐 2026-03-29 冻结命名
- `OverworkAnxiety` 当前仍沿用现有内部名
- 压力衍生牌当前统一归入：
  - `TokenCardPool`
- 为避免问号房 / relic / 其他 transform 链路误抽：
  - 当前已显式覆写 `CanBeGeneratedByModifiers = false`
- 仍保留自定义显式生成链路：
  - `CombatState.CreateCard(...)`
  - `CardPileCmd.AddGeneratedCardToCombat(...)`

### 7. 压力衍生牌生成逻辑

- 已新增隐藏 watcher power：
  - `TogawasakikoCombatWatcherPower`
- 当前逻辑：
  - `Weak` 新增时消耗 `2` 层压力并生成【人格解离】
  - `Vulnerable` 新增时消耗 `3` 层压力并生成【满脑子都想着自己】
  - `Strength / Dexterity` 数值下降时消耗 `1` 层压力并生成【自闭】
  - `Inferiority` 生效时消耗 `1` 层压力并生成【过劳焦虑】

### 8. `song` 逻辑 tag 与首批正常卡池牌

当前已按冻结文档接入：

- `song` 作为普通逻辑 tag
- 歌曲牌池：
  - 当前由 `TogawasakikoCardPool` 中所有带 `song` 标记的正常卡池牌构成
- 压力衍生牌池：
  - `Persona Dissociation`
  - `Social Withdrawal`
  - `All You Think About Is Yourself`
  - `Overwork Anxiety`

### 9. 2026-03-29 第二批 `song` 牌已接入代码

- 已新增工作稿文档：
  - `docs/t4-card-batch-02-workdraft-2026-03-29.md`
- 当前已完成：
  - 代码实现
  - 本地化接线
  - 角色卡池接线
  - `Two Moons` 战斗内唯一 `song` 计数 watcher 接线
  - `God, You Fool` 的 `lose HP` helper 接线
- 当前仍未完成：
  - 独立卡图资源接线
  - 2026-03-29 晚间 bugfix 后的新一轮实机测试

首批已实现正常卡池牌：

- `Compose`
- `AveMujica`
- `Ether`
- `CrucifixX`
- `Face`
- `SakiMovePlz`
- `MusicOfTheCelestialSphere`
- `KillKiss`

本轮新增第二批 `8` 张正常卡池牌：

- `BlackBirthday`
- `ChoirSChoir`
- `ImprisonedXII`
- `GodYouFool`
- `MasqueradeRhapsodyRequest`
- `STheWay`
- `TwoMoonsDeepIntoTheForest`
- `Sophie`

当前同时已接入的事件型 / 规则型 power：

- `AveMujicaPower`
- `FaceReactionPower`
- `KillKissPower`
- `TheWayTemporaryDexterityLossPower`

当前实现口径：

- `Compose`
  - 从歌曲牌池随机生成 `1` 张牌入手，并将其本场战斗费用改为 `0`
- `AveMujica`
  - 每回合开始抽牌后，随机生成 `1` 张压力衍生牌入手
  - 然后自动打出抽牌堆顶牌
  - 当前 `AveMujicaPower` 已改为可叠层；每多 `1` 层，就额外重复 `1` 次“生压力牌 + 处理牌堆顶”
- `Ether`
  - 单体多段伤害
  - 基础：`5 x 2` 并给予目标 `2` 层压力
  - 升级：`5 x 3` 并给予目标 `3` 层压力
  - 旧版“击杀目标后给其他敌人压力”的设计已明确作废并被覆盖
- `CrucifixX`
  - 对所有敌人造成 `X` 段基础攻击
  - 全场高压力时对所有敌人额外 `2` 段
  - 当前已改为逐波重取存活敌人的手动 AOE 结算，避免中途清场后丢目标卡死
  - 升级后每段伤害 `6 -> 7`
- `Face`
  - 获得格挡，并给予一回合持续型受击施压 power
  - 升级后格挡 `9 -> 13`
- `SakiMovePlz`
  - 若上一张已打出的牌是 `song`，则追加易伤
  - 条件满足时应转为金色高亮
  - 当前上一张牌来源已改为 combat watcher 持久记录，而非 `PlayPile` 短暂状态
  - 升级后伤害 `9 -> 13`
- `MusicOfTheCelestialSphere`
  - 按压力层数向下取整削减力量
  - 升级后阈值 `5 -> 4`
- `KillKiss`
  - 敌方回合开始时，对压力严格高于当前生命值一半的敌人造成伤害
  - 当前已拆成普通版 `KillKiss` 与升级版 `KillKiss+` 两种 power 表现
  - 升级版会覆盖普通版；普通版不会反向降级已存在的升级版
  - 升级后伤害从固定 `25` 改为目标最大生命值
- `BlackBirthday`
  - 单体判断压力后回能
  - 基础获得 `1` 点能量
  - 目标压力大于 `5` 时额外获得 `2` 点能量
  - 升级后费用 `1 -> 0`
- `ChoirSChoir`
  - 先弃 `1` 张牌
  - 再按消耗牌堆当前顺序自动打出其中所有牌
  - 需要目标时按当前合法敌人随机指定
  - 当前无牌可弃时也可正常打出
  - 升级后费用 `3 -> 2`
- `ImprisonedXII`
  - `Unplayable`
  - 每次实际进入手牌时触发抽牌
  - 当前通过 `AfterCardChangedPiles` 监听“进入手牌”事件
  - 额外抽牌使用独立最小 `PlayerChoiceContext` 子类接线
  - 升级后抽牌数 `1 -> 2`
- `GodYouFool`
  - 打出时先 `lose HP`，再抽 `2` 张牌
  - 当前通过反射调用 `Creature.LoseHpInternal(...)`，不走普通伤害链
  - 升级后失去生命值 `3 -> 1`
- `MasqueradeRhapsodyRequest`
  - 对目标施加等同于当前已损失生命值一半、向下取整的压力
  - 基础带 `Exhaust`
  - 升级后移除 `Exhaust`
- `STheWay`
  - 单体攻击 `8`
  - 击杀时抽 `1` 张牌
  - 未击杀时自身失去 `4` 点生命，并使自身与目标各失去 `1` 点敏捷直到回合结束
  - 当前临时减敏通过自定义 `TemporaryDexterityPower` 负面子类实现
  - 压力兑换只跟随真实 `DexterityPower` 数值下降，不再把临时减敏 bookkeeping power 额外算一次
  - 升级后伤害 `8 -> 14`
- `TwoMoonsDeepIntoTheForest`
  - 单体攻击 `20`
  - 基础费用 `7`
  - 本场战斗中每打出 `1` 张 `Song`，费用减少 `1`
  - 当前按原版 `BansheesCry` 路线由卡本体处理：
    - 进入战斗时用战斗历史补算已打出的 `Song` 张数
    - 后续打出新 `Song` 时调用 `EnergyCost.AddThisCombat(-1, false)`
    - 不再通过 watcher power 的 `TryModifyEnergyCostInCombat(...)` 全局改费
  - 升级后伤害 `20 -> 30`
- `Sophie`
  - 获得格挡
  - 对目标施加 `1` 层 `Inferiority`

- `IHaveAscended`
- `Thrilled`
- `Completeness`
- `SheIsRadiant`
- `Notebook`
  - 以上五张新增普通卡已补入 `TogawasakikoCardPool.GenerateAllCards()`
  - 若只写了卡类与本地化、但没进 `GenerateAllCards()`，则不会正常进入角色卡池链路，也可能影响 console 调取
  - `Thrilled` 当前已改为显式能量 var 文本链，避免偏离已精修的小能量 icon
  - `IHaveAscended` 已删去冗余的纯文本 `Exhaust / 消耗`，只保留真正关键词
  - `Completeness` 已恢复升级态描述差异：升级后正文显式显示 `Retain / 保留`，避免卡面升级预览看不出变化
  - `Completeness` 的弃牌选择提示已改为固定“任意张”，不再显示“选择 5 张牌来丢弃”这类上限提示
  - `Notebook` 已接为 `Uncommon` 非 song 单体技能：将目标当前全部【自闭】一次性转化为等量【压力】

当前状态说明：

- 第二批 `8` 张牌已进入角色正常卡池
- 代码与本地化均已接线
- 当前晚间 bugfix 已额外收口：
  - `KillKiss` / `KillKissPower` 的真实 `Entry` 本地化错位
  - reward / shop / `Compose` 的坏牌过滤
  - 战斗奖励少于 `3` 张时的补齐逻辑
- 当前仍需优先做实机验证，尤其是：
  - `ChoirSChoir` 的消耗堆自动打出链
  - `ImprisonedXII` 的各种入手触发口径
  - `GodYouFool` 的 `lose HP` 是否完全不走受伤链
  - `TwoMoonsDeepIntoTheForest` 的本战斗按张数计数减费

补充：

- 当前首批已实现攻击 / 格挡牌的 `DynamicVar` 已统一对齐为原版 `ValueProp.Move`
- 这轮修改覆盖了 starter、首批 song 牌，以及会造成伤害的压力衍生攻击牌
- 2026-03-28 复核结论：
  - `Compose` 以外的大部分升级逻辑其实已经在代码里
  - 之前真正缺的是“卡面升级展示”：
    - 多张牌的本地化描述把数值写死了
    - 导致升级后游戏内看起来像没变化
- 目的是让它们重新进入原版标准的伤害 / 格挡修正链，而不是继续按错误属性结算

## 三 本轮已打通的技术节点

### 1. 编译

当前已直接确认：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj`
  - 已通过

本轮已修掉的直接构建断点包括：

- `Entry.cs` 中对 `CharacterSelectInjector` 的错误引用
- `TogawasakikoPowers.cs` 中 `CombatSide` 缺少正确命名空间
- `Player` 类型缺失引用
- `CardEnergyCost` 构造参数不符合当前 API
- `System.Environment` 与 `Godot.Environment` 歧义
- `GetInstanceId()` 返回值类型与本地集合类型不匹配

当前仍保留少量 nullability / 空引用警告，但没有阻止构建。

### 2. runtime 资源最小闭环

本轮已完成最小闭环所需的 runtime 资源补齐与修线：

- 修正了 `char_select_bg_togawasakiko.tscn` 的背景图引用
- 修正了 PCK 打包链路：
  - `build-mod.sh` 现在会在打包前先跑一次 Godot 导入
  - `pack_pck.gd` 现在会把 `.godot/imported` 一并打进 PCK
- 已把以下对象同步到 `pack/`：
  - 选角图
  - 选角锁定图占位
  - 选角背景图
  - 顶栏头像
  - 顶栏 outline 占位
  - 费用图标
  - 能量球图层
  - 静态战斗立绘
  - starter relic 图
  - starter relic atlas / outline atlas 占位
  - `4` 张 basic 卡图占位
  - `4` 张压力衍生牌卡图占位
  - `4` 个 power / debuff icon 占位
  - transition material 占位

说明：

- 之前“角色头像 / 选角背景 / relic 图没有落地”的直接根因，不是代码入口本身，而是：
  - `png` 资源没有先导入
  - `.godot/imported/*.ctex` 没被打进 PCK
- 当前这条构建断点已修正。

同时，为了先打通第一次最小闭环，已从角色模型的 `ExtraAssetPaths` 中移除当前未接好的可选 runtime 项：

- merchant scene
- rest site scene
- card trail scene

这些对象当前不再作为“首轮进游戏必须存在”的资源位处理。

### 3. 最小导出物

当前已确认：

- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release`
  - 已成功

当前已生成：

- `exports/release/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire.dll`
- `exports/release/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire.pck`
- `exports/release/Togawasakiko_in_Slay_the_Spire/mod_manifest.json`

说明：

- 这只说明当前已经能产出最小 release 候选物
- 还不等于游戏内实际运行已经完全通过

### 4. 最小安装验证

当前已执行：

- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --create-mods-dir`

当前安装目标为：

- `SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire/`

当前已确认游戏目录内存在：

- `Togawasakiko_in_Slay_the_Spire.dll`
- `Togawasakiko_in_Slay_the_Spire.pck`
- `mod_manifest.json`

## 四 当前仍未确认的事项

### 1. 当前已确认的游戏内闭环

当前已经实机确认：

- 角色按钮可出现在选人界面
- 角色可被选中
- 角色可正常进入首场战斗
- starter relic、初始卡组与基础战斗流程已经跑通

当前仍需要继续验证的内容：

- 新增 `song` 牌与事件型 power 的边界行为
- 固定池生成逻辑是否始终稳定
- 多段伤害、自动打牌、敌方回合触发类机制是否有遗漏异常
- 商店与涅奥奖励是否已从“全局 room patch”切回原版分离 hook 后恢复正常
- `Imprisoned XII` 的正确 `ModelId` 修复后，是否已同时消除 `Broken Card / Compose 抽歌异常 / 奖励数量失真`

### 2. 规则与代码仍有一处待对齐

T2 冻结文档里把当前 `4` 张压力衍生牌都写成：

- 默认虚无
- 默认消耗

当前代码里已明确接上：

- 默认消耗

但“默认虚无”仍需要在后续实机前再补确认，不应写成已验证完成。

## 五 2026-04-16 全局 debug 审查回收

本轮先做了一次不改代码的全局风险审查，再按明确问题逐条回收，避免继续靠零散聊天记录判断当前稳定性。

这次确认并已落地修补的点：

- `TogawasakikoCombatWatcherPower.AfterPowerAmountChanged(...)`
  - 原先会把压力衍生牌发给 `applier?.Player ?? cardSource?.Owner`
  - 这在多人局里可能把 Sakiko 的衍生牌错误发给队友
  - 当前已改为始终发给 watcher 自己的 `Owner.Player`
- `Togawasakiko.CreateReplacementCard(...)`
  - 原先无论奖励还是商店替换都走 `CardFactory.CreateForReward(...)`
  - 当前已按 `CardCreationOptions.Source` 分流：
    - `Shop` 走 `CardFactory.CreateForMerchant(...)`
    - 其他来源继续走 `CreateForReward(...)`
- `TogawaEventRoomPatches.SetTogawaDescriptionSafely(...)`
  - 原先一旦 `LocString` 缺 key 就直接 `return false`
  - 这会把“缺少本地化”变成“房间描述静默空白”
  - 当前已改为：
    - 先写日志
    - 然后放行原版 `SetDescription(...)`
- merchant scene 文档口径已重新和源码对齐：
  - 当前运行时确实使用 `res://scenes/merchant/characters/togawasakiko_merchant.tscn`
  - 不再继续写成“已回退到原版 silent merchant”
- `CardFactory.CreateForMerchant(...)` 的 Harmony finalizer 也进一步收窄：
  - 不再兜住所有 `InvalidOperationException`
  - 当前只处理原版已确认那条 merchant rarity 缺桶异常：
    - `Can't generate a valid rarity for the merchant card options passed.`

另外，本轮也再次复核了构建阶段反复出现的 `.uid` 警告：

- 现象是 `pack/runtime_imports/*.ctex` 在 Godot 导入阶段提示 “Missing .uid file ... re-created from cache”
- 当前能确认的事实只有：
  - `.import` 文件本身有 source asset 的 `uid=...`
  - `.godot/imported/` 与可见 `runtime_imports/` 中都没有现成 `.uid` sidecar
  - 当前构建脚本是在可见化 runtime import 产物时重建 `runtime_imports/`
- 因为还没有证据证明“自动伪造 `.uid` 文件”比现状更安全，所以这条当前暂记为：
  - 非阻断构建噪音
  - 暂不盲修
  - 后续若要继续处理，应先基于 Godot 对 `.uid` sidecar 的真实契约再动脚本

## 六 当前已知风险

### 1. 首批正常卡池牌虽然已实现，但仍未做完整实机覆盖

- `Compose / Ave Mujica / Ether / Crucifix X / Face / Saki, Move Plz / Music of the Celestial Sphere / KillKiss`
  当前都需要逐张进战斗验证

### 2. 资源虽然已经补齐最小路径，但大量仍是占位

- `4` 张 basic / generated 卡图当前仍是占位
- 首批 `8` 张正常卡池牌当前仍复用共用占位卡图
- 原有 `4` 个 power / debuff icon 当前仍是占位
- 新增 `3` 个事件型 power icon 当前也只是占位复制
- 顶栏 outline 当前仍是占位
- 选角锁定图当前仍是占位

### 3. 当前虽已可玩，但卡池仍明显不完整

- 仍不是完整角色内容
- 仍不应进入平衡调整阶段

### 4. 商店 / 奖励链已再次回收实现方式，仍需优先复测

- `CardCreationOptions.ForRoom(...)` 的全局 Harmony patch 已删除
- `Togawasakiko : CharacterModel` 当前对 reward / merchant 的干预口径是：
  - 已移除 `ModifyCardRewardCreationOptions(...)`
  - `TryModifyCardRewardOptionsLate(...)`
    在原版奖励结果已生成后，仅替换 `starter` 漏入项
  - `ModifyMerchantCardCreationResults(...)`
    在原版商店角色牌结果已生成后，仅替换 `starter` 漏入项
  - 2026-03-31 新增恢复 `ModifyMerchantCardPool(...)`
    - 仅前置过滤成真正可卖的角色牌
    - 不再把 `Basic / starter / 缺本地化` 卡留到 merchant 生成阶段
  - 2026-03-31 新增 `CardFactory.CreateForMerchant(...)` 兜底 patch
    - 当某个 `CardType` 缺少被抽中的稀有度时，按现有桶回退
- 替换逻辑当前按来源分流，不再把商店替换也塞进 reward 工厂：
  - `Shop -> CardFactory.CreateForMerchant(...)`
  - 其他来源 -> `CardFactory.CreateForReward(player, 1, options)`
  - 目标仍是保留原版奖励数量、稀有度 odds、商店价格与绝大多数库存装配链
  - 避免再把商店、战斗奖励、涅奥奖励绑在同一层粗粒度 pool override 上
- 商店角色展示当前仍走 Sakiko 自己的 merchant scene：
  - 为了先占位可见，视觉层暂时直接使用战斗立绘
  - 真正的 merchant 专属资源后续再替换
- `ModifyCardRewardCreationOptionsLate(...)` 现也已补上 starter 过滤：
  - 目标是覆盖涅奥卡包 / 战斗奖励等不同奖励来源
  - 但是否足以完全消除 starter 泄漏，仍需实机复测
- `Imprisoned XII` 的内部类名已改为 `ImprisonedXii`：
  - 目的是让 `Slugify` 自然生成正确 `ModelId`：`CARD.IMPRISONED_XII`
  - 用于修复当前已确认的 `Broken Card` 根因
- 火堆 scene 也已从 silent 占位回收到 Sakiko 上传图：
  - 仍保留原版兼容节点结构
  - 但视觉层已改为显示 `rest_site_character_togawasakiko.png`
- 因此当前口径应写成：
  - “reward / merchant 接入方式已再次收窄”是已完成事实
  - “战斗奖励与商店已彻底修复”仍需实机复测，不应提前写成闭环

## 七 下一步最适合继续做的事

1. 从 Steam 继续逐张验证首批 `song / 压力联动` 牌
2. 优先确认：
   `Compose / Ave Mujica / Ether / Crucifix X / Face / KillKiss`
3. 若日志暴露固定池、自动打牌或敌方回合事件异常，再按具体报错回收
4. 在名字和效果继续冻结后，再按表格逐步扩正常卡池

## 七 建筑师事件状态

- 当前已补上两处原版兼容前提：
  - `Togawasakiko.GetArchitectAttackVfx()` 已提供非空攻击 VFX 列表。
  - `TheArchitect` 的角色对白入口已通过 Harmony patch 为 `TOGAWASAKIKO -> SILENT` 做临时映射。
- 这次修法刻意贴近原版：
  - 不手写“累计伤害打给建筑师/建筑师反击/强制退主菜单”的自定义流程。
  - 先恢复原版 Architect 事件依赖的对白驱动攻击节奏与角色攻击 VFX。
- 当前仍需实机确认：
  - 三阶段 boss 后的 Architect 事件是否已恢复完整结算链。
  - 若仍卡死，再继续向原版 `TheArchitect` 后续进度/结算分支下钻，而不是旁路重写。

## 八 新增非歌曲牌状态

- 当前已新增 4 张非歌曲牌：
  - `I Have Ascended / 我已成神`
  - `Thrilled / 兴奋不已`
  - `Completeness / 完美无缺`
  - `She Is Radiant / 她在发光`
- 其中 `I Have Ascended` 明确不是复制一张“功能相似牌”：
  - 已本地核对原版可直接调用的类名是 `MegaCrit.Sts2.Core.Models.Cards.Apotheosis`
  - 当前实现通过 `CombatState.CreateCard<Apotheosis>(owner)` + `CardPileCmd.Add(...)` 把原版牌加入战斗中的目标牌堆
  - 升级前进弃牌堆，升级后进手牌
- `Thrilled` 与 `Completeness` 都走原版手牌弃牌选择链：
  - `Thrilled` 只允许丢弃手中的歌曲牌；若手里没有歌曲牌，只获得能量
  - `Completeness` 走 `0..手牌数量` 的可确认弃牌选择，再按“实际弃牌数”抽牌并施压
- 稀有度对齐：
  - `Thrilled / 兴奋不已` 与 `Completeness / 完美无缺` 已从误写的 `CardRarity.Ancient` 改回 `CardRarity.Uncommon`
  - 当前口径里“非凡”应按 `Uncommon` 理解，不是 `Ancient`
- 当前仍需实机确认：
  - `Completeness` 的“弃 0 张牌并确认”在手柄/键鼠 UI 下是否完全正常
