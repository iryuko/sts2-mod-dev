# 当前状态

记录日期：2026-03-30

## 当前主线

当前仓库主线已经不是 `PrimalForceStrike`。

当前真正服务中的线程目标是：

- 稳定 `mods/Togawasakiko_in_Slay_the_Spire`
- 不重写 T4，而是接着中断前线程收口
- 优先修运行时 bug、资源接线 bug、文档落后问题
- 暂不扩更多新卡、机制或美术框架

## 当前阶段

`Togawasakiko_in_Slay_the_Spire` 已经走过“纯骨架”阶段，进入：

- 角色可编译、可安装、可被游戏识别的阶段
- 已能进选人、进局、进入战斗的阶段
- 已实现 starter、压力体系、首批与第二批歌曲牌的阶段
- 当前主任务是“稳定首个可玩闭环”的阶段

当前状态应理解为：

- 第一次可运行闭环已经打通
- 但仍在做高风险运行时问题收口
- 文档必须持续追平真实代码和安装状态

## 已确认

- 本仓库是工作区，不是游戏安装目录。
- 真实游戏路径仍以 `local/game-path.txt` 为准。
- 当前游戏 mod 安装目录仍是：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 构建与安装应优先使用：
  - `shared/scripts/build-mod.sh`
  - `shared/scripts/install-mod.sh`
- `Togawasakiko_in_Slay_the_Spire` 当前已经有稳定的 release 导出物：
  - `dll`
  - `pck`
  - `mod_manifest.json`
- 这条线程中已多次使用 release/install 哈希比对确认：
  - 游戏目录内运行中的安装包与工作区 release 一致
  - 不是“工作区修了但游戏目录还留旧包”

## 当前已落地的重要修复

- 奖励 / 商店 / `Compose` 坏牌污染：
  - 已定位到本地化 override 整体失效与 `KILL_KISS` key 错位
  - 已收口到“只允许已本地化、可见、非 starter/basics 的牌进入标准链路”
- `Slander / Unendurable` rarity：
  - 已改回 `Basic`
  - 不再应进入正常战斗奖励池
- 伤害 / 格挡修正链：
  - 已统一对齐原版 `ValueProp.Move`
- `CrucifixX`：
  - 已按原版 `X` 费 AOE 思路修正
- `SakiMovePlz`：
  - 已通过 `ShouldGlowGoldInternal` 对齐原版条件高亮逻辑
- `GodYouFool / STheWay`：
  - 已改为对齐原版 `Bloodletting` 路线，不再走危险的私有反射扣血
- `AveMujica`：
  - 现已先判定顶牌能否打出，不能打出则改为入手
- `KillKiss`：
  - 已从过早的回合钩子挪走
  - 又额外定位到一个更底层的胜利结算问题：
    - 原版 `ProgressSaveManager` 的部分 epoch 统计只识别内置角色
    - 自定义角色会抛 `ArgumentOutOfRangeException`
  - 当前已新增 patch，对自定义角色跳过这些 base-game-only 统计
- `Black Birthday` 与全局文本能量 icon：
  - 已改回原版小尺寸 `energyIcons()` 文本链
  - 且已把“卡牌左上角大 icon”与“文本小 icon”彻底分流

## 当前最重要的经验结论

- 自定义角色 mod 默认要优先对齐原版对象模型，不要自发明新链路。
- 任何卡牌进入奖励 / 商店 / `Compose` / console 前，都要先确认：
  - `Entry`
  - 本地化 key
  - pool
  - rarity
  - 是否应该显示在图鉴
- `Slugify(type.Name)` 生成的 `Entry` 必须和本地化 key 严格一致。
  - `KillKiss -> KILL_KISS` 是这轮最典型的实战坑。
- 静态初始化不要绑高风险反射。
  - 一旦静态构造炸掉，整套 `localization_override` 都可能没写进去。
- 原版里有一批“只认内置角色”的系统。
  - 自定义角色进入这些分支前，要么 patch，要么显式避开。
- 文本中的 icon 与卡牌左上角费用图不是同一条资源链。
  - 一个给 `CardPoolModel.EnergyIconPath`
  - 一个给 `EnergyIconHelper.GetPath(prefix)`
- 商店 scene 是高脆弱点。
  - 不满足原版节点契约时，问题不只是“立绘不显示”，而是整间商店都可能半坏。
- 构建与安装不要并行赌时序。
  - 必须先 build 完，再 install，再做哈希核对。

## 当前仍待复测 / 待收口

- `KillKiss` 击杀最后一只怪后，是否已彻底不再卡奖励结算
- 全局文本中的祥子能量小 icon 是否已恢复到原版观感
- merchant 目前虽已恢复交互，但角色立绘仍是回退方案，不是正式专属 scene
- 仍需要一次成体系的回归测试，覆盖：
  - 普通奖励
  - 商店
  - 火堆
  - `Compose`
  - 第二批歌曲牌

## 下一线程先读什么

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/thread-handoff.md`
5. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-implementation-status.md`
6. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-bugfix-round-2026-03-27.md`
7. `mods/Togawasakiko_in_Slay_the_Spire/docs/t4-lessons-and-guardrails.md`
