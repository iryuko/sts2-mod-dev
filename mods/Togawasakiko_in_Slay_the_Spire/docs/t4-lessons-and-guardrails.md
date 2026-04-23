# T4 经验与护栏

日期：2026-03-30

## 一 本文件目的

这不是状态汇报。

这份文档只记录：

- 已被实战 bug 证明过的规律
- 下个线程最容易重复踩的坑
- 写卡、接线、打包前应该先过一遍的检查项

## 二 已确认的硬经验

### 1. 尽量沿用原版逻辑链，不要自发明

- 伤害动态值优先用：
  - `ValueProp.Move`
- 格挡动态值优先用：
  - `ValueProp.Move`
- 条件高亮优先走：
  - `ShouldGlowGoldInternal`
- `X` 费 AOE 牌优先对齐原版目标模型：
  - `TargetType.AllEnemies`
  - 原版的 AOE / 多段思路

如果必须偏离原版，要先写明偏离原因。

### 2. `Entry` 必须按真实类型名来，不要靠肉眼猜

- `Slugify(type.Name)` 是实际约束
- 典型事故：
  - `KillKiss`
  - 实际 `Entry = KILL_KISS`
  - 旧文档 / 旧 key 却写成 `KILLKISS`

检查顺序应当始终是：

1. 真实类名
2. 实际 `Id.Entry`
3. 本地化 key
4. console 命令写法
5. 奖励 / 商店 / `Compose` / 显式生成逻辑

### 3. 本地化问题会污染整条卡牌链

不要把“缺本地化”理解成只是标题显示错。

在当前角色 mod 里，本地化缺失会真实污染：

- 战斗奖励
- 商店
- `Compose`
- 牌组遍历
- 火堆升级

因此进入标准卡池链路前，至少要满足：

- key 存在
- 运行时 override 已实际写入
- `ShouldShowInCardLibrary == true`
- 不属于 starter / token / 不该进常规池的对象

### 4. 静态初始化必须保守

不要在类型初始化阶段做高风险反射。

这轮已经证明：

- 一处反射失败
- 可以直接拖死整个 `localization_override`
- 然后表现成“第二批卡坏牌污染”

结论：

- 反射改惰性解析
- 能晚绑定就晚绑定
- 能判空就判空

### 5. 自定义角色要警惕原版“只认内置角色”的分支

这轮 `KillKiss` 最难的 bug，最终根因不在卡牌本身。

真正触发点是：

- 战斗胜利后进入 `ProgressSaveManager`
- 原版 epoch 检查只枚举内置角色
- 自定义角色直接抛 `ArgumentOutOfRangeException`

结论：

- 看到角色相关 switch / isinst 链时，要优先怀疑是否只支持原版角色
- 尤其是：
  - progress
  - unlock
  - achievement / epoch
  - 特殊房间 UI

### 6. 文本 icon 与卡牌 icon 必须分流

不要把下面两条资源链混为一谈：

- `CardPoolModel.EnergyIconPath`
  - 给卡牌左上角费用图
- `EnergyIconHelper.GetPath(prefix)`
  - 给对白 / hover / 事件 / 说明文本内嵌 icon

这轮已经出现过：

- helper 被误接到大卡牌 icon
- 整段文本被巨大 icon 挤坏

当前正确口径：

- 大图：`images/ui/card/energy_togawasakiko.png`
- 小图：`images/packed/sprite_fonts/togawasakiko_energy_icon.png`

### 7. `sprite_fonts` 资源不能拿原图直接糊过去

如果没有单独上传的小 icon 资源，也不能把大费用图原样复制进去。

当前实战结论：

- 即使路径对了
- 如果 `sprite_fonts` 里的图本体还是同一张大图
- 文本里看起来仍然会过大

当前临时正确做法是：

- 主体尺寸先压到接近原版行内 icon
- 再把整张画布收紧到接近文本 glyph 的边界
- 仅供文本链路使用

### 8. 商店 scene 优先保稳定，再追美术

merchant scene 一旦契约不对，坏的不是单个节点。

会一起坏：

- 立绘
- 商品位
- 交互
- 房间完成判定

因此正确顺序是：

1. 先用原版兼容 scene 保房间稳定
2. 再按原版 merchant 节点契约做角色专属 scene
3. 最后再追求正式立绘表现

