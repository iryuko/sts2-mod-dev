# 丰川祥子卡牌设计工作台

2026-09-21 新建的持续研究区。**请文档/资产整理会话保留这里，不归档、不合并进旧设计稿，不把提案写成已实现。** 本区不修改游戏。

- [打开交互图](index.html)
- [设计评价与三个攻击候选](design-review.md)
- [机器可读图谱](graph.json)
- [旧 PR 处理记录](../../../../docs/audits/pr-reconciliation-2026-09-21.md)

## 基线与范围

以已发布 `0.2.3` 的 `057b6396b0eb26d217eadade0be21b07d30c07f6` 为源码依据。
远端 main 的整合提交为 `d1df1c45e340a44b6f3e49fd0538ef5cc677844e`，二者 Git tree 相同。
共享工作区的旧 `src/` 不是这一基线，不能混读。

完整登记 **72 个具体 mod 卡牌类**：63 主池、4 压力衍生、3 往日之影、2 遗物授予。
其中常规奖励可选 58 张，Song 26 张。另有 3 个未批准提案，不计入游戏卡池。
当前 827 条有向条件边中，781 条涉及现有牌、46 条涉及提案。
同一对卡可因不同机制存在多条边；这些数字不是独立组合数，也不是强度评分。

“全卡池”保证全部节点覆盖，**不宣称穷尽所有组合**。
不会因为两张卡都带 Song/抽牌标签就自动连边；随机生成资格也不等于必定获得。
PerkUp 的主要输出是原版攻击牌，ShadowOfThePastI 的主要输出是最大生命，故允许没有直接 mod 卡牌边。
原版生成牌、遗物、敌人和房间属于边界，不伪装成本 mod 卡牌节点。

## 使用

### 本机编辑入口

在本目录运行以下命令，打开终端输出的本机URL：

```sh
/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3 server.py
```

默认仅监听 `127.0.0.1:8765`；端口占用时选择空闲端口，不停止别的服务。
其他机器可用Python 3.10+，先安装 `requirements-workbench.txt`。本机已验证Python 3.12/Pillow 12.3.0。
使用 `--port 0` 可始终自动选端口，`--workspace /absolute/path` 可选择另一个独立保存目录。
同一工作区只允许一个服务进程；多个浏览器页可以读取，旧revision写入会被拒绝。

图谱保留原卡目录；“新建草案”或“复制选中卡”进入编辑器。
填写名称、基础/升级费用、效果、独立词条、Song、来源、品质，并上传自己的卡图。
效果正文输入 `@` 选择卡名引用；保存的是稳定ID，不靠同名判断。
未填完的草案可以保存，纳入全局前需要完整的基础字段。
关系建议依据显式机制、类型、已知词条和Song资格；**不会从自由卡文自动理解任意效果**。

“试放并分析”只改变当前预览；关系可以确认、改写、拒绝，或手工添加。
“纳入全局提案”保存当前卡牌和确认关系的快照，仍是未实现研究提案。
继续编辑草案不会改变上一快照；再次纳入前显示字段差异。
费用、效果、机制等变化使关系待复核；改名、换图不影响机制身份。
撤回保留草稿；归档撤回快照，恢复不自动重新纳入。
没有已确认关系的草案也可纳入，不把零建议等同于差设计。

### 保存与恢复

- `workspace/workspace.json`：所有草稿、关系审核、发布快照与图片登记。
- `workspace/uploads/`：上传的原始图片；只接受可解码PNG/JPEG/WebP，最大10MiB、3200万像素，不重绘、不裁剪。
- `workspace/history/workspace-*.json`：最近20份旧JSON快照。图片不自动删除，因此旧快照仍可引用旧图。
- JSON导出不包含图片字节；完整备份应包含整个workspace目录。
- 恢复时先停止服务，保留当前JSON，再用一份history快照替换workspace.json并重启；不要在服务运行时手工改文件。
- 冲突或断线时保留本页输入、暂停后续写入。先“下载本页草稿”，再重新连接或明确丢弃本页内容并重新加载。
- 同一revision的服务重启允许重新连接继续保存；revision已变化则不能自动覆盖。
- 损坏或未知版本JSON不会被初始化成空工作区；启动会拒绝加载，需人工恢复。

