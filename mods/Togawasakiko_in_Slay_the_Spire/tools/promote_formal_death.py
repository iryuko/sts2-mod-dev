"""Promote the accepted death onto formal V31 without replacing regular actions."""

import hashlib
import importlib.util
import json
from pathlib import Path
import shutil

MOD = Path(__file__).resolve().parents[1]
SOURCE = MOD / "incoming_assets/character_in_combat_spine/togawasakiko/source_layers/redesign/v2_rig_ready/death_rig/single_switch_v1"
DEST = MOD / "pack/animations/characters/togawasakiko"
REGULAR = {
    "idle_loop": "a1a3a976f7d4149b2d5f07bd99f6c9f0690e11a660bf5184faccb0a39995f339",
    "attack": "07c1c3fdbc53ce447e243c8c8773c03787a24570665f59668ce023675d43d78c",
    "cast": "648ab6b21e6a4720ca7d2716cb16c0c6b702ba7404ca992628cc94f2a774cc8e",
    "hurt": "3aada40547390551bc9ae6cdc341e5de76a4878a384764f1fd0b49240a6b5666",
    "relaxed_loop": "60046dc8e434cce0b0a5d93b703b373885b77db16b104f7e21cf3cc67425bf91",
}


def verify_regular(skeleton):
    for action, expected in REGULAR.items():
        data = json.dumps(skeleton["animations"][action], sort_keys=True, separators=(",", ":")).encode()
        if hashlib.sha256(data).hexdigest() != expected:
            raise ValueError(f"Refusing to alter a non-V31 regular action: {action}")


def promote():
    spec = importlib.util.spec_from_file_location("accepted_death", SOURCE / "build_single_switch_death_v1.py")
    builder = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(builder)
    path = DEST / "togawasakiko_v2.spine-json"
    base = json.loads(path.read_text())
    verify_regular(base)
    # Remove only our own additions so promotion is repeatable.
    base["bones"] = [bone for bone in base["bones"] if bone["name"] != "death_fall_root"]
    base["slots"] = [slot for slot in base["slots"] if slot["name"] != "death_fall"]
    base["skins"][0]["attachments"].pop("death_fall", None)
    anchors = json.loads((SOURCE / "fall_prone_anchors.json").read_text())
    skeleton, _ = builder.apply_single_switch_death(base, anchors)
    # Formal V31 has overlay/death slots absent in the original V10 proof.
    for slot in base["slots"]:
        skeleton["animations"]["die"]["slots"][slot["name"]] = {
            "attachment": [{"time": 0.075, "name": None}]
        }
    verify_regular(skeleton)
    atlas_path = DEST / "togawasakiko_v2.atlas"
    atlas = atlas_path.read_text()
    page_name = "images/fall_prone_candidate_v3_idle_locked.png"
    if page_name not in atlas:
        atlas = atlas.rstrip() + "\n\n" + "\n".join([
            page_name, "size: 1024,1536", "format: RGBA8888", "filter: Linear,Linear",
            "repeat: none", "death_fall_prone", "  rotate: false", "  xy: 0, 0",
            "  size: 1024, 1536", "  orig: 1024, 1536", "  offset: 0, 0", "  index: -1", "",
        ])
    shutil.copy2(SOURCE / Path(page_name).name, DEST / page_name)
    path.write_text(json.dumps(skeleton, indent=2, ensure_ascii=True) + "\n")
    atlas_path.write_text(atlas)
    print("FORMAL_DEATH_PROMOTED: 0.21s; five V31 actions unchanged")


if __name__ == "__main__":
    promote()
