# T4 Bug 修复轮记录

日期：2026-03-27

补充更新：2026-03-29 晚间

## 一 本轮目标

本轮只处理首批已实现对象里的已知 bug，不扩新牌、不扩新机制。

优先级按当前线程要求固定为：

1. 角色牌未进入原版伤害 / 格挡修正链
2. `CrucifixX` 不是 AOE `X` 费
3. `SakiMovePlz` 条件判定与高亮失败
4. `4` 张压力衍生牌无法通过 console 调用

## 二 当前 bug 列表

### 0. 2026-03-29 晚间补充：第二批 song 牌接入后，奖励 / `Compose` / 商店牌位出现坏牌污染

- 当前状态：
  - 代码已补，已重新构建并安装
  - 待实机复测
- 新确认结论：
  - 第二批 `song` 牌接入后的坏牌污染，不只是“个别新卡没写 key”，还叠加了一个更根本的问题：
    - `ModSupport` 的静态初始化会在 `LoseHpInternal` 反射失败时直接抛错
    - 这会让 `EnsureLocalizationOverrides()` 整体提前失败
    - 结果是第二批牌即使在源码字典里已经有 key，也根本没被写进 `user://localization_override`
  - 在这个前提下，实际注册的 `Entry` 与本地化 key 还额外至少存在一处错位：
    - `KillKiss` 的真实 `Entry` 是 `KILL_KISS`
    - 旧本地化接线里仍保留了 `KILLKISS`
  - 这会让 reward / shop / `Compose` 一旦抽到该牌，就有概率落成损坏 card 或触发后续卡组遍历保护
  - 旧实现还只做了“替换不合格结果”，没有补回被污染后缺失的 reward 数量，因此会出现战斗奖励少于原版 `3` 张
- 本轮修改：
  - 把 `LoseHpInternal` 反射从 `ModSupport` 类型初始化阶段拆出，改为运行时惰性解析
    - 避免 `EnsureLocalizationOverrides()` 再被一并拖死
  - 补齐 `KILL_KISS.title / description`
  - 补齐 `KILL_KISS_POWER.title / description`
  - 保留旧 `KILLKISS*` key 作为兼容 alias，避免用户本地 override 残留时继续报错
  - 新增统一过滤：
    - `ModSupport.HasCardLocalization(card)`
  - `Compose` 的 `song` 池现在只从：
    - 已实现 `ISongCard`
    - 且已具备本地化 key
    的牌中抽取
  - `starter / song` 判定从“按 C# 类型”进一步收口为“按稳定的 `Id.Entry` 集合”
    - 降低运行时克隆 / 代理对象导致判定失效的风险
  - 压力衍生牌显式生成池也同步加了本地化过滤
  - 角色 reward / shop 候选牌池现在同时要求：
    - 非 starter
    - `ShouldShowInCardLibrary == true`
    - 已具备本地化 key
  - 奖励修补逻辑新增“补齐到 `3` 张”步骤，尽量恢复原版 reward 数量
- 需要复测：
  - 普通战斗奖励是否恢复为 `3` 张
  - 是否不再出现 `Broken Card`
  - 牌组查看、火堆升级等“遍历卡组”流程是否恢复
  - `Compose` 是否不再生成坏牌

### 0.1 2026-03-29 深夜补充：`KillKiss` / `Black Birthday` / 火堆立绘继续收口

- 当前状态：
  - 代码已补，已重新构建并安装
  - 待实机复测
- 新确认结论：
  - `KillKiss` 之前挂在 `BeforeSideTurnStart`
  - 原版回合启动链在这个钩子之后还会继续跑：
    - `Creature.AfterTurnStart(...)`
    - `ClearBlock`
    - `AfterBlockCleared`
  - 如果 `KillKiss` 在这里先打死人，就会污染“本回合要启动的敌人列表”，这与用户日志里落在 `ShouldClearBlock / ClearBlock / StartTurn` 的异常位置是一致的
  - `Black Birthday` 的能量文本报错不是贴图本体缺失，而是格式串写错：
    - 之前错误使用了 `energyIcons(1) / energyIcons(2)`
    - 该描述又手写了一次普通文本“消耗”，与真实 `Exhaust` keyword 冗余
  - 火堆立绘过小不是资源分辨率不够：
    - `rest_site_character_togawasakiko.png` 本体已是 `2048x2048`
    - 问题出在 `togawasakiko_rest_site.tscn` 里 portrait 的缩放过小
- 本轮修改：
  - `KillKissPower` 从 `BeforeSideTurnStart(...)` 改到 `AfterSideTurnStart(...)`
    - 先让原版敌方回合启动链把本回合对象列表处理完，再结算致死伤害
    - 同时改为通过 hook choice context 执行伤害，避免空上下文调用
  - `BlackBirthday.description` 改为纯文本能量描述：
    - 英文：`Gain 1 Energy... gain 2 more Energy`
    - 中文：`获得1点能量……再获得2点能量`
    - 不再内嵌会报错的能量 selector
    - 不再额外手写“消耗”
  - `EnsureLocalizationOverrides()` 额外补了 `LocManager.Instance != null` 防守
    - 避免 override 文件已写入但语言管理器尚未初始化时再次抛 `NullReference`
  - `togawasakiko_rest_site.tscn` 中 portrait scale / position 已上调
- 需要复测：
  - `KillKiss` 杀死怪物后，战斗是否正常进入奖励
  - `Black Birthday` 是否不再显示坏掉的能量 selector 文本
  - 火堆角色立绘尺寸是否回到可接受范围

### 0.2 2026-03-30 补充：`KillKiss` 胜利结算卡死与 `Black Birthday` 小能量图标

- 当前状态：
  - 代码已补，待实机复测
- 新确认结论：
  - 这次 `KillKiss` 剩余卡死已经不是“伤害命令没跑完”或“敌方回合 hook 太早”：
    - 最新 `godot.log` 已明确落到 `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch(...)`
    - 原版这个方法只枚举内置角色：
      - `Ironclad / Silent / Regent / Defect / Necrobinder / Deprived`
    - 对 mod 注入的 `Togawasakiko` 会直接抛 `ArgumentOutOfRangeException`
    - 一旦 `KillKiss` 在敌方回合开始阶段直接打出战斗胜利，胜利收尾会立刻撞上这条原版只认内置角色的 epoch 统计分支
  - `Black Birthday` 上一轮改成纯文本虽然能避开 selector 报错，但并没有对齐原版文本表现
    - 原版卡牌 / 遗物 / 对话中的小尺寸能量图标，走的是 `EnergyVar + energyIcons()` 链
    - 不是牌左上角的大能量底图
- 本轮修改：
  - 新增 `ProgressSaveManager` Harmony patch：
    - `CheckFifteenElitesDefeatedEpoch`
    - `CheckFifteenBossesDefeatedEpoch`
  - 对非原版角色直接跳过这两段 base-game-only epoch 统计
    - 保留正常战斗胜利与 reward 流程
    - 避免自定义角色在进度统计阶段抛异常卡住房间收尾
  - `BlackBirthday` 改回原版风格的小能量图标描述：
    - 文本恢复为 `energyIcons()` selector
    - 通过 `EnergyVar(1)` 与 `EnergyVar(\"BonusEnergy\", 2)` 提供动态值
    - 不再依赖大尺寸能量图标资源替代
