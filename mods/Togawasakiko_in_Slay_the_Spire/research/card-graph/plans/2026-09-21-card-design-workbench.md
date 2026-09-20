# Card Design Workbench Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在已有丰川祥子关系图上加入可辨识的边、卡牌引用预览，以及可靠落盘的草案设计与全局提案分析。

**Architecture:** 保留只读基线和原生前端，以小型Python本机服务保存独立工作区。规则建议在服务端计算，人工确认与已纳入快照分离；图谱仅在内存合成。前端把边样式、引用、编辑器、API状态分开，不引入框架重写。

**Tech Stack:** Python 3标准库、Pillow图片验证、HTML/CSS/JavaScript、Cytoscape 3.33.1、Lucide 0.468.0、unittest、Node内置测试及Playwright Chromium。

**Spec:** [已认可的设计规格](../specs/2026-09-21-card-design-workbench-design.md)。执行者先读规格，再按任务实现。本计划中所有新增函数与HTTP接口均为拟实现契约，不是已存在API。

## Global Constraints

- 服务仅绑定 `127.0.0.1`，不监听局域网地址。
- 现有72张已实现卡与3个固定研究提案仍是只读基线；新草案不能覆盖这些节点。
- 源码基线继续固定为0.2.3 / `057b6396`，不从脏的共享游戏源码自动重建。
- 本轮不生成C#、Harmony补丁、游戏资源包，不安装mod，不启动Godot/Steam/游戏。
- 不引入数据库、账号、云服务或模型API。
- 上传支持PNG/JPEG/WebP，单张不超过10MiB、3200万像素，以真实解码为准；不接受SVG、HTML或仅修改后缀的文件。
- 保留最近20份工作区备份；图片不自动清理，避免旧快照恢复后缺图。
- `file://`只读模式保留；草稿读写、上传及纳入提案使用本地HTTP地址。
- 本轮不自动Git推送、不创建release；研究上的纳入不是游戏实现或Git提交。
- 手工编辑用apply_patch；不改共享工作区已有的无关修改。每次提交先核对明确文件清单，禁止 `git add .`。

## Review Focus

1. 两个标签页交错保存或上传：旧revision不能覆盖新数据，冲突页仍保留未保存文本。任务3/4/6覆盖。
2. 一张卡有同名、短名或特殊符号：不能误链到另一实例，也不能把输入变成HTML；键盘/触屏能看浮层并返回原视口。任务2/7覆盖。
3. 自动保存期间发生上传失败、服务断开或撤回操作：顺序明确，旧图与编辑内容不丢失，不能虚报已保存。任务4/6/7覆盖。
4. 一个已纳入提案更新或归档，另一个提案仍引用旧机制：旧边需复核而非继续冒充已确认，也不能丢掉人工解释。任务3/5/7覆盖。
5. 未完成草稿、没有机制档案的特殊牌或带外部效果的牌：仍能保存/查看，零建议不等于无协同，也不能把生成、弃置、消耗、出牌混淆。任务3/5/7覆盖。

---

## 工作位置与验证命令

工作区根：`/Users/user/Desktop/sts2-mod-dev`。
以下文件表中的 `G/` 一律指 `mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/`，不是另建目录。
现有工具主体尚未纳入Git，只有规格已提交。执行前记录本区文件哈希与Git状态，按 using-git-worktrees 判断隔离方式；不能创建空工作树后误以为这些未跟踪文件会自动出现。
确需隔离时只复制本研究目录到指定工作树并明确 `--source-root`，不得复制整个脏仓库或覆盖发布工作树。

所有新Python测试在临时目录运行，禁止使用实际 `workspace/` 测试写入。
所有新HTTP浏览器测试自行启动测试服务、使用临时工作区，并在finally停止该测试服务。

本机运行时已存在，执行前重新确认路径：

```sh
export PY=/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3
export NODE=/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node
export NODE_PATH=/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules
cd /Users/user/Desktop/sts2-mod-dev/mods/Togawasakiko_in_Slay_the_Spire/research/card-graph
"$PY" -m unittest discover -s . -p 'test_*.py' -v
"$NODE" --test test_view_modules.cjs
"$NODE" test_ui.cjs
"$NODE" test_workbench_ui.cjs
```

Shell环境变量只在同一shell命令会话内有效；分开工具调用时使用绝对运行时路径。
任务表中运行命令均以G为cwd。旧 `test_graph.py`、`build_graph.py --check`、`test_ui.cjs` 必须持续通过。

## 文件职责

