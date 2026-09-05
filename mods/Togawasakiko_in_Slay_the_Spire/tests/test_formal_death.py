import hashlib
import json
from pathlib import Path
import unittest

MOD = Path(__file__).resolve().parents[1]
ANIMATION = MOD / "pack/animations/characters/togawasakiko"
REGULAR = {
    "idle_loop": "a1a3a976f7d4149b2d5f07bd99f6c9f0690e11a660bf5184faccb0a39995f339",
    "attack": "07c1c3fdbc53ce447e243c8c8773c03787a24570665f59668ce023675d43d78c",
    "cast": "648ab6b21e6a4720ca7d2716cb16c0c6b702ba7404ca992628cc94f2a774cc8e",
    "hurt": "3aada40547390551bc9ae6cdc341e5de76a4878a384764f1fd0b49240a6b5666",
    "relaxed_loop": "60046dc8e434cce0b0a5d93b703b373885b77db16b104f7e21cf3cc67425bf91",
}


class FormalDeathTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.skeleton = json.loads((ANIMATION / "togawasakiko_v2.spine-json").read_text())

    def test_regular_actions_frozen(self):
        for action, expected in REGULAR.items():
            with self.subTest(action=action):
                data = json.dumps(self.skeleton["animations"][action], sort_keys=True, separators=(",", ":")).encode()
                self.assertEqual(hashlib.sha256(data).hexdigest(), expected)

    def test_exact_single_switch_and_duration(self):
        die = self.skeleton["animations"]["die"]
        self.assertIn("death_fall", die["slots"])
        self.assertEqual(die["slots"]["death_fall"]["attachment"], [
            {"time": 0.0, "name": None}, {"time": 0.075, "name": "death_fall_prone"}])
        phases = die["bones"]["death_fall_root"]["translate"]
        self.assertEqual([key["time"] for key in phases], [0.075, 0.095, 0.125, 0.155, 0.21])
        self.assertEqual([key["y"] for key in phases], [-350.0, -358.0, -354.0, -358.0, -358.0])
        self.assertEqual(len({key["x"] for key in phases}), 1)
        self.assertEqual(set(self.skeleton["animations"]), {*REGULAR, "die"})

    def test_old_death_and_overlays_cannot_remain_visible(self):
        slots = self.skeleton["animations"]["die"]["slots"]
        for slot in self.skeleton["slots"]:
            name = slot["name"]
            if name == "death_fall":
                continue
            with self.subTest(slot=name):
                self.assertIn(name, slots)
                self.assertEqual(slots[name]["attachment"][-1], {"time": 0.075, "name": None})

    def test_accepted_art_is_pack_page(self):
        source = MOD / "incoming_assets/character_in_combat_spine/togawasakiko/source_layers/redesign/v2_rig_ready/death_rig/single_switch_v1/fall_prone_candidate_v3_idle_locked.png"
        page = ANIMATION / "images/fall_prone_candidate_v3_idle_locked.png"
        self.assertTrue(page.is_file(), "Accepted V3 image is missing from formal pack")
        self.assertEqual(page.read_bytes(), source.read_bytes())
        atlas = (ANIMATION / "togawasakiko_v2.atlas").read_text()
        self.assertEqual(atlas.count("images/fall_prone_candidate_v3_idle_locked.png"), 1)


if __name__ == "__main__":
    unittest.main()
