# 参考项目：lamali292/sts2_example_mod

记录日期：2026-03-10

## 目的

本文件只吸收这个示例项目的：

- 项目结构
- 构建产物结构
- 安装路径和安装方式
- 与游戏目录的连接方式

不在这里展开它的复杂功能逻辑。

## 仓库结构

该项目当前根目录主要包含：

- `ExampleMod.csproj`
- `ModEntry.cs`
- `Patches/`
- `Relics/`
- `pack/`
- `local.props.example`

它没有把源码再套进 `src/`，而是把：

- C# 源码放在项目根和功能子目录
- Godot 打包源放在 `pack/`

## 与游戏目录的连接方式

该项目通过 `local.props` 注入本机路径：

- `STS2GamePath`
- `GodotExePath`

然后在 `ExampleMod.csproj` 中派生：

- `ModsOutputDir = $(STS2GamePath)\\mods\\$(ModName)`
- `GameDataDir = $(STS2GamePath)\\data_sts2_windows_x86_64`

这说明它把：

- 游戏 DLL 引用路径
- 最终安装目录
- Godot 可执行文件路径

都纳入了项目构建配置，而不是手工到处改路径。

## 构建链路

构建前：

- 自动生成 `pack/project.godot`
- 自动生成 `pack/mod_manifest.json`

构建后：

- 把 `ExampleMod.dll` 复制到 `$(ModsOutputDir)`
- 调用 Godot `--export-pack` 生成 `$(ModName).pck` 到同一目录

## 安装后的目录结构

该项目当前的最终安装结构等价于：

```text
<GameRoot>/mods/ExampleMod/
├─ ExampleMod.dll
└─ ExampleMod.pck
```

注意：

- 示例项目没有把外部 `mod_manifest.json` 作为最终安装文件复制到 `mods/ExampleMod/`
- 这强烈说明 manifest 是构建输入的一部分，而不是最终外部安装产物的核心组成

## manifest 观察

该项目当前生成的 `pack/mod_manifest.json` 字段为：

- `pck_name`
- `name`
- `author`
- `version`

这与当前本机 `sts2.dll` IL 中看到的 `ModManifest` 字段高度一致，只少了一个可选的 `description`。

## 对我们自己的启发

最值得吸收的不是它的补丁逻辑，而是下面这条最小安装链路：

1. 通过项目配置知道游戏路径和 Godot 路径。
2. 在 `pack/` 内生成 Godot 项目文件和 manifest。
3. 构建 DLL。
4. 导出 PCK。
5. 最终安装外部产物只保留 DLL 和 PCK。

## 与当前 SmokeMod 的核心差异

当前 `SmokeMod` 与这个示例项目相比，最大的差异不是“功能太简单”，而是安装链路没有完全对齐：

- `SmokeMod.pck` 里还没有 `mod_manifest.json`
- 当前仍把外部 `mod_manifest.json` 当作核心安装产物
- 当前构建脚本和安装脚本还没有把“生成 manifest -> 打进 PCK -> 安装 DLL/PCK”固化成默认路径

## 备注

该示例项目当前按 Windows 路径组织：

- `$(STS2GamePath)\\mods\\$(ModName)`

这个路径不能直接覆盖我们当前 macOS 本机路径判断。

对我们而言，更合理的吸收方式是：

- 吸收它的“构建产物组织方式”和“自动安装方式”
- 本地路径则继续以本机 IL 和实际运行环境为准
