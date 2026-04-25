# 工作报告

日期：2026-03-24

对象：`Togawasakiko_in_Slay_the_Spire`

## 一 T2 报告保留区

### 1. 原 T2 报告定位

以下内容保留原有 T2 收尾报告的定位：

- 记录角色机制冻结状态
- 作为从 T2 切换到 T3 之前的基线结论

本区内容不因为 T3 线程接手而被覆盖。

### 2. T2 当前报告定位

当前报告覆盖到 `2026-03-24`，已经合并四轮工作：

1. 建立角色 mod 的长期主目录与首版机制文档框架
2. 完成目录修正与命名规范补强
3. 冻结当前已明确的角色机制内容，并补做本地命名核查
4. 完成 T2 收尾冻结，并准备切换到 T3 资源与美术资产线程

因此，这一部分以 T2 收尾时的最新状态为准。

### 3. T2 已完成内容

#### 3.1 目录与文档骨架

已建立并保留的主目录结构：

- `docs/`
- `assets/character/`
- `assets/cards/`
- `assets/icons/`
- `assets/relics/`
- `src/`
- `manifest/`
- `exports/debug/`
- `exports/release/`
- `localization/`
- `tests/`

当前不保留：

- `pack/`

#### 3.2 角色总定位

当前已冻结：

- 通过 debuff 控制敌人
- 通过压力衍生牌打出连锁节奏
- 战斗身份优先级：
  - 控制
  - 压制
  - 防御
  - 处决

其中：

- 处决可以存在
- 但处决是高稀有度 payoff
- 不是主框架

#### 3.3 压力系统规则

当前已冻结：

- 压力与普通 debuff 分层记录
- 敌我双方都可能拥有压力
- 当前主线优先围绕敌方压力展开
- 压力不衰减
- 压力无上限
- 压力没有基础效果
- 压力通过牌效与 debuff 消耗，再兑换压力衍生牌
- 单体敌人独立计算，不做全场共享

#### 3.4 当前冻结工作版压力衍生牌

当前冻结工作版共 `4` 张：

- 人格解离：
  - 由“虚弱”消耗 `2` 层压力兑换
  - 技能牌
  - `1` 费
  - 默认消耗
  - 施加 debuff【人格解离】
  - 对应 debuff 效果：每层都会让下一次受到的伤害在格挡结算前先翻倍，并按受伤事件逐层消耗
- 自闭：
  - 由“失去力量 / 失去敏捷”消耗 `1` 层压力兑换
  - 技能牌
  - `0` 费
  - 默认消耗
  - 施加 `3` 层【自闭】
  - 对应 debuff 效果：回合结束造成 `7` 点伤害后减 `1` 层，可叠加，可续层
- 满脑子都想着自己：
  - 由“易伤”消耗 `3` 层压力兑换
  - 攻击牌
  - `3` 费
  - 默认虚无
  - 默认消耗
  - 打 `9`
  - 晕一回合
- 过劳焦虑：
  - 由【自卑】消耗 `1` 层压力兑换
  - 技能牌
  - `0` 费
  - 默认消耗
  - 抽 `1` 张牌
  - 不进入正常卡池

#### 3.5 原创状态 / debuff 分层

当前已明确分开记录：

- 核心状态层：
  - 压力 `Pressure`
- 原创 debuff：
  - 人格解离 `Persona Dissociation`
  - 自闭 `Social Withdrawal`
  - 自卑 `Inferiority`

补充结论：

- 人格解离、自闭的效果已进入冻结范围
- 自卑的功能定位已进入冻结范围
- 自卑当前按“持续时间型 debuff”整理
- 自卑的持续时间数值仍保留后续微调空间

#### 3.6 起始 relic 与起始牌组

当前已整理：

- 初始 relic：
  - 显示名：【人偶的假面】
  - 效果：每个玩家回合开始时，给予所有敌人 `1` 层压力
  - 当前冻结候选内部名：`DollMask`
- 初始牌组：
  - `4` 张打击
  - `4` 张防御
  - `1` 张自定义白色攻击牌
  - `1` 张自定义白色技能牌
- 起始攻击牌冻结候选名：
  - `Slander`
  - `1` 费，打 `4`，每层压力 `+3`
  - 升级后改为 `0` 费
- 起始技能牌冻结候选名：
  - `Unendurable`
  - `2` 费，获得 `5` 格挡并给予 `2` 层压力
  - 升级后为 `8` 格挡并给予 `2` 层压力

#### 3.7 正常卡池结构

当前 `50` 张正常卡池的冻结工作版分布为：

- 普通品质：`12`
- 非凡品质：`22`
- 稀有品质：`16`

