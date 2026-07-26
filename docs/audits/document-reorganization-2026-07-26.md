# 文档档案整编审计

日期：2026-07-26

## 目标

本轮从工作区结构、Git 历史、文件日期、当前源码、release 哈希和本机安装状态重新建立文档秩序。重点不是删除旧记录，而是解决三个问题：

1. “当前事实”散落在数月追加日志中。
2. 历史规划、已实现状态和过期结论混用“当前”。
3. 项目加载链研究与 Togawasakiko 主项目互相挤占接班入口。

## 档案边界

纳入项目档案：

- 根目录、`docs/`、`local/`、`shared/` 中的项目自有说明。
- `mods/` 下各 mod 的设计、实现、测试和发布记录。
- 由本项目形成的反编译结论、专项审计和研究报告。

不作为项目档案校订对象：

- `references/` 中官方提取文本、反编译产物和第三方仓库自带 README/LICENSE。
- `local/tools/` 中第三方工具文档。
- 构建生成的 `obj/`、`bin/`、Godot import metadata 和外部依赖说明。

这些文件可以作为证据，但不能与项目自己的现行结论文档混为一类。

## 事实裁定顺序

出现冲突时采用：

1. 当前源码、manifest、scene 和实际文件。
2. 当前游戏版本反编译结果。
3. release、安装包和 SHA-256。
4. 实机测试记录。
5. 当前状态与专项审计。
6. 历史阶段日志和早期设计稿。

“构建成功”只证明静态构建完成，不自动等于实机通过。

## 新结构

项目级入口：

- `README.md`
- `docs/current-status.md`
- `docs/next-task.md`
- `docs/thread-handoff.md`
- `docs/decisions.md`
- `docs/project-timeline.md`
- `docs/index.md`

Togawasakiko 入口：

- `mods/Togawasakiko_in_Slay_the_Spire/docs/current-status.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/development-timeline.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/asset-status.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/open-questions.md`
- `mods/Togawasakiko_in_Slay_the_Spire/docs/index.md`

历史记录：

- `docs/archive/loading-chain/`：10 份 2026-03-10 初期加载链研究。
- `docs/archive/side-projects/`：5 份非当前主线研究。
- `mods/Togawasakiko_in_Slay_the_Spire/docs/archive/phase-logs/`：15 份 T3/T4/T5 追加日志、旧卡牌规格、旧资产表、战斗立绘实验和版本审计。
- `local/archive/`：初期路径候选和参考文件复制记录。

研究、模板和审计分别进入：

- `docs/research/`
- `docs/templates/`
- `docs/audits/`

## 时间线裁定

- 工作区加载链研究起于 2026-03-10。
- Git 历史起于 2026-03-11。
- Togawasakiko 文件最早形成于 2026-03-24。
- Togawasakiko 首次完整 Git 提交是 2026-04-23。
- 两者不矛盾：角色项目前一个月的工作在首次提交时整体纳入版本控制。
- 当前游戏参考版本为 v0.107.1，commit `59260271`。

完整阶段记录见 `docs/project-timeline.md` 和角色目录内的 `development-timeline.md`。

## 已裁定的主要矛盾

### 安装与 manifest

- 初期 `<GameRoot>/mods/` 只是候选路径，已归档。
- 当前 macOS 安装目录是 `.app/Contents/MacOS/mods/`。
- 当前 loader 契约需要 DLL、PCK 和外部 manifest 三件套。

### 卡池与 starter

- T2 的 `12 Common / 22 Uncommon / 16 Rare = 50` 是扩展规划。
- 当前实现是角色池总计 50 张：Basic 4、Common 14、Uncommon 23、Rare 8、Ancient 1。
- `Slander` 当前为每层压力 +2；升级后 0 费。
- `Unendurable` 当前为 8 格挡/3 压力；升级为 11 格挡/4 压力。

### 事件与 Ancient

- `UnattendedPiano` 是普通 Event，不是 Ancient。
- Teiji 当前允许 run 中有祥子的混合多人队伍进入，其他角色使用 agnostic dialogue。
- Teiji 的主图、节点、头像和背景已经进入 runtime；旧“prototype 尚未接入”状态已失效。
- `Curseslander` 已提供合法 Ancient 卡候选，但 `DarvPatches.cs` 仍接管原版选项流程，是未完成的回归任务。

### jukebox 与资产

- “进入 combat 自动 Off”是早期方案；当前目标是跨所有 room 持续播放。
- 当前已有 23 首 jukebox 曲目、1 首事件音乐和 1 个角色选择音效。
- 当前全部卡牌 model 均有对应 portrait；旧“50 张 normal 卡图待补”清单已归档。

### 原版参考版本

- `references/pck-extract/sts2-main/` 来自 v0.98.3，只能作为资源结构参考。
- v0.107.1 API 判断必须使用当前 DLL/反编译结果，不能从旧 PCK 提取物外推。

## 保留的未决事项

本轮只整理文书，没有用文档结论掩盖尚未闭环的问题：

- Darv patch 回归原版。
- `UnattendedPiano` 同进程 SL 实机验证。
- Windows 卡牌卡中间与 jukebox 换房同包回归。
- mutable 集合共享和 static private `FieldRefAccess` 风险。
- 主流程系统回归。

这些事项已统一收口到项目和角色的 `current-status.md`、`next-task.md` 与 `open-questions.md`。
