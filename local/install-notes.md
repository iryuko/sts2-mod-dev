# 安装说明草稿

这份文档用于约束“如何把 mod 成品安装到游戏目录”。

当前安装路径判断已更新为：

- 首要候选：`<GameRoot>/mods/`
- 证据等级：高可能
- 状态：待本机验证

这里的“高可能”来自当前抢先体验期的外部实测信息；它足以作为本机第一验证目标，但还不应写成“已完全确认的最终官方规则”。

## 基本原则

- 游戏根目录：`/Users/user/Library/Application Support/Steam/steamapps/common/Slay the Spire 2`
- 这是 Steam 安装目录，只用于运行游戏和放置最终成品。
- 安装时只复制构建产物，不复制源码、模板、草稿文档或临时脚本。
- 源码目录 `src/`、素材源文件、实验记录都应保留在工作区。

## 当前首要候选安装路径

- 候选安装根目录：`<GameRoot>/mods/`
- 候选 mod 目录：`<GameRoot>/mods/<ModName>/`
- 本机实测状态：当前 `mods/` 目录尚不存在
- 当前处理原则：
  - 默认按这个路径做 dry-run 预检查
  - 只有显式允许时，脚本才会安全创建 `mods/` 目录
  - 默认不覆盖同名已安装 mod 目录

## 通常属于成品的文件类型

以下仅表示常见候选，不表示已经验证为 STS2 的唯一格式：

- 编译后的 `.dll`
- 打包后的 `.pck`
- 安装所需的 `manifest` 或同类清单文件
- 运行时必须随 mod 一起分发的资源文件

## 推荐安装流程

1. 在工作区中完成构建，把候选成品放入某个 mod 的 `exports/release/`。
2. 检查 `exports/release/` 中是否只包含需要安装的文件。
3. 优先按 `<GameRoot>/mods/<ModName>/` 执行 dry-run 预检查。
4. 如果 `<GameRoot>/mods/` 不存在，只在明确要继续验证时，显式允许脚本安全创建。
5. 通过 `shared/scripts/install-mod.sh --apply` 执行真正复制。
6. 启动游戏，验证这一路径是否被实际识别。

## 安装前检查

- `local/game-path.txt` 路径是否正确。
- 游戏根目录是否存在。
- `<GameRoot>/mods/` 是否已存在；若不存在，是否准备按首要候选路径创建。
- 本次安装的是否为 `release` 产物，而不是 `src/` 或 `assets/` 原始文件。
- `<GameRoot>/mods/<ModName>/` 是否已经存在旧版本文件。
- 是否需要先备份已安装的旧成品。

## 安装后检查

- 游戏目录中是否只新增了本次需要的成品文件。
- `<GameRoot>/mods/<ModName>/` 目录层级是否符合预期。
- 目录层级是否保持完整，没有把整个源码目录复制进去。
- 未出现危险覆盖或无关文件污染。
- 测试完成后，把结果与异常记录到 `docs/findings.md` 或 `docs/errors.md`。
