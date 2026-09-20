# 五张 Bridge 卡图安装审计

记录日期：2026-09-06

## 结论

`0.2.2` 源码中的五张 bridge 卡此前都通过
`GetNormalSkillPlaceholderPortraitPath()` 共用
`res://mod_assets/cards/basic/unendurable.png`。因此文件即使已经归档，游戏仍只会显示占位图；
这不是单独一张“接续小节”绑定错误。

本次已为五张卡建立稳定的独立路径：

| 卡牌 | 品质 | runtime 路径 | 最终图 SHA-256 |
| --- | --- | --- | --- |
| `UnfinishedScore / 未完成的乐谱` | Common | `res://mod_assets/cards/normal/common/unfinished_score.png` | `fb4e979c4a3693bfc86290e87139eb58b0cb6f9be5ccbac73e5911a2d5cb7415` |
| `FollowingPhrase / 接续小节` | Common | `res://mod_assets/cards/normal/common/following_phrase.png` | `877c5179505773c35a7a386ae0661799d87a08925d9b95b601241d15d7e199db` |
| `Unmask / 揭下面具` | Uncommon | `res://mod_assets/cards/normal/uncommon/unmask.png` | `d9f5fa500a7926e5b263a210bad871769892fc916ae89b49c8041228664d3bfe` |
| `RehearsalOrder / 排练顺序` | Uncommon | `res://mod_assets/cards/normal/uncommon/rehearsal_order.png` | `2aef4b5362c1bdd3683cbd03d4b0e06d544e5be69e0c1f719ba5287ae060c059` |
| `BackstageSupport / 幕后支撑` | Uncommon | `res://mod_assets/cards/normal/uncommon/backstage_support.png` | `a95eb4f96604b9e4018bd3f063b5dd05d21b4f989e835b85b65a5d7af0d73e20` |

每张最终图均为 `1000x760` PNG；`assets/` 与 `pack/` 中同名文件逐字节一致。

## 归档映射

- `unfinished_score.png`：采用 `drafts/v2/unfinished_score_composition_v2.png`。
- `following_phrase.png`：采用 `drafts/v1/following_phrase_composition_v1.png`。
- `unmask.png`：采用已确认保留的 `drafts/v1/unmask_composition_v1.png`，未误用 v2。
- `rehearsal_order.png`：采用聊天确认版本，并归档为 `drafts/v2/rehearsal_order_chat_v1.png`。
- `backstage_support.png`：采用 `drafts/v2/backstage_support_chat_v2.png`。

源来稿仍位于共享工作区的
`mods/Togawasakiko_in_Slay_the_Spire/incoming_assets/cards/normal_pool/_originals/bridge_cards_2026-09-04/`，
不直接参与运行时加载。

## 验证与安装

- 先新增精确 portrait 路径断言；修复前断言复现了
  `unfinished_score.png` 实际得到 `basic/unendurable.png`。
- 修复后 gameplay regression：`107 passed, 0 failed`。
- 无界面 Godot 导入成功，PCK 路径表可检出五个独立 PNG 路径。
- 本地 release 与游戏安装目录三件套逐字节一致：
  - DLL：`53c6bce1e932df648b63ebfb65c2000f11bc47547ac1914c1f3630ffb48da709`
  - PCK：`829e3b68f10acf37b184aceb99382399efcc780c92eaf828b25fe79addbd4047`
  - manifest：`3d06dd0b6f26e0ecdf36196a275d8f8f6f969cb18174ab8c0fecef6fbf38f624`
- 覆盖前安装包备份位于
  `local/backups/togawasakiko-bridge-card-art-20260906/`。
- 未启动 Steam 或游戏；卡框裁切、升级态显示和图鉴中的实机视觉仍待确认。

本次是 manifest 仍为 `0.2.2` 的本地 post-release 热修。GitHub 上既有 `0.2.2` Release
不会因本地安装自动更新，若要公开分发必须另行发布新构建。

后续发布整理已将该修复升版为 `0.2.3`，不覆盖原 `0.2.2`。最终 release 哈希与
发布边界见 `mods/Togawasakiko_in_Slay_the_Spire/docs/releases/2026-09-06-0.2.3.md`。
