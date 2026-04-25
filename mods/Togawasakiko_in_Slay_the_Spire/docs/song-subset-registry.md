# Song 子集登记

日期：2026-04-03

## 一 本文件定位

本文件用于登记：

- 哪些正常卡池牌带 `song tag`
- 这些牌在资源、文档和后续实现里如何被统一识别

当前明确边界：

- song 牌属于正常卡池的一部分
- song 牌可以在战斗奖励中获得
- song 牌可以在商店中购买
- song 牌不是压力衍生牌
- song 只是普通逻辑 tag，不自带默认规则

## 二 当前冻结规则

### 1. tag 定位

- `song` 是普通逻辑 tag。
- `song` 本身不自动附带减费、抽牌、生成或任何默认收益。
- 一切收益都应由具体卡牌或 power 文本显式给出。

### 2. song 牌池

- 歌曲牌池包含所有带 `song tag` 的正常卡池牌。
- 不包含压力衍生牌。
- 不包含单纯为了生成而临时做出的 `Token` 牌。

### 3. 压力衍生牌池

当前冻结为固定 `4` 张：

- `Persona Dissociation`
- `Social Withdrawal`
- `All You Think About Is Yourself`
- `Overwork Anxiety`

当前要求：

- 压力衍生牌不进入正常卡池。
- 只能通过既定机制生成。
- 后续若未明确追加，不要自行扩池。

## 三 当前最少支持

song 当前首先是逻辑标签，不是图像资源类型。

song 子集首版至少需要：

- 本登记文档
- 在资产总表中为 song 留单独一栏
- 一套稳定的命名与登记规则

当前不需要：

- `song_tag.png`
- song 专属第二份卡图目录
- song 专属另一套卡池命名

## 四 资源与命名规则

- 正式卡图文件名仍按牌自身 `Entry` 命名
- 不把 `song` 直接拼进正式卡图文件名
- song 身份通过以下位置表达：
  - `song-subset-registry.md`
  - 后续资产总表字段
  - 后续实现中的标签字段

若未来为了 UI 可视化需要统一视觉提示，可再补：

- 共用 tag icon
- 共用角标 overlay

但这两项都不是当前首版必需资源。

建议字段：

| 字段 | 含义 |
| --- | --- |
| `Entry` | 英文稳定主键 |
| `Display` | 当前显示名 |
| `Rarity` | `Common / Uncommon / Rare / Basic` |
| `ArtPath` | 卡图文件路径 |
| `SongTag` | 是否带 `song tag` |
| `ArtStatus` | `placeholder / final` |
| `Notes` | 备注 |

## 五 当前登记状态

当前已接入源码、带 `song tag` 且已有独立卡图文件的条目如下：

