# 命名与资源规范

日期：2026-04-04

## 一 目的

本文件用于把命名规范落实到资源层。

当前目标不是提前拍死全部最终文案，而是固定：

- 逻辑层如何命名
- 资源文件如何命名
- 资源目录如何落位
- song 子标签如何进入资源体系

## 二 当前主规则

当前项目继续沿用已确认的原版主链：

- 类名：`PascalCase`
- `Entry`：由类名派生的全大写蛇形
- 资源文件 base name：`entry.ToLowerInvariant()`
- 本地化 key：围绕同一个 `Entry`

这条主链应始终保持一致：

- `ClassName`
- `ENTRY_NAME`
- `entry_name`
- `<Entry>.title / .description / ...`

## 三 稳定主键必须先于显示名

以下对象都必须先有稳定英文主键，再谈显示名：

- 角色
- 卡牌
- 压力
- 原创 debuff
- relic
- song tag
- 资源文件名
- 本地化 key

当前要求：

- 不把中文直接写进类名、`Entry` 或正式资源文件名
- 不把数值、稀有度、语言信息塞进内部 ID
- 不把显示名当作资源文件名

## 四 显示层与逻辑层分离

属于显示层的内容：

- 卡牌标题
- 卡牌描述
- power / debuff 标题与描述
- relic 标题、描述、flavor
- 角色显示名、说明文本、代词字段

属于逻辑层的内容：

- 类名
- `Entry`
- 资源文件 base name
- console / 调试输入名

## 五 角色命名规则

角色对象当前应按以下链路推进：

- 类名：`Togawasakiko`
- `Entry`：`TOGAWASAKIKO`
- 资源 base name：`togawasakiko`

## 六 角色资源命名规则

### 1. 角色选择图

- 主立绘：
  - `char_select_togawasakiko.png`
- 锁定图：
  - `char_select_togawasakiko_locked.png`

### 2. 面板头像

- 主头像：
  - `character_icon_togawasakiko.png`
- 轮廓：
  - `character_icon_togawasakiko_outline.png`

### 3. 头像 scene 包装位

未来若进入运行时资源层，应对齐原版样式：

- `togawasakiko_icon.tscn`

说明：

- 当前工作区 `assets/` 先存源图片
- `tscn` 只作为未来运行时目标命名预留，不在本轮提前制作

### 4. 角色能量资源

- 卡牌费用图标源图：
  - `energy_togawasakiko.png`
- 战斗能量球图层：
  - `togawasakiko_orb_layer_1.png`
  - `togawasakiko_orb_layer_2.png`
  - `togawasakiko_orb_layer_3.png`
  - `togawasakiko_orb_layer_4.png`
  - `togawasakiko_orb_layer_5.png`

### 5. 角色选人背景与联机手势

- 选人背景源文件目录：
  - `char_select_bg/`
- 游戏内静态立绘：
  - `in_combat_portrait_togawasakiko.png`
- 火堆房角色坐姿图：
  - `rest_site_character_togawasakiko.png`
- 联机手势：
  - `multiplayer_hand_togawasakiko_rock.png`
  - `multiplayer_hand_togawasakiko_paper.png`
  - `multiplayer_hand_togawasakiko_scissors.png`
  - `multiplayer_hand_togawasakiko_point.png`

## 七 卡牌命名规则

### 1. 通用规则

卡牌应采用：

- 类名：`PascalCase`
- `Entry`：全大写蛇形
- 卡图文件名：`entry-lower`
- 本地化 key：
  - `cards/<Entry>.title`
  - `cards/<Entry>.description`

### 2. 基础牌

基础打击 / 防御应按原版风格带角色后缀做消歧：

- `StrikeTogawasakiko -> STRIKE_TOGAWASAKIKO -> strike_togawasakiko.png`
- `DefendTogawasakiko -> DEFEND_TOGAWASAKIKO -> defend_togawasakiko.png`

### 3. 当前冻结候选起始牌

- `Slander -> SLANDER -> slander.png`
- `Unendurable -> UNENDURABLE -> unendurable.png`

### 4. 压力衍生牌

当前已经冻结英文内部名的对象：

