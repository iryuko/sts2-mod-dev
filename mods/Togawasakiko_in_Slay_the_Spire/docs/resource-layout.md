# 资源目录说明

日期：2026-04-04

## 一 目标

本文件用于把当前角色项目的资源目录固定到一个可长期迭代的结构上。

原则：

- 工作区里的源资源目录尽量贴近原版命名风格
- 但不把未来运行时 scene、atlas 打包细节提前硬写死进当前资产目录
- 目录先围绕“资源来源与维护责任”划分，再映射到原版风格的最终文件名

## 二 当前目录结构

```text
assets/
  ancients/
    dialogue_icons/
      previews/
    map_nodes/
      previews/
  audio/
    music/
      tracks/
      song_cues/
    sfx/
      character_select/
      jukebox/
      ui/
  cards/
    basic/
    generated_pressure/
    relic_granted/
    normal/
      common/
      uncommon/
      rare/
    placeholders/
  character/
    char_select/
    char_select_bg/
    energy/
    multiplayer_hand/
    top_panel/
  animations/
    characters/
      togawasakiko/
  icons/
    powers/
    tags/
    ui/
  relics/
    ancient/
    future/
    starter/
```

## 三 各目录职责

### 1. `assets/character/`

- `char_select/`
  - 角色选择主立绘与锁定图
- `char_select_bg/`
  - 选人背景源文件、分层稿、动画素材位
- `top_panel/`
  - 面板头像与轮廓图
- `energy/`
  - 卡牌费用 icon 源图与战斗能量球图层
- `multiplayer_hand/`
  - 联机手势图

### 1.2 `assets/animations/`

- `characters/togawasakiko/`
  - `Spine` 战斗立绘运行时资产正式库存位
  - 仅放已经整理好的 runtime 文件：
    - `togawasakiko.atlas`
    - `togawasakiko.skel`
    - `togawasakiko_skel_data.tres`
    - atlas page 导出的 `PNG`
  - 不放原始编辑工程，不放临时拆图工作稿

### 1.5 `assets/ancients/`

- `dialogue_icons/`
  - 原创先古之民事件对话头像与 outline 的正式库存位
  - 当前用于参考资产和原型验证，不等于已经接入 runtime
- `dialogue_icons/previews/`
  - 按原版 `ancient_dialogue_line.tscn` 叠放逻辑烘焙出的预览图
  - 用于看最终观感，不作为源图
- `map_nodes/`
  - 原创先古之民地图节点主图与 outline 的正式库存位
  - 当前用于参考资产和原型验证，不等于已经接入 runtime
- `map_nodes/previews/`
  - 按原版 `ancient_map_point.tscn` 着色逻辑烘焙出的预览图
  - 用于看最终观感，不作为源图

### 2. `assets/cards/`

- `basic/`
  - `StrikeTogawasakiko`
  - `DefendTogawasakiko`
  - `Slander`
  - `Unendurable`
- `relic_granted/`
  - 由 relic 直接加入牌组、且不属于正常奖励池的卡图库存位
  - 当前用于承接“先古之民 relic 自带塞入牌组的卡”
  - 这类卡不应混入 `normal/common|uncommon|rare/`
  - 也不应混入 `generated_pressure/`
- `normal/common/`
  - 正常卡池普通品质
- `normal/uncommon/`
  - 正常卡池非凡品质
- `normal/rare/`
  - 正常卡池稀有品质
- `generated_pressure/`
  - 当前 `4` 张压力衍生牌
- `placeholders/`
  - 可复用占位模板

### 2.5 `assets/audio/`

- `music/tracks/`
  - 可被完整播放的正式音乐文件
  - 歌曲牌与局内点歌系统优先共用这组文件
- `music/song_cues/`
  - 歌曲牌专用短版、切片或战斗内 cue
  - 只有完整曲目不适合直接播放时才单开
- `music/events/`
  - 普通事件或特殊事件专用背景音乐
  - 当前用于承接 `UnattendedPiano` 这类不应混入歌曲牌或 `jukebox` 曲库的事件音频