这版比例来自本地原版角色卡池资料的近似缩放，当前不在 T2 再继续改动。

#### 3.8 命名与资源规范

当前已固定的主规则：

- 类名：`PascalCase`
- `Entry`：全大写蛇形
- 资源文件 base name：`entry.ToLowerInvariant()`
- 本地化 key：围绕同一 `Entry`

当前项目内可直接沿用的工作版命名包括：

- `PressurePower -> PRESSURE_POWER`
- `PersonaDissociationPower -> PERSONA_DISSOCIATION_POWER`
- `SocialWithdrawalPower -> SOCIAL_WITHDRAWAL_POWER`
- `InferiorityPower -> INFERIORITY_POWER`
- `DollMask -> DOLL_MASK`
- `Slander -> SLANDER`
- `Unendurable -> UNENDURABLE`

### 4. T2 当前仍未冻结的事项

当前仍只归档、不擅自补完的事项包括：

- 初始 relic 的最终文本
- `DollMask` 是否在统一命名审校阶段继续维持不变
- `Slander` 与 `Unendurable` 是否在最终文案审校阶段继续维持不变
- 正常卡池中不同功能牌的比例
- 攻击 / 技能 / 能力牌的比例
- 稀有池中处决 payoff 的数量与强度
- `12 / 22 / 16` 是否在后续验证后仍需微调
- 是否需要后续单独讨论 `Ancient` 层
- 各原创 debuff 的具体数值微调

### 5. T2 本轮明确未做的内容

- 没有写正式功能代码
- 没有生成素材
- 没有展开攻击 / 技能 / 能力牌比例
- 没有扩写新的压力衍生牌
- 没有补完角色叙事气质
- 没有补完原创人设推导
- 没有冻结最终美术风格

### 6. T2 收尾结论

截至 T2 收尾，`Togawasakiko_in_Slay_the_Spire` 已经具备：

- 可长期维护的机制文档骨架
- 已冻结的角色核心机制
- 已冻结的 starter relic / starter cards 工作版命名
- 已冻结的 `50` 张正常卡池工作版比例
- 一套可长期复用的命名与资源规范
- 一份面向 T3 的资产交接摘要

T2 当前可视为收尾完成。

## 二 T3 本轮增补报告

线程：`T3-资源与资产`

### 1. T3 本轮定位

本轮不是正式出图轮，也不是功能实现轮。

本轮实际完成的是：

- 原版角色固有资产盘点
- 本角色完整资产总表建立
- 资源目录结构搭建
- 命名规范接入资源层
- song 子标签规则落位

### 2. T3 本轮已完成内容

#### 2.1 已完成原版角色固有资产盘点

本轮已直接基于工作区中的原版提取资源与场景文件，补做 STS2 当前原版角色资产盘点。

当前已确认，原版角色通常各自拥有：

- 角色选择主立绘
- 角色选择锁定图
- 顶栏头像
- 顶栏头像轮廓
- 顶栏头像 scene
- 角色选择背景 scene
- 角色卡牌费用图标
- 战斗能量计数器 scene
- 战斗能量计数器图层
- 联机手势图

同时已确认，以下内容属于系统公用资源，不需要每个角色重复自制：

- 卡牌框体
- 标题横幅
- portrait border
- type plaque
- 锁牌遮罩
- 角色选择按钮轮廓与遮罩
- 能量爆闪与星星图
- card / power / relic atlas

对应文档：

- `original-character-asset-audit.md`

#### 2.2 已建立本角色完整资产总表

本轮已把 `Togawasakiko_in_Slay_the_Spire` 的资产需求拆成五类：

- A 类：角色固有资产
- B 类：正常卡池资产
- C 类：压力体系资产
- D 类：song 子标签支持位
- E 类：后续扩展资产

当前卡图统计口径已固定为“唯一条目数”：

- `Basic`：`4`
- 正常卡池：`50`
- 压力衍生牌：`4`
- 全部卡图槽位：`58`

对应文档：

- `asset-master-list.md`

#### 2.3 已把 song 子标签纳入正式规则系统

本轮已明确：

- song 牌属于正常卡池子集
- song 牌不是压力衍生牌
- song 牌不单独拆出另一套卡池目录
- song 首先是逻辑标签，不是当前必需图像资产

当前首版最少需要：

- 一份独立 song 子集登记文档
- 资产总表中的独立 song 栏位
- 一套稳定的命名与登记规则

当前尚未冻结具体 song 牌条目，因此：

- 先留登记位
- 不编造具体条目

对应文档：

- `song-subset-registry.md`

#### 2.4 已把命名规范落实到资源层

本轮已把既有研究线程中的命名结论真正接到资源层，固定以下规则：

