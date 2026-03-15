# CrossCharacterCard

这是“跨角色加卡”最小闭环验证 mod。

当前目标：

- 把 `Ironclad` 的现有卡 `BodySlam`
- 追加进 `Silent` 的正常可获得卡池
- 验证 `Silent` 是否能在正常流程里拿到并使用这张牌

当前实现说明：

- 使用 `ModInitializerAttribute("Initialize")` 作为初始化入口。
- 不使用 Harmony。
- 直接调用游戏内建扩展点：
  - `ModHelper.AddModelToPool<SilentCardPool, BodySlam>()`

当前验证重点：

- mod 是否正常加载
- 初始化阶段是否没有异常
- `Silent` 的战后奖励里是否会出现 `BodySlam`
