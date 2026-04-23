# 普通问号房事件规格：无人问津的钢琴

## 事件定位

- 类型：普通问号房事件
- 仅限角色：`Togawasakiko`
- 出现频率：正常权重，无 `Act` 偏好
- 限制：每个 run 只出现一次
- 奖励主线：发放 `Shadow of the Past I / II / III`

## 事件标题

- 中文：`无人问津的钢琴`
- 英文：`Unattended Piano`

## 页面流程

### 初始页

- 描述：
  - `你走进了一间音乐教室...`
  - `靠窗处摆放着一架失去了灵魂的钢琴...`
- 主图：`start.png`
- 音乐：不播放

选项：

1. `不以为意`
   - 恢复 `12` 点生命值
   - 结束事件
2. `无形之手`
   - 切换主图到 `shadow_piano.png`
   - 开始播放事件音乐
   - 进入第一次弹琴询问

### 第一次弹琴询问

- 描述：
  - `你感受到电流从指尖传来`
  - `是否继续弹奏`
- 主图：`shadow_piano.png`
- 音乐：持续播放

选项：

1. `继续`
   - 失去 `6` 点生命值
   - 不放回随机获得 `1` 张 `Shadow`
   - 进入第二次弹琴询问
2. `停止弹琴`
   - 停止音乐
   - 离开事件

### 第二次弹琴询问

- 描述：
  - `若有若无的悲凉在你心中涌起`
  - `手指流出鲜血`
- 主图：`shadow_piano.png`
- 音乐：持续播放

选项：

1. `继续`
   - 失去 `6` 点生命值
   - 从剩余 `Shadow` 中不放回随机获得 `1` 张
   - 进入第三次弹琴询问
2. `停止弹琴`
   - 停止音乐
   - 离开事件

### 第三次弹琴询问

- 描述：
  - `往日种种...一切的美好都在你脑中来回播放`
  - `你放声大哭`
  - `手指血流如柱了`
- 主图：`shadow_piano.png`
- 音乐：持续播放

选项：

1. `继续`
   - 失去 `6` 点生命值
   - 获得最后 `1` 张 `Shadow`
   - 进入最终离场页
2. `停止弹琴`
   - 停止音乐
   - 离开事件

### 最终离场页

- 描述：
  - `你缓慢起身，蹒跚离开教室`
- 主图：`shadow_piano.png`
- 音乐：持续播放直到点击离开

选项：

1. `起身离开`
   - 停止音乐
   - 结束事件

## Shadow 发放规则

- 固定池：
  - `Shadow of the Past I`
  - `Shadow of the Past II`
  - `Shadow of the Past III`
- 发放方式：不放回随机
- 单次事件内不会重复
- 直接加入牌组，不加入手牌

## 资源接口

### 主图

- 当前初始主图实际接线：
  - `res://images/events/unattended_piano.png`
- 当前事件内切图实际接线：
  - `res://images/events/unattended_piano_shadow.png`

说明：

- 现版本 `EventModel.InitialPortraitPath` 不是可 override 成员。
- 因此普通事件初始主图必须对齐原版默认路径：
  - `images/events/<event_id>.png`
- 本事件当前 `event_id` 为：
  - `unattended_piano`
- 继续弹奏后的 portrait 切换，则由运行时显式调用事件房 portrait 接口完成。

### 音乐

- 当前播放路径口径：
  - 优先：`res://audio/music/tracks/unattended_piano.mp3`
  - 回退：`res://audio/music/events/unattended_piano.mp3`
- 当前实现口径：
  - 选择弹奏后开始播放
  - 停止弹奏 / 离开房间后停止播放
  - 若资源未到位，则静默跳过，不使事件报错

### 当前 bugfix 约束

- 由于运行时 `Entry` 派生规则会把 `ShadowOfThePastII / III` 拆成：
  - `SHADOW_OF_THE_PAST_I_I`
  - `SHADOW_OF_THE_PAST_II_I`
- 当前先通过补兼容本地化键止住牌组界面崩溃，不直接改类名：
  - 以避免打断已进入 run / 存档的事件牌实例
- 若后续需要彻底把事件牌 `Entry` 收口为：
  - `SHADOW_OF_THE_PAST_II`
  - `SHADOW_OF_THE_PAST_III`
  则必须把存档兼容策略一起设计清楚后再动

## 当前实现备注

- 事件本体按普通 `EventModel` 路线实现，不走 `AncientEventModel`
- 中途切图通过事件房间 portrait 接口完成
- “一局只出现一次”通过 run 的 visited-event 记录控制
- 当前事件内发放 `Shadow` 的展示与入组，必须走原版诅咒事件口径：
  - `CardPileCmd.AddCursesToDeck(IEnumerable<CardModel> curses, Player owner)`
- 不应在事件选项回调里直接生塞：
  - `CardCmd.PreviewCardPileAdd(...)`
  - 或 reward screen 的 `SpecialCardReward` 流程
  否则高概率导致事件 UI 锁死或无法继续交互
- 这条结论当前 **仅适用于事件内发放 `Curse / curse-like card`**
- 若后续要做“事件里展示并获得普通攻击 / 技能 / 能力牌”，应另找原版普通牌事件模板，不要复用这条诅咒接口模型
