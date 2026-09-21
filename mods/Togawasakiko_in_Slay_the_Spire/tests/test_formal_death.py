import hashlib
import json
from pathlib import Path
import unittest

MOD = Path(__file__).resolve().parents[1]
ANIMATION = MOD / "pack/animations/characters/togawasakiko"
# User-approved animation payload, Steam-tested on 2026-09-21.
REGULAR = {
    "idle_loop": "f8fb73e568f954a6accb10e3dcb86bd904ad3dd9707efcb1c24a4c73f333ae34",
    "attack": "a6f6771e80c997347e87799ee5ff1ff306b7f6b4ade02f996d4bae17aa5f3327",
    "cast": "7e5692904c9016fcc0fe07cd1192305db0b6b43f8b5bedef0d84590bb8446012",
    "hurt": "ddb196671663aa2405a9ad38dd36b10d3d49f9f2581df48ee15fb668a2389ec8",
    "relaxed_loop": "65efb1e64a4acefc83c93248b4a1868435f5cdfed65bcd8ab2acee51b96b7ef1",
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
            if name in {"death_fall", "death_halo"}:
                continue
            with self.subTest(slot=name):
                self.assertIn(name, slots)
                self.assertEqual(slots[name]["attachment"][-1], {"time": 0.075, "name": None})

    def test_accepted_art_is_pack_page(self):
        page = ANIMATION / "images/fall_prone_candidate_v3_idle_locked.png"
        self.assertTrue(page.is_file(), "Accepted V3 image is missing from formal pack")
        self.assertEqual(hashlib.sha256(page.read_bytes()).hexdigest(),
                         "44d29aa578320a1eba2ef50c9a94e2a338365763f7417a0cca4c0919e1533f39")
        atlas = (ANIMATION / "togawasakiko_v2.atlas").read_text()
        self.assertEqual(atlas.count("images/fall_prone_candidate_v3_idle_locked.png"), 1)
        self.assertEqual(atlas.count("images/death_halo_ring.png"), 1)
        self.assertEqual(hashlib.sha256((ANIMATION / "images/death_halo_ring.png").read_bytes()).hexdigest(),
                         "1b9aa0dce6f8d9347c7f76fb07ec78afea162e8ca399aed3fbf635bf55c2e008")

    def test_halo_has_independent_motion_and_stays_behind_body(self):
        bone = next(b for b in self.skeleton["bones"] if b["name"] == "death_halo_motion")
        self.assertEqual(bone["parent"], "death_fall_root")
        slots = [slot["name"] for slot in self.skeleton["slots"]]
        self.assertLess(slots.index("death_halo"), slots.index("death_fall"))
        motion = self.skeleton["animations"]["die"]["bones"]["death_halo_motion"]
        positions = motion["translate"]
        self.assertTrue(all(key["x"] == 0.0 and key["y"] == 0.0
                            for key in positions if key["time"] <= 0.24))
        self.assertTrue(all(a["x"] <= b["x"] for a, b in zip(positions, positions[1:])))
        self.assertAlmostEqual(positions[-1]["x"], 258.0)
        self.assertLess(motion["rotate"][-1]["value"], -230.0)

    def test_halo_fades_and_detaches_at_end(self):
        slot = self.skeleton["animations"]["die"]["slots"]["death_halo"]
        self.assertEqual(slot["attachment"], [
            {"time": 0.0, "name": None},
            {"time": 0.075, "name": "death_halo_ring"},
            {"time": 1.72, "name": None},
        ])
        self.assertEqual(slot["rgba"][-2:], [
            {"time": 1.32, "color": "ffffffff"},
            {"time": 1.72, "color": "ffffff00"},
        ])

    def test_other_actions_hide_the_detached_halo(self):
        for name in REGULAR:
            with self.subTest(action=name):
                self.assertEqual(self.skeleton["animations"][name]["slots"]["death_halo"]["attachment"],
                                 [{"time": 0.0, "name": None}])


if __name__ == "__main__":
    unittest.main()