- 需要复测：
  - `KillKiss` 击杀最后一只怪后，是否直接正常弹出奖励
  - 不保存退出重进的情况下，房间是否可正常结束
  - `Black Birthday` 卡牌文本是否显示为原版尺寸的小能量图标

### 0.3 2026-03-30 补充：全局文本能量 icon 误接到大卡牌图

- 当前状态：
  - 代码已补，待实机复测
- 新确认结论：
  - 之前的问题并不只发生在 `Black Birthday`
  - 当前 mod 曾把 `EnergyIconHelper.GetPath("togawasakiko")` 全局指到：
    - `res://images/ui/card/energy_togawasakiko.png`
  - 这是卡牌左上角费用图的大图资源
  - 结果是所有对白 / 事件 / hover / 说明文本里，只要插入祥子的能量 icon，就会被巨型 icon 挤压排版
- 本轮修改：
  - 保留 `CardPoolModel.EnergyIconPath -> res://images/ui/card/energy_togawasakiko.png`
    - 继续服务卡牌左上角费用图
  - 把 `EnergyIconHelper.GetPath("togawasakiko")` 改为：
    - `res://images/packed/sprite_fonts/togawasakiko_energy_icon.png`
    - 让所有文本内嵌 icon 统一走小尺寸 sprite-font 资源
- 需要复测：
  - `Black Birthday`、hover、先古之民对白等文本中的能量 icon 是否恢复正常比例
  - 卡牌左上角费用图是否保持不变

### 0.4 2026-03-30 夜间补充：boss 收尾卡死 / `Two Moons` 跨战斗减费 / `Inferiority` 二次兑换链 / 文本能量 icon 二次缩小

- 当前状态：
  - 代码与 runtime 资源已补
  - 已重新构建并安装
  - 待实机复测
- 新确认结论：
  - 击杀 Act 1 boss 后卡在无奖励状态，当前根因不是 `KillKiss` 伤害命令本身，而是原版 `ProgressSaveManager.ObtainCharUnlockEpoch(...)`
    - 这段会按 `角色 Entry + 2_EPOCH / 3_EPOCH / 4_EPOCH` 去查原版解锁 epoch
    - 对 `Togawasakiko` 这样的自定义角色会命中不存在的 `TOGAWASAKIKO2_EPOCH`
    - 结果是在战斗已经胜利后，收尾阶段抛异常并卡住奖励拉起
  - `Two Moons Deep Into The Forest` 的“永久减费”更像不是卡本体写坏了永久 cost，而是：
    - `TogawasakikoCombatWatcherPower` 里的“本战斗已打出的不同歌曲数”
    - 跨房间没有被显式清空
    - 导致新战斗仍沿用旧计数
  - `Inferiority` 这次真正的问题不是“减力不该兑换【自闭】”
    - 通过减力量触发【自闭】本来就是当前冻结规则的一部分
    - 实际写错的是【过劳焦虑】的触发时点
    - 冻结文档要求它在“施加自卑时”就尝试消耗 `1` 层压力兑换
    - 不该拖到后续“受伤 -> 减力量”的时点才与【自闭】一起出现
  - 文本能量 icon 上一轮虽然已分流到 `sprite_fonts`
    - 但图片主体仍然偏大
    - 在对白 / 教学 / hover 里依旧会明显撑高行内排版
    - 后续实测又确认不仅主体大小会影响观感，透明画布宽度也会直接参与文本排版
- 本轮修改：
  - 新增 `ProgressSaveManager.ObtainCharUnlockEpoch(...)` Harmony patch
    - 对非原版角色直接跳过 base-game-only 解锁 epoch 查询
    - 避免 boss 战胜利后在进度统计阶段再卡死
  - 回退上一轮对 `Inferiority` 的错误抑制逻辑
    - 恢复“减力量 -> 【自闭】”这条既有兑换链
  - 把【过劳焦虑】前移到“施加 `Inferiority` 时”尝试兑换
    - 当前由 `Sophie` 在成功施加 `Inferiority` 后立刻尝试消耗 `1` 层压力并生成【过劳焦虑】
  - 为 `TogawasakikoCombatWatcherPower` 新增 `ResetCombatState()`
    - 新 combat room 进入时显式清空：
      - 上一张已打出牌记录
      - 本战斗不同名 `song` 计数
    - 同步刷新所有 `Two Moons` 的本战斗费用
  - 将 `pack/images/packed/sprite_fonts/togawasakiko_energy_icon.png`
    - 基于大图的临时小 icon 再次缩小
    - 后续又按实测反馈把整张画布收紧为更接近文本 glyph 的尺寸
    - 继续只供文本链路使用
- 需要复测：
  - Act 1 boss 正常击杀后，是否直接进入奖励而不需小退重进
  - `Two Moons Deep Into The Forest` 在下一场战斗开始时是否恢复为 `4` 费
  - `Inferiority` 是否在施加时就尝试兑换【过劳焦虑】，而后续减力量仍可正常兑换【自闭】
  - 教学 / 对白 / hover 文本中的能量 icon 是否既保持小尺寸，又不再留下过宽空白

### 4. Bug 11：`STheWay` 临时减敏会额外显示 helper power，并重复兑换【自闭】

- 当前状态：
  - 代码已修，待实机复测
- 新确认结论：
  - 当前双重兑换的根因不在 `STheWay` 卡本体连打两次，而在 `TogawasakikoCombatWatcherPower.AfterPowerAmountChanged(...)`
    - 真实的 `DexterityPower` 下降会命中一次压力兑换
    - `TemporaryDexterityPower` 的临时 bookkeeping 变动又被当成第二次可兑换事件
  - 结果就是：
    - 角色与怪物身上会看到一个临时 helper power
    - 同一次“本回合失去敏捷”会多生成 `2` 张【自闭】
- 本轮修改：
  - 删除 `TemporaryStrengthPower / TemporaryDexterityPower` 的压力兑换分支
    - 现在只在真实 `StrengthPower / DexterityPower` 数值下降时兑换【自闭】
  - 将 `TheWayTemporaryDexterityLossPower` 设为不可见
    - 避免在 UI 上暴露原本只用于回合内恢复属性的 helper power
- 需要复测：
  - `STheWay` 未击杀目标时，双方是否仍只各失去 `1` 点敏捷直到回合结束
  - 同一结算里是否只生成 `1` 次【自闭】兑换，而不再出现双份
  - 角色与怪物 power 区是否不再出现多余的临时占位 power

### 5. Bug 12：新增普通卡未进入角色卡池，`AveMujica` power 不能叠层

- 当前状态：
  - 代码已修，待实机复测
