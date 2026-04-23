# T3 音声入库说明：Jukebox / BGM 曲库

日期：2026-04-05

## 一 这份说明是给谁的

这份说明只写给 `T3-资源与资产`。

目的只有一个：

- 让 T3 知道“局内点歌 / BGM 选择系统”现在实际会读取哪一层文件
- 以及 T3 该怎么把歌曲文件正确放进项目，而不是只登记在文档里

## 二 当前实现的真实口径

当前局内 `jukebox` 已经接了第一版 runtime 骨架，但它的读取口径非常简单：

- 只扫描 `pack/audio/music/tracks/`
- 只认真实存在的音频文件
- 支持扩展名：
  - `.ogg`
  - `.wav`
  - `.mp3`

当前不会读取：

- `incoming_assets/`
- 文档登记表
- `assets/audio/music/song_cues/`
- 任何“只在总表里登记但还没落 runtime 文件”的曲目

结论：

- 如果 `pack/audio/music/tracks/` 里没有文件，游戏里的 `jukebox` 就只会显示 `Off (null)`
- 只把曲名写进文档，不会让它出现在游戏里

## 三 T3 需要怎么交 / 怎么装

### 1. 原始来稿先放这里

- `incoming_assets/audio/music/full_tracks/`

这一层允许保留原始文件名，不要求立刻对齐正式名。

### 2. 正式库存要整理到这里

- `assets/audio/music/tracks/<track_id>.ogg`

要求：

- 使用 ASCII
- 小写
- 下划线分词
- 推荐优先 `OGG`

例如：

- `assets/audio/music/tracks/ave_mujica.ogg`
- `assets/audio/music/tracks/killkiss.ogg`
- `assets/audio/music/tracks/music_of_the_celestial_sphere.ogg`

### 3. runtime 文件必须同步到这里

- `pack/audio/music/tracks/<track_id>.ogg`

这是当前局内 `jukebox` 真正读取的目录。

如果只整理了 `assets/`，但没有同步进 `pack/`，那游戏里还是不会出现。

### 4. 同步后要做什么

T4 这边会在 build 时把 `pack/` 打进 PCK。

因此 T3 交接时至少要确认：

- 正式文件已经存在于 `assets/audio/music/tracks/`
- 同名 runtime 文件已经存在于 `pack/audio/music/tracks/`
- 文件名与 `track_id` 一致

## 四 当前推荐的最小入库流程

建议按下面这条最小流程做：

1. 把原始来稿放进 `incoming_assets/audio/music/full_tracks/`
2. 统一转成正式名，例如 `ave_mujica.ogg`
3. 落正式库存：
   - `assets/audio/music/tracks/ave_mujica.ogg`
4. 同步 runtime：
   - `pack/audio/music/tracks/ave_mujica.ogg`
5. 在资产总表里把该曲目标成“已入正式库、已进入 runtime”

## 五 当前 UI 口径，T3 不需要多做的东西

当前 `jukebox` UI 已经明确按下面口径收口：

- 不做图标
- 不做封面
- 不做复杂列表卡片
- 就是一行文本对应一首歌
- `Off (null)` 固定作为第一项

因此 T3 当前不需要准备：

- 歌曲图标
- 歌曲封面
- 点歌系统专属按钮贴图

## 六 当前音效口径

当前不再要求 T3 先补 `jukebox_open / jukebox_confirm / jukebox_next_track / jukebox_stop` 这组专属 UI 音效。

当前实现决策是：

- `jukebox` 优先复用原版 UI 音效口径
- 本轮不把“点歌系统专属 UI 音效”作为阻塞项

结论：

- `assets/audio/sfx/jukebox/` 可以继续保留目录和登记位
- 但它不是当前 T3 的优先交付物

## 七 当前行为约束

为了避免和战斗内原版 BGM 逻辑打架，当前实现固定遵守这条规则：

- 每次进入战斗，`jukebox` 都会自动重置到 `Off (null)`

这意味着：

- 不需要记忆上次选曲
- 不需要为“跨战斗持续播放”准备额外逻辑
- 当前更适合把它理解为“局内手动切换 BGM 的简单工具”，而不是长期播放状态机

## 八 当前建议 T3 先交哪几首

如果只做最小验证，优先级建议是：

1. `ave_mujica`
2. `killkiss`
3. `music_of_the_celestial_sphere`

原因：

- 这几首已经在曲目登记表里有稳定 `track_id`
- 也是最容易和现有歌曲牌命名体系对齐的候选

## 九 T3 交接时最好附带的信息

每次交一首歌，建议顺手写清楚：

- 原始文件名
- 最终 `track_id`
- 最终正式库存路径
- 最终 runtime 路径
- 时长
- 是否建议循环
- 是否需要后续再切一份 `song_cue`

## 十 当前一句话结论

对 T3 来说，最重要的不是“再建新目录”，而是把真实曲目文件按正式名同步进：

- `assets/audio/music/tracks/`
- `pack/audio/music/tracks/`

只要这两层都到位，当前 `jukebox` 就能直接在游戏里把它显示成可选 BGM。
