# Togawasakiko_in_Slay_the_Spire

该目录是角色 mod `Togawasakiko_in_Slay_the_Spire` 的长期主目录。

当前文档基线仍以 `2026-03-24` 的 T2 / T3 冻结结论为主，但项目状态已经推进到 `2026-04-04` 的 T4 源码实现与 T3 资产整理阶段。

当前仍保持不变的冻结基线：

- 角色总定位已冻结
- 压力基础规则已冻结
- 当前冻结工作版压力衍生牌数量已冻结为 `4` 张
- 起始 relic 的显示层方案已冻结
- 起始牌组与两张专属起始牌的效果基线已冻结
- 少数英文内部名已冻结为当前工作版候选并完成本地避冲突核查

当前线程状态：

- `T2-角色机制设计` 已完成收尾冻结
- `T3-资源与美术资产` 已完成首批资源入库与结构搭建
- 当前主线已经进入：
  - `T4-角色实现`
  - 目标是先打通“第一次最小可运行闭环”

当前已确认：

- 已存在源码实现目录：
  - `src/`
- 已存在 manifest 源文件：
  - `manifest/`
- 已存在 PCK 打包源 / runtime staging：
  - `pack/`
- 当前 `dotnet build` 已可通过
- 当前最小 release 候选物已生成：
  - `dll`
  - `pck`
  - 外部 `mod_manifest.json`
- 当前最小安装包已安装到本机游戏目录：
  - `SlayTheSpire2.app/Contents/MacOS/mods/Togawasakiko_in_Slay_the_Spire/`
- 当前构建链路已修正：
  - 打 PCK 前会先导入 `pack/` 中的贴图资源
  - `.godot/imported` 会被一并打进 PCK
- 当前角色接入路线已切到：
  - Harmony patch `ModelDb.AllCharacters()`
  - 不再以运行时手工注入选人按钮为主线

当前仍未确认：

- 角色是否已经在游戏内正常出现
- 是否能稳定进局
- 首次战斗内 starter relic / Pressure / 衍生牌逻辑是否无报错

## 当前范围

- 先把角色 mod 推到第一次最小可运行闭环：
  - 能 build
  - 能导出
  - 能安装
  - 能做第一次进游戏验证
- 当前不扩正常卡池，不扩更多机制，不做 polish。
- 不把未确认的数值、文案、对象类型写死成结论。

## 目录用途

- `docs/`：角色机制、卡池、资产与待确认问题的主文档区。
- `assets/`：角色专属素材目录。
- `assets/character/`：角色选择大立绘、角色头像等角色本体素材。
- `assets/cards/`：卡图与卡面相关素材。
- `assets/icons/`：压力与原创 debuff icon。
- `assets/relics/`：起始 relic 等遗物素材。
- `assets/audio/`：角色音效、歌曲轨道与点歌系统复用音乐的正式库存。
- `src/`：后续实现代码。
- `manifest/`：后续 manifest 源文件与安装元数据源。
- `exports/`：后续导出产物与最终安装候选成品。
- `pack/`：当前 T4 阶段的 runtime 资源与 PCK 打包源 / staging 目录。
- `pack/audio/`：音频 runtime staging，供后续导入、打包与接线。
- `localization/`：后续本地化表与语言层资源。
- `tests/`：后续测试记录、验证清单与结果留档。

## 当前边界

- 当前不推翻既有角色骨架重来。
- 当前不优先扩更多牌或更多机制。
- 当前优先修：
  - 编译断点
  - runtime 资源接线断点
  - 文档状态落后

## 当前文档入口

- `docs/index.md`
- `docs/mechanics-overview.md`
- `docs/pressure-system.md`
- `docs/original-statuses-and-debuffs.md`
- `docs/starter-kit.md`
- `docs/cardpool-structure.md`
- `docs/naming-and-resource-conventions.md`
- `docs/asset-checklist.md`
- `docs/audio-asset-library.md`
- `docs/audio-track-registry.md`
- `docs/t3-asset-handoff.md`
- `docs/t4-implementation-status.md`
- `docs/t4-asset-integration-status.md`
- `docs/open-questions.md`
- `docs/work-report-2026-03-23.md`