- 新确认结论：
  - `IHaveAscended / Thrilled / Completeness / SheIsRadiant` 虽然已实现卡类与本地化
    - 但先前漏加到 `TogawasakikoCardPool.GenerateAllCards()`
    - 这会让它们脱离角色标准卡池，也可能连带影响 console 调取
  - `AveMujicaPower` 先前被实现为 `PowerStackType.Single`
    - 多张同名 power 牌不会像原版可叠层 power 那样重复触发
- 本轮修改：
  - 将上述 `4` 张新增普通卡补入 `GenerateAllCards()`
  - 将 `AveMujicaPower` 改为 `PowerStackType.Counter`
    - 并在回合开始按 `Amount` 逐层重复执行效果
- 需要复测：
  - console 是否已经可以正常调取这 `4` 张新牌
  - 它们是否会正常出现在 reward / 商店 / 图鉴等标准卡池链路
  - `AveMujica` 打出多张后，回合开始是否会按层数重复结算

### 6. Bug 13：`Thrilled` 能量 icon 文本链路错误，`I Have Ascended` / `KillKiss` 文本与 power 状态不同步

- 当前状态：
  - 代码已修，待实机复测
- 新确认结论：
  - `Thrilled` 先前直接把数字和 `{Energy:energyIcons()}` 拼在一起
    - 没有像 `Black Birthday` 那样走显式 `EnergyVar`
    - 文本渲染不稳定，容易偏离已精修的小能量 icon 链路
  - `I Have Ascended` 已有真正的 `Exhaust` 关键词
    - 描述里再手写一段“消耗 / Exhaust”会造成冗余
  - `KillKissPower` 先前把升级态存在单个布尔字段里
    - 普通版与升级版的 power 名称 / 描述无法明确区分
    - 也不适合表达“升级版覆盖普通版、且不会被普通版降级覆盖”
- 本轮修改：
  - `Thrilled` 改为显式 `EnergyVar + UpgradedEnergy` 描述链
  - `I Have Ascended` 删除冗余纯文本 `Exhaust / 消耗`
  - `KillKiss` 拆为 `KillKissPower` 与 `KillKissPlusPower`
    - 升级版打出时会移除普通版并改挂 `KillKiss+`
    - 若角色已持有 `KillKiss+`，后续普通版不会把它降级覆盖
- 需要复测：
  - `Thrilled` 文本里的能量 icon 是否已回到已精修的小图标样式
  - `I Have Ascended` 描述是否只保留关键词 `Exhaust / 消耗`
  - 先打普通 `KillKiss` 再打升级版时，角色脚下是否切换为 `KillKiss+`
  - 持有 `KillKiss+` 后再打普通版，是否保持 `KillKiss+` 不变

### 7. Bug 14：`Completeness` 描述与弃牌提示歧义，`Two Moons` 费用跨战斗仍残留

- 当前状态：
  - 代码已修，待实机复测
- 新确认结论：
  - `Completeness` 先前描述里手写了 `IfUpgraded:show: NL [b]Retain.[/b]`
    - 这会在升级后重复显示纯文本 `Retain / 保留`
    - 也会把多余的 `NL` 暴露进文本
  - `Completeness` 的选择器还在复用系统默认 `DiscardSelectionPrompt`
    - UI 会按“当前最多可选数”渲染成“选择 5 张牌来丢弃”
    - 这和卡牌真正想表达的“任意张”不一致
  - `Two Moons Deep Into The Forest` 先前虽然重置了 watcher
    - 但费用刷新仍只覆盖 combat piles，且没有先把卡本身拉回基础费用
    - 因此跨战斗残留仍可能继续滞留
- 本轮修改：
  - `Completeness` 描述移除手写 `NL + Retain / 保留`
    - 升级后的 `Retain` 仅通过真实关键词显示
  - 新增专用选择提示 `COMPLETENESS_SELECT_ANY.prompt`
    - 固定显示“Discard any number of cards. / 选择任意张牌来丢弃。”
  - `RefreshTwoMoonsCosts(...)` 改为覆盖 `player.Deck + combat piles`
  - `TwoMoonsDeepIntoTheForest.RefreshCostFromCombatState()`
    - 每次先强制回基础费用 `4`
    - 再按本战斗不同 `song` 数重算本场费用
- 需要复测：
  - `Completeness` 升级后描述是否不再出现手写 `NL` 与纯文本 `Retain / 保留`
  - 打出 `Completeness` 时，顶部提示是否固定为“任意张”而不是“5 张”
  - `Two Moons Deep Into The Forest` 在新战斗开始时是否稳定回到 `4` 费

### 8. Card Update：新增 `Notebook`，并用新版设计覆盖旧 `Ether`

- 当前状态：
  - 代码已修，待实机复测
- 新确认结论：
  - `Notebook / 日记本`
    - 当前已接入为 `Uncommon` 非 song 单体技能
    - 效果是读取目标当前全部【自闭】层数，一次性清零，再一次性给予等量【压力】
    - 若目标没有【自闭】，则正常打出但没有数值效果
  - `Ether / 以太`
    - 旧版“击杀后给其他敌人压力”的设计本轮已明确废弃
    - 新版改为稳定单体多段伤害 + 稳定给目标压力
- 本轮修改：
  - 新增 `Notebook`
    - 升级后获得真实关键词 `Retain`
  - 重写 `Ether`
    - 基础：`5 x 2` + 目标 `2` 层压力
    - 升级：`5 x 3` + 目标 `3` 层压力
  - 已把 `Notebook` 补入 `TogawasakikoCardPool.GenerateAllCards()`
- 需要复测：
  - `Notebook` 对有【自闭】与无【自闭】目标时，是否都按预期结算
  - `Ether` 是否已完全不再走“击杀后给其他敌人压力”旧逻辑
  - `Ether` 升级后是否正确变为 `5 x 3` 且给目标 `3` 层压力

### 1. Bug 4：角色牌未进入原版伤害 / 格挡修正链

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - 多张攻击牌与格挡牌的 `DynamicVar` 之前使用了 `(ValueProp)0`
  - 原版对应牌的 `DamageVar / BlockVar` 使用的是 `ValueProp.Move`
  - `Ether` 还直接把每段伤害硬编码为 `3`
- 本轮修改：
  - 把以下对象的 `DamageVar / BlockVar` 改为 `ValueProp.Move`
    - `StrikeTogawasakiko`
    - `DefendTogawasakiko`
    - `Slander`
    - `Unendurable`
    - `Ether`
    - `CrucifixX`
    - `Face`
    - `SakiMovePlz`
    - `AllYouThinkAboutIsYourself`
  - `Ether` 改为通过 `DynamicVars.Damage.BaseValue` 结算每段伤害
- 需要复测：
  - 角色带 `Weak` 时，上述攻击牌的伤害是否正常降低
  - 角色带 `Frail` 时，`DefendTogawasakiko / Unendurable / Face` 的格挡是否正常降低

### 2. Bug 1：`CrucifixX` 目标逻辑错误

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - 之前 `TargetType` 是 `AnyEnemy`
  - 之前 `OnPlay(...)` 只循环攻击 `cardPlay.Target`
  - “额外 `2` 次伤害”也同样只打单体
