# 资源规格与素材提交流程

日期：2026-04-04

## 一 本文件定位

本文件回答两类问题：

- 这几个关键资源在原版里是怎么被使用的
- 后续素材最合理应该怎么交给项目

本轮只处理：

- 角色选择主立绘
- 角色游戏内静态立绘
- 卡图
- 顶栏头像
- 角色能量资源
- buff / debuff icon
- 起始 relic 图
- 歌曲牌 / 点歌系统音乐文件
- 角色选择与点歌系统短音效
- `Spine` 战斗立绘运行时资产

## 二 song 定义修正

`song` 当前应视为逻辑标签：

- 它是卡牌子集标签
- 它不等于一张图片资源
- 当前不要求必须提供专属 icon

当前最少支持只包括：

- `docs/song-subset-registry.md` 中的登记位
- 资产总表中的标记字段
- 与正常卡池对齐的命名规则

若未来确实要做 UI 可视化，再单独补：

- 共用 tag icon
- 共用角标 overlay

## 三 关键资源规格

| 资源 | 资源用途 | 当前原版使用方式 | atlas / scene / 分层依赖 | 静态图可直接提交 | 推荐提交格式 | 推荐提交尺寸 | 备注 / 风险点 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 角色选择主立绘 | 新 run 角色选择界面的背景大图 | 原版入口是 `char_select_bg_<character>.tscn`，最终观感来自场景运行时渲染，不是普通单图槽位 | 原版强依赖 scene；多角色还依赖 Spine / atlas page；当前已确认原版最终渲染成品是 `2560x1200` | 可以；当前 `Togawasakiko` 项目已明确采用静态终稿路线 | `PNG` | `2560x1200` | 当前本角色不追原版动态效果，正式静态成品已落位到 `assets/character/char_select_bg/char_select_bg_togawasakiko.png` |
| 角色游戏内静态立绘 | 战斗内角色主视觉的静态成图 | 原版入口更接近 `creature_visuals/<character>.tscn` 的运行时组合结果，不是单张现成 PNG | 原版强依赖 `SpineSprite`、skeleton 与附加层；当前项目若采用静态方案，则可直接交成品图 | 可以 | `PNG` | 推荐正式图 `1200x1200` | 当前项目把它视为静态战斗立绘正式资源，后续按静态显示方案接入 |
| 卡图 | 卡牌 portrait 主图 | 原版卡牌正文、费用、类型条、边框都由系统 UI 组合；卡图本体是单独 portrait 图 | 依赖公用卡框 / banner / portrait border，但单张卡图本身不需要角色单独做 scene | 可以 | `PNG` | 推荐正式图 `1000x760`；若后续要补 beta 图则用 `668x508` | 不要把卡框、费用数字、标题、稀有度条画进卡图；按原版样本，正式卡图与 beta 卡图是两套尺寸 |
| 顶栏头像 | 顶栏角色识别、联机状态识别 | 原版图是 `images/ui/top_panel/character_icon_<entry>.png`，由 `scenes/ui/character_icons/<entry>_icon.tscn` 包装；同图也用于联机玩家条 | 轻度依赖 scene；无复杂分层；另有一张独立 outline 图 | 可以 | 透明底 `PNG` | 推荐来稿 `256x256`；原版最终图 `88x88` | 显示区域是方形；顶栏主显示约 `54x54`，联机条约 `48x48`；如只交一张来稿，我会裁切缩放到最终 `88x88` |
| 角色能量图标 | 卡牌左上角费用图标、联机状态能量图标 | 卡牌与联机条使用 `energy_<character>` 图；数字是独立 `Label`，不在图里 | 卡牌 / 联机图标来自 atlas 区块，原版 atlas 区域是 `74x74`；战斗能量球另有独立 scene 和 `5` 层贴图 | 可以，但建议把“小图标”和“战斗能量球”分开提交 | 透明底 `PNG` | 卡牌 / 联机图标推荐来稿 `256x256`，我会整理成最终 `74x74`；战斗能量球建议 `5` 张 `256x256` 分层图 | 不要把数字画进 icon；战斗能量球当前 scene 是 `128x128` 容器，但原版层图实际是 `256x256` |
| buff / debuff icon | 状态栏 power / debuff 显示与相关特效引用 | 原版主资源是 `images/powers/<entry>.png`；atlas 由系统再打包；部分场景直接引用原图 | 不依赖每个 icon 自己的 scene；会进入 power atlas | 可以 | 透明底 `PNG` | `256x256` | `Pressure`、`Persona Dissociation`、`Social Withdrawal`、`Inferiority` 都按这套规格准备最稳 |
| 起始 relic 图 | relic 面板、提示、背包等显示 | 原版主资源是 `images/relics/<entry>.png`；系统同时有 relic atlas / outline atlas | 不需要单独 scene；atlas 由系统处理 | 可以 | 透明底 `PNG` | `256x256` | `DollMask` 先按这一规格提交即可；beta 版图不是当前必需项 |
| 完整音乐曲目 | 歌曲牌播放与局内点歌系统共享曲库 | 当前项目先按工作区资产库管理，不把未经验证的引擎导入细节写成官方事实 | 当前先不硬写 scene 依赖；后续 runtime 接线再确认 | 可以 | `OGG` 优先 | 保持母带比例，不强制统一像素尺寸 | 同一首歌优先只保留一份完整曲目，歌曲牌与点歌系统共用 |
| 歌曲牌短 cue | 战斗内短时播放、切片或截段 | 当前项目先按资产库管理；只有完整曲目不适合直接播放时才单开 | 当前不预设 runtime 包装细节 | 可以 | `OGG` 优先 | 不适用 | 文件名建议与完整曲目共用词根并加 `_cue` |
| 短音效 | 角色选择、点歌系统和局内 UI 提示 | 当前项目先按资产库管理，不把具体音频总线或事件系统写死 | 当前不预设 runtime 包装细节 | 可以 | `WAV` 或 `OGG` | 不适用 | 角色选人与点歌系统音效分别入各自子目录 |
| `Spine` 战斗立绘运行时资产 | 角色战斗内动态立绘 | 原版入口是 `creature_visuals/<character>.tscn`，核心节点是 `SpineSprite`，真正动画由 `*.skel + *.atlas + *_skel_data.tres` 驱动 | 强依赖 `Spine` skeleton、atlas、动画命名和 scene 接线 | 不适合只交单张成图；需要导出 runtime 资产 | `atlas + skel + png + tres` | atlas page 尺寸由导出决定；建议工作源图按统一画布维护 | 若要沿原版路径做动画，必须交付完整 `Spine` 运行时资产，而不是只有分层 PNG |

