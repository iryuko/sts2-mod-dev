# STS2 Mod 开发工作区

这个仓库用于长期维护《Slay the Spire 2》mod 开发，不是游戏安装目录。

当前已经存在游戏本体，真实路径记录在 `local/game-path.txt`：

`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2`

这个路径属于 Steam 安装区。开发源码、实验文件、分析笔记和模板都应该放在本工作区中，不应直接混放到游戏目录里。游戏目录只用于运行、验证和安装构建后的成品。

## 工作区与游戏目录的关系

- 工作区负责：源码、素材、模板、导出产物、分析记录、安装脚本。
- 游戏根目录负责：Steam 管理的游戏文件、`.app` bundle、运行所需资源、测试时实际加载的已安装 mod。
- 两者之间的联系只通过 `local/game-path.txt` 和 `shared/scripts/` 下的脚本建立。
- 未来如果要从游戏目录提取 DLL、资源或其它参考文件，也应先复制到 `references/`，再在工作区中分析。

## 目录结构

```text
sts2-mod-dev/
├─ mods/
│  ├─ 00-template-mod/      # 新 mod 的模板
│  └─ scratch/              # 临时实验区
├─ shared/
│  ├─ templates/            # 共享模板文件
│  ├─ scripts/              # 与游戏目录交互的脚本
│  └─ docs/                 # 共享说明占位
├─ references/
│  ├─ game-dlls/            # 从游戏目录复制出的引用文件
│  ├─ api-notes/            # API 观察记录
│  └─ decompile-notes/      # 反编译与结构分析记录
├─ local/                   # 本机路径与安装相关说明
├─ docs/                    # 路线图、错误与实验记录
├─ README.md
├─ AGENTS.md
└─ .gitignore
```

## 基本工作流

1. 在 `mods/` 下创建或复制一个 mod 目录。
2. 在 `src/`、`assets/`、`manifest/` 中开发与整理内容。
3. 将构建输出放入 `exports/debug/` 或 `exports/release/`。
4. 通过 `shared/scripts/install-mod.sh` 把 `release` 成品安装到游戏目录中的目标位置。
5. 启动游戏并测试，再把发现记录回 `docs/` 或 `references/`。

## 为什么不能直接在游戏目录里开发

- Steam 更新或校验可能覆盖你的改动。
- macOS 下游戏是 `.app` bundle 结构，直接混放源码会让目录更难维护。
- 开发文件和运行文件混在一起后，回滚、打包、比对和多 mod 并行开发都会变得混乱。
- 从工程管理角度，工作区应该是可版本化、可清理、可复制的；游戏安装区不具备这个职责。

## 下一步最推荐做的事情

1. 先运行 `shared/scripts/sync-game-files.sh`，确认后续参考文件应从哪些已存在路径复制到 `references/`。
2. 把 `mods/00-template-mod/` 复制成你的第一个真实 mod 目录，并补齐自己的 `manifest` 与入口代码。
3. 在确认游戏内实际 mod 安装位置后，再完善 `install-mod.sh` 的目标子目录配置与打包流程。
