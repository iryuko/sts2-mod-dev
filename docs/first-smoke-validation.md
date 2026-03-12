# First Smoke Validation

记录日期：2026-03-10

补充说明：

- 本文保留了早期验证历史。
- 其中关于“外部 `mod_manifest.json` 应直接安装到游戏目录”和“`<GameRoot>/mods/` 是本机首要路径”的部分，已经被后续更强证据部分覆盖。
- 当前应优先参考本文末尾新增的“基于示例项目与 IL 的重置结论”。

## 1. SmokeMod 的工作区源码目录

- `mods/SmokeMod/src/`
- `mods/SmokeMod/assets/`
- `mods/SmokeMod/manifest/`

这些目录当前只建立了骨架，还没有真实功能代码。

## 2. SmokeMod 的 release 成品目录

- `mods/SmokeMod/exports/release/SmokeMod/`

当前目录结构：

```text
mods/SmokeMod/exports/release/SmokeMod/
├─ SmokeMod.dll
├─ SmokeMod.pck
└─ mod_manifest.json
```

当前状态说明：

- `mod_manifest.json`：真实准备好的工作模板文件
- `SmokeMod.dll`：现已升级为真实 CLR 程序集，用于最小真实产物验证
- `SmokeMod.pck`：现已升级为真实 Godot PCK，用于最小真实资源包验证
- `mod_image.png`：本轮未提供，保持可选

## 3. 游戏中的目标安装目录

- 首要候选安装根目录：`<GameRoot>/mods/`
- SmokeMod 目标安装目录：`<GameRoot>/mods/SmokeMod/`
- 证据等级：高可能
- 状态：待本机验证

## 4. 本轮验证目标

本轮只验证以下四件事：

1. release 成品目录结构是否合理
2. `<GameRoot>/mods/SmokeMod/` 是否会被游戏扫描
3. `mod_manifest.json` 是否会被碰到
4. 游戏启动后是否出现任何最小反馈

## 5. 安装步骤

1. 先检查当前候选包：

```bash
./shared/scripts/install-mod.sh SmokeMod
```

2. 如果确认要继续做第一次本机验证，且 `<GameRoot>/mods/` 尚不存在：

```bash
./shared/scripts/install-mod.sh SmokeMod --apply --create-mods-dir
```

3. 如果 `<GameRoot>/mods/SmokeMod/` 已存在并且明确要覆盖同名文件：

```bash
./shared/scripts/install-mod.sh SmokeMod --apply --allow-overwrite
```

4. 安装后确认游戏目录中存在：

```text
<GameRoot>/mods/SmokeMod/
├─ SmokeMod.dll
├─ SmokeMod.pck
└─ mod_manifest.json
```

## 6. 启动游戏后应该观察哪些现象

- 游戏是否正常启动
- 是否出现任何与 mod、manifest、pck、load 相关的提示、报错或日志
- 游戏内是否出现与 modding 相关的列表、入口或条目变化
- 游戏目录下的 `<GameRoot>/mods/SmokeMod/` 是否保持完整，没有被游戏重写成异常结构

## 7. 哪些现象代表“路径大概率正确”

- 游戏日志或界面中出现对 `SmokeMod`、`mod_manifest.json`、`mods/` 的读取痕迹
- 游戏内出现与 SmokeMod 名称相关的条目、错误提示或识别反馈
- 调整 `<GameRoot>/mods/SmokeMod/` 中的内容后，游戏反馈随之变化

## 8. 哪些现象代表“manifest 或产物格式仍有问题”

- 游戏能启动，但完全没有任何关于 `SmokeMod` 或 `mods/` 的反馈
- 游戏明确报 manifest 解析失败
- 游戏明确报 `.dll` 或 `.pck` 文件无效
- 游戏能看到目录，但看不到条目或立即跳过该包

## 9. 本轮结论边界

- 即使 `<GameRoot>/mods/SmokeMod/` 被扫描，也不代表 manifest schema 已完全确认
- 即使游戏对 SmokeMod 有反馈，也不代表占位 `.dll/.pck` 可运行
- 第一次验证通过后，下一步应优先把占位 `.dll` 或 `.pck` 替换成至少一项真实产物

## 10. 当前本机结果

记录时间：2026-03-10