- 类名：`PascalCase`
- `Entry`：全大写蛇形
- 资源文件 base name：`entry.ToLowerInvariant()`
- 显示名与本地化文本不进入正式资源文件名

当前已明确的资源命名包括：

- `char_select_togawasakiko.png`
- `character_icon_togawasakiko.png`
- `character_icon_togawasakiko_outline.png`
- `energy_togawasakiko.png`
- `pressure_power.png`
- `persona_dissociation_power.png`
- `social_withdrawal_power.png`
- `inferiority_power.png`
- `doll_mask.png`

当前也已明确：

- song 不拼进正式卡图文件名
- 未冻结英文主键的对象，不抢锁正式文件名

对应文档：

- `naming-and-resource-conventions.md`

#### 2.5 已建立长期可维护的资源目录结构

本轮已在角色项目下建立并固定以下目录骨架：

- `assets/character/char_select/`
- `assets/character/char_select_bg/`
- `assets/character/top_panel/`
- `assets/character/energy/`
- `assets/character/multiplayer_hand/`
- `assets/cards/basic/`
- `assets/cards/normal/common/`
- `assets/cards/normal/uncommon/`
- `assets/cards/normal/rare/`
- `assets/cards/generated_pressure/`
- `assets/cards/placeholders/`
- `assets/icons/powers/`
- `assets/icons/tags/`
- `assets/icons/ui/`
- `assets/relics/starter/`
- `assets/relics/future/`

当前目录结构的重点是：

- 把角色固有 UI 资源与卡图、icon、relic 分层
- 把压力衍生牌与正常卡池分开
- 把 song 视为“标签子集”，而不是另一套卡池目录
- 为后续长期替换占位图保留稳定文件名

对应文档：

- `resource-layout.md`

#### 2.6 已区分首版可占位与应尽快正式制作的资源

当前建议尽快正式制作的资产包括：

- 角色选择主立绘
- 顶栏头像
- 角色卡牌能量图标
- 战斗能量球图层
- 压力 icon
- `3` 个原创 debuff icon
- starter relic 图

当前允许先占位的资产包括：

- `54` 张非衍生角色卡图
- `4` 张压力衍生牌卡图
- 锁定图
- 头像轮廓
- 角色选择背景
- 联机手势图

对应文档：

- `asset-checklist.md`

### 3. T3 当前上传策略已经明确

本轮新增一条重要结论：

- 源资源上传位置
- 运行时导出位置
- 游戏安装位置

这三者必须继续严格分开，不允许混用。

#### 3.1 现在应该上传哪些资源

当前应优先上传到角色项目工作区的，是“源资源”而不是安装成品。

第一优先级：

- `assets/character/char_select/char_select_togawasakiko.png`
- `assets/character/top_panel/character_icon_togawasakiko.png`
- `assets/character/energy/energy_togawasakiko.png`
- `assets/character/energy/togawasakiko_orb_layer_*.png`
- `assets/icons/powers/pressure_power.png`
- `assets/icons/powers/persona_dissociation_power.png`
- `assets/icons/powers/social_withdrawal_power.png`
- `assets/icons/powers/inferiority_power.png`
- `assets/relics/starter/doll_mask.png`

第二优先级：

- `assets/character/char_select/char_select_togawasakiko_locked.png`
- `assets/character/top_panel/character_icon_togawasakiko_outline.png`
- `assets/character/char_select_bg/` 下的背景源文件
- `assets/character/multiplayer_hand/` 下的联机手势图

第三优先级：

- `assets/cards/basic/`
- `assets/cards/normal/common/`
- `assets/cards/normal/uncommon/`
- `assets/cards/normal/rare/`
- `assets/cards/generated_pressure/`

说明：

- 卡图当前允许先用规范化占位图上传。
- 但占位图也必须按最终文件名落位，而不是以“临时乱命名文件”形式上传。

#### 3.2 这些资源应该怎么上传

当前推荐流程：

1. 先在工作区内整理为最终目标文件名。
2. 按资源类型放入 `mods/Togawasakiko_in_Slay_the_Spire/assets/` 对应子目录。
3. 若是卡图占位，也直接落成该牌自己的正式文件名。
4. 若对象英文稳定主键未冻结，只上传到对应目录位，不抢写最终正式文件名。
5. 上传后同步更新相关文档：
   - `asset-master-list.md`
   - `asset-checklist.md`
   - `song-subset-registry.md`
   - 必要时更新 `naming-and-resource-conventions.md`

当前不推荐的做法：

- 不把源资源直接扔进游戏安装目录
- 不把源资源直接扔进 `exports/release/`
- 不把中文显示名直接写进正式资源文件名
- 不把 song 牌单独另建一套卡图目录