- `sfx/character_select/`
  - 角色选择界面的独特音效
- `sfx/jukebox/`
  - 点歌系统自身的打开、确认、切歌、停止等操作音效
- `sfx/ui/`
  - 不属于点歌系统、但与角色局内功能有关的通用短音效

### 3. `assets/icons/`

- `powers/`
  - 压力与原创 debuff icon
- `tags/`
  - 未来如需可视化时使用的标签 icon 预留位
- `ui/`
  - 后续角色专属 UI 小图

### 4. `assets/relics/`

- `starter/`
  - 当前 starter relic 图
- `ancient/`
  - 原创先古之民事件给予的 relic 图标正式库存位
  - 当前用于承接“丰川定治事件”这类专属先古之民 relic
- `future/`
  - 后续专属 relic 图

## 四 与原版风格的映射关系

当前项目先维护“源资源路径”，后续运行时资源名尽量映射到原版样式。

| 工作区源路径 | 目标运行时风格 | 说明 |
| --- | --- | --- |
| `assets/character/char_select/char_select_togawasakiko.png` | `char_select_togawasakiko.png` | 对齐原版选人图模式 |
| `assets/character/char_select_bg/char_select_bg_togawasakiko.png` | `char_select_bg_togawasakiko` | 当前项目采用静态终稿路线，对应选角背景大图 |
| `assets/character/char_select/char_select_togawasakiko_locked.png` | `char_select_togawasakiko_locked.png` | 对齐原版锁定图 |
| `assets/character/in_combat_portrait/in_combat_portrait_togawasakiko.png` | `in_combat_portrait_togawasakiko.png` | 当前项目的静态战斗立绘正式图 |
| `assets/animations/characters/togawasakiko/togawasakiko.atlas` | `animations/characters/togawasakiko/togawasakiko.atlas` | `Spine` atlas 正式运行时文件 |
| `assets/animations/characters/togawasakiko/togawasakiko.skel` | `animations/characters/togawasakiko/togawasakiko.skel` | `Spine` skeleton 二进制运行时文件 |
| `assets/animations/characters/togawasakiko/togawasakiko_skel_data.tres` | `animations/characters/togawasakiko/togawasakiko_skel_data.tres` | Godot `SpineSkeletonDataResource` 包装 |
| `assets/character/rest_site/rest_site_character_togawasakiko.png` | `rest_site_character_togawasakiko.png` | 当前项目的火堆房角色坐姿正式图 |
| `assets/character/top_panel/character_icon_togawasakiko.png` | `character_icon_togawasakiko.png` | 对齐原版顶栏头像 |
| `assets/character/top_panel/character_icon_togawasakiko_outline.png` | `character_icon_togawasakiko_outline.png` | 对齐原版顶栏轮廓 |
| `assets/character/energy/energy_togawasakiko.png` | `energy_togawasakiko` | 对齐原版卡牌费用图标命名 |
| `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype.png` | `dialogue_icon_<ancient>.png` | 未冻结先古之民内部名时，用 `prototype` 作为正式库存占位 |
| `assets/ancients/dialogue_icons/ancient_dialogue_icon_prototype_outline.png` | `dialogue_icon_<ancient>_outline.png` | 与对话头像配套的 outline 库存位 |
| `assets/ancients/map_nodes/ancient_map_node_prototype.png` | `ancient_map_node_<ancient>.png` | 未冻结先古之民内部名时，用 `prototype` 作为正式库存占位 |
| `assets/ancients/map_nodes/ancient_map_node_prototype_outline.png` | `ancient_map_node_<ancient>_outline.png` | 与主节点图配套的 outline 库存位 |
| `assets/cards/basic/strike_togawasakiko.png` | `strike_togawasakiko.png` | 对齐 `entry-lower` |
| `assets/cards/relic_granted/<card>.png` | `<card>.png` | relic 直接塞入牌组的卡图，不混入正常卡池目录 |
| `assets/audio/music/tracks/<track_id>.ogg` | `audio/music/tracks/<track_id>.ogg` | 正式完整曲目库存位 |
| `assets/audio/music/song_cues/<cue_id>.ogg` | `audio/music/song_cues/<cue_id>.ogg` | 歌曲牌专用短 cue 库存位 |
| `assets/audio/music/events/<event_id>.mp3` | `audio/music/events/<event_id>.mp3` | 事件专用背景音乐库存位 |
| `assets/audio/sfx/character_select/<cue_id>.wav` | `audio/sfx/character_select/<cue_id>.wav` | 角色选人音效库存位 |
| `assets/audio/sfx/jukebox/<cue_id>.wav` | `audio/sfx/jukebox/<cue_id>.wav` | 点歌系统 UI 音效库存位 |
| `assets/icons/powers/pressure_power.png` | `pressure_power.png` | 对齐 power icon 命名 |
| `assets/relics/starter/doll_mask.png` | `doll_mask.png` | 对齐 relic icon 命名 |
| `assets/relics/ancient/<relic>.png` | `<relic>.png` | 先古之民事件 relic 图标库存位 |

