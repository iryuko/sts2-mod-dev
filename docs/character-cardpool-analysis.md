# Character Card Pool Analysis

日期：2026-03-11

## 已确认

- 当前 run 中的角色身份是通过 `Player.get_Character()` 读取的。
- `CharacterModel` 是角色的核心模型类型，并直接暴露：
  - `get_CardPool()`
  - `get_StartingDeck()`
  - `get_RelicPool()`
  - `get_PotionPool()`
- 三个当前重点角色与其卡池绑定关系已在本地 IL 中确认：
  - `Ironclad -> IroncladCardPool`
  - `Silent -> SilentCardPool`
  - `Defect -> DefectCardPool`
  - `Necrobinder -> NecrobinderCardPool`
  - `Regent -> RegentCardPool`
- 正常战后卡牌来源链路已确认：
  - `Player.get_Character()`
  - `CharacterModel.get_CardPool()`
  - `CardCreationOptions.ForRoom(player, roomType)`
  - `CardPoolModel.GetUnlockedCards(unlockState, multiplayerConstraint)`
- `UnlockState.get_CharacterCardPools()` 会把当前已解锁角色映射到各自 `CharacterModel.get_CardPool()`。
- `UnlockState.get_Cards()` 会汇总：
  - 各角色 `CardPoolModel.get_AllCards()`
  - 各角色 `StartingDeck`
- `CardPoolModel.get_AllCards()` 内部明确调用：
  - `MegaCrit.Sts2.Core.Modding.ModHelper.ConcatModelsFromMods<TModelType>(IPoolModel, IEnumerable<TModelType>)`
- `MegaCrit.Sts2.Core.Modding.ModHelper` 明确提供：
  - `AddModelToPool<TPoolType, TModelType>()`
  - `AddModelToPool(Type poolType, Type modelType)`

## 角色与卡牌归属

- 当前最强的运行时归属关系不是“卡牌有一个独立角色 color 字段”，而是：
  - `CharacterModel` 拥有一个 `CardPoolModel`
  - `CardPoolModel` 产出这名角色正常可获得的牌
- `CardModel` 本身也有 `get_Pool()` / `get_VisualCardPool()`，说明单张牌仍保留自身原始池归属与视觉池信息。
- 因此，“把现有牌跨角色加入另一角色卡池”最自然的做法不是改角色 ID，而是把现有 `CardModel` 额外加入目标 `CardPoolModel`。

## 五个角色到卡池的绑定

- `Ironclad -> IroncladCardPool`
- `Silent -> SilentCardPool`
- `Defect -> DefectCardPool`
- `Necrobinder -> NecrobinderCardPool`
- `Regent -> RegentCardPool`

## Epoch / 解锁过滤

- `IroncladCardPool`、`NecrobinderCardPool`、`RegentCardPool` 都覆写了 `FilterThroughEpochs(...)`。
- 这层过滤的实现方式不是“只保留本角色全部 epoch 卡”，而是：
  - 如果某个 epoch 尚未解锁，就从当前列表中移除“该 epoch 明确列出的那些卡”
- 这意味着：
  - 被 mod 额外加入目标角色卡池的外来卡，如果不属于目标角色自身 epoch 列表，默认不会被这层过滤掉。

## 候选干预点比较

### 1. 在卡牌注册阶段补充归属

- 高可能可行，但当前没有证据表明需要改动现有卡类本身。
- 改动面比“只追加进目标卡池”更大。

### 2. 在角色卡池构建阶段插入指定卡

- 已确认可行性最高。
- 直接使用游戏内建扩展点：
  - `ModHelper.AddModelToPool<TPoolType, TModelType>()`
- 会自然进入：
  - 正常战后奖励池
  - 依赖 `CardCreationOptions` 的常规取牌逻辑
- 不依赖 Harmony。

### 3. 在奖励生成阶段动态插入指定卡

- 理论上可行，因为 `CardCreationOptions` 支持 `WithCustomPool(...)` 和 `WithCardPools(...)`。
- 但当前没有找到比池级扩展更小、更稳的官方注入点。
- 若改这一层，后续还要覆盖多个奖励来源。

### 4. 在 run 开始后直接 patch 某个卡池集合

- 不推荐。
- `CardPoolModel.get_AllCards()` 有缓存，而且 mod 内容会在首次读取后冻结。
- `ModHelper` 明确提示：
  - 必须在 game initialized 之前追加内容，否则会抛错。

## 建议

- 建议采用：
  - 在角色卡池构建阶段插入指定卡
- 原因：
  - 改动最小
  - 基于已确认的原生扩展点
  - 不依赖 Harmony
  - 更容易直接影响“正常流程里能抽到/能拿到”的核心目标