#### 3.3 这些资源应该上传到哪

当前应分三层处理：

##### A. 工作源资源上传位置

这是当前 T3 线程最主要的上传目标。

应上传到：

- `mods/Togawasakiko_in_Slay_the_Spire/assets/...`

这是：

- 角色项目长期维护目录
- 资源线程当前唯一正确的主上传位置

##### B. 运行时成品导出位置

只有当后续开始做真正可安装成品时，才应把最终导出物放到：

- `mods/Togawasakiko_in_Slay_the_Spire/exports/debug/`
- `mods/Togawasakiko_in_Slay_the_Spire/exports/release/`

当前说明：

- `exports/` 用于导出后的候选成品
- 不是源 PSD、源 PNG、草稿图的上传位置

##### C. 游戏安装位置

真实游戏目录仍然只用于：

- 安装最终成品
- 运行验证

当前不允许：

- 把 `assets/` 下的源资源直接上传到游戏目录
- 把整个角色项目目录复制进游戏安装区

如果后续进入实际安装验证阶段，仍应优先通过脚本处理：

- `shared/scripts/install-mod.sh`

### 4. T3 当前仍未冻结的事项

当前仍只归档、不擅自补完的事项包括：

- 初始 relic 的最终文本
- `DollMask` 是否在统一命名审校阶段继续维持不变
- `Slander` 与 `Unendurable` 是否在最终文案审校阶段继续维持不变
- 正常卡池中不同功能牌的比例
- 攻击 / 技能 / 能力牌比例
- 稀有池中处决 payoff 的数量与强度
- 各原创 debuff 的具体数值微调
- song 子集的具体条目名单
- 【满脑子都想着自己】与【过劳焦虑】的最终英文稳定主键
- 角色选择背景是否在首版就进入正式制作

### 5. T3 本轮明确未做的内容

- 没有开始正式美术创作
- 没有大量生成卡图
- 没有写正式功能代码
- 没有重新定义角色机制
- 没有把源资源导出到 `exports/release/`
- 没有把任何资源安装到游戏目录

### 6. T3 本轮结论

截至本轮，`Togawasakiko_in_Slay_the_Spire` 已经具备：

- 一份原版角色固有资产盘点
- 一份完整的本角色资产总表
- 一套可长期维护的资源目录结构
- 一套已接入资源层的命名规则
- 一份 song 子标签资源登记位
- 一份资源规格与素材投递流程说明
- 一份明确区分“可占位 / 应尽快正式制作”的执行清单
- 一条清晰的上传策略：
  - 源资源上传到 `assets/`
  - 导出成品进入 `exports/`
  - 安装验证走脚本，不直接把源资源扔进游戏目录

当前可以视为：

- T3 资源线程已完成“研究与结构搭建”这一阶段
- 下一阶段可以开始按优先级持续上传角色 UI 资源、icon 与规范化占位卡图

## 三 T3 资产入库更新

日期：`2026-03-25`

本轮已把首批合格资产从 `incoming_assets/` 整理进正式 `assets/`。

### 1. 已入正式库

- `assets/character/char_select_bg/char_select_bg_togawasakiko.png`
- `assets/character/char_select/char_select_togawasakiko.png`
- `assets/character/top_panel/character_icon_togawasakiko.png`
- `assets/character/energy/energy_togawasakiko.png`
- `assets/character/energy/togawasakiko_orb_layer_1.png`
- `assets/character/energy/togawasakiko_orb_layer_2.png`
- `assets/character/energy/togawasakiko_orb_layer_3.png`
- `assets/character/energy/togawasakiko_orb_layer_4.png`
- `assets/character/energy/togawasakiko_orb_layer_5.png`
- `assets/relics/starter/doll_mask.png`

### 2. 本轮确认的当前口径

- 角色选择背景大图当前已明确按“静态终稿”路线处理，不追原版动态场景效果
- `incoming_assets/` 继续保留原始来稿，`assets/` 只放已经整理到正式命名与正式规格的成品
- 当前尚未入正式库的角色固有资源主要是：
  - `char_select_togawasakiko_locked.png`
  - `character_icon_togawasakiko_outline.png`
  - `multiplayer_hand_togawasakiko_<pose>.png`
  - 各状态 icon

### 3. 当前阶段结论

- 角色核心 UI 识别件已经从“仅有目录骨架”推进到“已有首批正式文件”
- 后续文档与资源状态应以 `assets/` 当前实际内容为准，不再把上述对象写成纯占位计划

## 四 T4 接班检查与实现状态更新

日期：`2026-03-26`

线程：`T4-角色实现`

### 1. T4 当前定位