## 五 song 子集在目录层的处理

song 牌当前不单开：

- `assets/cards/song/`

原因：

- 它们属于正常卡池，不是额外生成牌
- 单独目录会把“标签子集”误写成“另一套卡池”

当前正确做法：

- 卡图仍按 rarity 落到：
  - `assets/cards/normal/common/`
  - `assets/cards/normal/uncommon/`
  - `assets/cards/normal/rare/`
- 是否带 `song tag`，在 `song-subset-registry.md` 里登记
- 若后续真的需要 UI 可视化，再单独增设共用 tag icon 或 overlay

## 六 占位资产的放法

占位资产分两层：

- 通用模板：
  - 放在 `assets/cards/placeholders/`
- 实际用于某张牌的占位图：
  - 仍应输出成该牌自己的正式文件名

例如：

- 可以先有：
  - `assets/cards/placeholders/placeholder_normal.png`
- 但给 `Slander` 用时，落位仍应是：
  - `assets/cards/basic/slander.png`

这样做的原因：

- 不让“占位阶段”破坏最终资源命名
- 后续替换正式图时不需要重改文件名或引用关系

## 七 来稿提交区

为避免“原始来稿”和最终 `assets/` 混淆，当前新增单独投递区：

- `incoming_assets/cards/basic/`
- `incoming_assets/cards/event_granted/`
- `incoming_assets/cards/relic_granted/`
- `incoming_assets/cards/normal_pool/`
- `incoming_assets/cards/pressure_generated/`
- `incoming_assets/ancients/event_main/`
- `incoming_assets/ancients/map_node/`
- `incoming_assets/ancients/map_node_outline/`
- `incoming_assets/ancients/dialogue_icon/`
- `incoming_assets/ancients/dialogue_icon_outline/`
- `incoming_assets/character_select_splash/`
- `incoming_assets/character_select_icon/`
- `incoming_assets/character_in_combat_portrait/`
- `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/`
- `incoming_assets/character_in_combat_spine/togawasakiko/spine_export/`
- `incoming_assets/character_in_combat_spine/togawasakiko/previews/`
- `incoming_assets/character_merchant_portrait/`
- `incoming_assets/rest_site_character/`
- `incoming_assets/multiplayer_hand/`
- `incoming_assets/top_panel_portrait/`
- `incoming_assets/energy/card_icon/`
- `incoming_assets/energy/combat_orb_layers/`
- `incoming_assets/powers/`
- `incoming_assets/relics/starter/`
- `incoming_assets/relics/ancient/`
- `incoming_assets/relics/future/`
- `incoming_assets/audio/music/full_tracks/`
- `incoming_assets/audio/music/song_cues/`
- `incoming_assets/audio/music/events/`
- `incoming_assets/audio/sfx/character_select/`
- `incoming_assets/audio/sfx/jukebox/`
- `incoming_assets/audio/sfx/ui/`
- `incoming_assets/events/question_room/shadow_of_the_past/event_main/`
- `incoming_assets/events/question_room/shadow_of_the_past/background/`
- `incoming_assets/events/question_room/shadow_of_the_past/vfx_refs/`

