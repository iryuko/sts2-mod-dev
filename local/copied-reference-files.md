# 已复制参考文件记录

记录日期：2026-03-10

本次复制通过 `shared/scripts/sync-game-files.sh` 执行。
原则：

- 只从游戏目录复制少量关键参考文件到工作区。
- 不修改游戏本体目录。
- 大文件或不适合直接纳入参考区的文件只记录路径，不复制。

## 已复制文件

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Info.plist`
  - 目标：`references/api-notes/app/Info.plist`
  - 大小：`1.8K`
  - SHA-256：`b5f596217a0648aedaccd1a601975c2d7e52cd32406f7666301961988f3c2e42`

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/release_info.json`
  - 目标：`references/api-notes/app/release_info.json`
  - 大小：`112B`
  - SHA-256：`abd1cfcc2327940c6d2ec95e371c50f07455b670980782e68d6daa7b437dda6c`

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.runtimeconfig.json`
  - 目标：`references/api-notes/sts2/sts2.runtimeconfig.arm64.json`
  - 大小：`357B`
  - SHA-256：`bd81a252bc3f0e8bcb649b404c1da945913c5377b9d3a2c416c52730be8714c0`
  - 备注：与游戏目录中的 `x86_64` 版本哈希一致，因此当前只保留一份。

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.deps.json`
  - 目标：`references/api-notes/sts2/sts2.deps.arm64.json`
  - 大小：`35K`
  - SHA-256：`b5a9c3d643343e4a3f3e8d80eb94656ce0caf0cc1a2783dd4f013d9d287b9178`

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.deps.json`
  - 目标：`references/api-notes/sts2/sts2.deps.x86_64.json`
  - 大小：`34K`
  - SHA-256：`22532ab453dbb10db2ceb67210bc4f3d4cc2ca896eaf4986df066b2811a39375`

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/0Harmony.dll`
  - 目标：`references/game-dlls/shared/0Harmony.dll`
  - 大小：`2.0M`
  - SHA-256：`c9ff9059eee3bf09a96b14c06d0fa712e4e277c991f108e6b93e0bf3f180a55b`
  - 备注：与游戏目录中的 `x86_64` 版本哈希一致，因此当前只保留一份。

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/GodotSharp.dll`
  - 目标：`references/game-dlls/shared/GodotSharp.dll`
  - 大小：`5.4M`
  - SHA-256：`0e4897ecdfb31456a97c7d8028dfb8d7dbdc632e2f73fc9b438d7b266a139289`
  - 备注：与游戏目录中的 `x86_64` 版本哈希一致，因此当前只保留一份。

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll`
  - 目标：`references/game-dlls/sts2/arm64/sts2.dll`
  - 大小：`8.5M`
  - SHA-256：`472f59b79c4bdaa113b5a0403d9a39720e951e088963c4280df7d48ad8acd1da`

- 源：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll`
  - 目标：`references/game-dlls/sts2/x86_64/sts2.dll`
  - 大小：`8.5M`
  - SHA-256：`111085954248e3d10b0fc8029dfcd7add65802f075c3b9d4eb96e0b02220674a`

## 仅记录路径，未复制

- `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/Slay the Spire 2`
  - 原因：主运行二进制，体积较大，当前阶段不作为首批参考文件复制。

- `/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck`
  - 原因：主资源包，约 `1.5G`，不适合直接纳入工作区参考目录。
