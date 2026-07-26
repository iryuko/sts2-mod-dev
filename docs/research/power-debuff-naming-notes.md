# Power / Debuff 命名笔记

记录日期：2026-03-23

## 已确认规律

### 1. 状态类名统一以 `Power` 结尾

代表类型：

- `WeakPower`
- `VulnerablePower`
- `FrailPower`
- `StrengthPower`
- `DexterityPower`
- `PoisonPower`

因此内部命名层没有把“buff”与“debuff”拆成两套后缀体系。

### 2. `Entry` 统一是 `_POWER`

代表关系：

| 类名 | `Entry` | 显示名 | 图标文件 |
| --- | --- | --- | --- |
| `WeakPower` | `WEAK_POWER` | `Weak` | `weak_power.png` |
| `VulnerablePower` | `VULNERABLE_POWER` | `Vulnerable` | `vulnerable_power.png` |
| `FrailPower` | `FRAIL_POWER` | `Frail` | `frail_power.png` |
| `StrengthPower` | `STRENGTH_POWER` | `Strength` | `strength_power.png` |
| `DexterityPower` | `DEXTERITY_POWER` | `Dexterity` | `dexterity_power.png` |
| `PoisonPower` | `POISON_POWER` | `Poison` | `poison_power.png` |

### 3. 显示层来自 `powers.json`

`PowerModel` 已确认：

- `Title => new LocString("powers", Id.Entry + ".title")`
- `Description => new LocString("powers", Id.Entry + ".description")`
- `SmartDescription`、`RemoteDescription` 也仍围绕同一个 `Entry`

这意味着 power 的显示层仍是：

- 同一个内部 `Entry`
- 不同后缀 key
- 同一张 `powers.json`

### 4. Buff / Debuff 的区别由 `PowerType` 表达

已确认：

- `WeakPower.Type => PowerType.Debuff`
- `VulnerablePower.Type => PowerType.Debuff`
- `StrengthPower.Type => PowerType.Buff`
- `DexterityPower.Type => PowerType.Buff`

结论：

- 命名层不负责表达 buff / debuff 区别
- 逻辑层的 `PowerType` 才负责

### 5. power 图标路径模板非常稳定

`PowerModel` 已确认：

- 大图：`images/powers/{entry-lower}.png`
- 打包图集：`atlases/power_atlas.sprites/{entry-lower}.tres`

因此 power 资源命名几乎就是把 `Entry` 小写化。

## console 结论

`ApplyPowerConsoleCmd` 已确认：

- 命令名：`power`
- 参数第一个值会 `ToUpperInvariant()`
- 按 `p.Id.Entry == powerId` 精确匹配

也就是说：

- `power WEAK_POWER 2 1` 这种写法符合系统习惯
- `power Weak 2 1` 不应视为稳定写法

## 高可能

- 自定义状态若想与原版习惯对齐，类名最好直接命成 `<Meaning>Power`。
- 若该状态可能同时出现在描述文本、图标资源、console 和 hover tip 中，越贴近 `ENTRY_POWER` 这套规则，维护成本越低。

## 待验证

- `TemporaryStrengthPower`、`TemporaryStrengthDown`、`DexterityDownPower` 这类“临时 / down”系是否存在更细的命名子风格可继续抽样。
