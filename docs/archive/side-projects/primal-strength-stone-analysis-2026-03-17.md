# `Primal Force` / `Giant Rock` 分析

> 档案状态：PrimalForceStrike 旁支研究，不属于当前主线。

## 名称纠偏

用户口头目标里写的是：

- `Primal Strength`
- `Stone`

但本地资源与程序集中的真实对象是：

- `Primal Force`
- `Giant Rock`

已确认的本地化键：

- `PRIMAL_FORCE.title = Primal Force`
- `GIANT_ROCK.title = Giant Rock`

因此后续实现与文档应优先使用真实对象名，避免继续沿着错名分析。

## `Primal Force` 的生成链路

已确认 `PrimalForce.OnPlay(...)` 的核心流程：

1. 读取施放者手牌
2. 过滤手牌中满足以下条件的卡：
   - `IsTransformable`
   - `Type == Attack`
3. 对每张命中的卡：
   - `CombatState.CreateCard<GiantRock>(owner)`
   - 若当前 `Primal Force` 已升级，则升级新建的 `GiantRock`
   - `CardCmd.Transform(oldCard, newCard, ...)`

结论：

- `Primal Force` 不是“把 Strike 变成石头”
- 它是“把手牌里的全部可变形 Attack 变成 Giant Rock”

## `GiantRock` 的当前定义

已确认：

- `GiantRock` 是 `sealed` 的 `CardModel`
- 当前类里能看到：
  - 构造函数
  - 伤害动态值
  - 普通攻击式 `OnPlay`
  - 升级伤害逻辑
- 当前没有看到它覆盖 `get_CanonicalTags()`

同时，`CardModel.get_CanonicalTags()` 的默认实现返回空 `HashSet<CardTag>`。

因此当前结论是：

- 原版 `GiantRock` 默认没有 `Strike` tag
- 它当前不会被系统自动视为 `Strike` 体系牌

## 可扩展性判断

### 不适合的方向

- 直接继承 `GiantRock`
  - 原因：`GiantRock` 是 `sealed`
- 不加区分地全局改原版 `GiantRock`
  - 原因：会把全部 `GiantRock` 一起改变身份，影响范围超出“只影响 `Primal Force` 生成物”

### 更适合的方向

- 新建一个仅供 `Primal Force` 使用的 `GiantRock` 变体
- 复制原版 `GiantRock` 的战斗表现
- 在变体上补 `Strike` 身份
- 再让 `Primal Force` 在逐张变形时检查原牌是否本来就是 `Strike`
- 只有原牌本来是 `Strike` 时，才生成这个变体
- 如果原牌不是 `Strike`，则继续生成原版 `GiantRock`

这样可以把影响面收敛在：

- `Primal Force`
- 它生成出来、且来源于原始 `Strike` 的那批牌

## 当前边界

本轮只确认了：

- `Primal Force` 如何创建与替换 `GiantRock`
- 原版 `GiantRock` 当前不带 `Strike` tag
- `GiantRock` 不适合走继承式改造

还没有进入正式编码，也还没有最终决定第一版变体的具体类名与实现目录。