- `PersonaDissociation -> PERSONA_DISSOCIATION -> persona_dissociation.png`
- `SocialWithdrawal -> SOCIAL_WITHDRAWAL -> social_withdrawal.png`
- `AllYouThinkAboutIsYourself -> ALL_YOU_THINK_ABOUT_IS_YOURSELF -> all_you_think_about_is_yourself.png`

当前仍未冻结英文内部名的对象：

- 【过劳焦虑】

规则：

- 在英文稳定主键未冻结前，不抢锁正式文件名
- 先把资源目录位留好，不把中文显示名直接写进正式文件名

### 5. relic 附带入组卡

新增一类当前已明确需要、但不应混入正常卡池目录的卡图：

- `assets/cards/relic_granted/`

适用对象：

- 由 relic 直接加入牌组的卡
- 尤其是先古之民 relic 在获得时附带塞入牌组的卡

规则：

- 文件名仍然遵循 `entry.ToLowerInvariant()`
- 但目录落位不使用 `normal/common|uncommon|rare`
- 因为这类卡的维护重点是“获取来源”，不是“正常奖励池稀有度”

## 八 压力与原创 debuff 命名规则

### 1. 压力

- `PressurePower`
- `PRESSURE_POWER`
- `pressure_power.png`
- `powers/PRESSURE_POWER.title`
- `powers/PRESSURE_POWER.description`

### 2. 原创 debuff

- `PersonaDissociationPower -> PERSONA_DISSOCIATION_POWER -> persona_dissociation_power.png`
- `SocialWithdrawalPower -> SOCIAL_WITHDRAWAL_POWER -> social_withdrawal_power.png`
- `InferiorityPower -> INFERIORITY_POWER -> inferiority_power.png`

说明：

- buff / debuff 的区别不写进命名后缀，而是由对象类型表达
- 当前这三组英文语义名已可视作本项目稳定候选

## 九 Relic 命名规则

relic 应采用：

- 类名：`PascalCase`
- `Entry`：全大写蛇形
- 图标文件：`entry-lower`
- 本地化 key：
  - `relics/<Entry>.title`
  - `relics/<Entry>.description`
  - `relics/<Entry>.flavor`

当前 starter relic：

- `DollMask -> DOLL_MASK -> doll_mask.png`

当前 relic 库建议拆成：

- `assets/relics/starter/`
- `assets/relics/ancient/`
- `assets/relics/future/`

其中：

- `starter/`
  - 角色起始 relic
- `ancient/`
  - 先古之民事件给予的 relic
- `future/`
  - 尚未归类到 starter / ancient 的其它专属 relic

对于“先古之民给 relic，relic 再塞卡”的设计：

- relic 图标走 `assets/relics/ancient/`
- 被塞入牌组的卡图走 `assets/cards/relic_granted/`
- 不要为了图省事把二者混放进同一目录

## 十 song 子标签命名规则

song 当前不是 card pool 名，也不是显示文本替代名。
它首先是逻辑标签，而不是当前必需图像资源。

当前建议：

- 标签内部名：
  - `SongTag`

当前要求：

- 不把 `song` 拼进正式卡图文件名
- 不把 song 牌单独移出正常 rarity 目录
- 用登记文档记录 song 身份，而不是靠文件路径暗示

未来如果真的需要做 UI 可视化，再补可选资源位：

- `assets/icons/tags/song_tag.png`
- `assets/cards/placeholders/song_tag_overlay.png`

但这两个名字当前只作为预留，不是首版必需资源。

## 十一 音频命名规则

音频正式库存同样先有稳定主键，再谈显示名。

建议链路：

- 完整曲目：`TrackId -> track_id -> assets/audio/music/tracks/<track_id>.ogg`
- 歌曲牌短 cue：`CueId -> cue_id -> assets/audio/music/song_cues/<cue_id>.ogg`
- 角色选人 / UI 音效：`CueId -> cue_id -> assets/audio/sfx/<group>/<cue_id>.wav`

当前规则：

- 文件名统一使用 ASCII `lower_snake_case`
- 不把中文显示名直接写进正式音频文件名
- 同一首歌的完整播放版与短 cue 版应共用同一个词根
- 歌曲牌播放与点歌系统优先通过 `track_id` 绑定，而不是各自硬写不同文件名

当前预留示例：

