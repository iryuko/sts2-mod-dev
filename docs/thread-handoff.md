# 线程接班摘要

## 这条线程在做什么

当前接班对象不是加载链研究，也不是 `PrimalForceStrike`。

当前接班对象是：

- `mods/Togawasakiko_in_Slay_the_Spire`

线程目的不是重写角色，而是接着 T4 中断前的实现，把已经落仓的角色 mod 稳定下来。

## 当前主判断

- 角色已经不是“只有骨架”的状态
- 角色已接入、可构建、可安装、能被游戏识别
- 首批与第二批歌曲牌、压力体系、starter、商店/火堆/能量接线都已经有真实代码
- 当前最重要的工作不是扩内容，而是稳定运行时闭环

## 已经踩过且必须记住的坑

### 1. 不要把 `Entry` 与本地化 key 写偏

- `KillKiss` 的真实 `Entry` 是：
  - `KILL_KISS`
- 不是：
  - `KILLKISS`
- 一旦 key 偏了，坏掉的不只是单卡显示：
  - 奖励
  - 商店
  - `Compose`
  - 卡组遍历
  都可能被污染

### 2. 不要在静态初始化里绑高风险反射

- 这轮真实发生过：
  - `LoseHpInternal` 反射失败
  - 连带让 `EnsureLocalizationOverrides()` 整体提前失败
- 后果是：
  - 明明源码里写了本地化字典
  - 运行时却根本没写进 `user://localization_override`

### 3. 不要把卡牌左上角大费用图和文本小 icon 混成一条链

- `CardPoolModel.EnergyIconPath`
  - 是卡牌左上角用的大图
- `EnergyIconHelper.GetPath(prefix)`
  - 是对白 / hover / 事件 / 说明文本里的小 icon
- 之前把 helper 也指向大图，直接把整段文本排版挤坏

### 4. 自定义角色会撞到原版只认内置角色的系统

- 这轮 `KillKiss` 的最后残留 bug，不是伤害链本身没跑完
- 真正根因是：
  - `ProgressSaveManager.CheckFifteenElitesDefeatedEpoch`
  - `ProgressSaveManager.CheckFifteenBossesDefeatedEpoch`
  只认原版角色
- 自定义角色走进去会抛：
  - `ArgumentOutOfRangeException`

### 5. 商店 scene 是高脆弱资源

- merchant 出问题时，不是“立绘不显示”这么简单
- 它会连带：
  - 商品位异常
  - 交互失效
  - 房间流程无法结束
- 当前 merchant 先采用原版 `silent` scene 回退，是为了保交互稳定

### 6. build / install / 校验必须串行

- 先 `build`
- 再 `install`
- 再做 release/install 哈希比对
- 不要并行开着赌时序

## 当前最有价值的经验文档

下一线程建议优先看：

1. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-implementation-status.md`
2. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-bugfix-round-2026-03-27.md`
3. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-lessons-and-guardrails.md`
4. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-asset-integration-status.md`

## 当前真实断点

- `KillKiss` 的代码侧兼容 patch 已落地
- 文本小能量 icon 的全局分流与缩小版资源已落地
- 奖励 / 商店 / `Compose` 污染问题已有成体系修补
- 下一线程最值得做的是：
  - 系统回归测试
  - merchant 正式 scene 策略判断
  - 文档继续追平实机状态

## 这一轮明确不要做的事

- 不要从零重写角色框架
- 不要回到“只读文档不看代码”或“只看代码不更新文档”
- 不要继续把未验证推测写成事实
- 不要在当前稳定性回归结束前继续大规模扩新牌
