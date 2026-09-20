# 原版先古之民资产分类与规格

本文档回答的问题是：如果要在原版 STS2 环境下新增一个原创先古之民，最少需要准备哪些资产分类，这些资产在原版里以什么形式存在，以及原版现有资源的实际尺寸大致是什么。

## 结论先行

做一个原创先古之民，最小闭环需要 4 类图像资产加 1 类文本资产：

1. 事件主立绘
2. 地图节点头像
3. 地图节点头像 outline
4. 对话头像
5. 对话头像 outline
6. 文本资产：`title`、`epithet`、对话文本、选项文本

原版共享、无需每个先古之民单独制作的部分包括：

- 事件整体布局
- 名字条 UI
- 对话气泡和对话尾巴
- 选项按钮 UI
- 地图节点交互框体和选中 reticle

## 资产分类

### A. 事件主立绘

这是先古之民事件页中最主要的角色图。

- 原版对应路径：`images/ancients/<entry>_placeholder.png`
- 原版引用场景：
  - [nonupeipe.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/nonupeipe.tscn)
  - [darv.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/darv.tscn)
  - [orobas.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/orobas.tscn)
- 形式：
  - `Nonupeipe / Darv / Orobas / Pael / Tanx / Vakuu` 这一组是单张主图加可选场景特效。
  - `Neow / Tezcatara` 更接近 Spine / atlas 组合，不是单张静态图。
- 已导出原版实际尺寸：
  - `Darv Placeholder.png` `2560x1200`
  - `Nonupeipe Placeholder.png` `2560x1200`
  - `Orobas Placeholder.png` `2560x1200`
  - `Pael Placeholder.png` `2560x1200`
  - `Tanx Placeholder.png` `2560x1200`
  - `Vakuu Placeholder.png` `2560x1200`
- 场景显示框：
  - `Nonupeipe` 背景图在场景中被放进约 `2582x1220` 的 `TextureRect`
  - `Darv` 背景图在场景中被放进约 `2582x1221` 的 `TextureRect`

结论：

- 如果不追 `Neow / Tezcatara` 那种动态效果，原创先古之民最稳的主立绘规格就是透明底或普通底 `PNG`，尺寸按 `2560x1200` 准备。
- 原版允许在主图之上再叠粒子、光效和前景层，但这不是最小必需项。

### B. 地图节点头像

这是地图上先古之民节点本体的小图标。

- 原版对应路径：`images/packed/map/ancients/ancient_node_<entry>.png`
- 原版使用场景：[ancient_map_point.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_map_point.tscn)
- 形式：单独 `Texture2D`
- 原版已导出实际尺寸：
  - `Darv Node.png` `203x205`
  - `Neow Node.png` `204x166`
  - `Nonupeipe Node.png` `278x278`
  - `Orobas Node.png` `185x194`
  - `Pael Node.png` `196x202`
  - `Tanx Node.png` `196x200`
  - `Tezcatara Node.png` `203x183`
  - `Vakuu Node.png` `190x202`
- 场景容器：
  - 地图节点根控件尺寸约 `208x208`
  - `Icon` 和 `Outline` 都是全尺寸铺满容器

结论：

- 原版地图节点原图尺寸并不统一。
- 真正稳定的是“落在一个约 `208x208` 的透明容器里显示”。
- 原创先古之民建议交付透明底 `PNG`，以 `208x208` 为目标画幅，必要时保留透明留白，不必追求每个像素都填满。

### C. 地图节点头像 Outline

这是地图节点头像外面那一圈描边或发光轮廓。

- 原版对应路径：`images/packed/map/ancients/ancient_node_<entry>_outline.png`
- 原版使用场景：[ancient_map_point.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_map_point.tscn)
- 形式：单独 `Texture2D`
- 原版已导出实际尺寸：
  - `Darv Node Outline.png` `207x208`
  - `Neow Node Outline.png` `208x172`
  - `Nonupeipe Node Outline.png` `278x278`
  - `Orobas Node Outline.png` `192x201`
  - `Pael Node Outline.png` `202x208`
  - `Tanx Node Outline.png` `202x205`
  - `Tezcatara Node Outline.png` `208x189`
  - `Vakuu Node Outline.png` `196x208`

结论：

- outline 也没有统一原图像素。
- 但它和主节点图是成对使用的，最终都铺在同一个约 `208x208` 的 UI 容器内。
- 原创先古之民最稳的做法是：主图和 outline 用同一画幅制作，建议都按 `208x208` 交付。

### D. 对话头像

这是先古之民在事件对话行左侧出现的小头像。

- 原版对应路径：`images/ui/run_history/<entry>.png`
- 原版使用场景：[ancient_dialogue_line.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_dialogue_line.tscn)
- 形式：单独 `Texture2D`
- 原版已导出实际尺寸：
  - `Darv.png` `88x88`
  - `Neow.png` `88x88`
  - `Nonupeipe.png` `88x88`
  - `Orobas.png` `88x88`
  - `Pael.png` `88x88`
  - `Tanx.png` `88x88`
  - `Tezcatara.png` `88x88`
  - `Vakuu.png` `88x88`
- 场景容器：
  - `AncientIcon` 最小尺寸 `56x56`
  - 贴图以原尺寸缩放适配容器

结论：

- 对话头像这一类的原版规格是稳定的：`88x88 PNG`。
- 这类资源虽然放在 `run_history` 目录，但事件对话场景也直接复用它，不只是跑团历史页面使用。

