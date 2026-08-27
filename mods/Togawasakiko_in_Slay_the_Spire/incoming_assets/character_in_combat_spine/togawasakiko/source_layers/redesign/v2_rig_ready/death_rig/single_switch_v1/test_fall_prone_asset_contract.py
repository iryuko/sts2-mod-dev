import unittest
from collections import deque
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
ASSET = ROOT / "fall_prone_candidate_v2_clean.png"


class FallProneAssetContractTests(unittest.TestCase):
    def test_forward_fall_asset_is_full_canvas_rgba(self) -> None:
        self.assertTrue(ASSET.is_file())
        image = Image.open(ASSET)
        self.assertEqual(image.mode, "RGBA")
        self.assertEqual(image.size, (1024, 1536))
        alpha = image.getchannel("A")
        self.assertEqual(alpha.getextrema(), (0, 255))
        self.assertIsNotNone(alpha.getbbox())

    def test_clean_asset_has_no_visible_pixels_outside_the_character_band(self) -> None:
        alpha = Image.open(ASSET).getchannel("A")
        left, top, right, bottom = alpha.getbbox()
        self.assertGreater(left, 0)
        self.assertGreater(top, 700)
        self.assertLess(right, 1024)
        self.assertLess(bottom, 1100)

    def test_clean_asset_alpha_is_one_connected_silhouette(self) -> None:
        alpha = Image.open(ASSET).getchannel("A")
        width, height = alpha.size
        visible = {
            (x, y)
            for y in range(height)
            for x in range(width)
            if alpha.getpixel((x, y)) > 0
        }
        self.assertTrue(visible)
        queue = deque([next(iter(visible))])
        visible.remove(queue[0])
        while queue:
            x, y = queue.popleft()
            for next_x, next_y in (
                (x - 1, y - 1),
                (x, y - 1),
                (x + 1, y - 1),
                (x - 1, y),
                (x + 1, y),
                (x - 1, y + 1),
                (x, y + 1),
                (x + 1, y + 1),
            ):
                if (next_x, next_y) in visible:
                    visible.remove((next_x, next_y))
                    queue.append((next_x, next_y))
        self.assertFalse(visible)