- 当前未观察到任何明确与 `SmokeMod`、`mods/`、`mod_manifest.json`、`pck` 相关的游戏内反馈。
- 当前本机日志文件：
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T00.48.43.log`
- 对上述日志做关键词检索，未发现 `SmokeMod`、`manifest`、`pck`、`mods/` 的命中记录。
- 该结论对应的是“真实安装前”的一次检查。

## 11. 当前安装状态

记录时间：2026-03-10

- 已执行真实安装命令：

```bash
./shared/scripts/install-mod.sh SmokeMod --apply --create-mods-dir
```

- 当前游戏目录中已存在：
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/`
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/`
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/SmokeMod.dll`
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/SmokeMod.pck`
  - `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SmokeMod/mod_manifest.json`
- 当前文件类型：
  - `SmokeMod.dll`: 真实 CLR 程序集
  - `SmokeMod.pck`: 真实 PCK 数据文件
  - `mod_manifest.json`: JSON data
- 当前已确认：
  - 游戏目录中的 `SmokeMod.dll` 与工作区导出 DLL 的 SHA-256 一致
  - 游戏目录中的 `SmokeMod.dll` 可见 `SmokeMod`、`Entry`、`BuildMarker`、`Initialize` 等字符串
  - `SmokeMod.pck` 头部为 `GDPC`
- 因此，接下来再次启动游戏时，才能把“没有反馈”解释为更接近扫描、manifest 或产物格式问题，而不是“根本没装进去”。

## 12. 当前附加线索

- 本机用户设置文件中存在 `mod_settings` 字段：
  - `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save`
- 当前该字段值为 `null`。
- 这只能说明游戏设置结构里至少预留了 mod 相关设置位，不能据此确认外部 mod 已被扫描或启用。

## 13. 当前 SmokeMod 一致性检查

- 目录名：`SmokeMod`
- `dll` 文件名：`SmokeMod.dll`
- `pck` 文件名：`SmokeMod.pck`
- `mod_manifest.json` 中的对应字段：
  - `"name": "SmokeMod"`
  - `"dll": "SmokeMod.dll"`
  - `"pck": "SmokeMod.pck"`
- 目前没有发现文件名引用不一致的问题。
- 当前唯一明确的问题是：
  - `SmokeMod.pck` 只是 ASCII 占位文本，不是真实资源包
- 当前已确认：
  - `SmokeMod.dll` 不再是文本占位，而是一个真实的最小 CLR 程序集
  - `SmokeMod.pck` 不再是文本占位，而是一个真实的最小 Godot 资源包
- `id` 字段当前为 `smoke-mod`，与目录名 `SmokeMod` 大小写和连字符形式不同。
- 这个差异目前只能记为“未验证是否敏感”，不能直接判定为错误。

## 14. 当前重点从路径切换到落盘痕迹

- 用户已手动测试过以下两个候选安装位置，但都没有明显反馈：
  - `<GameRoot>/mods`
  - `.app/Contents/Resources/mods`
- 该信息将当前重心从“继续猜路径”切换为“检查游戏是否留下 mod 加载、忽略或配置落盘痕迹”。

## 15. 下一次复测前的最短动作

1. 现在直接启动游戏。

2. 退出后立刻检查：

```bash
rg -n -i 'SmokeMod|mods_enabled|disabled_mods|mod_settings|manifest|pck|mods/' \
  "/Users/user/Library/Application Support/SlayTheSpire2/logs" \
  "/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log"
```

3. 如果仍然没有命中，再把问题缩小为两类：
   - 游戏对当前包静默忽略
   - 当前真实 `dll` 仍不足以触发反馈，或占位 `pck` 仍让整个包无效

## 16. 真实 DLL 复测结果

记录时间：2026-03-10

- 在将 `SmokeMod.dll` 升级为真实 CLR 程序集并重新安装到游戏目录后，又产生了新的启动日志：
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T04.04.27.log`
  - `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T05.46.18.log`
- 最新修改时间：
  - `godot.log`: `2026-03-10 05:46:45`
  - `settings.save`: `2026-03-10 05:46:44`
  - `remote/settings.save`: `2026-03-10 05:46:44`
  - `async.log`: `2026-03-10 05:46:19`
- 但复测后仍未发现以下关键词命中：
  - `SmokeMod`
  - `mods_enabled`
  - `disabled_mods`
  - `manifest`
  - `pck`
  - `mods/`
- `settings.save` 与 `remote/settings.save` 仍只有：
  - `"mod_settings": null`
  - `"schema_version": 4`
- `async.log` 仍为空文件。

当前最保守的判断：

- 即使已经换成真实 DLL，仍没有让游戏留下任何新的 mod 痕迹。
- 当前更值得优先怀疑的是：
  - 当时的占位 `SmokeMod.pck` 让整个包无效
  - 或者当前 manifest / DLL / PCK 组合仍不足以进入游戏的可记录加载路径

## 17. 真实 PCK 已补齐

记录时间：2026-03-10