- 本轮修改：
  - `TargetType` 改为 `AllEnemies`
  - 结算改为原版 `Whirlwind` 风格：
    - `DamageCmd.Attack(...).WithHitCount(totalHits).TargetingAllOpponents(...)`
  - 保留规则：
    - 基础次数 = `X`
    - 满足全场压力条件时额外 `2` 次
    - `X = 0` 且满足条件时仍可打出额外 `2` 次
- 需要复测：
  - `CrucifixX` 是否不再需要单体目标
  - `X` 段基础伤害是否对所有敌人生效
  - 全场压力满足条件时额外 `2` 次是否同样作用于所有敌人

### 3. Bug 2：`SakiMovePlz` 条件识别失败

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - 之前只在 `OnPlay(...)` 中临时读取上一张牌
  - 没有覆写 `ShouldGlowGoldInternal`
  - 因此前置高亮不会变金色
- 本轮修改：
  - 新增统一条件函数，直接检查“本回合上一张已打出的牌是否为 `song`”
  - `OnPlay(...)` 与 `ShouldGlowGoldInternal` 共用同一套判定
  - 仍保留：
    - 若这是本回合第一张打出的牌，则不触发
- 需要复测：
  - 本回合第一张打出时是否不高亮、不施加易伤
  - 上一张为 `song` 时，手牌边框是否从蓝色转为金色
  - 满足条件时，是否正常对目标施加 `1` 层 `Vulnerable`

### 4. Bug 3：压力衍生牌无法通过 console 调用

- 当前状态：
  - 代码与安装包已修，待实机复测
- 定位结论：
  - `PersonaDissociationCard / SocialWithdrawalCard` 的类名会把内部 `Entry` 变成：
    - `PERSONA_DISSOCIATION_CARD`
    - `SOCIAL_WITHDRAWAL_CARD`
  - 这与冻结文档和本地化 key 使用的：
    - `PERSONA_DISSOCIATION`
    - `SOCIAL_WITHDRAWAL`
    不一致
  - 同时，压力衍生牌虽然注册进了 `TokenCardPool`，但之前继承的基类仍把它们的 `Pool / VisualCardPool` 指回角色正常卡池
  - `2026-03-27` 晚间 runtime log 进一步确认：
    - 当时实际安装到游戏目录的 DLL 仍是旧导出物
    - 运行中的类型名仍保留 `PersonaDissociationCard / SocialWithdrawalCard`
    - 因此 console 与本地化报错一度继续落在旧 `Entry`
- 本轮修改：
  - 卡牌类重命名为：
    - `PersonaDissociation`
    - `SocialWithdrawal`
  - 保持 `AllYouThinkAboutIsYourself / OverworkAnxiety` 不变
  - 压力衍生牌统一覆写到 `TokenCardPool`
  - 压力衍生牌统一设为“不在卡牌图鉴显示”
  - 已重新顺序执行：
    - `build-mod.sh`
    - `install-mod.sh`
  - 当前安装到游戏目录的 DLL 已确认不再包含旧 `...Card` 类型名
- 预期 console `Entry`：
  - `PERSONA_DISSOCIATION`
  - `SOCIAL_WITHDRAWAL`
  - `ALL_YOU_THINK_ABOUT_IS_YOURSELF`
  - `OVERWORK_ANXIETY`
- 需要复测：
  - `card PERSONA_DISSOCIATION`
  - `card SOCIAL_WITHDRAWAL`
  - `card ALL_YOU_THINK_ABOUT_IS_YOURSELF`
  - `card OVERWORK_ANXIETY`

### 5. 运行时集成补充：火堆 / 商店 / 部分交互房间无法进入

- 当前状态：
  - 代码与资源包已补，待实机复测
- 定位结论：
  - `2026-03-27` runtime log 明确报错：
    - `res://scenes/rest_site/characters/togawasakiko_rest_site.tscn`
    - `res://scenes/merchant/characters/togawasakiko_merchant.tscn`
    均不存在
  - 这会导致：
    - 火堆房进入时抛 `AssetLoadException`
    - 左上角出现不可清除的错误提示
    - 商店和部分依赖角色房间表现的场景高概率同样异常
  - 同轮检查还发现：
    - `pack/images/ui/hands/` 为空
    - 角色的 `multiplayer_hand_*` 贴图此前未进入最终 PCK
- 本轮修改：
  - 新增最小可用场景：
    - `pack/scenes/rest_site/characters/togawasakiko_rest_site.tscn`
    - `pack/scenes/merchant/characters/togawasakiko_merchant.tscn`
  - 把 `4` 张角色手势贴图复制进：
    - `pack/images/ui/hands/`
  - 角色 `ExtraAssetPaths` 同步补登记上述场景与手势贴图
  - 已重新导出并覆盖安装到游戏目录
- 需要复测：
  - 火堆是否正常出现休息 / 升级选项
  - 商店是否能正常进入并交互
  - 左上角错误提示是否消失
  - 相关问号事件若依赖角色表现层，是否不再报缺资源

### 5.1 2026-03-29 晚间补充：商店角色表现层与交互仍不稳定

- 当前状态：
  - 代码已进一步收口，已重新构建并安装
  - 待实机复测
- 新确认结论：
  - 旧 T4 的自定义 merchant scene 虽已落进资源包，但当前最现实的问题不是“有没有文件”，而是“scene 契约是否真的和原版 merchant 房一致”
  - 用户反馈现象包括：
    - 商店角色立绘不显示
    - 无色牌 / relic / 药水 / 删牌无法交互
    - 房间流程无法正常结束
  - 这一组症状更像 merchant 房表现层或节点契约仍在破坏整间房的运行，而不只是角色卡池过滤问题
- 本轮修改：
  - `CharacterModel.MerchantAnimPath` 对祥子角色临时回退到原版：
    - `res://scenes/merchant/characters/silent_merchant.tscn`
  - 当前策略是不再继续赌自定义 merchant scene 的兼容性，先以原版商店场景稳定房间流程
- 需要复测：
  - 商店是否重新显示角色表现
  - 无色牌 / relic / 药水 / 删牌是否恢复可交互
  - 完成购买或离店后，是否能正常进入下个房间

### 6. 新增实机问题：`Face` 触发条件与效果定义不符

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - 旧实现把 `Face` 写成了：
    - 怪物对你造成未被格挡的伤害时
    - 对伤害来源施加 `Weak`
  - 这与当前确认规则不符
  - 现在确认的目标规则应为：
    - 只要怪物的攻击命中你
    - 无论伤害被格挡还是掉血
    - 都对伤害来源施加 `1` 层 `Pressure`
- 本轮修改：
  - `FaceReactionPower` 从施加 `Weak` 改为施加 `Pressure`
  - 触发条件继续基于 `DamageResult.TotalDamage > 0`
    - 因此会覆盖“被格挡但确实命中”的情况
  - `FACE` 与 `FACE_REACTION_POWER` 的本地化描述同步修正
- 需要复测：
  - 有格挡时被攻击，攻击来源是否仍获得 `1` 层 `Pressure`
  - 无格挡掉血时，攻击来源是否同样获得 `1` 层 `Pressure`
  - 是否不再施加 `Weak`