本轮已不再停留在纯文档 / 纯资源整理阶段。

当前项目已经进入：

- 角色最小功能骨架实现
- runtime 资源接线
- 可构建 / 可打包前的接班检查

但截至本轮检查结束，当前状态仍应保守定义为：

- 已进入源码实现阶段
- 尚未进入“可构建、可导出、可安装验证”的稳定状态

### 2. T4 本轮已推进到的实现层

当前角色项目下已经新增并保留：

- `src/Togawasakiko_in_Slay_the_Spire.csproj`
- `src/Entry.cs`
- `src/ModSupport.cs`
- `src/Characters/Togawasakiko.cs`
- `src/Cards/TogawasakikoCards.cs`
- `src/Powers/TogawasakikoPowers.cs`
- `src/Relics/DollMask.cs`
- `manifest/mod_manifest.json`
- `pack/pack_pck.gd`
- `pack/scenes/`
- `pack/images/atlases/ui_atlas.sprites/card/energy_togawasakiko.tres`

当前代码层已经写出第一版最小角色骨架：

- 角色模型：
  - `Togawasakiko`
- starter relic：
  - `DollMask`
- starter deck：
  - `StrikeTogawasakiko`
  - `DefendTogawasakiko`
  - `Slander`
  - `Unendurable`
- 核心状态与原创 debuff：
  - `PressurePower`
  - `PersonaDissociationPower`
  - `SocialWithdrawalPower`
  - `InferiorityPower`
- 压力衍生牌：
  - `PersonaDissociationCard`
  - `SocialWithdrawalCard`
  - `AllYouThinkAboutIsYourself`
  - `OverworkAnxiety`
- runtime 接线尝试：
  - 角色选人按钮注入
  - 进入战斗后挂 watcher power
  - 自定义能量球 / 顶栏 icon / 选角背景 / 静态战斗立绘 scene

### 3. T4 接班检查已确认的事实

#### 3.1 manifest 源文件已经对齐当前 loader 最小字段

当前 `manifest/mod_manifest.json` 已包含：

- `id`
- `pck_name`
- `name`
- `author`
- `description`
- `version`
- `has_pck`
- `has_dll`
- `dependencies`
- `affects_gameplay`

因此从“源 manifest 模板是否符合 `v0.99.1+` 原生 loader 最小要求”这一点看，当前方向是对的。

但本轮同时确认：

- 当前还没有实际导出的 release 成品
- `exports/` 下仍没有可安装的：
  - `dll`
  - `pck`
  - 外部 `mod_manifest.json`

所以不能把“源 manifest 写对了”直接写成“当前 mod 已可被游戏正常加载”。

#### 3.2 当前实现与 T2 / T3 的主机制基线总体不冲突

本轮接班检查后，当前没有发现“已经大规模偏离 T2 冻结设计”的问题。

当前仍然保持在这些边界内：

- 主轴仍是：
  - debuff 控制
  - 压力消耗
  - 生成压力衍生牌
- starter relic 仍是：
  - 每个玩家回合开始时给予所有敌人 `1` 层压力
- starter deck 仍是：
  - `4` 打击
  - `4` 防御
  - `Slander`
  - `Unendurable`
- 当前没有擅自铺开 `50` 张正常卡池

当前真正的问题不在于“方向跑偏”，而在于：

- 编译还没打通
- runtime 资源接线还没闭环
- 文档状态与实际文件状态之间出现了偏差

### 4. T4 接班检查已确认的明确断点

#### 4.1 当前源码还不能通过本地构建

本轮已直接运行：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj`

结果已确认当前构建失败。

已抓到的明确错误包括：

- `Entry.cs` 里把 `CharacterSelectInjector` 当成顶级类型使用
- 但当前真实定义在：
  - `ModSupport` 的嵌套类型里
- `TogawasakikoPowers.cs` 中的 `CombatSide` 当前无法解析

因此，`t4-implementation-status.md` 中“可构建的最小角色骨架”这句话，目前不能继续按已成立处理。

#### 4.2 runtime 资源路径还没有真正闭环

本轮核对后已确认：

- `pack/` 里确实已经有若干 scene / atlas 文件
- 但 `pack/images/` 与 `pack/mod_assets/` 当前几乎没有真正资源文件

这意味着当前代码与 scene 引用的很多 `res://` 路径，实际上在 `pack/` 里并不存在。

已确认的典型情况包括：

- 角色选人图路径已写进角色模型
- 角色锁定图路径已写进角色模型
- 顶栏头像与 outline 路径已写进角色模型
- 卡图 portrait 路径已写进卡牌类
- 能量图标 `.tres` 已写了引用

但对应运行时资源目前并没有完整落到 `pack/` 对应位置。

