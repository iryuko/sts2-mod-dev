# 线程接班摘要

## 0.2.4 战斗动画与暗影荆棘（2026-09-21）

用户 Steam 实测反馈「测试没有问题，允许推送」，本轮采用的整套动作、死亡修边与光环滚落、暗影荆棘已获确认。[安装审计](audits/combat-animation-install-2026-09-21.md)保留实际通过测试的三件套身份。
0.2.4 的内容与验证见[发布说明](../mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-21-0.2.4.md)。发布前修复荆棘亚像素退化多边形，原生 3,543 组边界扫描通过；角色数据、贴图与玩法保持用户实测内容。
后续以用户新需求为准，不从下方旧状态自动恢复历史任务。共享主工作区的其他研究保持独立。


记录日期：2026-09-06

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
- mod manifest：`0.2.3`
- 当前本地安装与 `0.2.3` release 三件套哈希一致
- DLL：`42f223cad65812c3138b5bfb3b38a6ff7f32d1d92e2a00b491800fcc825aaad7`
- PCK：`c820289c7da963d42d9e4e66d100c94cee2c1a40cd6f911401e44b0f2251e193`
- manifest：`fc0c4410f295c5c749b86dcc03f7c6ac8679249b50543f207a9561feee9a0a26`
- 2026-07-26 Steam 启动：initializer 完成，主菜单无模组错误状态

## 最重要的校正

- jukebox 当前不是“进 combat 自动 Off”。
  - 源码意图是跨所有 room 持续播放。
  - 只在 `Off (null)` 或离开 run 时停止。
- merchant 不是纯原版 Silent fallback，也不是正式祥子 Spine。
  - 它是自定义兼容 scene，隐藏 Silent skeleton，显示静态祥子 portrait。
- `0.2.3` 继承当前战斗 Spine runtime；2026-09-06 的 bridge 卡图修复没有修改该部分。
- 五张 bridge 卡此前全部共用 `basic/unendurable.png`，不是只有“接续小节”错图。
  2026-09-06 已在 `0.2.3` 中改为五个独立 `1000x760` portrait，并进入安装 PCK；尚未启动游戏做视觉确认。
- Darv 目前还没有真正回归原版。
  - `Curseslander` 已解决 Ancient 空池。
  - 但 `DarvPatches.cs` 仍整体接管祥子选项生成，下一步应优先删除或证明必要性。
- T2 的“50 张正常卡”是历史规划。
  - 当前角色池共 50 张，其中 45 张是 Common/Uncommon/Rare。
- 退出 Godot 时的资源泄漏 `ERROR` 不等于 mod loader 见红。

## 当前断点

1. 五张 bridge 卡图已安装，待 Card Library 实机确认裁切和升级态。
2. `UnattendedPiano` SL 共享列表 bug 已改代码，待实机。
3. Win 卡牌卡中间的已知 API 漂移已修，待同包 Win 回归。
4. jukebox 换房保护已加，待 Win/Mac 生命周期回归。
5. Darv patch 应回归原版。
6. `MagneticForceHellWargodPower` 共享集合与 private `FieldRefAccess` 仍是静态风险。

## 必须遵守

- 先反编译当前版本原版对象，再改同类机制。
- 原版能完成的流程不另造一套。
- 不把 starter、token、event、Ancient 卡混进普通奖励。
- `Slugify(type.Name)`、本地化 key、资源文件名必须一致。
- 静态初始化不绑定高风险 private 反射。
- build、install、hash 校验必须串行。
- 不把“构建通过”写成“实机修复”。

## 标准命令

```bash
./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
```

完整历史见：

- `docs/project-timeline.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/development-timeline.md`
