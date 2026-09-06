# Togawasakiko 文档索引

校订日期：2026-09-06

本目录同时保存现行事实、设计基线、专项研究和历史过程。文件中的日期代表记录形成时间；判断当前实现时，不应把早期设计稿中的“当前”直接当作今天的事实。

## 接班必读

1. [当前状态](current-status.md)：版本、卡池、事件、jukebox、资产和风险的现行事实源。
2. [开发时间线](development-timeline.md)：从 2026-03-24 到当前的阶段演进。
3. [当前资产状态](asset-status.md)：源资产、runtime 资产和兼容方案的现行盘点。
4. [未决问题](open-questions.md)：仍需设计决定或实机证据的问题。
5. [项目级当前状态](../../../docs/current-status.md)：整个工作区的版本与发布基线。

## 机制与卡牌

- [角色机制总览](mechanics-overview.md)：角色定位与机制边界；尾部 T2 状态是历史阶段总结。
- [压力系统](pressure-system.md)：压力和 4 张压力衍生牌的规则。
- [初始 Relic 与起始牌](starter-kit.md)：当前 starter 数值与起始牌组。
- [原创状态与 Debuff](original-statuses-and-debuffs.md)：压力、人格解离、自闭和自卑的分层。
- [卡池结构规划](cardpool-structure.md)：2026-03-24 的 50 张奖励卡扩展目标，不是当前实现统计。
- [Song 子集登记](song-subset-registry.md)：song 标签和登记规则。
- [卡牌实现表格规范](card-implementation-table-spec.md)：卡牌规格交付格式。
- [命名与资源约定](naming-and-resource-conventions.md)：内部名、本地化和文件命名规则。

## 事件与 Ancient

- [丰川定治设计记录](ancient-togawa-head-design-note.md)：Teiji 的设计来源；实现状态以当前状态文档为准。
- [无人问津的钢琴设计](question-room-shadow-event-design.md)：事件页面与奖励设计。
- [无人问津的钢琴资产规格](question-room-shadow-event-asset-spec.md)：事件图片、音乐和 Shadow portrait 规格。
- [上下文状态审计](context-state-audit-2026-07-23.md)：同进程 SL 卡死根因和同类风险。

## 资产与音频

- [资产状态](asset-status.md)：资产是否存在、是否进入 runtime 的唯一现行清单。
- [五张 Bridge 卡图安装审计](../../../docs/audits/bridge-card-art-install-2026-09-06.md)：来稿映射、固定路径、PCK 与安装哈希证据。
- [资源目录](resource-layout.md)：目录职责和长期落位规范。
- [资源规格与提交流程](resource-specs-and-submission-workflow.md)：素材尺寸、格式与提交流程。
- [原版角色资产审计](original-character-asset-audit.md)：原版资源结构参考。
- [音频资产库](audio-asset-library.md)：音频目录和生命周期规范。
- [音频轨道登记](audio-track-registry.md)：23 首 jukebox 曲目及 cue 登记。

## 战斗立绘与 Spine

- [Spine 资产接口](combat-spine-asset-spec.md)：原版 runtime 契约。
- [原版攻击动画审计](combat-spine-original-attack-audit.md)：原版动作和附件参考。
- [动作责任分类](combat-spine-motion-classification.md)：主动、次级、被动和 overlay 分类。
- [重绘设计简报](combat-spine-redesign-brief.md)：美术与拆件目标。
- [分层拆分方案](combat-spine-layer-split-plan.md)：slot、bone 和遮挡规则。
- [蓝图分块方案](combat-spine-blueprint-block-plan.md)：主体、头发和左臂的生产分块。
- [生产批次计划](combat-spine-production-batch-plan.md)：分批制作与验收。

本组文件是历史研究依据和生产细则。新 Spine 正在独立制作，其当前工作稿和资产不纳入本轮提交，完成后再单独更新。

`incoming_assets/**/README.md` 是与具体来稿同目录保存的生产记录，不是角色状态源。旧源包允许保留，但必须服从本索引的优先级。

## 兼容性与护栏

- [T4 经验与护栏](t4-lessons-and-guardrails.md)：原版优先、资源接线和回归经验。
- [Windows 发布问题调查](win-release-bug-investigation-2026-06-24.md)：Win 卡牌卡中间与换房停歌的证据清单。

## 历史归档

- [角色阶段日志说明](archive/README.md)
- `archive/phase-logs/`：T3/T4/T5 过程日志、旧卡牌规格、旧 bug 计划、旧资产清单和版本审计。

归档文件用于回答“当时为什么这么做”，不能覆盖当前源码、[当前状态](current-status.md)或[资产状态](asset-status.md)。