| 文件 | 职责 |
| --- | --- |
| `G/relation-style.js` | 七种关系的统一样式及图例数据 |
| `G/card-references.js`, `G/card-aliases.js` | 受控引用分词、明确别名、浮层与卡牌跳转；别名用本地脚本以兼容file模式 |
| `G/graph.js`, `G/graph.css`, `G/index.html` | 保留现有图，接入样式/引用/工作区图，必要的布局调整 |
| `G/workbench_model.py` | 草稿与关系验证、稳定机制哈希、命令状态机 |
| `G/workbench_store.py` | 文件锁、revision、原子保存、20份备份 |
| `G/workbench_assets.py`, `G/server.py` | 图片验证、白名单静态服务与HTTP路由 |
| `G/mechanic-profiles.json`, `G/relation_rules.py` | 有证据的机制档案及建议规则 |
| `G/workbench-api.js` | 请求队列、revision、保存/冲突/离线状态 |
| `G/draft-editor.js`, `G/draft-editor.css` | 草稿表单、预览、上传、复制和归档 |
| `G/workbench.js` | 视图协调、关系审核、试放与纳入/撤回 |
| `G/test_view_modules.cjs`, `G/test_workbench_ui.cjs` | 纯前端及HTTP端到端回归 |
| `G/test_workbench_model.py`, `G/test_workbench_store.py`, `G/test_workbench_server.py`, `G/test_relation_rules.py` | 对应模块的独立测试 |
| `G/README.md`, `G/.gitignore`, `G/requirements-workbench.txt` | 操作说明、只忽略锁/临时文件/缓存、Python依赖说明 |

不手改原有 `graph.json`/`graph-data.js`/catalog。真实 `workspace/workspace.json` 与uploads保持可备份，不能用忽略整个workspace的方式让用户找不到自己的设计。

## 共享接口与数据结构

### 工作区与草案

采用JSON数据，不把用户输入转换成Python/JavaScript表达式。以下是新schema v1的完整顶层约定：

```json
{"schema_version":1,"revision":0,"baseline_revision":"057b6396b0eb26d217eadade0be21b07d30c07f6","entries":{},"images":{}}
```

`entries[id]`：`id`、`origin_id`、`archived`、`working`、`reviews`、`published`、`created_at`、`updated_at`。
ID由服务端生成 `draft:<uuid>`；`published` 为null或 `{card, relations, mechanism_hash, published_at}`。
`working` 为下列卡牌字段，保存时可以不完整，纳入时做完整验证：

```json
{
  "name":"","cost":null,"upgraded_cost":null,"type":null,"rarity":null,
  "source_pool":null,"song":false,"keywords":[],"upgraded_keywords":[],
  "base":"","upgrade":"","upgrade_mode":"unchanged","notes":"",
  "image_id":null,"portrait_source_id":null,"mechanics":[]
}
```

`upgrade_mode` 为unchanged/delta/full，复制旧图的升级差异用delta，不能冒充完整升级效果。
自定义关键词仅为文本；只有已知关键词、显式机制和Song属性参与建议。未设图可使用origin的portrait_source_id预览，用户上传后改为image_id。
名称上限200字符，base/upgrade各8000，notes12000，关键词每组32项且每项64字符，mechanics最多100项，工作区最多500张草稿/10000条人工关系。
空值仅用于未完成草稿；非空枚举/数字仍须有效，拒绝bool作为费用、NaN、未知顶层字段和畸形对象。

单条机制：

```json
{"action":"produce","resource":"pressure","scope":"enemy","timing":"on_play","variant":"both","amount":"3","limit":null,"condition":"目标存活","evidence":[]}
```

action取produce/consume/read/trigger/generate/replay/sequence；scope取self/enemy/all_enemies/ally，variant取base/upgraded/both。
resource和timing采用任务5维护的枚举表；amount只是显示说明，不执行任意算式。condition必填或明确“无额外条件”。
mechanism_hash规范化除name/image_id/portrait_source_id外所有机制相关字段及受控引用ID；保留文字原貌另存，不因空白格式产生不必要的关系复核。

review：`id, source, target, kind, mechanism, condition, reason, evidence, rule_id, decision, endpoint_hashes, profile_revision, self_reviewed`。
decision为accepted/rejected；端点哈希由服务端计算。显示stale是派生状态，不删除原review。
source或target至少一个等于该工作草案ID；其他端点只能来自基线、已纳入提案或同一草案，不能引用另一个未提交草稿。
建议ID按规则与端点稳定计算；人工关系由服务生成独立ID。

### HTTP契约