服务只白名单提供页面、已知卡图和上传图片，不公开仓库根目录、源码、备份文件。
所有写入、分析和试放需要同源Origin、正确Host和当次服务token；token不进入导出。
本轮没有游戏补丁生成、C#修改、资源打包、自动Git提交、安装或release功能。

### 文件只读入口

直接在浏览器打开 `index.html`，不需要服务器或在线依赖，但只显示固定基线，不读取个人workspace。
左侧按名称/C# 类名、类型和来源筛选卡牌目录；中间按关系类型筛图，聚焦模式还可筛流入/流出。
“全局”显示所有允许的节点；“选中牌关系”仅显示该牌的一跳关系，不暗示隐藏节点互无关系。
全局为避免名称重叠，只展开选中或悬停节点的标签；目录和详情始终保留完整名称。
点击节点查看基础/升级效果，点击边查看条件、取舍及固定版本源码链接。
小屏可以缩放、拖动，详情列表也保留完整卡名和全部条件边。导出按钮保存当前子图 JSON。

库已固定版本并存放在 `vendor/`；卡图直接引用已核验发布工作树中的既有资产，不重复复制素材。
移走该工作树会使卡图失效；迁移时用正确 `--source-root` 重新生成相对路径。
本工具不是游戏百科、战斗模拟器或 balance 自动评分器。

## 数据文件

| 文件 | 职责 |
| --- | --- |
| `catalog-core.json` | 46 张非 Song 的人工效果摘要及特定关系 |
| `catalog-songs.json` | 26 张 Song 的人工效果摘要及特定关系 |
| `relation-families.json` | 经源码核对的关系族和明确批准记录的自环 |
| `proposals.json` | 尚未实现的提案节点及关系 |
| `graph-config.json` | 源提交/tree、版本、工作树和范围 |
| `build_graph.py` | 节点清点、证据定位、关系展开和校验 |
| `graph.json` / `graph-data.js` | 生成结果；JS 供离线 HTML 使用，不手工改 |
| `test_graph.py` / `test_ui.cjs` | 数据契约及浏览器回归 |
| `server.py` / `workbench_store.py` / `workbench_model.py` | 本机接口、原子保存与状态机 |
| `mechanic-profiles.json` / `build_profiles.py` | 72张卡的显式机制档案与可复现人工映射；不解析卡文猜机制 |
| `relation_rules.py` | 带条件、变体、时点和证据的候选关系 |
| `draft-editor.js` / `workbench.js` / `workbench-api.js` | 编辑器、关系审核、顺序保存 |
| `workspace/` | 用户设计数据，不能清理或当作构建缓存 |

节点 ID 使用已存在的 C# 类名，不猜测本地化 Entry；提案使用 `proposal:` 前缀。
基本字段为 `id/name/base/upgrade/tags/notes/source`；已实现节点的类型、基础费用、稀有度、来源和 Song 身份由源码核对。
`upgrade` 是升级差异说明，`base` 是人工整理的基础规则，不等同于运行时动态卡面截图。

关系必须包含 `source/target/kind/mechanism/condition/reason/evidence`。
`evidence` 为 `src/...#实际符号` 或 `proposal:...`；生成器解析为固定提交及行号。
关系类型：`supply` 资源供给、`payoff` 效果收益、`sequence` 牌序、`replay` 重放、
`generation` 生成候选、`tradeoff` 资源取舍、`conflict` 流程冲突。
普通关系族不展开自环；显式自环要求 `self_reviewed: true`，并解释同类与同实例的区别。
任一端为提案时，整条关系都标记为未实现。

## 每次设计或改牌

1. 先确定要研究的提交和干净源码。游戏本体 API 相关判断先反编译对应版本，不按方法名猜。
2. 新构想先在工作台保存草稿，标明暂定数值、升级差异、限制及原作解释是否只是设计解读；不要覆盖固定基线 `proposals.json`。
3. 对照全部现有牌补供给、收益和负向边，至少检查压力存量/兑换、力量/多段、自卑、重放、Song 资格、生命/格挡代价、多人归属。没有直接关系可以不连边，不凑数量。
4. 实现获批且代码落入选定基线后，把节点移入对应 catalog，改用实际类名，迁移所有端点/证据；不可同时保留成两张牌。旧牌改效果时同时复审其入边和出边。
5. 更新 `graph-config.json` 的版本、revision、tree 和工作树。源码适配器遇到新声明格式应明确支持，不能跳过无法解析的牌。
6. 游戏基线真正更新时，重建graph并复审机制档案，再运行测试；workspace基线迁移应另行审查，不能直接改revision冒充兼容。更新 `design-review.md` 的结论。不要把数据测试通过写成游戏内实测通过。

