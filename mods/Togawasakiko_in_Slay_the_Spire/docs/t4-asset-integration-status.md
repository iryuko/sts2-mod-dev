# T4 资源接入状态

日期：2026-04-05

## 一 本文件定位

本文件只回答三件事：

- 当前哪些对象已经接入且文件存在
- 哪些已经接入但当前仍是占位
- 哪些对象曾在文档里预留，但当前最小闭环仍未接入

说明：

- 本文件以当前仓库实际文件状态为准
- 不再把“文档里规划过”直接写成“已接入”
- 当前同时关注：
  - `assets/` 中的源资源状态
  - `pack/` 中的 runtime 资源状态
- 当前 `pack/` 中的贴图资源不再只看裸文件是否存在，还要求：
  - 对应 `.import` 已生成
  - 对应 `.godot/imported/*.ctex` 已能随 PCK 一起打包

## 二 已接入且文件存在

### 1. 角色固有资源

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 备注 |
| --- | --- | --- | --- | --- |
| 角色选择主图 | `assets/character/char_select/char_select_togawasakiko.png` | `pack/images/packed/character_select/char_select_togawasakiko.png` | 已接入且文件存在 | 2026-03-28 已由 `incoming_assets/character_select_icon/` 更新 |
| 角色选择背景图 | `assets/character/char_select_bg/char_select_bg_togawasakiko.png` | `pack/images/packed/character_select/char_select_bg_togawasakiko.png` | 已接入且文件存在 | 2026-03-28 已由 `incoming_assets/character_select_splash/` 更新 |
| 游戏内静态立绘 | `assets/character/in_combat_portrait/in_combat_portrait_togawasakiko.png` | `pack/mod_assets/character/in_combat_portrait_togawasakiko.png` | 已接入且文件存在 | 2026-03-28 已由 `incoming_assets/character_in_combat_portrait/` 更新 |
| 顶栏头像 | `assets/character/top_panel/character_icon_togawasakiko.png` | `pack/images/ui/top_panel/character_icon_togawasakiko.png` | 已接入且文件存在 | 2026-03-28 已由 `incoming_assets/top_panel_portrait/` 更新 |
| 角色费用图标 | `assets/character/energy/energy_togawasakiko.png` | `pack/images/ui/card/energy_togawasakiko.png` | 已接入且文件存在 | `.tres` 已引用 |
| 文本内嵌能量 icon | 无独立上传源图 | `pack/images/packed/sprite_fonts/togawasakiko_energy_icon.png` | 已接入且文件存在 | 2026-03-30 由大费用图临时缩小生成；同日夜间先缩小主体，后按实机反馈把画布收紧到更接近文本 glyph 的尺寸，当前用于对白 / hover / 事件文本里的小尺寸 icon |
| 战斗能量球图层 | `assets/character/energy/togawasakiko_orb_layer_1.png` 至 `5.png` | `pack/images/ui/combat/energy_counters/togawasakiko/` | 已接入且文件存在 | 当前用于 `togawasakiko_energy_counter.tscn` |
| starter relic 图 | `assets/relics/starter/doll_mask.png` | `pack/images/relics/doll_mask.png` | 已接入且文件存在 | 使用正式资源 |
| ancient relic 图 | `assets/relics/ancient/best_companion.png` | `pack/images/relics/best_companion.png` | 已接入且文件存在 | 2026-04-03 已由 `incoming_assets/relics/ancient/` 更新 |
| ancient relic 图 | `assets/relics/ancient/black_limousine.png` | `pack/images/relics/black_limousine.png` | 已接入且文件存在 | 2026-04-04 已由中文来稿 `黑色高级车.png` 对齐替换 |

### 2. runtime scene / atlas