| 入口 | 请求 | 响应 |
| --- | --- | --- |
| GET `/api/bootstrap` | 无 | `{token, graph, workspace, profiles_revision, capabilities}` |
| POST `/api/commands` | `{expected_revision, command}` | `{workspace, result}`；result可含新id |
| POST `/api/analyze` | `{draft_id, card}` | `{suggestions, limitations, endpoint_hashes, profile_revision}`；纯分析不写文件 |
| POST `/api/uploads` | 图片原始字节，`X-Workspace-Revision` | `{workspace, image_id}` |
| GET `/api/images/<id>` | 服务生成的图片id | 已验证图片字节 |
| GET `/api/portraits/<card-id>` | URL编码后的已知基线卡id | 基线白名单卡图 |
| GET `/api/export` | 无 | 工作区JSON下载，不包含安全token |

全部写请求及analyze要求同源Origin、正确Host和 `X-Workbench-Token`。JSON最大2MiB，上传最大10MiB；不支持chunked请求体，缺Content-Length返回411。
明确返回400格式错误、403来源/token错误、404未知ID、409revision或审核哈希冲突、413超限、415不支持类型、422领域验证、503损坏数据/写失败；错误体 `{error:{code,message,fields}}`，不泄露绝对路径或堆栈。
token启动时生成、仅用于本会话、不落入workspace/导出。GET bootstrap可只读同源读取；拒绝无关Host及跨源Origin，无CORS授权。

command的action为create/update/review/publish/withdraw/archive/restore；create带card和可选origin_id，update带id/card，review带id/relation/decision/expected_endpoint_hashes，其余带id。
成功命令每次只使revision加1；失败不加。上传和保存共用前端队列，防止图片上传先推进revision后自动保存误覆盖。

## Task 1: 七类边与可交互图例

**Files:** Create `G/relation-style.js`, `G/test_view_modules.cjs`; modify `G/graph.js`, `G/graph.css`, `G/index.html`, `G/test_ui.cjs`。

**Interfaces:** `RelationStyle.styles(kind)`返回 `{color,lineStyle,dashPattern,arrowShape}`，`RelationStyle.legend()`返回七项 `{kind,label,color,lineStyle,dashPattern,arrowShape}`。UMD导出供浏览器window与Node require共用。graph新增edge.data.kind，不通过proposal样式覆盖语义色。

- [ ] 写失败测试，验证七种组合不同且未知kind明确报错：

```js
const test = require('node:test');
const assert = require('node:assert/strict');
const styles = require('./relation-style.js');
test('seven semantic styles remain distinct', () => {
  const values = styles.legend().map(x => JSON.stringify(styles.styles(x.kind)));
  assert.equal(values.length, 7);
  assert.equal(new Set(values).size, 7);
  assert.throws(() => styles.styles('missing'));
});
```

- [ ] 运行 `"$NODE" --test test_view_modules.cjs`，确认因新增模块未实现失败。
- [ ] 先读固定版本vendor及已保存官方文档，确认solid/dashed/dotted、line-dash-pattern与端点shape支持，再实现以下映射并为图例提供同样线段样本：

```js
const palette = {
  supply:     {color:'#177d70', lineStyle:'solid',  dashPattern:[1,0], arrowShape:'triangle'},
  payoff:     {color:'#2965b3', lineStyle:'solid',  dashPattern:[1,0], arrowShape:'diamond'},
  sequence:   {color:'#947208', lineStyle:'dashed', dashPattern:[9,4], arrowShape:'triangle'},
  replay:     {color:'#96528c', lineStyle:'dashed', dashPattern:[4,3], arrowShape:'vee'},
  generation: {color:'#198da6', lineStyle:'dotted', dashPattern:[1,3], arrowShape:'circle'},
  tradeoff:   {color:'#b96b22', lineStyle:'dashed', dashPattern:[9,4], arrowShape:'diamond'},
  conflict:   {color:'#bd3f4c', lineStyle:'solid',  dashPattern:[1,0], arrowShape:'tee'}
};
```

- [ ] 图例按钮复用现有kind筛选；edge选中只淡化非关联元素，切回卡牌清理dim/highlight。同步改详情列表的线样本，不让颜色含义随视图改变。
- [ ] 跑纯模块与 `test_ui.cjs`；增加全局/聚焦下七类实际cy样式、proposal不变灰、选边后非端点变淡的断言。截图桌面/手机，核对不是七种近似灰。
- [ ] 仅提交本任务文件；若首次需纳入未跟踪旧工具，先运行原始11项数据测试及浏览器测试，把核验过的G原有文件单独作基线提交，不带其他research目录。

## Task 2: 安全卡名引用、浮层与返回栈