- `togawasakiko_title_theme.ogg`
- `ave_mujica.ogg`
- `killkiss.ogg`
- `ave_mujica_cue.ogg`
- `togawasakiko_select.wav`
- `jukebox_open.wav`

## 十二 资源目录与文件落位规则

### 0. 原创先古之民参考资产

当先古之民英文内部名尚未冻结时，正式库存使用 `prototype` 占位，不把草稿误写成已冻结对象：

- `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype.png`
- `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype_outline.png`
- `assets/ancients/dialogue_icons/previews/ancient_dialogue_icon_prototype_preview.png`
- `assets/ancients/map_nodes/ancient_map_node_prototype.png`
- `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png`
- `assets/ancients/map_nodes/previews/ancient_map_node_prototype_baked_preview.png`

说明：

- `dialogue_icons` 这一组是先古之民对话头像与 outline 本体
- `map_nodes` 这一组是先古之民地图节点的白色 mask 资源本体
- `preview` 只用于查看接近原版 runtime 的叠放或着色效果，不作为源资源

### 1. 角色

- `assets/character/char_select_bg/char_select_bg_togawasakiko.png`
- `assets/character/char_select/char_select_togawasakiko.png`
- `assets/character/char_select/char_select_togawasakiko_locked.png`
- `assets/character/in_combat_portrait/in_combat_portrait_togawasakiko.png`
- `assets/character/rest_site/rest_site_character_togawasakiko.png`
- `assets/character/top_panel/character_icon_togawasakiko.png`
- `assets/character/top_panel/character_icon_togawasakiko_outline.png`
- `assets/character/energy/energy_togawasakiko.png`

### 2. 卡牌

- `assets/cards/basic/<entry-lower>.png`
- `assets/cards/relic_granted/<entry-lower>.png`
- `assets/cards/normal/common/<entry-lower>.png`
- `assets/cards/normal/uncommon/<entry-lower>.png`
- `assets/cards/normal/rare/<entry-lower>.png`
- `assets/cards/generated_pressure/<entry-lower>.png`

### 3. power / debuff icon

- `assets/icons/powers/<entry-lower>.png`

### 4. tag icon

- `assets/icons/tags/<entry-lower>.png`

### 5. relic 图

- `assets/relics/starter/<entry-lower>.png`
- `assets/relics/ancient/<entry-lower>.png`
- `assets/relics/future/<entry-lower>.png`

### 6. 音频

- `assets/audio/music/tracks/<track_id>.ogg`
- `assets/audio/music/song_cues/<cue_id>.ogg`
- `assets/audio/sfx/character_select/<cue_id>.wav`
- `assets/audio/sfx/jukebox/<cue_id>.wav`
- `assets/audio/sfx/ui/<cue_id>.wav`

## 十三 占位资产规则

占位分两类：

- 通用模板：
  - 允许放在 `assets/cards/placeholders/`
- 实际交付给某张牌的占位图：
  - 仍要使用该牌自己的正式文件名

例如：

- 模板可以叫：
  - `placeholder_normal.png`
- 真正给 `SLANDER` 使用时仍应落成：
  - `slander.png`

这样做的目的：

- 避免“占位阶段”污染最终命名
- 后续替换正式图时无需改名

## 十四 本地化表规则

继续按原版分表：

- 卡牌：`cards.json`
- power / debuff：`powers.json`
- relic：`relics.json`
- 角色：`characters.json`

当前要求：

- `eng` 作为主 key 集合
- `zhs` 与 `eng` 保持相同 key，只改 value
- 代码与资源命名都引用 `Entry`

## 十五 console / 测试输入规则

如后续对象需要经常用 console 验证，应优先让输入名直接等于完整 `Entry`：

- `card STRIKE_TOGAWASAKIKO`
- `card SLANDER`
- `power PRESSURE_POWER 1 1`
- `power INFERIORITY_POWER 1 1`
- `relic DOLL_MASK`

## 十六 当前结论

- 当前项目已经把原版命名主轴真正接到了资源层。
- 角色 portrait、选人立绘、头像、relic 图、压力 icon、原创 debuff icon、卡图占位、音频资产，都已有可执行的命名方式。
- song 作为正常卡池子标签，已经进入正式命名规则，不再是后补需求。