## 四 规格依据

本轮确认到的关键依据如下：

- 角色选择大立绘来自 `char_select_bg_<character>.tscn` 场景最终渲染，当前已导出原版 `5` 角色成品图，尺寸都是 `2560x1200`
- 已导出的原版 `Ironclad` 正式卡图样本实测是 `1000x760`
- 已导出的原版 `Ironclad` beta 卡图样本实测是 `668x508`
- 顶栏头像原图 `character_icon_ironclad.png` 实测是 `88x88`
- 顶栏头像 outline 原图 `character_icon_ironclad_outline.png` 实测也是 `88x88`
- 卡牌 / 联机能量图标 `energy_ironclad` 在 atlas 中的区域是 `74x74`
- 战斗能量球单层原图 `ironclad_orb_layer_1.png` 实测是 `256x256`
- `weak_power.png` 实测是 `256x256`
- `burning_blood.png` 实测是 `256x256`

## 五 素材应如何提交

当前最合理的工作流是：

1. 你先把原始来稿放进 `incoming_assets/`
2. 文件名在来稿阶段可以直观描述，不要求立刻对齐最终正式名
3. 我再根据原版规格做裁切、缩放、命名和正式落位
4. 整理完成后，正式资源才进入 `assets/`
5. 同步更新资产清单和占位 / 正式状态

若是战斗动态立绘：

1. 先在 `Spine` 工程里完成 bone / slot / animation
2. 导出运行时文件到 `incoming_assets/character_in_combat_spine/togawasakiko/spine_export/`
3. 原始拆图与工程源图放 `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/`
4. 我再整理到 `assets/animations/characters/togawasakiko/` 与 `pack/animations/characters/togawasakiko/`
5. 最后把 `pack/scenes/creature_visuals/togawasakiko.tscn` 切到 `SpineSprite` 正式链

这样做的原因：

- 不把原始来稿和最终安装资源混在一起
- 不要求你交稿时先记住所有正式文件名
- 后续同一资源多版本替换更容易追踪

## 六 来稿投递目录

当前已建立：

- `incoming_assets/cards/basic/`
- `incoming_assets/cards/normal_pool/`
- `incoming_assets/cards/pressure_generated/`
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
- `incoming_assets/relics/future/`
- `incoming_assets/audio/music/full_tracks/`
- `incoming_assets/audio/music/song_cues/`
- `incoming_assets/audio/sfx/character_select/`
- `incoming_assets/audio/sfx/jukebox/`
- `incoming_assets/audio/sfx/ui/`

推荐投递方式：

