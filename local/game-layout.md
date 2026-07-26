# 当前游戏目录布局

记录日期：2026-07-26

## 游戏根

实际路径由 `local/game-path.txt` 提供，当前为 macOS Steam 安装：

```text
/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2
```

该目录不是开发工作区。

## macOS 关键路径

```text
Slay the Spire 2/
└─ SlayTheSpire2.app/
   └─ Contents/
      ├─ MacOS/
      │  ├─ Slay the Spire 2
      │  └─ mods/
      ├─ Resources/
      │  ├─ Slay the Spire 2.pck
      │  ├─ release_info.json
      │  ├─ data_sts2_macos_arm64/
      │  └─ data_sts2_macos_x86_64/
      └─ Frameworks/
```

当前 mod 安装根已经实机确认：

```text
SlayTheSpire2.app/Contents/MacOS/mods/
```

每个 mod 使用独立子目录，只安装运行时三件套。

## 当前参考版本

- STS2 `v0.107.1`
- commit `59260271`
- `.NET 9`
- GodotSharp 4.5.1
- Harmony 2.4.2

版本事实源：

- `references/api-notes/app/release_info.json`
- `references/game-dlls/sts2/arm64/sts2.dll`
- `references/game-dlls/sts2/x86_64/sts2.dll`

Windows 参考位于：

- `references/windows-sts2/`

## 用户数据

macOS 日志与本地化 override 位于：

```text
~/Library/Application Support/SlayTheSpire2/
├─ logs/
└─ localization_override/
```

## 操作边界

- Steam 安装区只读参考文件、安装最终成品和运行游戏。
- 反编译结果、实验工程、缓存、源码和文档留在工作区。
- 与游戏目录交互优先使用 `shared/scripts/`。
- 旧版目录巡检原文见 `local/archive/game-layout-initial-2026-03-10.md`。