**Files:** Create `G/card-references.js`, `G/card-aliases.js`; modify `G/graph.js`, `G/graph.css`, `G/index.html`, `G/test_view_modules.cjs`, `G/test_ui.cjs`。

**Interfaces:** `CardReferences.tokenize(text,index,aliases)`返回text/card/missing片段；`CardReferences.render(container,tokens,onNavigate)`只建立DOM文本与按钮；`CardReferences.bindPreview(root,resolveCard,onNavigate)`绑定hover/focus/touch并返回dispose函数。
显式引用格式为 `[[card:CrucifixX]]` 或 `[[card:draft:<uuid>]]`；显示名由ID解析，不保存不可信HTML。
扩展 `window.cardGraphView`：`focusCard(id)`, `selectEdge(id)`, `capture()`, `restore(snapshot)`, `replaceGraph(graph,{preserveView:true})`；保留现有cy/state/data供测试，data为当前合成图。
capture包含state、nodePositions、zoom、pan；restore重建后回填仍存在节点的位置，失效ID回退到Completeness且告知原目标已不可用。

- [ ] 添加失败分词用例，不把“颜”“攻击”自动链成牌：

```js
test('explicit identity wins over ambiguous words', () => {
  const refs = require('./card-references.js');
  const index = new Map([['Face',{id:'Face',name:'颜'}]]);
  assert.deepEqual(refs.tokenize('颜不是攻击',index,{}),[{type:'text',text:'颜不是攻击'}]);
  assert.equal(refs.tokenize('[[card:Face]]',index,{})[0].id,'Face');
  assert.equal(refs.tokenize('[[card:missing]]',index,{})[0].type,'missing');
});
```

- [ ] 运行模块测试确认红灯；在Node环境不引用document，DOM方法只有调用时使用浏览器对象。
- [ ] 实现有限引用语法、已审查别名最长优先匹配及纯文本渲染。别名字典覆盖当前正文的TwoMoons/两轮月亮、Symbol III、Treasure Pleasure、Choir等，保留歧义词不自动链接。
- [ ] `card-aliases.js`采用UMD静态对象，经defer script装载到 `window.CardAliases`；不要在file模式fetch JSON导致跨源失败，HTTP与离线使用同一份别名。
- [ ] 建立单个浮层，hover移入浮层不立即关闭，focus可阅读，Escape关闭，触屏先预览后明确跳转。浮层定位限制视口，长效果滚动而不遮住关闭/进入详情操作。
- [ ] 给card/edge正文接入render；进入卡牌前capture入栈，返回restore。归档草稿引用从完整工作区resolve而不是当前可见nodes查找。
- [ ] 扩展浏览器测试，真实hover/focus/touch、输入`<img src=x onerror="window.injected=true">`仅为文本、同名两ID、缺失ID、连续跳转返回两次，验证原kind/direction/zoom/pan/选中边恢复。然后跑旧test_ui。
- [ ] 仅提交本任务文件。

## Task 3: 工作区状态机与可靠存储

**Files:** Create `G/workbench_model.py`, `G/workbench_store.py`, `G/test_workbench_model.py`, `G/test_workbench_store.py`; modify `G/.gitignore`。

**Interfaces:**

```text
empty_card() -> dict
validate_card(card: dict, *, publishing: bool = False) -> dict
mechanism_hash(card: dict) -> str
reduce_command(state: dict, command: dict, baseline: dict, profiles: dict) -> tuple[dict, dict]
compose_graph(baseline: dict, state: dict, profiles: dict, preview: dict | None = None) -> dict
WorkspaceStore(root: Path, baseline: dict, profiles: dict | None = None)
WorkspaceStore.read() -> dict
WorkspaceStore.apply(command: dict, expected_revision: int) -> dict
WorkspaceStore.close() -> None
```

以上为接口签名。领域错误使用带code/fields的 `ValidationError`，并发用 `RevisionConflict`，坏数据用 `CorruptWorkspace`，重复进程用 `WorkspaceLocked`，全部在workbench_model.py定义。
Store.apply返回 `{workspace,result}`，不会原地修改read返回的对象。published深拷贝working，只纳入accepted且哈希仍匹配的关系。compose_graph不改baseline，预览相同ID时替换其published节点。

- [ ] 先写生命周期与同名独立ID测试；以下例子以临时目录运行：

