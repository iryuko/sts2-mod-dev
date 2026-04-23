# 音频轨道与 Cue 登记表

日期：2026-04-05

## 一 目的

本文件记录：

- 哪些正式音频 `track_id / cue_id` 已占位
- 哪些用于歌曲牌
- 哪些可被点歌系统复用为 BGM
- 哪些属于角色选择或 UI 音效

当前规则：

- 没有正式入库文件时，也可以先登记稳定 `track_id / cue_id`
- 同一首歌优先只保留一份完整曲目文件
- 若歌曲牌需要短版播放，再单独登记 `_cue`

## 二 角色选择 / UI 音效

| CueId | 用途 | 规范化库存路径 | runtime 路径 | 当前状态 |
| --- | --- | --- | --- | --- |
| `togawasakiko_select` | 角色选择确认音效 | `assets/audio/sfx/character_select/togawasakiko_select.ogg` | `pack/audio/sfx/character_select/togawasakiko_select.ogg` | 已入正式库，已接选人界面本地播放补丁 |
| `togawasakiko_hover` | 角色选择 hover 音效 | `assets/audio/sfx/character_select/togawasakiko_hover.wav` | `pack/audio/sfx/character_select/togawasakiko_hover.wav` | 预留，待来稿 |
| `jukebox_open` | 点歌系统打开音效 | `assets/audio/sfx/jukebox/jukebox_open.wav` | `pack/audio/sfx/jukebox/jukebox_open.wav` | 登记位保留，当前不阻塞实现 |
| `jukebox_confirm` | 点歌系统确认音效 | `assets/audio/sfx/jukebox/jukebox_confirm.wav` | `pack/audio/sfx/jukebox/jukebox_confirm.wav` | 登记位保留，当前不阻塞实现 |
| `jukebox_next_track` | 点歌系统切歌音效 | `assets/audio/sfx/jukebox/jukebox_next_track.wav` | `pack/audio/sfx/jukebox/jukebox_next_track.wav` | 登记位保留，当前不阻塞实现 |
| `jukebox_stop` | 点歌系统停止 / 退出音效 | `assets/audio/sfx/jukebox/jukebox_stop.wav` | `pack/audio/sfx/jukebox/jukebox_stop.wav` | 登记位保留，当前不阻塞实现 |

## 三 音乐轨道