### 7. 新增实机问题：`Persona Dissociation` 伤害乘区计算错误

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - `PersonaDissociationPower.ModifyDamageMultiplicative(...)` 旧实现返回了：
    - `amount * 2`
  - 该接口实际应返回“乘数”，不是“最终伤害值”
  - 这会把原始伤害错误放大为：
    - 原伤害 × `(原伤害 * 2)`
  - 因此会出现：
    - `6` 伤害打出 `72`
    - 怪物意图数字异常膨胀
    - 相关战斗结算出现不合理的全局放大感
- 本轮修改：
  - 将返回值改为固定乘数：
    - 条件满足时返回 `2m`
    - 否则返回 `1m`
  - 该轮曾把移除条件收紧为：
    - 仅在 `result.UnblockedDamage > 0` 时移除
  - 这条口径现已被 2026-04-06 规则替换：
    - 人格解离按格挡前伤害翻倍
    - 即使最终被完全格挡，仍会消耗 `1` 层
- 需要复测：
  - 对带 `人格解离` 的目标使用 `StrikeTogawasakiko`，`6` 伤害是否正常变为 `12`
  - 怪物攻击意图是否恢复正常数量级
  - 没有 `人格解离` 的单位是否不再受到额外放大
  - 完全被格挡时，`人格解离` 是否仍保留

### 8. 新增实机问题：原创 debuff token 卡缺少 hover tip 解释

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - `PersonaDissociation` 与 `SocialWithdrawal` 卡面描述会提到原创 debuff
  - 但卡牌本身没有额外 `HoverTip`
  - 因此鼠标停留时不会弹出对应 debuff 的解释性说明
- 本轮修改：
  - 为 `PersonaDissociation` 增加 `PersonaDissociationPower` 的 hover tip
  - 为 `SocialWithdrawal` 增加 `SocialWithdrawalPower` 的 hover tip
  - 复用对应 power 的标题、描述与图标
- 需要复测：
  - 悬停 `人格解离` 牌时，是否出现 `人格解离` 效果说明
  - 悬停 `自闭` 牌时，是否出现 `自闭` 效果说明

### 9. 新增实机问题：问号房 transform 可能把压力衍生牌混入牌组并卡死后续流程

- 当前状态：
  - 代码已修，待实机复测
- 现象摘要：
  - 问号房变形 `2` 张牌时，`1` 张正常，另 `1` 张显示成白色发光素材并卡在屏幕上
  - 此后战斗中再发生“抽牌 / 临时加手牌”时，也可能继续卡住战斗流程
- 定位结论：
  - 当前压力衍生牌虽然已经归入 `TokenCardPool`
  - 但它们仍默认允许进入“modifier 驱动的随机生成链路”
  - 问号房 / relic 的随机 transform 结果大概率就是沿这条候选池把 token 牌抽了进去
  - `PersonaDissociation` 还额外覆写了 `HasBuiltInOverlay => false`，是当前最可疑的白色发光卡面来源
- 本轮修改：
  - 为 `GeneratedPressureCard` 显式覆写：
    - `CanBeGeneratedByModifiers => false`
  - 移除 `PersonaDissociation` 上单独的 `HasBuiltInOverlay => false`
- 需要复测：
  - 问号房 / `Astrolabe` / `Pandora's Box` 一类 transform 效果是否还会抽到压力衍生牌
  - `Compose`、`Inferiority`、`TogawasakikoCombatWatcherPower` 生成的临时牌是否仍能正常加入手牌
  - 是否还会出现白色发光卡面停留在屏幕上并阻塞流程

### 10. 新增实机问题：`Ether` 击杀后卡在屏幕中央，且未正常对其他敌人施压

- 当前状态：
  - 代码已修，待实机复测
- 现象摘要：
  - `Ether` 击杀目标后，牌面会停在屏幕中央
  - 后续“对其他敌人施压”的追加效果没有正常发生
  - 当前文案也没有明确写出基础压力层数
- 定位结论：
  - 旧实现是在目标死亡后，继续以“被击杀目标”作为“枚举其他敌人”的基准
  - 这条链路在击杀时序下不稳，最可能导致后续追加效果没有正常结算
  - 当前代码口径与冻结文档一致：
    - 基础效果是击杀后给所有其他敌人 `1` 层压力
    - 升级不加伤害，只把压力 `1 -> 2`
- 本轮修改：
  - `Ether` 改为始终以施放者为基准枚举当前存活敌人，再排除被击杀目标
  - 英文 / 中文描述同步改成明确的 `1` 层压力
- 需要复测：
  - `Ether` 击杀后，牌面是否不再停在屏幕中央
  - 若场上还有其他敌人，是否都会正常获得 `1` 层压力
  - 升级后是否改为给予 `2` 层压力，而不是提高伤害

### 11. 新增实机问题：多张牌升级后游戏内看起来没有变化

- 当前状态：
  - 代码已修，待实机复测
- 定位结论：
  - 复核后，`Compose` 以外并不是“升级逻辑都没写”
  - 当前已存在的升级实现包括：
    - `StrikeTogawasakiko` 伤害 `6 -> 9`
    - `DefendTogawasakiko` 格挡 `5 -> 9`
    - `Slander` 费用 `1 -> 0`
    - `Unendurable` 格挡 `5 -> 8`
    - `AveMujica` 费用 `3 -> 2`
    - `Ether` 击杀后压力 `1 -> 2`
    - `CrucifixX` 每段伤害 `6 -> 7`
    - `Face` 格挡 `9 -> 13`
    - `SakiMovePlz` 伤害 `9 -> 13`
    - `MusicOfTheCelestialSphere` 压力阈值 `5 -> 4`
    - `KillKiss` 伤害 `25 -> 目标最大生命值`
  - 真正的问题是卡牌本地化描述大量写死了基础数值
  - 因此升级后即使机械效果已变化，卡面文本仍看起来像未升级
- 本轮修改：
  - 把 starter 与首批 song 牌的描述改成原版模板：
    - `Damage:diff()`
    - `Block:diff()`
    - `IfUpgraded:show:...|...`
  - 同步补上 `Ether / Music / KillKiss` 这类“非纯数值升级”的卡面展示
- 需要复测：
  - 升级后的 `Strike / Defend / Face / SakiMovePlz / CrucifixX` 是否显示正确升级数值
  - `Ether / Music / KillKiss` 是否会在升级后显示正确的升级文本
  - `Unendurable` 是否显示为 `8` 格挡，而不是旧文档残留的 `9`

## 三 本轮未完成项

- 当前没有新的代码硬阻塞
- 以上 `11` 类 bug 仍缺实机验证结论
- 本轮还没有更新：
  - 游戏内复测截图
  - console 实测记录

### 12. 新增复测反馈：`SakiMovePlz` 上一张 `song` 识别不稳定，`CrucifixX` 提前清场后卡死

- 当前状态：
  - 代码已二次修正，待实机复测