```python
def test_published_snapshot_survives_working_edit(self):
    from workbench_model import empty_card
    from workbench_store import WorkspaceStore
    with tempfile.TemporaryDirectory() as tmp:
        store = WorkspaceStore(Path(tmp), {"meta": {"source_revision": "test"}, "nodes": [], "edges": []})
        try:
            card = dict(empty_card(), name="测试", cost=1, type="Attack", rarity="Common", source_pool="main", base="造成6伤害")
            created = store.apply({"action":"create", "card":card}, 0)
            identifier = created["result"]["id"]
            store.apply({"action":"publish", "id":identifier}, 1)
            store.apply({"action":"update", "id":identifier, "card":dict(card, base="造成9伤害")}, 2)
            entry = store.read()["entries"][identifier]
            self.assertEqual("造成6伤害", entry["published"]["card"]["base"])
            self.assertEqual("造成9伤害", entry["working"]["base"])
        finally:
            store.close()
```

- [ ] `"$PY" -m unittest test_workbench_model test_workbench_store -v`应先因模块不存在失败；补齐unittest、tempfile、Path导入。
- [ ] 按共享schema实现验证与纯状态机。草稿空字段可保存；publish缺字段拒绝。create/copy生成新id；withdraw保留working，archive撤回published，restore不自动重新纳入。拒绝变更基线id。
- [ ] 实现mechanism_hash与review版本记录。name/image修改不失效；费用/正文/关键词/类型/来源/品质/mechanics变动失效。另一提案重新纳入后，compose_graph不显示旧已确认边但保留原review供复核。
- [ ] 实现进程文件锁及线程锁，在同目录临时文件写入、flush/fsync后os.replace。原文件备份成功才替换；异常不推进内存revision，启动损坏数据不覆盖。备份轮转仅匹配服务自己的备份命名，不删除uploads。

```python
# Inside the locked write transaction, after validation and backup succeed.
payload = (json.dumps(next_state, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
with tempfile.NamedTemporaryFile(dir=root, prefix=".workspace-", suffix=".tmp", delete=False) as handle:
    temporary = Path(handle.name)
    handle.write(payload)
    handle.flush()
    os.fsync(handle.fileno())
try:
    os.replace(temporary, root / "workspace.json")
finally:
    temporary.unlink(missing_ok=True)
```
- [ ] 测试过期revision、两个Store抢同一路径、os.replace故障注入、20份备份上限、未知schema、JSON损坏、引用未知图片/节点、重复名字、X费、升级去关键词、归档恢复；对baseline序列化前后做字节相等检查。
- [ ] 全部通过后提交本任务；.gitignore只加 `.lock`、临时保存文件、缓存，不忽略用户workspace.json/uploads。

## Task 4: 仅本机HTTP服务与图片上传

**Files:** Create `G/server.py`, `G/workbench_assets.py`, `G/test_workbench_server.py`, `G/requirements-workbench.txt`; modify `G/workbench_store.py`以增加受控图片登记。

**Interfaces:** `validate_image(data:bytes)->dict`返回format/mime/width/height/sha256；`WorkspaceStore.add_image(data:bytes,expected_revision:int)->dict`返回workspace/image_id。
`create_server(root:Path,workspace:Path,port:int=8765)->http.server.ThreadingHTTPServer`为测试可调用入口；CLI `server.py --workspace PATH --port NUMBER`，port=0由系统选端口，默认8765被占用时改用0。
服务启动输出单行JSON `{url,pid}` 供测试读取，实际日志写stderr。进程退出关闭服务与Store锁。
create_server返回实例暴露 `server.store` 供测试清理；关闭遵循shutdown、server_close、join、store.close顺序，close可重复调用。

- [ ] 先写HTTP契约测试，通过create_server(root,临时目录,0)在线程启动，urllib.request实际请求；未带token的POST必须403，测试finally shutdown/server_close/join/store.close。
- [ ] 跑 `"$PY" -m unittest test_workbench_server -v`确认红灯。
- [ ] 实现bootstrap/commands/uploads/images/portraits/export及白名单静态服务。Task5再接analyze，之前返回结构化503“不具备分析能力”，不返回假建议。
- [ ] 图片验证采用Pillow先识别允许format，再verify并重新打开load，校验像素上限；不按扩展名判断、不重新编码。以服务UUID和验证所得扩展名落盘，登记metadata后返回新revision；失败不覆盖既有图片，孤立新文件可保留但不被任何卡引用。
- [ ] Host严格匹配实际127.0.0.1:port；POST额外检查Origin和token，拒绝跨源、null Origin、chunked与超限body。静态资源显式白名单，基线portrait按node-id映射并resolve确认在允许assets根内，禁止任意 `../`、编码穿越、symlink逃逸及history/workspace直接静态下载。
- [ ] 扩展故障测试，用原项目一张有效PNG读bytes作上传fixture，不写游戏资产：

