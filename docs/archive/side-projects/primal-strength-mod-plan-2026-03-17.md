# `Primal Force` 生成物接入 Strike 体系：第一版方案

> 档案状态：PrimalForceStrike 旁支方案，不属于当前主线。

## 目标重述

目标不是全局改所有石头牌，也不是把所有 `GiantRock` 一起改成 `Strike`。

真正目标是：

- 只让 `Primal Force` 生成出来的那批牌进入 `Strike` 体系
- 优先吃到当前已确认按 `CardTag.Strike` 判定的联动

## 候选路线比较

## A. 直接全局修改原版 `GiantRock`

优点：

- 改动点少
- 实现可能最短

问题：

- 影响范围大于目标范围
- 会把全部 `GiantRock` 的系统身份一起改掉
- 不符合“只影响 `Primal Force` 生成物”的目标
- 即使补了 `Strike` tag，也仍然进不了 `Basic Strike/Defend` 那条更窄体系

结论：

- 不适合作为第一版首选

## B. 只修改 `Primal Force`，在生成实例上补 `Strike` tag

做法：

- patch `PrimalForce.OnPlay`
- 保留原版“筛手牌 -> 新建 `GiantRock` -> 升级 -> `Transform`”流程
- 让 `Primal Force` 在逐张变形时先检查原牌是否本来就带 `CardTag.Strike`
- 只有原牌本来是 `Strike` 时，才给新生成的那张 `GiantRock` 实例补 `CardTag.Strike`
- 若原牌不是 `Strike`，则继续保持原版 `GiantRock` 身份

优点：

- 改动范围最小
- 只影响 `Primal Force` 生成物中的 `Strike` 来源子集
- 最容易回退
- 与当前目标最一致
- 不需要去 patch 多个消费方
- 不需要新增卡牌类型
- 不需要改 `ModelDb`

约束：

- 当前需要 Harmony patch `PrimalForce.OnPlay`
- 需要确认这条 patch 在本机运行环境里实机稳定

结论：

- 这是当前推荐的第一版路线

## C. 不改生成物，只在消费方里额外把它视为 Strike

做法：

- 去 patch `PerfectedStrike`
- patch `Hellraiser`
- patch `StrikeDummy`
- patch `SoldiersStew`
- 等等

问题：

- 需要同时改多个系统
- 容易遗漏消费方
- 验证成本高
- 回退和维护都更差

结论：

- 不适合作为第一版

## D. 其它更小路线

当前没有发现比 B 更小、同时又能保持边界清晰的路线。

如果后续只想覆盖某一个单独联动，才值得重新评估是否做定点消费方 patch。

## 推荐方案

第一版建议采用：

- B. 只修改 `Primal Force`，让它生成一个专用变体

## 推荐理由

### 改动范围

- 只动 `Primal Force`
- 只影响它生成出来的牌

### 对其它系统的影响

- 不会把全部 `GiantRock` 一起改掉
- 不会把多个消费方都 patch 一遍
- 不会引入新的卡牌类型或新的本地化资源

### 验证难度

- 最容易做单点验证
- 只需要验证：
  - `Primal Force` 是否仍正常把 Attack 变成石头
  - 原本是 `Strike` 的输入，是否会变成带 `Strike` 身份的 `GiantRock`
  - 原本不是 `Strike` 的输入，是否仍变成普通 `GiantRock`
  - 这些带 `Strike` 身份的 `GiantRock` 是否触发 `PerfectedStrike` / `Hellraiser` / `StrikeDummy` / `SoldiersStew`

### 回退成本

- 只需撤回一个 patch 点

### 与目标的一致性

- 最符合“只影响原始力量生成物”的边界
- 同时也更符合“只让原始 `Strike` 的转化产物进入 `Strike` 体系”的目标

## 第一版不承诺覆盖的范围

即使第一版成功，只补 `Strike` tag 也不应默认覆盖：

- `PandorasBox`
- `GhostSeed`
- `NutritiousSoup`
- `LeafyPoultice`

原因是这些对象当前看的是：

- `Basic` rarity
- 加 `Strike/Defend` tag

而不是单纯的 `Strike` tag。
