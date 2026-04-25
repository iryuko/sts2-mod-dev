# 丰川定治先古之民事件设计记录

日期：2026-04-03

## 一 当前冻结结论

当前项目新增的这位先古之民，设计前提固定为：

- 名称：`丰川定治`
- 定位：`丰川祥子专属先古之民`
- 出现层级：`仅进入 Act 3 ancient 池`
- 触发限制：
  - 只有当前 run 选择角色为 `Togawasakiko` 时，才允许进入该先古之民遭遇池
  - 装载 mod 但未选择 `Togawasakiko` 时，不会偶遇该先古之民

这条限制的目标很明确：

- 不为 `Ironclad / Silent / Defect / Necrobinder / Regent` 额外补角色回应文本
- 只维护 `丰川祥子` 这一套对白
- 降低“角色不匹配却进入事件”“缺回应文本”“事件分支缺字串”这类 bug 风险

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
  - 已以 `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png` 形式入正式库存
  - 当前与 `incoming_assets/ancients/map_node_outline/mapnode_outline.png` 一致
  - 当前仍未接入 runtime
- 对话头像：
  - 已以 `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype.png` 形式入正式库存
  - 当前与 `incoming_assets/ancients/dialogue_icon/丰川定治.png` 一致
- 对话头像 outline：
  - 已以 `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype_outline.png` 形式入正式库存
  - 当前正式库存来自 auto-generated outline
  - `incoming_assets/ancients/dialogue_icon_outline/丰川定治.png` 未与正式库存 outline 对齐，后续整理时不能直接视为最终版
- 事件主图：
  - `incoming_assets/ancients/event_main/丰川定治.png` 已存在
  - 当前尺寸为 `2560x1244`
  - 仍仅是来稿，尚未整理进 `assets/` 或 `pack/`

当前结论：

- `BestCompanion` 与 `BarkingBarkingBarking` 已不是“仅源码 stub”，而是已完成资源安装的对象
- `BlackLimousine` 与 `PullmanCrash` 已完成源码接线，并已于 2026-04-04 用正式来稿替换到 `assets/` / `pack/`
- 丰川定治作为先古之民的地图节点图与轮廓图，当前已从 `prototype` 库存复制出 runtime 占位对象
- `TogawaTeiji : AncientEventModel` 已接入源码，当前三选项为：
  - `BestCompanion`
  - `BlackLimousine`
  - `继续演出吧`：获得 `1000 gold`
- 当前源码侧已限制为：
  - 仅注入 `Act 3` 的 ancient 候选池
  - `Hook.ShouldAllowAncient(...)` 仍要求当前角色为 `Togawasakiko`
- 因此当前冻结口径应写成：
  - `丰川定治只会在 Act 3 出现`
  - `且只有丰川祥子能够遇到`
- 若后续冻结其稳定英文内部名，应优先做的是：
  - `prototype` 资源转正命名
  - event_main 主图规范化落库
  - 再决定事件 scene / 节点接线
