# Log And Userdata Hunt

记录日期：2026-03-10

## 目标

本轮不再优先猜测 mod 安装目录，而是只读查找游戏是否在日志、用户数据、配置或缓存中留下任何与 mod 加载、忽略或配置落盘有关的痕迹。

## 已确认的候选路径

### 高优先级

- `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log`
- `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T00.48.43.log`
- `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T02.21.00.log`
- `/Users/user/Library/Application Support/SlayTheSpire2/logs/godot2026-03-10T02.27.25.log`
- `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save`
- `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save.backup`
- `/Users/user/Library/Application Support/Steam/userdata/355081496/2868840/remote/settings.save`
- `/Users/user/Library/Application Support/Steam/userdata/355081496/2868840/remotecache.vdf`

### 次优先级

- `/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log`
- `/Users/user/Library/Caches/SentryCrash/Slay the Spire 2/Data/CrashState.json`
- `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/profile.save`
- `/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/profile1/saves/prefs.save`

### 低优先级

- `/Users/user/Library/Application Support/SlayTheSpire2/shader_cache/`
- `/Users/user/Library/Application Support/SlayTheSpire2/vulkan/`

这些路径更可能因为正常启动产生噪声，不适合作为第一轮 mod 痕迹判断依据。

## 只读检索结果

### 已确认

- 在本机 `settings.save` 和 Steam remote `settings.save` 中，都能看到：
  - `"mod_settings": null`
  - `"schema_version": 4`
- 在本机 `profile.save` 中可见：
  - `"schema_version": 2`
- 在本机 `prefs.save` 中可见：
  - `"schema_version": 2`
- `CrashState.json` 仅记录崩溃计数与 session 计数，没有 mod 相关字段。
- `async.log` 当前为空。

### 未发现

- 未在本机日志、`settings.save`、Steam remote `settings.save`、`remotecache.vdf`、`profile.save`、`prefs.save` 中发现以下关键词落盘：
  - `SmokeMod`
  - `mods_enabled`
  - `disabled_mods`
  - `pck_name`
  - `manifest`

### 现阶段能说到哪一步

- 已确认：游戏设置结构中至少存在 `mod_settings` 这个槽位。
- 未确认：这个槽位是否会在外部 mod 被识别后发生变化。
- 未确认：游戏是否会把“忽略某个无效 mod 包”的结果写入日志或配置。

## 启动前 / 启动后最小比对方案

### 启动前记录

先记录这些文件的修改时间和关键词命中情况：

```bash
stat -f '%Sm %N' -t '%Y-%m-%d %H:%M:%S' \
  "/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log" \
  "/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save" \
  "/Users/user/Library/Application Support/Steam/userdata/355081496/2868840/remote/settings.save" \
  "/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log" \
  "/Users/user/Library/Caches/SentryCrash/Slay the Spire 2/Data/CrashState.json"
```

```bash
rg -n -i 'SmokeMod|mods_enabled|disabled_mods|mod_settings|manifest|pck' \
  "/Users/user/Library/Application Support/SlayTheSpire2/logs" \
  "/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log"
```

### 启动游戏一次并退出

- 保持当前 `SmokeMod` 包不变。
- 启动游戏，进入主菜单后退出。

### 启动后比对

重点比较：

1. 是否生成新的 `godotYYYY-MM-DDTHH.MM.SS.log`
2. `godot.log` 是否更新
3. 本机 `settings.save` 是否更新
4. Steam remote `settings.save` 是否更新
5. `async.log` 或 `CrashState.json` 是否更新

复查命令：

```bash
stat -f '%Sm %N' -t '%Y-%m-%d %H:%M:%S' \
  "/Users/user/Library/Application Support/SlayTheSpire2/logs/godot.log" \
  "/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save" \
  "/Users/user/Library/Application Support/Steam/userdata/355081496/2868840/remote/settings.save" \
  "/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log" \
  "/Users/user/Library/Caches/SentryCrash/Slay the Spire 2/Data/CrashState.json"
```

```bash
rg -n -i 'SmokeMod|mods_enabled|disabled_mods|mod_settings|manifest|pck|mods/' \
  "/Users/user/Library/Application Support/SlayTheSpire2/logs" \
  "/Users/user/Library/Caches/com.megacrit.SlayTheSpire2/async.log"
```

```bash
strings "/Users/user/Library/Application Support/SlayTheSpire2/steam/76561198315347224/settings.save" | \
  rg -n -i 'SmokeMod|mods_enabled|disabled_mods|mod_settings|manifest|pck|schema_version'
```

## 当前判断

- 如果启动后只看到普通日志和普通设置写回，而没有任何 mod 关键词新增，更像是：
  - 包无效
  - 或者游戏对无效包静默忽略
- 这比“纯路径问题”更值得优先怀疑，因为用户已手动测试过两个候选安装位置：
  - `<GameRoot>/mods`
  - `.app/Contents/Resources/mods`
- 该外部信息目前记为“用户手动测试结果”，不是我本轮独立验证结果。
