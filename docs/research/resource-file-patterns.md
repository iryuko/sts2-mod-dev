# 资源文件命名模式

记录日期：2026-03-23

## 卡牌

`CardModel` 已确认的资源模板：

- 图集资源：`atlases/card_atlas.sprites/<pool>/<entry-lower>.tres`
- PNG 卡图：`packed/card_portraits/<pool>/<entry-lower>.png`
- beta 卡图：`packed/card_portraits/<pool>/beta/<entry-lower>.png`
- 叠层场景：`cards/overlays/<entry-lower>`

代表例子：

- `PRIMAL_FORCE -> ironclad/primal_force.png`
- `STRIKE_IRONCLAD -> ironclad/strike_ironclad.png`
- `GIANT_ROCK -> token/giant_rock.png`

## Power / Debuff

`PowerModel` 已确认：

- 打包图集：`atlases/power_atlas.sprites/<entry-lower>.tres`
- 大图：`images/powers/<entry-lower>.png`
- beta 大图候选：`images/powers/beta/<entry-lower>.png`

代表例子：

- `WEAK_POWER -> weak_power.png`
- `VULNERABLE_POWER -> vulnerable_power.png`
- `STRENGTH_POWER -> strength_power.png`

## Relic

`RelicModel` 已确认：

- 打包图集：`atlases/relic_atlas.sprites/<entry-lower>.tres`
- 轮廓图集：`atlases/relic_outline_atlas.sprites/<entry-lower>.tres`
- 大图：`images/relics/<entry-lower>.png`
- beta 大图候选：`images/relics/beta/<entry-lower>.png`

代表例子：

- `AKABEKO -> akabeko.png`
- `BURNING_BLOOD -> burning_blood.png`
- `STRIKE_DUMMY -> strike_dummy.png`

## 角色资源

`CharacterModel` 已确认：

- 顶栏图标：`images/ui/top_panel/character_icon_<entry-lower>.png`
- 顶栏轮廓：`images/ui/top_panel/character_icon_<entry-lower>_outline.png`
- 角色头像 scene：`scenes/ui/character_icons/<entry-lower>_icon.tscn`
- 选人图：`packed/character_select/char_select_<entry-lower>.png`
- 选人锁定图：`packed/character_select/char_select_<entry-lower>_locked.png`
- 选人背景：`scenes/screens/char_select/char_select_bg_<entry-lower>.tscn`
- 联机手势图：`images/ui/hands/multiplayer_hand_<entry-lower>_<pose>.png`

代表例子：

- `IRONCLAD -> char_select_ironclad.png`, `character_icon_ironclad.png`, `ironclad_icon.tscn`
- `DEFECT -> char_select_defect.png`, `character_icon_defect.png`, `defect_icon.tscn`
- `RANDOM_CHARACTER -> char_select_random.png`, `character_icon_random_character.png`

## 结论

- 资源层最常见的名字不是显示名，也不是类名原文，而是 `Entry` 的小写蛇形版本。
- 卡牌额外受 `CardPool.Title` 影响，因此同一个 `Entry` 规则还要配合对象所属池目录。

## 待验证

- 某些特殊 UI atlas 是否会用人工命名而不是 `entry-lower` 模板。
- 少数 beta / fallback 图是否会在缺图时替代标准路径。
