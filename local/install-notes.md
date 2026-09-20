# 当前安装说明

记录日期：2026-07-26

## 安装目标

`install-mod.sh` 会从 `local/game-path.txt` 读取游戏根，并按平台推导 mods 根。

当前 macOS 目标：

```text
<GameRoot>/SlayTheSpire2.app/Contents/MacOS/mods/<ModId>/
```

Togawasakiko 的安装目录中只应存在：

```text
Togawasakiko_in_Slay_the_Spire.dll
Togawasakiko_in_Slay_the_Spire.pck
mod_manifest.json
```

## 安装前

1. 先完成 release build。
2. 确认 `exports/release/<ModId>/` 只有运行时成品。
3. 先运行默认 dry-run：

```bash
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire
```

## 替换安装

确认 dry-run 目标正确后：

```bash
./shared/scripts/install-mod.sh Togawasakiko_in_Slay_the_Spire --apply --replace-target
```

`--replace-target` 会在 mods 根目录安全边界内删除并重建该 mod 目录。不要手工把整个开发目录复制进去。

其他脚本参数：

- `--apply`：执行写入；默认仅 dry-run。
- `--create-mods-dir`：目标 mods 根不存在时允许创建。
- `--allow-overwrite`：保留目标目录并覆盖同名文件。
- `--replace-target`：删除并重建目标 mod 子目录。

## 安装后

1. 比对 release 与 installed 的 DLL、PCK、manifest SHA-256。
2. 从 Steam 正常启动游戏。
3. 检查最新 `godot.log` 的 mod discovery、assembly/PCK load 和 initializer。
4. 将“安装成功”和“玩法实机通过”分开记录。

旧候选路径草稿保留在：

- `local/archive/install-notes-initial-draft.md`
