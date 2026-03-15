# 线程接班摘要

本项目最初在研究 STS2 macOS 的本地 mod 安装与加载链路，这条链路现在已经打通：

- 游戏会扫描：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- `SmokeMod` 已经实机被识别和加载过
- `UnifiedSavePath` 的 macOS 本地修正版也已经实机成功，说明当前工作区具备做可运行功能 mod 的基础条件
- `UnifiedSavePath` 当前源码也已进一步整理成跨平台实现：
  - Windows 走 Harmony patch 路线
  - macOS / 非 Windows 走已验证成功的 flag-thread workaround

当前主线已经切到：

- 做一个最小“跨角色加卡”功能 mod

当前已确认的技术结论：

- 当前 run 角色通过 `Player.get_Character()` 读取
- 角色正常卡牌来源走：
  - `CharacterModel.get_CardPool()`
  - `CardCreationOptions.ForRoom(...)`
  - `CardPoolModel.GetUnlockedCards(...)`
- 当前最适合的最小干预点不是 patch 奖励逻辑，而是直接走原生扩展点：
  - `ModHelper.AddModelToPool<TPoolType, TModelType>()`
- 角色卡池的 epoch 过滤只会移除该角色自己 epoch 中尚未解锁的卡
- 因此，mod 追加进去的外来卡默认有机会保留下来
- 当前用户后续实机观察到：
  - `BodySlam` 是作为正常卡牌奖励出现的
  - 这与 `ModHelper.AddModelToPool<SilentCardPool, BodySlam>()` 完全一致
- 我们后来加上的“运行时直接塞进当前牌组”只是诊断路径，不是必要实现
- 这条诊断路径曾导致两类真实问题：
  - 在过早时机写牌组会黑屏
  - 在 `Neow` / `Ancient` 房改牌组会卡住流程
- 因此当前已决定删掉这条路径，保持 mod 只做卡池扩展

当前首个验证组合已经固定：

- 来源角色：`Ironclad`
- 目标角色：`Silent`
- 目标卡牌：`BodySlam`

当前工作区新增内容：

- 分析文档：
  - `docs/character-cardpool-analysis.md`
  - `docs/cross-character-card-plan.md`
- 新 mod：
  - `mods/CrossCharacterCard/`
  - `mods/SilentBonusRelic/`

`CrossCharacterCard` 当前实现：

- 使用 `ModInitializerAttribute("Initialize")`
- 初始化时按集中规则表逐条注册
- 当前规则表中已有：
  - `BodySlam -> SilentCardPool`
- 当前版本不再订阅 `RunStarted` / `RoomEntered`
- 当前版本不再运行时修改 `Silent` 当前牌组
- 当前 release 产物已重新构建成功：
  - `mods/CrossCharacterCard/exports/release/CrossCharacterCard/CrossCharacterCard.dll`
  - `mods/CrossCharacterCard/exports/release/CrossCharacterCard/CrossCharacterCard.pck`
- 当前新版已经安装到：
  - `.../Contents/MacOS/mods/CrossCharacterCard/`

`SilentBonusRelic` 当前实现：

- 使用 `ModInitializerAttribute("Initialize")`
- 目标角色：
  - `Silent`
- 目标遗物：
  - `SneckoSkull`
- 当前已确认：
  - `Silent::get_StartingRelics()` 默认只有 `RingOfTheSnake`
  - `RelicCmd.Obtain<TRelic>(player)` 是游戏内现成加遗物命令
  - `Player.AddRelicInternal(...)` 是更底层的真实落地入口
- 进一步确认到：
  - `Player.PopulateStartingRelics()` 在 `RunStarted` 之前就完成
  - 当前没找到原生“扩展角色 StartingRelics 列表”的 mod helper
- 当前版本已改为更接近“第二个起始 relic”的实现：
  - `RunStarted` 触发时立刻检查当前玩家
  - 如果是 `Silent`
  - 则按集中规则表依次补发额外起始 relic
  - 当前规则表中已有：
    - `SneckoSkull`
    - `Shuriken`
  - 每个 relic 都会：
    - `FloorAddedToDeck = 1`
    - `SaveManager.MarkRelicAsSeen(...)`
    - `Player.AddRelicInternal(..., silent: true)`
- 当前 release 产物已构建并安装到：
  - `.../Contents/MacOS/mods/SilentBonusRelic/`

当前最关键的问题：

- `BodySlam` 进入 `Silent` 正常奖励池的频率是否足够稳定，能否作为后续多卡迁移的可靠模式
- `SilentBonusRelic` 改成 `RunStarted` 立即补发后，是否能稳定表现为 `Silent` 的第二个起始 relic
- `SilentBonusRelic` 新增 `Shuriken` 后，是否能稳定表现为 `Silent` 的两件额外起始 relic

下一步该做：

1. 启动游戏，确认 mod 是否无异常加载
2. 用 `Silent` 开新 run
3. 正常推进几场战斗并观察奖励
4. 确认 `BodySlam` 是否会作为正常奖励出现
5. 选择后继续确认：
   - `BodySlam` 能否正常进入牌组
   - 能否正常抽到并打出
6. 观察新 run 一开始是否就拥有 `SneckoSkull` 和 `Shuriken`
7. 如果卡牌路径稳定成立，再考虑把同一模式扩展到更多卡牌与角色组合

接手时先看：

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/thread-handoff.md`
5. `docs/decisions.md`
