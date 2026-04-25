# 资产总表

日期：2026-04-04

## 一 统计口径

本文件按“唯一资源条目”统计，不按牌组里同名牌的复制张数统计。

当前冻结前提：

- `Basic` 层独立统计
- 正常卡池 `50` 张，不含压力衍生牌
- 压力衍生牌当前固定 `4` 张
- song 牌属于正常卡池子集，不单独另算一套卡池

当前应预留的卡图总量：

- `Basic`：`4`
- 正常卡池：`50`
- 压力衍生牌：`4`
- 合计：`58`

## 二 A 类：角色固有资产

| 资产 | 建议文件 | 用途 | 当前优先级 | 当前状态 | 首版可占位 |
| --- | --- | --- | --- | --- | --- |
| 角色选择背景主立绘 | `assets/character/char_select_bg/char_select_bg_togawasakiko.png` | 角色选择界面背景大图 | 高 | 已入正式库 | 否 |
| 角色选择头像 | `assets/character/char_select/char_select_togawasakiko.png` | 角色选择界面按钮识别图 | 高 | 已入正式库 | 否 |
| 游戏内静态立绘 | `assets/character/in_combat_portrait/in_combat_portrait_togawasakiko.png` | 战斗内角色主视觉静态成图 | 高 | 已入正式库 | 否 |
| 火堆房间角色坐姿图 | `assets/character/rest_site/rest_site_character_togawasakiko.png` | 火堆房间角色坐姿正式图 | 中 | 已入正式库 | 否 |
| 角色选择锁定图 | `assets/character/char_select/char_select_togawasakiko_locked.png` | 选人界面锁定态 | 低 | 已入库，当前仍为占位 | 是 |
| 顶栏头像 | `assets/character/top_panel/character_icon_togawasakiko.png` | 顶栏、卡库、多人信息等 | 高 | 已入正式库 | 否 |
| 顶栏头像轮廓 | `assets/character/top_panel/character_icon_togawasakiko_outline.png` | 顶栏轮廓 / 高亮态 | 低 | 已入库，当前仍为占位 | 是 |
| 角色费用图标 | `assets/character/energy/energy_togawasakiko.png` | 卡牌费用图标源图 | 高 | 已入正式库 | 否 |
| 角色能量球图层 | `assets/character/energy/togawasakiko_orb_layer_1.png` 至 `togawasakiko_orb_layer_5.png` | 战斗内能量计数器 | 高 | `1` 至 `5` 已入正式库 | 否 |
| 联机手势图 | `assets/character/multiplayer_hand/multiplayer_hand_togawasakiko_<pose>.png` | 联机石头剪刀布等 | 低 | `point / paper / rock / scissors` 已入正式库 | 是 |

结论：

- “角色 portrait / 面板头像”在当前项目里应优先落到 `top_panel` 这组资源。
- 能量 icon 与能量球虽然不是最先想到的角色图，但在原版里是角色固有 UI 的一部分。

## 三 B 类：正常卡池资产

### 1. `Basic` 子集

| 对象 | 建议内部名 | 建议卡图文件 | 说明 |
| --- | --- | --- | --- |
| 打击 | `StrikeTogawasakiko` | `assets/cards/basic/strike_togawasakiko.png` | `4` 张复制，共用 `1` 份卡图 |
| 防御 | `DefendTogawasakiko` | `assets/cards/basic/defend_togawasakiko.png` | `4` 张复制，共用 `1` 份卡图 |
| 专属起始攻击牌 | `Slander` | `assets/cards/basic/slander.png` | 已冻结候选名 |
| 专属起始技能牌 | `Unendurable` | `assets/cards/basic/unendurable.png` | 已冻结候选名 |

当前状态补充：

- `4` 张 `Basic` 卡图已入正式库
- 当前保持原始上传尺寸，不额外缩放到 `1000x760`

### 2. 正常卡池 `50` 张

| 子类 | 数量 | 建议目录 | 说明 |
| --- | --- | --- | --- |
| `Common` | `12` | `assets/cards/normal/common/` | 正常奖励池 / 商店池的一部分 |
| `Uncommon` | `22` | `assets/cards/normal/uncommon/` | 当前冻结工作版最大头 |
| `Rare` | `16` | `assets/cards/normal/rare/` | 高价值 payoff 层 |

当前状态补充：