在仓库根目录运行：

```sh
python3 mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/build_graph.py
python3 mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/build_graph.py --check
python3 -m unittest discover -s mods/Togawasakiko_in_Slay_the_Spire/research/card-graph -p test_graph.py -v
node mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/test_ui.cjs
python3 -m unittest discover -s mods/Togawasakiko_in_Slay_the_Spire/research/card-graph -p 'test_*.py' -v
node --test mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/test_view_modules.cjs
node mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/test_workbench_ui.cjs
node --test mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/test_workbench_review.cjs
```

JS测试需要 Node、Playwright 及其 Chromium；本机验证使用 Codex bundled runtime 的 Node 与 NODE_PATH。
HTTP浏览器测试自行使用临时工作区、临时服务，结束后关闭，不会修改你的草稿。
浏览器截图输出至仓库 `local/card-graph-2026-09-21/`。该测试不会启动 Godot、Steam 或游戏。
可通过 `--source-root /absolute/path/to/checkout` 指定同一已核验树的另一检出目录。

## 证据边界

- 源码适配器只支持本项目已读过的 C# 声明格式，不是完整 C# 解析器。新牌未登记、未知结构、缺图、未知端点、缺证据、脏源码或基线漂移都应失败。
- 自动检查能证明清点和引用完整，不能证明每条人工语义永远正确。此次由核心牌与歌曲两路核对后交叉复审。
- “上一张歌”：祥，移动读取 `AfterCardPlayed` watcher；接续小节读取 `CardPlaysStarted`。嵌套打出时不要混为同一牌序。
- 压力在敌人上共享，兑换临时牌不是每个祥子都领一份；出牌/弃牌历史和消耗收益按各自 Owner 判断。
- 颜、自卑读取 `TotalDamage`，不等于生命损失。此次重新反编译发布 Mac 参考的 `DamageResult`，确认为 `BlockedDamage + UnblockedDamage`。DLL SHA-256：`e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`。这不是游戏内受击实测。
- 临时生成牌、DeckVersion、基础/升级分支、消耗后去向和回合时点都写进条件；类图仍不能表示每个运行时实例与所有嵌套顺序。
- 本区与当前新提案尚未推送到远端；已完成的远端操作仅为旧 PR 收尾和已发布源码归并。

## 本轮验证

工作台实现与最终审查记录见 [workbench-implementation-report.md](workbench-implementation-report.md)。
下述11项为最初只读图谱的历史验收，不代表工作台完整测试数。

2026-09-21：11项数据契约通过，生成结果 `--check` 通过，JavaScript语法检查通过。
Playwright在1440×1000和390×844核对搜索空结果、X费、类型/方向筛选、隐藏提案、
全局/单牌切换、条件证据、悬停标签、JSON下载、卡图和横向溢出；两端通过。
桌面/手机的聚焦与全局画布已截图人工检查并检查非空像素。研究文档本地链接检查通过。
这些只验证研究工具，不构成三张提案的游戏实现或平衡测试。

## 第三方依赖

只为离线查看打包 JS，未引入新的游戏依赖。

- [Cytoscape.js 3.33.1](https://github.com/cytoscape/cytoscape.js/tree/v3.33.1)，MIT，完整许可保留在 JS 文件头；[布局文档](https://js.cytoscape.org/#layouts/concentric)。
- [Lucide 0.468.0](https://github.com/lucide-icons/lucide/tree/0.468.0)，ISC，[本地许可证](vendor/LICENSE-lucide)。

下载源分别为 `https://cdn.jsdelivr.net/npm/cytoscape@3.33.1/dist/cytoscape.min.js`、
`https://cdn.jsdelivr.net/npm/lucide@0.468.0/dist/umd/lucide.min.js`。
SHA-256 分别为 `f55947f3daa3bae53209d4b885c195c157f595c225e508a6b382598d9452d6e2`、
`3411692820cb8d47543f69496aa25fd603a358f4498046f41c508a5a3342210e`。
