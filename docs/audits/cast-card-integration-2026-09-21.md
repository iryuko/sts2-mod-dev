# Cast 卡牌接入与本机安装（2026-09-21）

用户明确要求将现有 cast 接到技能牌、能力牌及压力衍生牌，并安装到 Steam。

## 已接入行为

- `TogawasakikoCard.BeforeCardPlayed` 只为正在结算的当前实例触发 Cast；涵盖所有继承该基类的 Skill/Power（含歌曲、事件技能和三张技能型压力衍生牌）。
- 使用原生 `CreatureCmd.TriggerAnim`，标准等待为角色的 0.25 秒；动作自身可继续播放并回到待机，无需等待整个 1.64 秒动作后才能出下一张牌。
- 攻击型压力衍生牌「满脑子都想着自己」使用 AttackCommand 的 Cast 动画替换，不额外播放 Attack；保留伤害、眩晕和荆棘。原有荆棘工具自动补足 0.06 秒，使标准命中仍在 0.31 秒；快速模式对应 0.125 + 0.03 = 0.155 秒。
- 四张压力衍生牌：人格解离、自闭、满脑子都想着自己、过劳焦虑。
- 原版牌沿用各自触发流程。实例筛选避免其他副本或队友重复触发；原生重放每次实际结算请求一次动作。

## 构建与安装身份

这是基于公开 0.2.4 的本地测试候选，manifest 仍显示 0.2.4，DLL 已更新。尚未推送或发布这一变更。
分支：`codex/cast-card-integration-20260921`；基线提交：`9e9c11694150c3b6c1154c4e465d46fe38e82b92`。

通过 `shared/scripts/build-mod.sh` 构建 DLL，复用与公开 0.2.4 完全相同的 PCK/manifest。角色动作、死亡、光环与荆棘资源不再重导入。
安装前确认游戏未运行，备份现有三件套；通过主工作区 `shared/scripts/install-mod.sh --apply --allow-overwrite` 安装。安装后目录严格只有三件套，逐件 SHA-256 与 staging 一致。

| 文件 | 安装后 SHA-256 |
| --- | --- |
| `Togawasakiko_in_Slay_the_Spire.dll` | `1b350ee945be379f4ef69726dc461fbf697b6f1047cca0ca2152194a9c3075dd` |
| `Togawasakiko_in_Slay_the_Spire.pck` | `0408ead3ce94cbbace2e6dd753a5888b68005896e38fb8faf37035cdc5d6b695` |
| `mod_manifest.json` | `1c2d0f3d2a890d5417feac345c1123fa7e2a9d6bcebcfc61e6d19459dc156bc9` |

## 验证

- 改动前回归：109 通过、8 失败，明确复现缺少 Cast 和攻击型压力牌仍使用 Attack。
- 改动后：117 通过、0 失败；新增 10 项覆盖真实原版出牌流程、施法命令、效果顺序、普通攻击、队友/副本隔离、Burst 重复和原版施法牌不重复。
- 安装 DLL 与上述测试使用 DLL 逐字节一致。
- 当前 Python 资源/兼容性/发布验证：28 通过。
- Mac 构建通过；Windows v0.107.1 引用构建 0 警告、0 错误。
- 独立代码复核无待修问题。动画探针只观察原生 TriggerAnim 命令，未替代原生方法；不能作为 Steam 画面已通过的证据。
- Steam 连续施法衔接、快速出牌观感和实际荆棘接触帧待用户确认；本轮没有启动 Steam，也没有更新 GitHub release。

安装与备份证据：[installation-evidence.json](/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev/local/cast-card-integration-20260921/installation-evidence.json)。
原安装备份：`/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev/local/cast-card-integration-20260921/installed-backup`。

## 用户后续实测

用户反馈「动画合理」，cast 实测已认可。随后要求从容应对改为 8／12 格挡，已安装的新候选见[数值调整审计](composed-response-balance-2026-09-21.md)。上方三件套身份记录的是此前通过动画实测的安装。
