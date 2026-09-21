# 从容应对格挡调整（2026-09-21）

用户反馈 cast「动画合理」，要求「从容应对」基础格挡由 6 改为 8，升级后由 8 改为 12。
已在当前 cast 分支上修改基础 BlockVar 为 8、升级增量为 4；费用仍为 1、移除目标至多 5 压力，施法接入保持。

已有回归更新到新数值，覆盖 0/1/4/5/8 压力、敏捷 +2 后实际格挡 10/14、卡面预览以及中英文升级/降级文本。
先验证旧数值导致 12 项预期失败，再修改源码；最终 117 通过、0 失败。未新增测试用例或修改本地化模板。
通过共享 build/install 脚本构建和安装，安装 DLL 与通过回归的 DLL 逐字节一致。游戏未运行；旧的、已获动画实测认可的 DLL 已备份。
PCK 和 manifest 与上轮完全相同；manifest 显示 0.2.4，本地候选尚未推送或另发 release。

安装 DLL SHA-256：`118fd49f60f77e0df998ec679c7ce667c6c931c93de25fb35a6b7fc5681f9209`。
安装时间：`2026-09-21T22:35:34.602016+08:00`。
详细三件套/备份身份：[installation-evidence.json](/Users/user/.codex/worktrees/combat-spine-attack-study/sts2-mod-dev/local/composed-response-balance-20260921/installation-evidence.json)。
用户随后反馈「测试合格」，本候选已通过 Steam 实测，并按既定流程准备 0.2.5 发布。