| 对象 | 当前路径 | 状态 | 备注 |
| --- | --- | --- | --- |
| 角色选择背景 scene | `pack/scenes/screens/char_select/char_select_bg_togawasakiko.tscn` | 已接入且文件存在 | 本轮已修正背景图引用 |
| 顶栏 icon scene | `pack/scenes/ui/character_icons/togawasakiko_icon.tscn` | 已接入且文件存在 | 已对齐当前顶栏头像路径 |
| 能量计数器 scene | `pack/scenes/combat/energy_counters/togawasakiko_energy_counter.tscn` | 已接入且文件存在 | 已有图层文件 |
| 角色战斗立绘 scene | `pack/scenes/creature_visuals/togawasakiko.tscn` | 已接入且文件存在 | 已有静态立绘文件 |
| card trail scene | `pack/scenes/vfx/card_trail_togawasakiko.tscn` | 已接入且文件存在 | 已恢复回合结束移牌与出牌 trail 所需链路 |
| 费用图标 atlas | `pack/images/atlases/ui_atlas.sprites/card/energy_togawasakiko.tres` | 已接入且文件存在 | 已有实际贴图文件可供引用 |
| starter relic atlas | `pack/images/atlases/relic_atlas.sprites/doll_mask.tres` | 已接入且文件存在 | 当前用单图 `AtlasTexture` 占位 |
| starter relic outline atlas | `pack/images/atlases/relic_outline_atlas.sprites/doll_mask.tres` | 已接入且文件存在 | 当前用同图占位 |
| transition material | `pack/materials/transitions/togawasakiko_transition_mat.tres` | 已接入且文件存在 | 当前引用原版 transition 贴图作最小占位 |

## 三 已接入但当前仍是占位

### 1. 角色固有占位

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 占位来源 |
| --- | --- | --- | --- | --- |
| 角色选择锁定图 | `assets/character/char_select/char_select_togawasakiko_locked.png` | `pack/images/packed/character_select/char_select_togawasakiko_locked.png` | 已接入但仍是占位 | 当前用选角图复制占位 |
| 顶栏头像轮廓 | `assets/character/top_panel/character_icon_togawasakiko_outline.png` | `pack/images/ui/top_panel/character_icon_togawasakiko_outline.png` | 已接入但仍是占位 | 当前用主头像复制占位 |
| starter relic outline atlas 视觉效果 | `pack/images/atlases/relic_outline_atlas.sprites/doll_mask.tres` | `pack/images/atlases/relic_outline_atlas.sprites/doll_mask.tres` | 已接入但仍是占位 | 当前直接复用主 relic 图 |

