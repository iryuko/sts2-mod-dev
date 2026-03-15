# Cross Character Card Plan

日期：2026-03-11

## 首个最小验证组合

- 来源角色：`Ironclad`
- 目标角色：`Silent`
- 目标卡牌：`BodySlam`

## 选择理由

- `BodySlam` 是现有游戏卡，不需要新资源。
- 识别度高，进入奖励时很容易一眼确认。
- 它不是 `Silent` 的起始卡，混入后信号清晰。
- 作为单卡验证目标，失败时排查面最小。

## 最小实现方案

1. 新建独立 mod 子项目：
   - `mods/CrossCharacterCard/`
2. 使用已确认可加载的最小结构：
   - `src/*.cs`
   - `manifest/mod_manifest.json`
   - `pack/pack_pck.gd`
3. 在 mod 初始化入口执行：
   - `ModHelper.AddModelToPool<SilentCardPool, BodySlam>()`
4. 构建 release 产物：
   - `CrossCharacterCard.dll`
   - `CrossCharacterCard.pck`
5. 用现有安装脚本进行安装验证。

## 成功标准

### 已确认即可判定成功

- 游戏能正常识别并加载 `CrossCharacterCard`
- 没有 mod initializer 异常

### 实机验证成功标准

- 使用 `Silent` 开新 run
- 在正常战后卡牌奖励中，能看到 `BodySlam`
- 选择后进入战斗，`BodySlam` 可以正常出现在手牌并可打出

## 当前不做

- 不做多张牌批量注入
- 不做 UI 菜单
- 不做新资源
- 不做多角色配置化
- 不做 Harmony patch
