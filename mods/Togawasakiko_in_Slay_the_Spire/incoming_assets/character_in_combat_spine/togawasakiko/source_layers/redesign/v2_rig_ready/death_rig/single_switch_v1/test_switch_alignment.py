import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent
ANCHORS = ROOT / "fall_prone_anchors.json"


class SwitchAlignmentTests(unittest.TestCase):
    def test_anchor_manifest_defines_two_landmarks_per_pose(self) -> None:
        data = json.loads(ANCHORS.read_text())
        for pose in ("standing_at_switch", "fall_attachment"):
            self.assertEqual(
                set(data[pose]),
                {"head_center", "hip_center"},
            )
            for point in data[pose].values():
                self.assertGreaterEqual(point["x"], 0)
                self.assertLessEqual(point["x"], 1024)
                self.assertGreaterEqual(point["y"], 0)
                self.assertLessEqual(point["y"], 1536)

    def test_switch_alignment_error_is_at_most_eight_pixels(self) -> None:
        data = json.loads(ANCHORS.read_text())
        self.assertLessEqual(data["qa"]["max_landmark_error_pixels"], 8.0)

    def test_standing_probe_has_transparent_margin_on_every_edge(self) -> None:
        data = json.loads(ANCHORS.read_text())
        left, top, right, bottom = data["qa"]["standing_probe_content_bbox"]
        self.assertGreater(left, 0)
        self.assertGreater(top, 0)
        self.assertLess(right, 1024)
        self.assertLess(bottom, 1536)
