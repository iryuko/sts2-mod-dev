# Togawasakiko in Slay the Spire

这是丰川祥子自定义角色 mod 的主开发目录，也是当前仓库唯一持续维护和发布的完整项目。

## 当前基线

- mod version：`0.2.5`
- minimum game version：`0.107.1`
- 当前 API 参考：STS2 `v0.107.1` / `59260271`
- 当前版本：技能、能力与压力衍生牌接入施法动作；从容应对为 8／12 格挡。用户 Steam 实测通过。

角色已经完成：

- build、PCK 导出和原生三件套安装。
- 选人、进局和战斗角色接入。
- 50 张角色卡池牌。
- 压力、衍生牌、song 与多组 power。
- starter、Ancient 和特殊 relic。
- 普通事件 `UnattendedPiano`。
- Ancient 事件 `TogawaTeiji`。
- jukebox、角色选择、静态战斗立绘、商店、火堆和 Ancient 资源。

继承 0.2.4 的战斗 Spine、光环滚落与暗影荆棘，详情见 [0.2.5 说明](docs/releases/2026-09-21-0.2.5.md)。

这不是“第一次最小闭环”阶段。当前不要继续按旧 T4 README 扩内容。

## 先读

1. `docs/current-status.md`
2. `docs/index.md`
3. `docs/development-timeline.md`
4. `docs/asset-status.md`
5. 仓库级 `docs/next-task.md`

T3/T4/T5 的长篇过程记录已经移入 `docs/archive/phase-logs/`，只在追溯具体 bug 时阅读。

## 目录

- `src/`：C# 模型、事件、patch、jukebox 和兼容辅助。
- `manifest/`：外部 loader manifest 的源文件。
- `pack/`：PCK runtime staging 与 Godot scene。
- `assets/`：正式源资产库存。
- `incoming_assets/`：来稿、原图和生产中素材。
- `docs/`：当前事实、设计规范、资产资料、审计和历史档案。
- `exports/release/`：当前展开成品与保留的回归基线包。
- `tests/`：测试留档入口。

## 构建与安装

```bash
./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
```

从仓库根目录运行。build、install、hash 校验必须串行。

## 开发准则

- 原版有可复用流程时，先读当前版本原版实现，再做最小扩展。
- 不复制原版选项表、状态机或私有字段作为长期方案。
- 代码已改、构建通过、安装成功和实机通过分别记录。
- starter、token、event、relic-granted、Ancient 与普通奖励池必须明确分离。
- 不在当前稳定性回归结束前扩新卡或新机制。
