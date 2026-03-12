# 最小可验证 Mod 成品规范

本文件描述“第一轮闭环验证”阶段，工作区中一个最小 STS2 mod 成品目录暂按什么结构组织。

注意：

- 这不是官方 schema 或最终发布规范。
- 这是为了先验证 `<GameRoot>/mods/` 这一路径是否能被识别。
- 状态：工作约定，可调整。

## 当前首要候选安装路径

- 安装根目录：`<GameRoot>/mods/`
- 安装目标目录：`<GameRoot>/mods/<ModName>/`
- 证据等级：高可能
- 状态：待本机验证

## 工作区中的最小安装候选目录

当前建议把可安装成品放在：

`mods/<ModName>/exports/release/<ModName>/`

这样可以把 `release` 根目录和“最终待安装包目录”分开。

这样脚本会把它们复制到：

`<GameRoot>/mods/<ModName>/`

## 优先候选文件

以下文件是当前第一轮验证优先准备的最小集合：

- `<ModName>.dll`
- `<ModName>.pck`
- `mod_manifest.json`
- `mod_image.png`（可选）

## 建议的最小 release 目录示例

```text
mods/<ModName>/exports/release/<ModName>/
├─ <ModName>.dll
├─ <ModName>.pck
├─ mod_manifest.json
└─ mod_image.png        # 可选
```

## 哪些属于源码目录，哪些才是安装候选目录

源码或工作文件，不应直接安装：

- `mods/<ModName>/src/`
- `mods/<ModName>/assets/`
- `mods/<ModName>/manifest/`
- `docs/`
- `references/`

当前安装候选目录：

- `mods/<ModName>/exports/release/<ModName>/`

## 当前约定的脚本行为

- `shared/scripts/install-mod.sh` 优先把 `exports/release/<ModName>/` 中的文件复制到 `<GameRoot>/mods/<ModName>/`
- 默认 dry-run
- 默认不覆盖同名已安装 mod 目录
- 如果 `<GameRoot>/mods/` 不存在，只在显式允许时安全创建

## 待验证

- 游戏是否要求 `<ModName>` 目录名与 manifest 中某个字段一致
- `.dll`、`.pck`、`mod_manifest.json` 是否都必需
- `mod_image.png` 是否只影响显示，不影响加载
