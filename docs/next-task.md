# 下一轮任务

## 本轮任务

围绕新的主线推进三件事：

- 确认游戏内是否存在可见的 mod 列表 / mod 管理界面 / 已加载 mod 查看入口。
- 维持“游戏目录安装版 SmokeMod 与工作区发布版一致”的状态，避免后续测试基线漂移。
- 继续定位当前存档问题，但不要再重复试当前公开版 `UnifiedSavePath`；先分析它在 macOS 上的 Harmony patch 失败原因。

## 预期输出物

- 一条明确结论：
  - 游戏内是否有内置 mod 列表或 mod 管理界面
  - 当前已观察到的入口和证据是什么
- 一条一致性结论：
  - 已安装版 SmokeMod 与工作区发布版是否一致
  - 如果不一致，已按安装成功版本反向修正了哪些发布产物
- 一次存档回归测试结论：
  - 删除安装版 SmokeMod 后，目标存档是否恢复正常
- 一条候选 mod 评估结论：
  - `UnifiedSavePath` 在 macOS 上属于哪一档
  - 关键证据和风险点是什么
- 一条失败根因结论：
  - 为什么 `UnifiedSavePath` 在 macOS 上初始化失败
  - 是否能通过改 patch 策略规避
- 对应文档更新：
  - `docs/current-status.md`
  - `docs/next-task.md`
  - `docs/thread-handoff.md`
  - `docs/unified-save-path-macos-check.md`

## 不要做的事情

- 不要重新回到“mod 是否被识别”这一已跨过的问题。
- 不要继续扩展 SmokeMod 功能或引入新的实验变量。
- 不要先通读全部旧日志或全部 `docs/`。
- 不要把尚未在游戏内看到的 mod 菜单行为写成事实。
- 不要在未备份真实存档前直接试装会改存档路由的 mod。
- 不要重复安装当前这个已确认在 macOS 上初始化失败的 `UnifiedSavePath` 发布版。
- 不要先讨论复杂 API、Harmony patch 或大规模 mod 框架。

## 完成标准

- 能把“有无内置 mod 列表 / 管理界面”收敛到明确结论或极小候选范围。
- 能明确说明安装版 SmokeMod 与工作区发布版是否一致。
- 能对 `UnifiedSavePath` 给出基于本地证据的明确档位判断。
- 能把 `UnifiedSavePath` 的 macOS 失败点收敛到可解释的技术原因。
- 能给出一次面向存档问题的人工回归验证建议或结果，而不是只停留在文件分析。
- 当前项目状态文档已更新到足以让下一线程直接接手，不必重新翻旧记录。
