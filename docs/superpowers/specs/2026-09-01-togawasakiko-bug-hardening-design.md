# Togawasakiko 兼容性与联机风险收口设计

日期：2026-09-01  
状态：聊天方案 A 已确认，待本文书面审阅

## 目标

从 PR #4 当前远端头 `8b5e17ee` 建立可复现的修复分支，收回已经在
2026-07-29 工作区中验证过、但尚未进入远端的联机修复，并处理当前已确认的三类
结构性风险：复制原版 Darv 流程、静态绑定游戏私有字段、Mac 单端引用构建。

本轮不发布成品，也不修改人物立绘、动作、Spine、Godot 场景或 PCK。修复提交将交给
正在独立推进的 `v0.2.2` 发布工作树集成，避免两个任务同时改写同一批美术与发布资产。

## 事实源与隔离边界

- Git 基线：`origin/codex/summarize-project-status` 的 `8b5e17ee`。
- 原版 API 基线：STS2 `v0.107.1`，commit `59260271`。
- 原版行为依据：当前 Mac 与 Windows 程序集的反编译结果，不依据旧版本记忆推断。
- 2026-07-29 主工作区修改只作为候选补丁来源；每项都要重新对照原版后单独移植，
  不整体复制目录。
- 当前脏主工作区、人物动作工作树和发布工作树均只读，不 reset、不 checkout、不覆盖。
- 本修复分支不得修改 `assets/`、`incoming_assets/`、`pack/` 中的视觉资源、PCK、
  发布压缩包或动作测试资产。

## 已知基线

- `8b5e17ee` 使用 Mac arm64 引用可以 Release 构建，结果为 0 error、4 个既有可空性
  warning。
- 现有 single-switch 动作测试依赖 Pillow；当前系统 Python 缺少该依赖，因此动作测试
  基线是“环境未满足”，不是本轮代码失败。本轮不改动作目录。
- Windows `v0.107.1` 引用保存在主工作区的忽略目录中；新工作树不能假定该目录被 Git
  跟踪，因此验证入口必须接收外部引用目录参数，不能写死本机绝对路径。

## 修复架构

### 1. Darv 回归原版流程

删除 `Patches/DarvPatches.cs`，不再 prefix 接管
`Darv.GenerateInitialOptions()`。

原版 `Darv` 已负责：

- 依据当前 act 和 run modifier 过滤 relic 候选；
- 使用事件 RNG 打乱并选择 relic；
- 随机决定是否加入 `DustyTome`；
- 调用 `DustyTome.SetupForPlayer(owner)`。

原版 `DustyTome.SetupForPlayer()` 又会从当前角色已解锁的 Ancient 卡池中选择合法卡。
祥子卡池已有 Ancient `Curseslander`，因此模组只需保证该卡确实属于祥子卡池、稀有度为
Ancient，并保持原版奖励流程可见，不需要复制 Darv 的选项表。

### 2. 逐项恢复 2026-07-29 联机修复

以下修改按逻辑逐项移植，不按文件整包覆盖：

1. 开局迁移遍历 `RunState.Players`，以稳定顺序处理每位玩家的 Shadow 卡实例和
   Two Moons 遗留费用；本地玩家解析不得决定全局 RunState 的迁移范围。
2. `ImprisonedXii` 触发额外抽牌时，按原版 Hook 生命周期将任务交给
   `HookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion`，避免洗牌或后续选牌等待
   一个未登记任务。
3. 已有 `PlayerChoiceContext` 的 Pressure 与 Power 操作继续透传该 context；删除会创建
   detached context 的 gameplay 重载。没有上游 context 的 Power hook 使用原版同类
   Power 的 `ThrowingPlayerChoiceContext` 方式。
4. 战斗 watcher 按 `CombatState.Players` 顺序安装。未接入当前 combat、重复 watcher、
   安装后数量不为 1 都是 gameplay 状态错误，必须显式失败，不能 catch 后继续产生分叉。
   多祥子同时存在时，一次 Pressure 兑换只能由一个稳定确定的 watcher 处理。
5. Teiji UI 不再反射写入 `Player.RunState`。`BestCompanion` 与 `BlackLimousine` 按原版
   `DustyTome.AfterObtained()` 使用
   `RunState.CreateCard -> CardPileCmd.Add -> PreviewCardPileAdd` 加牌。
6. `MagneticForceHellWargodPower` 的 replay 集合采用 mutable power 实例上的 nullable
   backing field 和惰性初始化，不能由 canonical model 与 mutable clone 共享集合。

每项移植前都要确认候选改动仍符合 `v0.107.1` 的原版签名和生命周期。无法从原版或当前
程序集证明的候选改动不进入本分支。

### 3. 私有反射降级规则

所有 Harmony patch 类型在类加载时执行的静态 `FieldRefAccess` 都属于高风险入口：字段名
变化会在模组初始化阶段抛错，使不相关 gameplay 一并见红。

本轮采用以下规则：

- 先确认原版公开属性、节点 API 或既有 mod helper 是否可以完成同一件事；可以时删除
  私有字段访问。
- 确实没有公开入口的可选 UI 功能，在 patch 调用时惰性解析成员。解析失败只跳过该功能，
  记录一次包含类型和成员名的 warning，并继续执行原版方法。
- 不缓存由失败解析产生的类型初始化异常，也不通过 prefix 的 `false` 抑制原版流程。
- gameplay 同步、卡牌结算和 run 状态不使用“失败后静默继续”的 UI 容错规则。

优先审计：

