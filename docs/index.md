# 文档导航

本页用于降低新线程接手成本。默认先读少量结论文档，不要先通读整个 `docs/`。

## 必读

- `docs/current-status.md`
  - 当前阶段、当前目标、已确认与阻塞。
- `docs/next-task.md`
  - 下一轮最值得做的单一任务。
- `docs/thread-handoff.md`
  - 给下一位 Codex 的高压缩摘要。
- `docs/decisions.md`
  - 已定规则与不再反复讨论的决策。

## 按需

- `docs/findings.md`
  - 较完整的实验记录与观察结果。
- `docs/loading-clues.md`
  - 与加载规则相关的证据整理。
- `docs/log-and-userdata-hunt.md`
  - 日志与用户数据路径追踪。
- `docs/reference-example-mod.md`
  - 外部示例项目吸收结论。
- `docs/class-map.md`
  - 类型名、命名空间与元数据线索。
- `docs/dll-metadata-notes.md`
  - DLL 与运行时元数据证据。

## 旧记录 / 档案

- `docs/archive/`
  - 归档区。默认不作为新线程必读内容。
- 旧 checklist、早期 smoke 验证记录、已被新结论覆盖的过程文档
  - 仅在需要追溯证据或核对历史变化时再读。

## 新线程推荐阅读顺序

1. `AGENTS.md`
2. `docs/current-status.md`
3. `docs/next-task.md`
4. `docs/thread-handoff.md`
5. `docs/decisions.md`
6. 如仍有缺口，再读 `docs/findings.md` 或具体专题文件

## 使用原则

- 历史日志和旧研究记录默认不作为新线程必读内容。
- 新线程应优先读结论文档，而不是通读整个 `docs/`。
- 只有当 `docs/current-status.md` 和 `docs/next-task.md` 无法支撑当前任务时，才去下钻旧文档、原始日志或引用资料。