### 2. 卡牌占位

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 占位来源 |
| --- | --- | --- | --- | --- |
| `StrikeTogawasakiko` | `assets/cards/basic/strike_togawasakiko.png` | `pack/mod_assets/cards/basic/strike_togawasakiko.png` | 已替换为正式图 | 2026-03-30 已同步到 `pack/` 并重新 build / install |
| `DefendTogawasakiko` | `assets/cards/basic/defend_togawasakiko.png` | `pack/mod_assets/cards/basic/defend_togawasakiko.png` | 已替换为正式图 | 同上 |
| `Slander` | `assets/cards/basic/slander.png` | `pack/mod_assets/cards/basic/slander.png` | 已替换为正式图 | 同上 |
| `Unendurable` | `assets/cards/basic/unendurable.png` | `pack/mod_assets/cards/basic/unendurable.png` | 已替换为正式图 | 同上 |
| `PersonaDissociation` | `assets/cards/generated_pressure/persona_dissociation.png` | `pack/mod_assets/cards/generated_pressure/persona_dissociation.png` | 已替换为正式图 | 2026-04-01 已由中文来稿 `人格解离.png` 对齐替换 |
| `SocialWithdrawal` | `assets/cards/generated_pressure/social_withdrawal.png` | `pack/mod_assets/cards/generated_pressure/social_withdrawal.png` | 已替换为正式图 | 2026-04-01 已由中文来稿 `自闭.png` 对齐替换 |
| `AllYouThinkAboutIsYourself` | `assets/cards/generated_pressure/all_you_think_about_is_yourself.png` | `pack/mod_assets/cards/generated_pressure/all_you_think_about_is_yourself.png` | 已替换为正式图 | 2026-04-01 已由中文来稿 `满脑子想的都是你自己.png` 对齐替换 |
| `OverworkAnxiety` | `assets/cards/generated_pressure/overwork_anxiety.png` | `pack/mod_assets/cards/generated_pressure/overwork_anxiety.png` | 已替换为正式图 | 2026-04-01 已由中文来稿 `过劳焦虑.png` 对齐替换 |
| `BarkingBarkingBarking` | `assets/cards/relic_granted/barking_barking_barking.png` | `pack/mod_assets/cards/relic_granted/barking_barking_barking.png` | 已替换为正式图 | 2026-04-03 已由中文来稿 `大狗大狗叫叫叫.png` 对齐替换 |
| `PullmanCrash` | `assets/cards/relic_granted/pullman_crash.png` | `pack/mod_assets/cards/relic_granted/pullman_crash.png` | 已替换为正式图 | 2026-04-04 已由中文来稿 `普尔曼冲击.png` 对齐替换；当前尺寸 `1000x750` |
| `TreasurePleasure` | `assets/cards/normal/uncommon/treasure_pleasure.png` | `pack/mod_assets/cards/normal/uncommon/treasure_pleasure.png` | 已替换为正式图 | 2026-04-06 已由来稿 `Treasure Pleasure.png` 对齐替换；当前尺寸 `1000x753` |
| `GeorgetteMeGeorgetteYou` | `assets/cards/normal/uncommon/georgette_me_georgette_you.png` | `pack/mod_assets/cards/normal/uncommon/georgette_me_georgette_you.png` | 已替换为正式图 | 2026-04-19 已由来稿 `Georgette Me, Georgette You.png` 对齐安装；当前尺寸 `900x900` |
| `Angles` | `assets/cards/normal/uncommon/angles.png` | `pack/mod_assets/cards/normal/uncommon/angles.png` | 已替换为正式图 | 2026-04-21 已由来稿 `Angles.png` 对齐安装；当前尺寸 `900x900` |
| `PerkUp` | `assets/cards/normal/common/perk_up.png` | `pack/mod_assets/cards/normal/common/perk_up.png` | 已替换为正式图 | 2026-04-13 已由中文来稿 `抖擞精神.png` 对齐替换；当前尺寸 `1000x758` |
| `Speak` | `assets/cards/normal/common/speak.png` | `pack/mod_assets/cards/normal/common/speak.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `说话！.png` 对齐替换；当前尺寸 `1000x696` |
| `RestorationOfPower` | `assets/cards/normal/common/restoration_of_power.png` | `pack/mod_assets/cards/normal/common/restoration_of_power.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `复权.png` 对齐替换；当前尺寸 `1000x754` |
| `SymbolI` | `assets/cards/normal/common/symbol_i.png` | `pack/mod_assets/cards/normal/common/symbol_i.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol I.png` 对齐替换；当前尺寸 `900x900` |
| `SoManyMaggots` | `assets/cards/normal/common/so_many_maggots.png` | `pack/mod_assets/cards/normal/common/so_many_maggots.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `好多蛆.png` 对齐替换；当前尺寸 `1000x679` |
| `WeightliftingChampion` | `assets/cards/normal/common/weightlifting_champion.png` | `pack/mod_assets/cards/normal/common/weightlifting_champion.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `举重冠军.png` 对齐替换；当前尺寸 `1000x703` |
| `PutOnYourMask` | `assets/cards/normal/common/put_on_your_mask.png` | `pack/mod_assets/cards/normal/common/put_on_your_mask.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `面具戴好.png` 对齐替换；当前尺寸 `1000x672` |
| `SeverThePast` | `assets/cards/normal/common/sever_the_past.png` | `pack/mod_assets/cards/normal/common/sever_the_past.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `斩断过去.png` 对齐替换；当前尺寸 `1000x756` |
| `AnswerMe` | `assets/cards/normal/common/answer_me.png` | `pack/mod_assets/cards/normal/common/answer_me.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `做出回答.png` 对齐替换；当前尺寸 `1000x732` |
| `LeaveItToMe` | `assets/cards/normal/uncommon/leave_it_to_me.png` | `pack/mod_assets/cards/normal/uncommon/leave_it_to_me.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `交给我吧.png` 对齐替换；当前尺寸 `1000x733` |
| `Innocence` | `assets/cards/normal/uncommon/innocence.png` | `pack/mod_assets/cards/normal/uncommon/innocence.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `天真.png` 对齐替换；当前尺寸 `1000x721` |
| `DawnOfDespair` | `assets/cards/normal/uncommon/dawn_of_despair.png` | `pack/mod_assets/cards/normal/uncommon/dawn_of_despair.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `绝望伊始.png` 对齐替换；当前尺寸 `1000x710` |
| `BailMoney` | `assets/cards/normal/uncommon/bail_money.png` | `pack/mod_assets/cards/normal/uncommon/bail_money.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `保释金.png` 对齐替换；当前尺寸 `1000x705` |
| `Housewarming` | `assets/cards/normal/uncommon/housewarming.png` | `pack/mod_assets/cards/normal/uncommon/housewarming.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `乔迁.png` 对齐替换；当前尺寸 `1000x709` |
| `SymbolIi` | `assets/cards/normal/uncommon/symbol_ii.png` | `pack/mod_assets/cards/normal/uncommon/symbol_ii.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol II.png` 对齐替换；当前尺寸 `900x900` |
| `SymbolIii` | `assets/cards/normal/uncommon/symbol_iii.png` | `pack/mod_assets/cards/normal/uncommon/symbol_iii.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol III.png` 对齐替换；当前尺寸 `900x900` |
| `FinalCurtain` | `assets/cards/normal/rare/final_curtain.png` | `pack/mod_assets/cards/normal/rare/final_curtain.png` | 已替换为正式图 | 2026-04-19 已由中文来稿 `谢幕.png` 对齐替换；当前尺寸 `1000x711` |
| `AWonderfulWorldYetNowhereToBeFound` | `assets/cards/normal/rare/a_wonderful_world_yet_nowhere_to_be_found.png` | `pack/mod_assets/cards/normal/rare/a_wonderful_world_yet_nowhere_to_be_found.png` | 已替换为正式图 | 2026-04-21 已由来稿 `A Wonderful World, Yet Nowhere to Be Found.png` 对齐安装；当前尺寸 `900x900` |
| `SymbolIv` | `assets/cards/normal/rare/symbol_iv.png` | `pack/mod_assets/cards/normal/rare/symbol_iv.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol IV.png` 对齐替换；当前尺寸 `900x900` |

