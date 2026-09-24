# GitHub 上传记录

日期：2026-09-22。用户批准将工具放入现有 `iryuko/sts2-mod-dev` 仓库，以独立分支提交 PR。

## 范围

- 分支：`codex/card-design-workbench-20260922`，基于远端 main `9fdfd4e6`。
- 从本地 `ca9dea9f` 搬入工具的独立提交，仅包含本研究目录；仓库 README 增加入口。
- 游戏源码、资产、pack、manifest 和发布版本保持 main 原样，不制作游戏 release。
- 个人workspace、上传原图、备份、服务token、日志、虚拟环境与浏览器测试截图不上传。
- 原共享工作区及正在使用的本机服务不迁移、不停止，个人设计继续留在原目录。

## 迁移修正

启动说明不再依赖 Codex 的本机Python路径，改为虚拟环境与固定Pillow依赖。
浏览器测试支持PATH中的python3或显式PYTHON。编辑服务使用POSIX文件锁，原生Windows未适配，需WSL。
固定版本源码/卡图仍按配置检出到 `.worktrees/bridge-card-art-fix-20260906`；不是独立HTML下载，也不是GitHub Pages部署。

新检出环境首次 `build_graph.py --check` 失败。调查发现旧source_hashes混入3个被Git忽略的obj生成文件，
它们只存在于原机器的编译目录。新增失败回归后，将指纹来源改为Git跟踪的C#文件并重新生成结果。
graph.json/graph-data.js仅移除3个obj指纹，节点、边、证据、数值、基线版本和机制档案未改。
研究基线仍为0.2.3，不宣称覆盖随后0.2.4/0.2.5游戏变化；这次不是草案schema迁移。

## 验证

- 独立干净基线检出、全新Python虚拟环境及Pillow依赖安装完成。
- 36项Python测试通过，包含新增的“生成C#文件不影响源码指纹”回归。
- 2项Node模块与4项浏览器回归通过；PYTHON指定新虚拟环境。
- file模式图谱与HTTP工作台端到端套件通过，覆盖1440/768/390宽度；HTTP套件另验证PATH中的python3。
- 新检出服务的72张卡图全部HTTP 200。
- `build_graph.py --check`通过，仍为72张已实现牌、3个固定提案、827条关系。
- 对main的游戏源码、资产、pack、manifest差异为空；工具内常见凭证/私钥模式扫描无命中。

测试针对研究工具，不等同于游戏内实测或所有操作系统实测。原完整实现和审查记录见
[workbench-implementation-report.md](workbench-implementation-report.md)。
