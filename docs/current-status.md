# 当前状态

记录日期：2026-03-12

## 当前阶段

已跨过“mod 是否会被 macOS 版 STS2 识别和加载”这一阶段。

当前进入第一条真实功能 mod 主线：

- 做一个最小“跨角色加卡” mod
- 目标是把一张现有角色卡加入另一角色的正常可获取卡池
- 同时新增一个平行试验支线：
  - 验证“按角色自动补一个现成 relic”的最小实现

## 当前目标

- 先完成单点闭环：
  - `Ironclad` 的 `BodySlam`
  - 加入 `Silent` 的可获取卡池
- 当前主线已收敛为：
  - 只保留 `BodySlam -> SilentCardPool`
  - 不再做运行时直接塞牌
- 先确定：
  - mod 是否正常初始化
  - `Silent` 的正常奖励池里是否稳定会出现 `BodySlam`

## 已确认

- 本仓库是工作区，不是游戏安装目录；真实游戏路径以 `local/game-path.txt` 为准。
- macOS 本机实际扫描到的 mod 目录是：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 游戏已经明确提示加载过我们的 `SmokeMod`，说明当前安装与加载链路可用。
- `UnifiedSavePath` 的 macOS 本地修正版已经实机成功：
  - 公开版失败根因是 Harmony 动态 patch 在本机运行环境中失败
  - 当前活动版改成无 Harmony workaround 后，用户已确认“存档同步成功”
  - 当前工作区已继续收敛为“单一跨平台源码”方案：
    - Windows 走 Harmony patch 模式
    - macOS / 非 Windows 走已验证成功的 flag-thread workaround
- 角色身份读取链路已确认：
  - 当前 run 中通过 `Player.get_Character()` 读取角色
- 角色到卡池链路已确认：
  - `CharacterModel.get_CardPool()`
- 常规战后取牌链路已确认：
  - `CardCreationOptions.ForRoom(player, roomType)`
  - 使用当前角色的 `CardPool`
  - 再通过 `CardPoolModel.GetUnlockedCards(...)` 生成可选卡
- 五个角色与卡池绑定已确认：
  - `Ironclad -> IroncladCardPool`
  - `Silent -> SilentCardPool`
  - `Defect -> DefectCardPool`
  - `Necrobinder -> NecrobinderCardPool`
  - `Regent -> RegentCardPool`
- 游戏内建 mod 扩展点已确认：
  - `MegaCrit.Sts2.Core.Modding.ModHelper.AddModelToPool<TPoolType, TModelType>()`
- `CardPoolModel.get_AllCards()` 已确认会调用：
  - `ModHelper.ConcatModelsFromMods(...)`
  这说明“把额外卡模型加入现有卡池”是游戏原生支持的扩展方向。
- 角色卡池的 epoch 过滤逻辑已确认：
  - 只会移除“该角色自己 epoch 列表里、但尚未解锁”的卡
  - 不会自动移除 mod 追加进去、且不属于该角色 epoch 列表的外来卡
- `Silent::get_StartingDeck()` 已在本地 IL 中确认是硬编码实现：
  - 5x `StrikeSilent`
  - 5x `DefendSilent`
  - `Neutralize`
  - `Survivor`
- 新 run 起始牌组填充链路已确认：
  - `Player.CreateForNewRun(...)`
  - `Player.PopulateStartingInventory()`
  - `Player.PopulateStartingDeck()`
  - `Player.Deck.AddInternal(...)`
- `RunState.get_CurrentRoomCount()` 与 `RunState.get_CurrentRoom()` 可用于区分开局房和后续房间。
- 已新增结论文档：
  - `docs/character-cardpool-analysis.md`
  - `docs/cross-character-card-plan.md`
- 已新增最小功能 mod 子项目：
  - `mods/CrossCharacterCard/`
  - `mods/SilentBonusRelic/`
- `CrossCharacterCard` 当前实现已收敛为单一路径：
  - 初始化时按集中规则表依次注册卡池规则
  - 当前规则集里已有：
    - `BodySlam -> SilentCardPool`
  - 不再订阅 `RunStarted` / `RoomEntered`
  - 不再在运行时直接修改 `Silent` 当前牌组
- `CrossCharacterCard` 当前 release 产物已构建成功：
  - `mods/CrossCharacterCard/exports/release/CrossCharacterCard/CrossCharacterCard.dll`
  - `mods/CrossCharacterCard/exports/release/CrossCharacterCard/CrossCharacterCard.pck`
- `CrossCharacterCard` 已重新安装到游戏目录：
  - `.../Contents/MacOS/mods/CrossCharacterCard/`
