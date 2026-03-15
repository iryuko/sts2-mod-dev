# 下一轮任务

## 本轮任务

围绕“干净版” `CrossCharacterCard` 做奖励池路径验证：

- 已安装新版 `CrossCharacterCard`
- 确认游戏是否正常加载该 mod
- 验证 `Silent` 正常战后奖励里是否会出现 `BodySlam`
- 验证选择后是否能正常进入牌组、抽到并打出
- 验证集中规则表写法是否保持与旧版同样的行为

## 预期输出物

- 一条加载结论：
  - `CrossCharacterCard` 是否被游戏识别
  - 是否出现 mod initializer 异常
- 一条功能结论：
  - `Silent` 是否能正常开新 run
  - 正常奖励里是否会出现 `BodySlam`
- 一条可用性结论：
  - 通过奖励拿到的 `BodySlam` 是否能正常抽到并打出
- 一条结构结论：
  - `CrossCharacterCard` 是否已经改成“集中规则表维护”的形态
- 文档更新：
  - `docs/current-status.md`
  - `docs/next-task.md`
  - `docs/thread-handoff.md`

## 新增支线

围绕 `SilentBonusRelic` 做第一轮 relic 注入验证：

- 确认游戏是否正常加载该 mod
- 用 `Silent` 开新 run
- 进入 run 后立刻查看 relic 栏
- 验证是否一开始就拥有：
  - `SneckoSkull`
  - `Shuriken`
- 如果仍失败，优先判断：
  - `RunStarted` 是否没命中
  - 还是 `AddRelicInternal(..., silent: true)` 没有带来可见效果

围绕 `UnifiedSavePath` 做跨平台实现整理：

- 确认当前源码版是否已具备：
  - Windows 走 Harmony patch
  - macOS / 非 Windows 走 flag-thread workaround
- 如果后续有 Windows 实机条件，再单独验证 Windows 分支是否保持原版行为

## 建议执行顺序

1. 启动游戏并进入 `Silent` 单人新 run
2. 正常推进到数场战斗奖励
3. 观察 `BodySlam` 是否作为正常奖励出现
4. 选择后确认它能正常进牌组、抽到并打出
5. 读取日志，确认 `CrossCharacterCard` 初始化是否成功且无异常
6. 单独观察 `SilentBonusRelic`：
   - 新 run 一开始是否就拥有 `SneckoSkull` 和 `Shuriken`
   - 是否出现新的异常日志

## 不要做的事情

- 不要一开始就扩展到多张牌、多角色配置。
- 不要引入 Harmony。
- 不要重新引入运行时直接塞牌逻辑，除非再次需要做诊断。
- 不要把“理论可行”写成“已实机确认”。
- 不要把 `SilentBonusRelic` 的新实现直接写成“已经成功”，直到拿到实机结果。

## 完成标准

- 能明确回答：
  - 首版 `CrossCharacterCard` 是否成功加载
  - `BodySlam` 是否已经通过卡池扩展稳定进入 `Silent` 正常奖励池
  - 该实现是否在不干扰开局流程的前提下可复用到更多跨角色卡
