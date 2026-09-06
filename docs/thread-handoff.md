# 线程接班摘要

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
- mod manifest：`0.2.2`
- 当前本地 post-release 热修安装与 release 三件套哈希一致
- DLL：`53c6bce1e932df648b63ebfb65c2000f11bc47547ac1914c1f3630ffb48da709`
- PCK：`829e3b68f10acf37b184aceb99382399efcc780c92eaf828b25fe79addbd4047`
- manifest：`3d06dd0b6f26e0ecdf36196a275d8f8f6f969cb18174ab8c0fecef6fbf38f624`
- 2026-07-26 Steam 启动：initializer 完成，主菜单无模组错误状态

## 最重要的校正

- jukebox 当前不是“进 combat 自动 Off”。
  - 源码意图是跨所有 room 持续播放。
  - 只在 `Off (null)` 或离开 run 时停止。
- merchant 不是纯原版 Silent fallback，也不是正式祥子 Spine。
  - 它是自定义兼容 scene，隐藏 Silent skeleton，显示静态祥子 portrait。
- `0.2.2` 已包含当前战斗 Spine runtime；2026-09-06 的 bridge 卡图热修没有修改该部分。
- 五张 bridge 卡此前全部共用 `basic/unendurable.png`，不是只有“接续小节”错图。
  2026-09-06 已在本地热修中改为五个独立 `1000x760` portrait，并进入安装 PCK；尚未启动游戏做视觉确认。
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
