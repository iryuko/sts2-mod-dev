# 项目开发时间线

记录日期：2026-07-26

## 证据规则

本时间线由以下证据交叉重建：

- Git commit 与 tag：表示进入版本库或发布节点。
- 文档内日期：表示当时记录的研究或实施日期。
- 文件创建/修改时间：用于补足提交前存在、后续整体入库的材料。
- 当前源码与发布物：用于判断历史计划最终是否落地。

Togawasakiko 的 Git 首次提交是 2026-04-23，但角色文档和文件创建时间可追溯到 2026-03-24。两者不矛盾：3 月下旬到 4 月上旬的工作是在首次正式提交前完成的。

## 时间线

### 2026-03-10：工作区与加载链研究起点

- 检查 macOS Steam 游戏目录和 `.app` bundle。
- 初始游戏版本为 `v0.98.2`，commit `f4eeecc6`。
- 同步 `sts2.dll`、`0Harmony.dll`、`GodotSharp.dll` 和 release metadata。
- 建立 SmokeMod、manifest、日志路径和加载 warning 的最小验证记录。
- 当时关于安装位置和 manifest 的一部分内容仍是候选判断，现已归档。

### 2026-03-11 至 2026-03-16：仓库建立与旁支兼容研究

- 2026-03-11 完成仓库初始导入。
- 建立构建、安装、打包、同步和 mod consent 脚本。
- 调查 UnifiedSavePath 在 macOS 的 Harmony 与存档路由行为。
- 2026-03-14、03-16 合并 UnifiedSavePath 的 Mac/Win 兼容工作。
- 这些内容后来不再是主线，现保留在 `docs/archive/side-projects/`。

### 2026-03-17 至 2026-03-22：机制旁支与原版资产研究

- 调查 `Primal Force`、`Giant Rock` 与 Strike tag。
- 渲染原版角色选择背景和战斗角色场景，建立视觉参考图。
- 相关研究分别归入旁支档案和 `docs/research/`。

### 2026-03-23 至 2026-03-29：Togawasakiko T2/T3 设计与首轮实现

- 建立角色机制、压力系统、起始牌、卡池、命名和资源规范。
- 冻结 4 张压力衍生牌与核心 debuff 方向。
- 建立资产目录、音频目录和首批 song 卡设计。
- 3 月 26 至 27 日打通首次 build、导出、安装、选人和首战闭环。
- 3 月 27 至 31 日集中处理奖励、商店、`Compose`、卡牌动态值、图标和胜利结算问题。
- 这一时期的追加式工作报告与 bug 日志现已归档。

### 2026-04-01 至 2026-04-06：原生 loader 与 T4 扩展

- 游戏参考更新到 `v0.99.1`。
- 确认当前 macOS 安装根为 `.app/Contents/MacOS/mods/`。
- 当前原生安装成品收敛为 DLL、PCK、外部 `mod_manifest.json` 三件套。
- Togawasakiko 扩展正常卡池、audio registry、jukebox、先古之民设计与 runtime 资源。

### 2026-04-19 至 2026-04-23：事件、音乐与第一轮公开入库

- 针对 `v0.103.2` 重新核对 API。
- 接入普通事件 `UnattendedPiano`、Shadow 卡与事件音乐。
- 修正事件掉血、发牌和本地化 key。
- 2026-04-23 首次将完整 Togawasakiko 项目正式提交到 Git。
- 同日修复压力 tooltip 和 jukebox/火堆音乐冲突。

### 2026-04-25 至 2026-04-28：v0.2.1 与 T5 反馈轮

- 2026-04-25 发布 `v0.2.1`。
- 对照原版修复 `Symbol III` 永久属性、Shadow 跨战斗、`Touch of Orobas`、`Perk Up` 和多人同步。
- 2026-04-28 发布多人同步测试包并冻结多人压力共享语义。

### 2026-05-04 至 2026-05-13：调试入口与 jukebox 目标调整

- 建立当前 dev console 指令参照。
- 卡牌库入口采用私有字段注入方案，留下版本漂移风险。
- jukebox 的设计目标改为选曲后跨所有 room 持续播放，包括 combat；离开 run 或选择 `Off (null)` 才停止。
- 早期“进入 combat 自动 Off”的口径自此失效。

### 2026-06-10 至 2026-06-15：本体更新、资产发布与事件修复

- 对 STS2 `v0.103.3` 做兼容审计。
- 2026-06-14 发布完整资产与修复版，加入 Teiji、多人事件兼容和 Darv 防护。
- 为 Darv 的 `DustyTome` 补入 Ancient 卡 `Curseslander`。
- 2026-06-15 修复 `Aroma of Chaos` 所需 `aromaPrinciple` 等角色本地化 key。
- 同期完成 Teiji 地图节点、Ancient 卡图和多项角色资产接入。

### 2026-06-24 至 2026-07-04：Win 反馈与 v0.107.1 API 同步

- Windows 玩家报告牌停在屏幕中央和 jukebox 换房停歌。
- 2026-07-03 收到 Windows `v0.107.1` 文件，确认问题主要来自版本/API 漂移，而不是简单的平台文件路径差异。
- 发现并兼容：
  - generated-card API 参数变化；
  - `PowerCmd` context 参数变化；
  - `Creature.CombatState` 返回类型迁移为 `ICombatState`；
  - AOE targeting 参数迁移；
  - 多个 power/relic 虚方法签名变化；
  - `NRunMusicController.UpdateTrack` 重载导致 Harmony 歧义。
- 2026-07-04 完成对 `v0.107.1` 的编译级同步并生成兼容包。

### 2026-07-21 至 2026-07-23：静态复核、SL 状态与 Spine 重启

- 复核安装包和 release 哈希，确认高风险旧 member reference 已移除。
- 定位 `UnattendedPiano` 同进程 SL 卡死：mutable clone 与 canonical event 共享 `_remainingShadows` 列表。
- 按原版 mutable-state 模式修正并移除重复 visited-event 私有反射。
- 全局扫描发现 `MagneticForceHellWargodPower` 共享集合和多个静态 `FieldRefAccess` 风险。
- 战斗 Spine v2 进入独立制作与验证阶段；制作资产不纳入本轮兼容修复提交。

### 2026-07-26：主菜单红字、工作区清理与档案整编

- 主菜单红字根因确认是禁用 Watcher 的旧式 dependency schema，不是 Togawasakiko 初始化失败。
- 迁移 Watcher manifest，补齐 Togawasakiko `min_game_version`，Steam 实机启动恢复正常状态。
- 清理约 5.37 GiB 可再生缓存与已取代发布包，保留原版参考、双端 DLL、Spine/SAM 生产资料和当前 release。
- 开始本次文档整编：分离当前事实源、研究资料、旁支项目和历史过程日志。

## 当前阶段结论

项目已经从“研究如何加载 mod”转为“维护一个可发布的复杂自定义角色 mod”。加载链、SmokeMod 和旁支实验是历史基础；当前资源和工程决策必须服务于 Togawasakiko 的稳定性、双端兼容和原版流程对齐。