因此当前不能把这些对象写成“已经真正接入运行时可加载资源”。

#### 4.3 选角背景 scene 当前接错了图片

本轮核对 `pack/scenes/screens/char_select/char_select_bg_togawasakiko.tscn` 后确认：

- 这个 scene 当前引用的是：
  - `char_select_togawasakiko.png`
- 但从文档与资源命名上看，选角背景应对应：
  - `char_select_bg_togawasakiko.png`

这属于明确接线错误，不只是“尚未验证”。

#### 4.4 文档里有一批“已接入”表述已经落后于当前实际状态

本轮已确认以下偏差：

- `README.md` 仍保留：
  - 当前不直接写正式功能代码
  - `pack/` 当前不保留
- 但当前项目里实际上已经有：
  - `src/`
  - `manifest/`
  - `pack/`
  - 多个 runtime scene

同时，`t4-asset-integration-status.md` 里把多项资源写成：

- 占位接入
- 已使用

但本轮实际核对后确认，当前仓库里仍缺：

- `4` 张 starter / generated 卡图占位文件
- `4` 个 power / debuff icon 占位文件
- 顶栏 outline 文件

因此，这些内容更准确的写法应是：

- 路径规划已固定
- 代码与文档已预留引用位
- 但实际文件仍待补交 / 待生成

#### 4.5 压力衍生牌的“虚无 + 消耗”规则尚未完全体现在代码层

T2 冻结文档里已经把当前 `4` 张压力衍生牌写成：

- 默认虚无
- 默认消耗

但本轮核对代码后，只明确看到：

- `GeneratedPressureCard` 设置了默认消耗

当前还没有同等级别的清晰证据表明：

- `4` 张牌都已经按冻结规则接上“默认虚无”

因此这一点当前应记为：

- 代码 / 文档未完全对齐
- 需要在下一轮实现中补确认

### 5. T4 当前可继续沿用的部分

以下内容当前可以继续沿用，不需要推翻重来：

- 角色整体方向与机制边界
- starter relic / starter deck 的对象选择
- 压力与 `4` 张衍生牌的当前工作版范围
- 角色命名主轴：
  - `Togawasakiko -> TOGAWASAKIKO -> togawasakiko`
- `DollMask / Pressure / PersonaDissociation / SocialWithdrawal / Inferiority` 这批已冻结对象名
- 已入正式库的角色资源：
  - 选角图
  - 顶栏头像
  - 能量图标
  - 能量球图层
  - starter relic 图
- `manifest/mod_manifest.json` 的最小字段结构
- `pack/` 下已经写出的 scene / atlas 骨架

也就是说，当前不需要“从零重写角色 mod”。

更合理的做法是：

- 在现有骨架上修编译
- 修资源路径
- 补最小占位文件
- 再做首轮 build / export / install / 实机验证

### 6. T4 下一步最合理的推进顺序

当前最优先不应是扩卡，也不应是继续扩机制。

最合理的执行顺序应是：

1. 先修当前构建错误
2. 统一并补齐最小 runtime 资源路径
3. 生成最小可用导出物：
   - `dll`
   - `pck`
   - 外部 `mod_manifest.json`
4. 再进入首轮实机验证：
   - 角色按钮是否出现
   - 是否能正常选中并进局
   - starter relic / starter deck 是否生效
   - Pressure 与衍生牌逻辑是否能触发
5. 实机验证后再回写：
   - `README.md`
   - `t4-implementation-status.md`
   - `t4-asset-integration-status.md`

### 7. T4 当前阶段结论

截至 `2026-03-26` 的接班检查结束，`Togawasakiko_in_Slay_the_Spire` 当前应被定义为：

- 已从 T3 资源准备推进到 T4 源码实现阶段
- 已具备角色最小闭环所需的大部分对象骨架
- 但当前仍停在“源码与 runtime 资源接线中途”
- 还不能写成：
  - 已可构建
  - 已可导出
  - 已可安装验证

当前最关键的事实是：

- 旧线程不是没做事
- 当前也不是方向错误
- 真正卡住的是：
  - 编译断点
  - 资源接线断点
  - 文档状态落后

因此下一轮应继续基于现有工作推进，而不是推翻重来。

## 五 T4 推进更新

日期：`2026-03-26`

本段用于补记接班检查之后、同日继续推进得到的新状态。

### 1. 当前已打通 build

本轮已继续修复源码层的直接构建断点。

当前已确认：

- `dotnet build mods/Togawasakiko_in_Slay_the_Spire/src/Togawasakiko_in_Slay_the_Spire.csproj`
  - 已通过

本轮实际修掉的构建问题包括：

