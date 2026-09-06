# 当前资产状态

记录日期：2026-09-06

## 权威边界

本文件记录“文件是否存在、是否有 runtime 对应物、当前表现属于正式还是兼容方案”。

- 详细命名与提交规则仍见 `resource-layout.md` 和 `resource-specs-and-submission-workflow.md`。
- 旧资产总表与 checklist 已归档，因为其中多处“待补/未接入”已过期。
- 文件存在不等于视觉质量已最终验收。

## 卡牌 portrait

当前源码中的全部卡牌 model 均有对应源 portrait：

| 分类 | 源 portrait 数量 | 状态 |
| --- | ---: | --- |
| Basic | 4 | 已存在并接 runtime |
| Common | 14 | 已存在并接 runtime |
| Uncommon | 23 | 已存在并接 runtime |
| Rare | 8 | 已存在并接 runtime |
| Ancient | 1 | `curseslander.png` 已存在并接 runtime |
| 压力衍生牌 | 4 | 已存在并接 runtime |
| Shadow 事件牌 | 3 | 已存在并接 runtime |
| relic-granted | 2 | 已存在并接 runtime |

T2 的“50 张 normal 卡图仍待补”已经不是当前文件事实。当前角色池共 50 张，其中 Common/Uncommon/Rare 为 45 张。

2026-09-06 已纠正五张 bridge 卡的资源接线：

| 卡牌 | runtime portrait | 状态 |
| --- | --- | --- |
| `UnfinishedScore` | `mod_assets/cards/normal/common/unfinished_score.png` | 独立正式图已入 PCK |
| `FollowingPhrase` | `mod_assets/cards/normal/common/following_phrase.png` | 独立正式图已入 PCK |
| `Unmask` | `mod_assets/cards/normal/uncommon/unmask.png` | 独立正式图已入 PCK |
| `RehearsalOrder` | `mod_assets/cards/normal/uncommon/rehearsal_order.png` | 独立正式图已入 PCK |
| `BackstageSupport` | `mod_assets/cards/normal/uncommon/backstage_support.png` | 独立正式图已入 PCK |

此前五张卡的构造器都复用 `basic/unendurable.png`；该旧状态已由精确路径回归测试覆盖。
当前仍缺游戏内卡框裁切验收，不把文件/PCK 验证写成最终视觉通过。

## 角色资源

已存在并接入：

- 角色选择按钮和锁定图。
- 角色选择背景 scene。
- top panel 图标与 outline。
- multiplayer hand 四种手势。
- 文本小能量 icon。
- 战斗能量计数器。
- 静态战斗角色 portrait 与 scene。
- merchant portrait 和兼容 scene。
- rest site portrait 和兼容 scene。

当前表现边界：

- 当前 `0.2.2` 分支已包含战斗 Spine runtime；本次 bridge 卡图热修不修改该部分。
- merchant/rest site 当前主要显示静态 portrait，不是正式专属 Spine 动画。
- 锁定图、top panel outline 等文件已存在；本轮未重新做视觉质量验收，不标成“最终完成”。
- 能量计数器缺两个 VFX 节点是已接受缺陷。

## Ancient

当前正式文件和 runtime 均已存在：

- Teiji 事件主图。
- Teiji 地图节点与 outline。
- Teiji 对话头像与 outline。
- Teiji background scene。
- `Curseslander` Ancient portrait。
- `BestCompanion`、`BlackLimousine`、`PianoOfMom` relic 图。

`prototype` 文件仍保留为生产过程参考，但不再代表“Teiji 尚未接入”。

## 普通事件

`UnattendedPiano` 已存在：

- 初始图。
- Shadow piano 图。
- 事件音乐。
- 3 张 Shadow portrait。

当前开放项是事件状态/SL 实机验证，不是资产缺失。

## 音频

- jukebox runtime tracks：23 首。
- 事件音乐：1 首。
- 角色选择本地音效：1 个。
- 源库存与 `pack/audio/` runtime staging 均存在。
- `audio-track-registry.md` 是曲目级登记表。

## 资源链

正式流程：

```text
incoming_assets/ -> assets/ -> pack/ -> Godot import -> runtime_imports/ -> PCK
```

检查顺序：

1. 源资产是否在 `assets/`。
2. runtime staging 是否在 `pack/`。
3. `.import` 和 `runtime_imports` 是否由 build 生成。
4. PCK 是否包含资源。
5. scene/代码是否引用正确路径。
6. 实机是否正确显示或播放。

不要仅凭 `assets/` 中存在文件就写“已接入”。
