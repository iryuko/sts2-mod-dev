import unittest
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
ASSET = ROOT / "fall_prone_candidate_v1.png"


class FallProneAssetContractTests(unittest.TestCase):
    def test_forward_fall_asset_is_full_canvas_rgba(self) -> None:
        self.assertTrue(ASSET.is_file())
        image = Image.open(ASSET)
        self.assertEqual(image.mode, "RGBA")
        self.assertEqual(image.size, (1024, 1536))
        alpha = image.getchannel("A")
        self.assertEqual(alpha.getextrema(), (0, 255))
        self.assertIsNotNone(alpha.getbbox())