- 当前 `assets/cards/normal/` 已有 `31` 张独立卡图正式入库，并已被当前源码与 `pack/` 接线使用
- `Common` 已入库 `10` 张：
  - `answer_me`
  - `face`
  - `god_you_fool`
  - `perk_up`
  - `put_on_your_mask`
  - `s_the_way`
  - `saki_move_plz`
  - `sever_the_past`
  - `she_is_radiant`
  - `speak`
- `Uncommon` 已入库 `16` 张：
  - `black_birthday`
  - `completeness`
  - `compose`
  - `crucifix_x`
  - `ether`
  - `georgette_me_georgette_you`
  - `imprisoned_xii`
  - `innocence`
  - `leave_it_to_me`
  - `masquerade_rhapsody_request`
  - `music_of_the_celestial_sphere`
  - `notebook`
  - `sophie`
  - `thrilled`
  - `treasure_pleasure`
  - `two_moons_deep_into_the_forest`
- `Rare` 已入库 `5` 张：
  - `ave_mujica`
  - `choir_s_choir`
  - `final_curtain`
  - `i_have_ascended`
  - `killkiss`
- 按冻结总量 `50` 张计算，当前 normal 池仍有 `19` 张卡图待补

### 3. song 子集

song 牌不单开另一套卡图目录。

当前规则：

- song 牌仍然放在各自 rarity 目录
- 是否属于 song 子集，写进：
  - `song-subset-registry.md`
- 若后续需要统一视觉角标，也只加一个共用 tag icon，不复制出第二份 song 专用卡图

## 四 C 类：压力体系资产

### 1. 压力与原创 debuff icon

| 对象 | 建议内部名 | 建议图标文件 | 当前状态 |
| --- | --- | --- | --- |
| 压力 | `PRESSURE_POWER` | `assets/icons/powers/pressure_power.png` | 命名已冻结 |
| 人格解离 | `PERSONA_DISSOCIATION_POWER` | `assets/icons/powers/persona_dissociation_power.png` | 命名已冻结 |
| 自闭 | `SOCIAL_WITHDRAWAL_POWER` | `assets/icons/powers/social_withdrawal_power.png` | 命名已冻结 |
| 自卑 | `INFERIORITY_POWER` | `assets/icons/powers/inferiority_power.png` | 命名已冻结 |

### 2. 压力衍生牌卡图

| 对象 | 建议资源位 | 命名状态 | 说明 |
| --- | --- | --- | --- |
| 人格解离 | `assets/cards/generated_pressure/persona_dissociation.png` | 可直接使用英文词根 | 压力衍生牌，不进正常卡池 |
| 自闭 | `assets/cards/generated_pressure/social_withdrawal.png` | 可直接使用英文词根 | 压力衍生牌，不进正常卡池 |
| 满脑子都想着自己 | `assets/cards/generated_pressure/all_you_think_about_is_yourself.png` | 命名已冻结 | 压力衍生牌，不进正常卡池 |
| 过劳焦虑 | `assets/cards/generated_pressure/overwork_anxiety.png` | 当前 runtime 已按该文件名落库 | 压力衍生牌，不进正常卡池；若后续统一改英文稳定主键，需同步重命名 |

结论：

- 压力体系的独有资产不只是 `4` 个 icon。
- 它还包括 `4` 张压力衍生牌 portrait。

## 五 D 类：song 子标签支持位

| 对象 | 建议落位 | 当前必要性 | 说明 |
| --- | --- | --- | --- |
| song 子集登记文档 | `docs/song-subset-registry.md` | 高 | 记录哪些正常卡带 `song tag` |
| song 标记字段 | 正常卡池资产表字段 | 高 | 在资产清单中标记 song 身份 |
| 未来可选可视化位 | `assets/icons/tags/` | 低 | 只有后续确实要做 UI 可视化时才补 |

本轮结论：

- song 当前是逻辑标签，不是当前必需图像资产
- 至少需要一份独立登记文档和稳定字段规则
- 不需要为 song 牌再单独复制整套卡图目录

## 六 E 类：relic 与后续扩展资产