- 现象摘要：
  - `SakiMovePlz` 只会瞬间闪金边，随后失去高亮
  - 命中条件时也仍可能不稳定，表现为未正确识别上一张 `song`
  - `CrucifixX` 在剩余攻击次数未打完前击杀全场时，会卡死战斗流程
- 定位结论：
  - `SakiMovePlz` 之前虽然补了统一判定，但“上一张牌”的来源仍依赖 `PlayPile`
  - 若上一张 `song` 会快速离开 `PlayPile`，例如被消耗或被其他流程移走，金边条件就只会短暂成立
  - `CrucifixX` 之前改成 `TargetingAllOpponents(...).WithHitCount(...)` 风格后，仍可能在“中途全体死亡”时把剩余攻击波次挂在无效目标链上
- 本轮修改：
  - 在 `TogawasakikoCombatWatcherPower` 中新增本回合上一张已打出牌的持久记录
  - 通过 `AfterCardPlayed(...)` 记录玩家上一张实际打出的牌
  - 通过 `AfterPlayerTurnStartEarly(...)` 在新回合开始时清空记录
  - `SakiMovePlz` 的条件判定改为读取 watcher 中的上一张已打出牌，而不再读 `PlayPile`
  - `CrucifixX` 改为手动逐波结算：
    - 每一波开始时重新抓取当前所有存活敌人
    - 若场上已无存活敌人，则直接结束后续波次
    - 从而避免剩余攻击次数挂在已失效目标上
- 需要复测：
  - 先打 `Compose`，再观察 `SakiMovePlz` 是否稳定保持金色高亮直到打出或条件改变
  - 命中条件时，`SakiMovePlz` 是否稳定施加 `1` 层 `Vulnerable`
  - `CrucifixX` 在多段过程中清空全场后，是否直接正常结束而不再卡死

### 13. 新增复测反馈：`Compose` 生成歌曲牌时卡住战斗流程

- 当前状态：
  - 代码已修，待实机复测
- 现象摘要：
  - `Compose` 打出后，随机生成的歌曲牌没有正常加入手牌
  - 生成出的牌会卡在屏幕左上角
  - `Compose` 本身停在屏幕中央，导致整场战斗无法继续结算
- 定位结论：
  - `Compose` 自身只调用共享 helper `GiveRandomSongCardToPlayer(...)`
  - 当前所有“自动生成牌入手”都统一走 `CardPileCmd.AddGeneratedCardToCombat(..., addedByPlayer: true, ...)`
  - 结合现象，高可能是“自动生成入手”被错误标成玩家主动加入，触发了不适配当前流程的展示 / 结算链
  - 同一 helper 还会影响：
    - `AveMujica` 生成压力衍生牌
    - `Pressure / PersonaDissociation / Inferiority / SocialWithdrawal` 等效果生成的 token 牌
  - `Compose` 还会在卡牌真正入手前先把费用改成 `0`
  - 为避免对象尚未完成入手就先触发费用变更，这一步也需要顺序后移
- 本轮修改：
  - 把共享 helper 中的 `AddGeneratedCardToCombat(..., addedByPlayer: true, ...)` 统一改为 `addedByPlayer: false`
  - `GiveRandomSongCardToPlayer(...)` 改为先完成入手，再设置“本场战斗费用变为 `0`”
  - 修改覆盖：
    - `GiveGeneratedCardToPlayer<T>()`
    - `GiveRandomSongCardToPlayer(...)`
    - `GiveRandomPressureGeneratedCardToPlayer(...)`
- 需要复测：
  - `Compose` 打出后，随机歌曲牌是否正常进入手牌而不再卡在左上角
  - `Compose` 自身是否正常结算并离场
  - `AveMujica` 和其它自动生成 token 牌的效果是否仍能正常把牌加入手牌

### 14. 新增复测反馈：商店进入后黑屏但鼠标仍可移动

- 当前状态：
  - 代码 / 资源已修，待实机复测
- 现象摘要：
  - 进入商店后画面黑屏
  - 鼠标仍可移动，说明主线程未完全冻结
  - 更像房间 UI 初始化半途抛错或角色展示 scene 未按约定完成挂载
- 定位结论：
  - 目前没有抓到主游戏运行时异常栈，因此这里只能记录“高可能根因”
  - 这条问题不再只看 merchant scene，本轮复核后还发现另一条高风险点：
    - `RewardGenerationPatches` 之前直接重建了 `CardCreationOptions.ForRoom(...)` 的返回值
    - 这样虽然能把自定义卡池接进去，但也高可能把原版针对 `Shop` 的 flags / rng / 内部筛选链一并抹掉
  - 这能解释：
    - 战斗奖励牌池看似工作
    - 但进入商店时，商店库存生成链可能失配并卡在半初始化状态，表现为黑屏但鼠标仍可移动
  - scene 契约问题仍然保留为一条已修风险：
    - merchant / rest site scene 已改回原版兼容的 `SpineSprite` 占位结构
- 本轮修改：
  - `pack/scenes/merchant/characters/togawasakiko_merchant.tscn`
    - 改成原版兼容的 `Node2D + SpineSprite` 结构
    - 暂时使用原版 `silent` 的 merchant skeleton 资源作占位
  - `pack/scenes/rest_site/characters/togawasakiko_rest_site.tscn`
    - 同样改成原版兼容的 `SpineSprite + ControlRoot + Hitbox + ThoughtBubble` 结构
    - 暂时使用原版 `silent` 的 rest site skeleton 资源作占位
  - 这轮目标不是补 Sakiko 的正式商店 / 火堆动画，而是先恢复房间可进入性和脚本兼容性
  - `RewardGenerationPatches`
    - 不再整包重建 `CardCreationOptions`
    - 改为保留原版 `ForRoom(...)` 返回值，并仅通过 `WithCardPools(...)` 替换牌池
    - 这样保留商店原本的 options / flags / rng / odds 链
  - 同时加入 starter 过滤：
    - `StrikeTogawasakiko`
    - `DefendTogawasakiko`
    - `Slander`
    - `Unendurable`
    不再进入战斗奖励与商店卡池
- 需要复测：
  - 商店是否不再黑屏并能正常显示、退出、购买
  - `中伤 / 难熬 / 打击 / 防御` 是否不再出现在战斗奖励与商店牌池
  - 火堆是否仍保持可交互
  - 其他依赖角色 room scene 的事件房是否还会出现同类黑屏 / 卡住

### 15. 新增复测反馈：商店虽可进入，但角色展示缺失，且无色牌 / relic / potion / 删卡交互异常；涅奥稀有奖励出现升级打击并卡住

- 当前状态：
  - 代码已调整并重新安装，待实机复测
- 新现象摘要：
  - 商店已能进入，但角色 merchant 展示丢失
  - 商店里只有部分角色牌表现正常
  - 无色牌、relic、potion、删卡入口像是“房间打开了但库存链没完整加载”
  - 一层涅奥“获得一张稀有牌”会给出一张金色打击，并卡住整个流程
