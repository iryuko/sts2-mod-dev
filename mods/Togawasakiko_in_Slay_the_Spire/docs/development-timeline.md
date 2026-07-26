# Togawasakiko 开发时间线

记录日期：2026-07-26

## 时间证据说明

- 角色文件最早创建于 2026-03-24。
- 首次完整 Git 提交是 2026-04-23。
- 因此前一个月的设计和实现记录不是“倒填日期”，而是提交前工作后续整体入库。
- 下面按文档日期、文件时间、Git commit/tag 与当前代码交叉整理。

## T2/T3：设计与资源基础

### 2026-03-24

- 冻结角色控制/压制定位。
- 建立压力系统、4 张压力衍生牌和原创 debuff。
- 冻结 starter、起始牌组、初始生命和卡池扩展方向。
- 建立命名、本地化与资源路径规则。

### 2026-03-25 至 2026-03-27

- 建立资产提交工作流和卡牌实现表格规范。
- 完成原版角色资产盘点。
- 开始首批 song/压力联动卡。
- 打通 build、PCK、安装、选人和首战闭环。

### 2026-03-29 至 2026-03-31

- 扩入第二批 song 卡。
- 集中修复：
  - `ValueProp.Move` 伤害/格挡链。
  - `CrucifixX` AOE/X 费。
  - `SakiMovePlz` 条件高亮。
  - reward、merchant、`Compose` 污染。
  - `KillKiss` 胜利结算。
  - 文本能量 icon 与卡牌费用图分流。
  - merchant 稀有度桶与 scene 契约。

原始过程见 `docs/archive/phase-logs/`。

## T4：完整角色实现

### 2026-04-04 至 2026-04-06

- 建立正式音频库、track registry 和 jukebox。
- 接入先古之民设计、relic、relic-granted 卡。
- 实装更多 normal 卡和 power。
- 角色进入持续可玩但高风险流程待收口阶段。

### 2026-04-08 至 2026-04-14

- 研究战斗角色静态图与 Spine 资源契约。
- 扩充卡图、角色 scene 和资源导入链。

### 2026-04-16 至 2026-04-20

- 做全局 debug 回收，收窄 reward/merchant fallback。
- 同步 STS2 `v0.103.2`。
- 接入 `UnattendedPiano`、Shadow 卡、事件图片和音乐。
- 对齐原版事件掉血和 `AddCursesToDeck` 流程。

### 2026-04-23 至 2026-04-25

- 2026-04-23 首次完整提交角色项目。
- 修复压力 tooltip 和 jukebox/火堆音频时序。
- 2026-04-25 发布 `v0.2.1`。
- 修正 `Symbol III` 永久格挡、升级预览和若干卡牌原版路径。

## T5：玩家反馈与多人兼容

### 2026-04-26 至 2026-04-28

- 修复 Shadow 跨战斗计数、`Perk Up` 污染、`Touch of Orobas` starter 升级。
- 处理帝皇蟹方向和 mixed multiplayer 事件限制。
- 冻结压力在多人局中为目标共享 counter。
- 修复本地玩家解析、combat watcher owner 和多人首战同步。
- 2026-04-28 发布 multiplayer sync 测试包。

### 2026-05-04

- 完成 dev console 指令研究。
- 接入卡牌库角色筛选。
- 当前卡牌库方案依赖私有字段，成为后续版本风险。

### 2026-05-13

- jukebox 目标从“进入 combat 自动 Off”改为“跨所有 room 持续播放”。
- 播放器迁移到 scene tree root。
- 引入 FMOD/Godot BGM 遮罩和 run-exit cleanup。

## 资产、Ancient 与事件修复

### 2026-06-10

- 对 STS2 `v0.103.3` 做兼容审计。
- 确认旧 PCK extract 已不能代表当前 scene/API。

### 2026-06-14

- 发布 v0.2.1 资产与修复版。
- 修复 mixed multiplayer 下 `UnattendedPiano` 与 Teiji。
- 修复 Teiji 地图节点资源。
- 定位 Darv `DustyTome` 对空 Ancient 卡池取值导致卡死。
- 增加过渡 `DarvPatches.cs`。
- 新增 Ancient 卡 `Curseslander`，恢复合法 DustyTome 候选。

### 2026-06-15

- 定位 `Aroma of Chaos` 升级后卡死为 `TOGAWASAKIKO.aromaPrinciple` 缺失。
- 补齐原版按角色 ID 拼接的同类 localization key。
- 发布事件本地化修正版。

## Windows 与 v0.107.1

### 2026-06-24

- 玩家报告 Windows 牌停在屏幕中央。
- 玩家报告 jukebox 进入新 room 停歌。
- 建立 Win 证据清单，停止把问题简单归因于 OS。

### 2026-07-03

- 收到 Windows `v0.107.1` 文件。
- 确认旧 release 直接绑定已变化的 generated-card API。
- 发现 jukebox `_ExitTree` 生命周期假设过宽。

### 2026-07-04

- 继续发现 `PowerCmd`、`HookPlayerChoiceContext`、`Creature.CombatState`、AOE targeting 和虚方法签名变化。
- 当前 Mac 游戏也升级到同一 API 形态，因此 Win 兼容改动不是平台特例。
- 更新编译引用到 `v0.107.1`。
- 消除 `NRunMusicController.UpdateTrack` Harmony 重载歧义。
- 生成 `v0107-api-sync-no-energy-scene` 回归基线包。

## 当前稳定性阶段

### 2026-07-21

- 静态确认安装版与 release 哈希一致。
- 确认已知旧 member reference 不再存在。

### 2026-07-23

- 定位 `UnattendedPiano` 同进程 SL 卡死为 canonical/mutable 浅拷贝共享列表。
- 按原版 mutable event 状态模式修正。
- 删除重复 visited-event 私有反射。
- 发现同类共享集合和静态 private `FieldRefAccess` 风险。
- 战斗 Spine v2 启动独立制作和验证；本轮兼容修复继续保留静态战斗立绘基线。

### 2026-07-26

- 主菜单见红确认来自禁用 Watcher 的旧 dependency schema。
- Togawasakiko initializer 实际正常。
- 补 `min_game_version` 并修构建脚本 manifest 传递。
- Steam 实机确认主菜单恢复正常。
- 文档档案整编：旧 T3/T4/T5 追加日志归档，建立当前状态、资产状态和本时间线。

## 当前未闭环

- Darv patch 尚未回归原版。
- `UnattendedPiano` SL 修复待实机。
- Win 卡中间与 jukebox 换房待同包回归。
- 共享 mutable collection 与 private FieldRef 风险待收口。
- 角色主流程需要一次系统回归。