```python
def test_svg_is_rejected_without_state_change(self):
    from workbench_assets import validate_image
    from workbench_model import ValidationError
    with self.assertRaises(ValidationError):
        validate_image(b'<svg xmlns="http://www.w3.org/2000/svg"></svg>')
```

- [ ] 补真实HTTP测试：PNG保存重启可读，假MIME/坏图片/10MiB以上/超过像素上限拒绝，图片上传后旧revision保存409，损坏workspace返回503且文件不变，未知image/portrait404，无CORS、错误Origin/Host/token拒绝、导出无token、占用8765时未停止原进程。
- [ ] 记录实际Pillow版本到requirements-workbench，只有当前平台实测安装验证过的版本才固定，不猜版本号。运行全部Python测试再提交本任务。

## Task 5: 全卡池机制档案与可解释建议规则

**Files:** Create `G/mechanic-profiles.json`, `G/relation_rules.py`, `G/test_relation_rules.py`; modify `G/server.py`接入analyze及profiles初始化。

**Interfaces:** `load_profiles(path:Path,baseline:dict)->dict`严格核对72张已实现ID；每个profile含 `{facts,limitations,evidence}`。
`suggest_relations(draft_id:str,card:dict,baseline:dict,workspace:dict,profiles:dict)->dict`返回共享HTTP中定义的分析结果；不写Store。
文件顶层为 `{schema_version:1,source_revision,cards:{cardId:{facts,limitations,evidence}}}`；load_profiles额外计算返回 `revision`，哈希只覆盖文件原始规范JSON，避免自包含哈希。已有固定提案没有档案时列入limitations，不伪装成缺失已实现节点。

- [ ] 建立规则失败测试，使用真实graph作为只读fixture，draft通过empty_card构建：

```python
def test_non_song_does_not_discount_two_moons(self):
    from workbench_model import empty_card
    from relation_rules import load_profiles, suggest_relations
    graph = json.loads((Path(__file__).parent / "graph.json").read_text())
    profiles = load_profiles(Path(__file__).parent / "mechanic-profiles.json", graph)
    state = {"entries": {}}
    card = dict(empty_card(), name="测试攻击", cost=1, type="Attack", song=False)
    result = suggest_relations("draft:test", card, graph, state, profiles)
    self.assertFalse(any(e["target"] == "TwoMoonsDeepIntoTheForest" and e["rule_id"] == "song-count" for e in result["suggestions"]))
```

- [ ] `"$PY" -m unittest test_relation_rules -v`先确认失败，再对照catalog、families与发布工作树源码填写72个profile。可复用已审查证据，不用正则从卡文猜机制；特殊事件等用明确limitations覆盖。
- [ ] 定义有限资源枚举：pressure/block/hp_loss/heal/strength/dexterity/inferiority/despair_echo/damage_received/pressure_token/card_discarded/card_exhausted/token_played/song_played/attack_played/card_type_order/song_pool_access/exhaust_pile_access/discard_pile_access/draw_pile_access；时点枚举on_play/after_play/after_draw/enemy_hit/own_turn_start/own_turn_end/enemy_turn_start/continuous。遇到其他机制注明人工分析，不静默改成最近的枚举。
- [ ] 规则按以下明确条件产生候选而非已确认边：

```python
# Resource matching is only a proposed relation with retained scope/timing conditions.
def matching_resource(producer, consumer):
    return (producer["action"] == "produce"
            and consumer["action"] in {"consume", "read"}
            and producer["resource"] == consumer["resource"]
            and producer["scope"] == consumer["scope"])
```

- [ ] 实现规则表：资源供给→消耗/读取、消耗压力→读存量的tradeoff；弃牌/消耗/打出严格不同；力量→Attack；正伤害→自卑/回响风险；Song可打出→TwoMoons，Song+品质→检索候选；攻击重放→Attack，八芒星+攻击重放→冲突；生成/重放候选必须有正确来源/牌堆/关键词条件。all_enemies→enemy仅作为“同一受影响敌人”的有条件匹配，不当成任意目标。
- [ ] evidence只引用已知profile证据/草案id，保留base/upgraded变体和持有者时点。输出稳定rule_id、两端哈希与limitations。基础无消耗、升级有消耗/反之必须分别判断；共同tag或名字相同不能连边。
- [ ] 测试：完美无缺→弃牌收益；乔迁生成大狗的消耗条件；Face延迟施压不赶当前KillKiss时点；主动消耗不触发Backstage；ImprisonedXii正常不可打出；无Song不减TwoMoons；非凡/普通检索不匹配稀有；草稿自由文本“压力”不产生声明为必然的边；自环需手动审核；未纳入其他草稿排除。
- [ ] 将analyze接到上述纯函数，测试分析前后workspace字节不变、未知draft id拒绝、非空自由备注仍能返回limitations。全部Python测试通过后提交。