- `Entry.cs` 中 `CharacterSelectInjector` 的错误引用方式
- `TogawasakikoPowers.cs` 中 `CombatSide` 缺少正确命名空间
- `Player` 类型缺少引用
- `CardEnergyCost` 构造参数不符合当前 API
- `System.Environment` / `Godot.Environment` 命名冲突
- `GetInstanceId()` 返回值与本地集合类型不匹配

### 2. 当前已补最小 runtime 资源闭环

本轮已继续把“第一次最小可运行闭环”所必需的资源补到真实路径。

已新增或补齐：

- `pack/images/packed/character_select/`
- `pack/images/ui/top_panel/`
- `pack/images/ui/card/`
- `pack/images/ui/combat/energy_counters/togawasakiko/`
- `pack/images/powers/`
- `pack/images/relics/`
- `pack/mod_assets/cards/basic/`
- `pack/mod_assets/cards/generated_pressure/`
- `pack/mod_assets/character/`

当前处理原则是：

- 缺正式图时允许继续用占位
- 但运行时引用的路径必须真实存在

本轮还额外修掉了一条明确接线错误：

- `char_select_bg_togawasakiko.tscn`
  - 已改回引用 `char_select_bg_togawasakiko.png`
  - 不再错误指向 `char_select_togawasakiko.png`

### 3. 当前已生成最小 release 候选物

本轮已直接成功执行：

- `./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release`

当前已生成：

- `Togawasakiko_in_Slay_the_Spire.dll`
- `Togawasakiko_in_Slay_the_Spire.pck`
- 外部 `mod_manifest.json`

### 4. 当前已完成第一次安装准备

本轮已直接执行：

- `./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --create-mods-dir`

当前游戏目录内已存在：

- `SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire.dll`
- `SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire/Togawasakiko_in_Slay_the_Spire.pck`
- `SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire/mod_manifest.json`

### 5. 当前仍未确认的事情

虽然 build / export / install 已经打通，但本轮仍没有拿到足够干净的最新游戏日志来确认：

- loader 已发现这个 mod
- `Initialize()` 已真正命中
- 角色按钮已在游戏里出现
- 角色已能稳定进局

因此当前状态应更新为：

- 已从“源码骨架未闭环”推进到“可 build / 可导出 / 可安装”
- 但仍未推进到“已确认游戏内首轮验证通过”

## 六 T4 首轮游戏内闭环与选角收口

日期：`2026-03-27`

本段用于补记 `2026-03-26` 晚间到 `2026-03-27` 完成的首轮实机验证与收口结果。

### 1. 当前已确认的首轮游戏内验证结果

本轮已通过从 Steam 正常启动游戏完成首轮验证，当前已确认：

- mod 已被 loader 发现并正常初始化
- `Togawasakiko` 已出现在角色选择界面
- 角色当前已可被选中
- 角色选择主图、背景图、顶栏头像、starter relic 图已在游戏内落地
- 首轮最小角色闭环已经成立

当前阶段判断应更新为：

- 已从“可 build / 可导出 / 可安装”推进到“已完成第一次最小可运行闭环”
- 当前后续主线可从“让角色出现并能选中”切换到“补完剩余资源 / 继续做战斗内机制验证”

### 2. 本轮查实并修掉的核心断点

#### 2.1 资源导入产物未随 PCK 落地

此前“角色头像 / 选角背景 / relic 图没有落地”的根因已查实：

- `pack/` 下的 `png` 资源虽然存在
- 但运行时实际读取的是 `.import` 指向的 `.ctex`
- `.godot/imported/*.ctex` 没有稳定随 PCK 一起落地

本轮已改为：

- 在 `build-mod.sh` 中先跑 Godot 导入
- 再把导入产物转存到可见的 `runtime_imports/`
- 同步重写 `.import` 中的 `path` 与 `dest_files`

结果：

- 自定义角色相关 `png` 资源当前已可在挂载后的 PCK 中被正常加载

#### 2.2 角色选择流程中途异常导致选中态与 relic 预览失真

此前“祥子头像保持灰色 / 没有黄框 / starter relic 预览沿用上一个角色”的真正根因，本轮也已查实并修掉：

- 点击 `Togawasakiko` 按钮时，原生 `NCharacterSelectScreen.SelectCharacter(...)` 已被调用
- 但在生成 starter relic 描述时抛出异常：
  - `RelicModel.get_Pool() -> Sequence contains no matching element`
- 根因是：
  - `DollMask` 虽然被设为 starter relic
  - 但没有被放进 `TogawasakikoRelicPool`

本轮修复后：

- `DollMask` 已进入 `TogawasakikoRelicPool`
- 角色选择流程不再在 starter relic 预览阶段中断
- 选中态、starter relic 图标与描述随之恢复正常