- 本地 relic 线索已确认：
  - `Silent::get_StartingRelics()` 默认只有 `RingOfTheSnake`
  - 目标遗物类型 `MegaCrit.Sts2.Core.Models.Relics.SneckoSkull` 存在
  - 官方运行时加遗物入口 `RelicCmd.Obtain<TRelic>(player)` 存在
- 已新增最小 relic 验证 mod：
  - `SilentBonusRelic`
  - 目标是在 `Silent` 开新 run 时作为“第二个起始 relic”立刻补上 `SneckoSkull`
- 用户最新反馈：
  - 上一版 `SilentBonusRelic` 没有实际把 `SneckoSkull` 发给玩家
- 已确认关键事实：
  - `Silent::get_StartingRelics()` 是硬编码，只返回 `RingOfTheSnake`
  - `Player.PopulateStartingRelics()` 在 `RunStarted` 之前就已完成
  - 当前没找到原生 `ModHelper` 级别的 `StartingRelics` 扩展点
- `SilentBonusRelic` 当前实现已继续收敛为：
  - 不再等到首个战斗房间
  - 改成集中规则表，在 `RunStarted` 里给 `Silent` 静默补发起始 relic
  - 当前规则集里已有：
    - `SneckoSkull`
    - `Shuriken`
  - 作为“额外起始 relic”方案的近似实现
- `SilentBonusRelic` release 产物已构建成功：
  - `mods/SilentBonusRelic/exports/release/SilentBonusRelic/SilentBonusRelic.dll`
  - `mods/SilentBonusRelic/exports/release/SilentBonusRelic/SilentBonusRelic.pck`
- `SilentBonusRelic` 已安装到游戏目录：
  - `.../Contents/MacOS/mods/SilentBonusRelic/`

## 高可能

- 当前用户实机反馈表明：
  - `BodySlam` 会作为正常卡牌奖励出现
  - 这与单纯的卡池扩展路径一致

## 待验证

- `CrossCharacterCard` 安装后，游戏是否会正常识别并加载该 mod。
- `CrossCharacterCard` 初始化时是否无异常。
- `BodySlam` 进入 `Silent` 奖励池的频率和稳定性如何。
- 选择后是否能正常进入牌组、抽到并打出。
- `SilentBonusRelic` 改用 `AddRelicInternal(...)` 后，是否会在首个真正战斗房间稳定拿到 `SneckoSkull`。
- `SilentBonusRelic` 改成 `RunStarted` 立即补发后，是否会在新 run 一开始就稳定拥有 `SneckoSkull`。
- `SilentBonusRelic` 新增 `Shuriken` 后，是否会与 `SneckoSkull` 一起稳定出现在 `Silent` 开局 relic 栏里。

## 当前阻塞

- 当前没有新的功能性阻塞。
- 已确认过去的黑屏/流程卡住来自“运行时直接塞牌”实验路径，而不是卡池扩展路径本身。

## 最近完成

- 已把主线从“加载链路研究”切换到“最小功能 mod 验证”。
- 已完成角色 -> 卡池 -> 奖励链路分析。
- 已确认原始最小干预点应选择“在目标角色卡池中追加现有卡”。
- 用户已实机确认：
  - `Silent` 可以在正常流程中把 `BodySlam` 当成卡牌奖励拿到
  - 该行为与 `AddModelToPool<SilentCardPool, BodySlam>()` 的预期一致
- 已将 `CrossCharacterCard` 收敛回“只改卡池、不改当前牌组”的干净版本，并改成集中规则表写法。
- 用户反馈 `SilentBonusRelic` 上一版没有成功发放 `SneckoSkull`。
- 已确认“字面上改 `StartingRelics` 列表”在当前无 Harmony 路线下没有现成原生扩展点。
- 已把 `SilentBonusRelic` 改成 `RunStarted` 立即静默补发的集中规则表版本。
- 当前 `SilentBonusRelic` 规则集已包含：
  - `SneckoSkull`
  - `Shuriken`
- 已把 `UnifiedSavePath` 从“仅 macOS workaround 源码”改成单一跨平台实现：
  - Windows：优先走 Harmony patch 路线
  - macOS / 非 Windows：直接走 flag-thread workaround

## 下一步依赖

- 启动游戏，用 `Silent` 开新 run。
- 继续用正常流程做几轮奖励验证。
- 确认 `BodySlam` 在奖励池中的出现是否足够稳定。
- 如果要扩展到更多跨角色卡，优先沿用同一模式：
  - 在 `CrossCharacterCard` 集中规则表中新增一条规则
- 继续验证 `SilentBonusRelic` 新版是否会在新 run 一开始就稳定拥有：
  - `SneckoSkull`
  - `Shuriken`
