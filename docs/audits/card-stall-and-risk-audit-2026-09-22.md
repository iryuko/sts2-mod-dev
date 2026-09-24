# 卡牌悬空与同类风险审计（2026-09-22）

## 基线与交付边界

- 用户反馈：打出「接续小节」后，后续卡牌停在空中。
- 本轮源码基于已发布 0.2.5 提交 `f3a3e4c29d9adb247a4eeba319cb7f55e5501bf8`，不是共享主工作区里的旧分支。
- 修复分支：`codex/card-stall-fix-20260922`。
- 工作树：`/Users/user/.codex/worktrees/card-stall-fix-20260922/sts2-mod-dev`。
- 游戏/API：`v0.107.1` / `59260271`。保留的 Mac 与 Win DLL 不是同一二进制，但属于同一游戏版本。
- 修复阶段只改 C# 逻辑、测试和文档。2026-09-24 用户追加发布授权后更新内外版本号为 0.2.6，重新打包 PCK；卡图、Spine、GDScript 特效和玩家存档未变。0.2.6 已发布，PR #10 尚未合并，本机未安装。

本机反馈时安装的 DLL SHA-256 为 `118fd49f60f77e0df998ec679c7ce667c6c931c93de25fb35a6b7fc5681f9209`，与 0.2.5 发布 DLL 一致；PCK/manifest 仍是先前通过用户测试的 0.2.4 标记候选。这是标签差异，不是本次异常的原因。

## 推送前复核（2026-09-24）

