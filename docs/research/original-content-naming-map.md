# 原版内容命名映射表

记录日期：2026-03-23

本表只收录当前已经直接拿到证据的代表性对象，目的是给后续命名规范提供查表基线，不追求穷举全游戏。

## 卡牌

| 类别 | 类名 | `Entry` | 文件/资源名 | 显示名 | console 建议 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| Card | `StrikeIronclad` | `STRIKE_IRONCLAD` | `ironclad/strike_ironclad.png` | `Strike` | `card STRIKE_IRONCLAD` | 已确认 |
| Card | `DefendIronclad` | `DEFEND_IRONCLAD` | `ironclad/defend_ironclad.png` | `Defend` | `card DEFEND_IRONCLAD` | 已确认 |
| Card | `PrimalForce` | `PRIMAL_FORCE` | `ironclad/primal_force.png` | `Primal Force` | `card PRIMAL_FORCE` | 已确认 |
| Card | `Hellraiser` | `HELLRAISER` | `ironclad/hellraiser.png` | `Hellraiser` | `card HELLRAISER` | 已确认 |
| Card | `GiantRock` | `GIANT_ROCK` | `token/giant_rock.png` | `Giant Rock` | `card GIANT_ROCK` | 已确认 |

## Power / Debuff

| 类别 | 类名 | `Entry` | 文件名 | 显示名 | console 建议 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| Power | `WeakPower` | `WEAK_POWER` | `weak_power.png` | `Weak` | `power WEAK_POWER <amount> <target>` | 已确认 |
| Power | `VulnerablePower` | `VULNERABLE_POWER` | `vulnerable_power.png` | `Vulnerable` | `power VULNERABLE_POWER <amount> <target>` | 已确认 |
| Power | `StrengthPower` | `STRENGTH_POWER` | `strength_power.png` | `Strength` | `power STRENGTH_POWER <amount> <target>` | 已确认 |
| Power | `DexterityPower` | `DEXTERITY_POWER` | `dexterity_power.png` | `Dexterity` | `power DEXTERITY_POWER <amount> <target>` | 已确认 |
| Power | `PoisonPower` | `POISON_POWER` | `poison_power.png` | `Poison` | `power POISON_POWER <amount> <target>` | 已确认 |

## Relic

| 类别 | 类名 | `Entry` | 文件名 | 显示名 | console 建议 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| Relic | `Akabeko` | `AKABEKO` | `akabeko.png` | `Akabeko` | `relic AKABEKO` | 已确认 |
| Relic | `BurningBlood` | `BURNING_BLOOD` | `burning_blood.png` | `Burning Blood` | `relic BURNING_BLOOD` | 已确认 |
| Relic | `StrikeDummy` | `STRIKE_DUMMY` | `strike_dummy.png` | `Strike Dummy` | `relic STRIKE_DUMMY` | 已确认 |
| Relic | `PaperKrane` | `PAPER_KRANE` | `paper_krane.png` | `Paper Krane` | `relic PAPER_KRANE` | 已确认 |
| Relic | `PaperPhrog` | `PAPER_PHROG` | `paper_phrog.png` | `Paper Phrog` | `relic PAPER_PHROG` | 已确认 |

## 角色

| 类别 | 类名 | `Entry` | 资源名 | 显示名 | 备注 | 状态 |
| --- | --- | --- | --- | --- | --- | --- |
| Character | `Ironclad` | `IRONCLAD` | `char_select_ironclad.png`, `character_icon_ironclad.png`, `ironclad_icon.tscn` | `The Ironclad` | 显示名带定冠词，内部名不带 | 已确认 |
| Character | `Silent` | `SILENT` | `char_select_silent.png`, `character_icon_silent.png`, `silent_icon.tscn` | `The Silent` | 同上 | 已确认 |
| Character | `Defect` | `DEFECT` | `char_select_defect.png`, `character_icon_defect.png`, `defect_icon.tscn` | `The Defect` | 同上 | 已确认 |
| Character | `Regent` | `REGENT` | `char_select_regent.png`, `character_icon_regent.png`, `regent_icon.tscn` | `The Regent` | 同上 | 已确认 |
| Character | `Necrobinder` | `NECROBINDER` | `char_select_necrobinder.png`, `character_icon_necrobinder.png`, `necrobinder_icon.tscn` | `The Necrobinder` | 同上 | 已确认 |
| Character | `RandomCharacter` | `RANDOM_CHARACTER` | `char_select_random.png`, `character_icon_random_character.png` | `Random` | 显示名不是 `The Random Character` | 已确认 |

## 归纳

- 类名是逻辑层的起点
- `Entry` 是 console、定位、本地化、资源命名的共同中轴
- 显示名经常被简化、美化或复用，不能拿来当内部主键
- 资源名大多是 `entry-lower`
