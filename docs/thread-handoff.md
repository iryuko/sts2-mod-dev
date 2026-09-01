# 线程接班摘要

记录日期：2026-09-01

## 一句话状态

当前唯一主项目是 Togawasakiko。它已经是完整可构建、可安装、可进局的角色 mod；当前工作是 v0.107.1 双端兼容与高风险流程回归，不是从零实现角色。

## 先读

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/decisions.md`
5. `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`
6. `mods/Togawasakiko_in_Slay_the_Spire/docs/index.md`

不要先读 T4/T5 长日志。它们已归档，只在追溯具体 bug 时下钻。

## 当前版本

- STS2：`v0.107.1` / `59260271`
- mod manifest：`0.2.1`
- 兼容性分支基线：PR #4 `8b5e17ee`
- 本分支未安装、打包或发布；`v0.2.2` 留给独立发布工作树
- 2026-07-26 本机安装与 release 三件套哈希一致，但不包含本轮源码修复
- 21 项标准库测试通过；Mac/Windows 参考程序集构建均为 0 warning、0 error
- 2026-07-26 Steam 启动：initializer 完成，主菜单无模组错误状态

## 最重要的校正

- jukebox 当前不是“进 combat 自动 Off”。
  - 源码意图是跨所有 room 持续播放。
  - 只在 `Off (null)` 或离开 run 时停止。
- merchant 不是纯原版 Silent fallback，也不是正式祥子 Spine。
  - 它是自定义兼容 scene，隐藏 Silent skeleton，显示静态祥子 portrait。
- 战斗 Spine 正在独立制作，本轮 PR 继续保留静态战斗立绘基线。
- Darv 已删除模组覆盖，回归原版 `GenerateInitialOptions()` 与 `DustyTome`；双端 runtime 待验证。
- 7 月联机修复已经成为受跟踪源码：全玩家迁移、托管 hook task、唯一 Pressure watcher、mutable replay 状态和原版遗物入牌链均已落地。
- Card Library 与 Teiji 的 private field 访问已惰性化；缺 Card Library 字段只会缺少祥子图鉴筛选按钮。
- T2 的“50 张正常卡”是历史规划。
  - 当前角色池共 50 张，其中 45 张是 Common/Uncommon/Rare。
- 退出 Godot 时的资源泄漏 `ERROR` 不等于 mod loader 见红。

## 当前断点

1. 在独立发布工作树集成并生成 `v0.2.2`，通过 release staging identity gate。
2. `UnattendedPiano` SL 共享列表 bug 已改代码，待实机。
3. Win 卡牌卡中间的已知 API 漂移已修，待同包 Win 回归。
4. jukebox 换房保护已加，待 Win/Mac 生命周期回归。
5. Teiji、Aroma、人物动作与双玩家关键联机流程均待 runtime 证据。

## 必须遵守

- 先反编译当前版本原版对象，再改同类机制。
- 原版能完成的流程不另造一套。
- 不把 starter、token、event、Ancient 卡混进普通奖励。
- `Slugify(type.Name)`、本地化 key、资源文件名必须一致。
- 静态初始化不绑定高风险 private 反射。
- build、install、hash 校验必须串行。
- 不把“构建通过”写成“实机修复”。
- 能量计数器缺 VFX 是已接受视觉缺陷，不重新列入 bug 清单。

## 标准命令

```bash
./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
python3 mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py --help
python3 mods/Togawasakiko_in_Slay_the_Spire/scripts/validate-release-staging.py --help
```

完整历史见：

- `docs/project-timeline.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/development-timeline.md`