说明：

- 之前用户观察到“如果上一个角色是 Ironclad / Defect，则祥子的 relic 图与文本会沿用上一角色”的现象，并不是 atlas 简单缺图
- 而是角色选择流程在中途异常退出，导致 UI 没有完整刷新

### 3. 当前已完成的视觉收口

本轮已对选角界面做了最小必要视觉调整：

- 选角背景 scene 已从“整屏拉伸裁切”改为“固定尺寸摆放”
- 并根据实机反馈继续做了水平偏移微调
- 角色介绍文案已改为新的中文版本：

  - `丰川财团的继承人，世界的神明`
  - `将本次Ave Mujica的演出舞台选在一座高塔，她会带来震撼的表演`

当前这部分应理解为：

- 已完成首轮可用性收口
- 仍可能继续按实机观感做小幅位置微调
- 但不再属于“角色无法落地”的阻塞性问题

### 4. 当前仍保留的边界

虽然最小闭环已成立，但以下内容仍未进入“正式完成”状态：

- 顶栏 outline 仍是占位
- 多张卡图仍是占位
- 多个 power / debuff icon 仍是占位
- merchant / rest site / trail 等扩展 runtime scene 仍未接入
- 战斗内完整机制联调仍未做完

因此当前最准确口径是：

- `T4` 的“角色进入游戏并完成首轮可选中闭环”已完成
- `T4` 的“资源正式化 + 战斗机制完整验证”仍在后续推进范围内

### 5. 对后续线程的直接结论

如果下一线程继续接手，当前不需要再回头解决：

- 角色是否能出现在选人界面
- 角色是否能被选中
- starter relic 预览为什么错乱
- 自定义选角相关资源为什么不落地

下一线程可以直接把注意力转到：

1. 战斗内 starter deck / Pressure / watcher power 的继续验证
2. 剩余占位资源替换
3. 非首轮闭环必需的角色扩展 scene 接入

## 七 T4 选角与战斗内美术收口补记

本轮继续围绕“首轮可运行闭环后的美术可用性”做小范围收口，不扩写卡池与新机制。

### 1. 透明资源问题已再次核实并修正

用户重新提交了两张已扣背景的角色资源：

- `incoming_assets/top_panel_portrait/top_panel_portrait_togawasakiko.png`
- `incoming_assets/character_in_combat_portrait/character_in_combat_portrait_togawasakiko.png`

本轮已再次核实两者均带透明通道，并已覆盖到当前实际使用的资源位：

- 顶栏头像已重新生成并替换
- 战斗内静态立绘已重新替换进 `pack/mod_assets/character/`

说明：

- 之前出现的白底并非游戏渲染自动补白
- 而是此前接入的那一批源图本身没有 alpha 通道
- 本轮在用户重交透明资源后，这一问题应已具备收口条件

### 2. 战斗能量球改为静止层表现

此前用户反馈：

- 祥子的能量球并非完整圆形
- 而是月牙式、带缺口的图形
- 继续沿用原版“部分层旋转、部分层静止”的表现，会明显暴露上下层错位

本轮处理：

- 保留原 scene 结构
- 但把实际可见的 `Layer2` / `Layer3` 从 `RotationLayers` 中移出
- 使其视觉上不再跟随旋转层运动

结论：

- 这不是原版系统错误
- 而是自定义月牙形素材不适合继续沿用原版能量球的旋转表现
- 当前已改为更稳的静态重合方案

### 3. 战斗内立绘位置做了连续微调

用户在实机中确认：

- 新立绘虽然已成功替换
- 但角色整体仍然过低
- 脚部低于角色生命值条，不符合期望构图

本轮已继续微调 `pack/scenes/creature_visuals/togawasakiko.tscn` 中的 `Visuals.position`，并在上一轮基础上再次上提。

当前口径：

- 这一轮之后，战斗内立绘位置已进入“接近完成、仅可能再做极小幅微调”的状态
- 角色选角界面与战斗内视觉问题，已基本脱离阻塞级 bug 范畴

### 4. 主线优先级已转向卡牌命名与效果实现

经本轮确认，后续主线不再继续停留在角色基础落地与选角美术修补。

下一阶段建议按以下顺序推进：

1. 先冻结首批卡牌名字与效果定义
2. 再按已冻结的卡牌名字逐张接收与落库卡图资源
3. 之后继续做卡牌实现、效果验证与战斗内联调

这一点与用户当前提供资源的节奏一致：

- 卡图资产将按“已确定名字”的顺序逐步上传
- 因此实现线程应先完成卡牌命名与效果表，再持续接图