| TrackId | 主要用途 | 正式库存路径 | runtime 路径 | 歌曲牌可用 | 点歌系统可用 | 当前状态 |
| --- | --- | --- | --- | --- | --- | --- |
| `togawasakiko_title_theme` | 角色主题曲 / 选人页预留 | `assets/audio/music/tracks/togawasakiko_title_theme.mp3` | `pack/audio/music/tracks/togawasakiko_title_theme.mp3` | 否 | 是 | 预留，待来稿 |
| `ave_mujica` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/ave_mujica.mp3` | `pack/audio/music/tracks/ave_mujica.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `killkiss` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/killkiss.mp3` | `pack/audio/music/tracks/killkiss.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `music_of_the_celestial_sphere` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/music_of_the_celestial_sphere.mp3` | `pack/audio/music/tracks/music_of_the_celestial_sphere.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `choir_s_choir` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/choir_s_choir.mp3` | `pack/audio/music/tracks/choir_s_choir.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `ether` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/ether.mp3` | `pack/audio/music/tracks/ether.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `imprisoned_xii` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/imprisoned_xii.mp3` | `pack/audio/music/tracks/imprisoned_xii.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `masquerade_rhapsody_request` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/masquerade_rhapsody_request.mp3` | `pack/audio/music/tracks/masquerade_rhapsody_request.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `sophie` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/sophie.mp3` | `pack/audio/music/tracks/sophie.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `s_the_way` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/s_the_way.mp3` | `pack/audio/music/tracks/s_the_way.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `two_moons_deep_into_the_forest` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/two_moons_deep_into_the_forest.mp3` | `pack/audio/music/tracks/two_moons_deep_into_the_forest.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `god_you_fool` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/god_you_fool.mp3` | `pack/audio/music/tracks/god_you_fool.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `face` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/face.mp3` | `pack/audio/music/tracks/face.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `black_birthday` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/black_birthday.mp3` | `pack/audio/music/tracks/black_birthday.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `crucifix_x` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/crucifix_x.mp3` | `pack/audio/music/tracks/crucifix_x.mp3` | 是 | 是 | 已入正式库，已进入 runtime |
| `georgette_me_georgette_you` | 局内点歌候选 | `assets/audio/music/tracks/georgette_me_georgette_you.mp3` | `pack/audio/music/tracks/georgette_me_georgette_you.mp3` | 否 | 是 | 已入正式库，已进入 runtime |
| `treasure_pleasure` | 局内点歌候选 | `assets/audio/music/tracks/treasure_pleasure.mp3` | `pack/audio/music/tracks/treasure_pleasure.mp3` | 否 | 是 | 已入正式库，已进入 runtime |
| `symbol_i` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/symbol_i.mp3` | `pack/audio/music/tracks/symbol_i.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - Symbol I _ △ (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `312.23s` |
| `symbol_ii` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/symbol_ii.mp3` | `pack/audio/music/tracks/symbol_ii.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - Symbol II _ Air (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `212.89s` |
| `symbol_iii` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/symbol_iii.mp3` | `pack/audio/music/tracks/symbol_iii.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - Symbol III _ ▽ (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `257.00s` |
| `symbol_iv` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/symbol_iv.mp3` | `pack/audio/music/tracks/symbol_iv.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - Symbol IV _ Earth (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `239.21s` |
| `a_wonderful_world_yet_nowhere_to_be_found` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/a_wonderful_world_yet_nowhere_to_be_found.mp3` | `pack/audio/music/tracks/a_wonderful_world_yet_nowhere_to_be_found.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - 素晴らしき世界 でも どこにもない場所 (Utopia) (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `269.55s` |
| `angles` | 歌曲牌与局内点歌候选 | `assets/audio/music/tracks/angles.mp3` | `pack/audio/music/tracks/angles.mp3` | 是 | 是 | 2026-04-21 已由来稿 `Ave Mujica - Angles (Official Music Video)_音频.mp4` 转存并入库；当前时长约 `276.33s` |

## 四 歌曲牌短 Cue

只有当完整曲目不适合直接战斗内播放时，才登记这一层。

| CueId | 对应 TrackId | 规范化库存路径 | runtime 路径 | 当前状态 |
| --- | --- | --- | --- | --- |
| `ave_mujica_cue` | `ave_mujica` | `assets/audio/music/song_cues/ave_mujica_cue.ogg` | `pack/audio/music/song_cues/ave_mujica_cue.ogg` | 预留，待判断是否需要 |
| `killkiss_cue` | `killkiss` | `assets/audio/music/song_cues/killkiss_cue.ogg` | `pack/audio/music/song_cues/killkiss_cue.ogg` | 预留，待判断是否需要 |

## 五 当前结论

- 当前已经有稳定的 `track_id / cue_id` 登记位
- `togawasakiko_select` 当前已不是纯库存状态，已由 Harmony patch 在选人界面直接播放本地 `ogg`
- 当前 `jukebox` 已正式入库并接入 runtime 的曲目共 `23` 首，文件格式本轮按 `mp3` 收口
- 歌曲牌播放与点歌系统默认共用 `music/tracks/`
- 只有在完整曲目不适合直接战斗内播放时，才新增 `music/song_cues/`
- 2026-04-05 起，局内 `jukebox` UI 已直接扫描 `pack/audio/music/tracks/`
- 当前正式曲库已不再为空，`jukebox` 应直接枚举这 `23` 首 runtime 曲目
- 当前 `jukebox` 默认采用文本列表，不要求额外图标资源
- 当前 `jukebox` 专属 UI 音效不是阻塞项，优先按原版 UI 音效口径处理