- `CardLibraryPatches`：祥子卡池筛选是可选 UI 扩展；私有字典不可用时只缺少专属筛选
  toggle，卡池和游戏流程仍应加载。
- `TogawaEventRoomPatches`：先删除对 `Player.RunState` 的写入；只保留无法由原版
  `NEventRoom` 流程覆盖且有证据需要的展示兼容代码，并改为惰性、局部失败。

### 4. Mac / Windows 双引用构建

项目文件增加 `Sts2ReferenceDir` 与 `HarmonyReferencePath` 两个可覆盖属性。
`sts2.dll` 和 `GodotSharp.dll` 从 `Sts2ReferenceDir` 解析；`0Harmony.dll` 从
`HarmonyReferencePath` 解析。默认值继续指向仓库中的 Mac arm64 与 shared
`v0.107.1` 引用。验证脚本接收 Mac 和 Windows 引用目录，Windows 构建显式把
`HarmonyReferencePath` 指向 Windows 证据目录中的 `0Harmony.dll`，两次构建分别写入
独立输出目录。

约束如下：

- 不按运行平台编译不同 gameplay 分支；双构建只用于证明同一源码同时满足两端 API。
- 不把本机绝对路径提交到项目文件。
- 两次构建都必须使用 `v0.107.1` / `59260271` 证据目录。
- 分别记录使用的 `sts2.dll`、`GodotSharp.dll`、`0Harmony.dll` SHA-256。
- 若两次生成 DLL 不同，先解释程序集元数据差异，再由发布流程选定单一成品；不得把两个
  同名 DLL 混入一个包。

### 5. 版本与发布身份门禁

本修复分支不直接打 tag、GitHub Release 或覆盖本机安装。新增的发布验证入口必须要求调用方
显式传入预期版本，并检查：

- source manifest 与 pack manifest 版本一致；
- staged 外部 manifest 与 source manifest 一致；
- `min_game_version` 一致且为当前支持基线；
- 待发布 DLL、PCK、manifest 和 zip 的哈希来自同一次 staging；
- 发布目录只包含允许的三个安装文件，zip 只有一个顶层 mod 目录。

`v0.2.2` 的版本更新、PCK 构建和最终发布只在独立发布工作树中完成一次。本分支提供经验证
的代码提交和门禁，避免以相同 `0.2.1` 标签覆盖不同 DLL，也避免抢先修改动作资产。

## 错误处理

- 可选 UI 注入失败：记录一次 warning，保留原版 UI 和 gameplay。
- 原版可完成的事件或奖励流程：删除自制替代流程，不做 fallback 复制。
- 联机 gameplay 不变量失败：抛出带玩家、combat 和数量信息的异常，阻止客户端在不完整
  状态下继续。
- 引用版本、manifest 或 staging 身份不一致：验证脚本非零退出，禁止进入安装或发布。

## 测试策略

先增加只依赖 Python 标准库的源码契约测试，并确认其在当前基线上针对待修问题失败；随后
逐项修到通过。契约至少覆盖：

- Darv patch 不存在，`Curseslander` 仍属于祥子 Ancient 卡池；
- run-wide 迁移不以 `LocalContext` 作为玩家集合；
- `ImprisonedXii` 使用受管理的 Hook choice task 生命周期；
- gameplay Pressure/Power 路径没有 detached context；
- Teiji 不写 `Player.RunState`，两件 relic 使用原版加牌命令链；
- replay 集合是每个 mutable power 的实例状态；
- 高风险 patch 不含静态 `FieldRefAccess` 初始化；
- 项目引用可由外部目录覆盖，版本/打包门禁能拒绝不一致输入。

验证顺序：

1. 运行新增契约测试。
2. 使用 Mac `v0.107.1` 引用 Release 构建。
3. 使用 Windows `v0.107.1` 引用 Release 构建。
4. `git diff --check`。
5. 检查 diff 路径，确认没有视觉资产、动作、PCK 或发布包变化。

自动化结果只能证明源码契约和双端 API 编译兼容。Darv、Teiji、卡牌打出、jukebox 和联机
行为仍必须分别标记为“待 Mac 实机”或“待 Windows 实机”，不能写成已经实机修复。

## 提交与交付

按风险分组提交：测试与双端构建门禁、Darv 原版回归、联机状态修复、反射局部降级、文档
同步。完成后提供本分支的提交范围和验证证据，由 `v0.2.2` 发布工作树显式合并或 cherry-pick。
在合并决定前不改 PR #4 远端头，不创建 tag，不发布 Release。

## 不在本轮范围

- 人物立绘、死亡动作、五个常规动作、Spine promotion、Godot import 和 PCK。
- 新卡、新 relic、新事件或能量计数器视觉修正。
- 对未取得实机日志的 Win 卡牌卡中间、jukebox 换房问题宣称完成运行时修复。
- Steam 启动和真实双人联机自动化。

## 验收标准

- 分支历史从 `8b5e17ee` 起步，主工作区和另外两个工作树未被修改。
- 上述源码契约测试经历预期失败后全部通过。
- Darv 完全由原版 `GenerateInitialOptions()` 与 `DustyTome` 处理祥子。
- 2026-07-29 六组联机修复逐项重新验证并移植。
- 可选 UI 私有成员缺失不会导致模组类初始化失败；gameplay 状态错误不会被吞掉。
- 同一源码分别通过 Mac 与 Windows `v0.107.1` 引用构建，warning 不多于基线 4 个。
- diff 不包含视觉资源、动作、PCK 或 release artifact。
- 文档明确区分源码、构建、安装、Mac 实机和 Windows 实机状态。