## Task 6: 草案编辑器、上传与顺序保存

**Files:** Create `G/workbench-api.js`, `G/draft-editor.js`, `G/draft-editor.css`, `G/test_workbench_ui.cjs`; modify `G/index.html`, `G/graph.js`, `G/graph.css`。

**Interfaces:** `WorkbenchApi.connect()->Promise<{graph,workspace,capabilities}>`, `command(command)->Promise<{workspace,result}>`, `upload(file)->Promise<{workspace,image_id}>`, `analyze(id,card)->Promise<analysis>`, `subscribe(listener)->unsubscribe`。只在一个模块持有token/revision和串行写队列。
`DraftEditor.mount(root,{api,onPreview,onOpenCard})->{open(entry),flush(),dispose()}`；内部表单working独立于服务端快照，成功响应只确认对应编辑序号，不覆盖请求发出之后的新输入。

- [ ] 先在新Playwright套件建立失败用例：启动任务4服务指向临时workspace，打开页面、点新建、输入名字及基础效果、保存、重载仍存在。

```js
await page.getByRole('button',{name:'新建草案',exact:true}).click();
await page.getByLabel('卡牌名称',{exact:true}).fill('浏览器测试牌');
await page.getByLabel('基础效果',{exact:true}).fill('造成6点伤害');
await page.getByRole('button',{name:'保存草稿',exact:true}).click();
await page.getByRole('status').filter({hasText:'已保存'}).waitFor();
await page.reload();
await page.getByRole('button',{name:'浏览器测试牌',exact:true}).click();
assert.equal(await page.getByLabel('基础效果',{exact:true}).inputValue(),'造成6点伤害');
```

- [ ] 运行 `"$NODE" test_workbench_ui.cjs`确认红灯。套件spawn Python server、读取启动URL，记录pageerror；必须finally退出浏览器、服务和删除其临时目录。
- [ ] 建立图谱/草案tabs及空白/复制入口，按schema添加表单；升级费用、效果模式与关键词独立，添加自定义词条不会声称游戏支持。复制只读卡保留origin并显示升级描述为delta。
- [ ] 图片input支持预览/移除关联；使用实际source原图，不额外安装生成工具。HTTP基线图像URL改走portraits映射，file模式沿用原相对路径，防止HTTP第一次渲染尝试读整个仓库。
- [ ] API connect失败时保持只读图谱，并明确编辑不可用；自动保存防抖700ms，flush用于显式保存/切换/纳入。写队列遇409/503停止后续依赖写入并保留dirty模型，显示下载本地JSON与重新加载命令，不自动覆盖。
- [ ] 服务重启导致403或断线时允许重新连接并获取新token，但先保留本页表单；新的revision与原读版本不同仍进入冲突，不因重连而覆盖本地编辑。
- [ ] 给表单文本接入任务2的@选择器；生成显式ID引用，预览只渲染安全DOM。实时卡面有固定图片比例，基础/升级可切换，长文流式换行。
- [ ] 测试空白草稿、复制、同名、X费与费用不变、关键词升级增删、图片上传并重载、上传失败旧图保留、快速输入晚响应不回滚新字符、两个标签页409、断网保存失败恢复、未保存离开提示、file模式禁写及桌面/手机文字溢出。
- [ ] 跑模块、Python、两套浏览器回归，通过后提交本任务。

## Task 7: 试放、关系审核与全局快照

**Files:** Create `G/workbench.js`; modify `G/draft-editor.js`, `G/workbench-api.js`, `G/graph.js`, `G/graph.css`, `G/index.html`, `G/test_workbench_ui.cjs`, `G/test_workbench_model.py`, `G/test_relation_rules.py`。

**Interfaces:** `Workbench.mount({api,graphView,editor})`负责选择、试放及发布操作，不再复制Store状态机。试放先保存有效草稿版本，再analyze；纯视图graph来自compose_graph等价DTO，服务command成功响应可附 `result.graph` 避免前端重复实现哈希校验。
新增GET `/api/graph`获取published合成图；POST `/api/preview`带 `{draft_id,card}`返回 `{graph,analysis}`，执行与analyze相同校验、不落盘。纳入/撤回后统一获取/api/graph；这两个入口沿用任务4安全限制。