- 使用官方 `Godot 4.5.1` headless 模式生成了新的 `SmokeMod.pck`。
- 当前工作区中的 `SmokeMod.pck` 类型为 `data`，不是文本文件。
- 当前工作区中的 `SmokeMod.pck` 文件头为：
  - `GDPC`
- 当前该 PCK 只打包了最小验证文件：
  - `smoke_marker.txt`
- 该 PCK 的意义是“验证一个真实 Godot 资源包是否会改变游戏痕迹”，不是宣称其内容结构已符合 STS2 最终要求。

## 18. 当前游戏目录附加观察

- 当前游戏目录中的 `SmokeMod` 目录除了安装候选文件外，还存在：
  - `.build/`
  - `src/`
  - `assets/`
  - `exports/`
  - `manifest/`
  - `README.md`
- 这些更像开发区样式内容，不应默认视为安装必需内容。
- 本轮只记录该状态，不自动删除任何游戏目录内容。

## 19. 基于示例项目与 IL 的重置结论

记录时间：2026-03-10

在参考 `lamali292/sts2_example_mod` 并结合本机 `sts2.dll` IL 后，当前更可靠的最小安装链路应重置为：

1. 在工作区中准备一个 Godot `pack/` 源目录。
2. 在构建前生成或复制 `pack/mod_manifest.json`。
3. 用 Godot 把整个 `pack/` 导出为 `<ModName>.pck`。
4. 把 `<ModName>.dll` 与 `<ModName>.pck` 安装到游戏扫描的 `mods` 目录中。

当前不再把“外部同目录 `mod_manifest.json`”视为最核心的安装产物。

## 20. 为什么这个重置很关键

- 示例项目构建后实际安装的只有：
  - `ExampleMod.dll`
  - `ExampleMod.pck`
- 示例项目的 manifest 是在 `pack/` 内生成的。
- 本机 IL 显示游戏读取的是：
  - `res://mod_manifest.json`

这两条证据拼在一起后，当前更合理的理解是：

- manifest 的正确位置首先是 PCK 内部。
- 外部目录结构只需要保证游戏能扫描到 `.pck`，并在同目录找到可选的同名 `.dll`。

## 21. 当前 SmokeMod 与这条链路的主要差异

当前 `SmokeMod` 还存在以下不一致：

- `SmokeMod.pck` 的打包源中没有 `mod_manifest.json`。
- 当前额外导出并安装了外部 `mod_manifest.json`，但 loader 当前读取的关键位置是 PCK 内部。
- 当前 `SmokeMod.dll` 与 `SmokeMod.pck` 的导出已经接近真实产物，但整体仍没有对齐“示例项目式的 pack 目录构建链路”。

## 22. 下一次复测前的最短动作

下一轮复测前，最短且最值钱的动作应该是：

1. 让 `SmokeMod` 在构建时生成/复制 `pack/mod_manifest.json`。
2. 重打 `SmokeMod.pck`，确认包内包含 `res://mod_manifest.json`。
3. 安装时先只关心：
   - `SmokeMod.dll`
   - `SmokeMod.pck`
4. 再按当前本机 macOS 的真实扫描路径做一次最小验证。

## 23. 新一轮复测结果：已进入扫描链，但被 warning 阻塞

记录时间：2026-03-10

本轮已把 `SmokeMod` 调整为更接近示例项目的最小结构：

- `pack/mod_manifest.json` 在构建时进入 PCK
- release 目录只保留：
  - `SmokeMod.dll`
  - `SmokeMod.pck`
- 安装路径改为：
  - `SlayTheSpire2.app/Contents/MacOS/mods/SmokeMod/`

启动游戏后，最新 `godot.log` 中首次出现了明确命中：

- `Found mod pck file .../Contents/MacOS/mods/SmokeMod/SmokeMod.pck`
- `Skipping loading mod SmokeMod.pck, user has not yet seen the mods warning`

## 24. 这次结果说明了什么

这次结果已经能确认：

- 本机 macOS 的本地扫描目录不是继续停留在“纯猜测”状态，而是已有日志级证据支持：
  - `Contents/MacOS/mods/`
- 当前 `SmokeMod.pck` 已经进入真实识别路径。
- 这次阻塞点已经从：
  - “路径可能不对”
  - “manifest 可能完全没被读到”

切换为：

- “用户尚未看到或确认 mods warning，所以游戏主动跳过加载”

## 25. 当前最保守的判断

到这一步，当前最保守且最可靠的判断是：

- 当前 SmokeMod 的“最小包结构”已经至少满足“被扫描到”的条件。
- 下一步最值钱的问题不再是继续改最小包目录，而是找出：
  - mod warning 触发/确认路径
  - 对应的设置状态如何落盘
