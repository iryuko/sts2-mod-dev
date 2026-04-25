# 原版角色固有资产盘点

日期：2026-03-24

## 一 盘点范围

本文件只回答一件事：

- 原版角色在 STS2 当前资源结构里，通常分别准备了哪些角色固有资产
- 哪些是系统公用资源，不需要每个角色重复做

本轮证据主要来自：

- `references/pck-extract/sts2-main/images/packed/character_select/`
- `references/pck-extract/sts2-main/images/ui/top_panel/`
- `references/pck-extract/sts2-main/scenes/ui/character_icons/`
- `references/pck-extract/sts2-main/scenes/screens/char_select/`
- `references/pck-extract/sts2-main/scenes/combat/energy_counters/`
- `references/pck-extract/sts2-main/images/ui/combat/energy_counters/`
- `references/pck-extract/sts2-main/images/atlases/ui_atlas.sprites/card/`
- `references/pck-extract/sts2-main/images/ui/hands/`
- `references/pck-extract/sts2-main/images/packed/card_portraits/`
- `references/pck-extract/sts2-main/images/powers/`
- `references/pck-extract/sts2-main/images/relics/`

## 二 已确认：原版每个角色都各自拥有的资源

| 资源类别 | 原版命名模式 | 已确认例子 | 结论 |
| --- | --- | --- | --- |
| 角色选择主立绘 | `char_select_<entry-lower>.png` | `char_select_ironclad.png` | 已确认 |
| 角色选择锁定图 | `char_select_<entry-lower>_locked.png` | `char_select_silent_locked.png` | 已确认 |
| 顶栏头像 | `character_icon_<entry-lower>.png` | `character_icon_regent.png` | 已确认 |
| 顶栏头像轮廓 | `character_icon_<entry-lower>_outline.png` | `character_icon_defect_outline.png` | 已确认 |
| 顶栏头像 scene | `<entry-lower>_icon.tscn` | `ironclad_icon.tscn` | 已确认 |
| 角色选人背景 scene | `char_select_bg_<entry-lower>.tscn` | `char_select_bg_necrobinder.tscn` | 已确认 |
| 卡牌费用图标 | `energy_<entry-lower>.tres` | `energy_ironclad.tres` | 已确认 |
| 战斗能量计数器 scene | `<entry-lower>_energy_counter.tscn` | `silent_energy_counter.tscn` | 已确认 |
| 战斗能量计数器图层 | `<entry-lower>_orb_layer_*.png` | `ironclad_orb_layer_1.png` | 已确认 |
| 联机手势图 | `multiplayer_hand_<entry-lower>_<pose>.png` | `multiplayer_hand_defect_scissors.png` | 已确认 |

## 三 已确认：系统公用资源

以下资源在原版中是系统共用底板，不是每个角色单独做一套：

- 角色选择按钮轮廓与遮罩：
  - `char_select_outline.png`
  - `char_select_outline_remote.png`
  - `char_select_button_mask.png`
  - `char_select_lock3.png`
- 卡牌框体、标题横幅、portrait border、type plaque、锁牌遮罩、星标：
  - 位于 `images/atlases/ui_atlas.sprites/card/` 与 `images/packed/common_ui/locked_card.png`
- power atlas、relic atlas、card atlas：
  - 属于系统公共打包资源
- 能量爆闪与星星图：
  - `images/ui/combat/energy_counters/energy_burst/`
  - `images/ui/combat/energy_star.png`

这意味着：

- 自定义角色不需要从零设计整套卡牌框体
- 不需要自己做通用 rarity banner、type plaque、锁牌遮罩
- 不需要自己做角色选择按钮轮廓与锁链底板

## 四 卡牌、relic、状态、角色资源在原版中的形式

### 1. 卡牌

- 正式卡图：
  - `images/packed/card_portraits/<pool>/<entry-lower>.png`
- beta 卡图：
  - `images/packed/card_portraits/<pool>/beta/<entry-lower>.png`

结论：

- 卡牌图像是每张牌一个独立 portrait 文件
- 目录由卡池决定，不是由显示名决定

### 2. Power / Debuff

- 正式 icon：
  - `images/powers/<entry-lower>.png`
- beta icon 候选目录：
  - `images/powers/beta/`
- 已看到系统缺图占位：
  - `images/powers/missing_power.png`

结论：

- 状态 / debuff icon 是每个对象独立准备的
- 系统里虽然存在 `missing_power`，但不应把它当成正式方案依赖

### 3. Relic

- 正式图：
  - `images/relics/<entry-lower>.png`
- beta 图：
  - `images/relics/beta/<entry-lower>.png`

结论：

- relic 图也是独立文件
- 原版允许 beta 图并存，但正式发布并不依赖 beta 图

### 4. 角色 portrait / 面板头像

本轮看到的原版“角色 portrait / 面板头像”更接近以下组合，而不是另一张独立大头像：

- `images/ui/top_panel/character_icon_<entry-lower>.png`
- `images/ui/top_panel/character_icon_<entry-lower>_outline.png`
- `scenes/ui/character_icons/<entry-lower>_icon.tscn`

结论：

- 若说“角色 portrait / 面板头像”，在 STS2 当前资源结构里，应优先理解为顶栏头像这一组资源
- 不是另有一套必须单独命名的“大头像 PNG”

### 5. 角色选择大立绘

- 角色选择主画面里，原版既有：
  - `char_select_<entry>.png`
- 也有：
  - `char_select_bg_<entry>.tscn`

结论：

- 选人页至少有一张角色主立绘 PNG
- 原版完整角色通常还带一套更丰富的背景 / 动画 scene

## 五 哪些资源首版“没有就不行”

基于原版结构，本轮建议把以下对象视为自定义角色首版必备：

- 角色选择主立绘
- 顶栏头像
- 顶栏头像 scene 包装位
- 角色卡牌费用图标
- 角色战斗能量计数器资源
- 每张可见卡牌的 portrait 文件
- 每个自定义 power / debuff 的 icon
- 每个自定义 relic 的 icon

说明：

- 这里的“必备”是按当前原版结构与 UI 消费位置判断
- 不是说今天就要把它们全部做成最终精修版
- 但这些资源位不能空着不规划

## 六 哪些资源首版可以先占位

当前可以先占位，但应提前留好资源位：

- 角色选择锁定图
- 顶栏头像轮廓
- 角色选择背景 scene
- 联机手势图
- 全量卡图的正式成稿
- beta 卡图 / beta relic 图

## 七 本轮结论

- 原版角色资源不是只有“选人图 + 一张头像”这么简单。
- 与角色强绑定的最小资源束，至少包括：
  - 选人图
  - 顶栏头像组
  - 费用图标
  - 战斗能量计数器
- 卡牌框体、通用按钮轮廓、共用 UI 底板属于系统资源，可以复用。
- 对 `Togawasakiko_in_Slay_the_Spire` 来说，首版应先把这些角色固有资源槽位建完整，再谈大规模卡图制作。