| 资产 | 建议文件 | 当前状态 |
| --- | --- | --- |
| starter relic 图 | `assets/relics/starter/doll_mask.png` | 已入正式库 |
| starter relic 升级态图 | `assets/relics/starter/upgraded_doll_mask.png` | 已建立独立正式文件位，当前仍复用 `doll_mask` 视觉 |
| 先古之民 relic 图 | `assets/relics/ancient/best_companion.png` | 已入正式库 |
| 先古之民 relic 图 | `assets/relics/ancient/black_limousine.png` | 已入正式库 |
| relic 附带入组卡图 | `assets/cards/relic_granted/barking_barking_barking.png` | 已入正式库 |
| relic 附带入组卡图 | `assets/cards/relic_granted/pullman_crash.png` | 已入正式库 |
| event-only 长期牌卡图 | `assets/cards/event_granted/shadow_of_the_past_i.png` | 已入正式库 |
| event-only 长期牌卡图 | `assets/cards/event_granted/shadow_of_the_past_ii.png` | 已入正式库 |
| event-only 长期牌卡图 | `assets/cards/event_granted/shadow_of_the_past_iii.png` | 已入正式库 |
| 普通问号房 `Shadow` 事件主图 | `assets/events/question_room/shadow_of_the_past/shadow_of_the_past_event.png` | 已建目录，待来稿 |
| 普通问号房 `Shadow` 事件背景目录 | `assets/events/question_room/shadow_of_the_past/background/` | 已预留 |
| 未来专属 relic 图 | `assets/relics/future/` | 预留 |
| 未来更多 power / debuff icon | `assets/icons/powers/` | 预留 |
| 后续角色 UI 小图 | `assets/icons/ui/` | 预留 |
| 替换正式卡图计划位 | `assets/cards/placeholders/` | 已预留 |

## 七 F 类：原创先古之民参考资产

当前这组不是角色主线 runtime 已接入资产，而是“已入正式库存、可继续迭代”的原型参考资产。

| 资产 | 建议文件 | 当前状态 | 说明 |
| --- | --- | --- | --- |
| 先古之民地图节点主图原型 | `assets/ancients/map_nodes/ancient_map_node_prototype.png` | 已规范化入正式库存 | 当前与 `incoming_assets/ancients/map_node/mapnode.png` 一致；已扩到 `416x416`；未接入 `pack/` |
| 先古之民地图节点 outline 原型 | `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png` | 已规范化入正式库存 | 当前与 `incoming_assets/ancients/map_node_outline/mapnode_outline.png` 一致；未接入 `pack/` |
| 地图节点烘焙预览 | `assets/ancients/map_nodes/previews/ancient_map_node_prototype_baked_preview.png` | 已入正式库 | 仅用于查看接近原版 runtime 的着色效果 |
| 先古之民对话头像原型 | `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype.png` | 已规范化入正式库存 | 当前与 `incoming_assets/ancients/dialogue_icon/丰川定治.png` 一致；未接入 `pack/` |
| 先古之民对话头像 outline 原型 | `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype_outline.png` | 已规范化入正式库存 | 当前正式库存来自 auto-generated outline；未与 `incoming_assets/ancients/dialogue_icon_outline/丰川定治.png` 对齐 |
| 对话头像预览 | `assets/ancients/dialogue_icons/previews/ancient_dialogue_icon_prototype_preview.png` | 已入正式库 | 仅用于查看接近原版叠放效果 |
| 先古之民主图来稿 | `incoming_assets/ancients/event_main/丰川定治.png` | 仅保留来稿 | 当前尺寸 `2560x1244`；尚未规范化进入 `assets/` 或 `pack/` |

补充规则：

- 先古之民英文内部名未冻结前，不把 `prototype` 误记成最终对象名。
- 原版事件对话里的玩家角色头像通常复用 `top_panel` 头像与 outline，不需要另做一套角色专用 `dialogue icon`。
- `best_companion` relic 图与 `barking_barking_barking` 卡图已经完成规范化入库，并已进入 `pack/`。

## 八 G 类：音频资产库

当前这组用于歌曲牌播放、角色选择音效与局内点歌系统，不改变既有卡图或 `song` 标签规则。