- [ ] 加失败回归：保存攻击草案→试放出现单节点→接受一条建议→纳入后全图存在→修改草稿但不重纳入，全图仍旧效果→撤回后全图不存在但草稿仍在。
- [ ] 运行相关测试确认缺入口/交互的失败，而非放宽断言绕过。
- [ ] 关系面板分建议/已确认/待复核/已拒绝，展示理由、条件、profile证据、状态；支持逐条接受、编辑再接受、拒绝、手动连边。手动边方向/类型/条件/解释必填，自环明确勾选审核；不能伪造runtime证据链接。
- [ ] 切换预览只保留一个草稿ID节点，建议线低透明度但保留语义颜色形状；全局只展示accepted且版本匹配的提案边。新增来源筛选与提案计数，不把draft算进72张已实现统计。
- [ ] 纳入前flush保存、展示与上一快照的字段/关系差异，要求处理stale；没有关系的设计允许纳入，显示“无已确认关系”，不阻止保存罕见创意。
- [ ] 发布/撤回/归档均走命令队列；归档显示明确影响并需确认，恢复只恢复草稿。归档引用可预览旧内容但不能在正常全图伪装为活动节点。
- [ ] 增加具体状态测试：

```python
def test_name_changes_do_not_invalidate_mechanisms(self):
    from workbench_model import empty_card, mechanism_hash
    card = dict(empty_card(), name="旧名", base="造成6点伤害", cost=1)
    self.assertEqual(mechanism_hash(card), mechanism_hash(dict(card,name="新名")))
    self.assertNotEqual(mechanism_hash(card), mechanism_hash(dict(card,cost=0)))
```

- [ ] 验证跨提案更新使关联旧review stale且人工文字保留；相关mechanics变化后的已拒绝建议重新出现为新版本候选；无关改图不会重置拒绝。两次返回栈恢复原边与视口，包括试放→查看旧卡→返回。
- [ ] 新旧测试通过后提交本任务。

## Task 8: 完整验收、交付入口与文档

**Files:** Modify `G/README.md`, `G/test_workbench_ui.cjs`, `docs/current-status.md`, `docs/next-task.md`, `docs/thread-handoff.md`及角色docs/current-status.md；只在明确交付后更新完成状态。

**Interfaces:** 不新增产品接口。补最终只读模式与HTTP模式的运行说明、保存位置、备份恢复、图片限制、规则建议的证据边界。

- [ ] 运行所有实际命令，禁止仅把计划里的预期结果写成测试结果：

```sh
"$PY" build_graph.py --check
"$PY" -m unittest discover -s . -p 'test_*.py' -v
"$NODE" --test test_view_modules.cjs
"$NODE" test_ui.cjs
"$NODE" test_workbench_ui.cjs
```

- [ ] 两种模式分别在1440×1000、768×1024、390×844验证，HTTP再使用触摸context。断言无pageerror、无横向溢出、上传图naturalWidth>0、画布非空、popover位于视口内、长名称/中文/英文长词/8000字效果不遮住操作。
- [ ] 加完整保护断言，测试前后记录baseline graph、catalog及发布游戏src/assets/pack的哈希；这些文件不得因服务/编辑器测试而改变。测试产生的图片及workspace只能在临时目录。
- [ ] 对截图做人工检查与非空像素检查，覆盖七类关系、所选边、引用浮层、基础/升级卡面、suggestions与published。确认密集全局不重新堆叠全部标签。
- [ ] 更新README的启动命令、依赖实际路径、workspace位置、JSON导出不含图像像素、20份备份恢复方式、file只读限制、没有游戏补丁功能；补交接状态，不覆盖旧审计记录。
- [ ] 做一次独立全改动审查，重点检查Review Focus五项；发现真实问题后补失败测试并修复，再重跑受影响及完整集。若审查工具不可用，明确记为自审，不冒充独立审查。
- [ ] 使用真实workspace启动仅本机服务，端口占用则改新端口，给出实际URL并在Codex打开；不把测试服务留作用户生产入口。
- [ ] 最后只提交本研究工具及本轮明确文档改动，不推送或发布。最终回复给URL、保存位置、测试结果及真实剩余限制。

## 自检与执行选择

规格第1/2/7/8/9节由任务3/4/8落实，第3节由任务3/6/7落实，第4节由任务1落实，第5节由任务2/6/7落实，第6节由任务5/7落实，第10节由全部任务和任务8总验收落实。
五项Review Focus已分别加入所属任务，安全性与数据保护随相关功能一并验收。
任务间共享数据与交互接口较多，推荐 **Native：当前会话连续实施，最后一次独立审查**，避免反复交接状态。
也可选择Subagent-driven：每个任务独立实现者与审查者，检查更密但开销更高。用户审核本计划并选择方式后才开始实施。
