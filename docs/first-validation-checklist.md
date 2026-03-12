# 首轮验证 Checklist

目标：建立“最小可验证闭环”，验证 `<GameRoot>/mods/` 是否是本机有效的首要安装路径。

## 一、准备最小产物

需要准备的最小候选文件：

- `<ModName>.dll`
- `<ModName>.pck`
- `mod_manifest.json`
- `mod_image.png`（可选）

工作区中的建议位置：

- `mods/<ModName>/exports/release/<ModName>/`

安装前确认：

- `exports/release/<ModName>/` 中只放最终候选成品
- 不把 `src/`、`assets/`、草稿 manifest、笔记文件带进游戏目录

## 二、执行安装预检查

1. 确认 `local/game-path.txt` 指向正确的游戏根目录。
2. 执行：

```bash
./shared/scripts/install-mod.sh <ModName>
```

3. 观察脚本输出：

- 是否正确解析到 `<GameRoot>/mods/<ModName>/`
- 是否提示 `<GameRoot>/mods/` 当前不存在
- 是否正确列出将复制的 release 文件

## 三、如需继续验证路径，执行真实复制

如果确认要按首要候选路径继续验证，并且 `<GameRoot>/mods/` 尚不存在：

```bash
./shared/scripts/install-mod.sh <ModName> --apply --create-mods-dir
```

如果 `<GameRoot>/mods/<ModName>/` 已存在，且明确要覆盖同名文件：

```bash
./shared/scripts/install-mod.sh <ModName> --apply --allow-overwrite
```

注意：

- 默认不覆盖已存在同名 mod 目录
- 即便允许覆盖，也不会删除目标目录中多余的旧文件

## 四、启动游戏后优先观察的现象

优先观察这些最小现象：

- 游戏是否正常启动，没有因为安装路径错误立即崩溃
- 游戏内是否出现与 modding 相关的界面入口或列表变化
- 游戏日志、报错弹窗或界面提示中是否提到 manifest、pck、mod、load 等关键词
- 游戏目录下的 `<GameRoot>/mods/<ModName>/` 是否保持完整，没有被游戏重写成异常结构

## 五、哪些结果代表“安装路径基本正确”

以下结果可视为“安装路径基本正确”的强信号：

- 游戏明确识别到该 mod 目录或 manifest
- 游戏内出现 mod 名称、mod 图片、mod 条目或相关提示
- 日志中出现对 `<GameRoot>/mods/` 或当前 mod 目录的读取记录
- 仅调整安装内容后，游戏反馈随之变化

以下结果只能说明“还不够确认”：

- 游戏能正常启动，但完全没有任何 mod 相关反馈
- 目录存在，但没有证据表明游戏扫描了它

## 六、本轮不应直接下的结论

- 不能因为游戏没有立刻崩溃，就断定 `<GameRoot>/mods/` 一定正确
- 不能因为目录创建成功，就断定 manifest 结构已经正确
- 不能因为游戏界面里出现 modding 相关词汇，就断定成品格式已经完全匹配
