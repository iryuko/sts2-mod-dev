# 丰川定治先古之民事件设计记录

初建日期：2026-04-03；实现状态校订：2026-07-26

> 档案状态：本文件保存丰川定治 Ancient 的设计来源和对白口径。事件、地图节点、头像、背景和奖励已经接入；当前技术债是 `DarvPatches.cs` 仍复制原版选项流程，与本事件是否存在无关。实现事实见 [当前状态](current-status.md)。

## 一 初始设计结论（历史）

2026-04-03 形成的初始设计前提为：

- 名称：`丰川定治`
- 定位：`丰川祥子专属先古之民`
- 出现层级：`仅进入 Act 3 ancient 池`
- 触发限制：
  - 只有当前 run 选择角色为 `Togawasakiko` 时，才允许进入该先古之民遭遇池
  - 装载 mod 但未选择 `Togawasakiko` 时，不会偶遇该先古之民

这条限制当时的目标很明确：

- 不为 `Ironclad / Silent / Defect / Necrobinder / Regent` 额外补角色回应文本
- 只维护 `丰川祥子` 这一套对白
- 降低“角色不匹配却进入事件”“缺回应文本”“事件分支缺字串”这类 bug 风险

当前多人实现已取代上述单角色限制：

- run 中至少存在一名 `Togawasakiko` 时，Teiji 才会进入 Act 3 Ancient 候选。
- 混合多人队伍允许进入。
- 祥子使用角色专属 dialogue；其他队友使用 agnostic dialogue。

## 二 副标题

中文副标题当前定为：

- `家主`

英文暂定：

- `Lord`

说明：

- `Lord` 能表达“上位家主、掌权者”的基本意思，因此可用。
- 但如果目标是更贴近原版 `epithet` 的气质，`Lord` 会显得略泛。
- 原版副标题更常见的是“身份称号化”的短语，例如：
  - `The Hoarder`
  - `Living Rainbow`
  - `The First Demon`
  - `Mother of Resurrection (Exiled)`

因此，当前建议分两层看：

- 如果只是先推进事件接入与本地化闭环：
  - `Lord` 可以先用
- 如果后面要再润一次原版风格：
  - 更值得比较的候选会是：
    - `Patriarch`
    - `House Lord`
    - `Head of House Togawa`

当前不强行冻结英文副标题，先保留：

- 中文：`家主`
- 英文工作版：`Lord`

## 三 当前对白逻辑

当前冻结对白顺序如下：

1. 丰川祥子：`爷爷你可真是无处不在`
2. 丰川定治：`光靠口舌之快可不能背负其他人的人生`
3. 丰川祥子：`……`
4. 丰川定治：`唉……拿上这些，继续走吧`
5. 随后展示选项

## 四 实现层含义

按这条设计，后续本地化与事件结构应采用：

- 一套先古之民名字文本
  - `title`
  - `epithet`
- 一套 `丰川祥子` 专属对话文本
- 一套该事件自己的选项文本
- 不额外为其他角色写回应对白

额外说明：

- 原版 `ancient_dialogue_line` 中玩家角色侧头像通常复用角色现有 `top_panel` 头像
- 所以该事件不需要为 `丰川祥子` 再单独补一套角色专用对话头像资源

## 五 当前未冻结项

以下内容当前仍应视为待后续确认：

- 丰川定治的稳定英文内部名
- `家主` 的最终英文定稿是否保持 `Lord`
- 事件具体给什么奖励
- 选项分支数量与文案

## 六 已落源码与资源状态

当前已经先把其中一个 relic 与其附带卡牌接入源码，并完成对应资源安装：

- relic：
  - 内部类：`BestCompanion`
  - 当前本地化：
    - 中文：`最好的伙伴(?`
    - 英文工作版：`Best Companion(?)`
  - 当前效果：
    - 获得时，将 `Barking Barking Barking` 加入牌组
- 附带卡牌：
  - 内部类：`BarkingBarkingBarking`
  - 中文：`大狗大狗叫叫叫`
  - 类型：攻击
  - 品质：稀有
  - 费用：`1`
  - 效果：
    - 造成 `8` 点伤害
    - 获得原版 `Regen / 回复` `3` 层
  - 升级后：
    - 伤害 `11`
    - `Regen / 回复` `4` 层

当前约束：

- 该卡已注册为“relic granted card”，不进入祥子普通奖励池
- 该 relic 已注册为“ancient relic stub”，不进入当前角色常规 relic 池
- `assets/relics/ancient/best_companion.png` 与 `pack/images/relics/best_companion.png` 已存在
- `assets/cards/relic_granted/barking_barking_barking.png` 与 `pack/mod_assets/cards/relic_granted/barking_barking_barking.png` 已存在
- `best_companion` 已补齐 runtime atlas 包装：
  - `pack/images/atlases/relic_atlas.sprites/best_companion.tres`
  - `pack/images/atlases/relic_outline_atlas.sprites/best_companion.tres`

当前同一条先古之民链路上的其余资源状态：

- 地图节点主图：
  - 已以 `assets/ancients/map_nodes/ancient_map_node_prototype.png` 形式入正式库存
  - 当前与 `incoming_assets/ancients/map_node/mapnode.png` 一致
  - 已从旧 `208x208` 稿扩到当前库存的 `416x416`
- 地图节点 outline：
  - 生产原型仍保存在 `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png`
  - 正式 Teiji 节点与 outline 已进入 `assets/` 和 `pack/`
- 对话头像：
  - 已以 `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype.png` 形式入正式库存
  - 当前与 `incoming_assets/ancients/dialogue_icon/丰川定治.png` 一致
- 对话头像 outline：
  - 已以 `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype_outline.png` 形式入正式库存
  - 当前正式库存来自 auto-generated outline
  - `incoming_assets/ancients/dialogue_icon_outline/丰川定治.png` 未与正式库存 outline 对齐，后续整理时不能直接视为最终版
- 事件主图：
  - 原始来稿仍保留在 `incoming_assets/`
  - 正式图已整理为 `assets/ancients/event_main/togawa_teiji.png`
  - runtime 为 `pack/images/events/togawa_teiji.png`

当前结论：

- `BestCompanion` 与 `BarkingBarkingBarking` 已不是“仅源码 stub”，而是已完成资源安装的对象
- `BlackLimousine` 与 `PullmanCrash` 已完成源码接线，并已于 2026-04-04 用正式来稿替换到 `assets/` / `pack/`
- 丰川定治的地图节点、轮廓图、对话头像、事件主图和背景 scene 已进入 runtime
- `TogawaTeiji : AncientEventModel` 已接入源码，当前三选项为：
  - `BestCompanion`
  - `BlackLimousine`
  - `继续演出吧`：获得 `1000 gold`
- 当前源码侧已限制为：
  - 仅注入 `Act 3` 的 Ancient 候选池
  - `Hook.ShouldAllowAncient(...)` 要求 run 中至少存在一名 `Togawasakiko`
- 因此当前口径应写成：
  - `丰川定治只会在 Act 3 出现`
  - `run 中必须有丰川祥子；混合多人队伍可以进入`
- `prototype` 资源继续作为生产参考，不再代表 Teiji 尚未转正。
