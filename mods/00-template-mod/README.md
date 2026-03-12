# 00-template-mod

这是一个模板 mod，用来复制出新的 mod 目录结构，不代表已经接入真实的 STS2 加载接口。

## 目录用途

- `src/`：放源码目录；当前模板 mod 里只保留空目录，占位入口模板位于 `shared/templates/entry.cs`。
- `assets/`：放 mod 自己的资源文件或待打包素材。
- `manifest/`：放清单、配置或安装说明草稿；占位清单位于 `shared/templates/mod_manifest.json`。
- `exports/`：放构建输出。
- `exports/debug/`：调试阶段产物。
- `exports/release/`：未来安装进游戏的候选成品目录。

## 如何从模板复制新 mod

1. 复制整个 `mods/00-template-mod/`。
2. 把目录名改成你的 mod 名称。
3. 从 `shared/templates/` 复制需要的占位模板到新 mod 目录。
4. 根据已验证的信息补齐入口代码、`manifest/` 和导出规则。

## 安装边界

- `release` 才是未来进入安装流程的候选目录。
- `src/`、`assets/`、`manifest/` 原始内容本身不直接安装进游戏。
- 安装动作应通过 `shared/scripts/install-mod.sh` 执行，不要手工把整个 mod 目录丢进游戏安装区。