后续发布状态：用户另行明确要求发 Release；[0.2.6](https://github.com/iryuko/sts2-mod-dev/releases/tag/togawasakiko-v0.2.6-20260924) 已于 2026-09-24 12:46:45 UTC 正式发布。Tag 指向 `d8c1e7c6`，构建源码为 `8dc924c1`。571 项 PCK 内容只有内嵌版本标签变化，最终 ZIP 三件套与本地 staging 一致，两个上传资产的 GitHub SHA-256 核对通过。完整制品身份与验收边界见[发布说明](../../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-24-0.2.6.md)。以下是此前仅推送代码阶段的历史复核记录。

- 用户明确允许推送。先快进同步远端 main `b2f70818`，保留 PR #9 卡牌设计工作台；它未修改本轮生产代码。
- 代码提交：`f66d1c38`；[PR #10](https://github.com/iryuko/sts2-mod-dev/pull/10) 目标为 main。只推送功能分支，不合并、不发布 Release、不改本机安装；另一任务的百科卡牌显示候选不在本 PR 中。
- 新一轮验证：147 项玩法回归、28 项 Python 检查通过；Mac/Win v0.107.1 API 构建均 0 警告、0 错误。
- 原生检查首次复跑的 11 项行为均通过，但退出出现 5 个 AudioStreamPlaybackWAV 和一个测试 WAV 残留，脚本正确返回失败。详细日志明确指向 `res://piano_fixture.tres`，不是游戏音频资源。
- 对照 [Godot 4.5.1 AudioServer](https://github.com/godotengine/godot/blob/4.5.1-stable/servers/audio_server.cpp) 的停止、淡出和延迟释放流程，移除 jukebox 测试夹具固定 50ms 等待，改为逐帧确认原生引用只剩夹具自身并设 2 秒超时。该检查不强制减引用、不屏蔽告警，也不修改生产代码；随后 11 项原生检查连续复跑 5 次全部通过，无 ERROR 或资源泄漏 WARNING。
- 推送前验证构建 DLL SHA-256：`ceefff201bb147fb3faf5ed20ad565d2bdd208e8d58e5e679314d649be719047`，仅记录这次本地构建，不是发布资产或当前安装身份。9 月 22 日候选哈希保留在下方历史验证记录。
- 本次日志：`local/push-20260924-gameplay.log`、`local/push-20260924-api.log`、`local/push-20260924-native-verbose.log`、`local/push-20260924-native-repeat-{1..5}.log`。原始日志、参考 DLL、缓存和构建产物未提交。

## 本次卡死的直接证据

原日志保存在本工作树 `references/bug-evidence/2026-09-22-card-stall/godot.log`，SHA-256：
`aee84aebce4a36d0081e202d46419dcd8c5a6a5ba1c0d3961ed7ff6b84db7f84`。
原始日志只作本地证据，不应不经检查直接上传公开仓库。

1. 327、329、334 行：之前的中伤/打击成功打出。
2. 343 行开始卸载资源；357 行同进程继续游戏。
3. 596 行接续小节；随后振作、防御、她在发光、防御继续正常执行。
4. 609 行中伤、740 行打击才报错。两次异常都是 `ObjectDisposedException: Godot.PackedScene`。
5. 堆栈均进入 `TogawasakikoCombatVfx.CreateThornNodeGroup -> PackedScene.Instantiate`，随后使 `AttackCommand.Execute` 和原版打牌 action 异常结束，未正常完成清场。

因此日志不支持“接续小节破坏了出牌逻辑”；时间相邻不等于因果。直接故障是攻击特效持有已释放场景。新增接续小节接普通攻击的原版 wrapper 回归也通过。

## 已复现并修复

| 优先级 | 问题 | 原版流程修复 | 复现证据 |
| --- | --- | --- | --- |
| P1 | 荆棘的静态 PackedScene 跨 SL 存活，但原版 AssetCache 已 Dispose 它 | 删除模组永久缓存，每次使用 `PreloadManager.Cache.GetScene` | 实际 Godot .NET 第二次加载断言失败；改后连续 3 轮加载、实例化、原版卸载、重载通过 |
| P1 | 弹琴音频静态引用跨 SL 存活；读取 ResourcePath 即抛异常，可能阻断事件翻页 | 删除静态 AudioStream，使用 `PreloadManager.Cache.GetAsset<AudioStream>` | 原生宿主第二次播放抛 ObjectDisposedException；改后 2 轮通过 |
| P1/P2 | 磁力在 AfterCardPlayed 内嵌套 AutoPlay；Spiral 重复附魔可再次进入递归，X 支付被重采样，强制消耗会落入弃牌堆 | 使用原版 `ModifyCardPlayCount`，每张己方攻击额外 1 次，回合末原版移除能力 | 修前 4 项失败：Spiral 超过 3 次、与 One Two Punch 乘叠、X=3 少打一次、强制消耗变弃牌；修后通过 |
| P2 | GetEnemyCreatures 把所有活着的 Monster 当敌人，包含玩家侧奥斯提及不可命中单位 | 统一复用原版 `CombatState.HittableEnemies` | 真正的原版奥斯提被 Innocence 上自闭、被 KillKiss 伤害；另有不可命中复活敌人误选。修前 5 项失败，修后通过 |
| P2 | Ave Mujica 用手动 CanPlay 的费用条件判断免费自动打出，费用不足就错误抽进手牌 | 保留原版 CanPlay 返回的限制，只排除能量/星星不足；打出仍走原版 AutoPlayFromDrawPile | 3 能量时 4 费防御修前不打出；修后自动打出且不扣能量，Unplayable 仍抽进手牌 |

磁力保留原设计的回合内持续效果；重复获得能力不会把每次额外打出的数量乘上 Amount。原版负责重复的目标处理、费用信息和最终去向，不再维护自定义防递归集合。原版次数流程保留当前目标，不沿用旧模组在目标死亡后自行随机选新目标的行为。

## 第二轮：剩余四项已修

用户随后要求一起修完。以下四项均补了有效失败测试并修正；它们不是这次原始打牌日志中的异常，不再仅以静态推测登记。

1. **P2：弹琴异常离场残留音乐。** 原生测试执行真正的 EventModel.BeginEvent/EventOption.Chosen，然后移除事件节点；旧实现仍播放。现在播放器归属原版 `EventModel.Node`，不再挂全局根节点，也不再保留静态播放器。Godot 在场景离开时停止并释放它；正常结束才恢复原版音乐。正常结束、场景移除、缓存卸载重载、旧事件延迟清理不误停新事件均有测试。
2. **P2：远端弹琴选择串扰本机展示。** 旧实现的三个回归失败：远端弹琴改图/播歌、远端离开停本地歌、无本地身份仍执行展示。现按原版 Trial 使用 `LocalContext.IsMe(Owner)`，只围住画面/音频，不限制事件逻辑。测试仍执行真实选项、三次扣血、三张 Shadow 入拥有者牌组，并验证新 mutable 事件能重新开始完整路线。
3. **P2：jukebox 音量设置叠声。** 在真实 Godot 宿主执行原版 NBgmVolumeSlider 和 NAudioManager，仅以记录代理替换外部 FMOD 端点；旧实现把 25% 滑块转换为 0.0625 实际 BGM 输出，打破静音。新 `JukeboxBgmVolumePatch` 仅过滤原版 SetBgmVol 的输入参数，静音期间传 0，保存设置与原版音量换算均不改。Off/离开 run 先释放静音标记，再通过原版 API 恢复用户最新设置；无每帧轮询、无递归调用、无自建设置存储。五项测试涵盖 Off、连续滑块变更、恢复新音量、重复进出和其他音量独立性。
4. **P3：Shadow 获得楼层丢失。** 三种 Shadow 的原版反序列化与实际 SanitizeShadowCards 回归都复现了 floor 0 变 null。重建时现同时复制 `FloorAddedToDeck` 与 `CombatsSeen`。六项测试覆盖三种卡、三名拥有者、0/非零/null 楼层、重复迁移、牌组位置和实例/动态变量隔离。

当前版本的 C# `NAudioManager` 提供 SetBgmVol，但没有独立 BGM 静音公共接口，因此 jukebox 只在这个原版调用边界过滤参数，不复制音频管理器或改写设置流程。

执行过程与分工见[收口计划](../superpowers/plans/2026-09-22-stability-closeout.md)。九类已识别问题的代码修复齐备，不等于已经安装或完成双端实机验收。

代码扫描涉及卡牌、能力、遗物、事件、压力兑换、生成/选择/自动打出、Harmony 接入、存档迁移、资源和音频生命周期。重点对照当前 DLL，不以旧反编译目录直接认定新版 API。没有发现能将接续小节认定为直接根因的证据。

这不是“全 mod 无 bug”的证明。特别是实际 Win 进程、双机传输、完整事件 UI、最后敌人死亡后的奖励衔接仍需实机。已接受的能量计数器 VFX 缺口与火堆静态图方案不属于本轮修复。

额外审查：角色选择音效也有静态缓存，但当前未找到该自定义 OGG 进入原版预加载/卸载链的证据，不能把它定为已复现的同类 bug，本轮不改。Ave Mujica 纯星星不足/限制叠加、磁力首击致死现已补测试；后者直接与原版 One Two Punch 对照，保持原目标而非自行随机转移。

## 验证

- `bash shared/scripts/test-togawasakiko-gameplay.sh`：**147 passed, 0 failed**。使用真实游戏逻辑、Hook、牌堆和原版 TestMode，不运行完整 UI/联机传输。
- `bash shared/scripts/test-togawasakiko-resources.sh`：实际 Godot 4.5.1 .NET + .NET 9 进程；**11 项场景/事件音频/jukebox 行为检查通过**。不是伪造 Godot 指针的托管测试。Godot 加载、实例化、播放和释放真实执行；音频使用静音夹具，原版音乐恢复命令与外部 FMOD 端点在宿主中被观察/替换，不等于真实声卡听测。脚本同时拒绝超时无结果、ERROR 和资源泄漏 WARNING。
- Python 现有测试：**28/28**。磁力原本强制要求 HashSet 的旧源码检查已替换成原版次数钩子约束，并由上述行为回归补足。
- `verify-api-compatibility.py`：Mac arm64 与 Windows x86_64 的 v0.107.1 引用均 **0 警告、0 错误**；仅证明 API 编译兼容，不等于双端实机测试。
- `git diff --check`：通过。
- 第二轮只读交叉复核：未发现本轮弹琴、jukebox 修复引入的重要回归。测试宿主的脱树临时 Node 已改为显式 Free；最终原生测试无 ERROR 或资源泄漏 WARNING。
- 本轮 Release 构建 DLL：`d93472b95b07a5653cab643e4dfe3872aec6b6262f3ba305104c2560dcd87c14`。它仍是未发布候选，不能只凭旧 manifest 版本认定安装状态。

9 月 22 日第二轮验证输出在本工作树 `local/stability-round2-gameplay-final.log`、`local/stability-round2-native-final.log`、`local/stability-round2-api-final.log`；9 月 24 日最新复核见上节。`card-stall-*.log` 是第一轮证据，不代表第二轮最终制品。旧的无 Godot 原生宿主尝试未运行到业务断言，不计为红灯证据；有效红灯来自实际 Godot 宿主以及原版玩法回归。

## 实机回归顺序

1. 后续安装候选时从此隔离工作树构建，先校验并备份本机三件套；不要从共享旧源码打包覆盖 0.2.5。
2. 同进程先打一次荆棘攻击，保存退出主菜单，再继续；打出接续小节、中伤、打击，确认手牌/Play 区清理及日志无异常。
3. 磁力搭配 Spiral、X 攻击、强制消耗；与死灵同队，确认群体效果不伤奥斯提。
4. 弹琴后退出/继续，再弹琴；验证主菜单无残留钢琴，两名玩家的选择不串扰本地画面或音频。jukebox 播放时调整 BGM 滑块，确认不叠声，Off 后恢复新音量。
5. Win 使用同一候选制品做上述流程并带回完整日志和 DLL/PCK/manifest 哈希，再决定发布。
