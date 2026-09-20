# 文档导航

记录日期：2026-07-26

## 当前事实源

- `docs/current-status.md`
  - 项目主线、版本、构建安装、已收口问题与开放风险。
- `docs/next-task.md`
  - 下一轮执行顺序和完成标准。
- `docs/thread-handoff.md`
  - 上下文压缩后的最小接班摘要。
- `docs/decisions.md`
  - 当前不应反复推翻的工程与产品规则。
- `docs/project-timeline.md`
  - 2026-03 至今的项目时间线与证据说明。
- `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`
  - 角色 mod 的实现级事实源。

## 专项审计

- `docs/audits/document-reorganization-2026-07-26.md`
  - 本轮档案边界、时间证据、目录重组和主要矛盾裁定。
- `docs/audits/workspace-cleanup-and-mod-error-2026-07-26.md`
  - 主菜单红字根因、manifest 修正和工作区清理边界。

## 研究资料

`docs/research/` 保存可复用但不直接表达当前任务状态的研究：

- 命名、本地化、console、资源命名。
- 原版 Ancient 资产规格。
- 原版角色卡池结构。
- 角色选择与战斗场景渲染报告。

研究资料必须结合文档记录日期和游戏版本使用。

## 模板

- `docs/templates/error-record.md`
- `docs/templates/new-thread.md`

## 历史档案

- `docs/archive/loading-chain/`
  - 2026-03-10 SmokeMod、loader、日志和最小包研究。
- `docs/archive/side-projects/`
  - UnifiedSavePath、CrossCharacterCard、PrimalForceStrike 等旁支。
- `mods/Togawasakiko_in_Slay_the_Spire/docs/archive/phase-logs/`
  - T2/T3/T4/T5 工作报告、bug 日志、旧资产清单和旧版本审计。

档案文件用于追溯，不覆盖当前事实源。

## 本机与生成报告

- `local/game-path.txt`：当前机器游戏路径。
- `local/game-layout.md`：当前 macOS 安装结构。
- `local/install-notes.md`：当前安装操作说明。
- `local/*.md`、`local/*.json` 中的导出报告：脚本生成物，不是项目状态源。

`local/tools/`、`Tools/`、`references/pck-extract/` 中的第三方 README、LICENSE 和原版 patch notes 不纳入项目文档体系。
