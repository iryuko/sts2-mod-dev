# 已定决策

记录日期：2026-07-26

本文件只记录当前仍有效、后续不应反复推翻的规则。

## 仓库边界

- 本仓库是研究与开发工作区，不是游戏安装目录。
- 游戏根路径只从 `local/game-path.txt` 读取。
- Steam 目录只用于读取参考、安装成品和运行验证。
- 游戏文件先复制到 `references/`，再分析。
- 不把源码、实验脚本、文档或整个 mod 开发目录复制进游戏安装区。

## 当前安装模型

- macOS 安装根：
  - `SlayTheSpire2.app/Contents/MacOS/mods/`
- 当前原生 loader 成品是三件套：
  - `<ModId>.dll`
  - `<ModId>.pck`
  - 外部 `mod_manifest.json`
- 外部 manifest 不是可省略文件。
- manifest 至少要与当前 loader schema 对齐；Togawasakiko 当前声明：
  - `version: 0.2.1`
  - `min_game_version: 0.107.1`
  - `dependencies: []`
- 构建脚本生成外部 manifest 时必须保留 `min_game_version`。

## 版本纪律

- 当前 API 事实源是 STS2 `v0.107.1` / `59260271`。
- `references/pck-extract/sts2-main` 来自旧版 `v0.98.3`，只可用于未被新版证据推翻的资源结构研究。
- 每份 API/scene 结论必须写明参考版本。
- Windows 与 macOS 问题先比较游戏版本和 DLL，不先假定是操作系统差异。

## 原版对齐

- 原版存在可复用流程时，以当前版本原版实现为基础。
- 修改前先反编译或反射同类原版卡、事件、relic、power 或 scene。
- 不复制会随版本变化的原版选项表、状态机或私有字段，除非有不可替代的明确原因。
- 自定义补丁应只补缺失接口，不重写整个原版流程。
- 当前因此应重新审计并尽量删除 `DarvPatches.cs`。

## Togawasakiko 行为

- 压力是目标身上的共享 counter，不按祥子玩家分账。
- 压力衍生牌是 token，不进入普通奖励、商店或 transform 池。
- `Curseslander` 生成的两张压力牌仅新实例本场费用为 0，不改变 canonical 或其他来源的同名牌。
- jukebox 当前目标是跨所有 room 持续播放，包括 combat。
- jukebox 只在选择 `Off (null)` 或离开 run 时停止并恢复原版 BGM。
- 能量计数器缺少两个 VFX 节点是当前明确接受的视觉缺陷，不作为稳定性修复项。

## 构建与验证

- 顺序固定为：
  1. 同步当前参考。
  2. 修改源码/资源/文档。
  3. build。
  4. install。
  5. release/install 哈希比对。
  6. 实机测试。
- 不并行执行 build 与 install。
- 不把旧 zip 文件名、旧日志或旧哈希当作当前安装证据。

## 文档分层

- `current-status.md`：当前事实。
- `next-task.md`：下一轮执行顺序。
- `thread-handoff.md`：压缩接班摘要。
- `decisions.md`：稳定规则。
- `project-timeline.md` / `development-timeline.md`：历史脉络。
- `research/`：可复用研究。
- `audits/`：有明确日期和证据边界的专项核查。
- `archive/`：已被后续状态覆盖的过程记录。

历史文档原文不因过时而删除；应归档并标明不再代表当前状态。