| 资产 | 建议文件 | 当前状态 | 说明 |
| --- | --- | --- | --- |
| 角色选择确认音效 | `assets/audio/sfx/character_select/togawasakiko_select.ogg` | 已入正式库 | 当前由来稿 `oblivionis.mp3` 规范化转存为 `ogg`；后续可用于替换当前默认 `CharacterSelectSfx` |
| 角色选择 hover 音效 | `assets/audio/sfx/character_select/togawasakiko_hover.wav` | 已建目录，待来稿 | 可选项 |
| 点歌系统打开音效 | `assets/audio/sfx/jukebox/jukebox_open.wav` | 已建目录，待来稿 | 点歌 UI 操作音效 |
| 点歌系统确认音效 | `assets/audio/sfx/jukebox/jukebox_confirm.wav` | 已建目录，待来稿 | 点歌 UI 操作音效 |
| 点歌系统切歌音效 | `assets/audio/sfx/jukebox/jukebox_next_track.wav` | 已建目录，待来稿 | 点歌 UI 操作音效 |
| 点歌系统停止音效 | `assets/audio/sfx/jukebox/jukebox_stop.wav` | 已建目录，待来稿 | 点歌 UI 操作音效 |
| 歌曲完整曲目库存 | `assets/audio/music/tracks/<track_id>.mp3` | 已入正式库 `23` 首，已进入 runtime | 当前 `jukebox` 曲库本轮按 `mp3` 收口；歌曲牌与点歌系统优先共用这一层 |
| `Symbol I` 曲目 | `assets/audio/music/tracks/symbol_i.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| `Symbol II` 曲目 | `assets/audio/music/tracks/symbol_ii.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| `Symbol III` 曲目 | `assets/audio/music/tracks/symbol_iii.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| `Symbol IV` 曲目 | `assets/audio/music/tracks/symbol_iv.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| `A Wonderful World, Yet Nowhere to Be Found` 曲目 | `assets/audio/music/tracks/a_wonderful_world_yet_nowhere_to_be_found.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| `Angles` 曲目 | `assets/audio/music/tracks/angles.mp3` | 已入正式库 | 2026-04-21 已由来稿转存并同步到 runtime |
| 歌曲牌短 cue 库存 | `assets/audio/music/song_cues/<cue_id>.ogg` | 已建目录，待判断是否需要 | 只有完整曲目不适合直接播放时才单开 |
| 事件专用背景音乐库存 | `assets/audio/music/events/<event_id>.mp3` | 已建目录，首条已入正式库 | 当前用于承接 `UnattendedPiano` 等普通事件专用音乐 |
| `UnattendedPiano` 事件音乐 | `assets/audio/music/events/unattended_piano.mp3` | 已入正式库 | 当前由来稿 `UnattendedPiano-La Fille Aux Checeux De Lin.m4a` 规范化转存为 `mp3`；已同步到 runtime |

当前登记的稳定主键见：

- `docs/audio-track-registry.md`

## 九 首版占位与正式制作分界

### 1. 建议尽快做正式版

- `char_select_bg_togawasakiko.png`
- `char_select_togawasakiko.png`
- `in_combat_portrait_togawasakiko.png`
- `rest_site_character_togawasakiko.png`
- `character_icon_togawasakiko.png`
- `energy_togawasakiko.png`
- `togawasakiko_orb_layer_*.png`
- `pressure_power.png`
- `persona_dissociation_power.png`
- `social_withdrawal_power.png`
- `inferiority_power.png`
- `doll_mask.png`
- 角色选择确认音效
- 首批可复用的歌曲完整曲目

### 2. 首版允许占位

- `54` 张非衍生角色卡图
- `4` 张压力衍生牌卡图
- `multiplayer_hand_togawasakiko_point.png`
- `multiplayer_hand_togawasakiko_*`
- `assets/audio/music/song_cues/`
- `assets/audio/sfx/jukebox/`
- `assets/audio/sfx/ui/`

## 十 本轮结论

- 本角色当前已经有完整的资产总表骨架，并且真实入库状态明显领先于 2026-03-24 旧版文档。
- 角色固有资源、`Basic 4` 张、压力衍生牌 `4` 张、先古之民 relic / relic-granted card 都已入库。
- 当前已实装的 normal 池卡图共 `31` 张已落库并接线；按冻结目标仍有 `19` 张待补。
- 锁定图、顶栏 outline 与部分事件型 power icon 当前仍属于“文件已存在但视觉仍占位”的层级。
- 丰川定治相关地图节点 / 对话头像当前已用 `prototype` 名义入正式库存，但仍未接入 runtime。
- 音频资产库已完成目录建模，并已有角色选人音效与 `16` 首 `jukebox` 曲目正式入库。