- 本轮进一步定位结论：
  - 原版并不是单靠 `CardCreationOptions.ForRoom(...)` 同时覆盖商店与卡牌奖励
  - `sts2.dll` 已确认存在独立 hook：
    - `AbstractModel.ModifyMerchantCardPool(...)`
    - `AbstractModel.ModifyCardRewardCreationOptions(...)`
    - `Hook.ModifyMerchantCardPool(...)`
    - `Hook.ModifyCardRewardCreationOptions(...)`
  - `MerchantInventory` 也有独立库存填充流程：
    - `PopulateCharacterCardEntries()`
    - `PopulateColorlessCardEntries()`
    - `PopulateRelicEntries()`
    - `PopulatePotionEntries()`
    - `CardRemovalEntry`
  - 因此此前那层全局 `RewardGenerationPatches -> CardCreationOptions.ForRoom` patch 属于改面过宽：
    - 它会把商店与奖励链揉在一起
    - 这很可能正是“商店能进但库存不完整 / 涅奥稀有奖励污染”的共同根因
  - 另一个独立问题是：
    - `CharacterModel.MerchantAnimPath / RestSiteAnimPath / EnergyCounterPath` 是非虚 getter
    - 仅把对应 scene 放进 `ExtraAssetPaths` 并不会自动让原版 UI 使用这些路径
    - 这能解释为什么商店角色展示仍会缺失
- 本轮修改：
  - 删除 `RewardGenerationPatches.cs`
    - 不再 Harmony patch `CardCreationOptions.ForRoom(...)`
  - `Togawasakiko : CharacterModel`
    - 新增 `ModifyCardRewardCreationOptions(...)`
    - 新增 `ModifyMerchantCardPool(...)`
    - 将 starter 过滤与 Sakiko 奖励牌池约束收回角色模型自身
  - `CharacterModelPatches.cs`
    - 新增 `MerchantAnimPath` getter postfix
    - 新增 `RestSiteAnimPath` getter postfix
    - 新增 `EnergyCounterPath` getter postfix
    - 对 `Togawasakiko` 明确返回：
      - `res://scenes/merchant/characters/togawasakiko_merchant.tscn`
      - `res://scenes/rest_site/characters/togawasakiko_rest_site.tscn`
      - `res://scenes/combat/energy_counters/togawasakiko_energy_counter.tscn`
- 当前口径：
  - “移除全局 room patch，改回角色级 reward / merchant hook”是已完成事实
  - “这能完全修复商店库存与涅奥稀有奖励”仍需实机复测，不应提前写成已闭环
- 需要复测：
  - 商店角色展示是否恢复
  - 商店中的无色牌、relic、potion、删卡是否都恢复可见且可交互
  - 战斗奖励与涅奥稀有奖励是否不再出现 `打击 / 防御 / 中伤 / 难熬`
  - 一层涅奥“获得一张稀有牌”是否不再给出升级打击且不再卡流程

### 16. 新增复测反馈：首战奖励只掉 1 张 `Defend`；商店虽能进入但价格全为 `99`

- 当前状态：
  - 代码已再次调整并重新安装，待实机复测
- 新现象摘要：
  - 第一场战斗后的卡牌奖励数量异常，只有 `1` 张
  - 该奖励仍可能是 `starter Defend`
  - 商店里角色展示缺失、商品不完整、价格全显示默认 `99`
- 这轮进一步定位结论：
  - 问题高概率不在“新 8 张牌加入角色卡池”本身
  - 更高风险点是此前角色级 override 仍然改得太早：
    - `ModifyCardRewardCreationOptions(...)`
    - `ModifyMerchantCardPool(...)`
  - 它们虽然比全局 `ForRoom(...)` patch 收敛，但仍然属于“从源头重建输入池”
  - 反射确认 `CharacterModel` 还存在更晚阶段、更窄范围的入口：
    - `TryModifyCardRewardOptionsLate(...)`
    - `ModifyMerchantCardCreationResults(...)`
    - `ModifyMerchantPrice(...)`
    - `ModifyMerchantCardRarity(...)`
  - 同时确认原版可用：
    - `CardFactory.CreateForReward(player, count, options)`
    用于按原版规则补足被替换掉的卡牌结果
- 本轮修改：
  - `Togawasakiko : CharacterModel`
    - 删除 `ModifyCardRewardCreationOptions(...)`
    - 删除 `ModifyMerchantCardPool(...)`
    - 改为在 `TryModifyCardRewardOptionsLate(...)` 中仅替换 `starter` 漏入项
    - 改为在 `ModifyMerchantCardCreationResults(...)` 中仅替换 `starter` 漏入项
  - 替换逻辑不再手写塞卡：
    - 先保留原版已生成结果
    - 只对 `starter` 结果做定点替换
    - 替换卡通过 `CardFactory.CreateForReward(player, 1, options)` 补出
  - `CharacterModelMerchantAnimPathPatch`
    - 暂时把 Sakiko 商店展示回退到原版：
      `res://scenes/merchant/characters/silent_merchant.tscn`
    - 这一步的目标是先恢复商店可见角色展示，不在这一轮继续赌自定义 merchant scene
- 当前口径：
  - “已撤销源头改池，改成原版生成后晚阶段替换 starter”是已完成事实
  - “这足以完全修复商店库存 / 价格 / 奖励数量”仍需实机复测，不应提前写成闭环
- 需要重新测试：
  - 首战奖励是否恢复为正常数量，且不再出现 `starter Defend`
  - 涅奥稀有奖励是否不再污染为升级 starter
  - 商店角色展示是否恢复可见
  - 商店中的无色牌、relic、potion、删卡是否恢复显示与交互
  - 商店价格是否不再统一显示为默认 `99`

### 17. 新增复测反馈：卡包 / 战斗奖励出现 `Broken Card`；`Compose` 抽歌异常；火堆人物错误显示为 `silent`

- 当前状态：
  - 已完成代码回收与资源修正，待实机复测
- 本轮确认的明确问题：
  - `Broken Card` 的直接根因已定位到新增牌：
    - 原类名 `ImprisonedXII` 会被 `Slugify` 成错误内部 ID：
      `CARD.IMPRISONED_XI_I`
    - 但本地化与冻结文档写的是：
      `CARD.IMPRISONED_XII`
    - 这会导致奖励、卡包、随机生牌时命中该牌后，标题 / 描述 / 检索链错位
    - 这也是当前 `Compose` 随机歌曲牌异常的高概率共同根因
  - 奖励 starter 过滤此前只放在结果替换链，覆盖面仍可能不足：
    - 某些奖励来源可能先于 `TryModifyCardRewardOptionsLate(...)` 就已经定型
    - 因此新增把 starter 过滤前移到 `ModifyCardRewardCreationOptionsLate(...)`
      但仍只是在原版 options 上追加 filter，不再重建 pool
  - 火堆人物显示成 `silent` 不是素材缺失：
    - 仓库里已存在正式资源：
      `incoming_assets/rest_site_character/rest_site_character_togawasakiko.png`
      以及
      `assets/character/rest_site/rest_site_character_togawasakiko.png`
    - 之前把 `silent` 占位沿用到火堆 scene，是为了先保场景兼容时收得过粗，这一条已确认是错误处理
