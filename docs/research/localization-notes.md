# 本地化笔记

记录日期：2026-03-23

## 目录结构已确认

原版本地化目录为：

- `res://localization/eng/`
- `res://localization/zhs/`
- 以及其他语言目录

每种语言都按“表”拆分 JSON，例如：

- `cards.json`
- `powers.json`
- `relics.json`
- `characters.json`
- `monsters.json`
- `events.json`

`eng` 是源语言目录。

## `LocString` 的工作方式已确认

`LocString` 只存两段：

- `table`
- `key`

读取时走：

- `LocManager.Instance.GetTable(table).GetRawText(key)`

这说明显示文本不是散落在模型类里，而是明确与逻辑层分离。

## 各系统的 key 挂接方式

### 卡牌

- `CardModel.TitleLocString => cards/<Entry>.title`
- `CardModel.Description => cards/<Entry>.description`

### Power

- `PowerModel.Title => powers/<Entry>.title`
- `PowerModel.Description => powers/<Entry>.description`
- 还可能存在：
  - `<Entry>.smartDescription`
  - `<Entry>.remoteDescription`

### Relic

- `RelicModel.Title => relics/<Entry>.title`
- `RelicModel.Description => relics/<Entry>.description`
- `RelicModel.Flavor => relics/<Entry>.flavor`

### 角色

- `CharacterModel.Title => characters/<Entry>.title`
- 另有：
  - `.description`
  - `.titleObject`
  - `.pronounObject`
  - `.possessiveAdjective`
  - `.unlockText`

结论：原版本地化不是“只有标题和描述”，角色尤其明显带了整套语法相关字段。

## 语言加载与回退

`LocManager` 已确认：

- 当前语言若不是 `eng`，会先加载 `eng` 作为 fallback
- 再加载目标语言表
- 缺失项可回退到英文

这意味着：

- 英文 key 集合是主干
- 其他语言更像“覆盖层”

## mod 与 override 的挂接方式

### 1. mod PCK 本地化

`ModManager.GetModdedLocTables(language, file)` 已确认会查找：

- `res://<mod.manifest.pckName>/localization/<language>/<file>`

也就是：

- mod 可以把同名表文件打进自己的 PCK
- 游戏会把它 merge 到基础表

### 2. 用户本地 override

`LocManager` 已确认会读：

- `user://localization_override/<lang>/<file>.json`

并把 override merge 到当前表。

## 对中英双语 mod 的直接建议

1. 逻辑层只维护一个稳定 `Entry`，不要把语言信息写进类名或 ID。
2. `eng` 与 `zhs` 保持完全相同的 key 集合，只改 value。
3. 卡牌、relic、power、角色分别写进各自表，不要混表。
4. 代码里只引用 `Entry`，不要硬编码显示标题。
5. 临时实验可用 `user://localization_override`，正式发布更贴近原版的是随 mod PCK 提供 `localization/<lang>/<file>.json`。

## 高可能

- 若后续自定义角色需要长期维护，中英双语最稳的分层是：
  - `类名 / Entry / 资源 base name` 属于逻辑层
  - `*.title / *.description / 角色代词字段` 属于语言层
  - 两者通过同一个 `Entry` 连接

## 待验证

- 自定义表中 `EXTENSION.*` 这类 key 的具体用途。
- `LocTable.IsLocalKey(...)` 对格式化文化设置的影响边界。