当前原则：

- 原始来稿先进入 `incoming_assets/`
- 规范化文件再进入 `assets/`
- 不要求来稿阶段就强行对齐最终文件名
- 工作区根下的 `images/先古之民/` 仅保留原版参考导出，不作为项目来稿提交区

补充规则：

- 普通问号房事件直接送入牌组的卡：
  - 原始来稿先进 `incoming_assets/cards/event_granted/`
  - 规范化后进入 `assets/cards/event_granted/`
- 普通问号房 `Shadow` 事件主图与背景来稿：
  - 原始来稿先进 `incoming_assets/events/question_room/shadow_of_the_past/`
  - 正式主图建议进入 `assets/events/question_room/shadow_of_the_past/`
  - 若后续需要独立背景 scene，再把底图 / 前景层整理进 `assets/events/question_room/shadow_of_the_past/background/`
- 先古之民事件送出的 relic：
  - 原始来稿先进 `incoming_assets/relics/ancient/`
  - 规范化后进入 `assets/relics/ancient/`
- relic 直接附带加入牌组的卡：
  - 原始来稿先进 `incoming_assets/cards/relic_granted/`
  - 规范化后进入 `assets/cards/relic_granted/`

这样分层的原因：

- relic 图标和 relic 所塞卡牌虽然强关联，但运行时接线对象不同
- relic 本体属于 relic 库
- 被塞入牌组的对象属于 card 库
- 这类卡不经过正常战斗奖励池，因此不应伪装成 `normal/common|uncommon|rare`

音频来稿补充规则：

- `incoming_assets/audio/music/full_tracks/`
  - 用于完整曲目来稿
  - 后续规范化进入 `assets/audio/music/tracks/`
- `incoming_assets/audio/music/song_cues/`
  - 用于歌曲牌短版、切片或专用 cue 来稿
  - 后续规范化进入 `assets/audio/music/song_cues/`
- `incoming_assets/audio/music/events/`
  - 用于普通事件或特殊事件专用背景音乐来稿
  - 后续规范化进入 `assets/audio/music/events/`
- `incoming_assets/audio/sfx/character_select/`
  - 用于角色选择界面音效来稿
- `incoming_assets/audio/sfx/jukebox/`
  - 用于点歌系统自身的操作音效来稿
- `incoming_assets/audio/sfx/ui/`
  - 用于局内功能相关的其它通用短音效来稿

音频库存与 runtime 分层规则：

- 正式库存维护在 `assets/audio/`
- runtime staging 维护在 `pack/audio/`
- 歌曲牌播放与点歌系统优先复用同一份 `music/tracks/` 正式曲目，不额外复制两份相同音频

先古之民来稿补充规则：

- `incoming_assets/ancients/event_main/`
  - 用于先古之民事件主立绘
  - 建议按原版静态型规格提交 `2560x1200 PNG`
- `incoming_assets/ancients/map_node/`
  - 用于地图节点头像
  - 建议提交 `208x208 PNG`
- `incoming_assets/ancients/map_node_outline/`
  - 用于地图节点头像 outline
  - 建议提交 `208x208 PNG`
- `incoming_assets/ancients/dialogue_icon/`
  - 用于事件对话头像
  - 建议提交 `88x88 PNG`
- `incoming_assets/ancients/dialogue_icon_outline/`
  - 用于事件对话头像 outline
  - 建议提交 `88x88 PNG`

## 八 当前结论

- 当前目录结构已经能承接：
  - 角色固有 UI 资源
  - 游戏内静态立绘
  - 火堆房角色坐姿图
  - 先古之民事件 relic 图标
  - relic 直接附带入组的专用卡图
  - `54` 张非衍生角色卡图
  - `4` 张压力衍生牌卡图
  - 压力 / debuff icon
  - 歌曲牌、角色选人音效与点歌系统共用的音频资产库
  - starter relic 与未来扩展 relic
- 这套结构适合长期迭代，因为它把“原始来稿”“角色固有资源”“正常卡池”“生成牌”“relic”清楚分开了。
