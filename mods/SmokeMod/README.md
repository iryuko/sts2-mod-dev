# SmokeMod

这是首轮“最小空壳安装包验证”专用的 smoke mod。

当前目标：

- 验证 `exports/release/SmokeMod/` 的成品结构是否合理
- 验证 `<GameRoot>/mods/SmokeMod/` 是否会被游戏扫描
- 验证 `mod_manifest.json` 是否会被碰到
- 记录最小反馈，不宣称已确认最终加载规则

当前文件状态：

- `src/SmokeMod.csproj`：最小真实 .NET 类库项目
- `src/Entry.cs`：最小真实程序集源码，不宣称已接入 STS2 已确认入口
- `manifest/mod_manifest.json`：当前工作清单源文件
- `exports/release/SmokeMod/mod_manifest.json`：由清单源同步出的安装候选文件
- `exports/release/SmokeMod/SmokeMod.dll`：已可由 `./shared/scripts/build-mod.sh SmokeMod` 生成真实程序集
- `exports/release/SmokeMod/SmokeMod.pck`：占位文件，不是可运行资源包
- `exports/release/SmokeMod/mod_image.png`：当前未提供，保持可选