### 9. token / generated 牌不能随便进标准池

衍生牌必须明确区分：

- `Pool`
- `VisualCardPool`
- rarity
- 图鉴可见性
- 是否可被随机生成

当前角色里的压力衍生牌已经证明：

- 如果这些边界不收口
- console、奖励、商店、变形链都会被污染

### 10. 原版进度统计不仅有“十五精英 / 十五 boss”，还有角色解锁 epoch

即使已经补掉：

- `CheckFifteenElitesDefeatedEpoch`
- `CheckFifteenBossesDefeatedEpoch`

也不代表自定义角色的战斗收尾就安全了。

这轮又实锤了一条：

- `ProgressSaveManager.ObtainCharUnlockEpoch(...)`

它会直接拼：

- `CHARACTER_ENTRY + 2_EPOCH`
- `CHARACTER_ENTRY + 3_EPOCH`
- `CHARACTER_ENTRY + 4_EPOCH`

去查原版解锁 epoch。

对自定义角色如果没有对应原版 timeline 资源，就会在胜利收尾阶段抛错并卡奖励。

### 11. “本战斗”状态不要假设引擎一定会帮你清

隐藏 watcher power 就算名字叫 combat watcher，也不能假设它的内部字段会在新战斗自动归零。

这轮已经踩到：

- `Two Moons Deep Into The Forest`
  - 依赖 watcher 记录“本战斗已打出的不同名 song”
  - watcher 若跨房间保留旧计数
  - 卡就会表现成跨战斗永久减费

当前护栏是：

- 进入新的 `CombatRoom`
- 显式 reset watcher 的战斗内字段
- 再刷新依赖该状态的卡牌费用

### 12. 事件触发型 debuff 自己施加的负面状态，可能误撞通用 watcher

如果某个 debuff 的内部效果本身会施加：

- `StrengthPower(-n)`
- `DexterityPower(-n)`

就要检查它会不会又被“减力量 / 减敏捷 => 压力兑换衍生牌”的全局 watcher 吃一遍。

这轮 `Inferiority` 的问题最终确认不是 watcher 多吃了一次。

真正要核对的是：

- 【过劳焦虑】到底该在“施加 debuff 时”触发
- 还是在“后续 debuff 效果结算时”触发

如果冻结文档写的是“施加时兑换”，就不要把它偷懒挂在后续 `AfterDamageReceived` 之类的持续效果里。

### 13. 构建与安装纪律要写死

正确顺序：

1. 改代码 / 文档 / 资源
2. `dotnet build`
3. `build-mod.sh`
4. `install-mod.sh --replace-target`
5. release/install 哈希比对

禁止做的事：

- build/install 并行开
- 把临时备份文件留在 `pack/`
- 默认相信“应该已经装进去了”

## 三 新增卡牌前检查清单

1. 类名是否已最终确定
2. `Entry` 是否与本地化 key 对齐
3. rarity / type / target 是否符合原版语义
4. 是否应该进入正常奖励池
5. 是否应该进商店
6. 是否应该在图鉴显示
7. 是否需要 token pool
8. 是否需要 hover tip
9. 是否走原版伤害 / 格挡 / 扣血链
10. 是否会碰到特殊系统：
   - `Compose`
   - auto-play
   - discard
   - exhaust
   - X 费
   - 条件高亮

## 四 资源接线前检查清单

1. 这是卡牌图、文本 icon、scene 贴图还是 energy counter 图层
2. 它属于：
   - `assets/`
   - `pack/`
   - 还是二者都要更新
3. 路径是否真被代码 / scene / atlas 使用
4. `.import` 是否存在
5. 是否会被 `build-mod.sh` 打进最终 PCK
6. 是否留下了不该进包的临时文件

## 五 回归测试最低清单

每次改高风险牌或资源后，至少回归：

1. 选人
2. 进局
3. 普通战斗奖励
4. 商店
5. 火堆
6. `Compose`
7. 第二批歌曲牌中最新改动涉及的对象
8. 最新一次修复对应的日志关键字是否消失

## 六 当前仍适用的总体策略

- 先收口，再扩写
- 先稳定标准链路，再谈额外表现
- 先让文档追平真实状态，再开下一轮线程