### 2.1 event-only 长期牌 / 普通事件资产状态

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 备注 |
| --- | --- | --- | --- | --- |
| `ShadowOfThePastI` | `assets/cards/event_granted/shadow_of_the_past_i.png` | `pack/mod_assets/cards/event_granted/shadow_of_the_past_i.png` | 已替换为正式图 | 当前尺寸 `1000x715`；2026-04-20 已补齐规范来稿区 `incoming_assets/cards/event_granted/往日之影1.png` |
| `ShadowOfThePastII` | `assets/cards/event_granted/shadow_of_the_past_ii.png` | `pack/mod_assets/cards/event_granted/shadow_of_the_past_ii.png` | 已替换为正式图 | 当前尺寸 `1000x740`；2026-04-20 已补齐规范来稿区 `incoming_assets/cards/event_granted/往日之影2.png` |
| `ShadowOfThePastIII` | `assets/cards/event_granted/shadow_of_the_past_iii.png` | `pack/mod_assets/cards/event_granted/shadow_of_the_past_iii.png` | 已替换为正式图 | 当前尺寸 `1000x735`；2026-04-20 已补齐规范来稿区 `incoming_assets/cards/event_granted/往日之影3.png` |
| `UpgradedDollMask` | `assets/relics/starter/upgraded_doll_mask.png` | `pack/images/relics/upgraded_doll_mask.png` | 已建独立正式文件位，当前仍复用旧图 | 2026-04-20 已建立稳定文件名并同步到 runtime；当前视觉内容仍与 `doll_mask.png` 相同，待正式来稿替换 |
| 普通问号房 `Shadow` 事件主图 | `assets/events/question_room/shadow_of_the_past/start.png` | `pack/images/events/unattended_piano.png` | 已接入运行时 | 2026-04-20 已由来稿 `incoming_assets/events/question_room/shadow_of_the_past/background/Start.png` 同步到 runtime；当前先按原版普通事件默认主图路径接线 |
| 普通问号房 `Shadow` 事件切图 | `assets/events/question_room/shadow_of_the_past/shadow_piano.png` | `pack/images/events/unattended_piano_shadow.png` | 已接入运行时 | 2026-04-20 已由来稿 `incoming_assets/events/question_room/shadow_of_the_past/background/Piano shadow.png` 同步到 runtime；当前用于事件内继续弹奏后的 portrait 切换 |
| 普通问号房 `Shadow` 事件音乐源文件 | `assets/audio/music/events/unattended_piano.mp3` | `pack/audio/music/events/unattended_piano.mp3` | 已接入运行时 | 2026-04-20 已由来稿 `incoming_assets/audio/music/events/UnattendedPiano-La Fille Aux Checeux De Lin.m4a` 转存为稳定文件名；当前时长约 `101.89s` |
| 普通问号房 `Shadow` 事件音乐播放路径 | `assets/audio/music/tracks/unattended_piano.mp3` | `pack/audio/music/tracks/unattended_piano.mp3` | 已接入运行时 | 2026-04-20 为兼容 `PlayCustomMusic` 补进 `tracks/` 层；当前事件代码优先播放这一层，缺失时才回退 `events/` 路径 |

