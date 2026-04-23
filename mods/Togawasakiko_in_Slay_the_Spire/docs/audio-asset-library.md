# 音频资产库

日期：2026-04-04

## 一 目的

本文件用于把 `Togawasakiko_in_Slay_the_Spire` 的音频资产固定到一套可长期维护的结构上。

当前覆盖三类用途：

- 歌曲牌打出时播放音乐
- 角色选择时播放独特音效
- 局内点歌系统复用已有音乐文件切换 BGM

当前原则：

- 先建稳定资产库，不先拍死 runtime 实现细节
- 歌曲牌播放与点歌系统优先共用同一份正式音乐文件
- 原始来稿、正式库存、runtime staging 三层分离

## 二 目录结构

```text
assets/
  audio/
    music/
      tracks/
      song_cues/
    sfx/
      character_select/
      jukebox/
      ui/

incoming_assets/
  audio/
    _originals/
    music/
      full_tracks/
      song_cues/
    sfx/
      character_select/
      jukebox/
      ui/

pack/
  audio/
    music/
      tracks/
      song_cues/
    sfx/
      character_select/
      jukebox/
      ui/
```

## 三 各层职责

### 1. `incoming_assets/audio/`

- 只放原始来稿
- 文件名可先直观描述，不要求一开始就对齐正式名
- 若同一音频多次返修，旧版先归到 `_originals/`

### 2. `assets/audio/`

- 放规范化后的正式库存
- 文件名统一使用 ASCII `lower_snake_case`
- 这一层是 T3 维护的音频真值源

### 3. `pack/audio/`

- 放 runtime staging 资源
- 用于后续 Godot 导入、PCK 打包与实机接线
- 原则上由 `assets/audio/` 整理后同步进入

## 四 分类规则

### 1. `music/tracks/`

- 放可被直接完整播放的正式音乐文件
- 歌曲牌与点歌系统优先共用这组文件
- 不为“歌曲牌播放”和“点歌 BGM”各复制一份相同音频

### 2. `music/song_cues/`

- 放歌曲牌专用的短版、切片或单独 cue
- 只有当完整曲目不适合直接在出牌时播放时才单开
- 若可以直接用 `tracks/`，就不要重复导出一份

### 2.5 `music/events/`

- 放普通事件或特殊事件专用的背景音乐
- 只用于不应混入 `tracks/` 或 `song_cues/` 的事件独占音频
- 当前 `UnattendedPiano` 事件建议按：
  - `assets/audio/music/events/unattended_piano.mp3`
  - `pack/audio/music/events/unattended_piano.mp3`

### 3. `sfx/character_select/`

- 放角色选择界面的独特音效
- 至少预留：
  - `togawasakiko_select`
- 可选预留：
  - `togawasakiko_hover`
  - `togawasakiko_confirm`
  - `togawasakiko_locked`

### 4. `sfx/jukebox/`

- 放点歌系统自身的 UI / 操作音效
- 例如：
  - 打开
  - 切歌
  - 确认
  - 停止

### 5. `sfx/ui/`

- 放不属于点歌系统、但和角色局内功能相关的通用短音效
- 例如未来的 song watcher 提示、资源切换提示等

## 五 命名规则

正式库存文件统一使用：

- ASCII
- 小写
- 下划线分词
- 不把中文显示名直接写进最终文件名

建议模式：

- 完整曲目：
  - `<track_id>.ogg`
- 歌曲牌短 cue：
  - `<track_id>_cue.ogg`
- 循环版：
  - `<track_id>_loop.ogg`
- 角色选人音效：
  - `togawasakiko_select.wav`
  - `togawasakiko_hover.wav`
- 点歌系统音效：
  - `jukebox_open.wav`
  - `jukebox_confirm.wav`
  - `jukebox_next_track.wav`

## 六 当前项目规范建议

以下是当前项目建议，不写成“STS2 官方硬要求”：

- 完整音乐优先使用 `OGG`
- 短音效优先使用 `WAV` 或 `OGG`
- 同一首曲目的“完整播放版”和“短 cue 版”共用同一个 `track_id`
- 歌曲牌和点歌系统优先通过 `track_id` 绑定，而不是各自硬写不同文件名

## 七 与现有设计的关系

- `song` 仍然首先是逻辑标签，不因为建立音频库就变成“必须有单独图像 / 单独卡池”
- 歌曲牌音频属于新增的声音资产层，不改变现有卡图目录规则
- 角色选人音效不影响当前 `CharacterSelectSfx` 代码口径，当前只是先把资产位建好
- 点歌系统未来应优先读取 `music/tracks/`，而不是绕开资产库直接扫来稿目录

## 八 当前结论

- 音频资产库已经作为正式目录层建立
- 后续可按“来稿 -> 正式库存 -> runtime staging”三层流程持续接收声音资源
- 当前尚未默认安装任何角色专属音频文件，也尚未开始 runtime 播放接线