| 你要交的东西 | 放到哪里 | 我后续会怎么处理 |
| --- | --- | --- |
| 起始牌 / Basic 卡图 | `incoming_assets/cards/basic/` | 检查画幅并按正式文件名整理到 `assets/cards/basic/` |
| 正常卡池卡图 | `incoming_assets/cards/normal_pool/` | 按卡牌内部名、稀有度和正式命名规则整理到 `assets/cards/normal/` |
| 压力衍生牌卡图 | `incoming_assets/cards/pressure_generated/` | 按衍生牌正式名整理到 `assets/cards/generated_pressure/` |
| 角色选择静态整图 | `incoming_assets/character_select_splash/` | 检查画幅；若确认采用静态终稿路线，则整理并落位到 `assets/character/char_select_bg/char_select_bg_togawasakiko.png` |
| 选角色时的小头像 | `incoming_assets/character_select_icon/` | 整理为选角按钮图，后续落位到 `assets/character/char_select/char_select_togawasakiko.png` |
| 角色游戏内静态立绘 | `incoming_assets/character_in_combat_portrait/` | 作为战斗内角色静态立绘来稿保留；当前规范下整理为 `assets/character/in_combat_portrait/in_combat_portrait_togawasakiko.png` |
| 角色战斗 `Spine` 源拆图 | `incoming_assets/character_in_combat_spine/togawasakiko/source_layers/` | 保留 `Spine` 工程用的原始分层素材，不直接进入 runtime |
| 角色战斗 `Spine` 导出包 | `incoming_assets/character_in_combat_spine/togawasakiko/spine_export/` | 检查命名后整理到 `assets/animations/characters/togawasakiko/` 与 `pack/animations/characters/togawasakiko/` |
| 角色战斗 `Spine` 预览 | `incoming_assets/character_in_combat_spine/togawasakiko/previews/` | 仅作校对，不进入 runtime |
| 商店角色静态立绘 | `incoming_assets/character_merchant_portrait/` | 作为商店角色专属静态立绘来稿保留；不再与战斗立绘共用。当前规范下整理为 `assets/character/merchant/merchant_portrait_togawasakiko.png` |
| 火堆房间角色坐姿图 | `incoming_assets/rest_site_character/` | 作为火堆房间静态角色资源来稿保留；当前规范下整理为 `assets/character/rest_site/rest_site_character_togawasakiko.png` |
| 联机手势图 | `incoming_assets/multiplayer_hand/` | 当前首版可先只交 `point` 一张；后续整理为 `assets/character/multiplayer_hand/multiplayer_hand_togawasakiko_<pose>.png` |
| 顶栏头像来稿 | `incoming_assets/top_panel_portrait/` | 裁切成方形，导出最终 `character_icon_*.png`，必要时再生成 outline 需求位 |
| 卡牌 / 联机能量 icon | `incoming_assets/energy/card_icon/` | 缩放并落位到 `assets/character/energy/energy_togawasakiko.png` |
| 战斗能量球分层图 | `incoming_assets/energy/combat_orb_layers/` | 按层序整理为 `togawasakiko_orb_layer_*.png` |
| 压力 / debuff icon | `incoming_assets/powers/` | 按内部主键重命名并落位到 `assets/icons/powers/` |
| starter relic 图 | `incoming_assets/relics/starter/` | 整理为 `assets/relics/starter/doll_mask.png` |
| 完整音乐曲目 | `incoming_assets/audio/music/full_tracks/` | 规范化命名后整理到 `assets/audio/music/tracks/`，必要时同步进入 `pack/audio/music/tracks/` |
| 歌曲牌短 cue | `incoming_assets/audio/music/song_cues/` | 规范化命名后整理到 `assets/audio/music/song_cues/` |
| 事件专用背景音乐 | `incoming_assets/audio/music/events/` | 规范化命名后整理到 `assets/audio/music/events/`，必要时同步进入 `pack/audio/music/events/` |
| 角色选择音效 | `incoming_assets/audio/sfx/character_select/` | 规范化命名后整理到 `assets/audio/sfx/character_select/` |
| 点歌系统音效 | `incoming_assets/audio/sfx/jukebox/` | 规范化命名后整理到 `assets/audio/sfx/jukebox/`；当前不是实现阻塞项 |
| 其它局内 UI 音效 | `incoming_assets/audio/sfx/ui/` | 规范化命名后整理到 `assets/audio/sfx/ui/` |

## 七 哪些可以先占位，哪些应尽快正式

建议尽快正式提交：

- 角色选择主立绘
- 顶栏头像
- 卡牌 / 联机能量 icon
- 战斗能量球分层图
- `Pressure` 与 `3` 个原创 debuff icon
- `DollMask` relic 图

可以先占位：

- 联机手势图
- 大批量卡图
- song 的未来可视化 icon
- 角色选择与点歌系统短音效
- 歌曲牌短 cue

当前线程补充决策：

- 锁定图当前不做
- 顶栏头像 outline 当前不做
- 联机手势图若要启动，先做 `point` 即可
- 若战斗立绘重启动态方案，优先沿原版 `SpineSprite` 路径，不再继续自定义 Godot 分层 Tween 偏路

## 八 当前结论

- `song` 已明确纠正为逻辑标签，不再作为当前必需图像资产推进
- 当前最优工作流是“先投递到 `incoming_assets/`，再由我整理进 `assets/`”
- 当前音频也走同一套三层流程：
  - `incoming_assets/audio/`
  - `assets/audio/`
  - `pack/audio/`
- 当前 `jukebox` UI 走文本列表口径，不要求歌曲图标或封面
- 当前 `jukebox` 专属 UI 音效不是优先提交项，默认优先复用原版 UI 音效口径
- 这套流程已经足够支持后续持续交图与交音，而不会污染正式资源目录