### 3. 当前已实现正常卡池牌资源状态

当前源码已实现并实际接线使用的 `normal` 池卡图，已经全部切换到各自独立文件，不再复用 `basic` 占位图。

当前已核对存在于 `assets/` 与 `pack/` 的 `normal` 卡图共 `43` 张：

- `Common 14`
  - `answer_me`
  - `face`
  - `god_you_fool`
  - `perk_up`
  - `put_on_your_mask`
  - `restoration_of_power`
  - `s_the_way`
  - `saki_move_plz`
  - `sever_the_past`
  - `she_is_radiant`
  - `so_many_maggots`
  - `speak`
  - `symbol_i`
  - `weightlifting_champion`
- `Uncommon 22`
  - `angles`
  - `bail_money`
  - `black_birthday`
  - `completeness`
  - `compose`
  - `crucifix_x`
  - `dawn_of_despair`
  - `ether`
  - `georgette_me_georgette_you`
  - `housewarming`
  - `imprisoned_xii`
  - `innocence`
  - `leave_it_to_me`
  - `masquerade_rhapsody_request`
  - `music_of_the_celestial_sphere`
  - `notebook`
  - `sophie`
  - `symbol_ii`
  - `symbol_iii`
  - `treasure_pleasure`
  - `thrilled`
  - `two_moons_deep_into_the_forest`
- `Rare 7`
  - `a_wonderful_world_yet_nowhere_to_be_found`
  - `ave_mujica`
  - `choir_s_choir`
  - `final_curtain`
  - `i_have_ascended`
  - `killkiss`
  - `symbol_iv`

说明：

- 以上 `37` 张文件均已存在于 `assets/cards/normal/...` 与 `pack/mod_assets/cards/normal/...`
- 当前“normal 池仍复用 basic 占位图”的旧文档口径已失效

### 4. Power / Debuff icon 占位

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 占位来源 |
| --- | --- | --- | --- | --- |
| `PRESSURE_POWER` | `assets/icons/powers/pressure_power.png` | `pack/images/powers/pressure_power.png` | 已替换为正式图 | 来自 `incoming_assets/powers/压力.png` |
| `PERSONA_DISSOCIATION_POWER` | `assets/icons/powers/persona_dissociation_power.png` | `pack/images/powers/persona_dissociation_power.png` | 已替换为正式图 | 来自 `incoming_assets/powers/人格解离.png` |
| `SOCIAL_WITHDRAWAL_POWER` | `assets/icons/powers/social_withdrawal_power.png` | `pack/images/powers/social_withdrawal_power.png` | 已替换为正式图 | 来自 `incoming_assets/powers/自闭.png` |
| `INFERIORITY_POWER` | `assets/icons/powers/inferiority_power.png` | `pack/images/powers/inferiority_power.png` | 已替换为正式图 | 来自 `incoming_assets/powers/过劳焦虑.png`；按当前冻结 debuff 槽位落到 `Inferiority` |
| `AVE_MUJICA_POWER` | `assets/icons/powers/ave_mujica_power.png` | `pack/images/powers/ave_mujica_power.png` | 已替换为正式图 | 2026-04-06 已由来稿 `Ave Mujica.png` 对齐替换；当前尺寸 `256x256` |
| `FACE_REACTION_POWER` | `assets/icons/powers/face_reaction_power.png` | `pack/images/powers/face_reaction_power.png` | 已替换为正式图 | 2026-04-06 已由来稿 `颜.png` 对齐替换；当前尺寸 `256x256` |
| `KILL_KISS_POWER` | `assets/icons/powers/killkiss_power.png` | `pack/images/powers/killkiss_power.png` | 已替换为正式图 | 2026-04-06 已由来稿 `KillKiss.png` 对齐替换；当前尺寸 `256x256`。同时补了 `kill_kiss_power.png` 兼容落位，避免运行时按 `KILL_KISS_POWER` 查找时继续吃占位。 |
| `KILL_KISS_PLUS_POWER` | `assets/icons/powers/kill_kiss_plus_power.png` | `pack/images/powers/kill_kiss_plus_power.png` | 已替换为正式图 | 2026-04-06 已由来稿 `KillKiss.png` 对齐替换；与普通版共用同一素材，当前尺寸 `256x256`。 |
| `MAGNETIC_FORCE_HELL_WARGOD_POWER` | `assets/icons/powers/magnetic_force_hell_wargod_power.png` | `pack/images/powers/magnetic_force_hell_wargod_power.png` | 已替换为正式图 | 2026-04-06 已由来稿 `磁场力量.png` 对齐替换，并按新稳定名复制接入；当前尺寸 `256x256` |
| `MIRROR_FLOWER_WATER_MOON_POWER` | `assets/icons/powers/mirror_flower_water_moon_power.png` | `pack/images/powers/mirror_flower_water_moon_power.png` | 已替换为正式图 | 2026-04-21 已由来稿 `镜花水月.png` 缩放对齐替换；当前尺寸 `256x256` |
| `INNOCENCE_POWER` | `assets/icons/powers/innocence_power.png` | `pack/images/powers/innocence_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `天真.png` 缩放对齐替换；当前尺寸 `256x256` |
| `SAKIKO_DESPAIR_ECHO_POWER` | `assets/icons/powers/sakiko_despair_echo_power.png` | `pack/images/powers/sakiko_despair_echo_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `绝望回响.png` 缩放对齐替换；当前尺寸 `256x256` |
| `SYMBOL_I_POWER` | `assets/icons/powers/symbol_i_power.png` | `pack/images/powers/symbol_i_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol I.png` 缩放对齐替换；当前尺寸 `256x256` |
| `SYMBOL_II_POWER` | `assets/icons/powers/symbol_ii_power.png` | `pack/images/powers/symbol_ii_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol II.png` 缩放对齐替换；当前尺寸 `256x256` |
| `SYMBOL_III_POWER` | `assets/icons/powers/symbol_iii_power.png` | `pack/images/powers/symbol_iii_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol III.png` 缩放对齐替换；当前尺寸 `256x256` |
| `SYMBOL_IV_POWER` | `assets/icons/powers/symbol_iv_power.png` | `pack/images/powers/symbol_iv_power.png` | 已替换为正式图 | 2026-04-20 已由来稿 `Symbol IV.png` 缩放对齐替换；当前尺寸 `256x256` |

