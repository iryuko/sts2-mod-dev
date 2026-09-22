# STS2 Mod 研究与开发工作区

本仓库用于研究《Slay the Spire 2》的原生 mod 链路，并维护本地 mod。它不是游戏安装目录。

## 当前主项目

当前唯一持续开发和发布的项目是：

- `mods/Togawasakiko_in_Slay_the_Spire`

其他 `mods/` 子目录主要是模板、兼容性研究、最小验证或旁支实验。除非任务明确指向它们，不应把这些项目误当成当前主线。

当前游戏与 API 参考基线：

- STS2 `v0.107.1`
- commit `59260271`
- 详情见 `references/api-notes/app/release_info.json`

## 卡牌设计工作台

[丰川祥子卡牌设计工作台](mods/Togawasakiko_in_Slay_the_Spire/research/card-graph/README.md)
提供卡牌关系图、悬浮卡面、草案编辑、卡图上传和关系审核。
它是独立的本机研究工具，不需要启动游戏，不生成游戏补丁；个人设计默认不上传 Git。
研究数据固定在 mod 0.2.3，不能视为后续游戏版本的最新卡面数值。启动前请按工具说明准备固定版本卡图。

## 目录职责

```text
sts2-mod-dev/
├─ mods/          # 各 mod 的源码、资源、文档和导出物
├─ shared/        # 共用构建、安装、打包和同步脚本
├─ references/    # 从游戏目录复制或提取的只读参考资料
├─ local/         # 当前机器路径、工具、临时分析和生成报告
├─ docs/          # 项目级当前结论、时间线、研究与档案导航
├─ images/        # 原版参考图与美术研究产物
├─ Tools/         # 独立工具及第三方工具文件
├─ AGENTS.md      # 工作规则
└─ README.md
```

目录边界：

- 游戏路径只从 `local/game-path.txt` 读取。
- 游戏目录只用于读取参考、安装成品和运行验证。
- 源码、实验脚本和文档不得直接放进 Steam 安装目录。
- 第三方工具自带的 README、LICENSE 和原版 patch notes 不属于本项目档案。

## 事实源顺序

接手当前任务时按以下顺序阅读：

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/thread-handoff.md`
5. `docs/decisions.md`
6. `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`

历史过程记录位于 `docs/archive/` 和角色目录下的 `docs/archive/`。历史文档用于追溯，不覆盖当前事实源。

## 标准工作流

构建：

```bash
./shared/scripts/build-mod.sh Togawasakiko_in_Slay_the_Spire --configuration Release
```

安装预检查：

```bash
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
```

确认后替换本机安装：

```bash
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
```

执行纪律：

1. 先同步当前游戏 DLL 和版本信息。
2. 先按原版实现核对 API、对象生命周期和资源契约。
3. build 完成后再 install。
4. 对 release 与安装目录的 DLL、PCK、manifest 做哈希比对。
5. “代码已改”“构建通过”“已安装”“实机通过”必须分别记录。

## 文档维护

- 当前状态文件只保留仍然有效的结论和开放问题。
- 过程型长日志使用日期命名，完成阶段后移入 archive。
- 旧版本 API 记录必须标出对应 STS2 版本。
- 发生结论反转时，保留旧记录并标注被哪个新证据取代，不直接篡改历史。
- 项目全程时间线见 `docs/project-timeline.md`。
