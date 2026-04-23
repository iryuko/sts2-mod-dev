# 普通问号房 Shadow 事件资产规格

日期：2026-04-20

## 一 本文件定位

本文件只回答一件事：

- 如果后续要把【往日之影 / Shadow of the Past】做成普通问号房 `Event Room` 事件，T3 资源线程需要准备哪些资产，哪些是当前已确认的硬需求，哪些只是高概率需求，哪些目前不需要。

说明：

- 本文件明确区分：
  - 已确认
  - 高概率但待验证
  - 当前不要求
- 本文件不讨论事件机制分支设计，只讨论资产层。
- 当前目标是给 T3 一份可执行的来稿搜集与目录建立清单。

## 二 当前已确认的普通事件资产事实

基于当前原版参考文件，已经确认这些事实：

### 1. 普通事件房间不是 `Ancient` 路线

- 原版普通事件房间 scene 为：
  - `scenes/rooms/event_room.tscn`
- 原版普通事件使用的公用事件布局资源包括：
  - `scenes/events/default_event_layout.tscn`
  - `scenes/events/combat_event_layout.tscn`
  - `scenes/events/event_option_button.tscn`

结论：

- `Shadow` 事件后续不应沿用 `AncientEventModel` 的资源组织方式。
- 资源线程不应把它和 `ancients/` 目录混在一起。

### 2. 原版普通事件确实存在独立的事件主图目录

已确认原版存在：

- `images/events/`

并且其中有大量普通事件主图，如：

- `images/events/abyssal_baths.png`
- `images/events/bugslayer.png`
- `images/events/colossal_flower.png`
- `images/events/relic_trader.png`

结论：

- 自定义普通问号房事件至少应预留一张“事件主图”。
- 路径模式高概率应对齐：
  - `images/events/<event_id>.png`

### 3. 原版普通事件还存在背景 scene 目录

已确认原版存在：

- `scenes/events/background_scenes/`

其中包括：

- `neow.tscn`
- `darv.tscn`
- `orobas.tscn`
- `tezcatara.tscn`

结论：

- 普通事件可以拥有独立背景 scene。
- 但对我们当前最小 `Shadow` 事件而言，这条目前只应记为“高概率可选项”，不是第一个阻塞项。

## 三 当前对 Shadow 事件的资产需求分层

### 1. 第一优先级：最小可运行必需资产

这些资产是做一个“能进入问号房、能显示文本、能选一张 Shadow 入组”的最小事件时，最值得先准备的：

| 对象 | 建议内部名 | 建议正式路径 | 当前状态 | 备注 |
| --- | --- | --- | --- | --- |
| 普通事件主图 | `shadow_of_the_past_event` | `assets/events/question_room/shadow_of_the_past/shadow_of_the_past_event.png` | 待准备 | 高优先级；建议最终同步到 `pack/images/events/shadow_of_the_past_event.png` |
| `Shadow I` 卡图 | `shadow_of_the_past_i.png` | `assets/cards/event_granted/shadow_of_the_past_i.png` | 当前仍为运行时占位 | 当前代码已经按 `event_granted` 卡图路径接线 |
| `Shadow II` 卡图 | `shadow_of_the_past_ii.png` | `assets/cards/event_granted/shadow_of_the_past_ii.png` | 当前仍为运行时占位 | 同上 |
| `Shadow III` 卡图 | `shadow_of_the_past_iii.png` | `assets/cards/event_granted/shadow_of_the_past_iii.png` | 当前仍为运行时占位 | 同上 |
| `UpgradedDollMask` relic 图 | `upgraded_doll_mask.png` | `assets/relics/starter/upgraded_doll_mask.png` | 当前仍复用 `doll_mask` | `Shadow III` 奖励触发后需要视觉上可区分升级态 |

### 2. 第二优先级：建议准备，但不是第一轮阻塞

| 对象 | 建议内部名 | 建议正式路径 | 当前状态 | 备注 |
| --- | --- | --- | --- | --- |
| 普通事件背景 scene 用底图 | `shadow_of_the_past_bg.png` | `assets/events/question_room/shadow_of_the_past/background/shadow_of_the_past_bg.png` | 待准备 | 若后续要做独立 `background_scenes/<event>.tscn`，这张会是主要底图 |
| 普通事件前景 / 叠层素材 | `shadow_of_the_past_fg_*.png` | `assets/events/question_room/shadow_of_the_past/background/` | 待准备 | 只有确定要做多层背景时才需要 |
| 事件专属 VFX 参考图 | `shadow_of_the_past_vfx_ref_*.png` | `incoming_assets/events/question_room/shadow_of_the_past/vfx_refs/` | 待准备 | 当前不是实现阻塞项，但若后续要做专属 scene / shader，会有帮助 |