补充说明：

- 旧的 runtime 文件名 `next_attack_replay_power.png` 已从工作区运行时路径与游戏安装目录清除，不再参与打包或加载。
- 原始来稿与改名前占位文件仍保留在 `incoming_assets/_originals/` 下，作为资源线程留档，不参与运行时。
- 2026-03-28 已把上传到 `incoming_assets/powers/` 的 4 张原创状态图正式替换进源资源与 runtime 资源。
- 第 4 张上传原稿名为 `过劳焦虑.png`，但当前代码里没有 `OverworkAnxietyPower`，只有 `InferiorityPower` 这一冻结 debuff 槽位，因此本轮按既有实现主键落位到 `inferiority_power.png`。
- 如果后续设计确认 `Inferiority` 与【过劳焦虑】需要拆成两个不同状态对象，则必须同步补一次对象命名和资源重映射。

## 四 当前未进入 runtime 的对象

这些对象当前要么只有库存 / 来稿，要么虽然文件存在，但并不是当前运行时实际使用路径：

| 对象 | 原规划路径 | 当前状态 | 说明 |
| --- | --- | --- | --- |
| merchant scene 文件 | `pack/scenes/merchant/characters/togawasakiko_merchant.tscn` | 文件存在，且当前已作为运行时 merchant path 使用 | 当前由 `CharacterModel.MerchantAnimPath` patch 显式返回该 scene |
| 先古之民事件主图来稿 | `incoming_assets/ancients/event_main/丰川定治.png` | 已复制进入 runtime，原来稿仍保留 | 尺寸 `2560x1244`；当前已整理进 `assets/ancients/event_main/togawa_teiji.png`，并同步进入 `pack/images/events/togawa_teiji.png` |
| 先古之民地图节点原型 | `assets/ancients/map_nodes/ancient_map_node_prototype.png` | 已入库存，且当前已复制出 runtime 占位版本 | 当前仍是原型资源来源 |
| 先古之民地图节点 outline 原型 | `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png` | 已入库存，且当前已复制出 runtime 占位版本 | 同上 |
| 丰川定治地图节点 runtime 占位 | `assets/ancients/map_nodes/togawa_teiji_map_node.png` | `pack/images/packed/ancients/map_nodes/togawa_teiji_map_node.png` | 已接入但仍是占位 | 当前复制 `prototype` 图用于 ancient runtime |
| 丰川定治地图节点 outline runtime 占位 | `assets/ancients/map_nodes/togawa_teiji_map_node_outline.png` | `pack/images/packed/ancients/map_nodes/togawa_teiji_map_node_outline.png` | 已接入但仍是占位 | 当前复制 `prototype outline` 图用于 ancient runtime |
| 丰川定治对话头像 | `assets/ancients/dialogue_icons/togawa_teiji.png` | 已入正式库，runtime 已同步 | 当前与 `incoming_assets/ancients/dialogue_icon/丰川定治.png` 一致；runtime 位于 `pack/images/ui/run_history/togawa_teiji.png` |
| 丰川定治对话头像 outline | `assets/ancients/dialogue_icons/togawa_teiji_outline.png` | 已入正式库，runtime 已同步 | 当前与 `incoming_assets/ancients/dialogue_icon_outline/丰川定治.png` 一致；runtime 位于 `pack/images/ui/run_history/togawa_teiji_outline.png` |

