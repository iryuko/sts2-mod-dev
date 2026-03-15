# SilentBonusRelic

这是“角色进 run 后自动补一个指定遗物”的最小验证 mod。

当前目标：

- 当玩家选择 `Silent`
- 并正常进入新 run 后
- 自动获得一个现成遗物 `SneckoSkull`

当前实现说明：

- 使用 `ModInitializerAttribute("Initialize")` 作为初始化入口。
- 不使用 Harmony。
- 不直接手改 relic UI。
- 使用游戏内现成命令：
  - `RelicCmd.Obtain<SneckoSkull>(player)`

当前验证重点：

- mod 是否正常加载
- `Silent` 在选完 `Neow` 奖励后是否还能继续进入第一场战斗
- 进入第一场战斗后是否自动获得 `SneckoSkull`