### 3. 当前明确不需要先做的资产

这些在第一轮普通事件接入时，不应作为阻塞项：

- `Ancient` 对话头像
- 地图节点头像
- `Ancient` 地图节点 outline
- `Ancient` 对话头像 outline
- `Ancient` 主图
- 事件专属配音
- 事件专属 UI 按钮贴图

原因：

- 它们属于 `Ancient` 路线或高定制路线，不属于普通问号房最小闭环。

## 四 当前建议的正式目录与来稿区

### 1. 正式库存建议

建议新增并固定以下正式目录：

- `assets/events/question_room/shadow_of_the_past/`
- `assets/events/question_room/shadow_of_the_past/background/`
- `assets/cards/event_granted/`
- `assets/relics/starter/`

说明：

- `assets/cards/event_granted/` 是当前 `Shadow I / II / III` 最合理的正式卡图库存位。
- `Shadow` 当前是 event-only curse-like cards，不应混入 `assets/cards/normal/`。

### 2. 来稿区建议

建议 T3 建立以下来稿目录：

- `incoming_assets/events/question_room/shadow_of_the_past/event_main/`
- `incoming_assets/events/question_room/shadow_of_the_past/background/`
- `incoming_assets/events/question_room/shadow_of_the_past/vfx_refs/`
- `incoming_assets/cards/event_granted/`
- `incoming_assets/relics/starter/upgraded/`

用途对应：

| 来稿目录 | 用途 |
| --- | --- |
| `incoming_assets/events/question_room/shadow_of_the_past/event_main/` | 普通事件主图来稿 |
| `incoming_assets/events/question_room/shadow_of_the_past/background/` | 普通事件背景底图 / 前景叠层来稿 |
| `incoming_assets/events/question_room/shadow_of_the_past/vfx_refs/` | 若后续要做独立 scene / shader / 动效，可先放视觉参考 |
| `incoming_assets/cards/event_granted/` | `Shadow I / II / III` 三张长期牌卡图来稿 |
| `incoming_assets/relics/starter/upgraded/` | `UpgradedDollMask` 升级态遗物图来稿 |

## 五 当前建议的命名

### 1. 事件主图

- 正式文件名建议：
  - `shadow_of_the_past_event.png`

说明：

- 不建议直接叫 `shadow_of_the_past.png`，因为后续若同时出现背景 scene 贴图、事件主图、预览图，会更容易撞名。

### 2. Shadow 卡图

- `shadow_of_the_past_i.png`
- `shadow_of_the_past_ii.png`
- `shadow_of_the_past_iii.png`

### 3. 升级 starter relic

- `upgraded_doll_mask.png`

说明：

- 不建议把升级态继续复用 `doll_mask.png` 的文件名。
- 运行时可以继续暂时复用旧图，但正式库存应有独立稳定主键。

## 六 推荐规格

### 1. 普通事件主图

- 推荐格式：透明或不透明 `PNG`
- 推荐画布：先按 `2560x1200` 准备

说明：

- 这是沿当前原版事件主图与本项目已有事件主图习惯给出的保守建议。
- 具体构图仍需以普通 `Event Room` 实机挂接后再最终收口。

### 2. `Shadow` 三张卡图

- 推荐格式：`PNG`
- 推荐尺寸：`1000x760`

### 3. `UpgradedDollMask`

- 推荐格式：透明底 `PNG`
- 推荐尺寸：`256x256`

## 七 当前与 T3 交接时应明确说明的事项

交给资源线程时，建议明确写这几点：

1. `Shadow` 系列不是 normal pool 卡图，不要放进 `cards/normal_pool/`
2. `Shadow` 系列当前正式库存位应视为：
   - `assets/cards/event_granted/`
3. 这条事件是普通问号房路线，不是 `Ancient` 路线
4. 第一轮最重要的不是做复杂背景 scene，而是先补：
   - 事件主图
   - 三张 `Shadow` 卡图
   - `UpgradedDollMask` 图
5. 若时间不够，背景 scene 相关素材可以晚于卡图和 relic 图

## 八 当前结论

- `Shadow` 事件的资产线程应从普通问号房 `Event Room` 出发，而不是从 `Ancient` 资产目录出发。
- 当前最小实现所需的资产优先级很明确：
  1. 普通事件主图
  2. `Shadow I / II / III` 三张 event-granted 卡图
  3. `UpgradedDollMask` 图
- 背景 scene 贴图和更复杂的事件视觉层属于第二阶段，不应阻塞第一轮功能接入。