说明：

- 它们不属于当前角色最小战斗闭环的必需项
- 若后续要推进丰川定治事件接入，需先冻结先古之民英文内部名，再决定是否把 `prototype` 库存重命名并接入 runtime
- 2026-04-03 追加核对：
  - 对于“只进 Act 3 池、能正常弹 ancient 选项并发 relic / gold / 送牌”的当前最小实现，所需 runtime 资产已齐
  - 当前新增 runtime 资源为：
    - `pack/images/events/togawa_teiji.png`
    - `pack/scenes/events/background_scenes/togawa_teiji.tscn`
    - `pack/images/ui/run_history/togawa_teiji.png`
    - `pack/images/ui/run_history/togawa_teiji_outline.png`
  - 当前仍未完成的是地图节点从 prototype 来源彻底转正，以及实机验证，不应写成“丰川定治全部资产已最终完成”
- 2026-04-04 实机异常补充：
  - 当前已确认“地图可显示、点进去卡住”不是 ancient 地图节点缺图，而是事件房间打开阶段异常。
  - `godot.log` 真实堆栈为 `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom.SetupLayout()` 空引用。
  - `TogawaTeiji` 事件模型的 `InitialPortraitPath` 会按原版口径解析到 `res://images/events/togawa_teiji.png`。
  - 此前该 runtime 文件缺失，已补入：
    - `pack/images/events/togawa_teiji.png`
  - 需要区分：
    - `pack/images/events/togawa_teiji.png` 是事件模型实际读取的初始立绘
    - `pack/scenes/events/background_scenes/togawa_teiji.tscn` 当前内部仍引用 `pack/images/ancients/togawa_teiji_placeholder.png` 作为背景 scene 贴图
  - 因此当前状态不应再写成“事件主图仅存在于 placeholder 稳定路径”，而应写成“初始立绘已转正进 runtime，背景 scene 贴图仍是 placeholder 命名”

## 四点五 音频 / Jukebox 当前状态

当前与局内 `jukebox` 直接相关的 runtime 资源状态如下：

| 对象 | 源资源路径 | runtime 路径 | 当前状态 | 备注 |
| --- | --- | --- | --- | --- |
| 角色选人确认音效 | `assets/audio/sfx/character_select/togawasakiko_select.ogg` | `pack/audio/sfx/character_select/togawasakiko_select.ogg` | 已接入且文件存在 | 已由选人界面本地播放 patch 使用 |
| 点歌完整曲库目录 | `assets/audio/music/tracks/` | `pack/audio/music/tracks/` | 已入正式曲目 `23` 首 | `jukebox` UI 直接扫描这一层；2026-04-21 新增 `symbol_i / symbol_ii / symbol_iii / symbol_iv / a_wonderful_world_yet_nowhere_to_be_found / angles` |
| 点歌短 cue 目录 | `assets/audio/music/song_cues/` | `pack/audio/music/song_cues/` | 已建目录，当前无正式文件 | 当前 `jukebox` 未直接读取这一层 |
| 点歌系统 UI 音效目录 | `assets/audio/sfx/jukebox/` | `pack/audio/sfx/jukebox/` | 已建目录，当前无正式文件 | `jukebox_open / confirm / next_track / stop` 仍待来稿 |

说明：

- 2026-04-05 已新增局内 `jukebox` UI 骨架，但它不会读取 `incoming_assets/` 或文档登记项。
- 当前只有真正落进 `pack/audio/music/tracks/<track_id>.(ogg|wav|mp3)` 的 runtime 文件，才会显示成可选 BGM 项。
- 当前 `pack/audio/music/tracks/` 已有 `23` 首 runtime 曲目，`jukebox` 不应再只显示 `Off (null)`。