- 本轮修改：
  - 将内部类名 `ImprisonedXII -> ImprisonedXii`
    - 使其自然生成正确 `ModelId`：
      `CARD.IMPRISONED_XII`
  - `Togawasakiko : CharacterModel`
    - 新增 `ModifyCardRewardCreationOptionsLate(...)`
    - 继续保留原版 pool / source / odds，仅追加 starter 过滤
  - `pack/scenes/rest_site/characters/togawasakiko_rest_site.tscn`
    - 保留原版兼容的 rest site 节点结构
    - 新增 Sakiko 静态火堆图 overlay
    - 将原 `silent` spine 占位隐藏
  - 新增运行时火堆图片资源：
    - `pack/images/packed/character/rest_site/rest_site_character_togawasakiko.png`
- 当前未能完全确认的部分：
  - 商店全链路异常是否完全由 `Broken Card` 污染连带引起，当前仍不能写成已定位完成
  - 若修正 `IMPRISONED_XII` 后商店依然出现“价格混乱 / 商品缺失 / 无法完成房间交互”，则说明仍有独立的 merchant inventory 契约问题待拆
- 商店展示占位更新：
  - 已不再把 merchant path 指回原版 `silent_merchant.tscn`
  - 当前恢复为 Sakiko 自己的 merchant scene
  - 但视觉层先按当前冻结口径，临时使用战斗立绘作为占位图
- 需要重新测试：
  - 涅奥卡包奖励是否不再出现 `Broken Card`
  - 战斗奖励是否恢复为正常 `3` 张，且不再出现 `Broken Card`
  - 获取含 `Imprisoned XII` 的奖励后，是否还能正常查看牌组
  - `Compose` 是否能重新稳定加入歌曲牌
  - 火堆是否显示 Sakiko 上传图，而不是 `silent`
  - 商店若仍异常，需要和 `Broken Card` 问题拆开单独复测

## 四 建议下一步复测顺序

1. 先用 `Weak / Frail` 复测 starter 与首批已实现牌的标准数值修正
2. 再复测 `CrucifixX` 的 AOE `X` 费行为，以及“中途清场是否直接收束”
3. 再复测 `SakiMovePlz` 的高亮、上一张 `song` 识别与易伤
4. 复测 `Compose`、`AveMujica` 与其他自动生牌效果，确认入手流程不再卡死
5. 最后用 console 精确验证 `4` 张压力衍生牌
6. 进火堆 / 商店 / 相关事件房，确认角色场景资源链已恢复，且商店不再黑屏
7. 复测 `Face` 在“掉血 / 被格挡”两种场景下都给攻击来源加 `Pressure`
8. 复测 `Persona Dissociation` 是否恢复为正常双倍伤害
9. 复测 `人格解离 / 自闭` token 卡的 hover tip 说明
10. 复测 transform 事件 / relic 是否还会把压力衍生牌混进正常牌组并导致白卡卡死
11. 复测 `Ether` 的新版单体多段 + 目标施压表现
12. 复测各已实现牌的升级后卡面显示与实际效果是否一致

## 六 2026-03-31 商店黑屏补充：merchant 稀有度桶缺失导致库存生成中断

- 新暴露问题：
  - 进入商店后黑屏，但鼠标仍可移动。
  - 最新 `godot.log` 明确报错：
    - `System.InvalidOperationException: Can't generate a valid rarity for the merchant card options passed.`
    - 抛点位于 `CardFactory.CreateForMerchant(...) -> MerchantInventory.PopulateCharacterCardEntries()`
- 新确认结论：
  - 这次不是 merchant scene 资源链，而是商店角色牌库存生成阶段直接中断。
  - `Togawasakiko` 当前卡池在某些 `CardType` 下并不覆盖 `Common / Uncommon / Rare` 全桶：
    - 例如攻击牌没有 `Rare`
    - power 牌没有 `Uncommon`
  - 原版 merchant 一旦对该类型 roll 到缺失稀有度，就会在库存未完成时中断，表现为黑屏但鼠标仍可移动。
  - 此外，若把 `Basic / starter / 缺本地化` 卡继续留到 merchant 生成前，也会增加 merchant 前半段的不稳定性。
- 本轮修改：
  - 恢复 `ModifyMerchantCardPool(...)`
    - 在 merchant 生成前先过滤成真正可卖的角色牌。
  - 新增 `CardFactory.CreateForMerchant(Player, IEnumerable<CardModel>, CardType)` Harmony finalizer
    - 仅对 `Togawasakiko` 生效。
    - 当原版因缺失稀有度抛 `InvalidOperationException` 时，按当前类型可用桶回退到：
      - `Uncommon -> Common -> Rare`
- 需要复测：
  - 进入商店时是否不再黑屏。
  - 日志里是否不再出现 `Can't generate a valid rarity for the merchant card options passed.`
  - 商店角色牌是否恢复正常生成，而不是只靠 scene 层显示成功。

## 五 2026-03-30 建筑师结算兼容补丁

- 新暴露问题：
  - 祥子通关三阶段 boss 后，进入建筑师事件时没有按原版角色那样把本局累计伤害打给建筑师。
  - 建筑师也无法正常反击、击杀玩家、展示结算信息并返回主菜单。
- 对照原版后确认的直接根因：
  - `TheArchitect.LoadDialogue()` 不是“无条件通用流程”。
  - 它会调用 `DialogueSet.GetValidDialogues(owner.Character.Id, ..., allowAnyCharacterDialogues: false)`。
  - 原版 `TheArchitect.DefineDialogues()` 只给内置角色准备了 `CharacterDialogues` 条目。
  - 自定义角色若没有自己的 Architect 对白入口，就拿不到对白，也拿不到对白内的 `StartAttackers / EndAttackers` 节奏标记。
  - 同时，角色还必须提供非空的 `GetArchitectAttackVfx()`，否则玩家侧的建筑师攻击演出链也不完整。
- 本轮修改：
  - `Togawasakiko.GetArchitectAttackVfx()` 不再返回空列表，改为暂时复用原版 silent 风格的匕首/斩击 VFX。
  - 新增 `ArchitectEventPatches.cs`：
    - 对 `TheArchitect.get_DialogueSet()` 做 Harmony postfix。
    - 若当前角色是 `TOGAWASAKIKO` 且 Architect 对白集中没有该角色条目，则把 `TOGAWASAKIKO` 临时映射到 `SILENT` 的 Architect 对白列表。
- 当前策略的边界：
  - 这是“恢复原版建筑师事件完整结算链”的兼容补丁，不是最终的 Sakiko 专属 Architect 演出。
  - 当前不新增 Sakiko 专属 Architect 台词，只借用 Silent 现成对白与攻击节奏，避免继续卡死在胜利后处理链。
- 需要重新测试：
  - 祥子通关三阶段 boss 后，建筑师是否会正常承受本局累计伤害。
  - 建筑师是否会正常反击并结束本局。
  - 是否能正常进入原版结算/主菜单返回链。
