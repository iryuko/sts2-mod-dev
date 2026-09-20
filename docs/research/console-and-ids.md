# Console 与内部 ID

记录日期：2026-03-23

## 相关命令已确认存在

DLL 中已直接确认以下与对象测试强相关的 console 命令类型：

- `CardConsoleCmd`
- `RemoveCardConsoleCmd`
- `UpgradeCardConsoleCmd`
- `ApplyPowerConsoleCmd`
- `RelicConsoleCmd`
- `PotionConsoleCmd`
- `AfflictConsoleCmd`
- `EnchantConsoleCmd`
- 以及 `ActConsoleCmd`、`AncientConsoleCmd`、`EventConsoleCmd`、`FightConsoleCmd`、`RoomConsoleCmd`

结论：STS2 的 dev console 不只是日志窗口，而是明确具备“按对象名生成 / 修改内容”的测试入口。

## 各对象到底吃什么名字

| 命令 | 主要输入 | 匹配方式 | 当前结论 |
| --- | --- | --- | --- |
| `card` | 卡牌 ID | `Id.Entry == 输入.ToUpperInvariant()` | 精确匹配内部 `Entry` |
| `remove_card` | 卡牌 ID | 同上 | 精确匹配内部 `Entry` |
| `power` | power ID | `Id.Entry == 输入.ToUpperInvariant()` | 精确匹配内部 `Entry` |
| `potion` | potion ID | `Id.Entry == 输入.ToUpperInvariant()` | 精确匹配内部 `Entry` |
| `afflict` | affliction ID | 手工组 `AFFLICTION.<ENTRY>` | 精确匹配内部 `Entry` |
| `enchant` | enchantment ID | 手工组 `ENCHANTMENT.<ENTRY>` | 精确匹配内部 `Entry` |
| `relic` | relic ID | `==` 再 `StartsWith` 再 `Contains` | 相对宽松，但完整 `Entry` 最稳 |
| `upgrade` | 手牌位置 | 按 index | 不吃卡牌名 |

## 已确认的关键差异

### 1. 卡牌命令文案明确要求“全大写蛇形”

`CardConsoleCmd.Description`：

- `Screaming snake case ('BODY_SLAM', not 'Body Slam')`

`PotionConsoleCmd.Description` 也有同样表述。

这不是用户社区口口相传的习惯，而是原版命令文案自己写出来的要求。

### 2. relic 命令更宽松，但不应当依赖这种宽松

`RelicConsoleCmd` 的模糊匹配顺序是：

1. 全等
2. 前缀
3. 包含

所以 `relic paper` 之类写法“有机会工作”，但它不是长期稳定规范。

### 3. console 更接近逻辑层，不接近显示层

从这些命令的实现看，匹配对象时都在看：

- `ModelDb.AllCards / AllPowers / AllRelics / AllPotions`
- 然后比较 `Id.Entry`

而不是：

- `Title`
- 本地化显示名
- 文件名

## 对自定义 mod 测试的直接建议

### 已确认可直接模仿

- 卡牌：让 console 输入直接等于 `Entry`
- power：让 console 输入直接等于 `Entry`
- potion：让 console 输入直接等于 `Entry`

### 最稳的命名策略

1. 内部名先服务 `Id.Entry`，再服务显示名。
2. `Entry` 必须全局唯一，不要指望显示名帮你消歧。
3. 如果对象要经常用 console 测，优先使用完整 `ENTRY`，不要依赖 relic 那类模糊命中。
4. 同一对象的类名、`Entry`、资源 base name 尽量保持一一对应。
5. 若对象会有多角色同名版本，像原版 `STRIKE_IRONCLAD` 那样在内部 ID 上明确消歧。

## 待验证

- `event`、`fight`、`room`、`act`、`ancient` 这些命令对各自对象名的精确匹配策略，还可以继续做一轮专项抽样。