## 五 对 T3 的当前交接口径

当前 T3 后续最应该直接补交或整理的，仍然是这些资源：

1. 顶栏 outline 正式图
2. 新增 `3` 个事件型 power 的正式 icon
3. 若后续决定保留锁定态展示，再补正式锁定图
4. 丰川定治事件主图从 `incoming_assets/` 规范化进入正式库存
5. 丰川定治地图节点 / 对话头像在英文内部名冻结后，从 `prototype` 库存转正命名

当前补图原则：

- 不改现有文件路径
- 只替换同名文件
- `OverworkAnxiety` 若后续统一改英文稳定主键，需要同步改：
  - 代码类名
  - 本地化 key
  - 源资源文件名
  - runtime 文件名
  - 文档登记

## 六 本轮 bug 修复对资源层的影响

- 本轮不仅有规则修复，也补了运行时缺失的角色资源。
- 2026-03-28 追加确认：
  - `merchant / rest site / energy counter` 这 3 条路径并不会因为写进 `ExtraAssetPaths` 就自动被原版 UI 采用
  - 代码侧已改为通过 `CharacterModel` getter patch 显式返回：
    - `MerchantAnimPath -> res://scenes/merchant/characters/togawasakiko_merchant.tscn`
    - `RestSiteAnimPath -> res://scenes/rest_site/characters/togawasakiko_rest_site.tscn`
    - `EnergyCounterPath -> res://scenes/combat/energy_counters/togawasakiko_energy_counter.tscn`
  - 因此当前这 3 个资源对象都已有明确运行时路径绑定
- 2026-04-03 再次核对确认：
  - `pack/scenes/merchant/characters/togawasakiko_merchant.tscn` 文件仍随包保留
  - 当前源码里的 `MerchantAnimPath` 已显式返回该自定义 scene
  - 因此当前 merchant scene 应视为“文件存在且已被运行时启用”
- 火堆这边则不再继续使用 `silent` 可见占位：
  - 已新增 `pack/images/packed/character/rest_site/rest_site_character_togawasakiko.png`
  - `togawasakiko_rest_site.tscn` 当前显示的是 Sakiko 上传的正式静态火堆图
  - 2026-03-29 已由 `incoming_assets/rest_site_character/` 更新为 `3048x3048` 版本，并同步替换到 `assets/character/rest_site/` 与 runtime 路径
- 新增进入最终 `pack/` 的文件有：
  - `pack/scenes/rest_site/characters/togawasakiko_rest_site.tscn`
  - `pack/scenes/merchant/characters/togawasakiko_merchant.tscn`
  - `pack/images/ui/hands/multiplayer_hand_togawasakiko_point.png`
  - `pack/images/ui/hands/multiplayer_hand_togawasakiko_rock.png`
  - `pack/images/ui/hands/multiplayer_hand_togawasakiko_paper.png`
  - `pack/images/ui/hands/multiplayer_hand_togawasakiko_scissors.png`
- 压力衍生牌的代码类名已对齐冻结 `Entry`，但对应卡图资源文件名仍保持原路径：
  - `persona_dissociation.png`
  - `social_withdrawal.png`
  - `all_you_think_about_is_yourself.png`
  - `overwork_anxiety.png`
- 2026-04-03 资源实查补充：
  - `best_companion.png` 与 `barking_barking_barking.png` 已完成 `assets -> pack` 同步
  - `best_companion` 现在已补 `relic_atlas / relic_outline_atlas` 的 `AtlasTexture` 包装：
    - `pack/images/atlases/relic_atlas.sprites/best_companion.tres`
    - `pack/images/atlases/relic_outline_atlas.sprites/best_companion.tres`
- 先古之民地图节点当前已从 `prototype` 复制出 `TogawaTeiji` runtime 占位版本并进入 `pack/`
- 先古之民对话头像当前仍只在 `assets/ancients/` 保留为原型库存，尚未进入 `pack/`
- 因此当前不需要 T3 重新交图，但需要后续实机重点复测：
  - 火堆与商店是否不再报缺场景
  - 角色手势相关交互是否不再引用空路径
  - `4` 张压力衍生牌在生成与 console 调用时，卡图是否仍正常显示
  - `人格解离 / 自闭` token 卡新增 hover tip 后，图标与说明是否正常弹出
  - `CrucifixX / SakiMovePlz / Face / Ether` 修规则后，是否没有引入新的资源引用断点