### E. 对话头像 Outline

这是对话头像的轮廓层。

- 原版对应路径：`images/ui/run_history/<entry>_outline.png`
- 原版使用场景：[ancient_dialogue_line.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_dialogue_line.tscn)
- 形式：单独 `Texture2D`
- 原版已导出实际尺寸：
  - `Darv Outline.png` `88x88`
  - `Neow Outline.png` `88x88`
  - `Nonupeipe Outline.png` `88x88`
  - `Orobas Outline.png` `88x88`
  - `Pael Outline.png` `88x88`
  - `Tanx Outline.png` `88x88`
  - `Tezcatara Outline.png` `88x88`
  - `Vakuu Outline.png` `88x88`

结论：

- 对话头像 outline 也是稳定的 `88x88 PNG`。
- 原创先古之民建议把头像图和 outline 一起做，避免后续临时补轮廓。

### F. 文本资产

名字条本身是共享 UI，但文本内容是每个先古之民单独需要的。

- 原版本地化文件：
  - [eng/ancients.json](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/localization/eng/ancients.json)
  - [zhs/ancients.json](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/localization/zhs/ancients.json)
- 至少需要：
  - `<ANCIENT>.title`
  - `<ANCIENT>.epithet`
  - 对话台词
  - 分支按钮文本
  - 如果事件包含主角回话，还要有角色回应文本
- 名字条 UI 由共享场景 [ancient_name_banner.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_name_banner.tscn) 提供，不需要单独做 banner 图。

## 原版共享资源

下面这些是系统公用资源，不需要每个原创先古之民自己从零做：

- 事件页共享布局：[ancient_event_layout.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_event_layout.tscn)
- 名字条：[ancient_name_banner.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_name_banner.tscn)
- 对话气泡和尾巴：[ancient_dialogue_line.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_dialogue_line.tscn)
- 地图节点交互框体：[ancient_map_point.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_map_point.tscn)
- 事件选项按钮：[ancient_event_option_button.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_event_option_button.tscn)

这意味着，原创先古之民通常不需要自己再做：

- 对话框背景
- 对话尾巴
- 标题 banner 贴图
- 选项按钮底图
- 地图点选中的 reticle

## 原版尺寸与推荐交付规格

| 资产类别 | 原版路径模式 | 原版实际像素 | 原版使用方式 | 原创建议交付规格 |
| --- | --- | --- | --- | --- |
| 事件主立绘 | `images/ancients/<entry>_placeholder.png` | 静态型先古之民普遍为 `2560x1200` | 背景场景中的主图层 | `2560x1200 PNG` |
| 地图节点头像 | `images/packed/map/ancients/ancient_node_<entry>.png` | 不统一，约 `185x194` 到 `278x278` | 放进约 `208x208` 节点容器 | `208x208 PNG`，允许透明留白 |
| 地图节点头像 outline | `images/packed/map/ancients/ancient_node_<entry>_outline.png` | 不统一，约 `192x201` 到 `278x278` | 与主节点图同容器叠放 | `208x208 PNG` |
| 对话头像 | `images/ui/run_history/<entry>.png` | 稳定 `88x88` | 事件对话行头像 | `88x88 PNG` |
| 对话头像 outline | `images/ui/run_history/<entry>_outline.png` | 稳定 `88x88` | 与对话头像叠放 | `88x88 PNG` |
| 文本资产 | `localization/*/ancients.json` | 不适用 | title / epithet / 对话 / 选项 | 按本地化 key 组织 |

## 最小资产包建议

如果只是想做一个“原版风格可接入”的原创先古之民，最小资产包建议如下：

1. `event_main.png`
   - 事件主立绘
   - 建议 `2560x1200`
2. `map_node.png`
   - 地图节点头像
   - 建议 `208x208`
3. `map_node_outline.png`
   - 地图节点头像 outline
   - 建议 `208x208`
4. `dialogue_icon.png`
   - 对话头像
   - 建议 `88x88`
5. `dialogue_icon_outline.png`
   - 对话头像 outline
   - 建议 `88x88`
6. `localization`
   - `title`
   - `epithet`
   - 对话文本
   - 选项文本

## 关于动态效果

原版不是所有先古之民都走同一套资源形态：

- `Darv / Nonupeipe / Orobas / Pael / Tanx / Vakuu` 更接近“静态主图 + 场景特效”
- `Neow / Tezcatara` 更接近“Spine / atlas 动态角色”

所以，如果原创先古之民不打算做动态效果，不是问题。最稳的路线反而是直接采用“静态主立绘型”的原版做法，也就是准备一张 `2560x1200` 的主图，再把节点头像和对话头像补齐。

## 本文档的证据来源

- 原版先古之民背景场景：
  - [nonupeipe.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/nonupeipe.tscn)
  - [darv.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/darv.tscn)
  - [orobas.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/background_scenes/orobas.tscn)
- 原版地图节点场景：
  - [ancient_map_point.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_map_point.tscn)
- 原版对话行场景：
  - [ancient_dialogue_line.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_dialogue_line.tscn)
- 原版名字条：
  - [ancient_name_banner.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/ui/ancient_name_banner.tscn)
- 原版布局：
  - [ancient_event_layout.tscn](/Users/user/Desktop/sts2-mod-dev/references/pck-extract/sts2-main/scenes/events/ancient_event_layout.tscn)
- 已导出参考 PNG：
  - [images/先古之民](/Users/user/Desktop/sts2-mod-dev/images/先古之民)