| Entry | Display | Rarity | ArtPath | SongTag | ArtStatus | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| `AVE_MUJICA` | `Ave Mujica` | `Rare` | `assets/cards/normal/rare/ave_mujica.png` | `true` | `final` | 已入库并接 runtime |
| `ETHER` | `以太` | `Uncommon` | `assets/cards/normal/uncommon/ether.png` | `true` | `final` | 已入库并接 runtime |
| `CRUCIFIX_X` | `十字架X` | `Uncommon` | `assets/cards/normal/uncommon/crucifix_x.png` | `true` | `final` | 已入库并接 runtime |
| `FACE` | `颜` | `Common` | `assets/cards/normal/common/face.png` | `true` | `final` | 已入库并接 runtime |
| `MUSIC_OF_THE_CELESTIAL_SPHERE` | `天穹之乐` | `Uncommon` | `assets/cards/normal/uncommon/music_of_the_celestial_sphere.png` | `true` | `final` | 已入库并接 runtime |
| `KILL_KISS` | `KillKiss` | `Rare` | `assets/cards/normal/rare/killkiss.png` | `true` | `final` | 实际 `Entry` 已对齐为 `KILL_KISS` |
| `BLACK_BIRTHDAY` | `Black Birthday` | `Uncommon` | `assets/cards/normal/uncommon/black_birthday.png` | `true` | `final` | 已入库并接 runtime |
| `TREASURE_PLEASURE` | `Treasure Pleasure` | `Uncommon` | `assets/cards/normal/uncommon/treasure_pleasure.png` | `true` | `final` | 2026-04-06 已由来稿 `Treasure Pleasure.png` 对齐替换并接入 runtime |
| `GEORGETTE_ME_GEORGETTE_YOU` | `Georgette Me, Georgette You` | `Uncommon` | `assets/cards/normal/uncommon/georgette_me_georgette_you.png` | `true` | `final` | 2026-04-19 已由来稿 `Georgette Me, Georgette You.png` 对齐安装并接入 runtime |
| `SYMBOL_I` | `Symbol I` | `Common` | `assets/cards/normal/common/symbol_i.png` | `true` | `final` | 2026-04-20 已由来稿 `Symbol I.png` 对齐替换并接入 runtime |
| `SYMBOL_II` | `Symbol II` | `Uncommon` | `assets/cards/normal/uncommon/symbol_ii.png` | `true` | `final` | 2026-04-20 已由来稿 `Symbol II.png` 对齐替换并接入 runtime |
| `SYMBOL_III` | `Symbol III` | `Uncommon` | `assets/cards/normal/uncommon/symbol_iii.png` | `true` | `final` | 2026-04-20 已由来稿 `Symbol III.png` 对齐替换并接入 runtime |
| `SYMBOL_IV` | `Symbol IV` | `Rare` | `assets/cards/normal/rare/symbol_iv.png` | `true` | `final` | 2026-04-20 已由来稿 `Symbol IV.png` 对齐替换并接入 runtime |
| `CHOIR_S_CHOIR` | `Choir 'S' Choir` | `Rare` | `assets/cards/normal/rare/choir_s_choir.png` | `true` | `final` | 已入库并接 runtime |
| `IMPRISONED_XII` | `Imprisoned XII` | `Uncommon` | `assets/cards/normal/uncommon/imprisoned_xii.png` | `true` | `final` | 已入库并接 runtime |
| `GOD_YOU_FOOL` | `God, You Fool` | `Common` | `assets/cards/normal/common/god_you_fool.png` | `true` | `final` | 已入库并接 runtime |
| `MASQUERADE_RHAPSODY_REQUEST` | `Masquerade Rhapsody Request` | `Uncommon` | `assets/cards/normal/uncommon/masquerade_rhapsody_request.png` | `true` | `final` | 英文显示名仍可后续再校对 |
| `S_THE_WAY` | `'S/' The Way` | `Common` | `assets/cards/normal/common/s_the_way.png` | `true` | `final` | 已入库并接 runtime |
| `TWO_MOONS_DEEP_INTO_THE_FOREST` | `Two Moons Deep Into The Forest` | `Uncommon` | `assets/cards/normal/uncommon/two_moons_deep_into_the_forest.png` | `true` | `final` | 已入库并接 runtime |
| `SOPHIE` | `Sophie` | `Uncommon` | `assets/cards/normal/uncommon/sophie.png` | `true` | `final` | 已入库并接 runtime |

当前明确不是 song 的条目：

| Entry | Display | Notes |
| --- | --- | --- |
| `COMPOSE` | `谱曲` | 生成歌曲牌，但自身不带 `song tag` |
| `SAKI_MOVE_PLZ` | `祥，移动` | 与“上一张是否为歌曲牌”联动，但自身不带 `song tag` |

## 六 当前结论

- song 已被明确写入资源系统，不再是“以后再想”的隐含需求。
- 当前已入源码并带 `song tag` 的条目共 `20` 张，且都已有稳定文件路径。
- pressure 衍生牌池与 song 池已经明确分离。
- `Compose` 与 `Ave Mujica` 这类“池生成”效果，后续实现时必须严格从冻结对象集合中取，不要做全池随机或自行补池。
