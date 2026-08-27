import math
import unittest

from PIL import Image

from build_single_switch_death_v1 import (
    ANCHORS,
    BASE_JSON,
    CANDIDATE,
    EXPECTED_BASE_SHA256,
    RUNTIME_ROOT,
    build_runtime_project,
    build_single_switch_skeleton,
    read_json,
    sha256,
)


NON_DEATH_ANIMATIONS = (
    "idle_loop",
    "attack",
    "cast",
    "hurt",
    "relaxed_loop",
)
PHASE_TIMES = [0.30, 0.42, 0.54, 0.68, 0.74, 0.82, 0.95]


def keyed_at(timeline: list[dict], time: float) -> dict:
    return next(key for key in timeline if key["time"] == time)


def value_without_curve_or_time(key: dict) -> dict:
    return {
        name: value
        for name, value in key.items()
        if name not in {"curve", "time"}
    }


class SingleSwitchDeathBuildTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.runtime_root = build_runtime_project()

    def test_canonical_v10_sha_is_locked(self) -> None:
        self.assertEqual(EXPECTED_BASE_SHA256, sha256(BASE_JSON))

    def test_die_uses_one_switch_and_no_v87_stage_slots(self) -> None:
        skeleton, manifest = build_single_switch_skeleton()
        die = skeleton["animations"]["die"]
        self.assertEqual(manifest["duration_seconds"], 0.95)
        self.assertEqual(manifest["switch_time_seconds"], 0.30)
        self.assertEqual(manifest["hold_time_seconds"], 0.82)
        self.assertEqual(manifest["attachment_switch_count"], 1)
        self.assertFalse(
            any(name.startswith("v87_kneel_") for name in die.get("slots", {}))
        )
        self.assertFalse(
            any(name.startswith("v87_brace_") for name in die.get("slots", {}))
        )
        self.assertNotIn("v87_prone_final", str(die))

        shown_attachments = [
            key
            for slot in die["slots"].values()
            for key in slot.get("attachment", [])
            if key.get("name") is not None
        ]
        self.assertEqual(
            shown_attachments,
            [{"time": 0.30, "name": "death_fall_prone"}],
        )

    def test_non_death_animations_are_unchanged(self) -> None:
        base = read_json(BASE_JSON)["animations"]
        skeleton, _ = build_single_switch_skeleton()
        for name in NON_DEATH_ANIMATIONS:
            self.assertEqual(skeleton["animations"][name], base[name])

    def test_build_returns_an_independent_deep_copy(self) -> None:
        first, _ = build_single_switch_skeleton()
        second, _ = build_single_switch_skeleton()
        first["animations"]["idle_loop"]["task3_mutation_probe"] = True
        self.assertNotIn("task3_mutation_probe", second["animations"]["idle_loop"])
        self.assertNotIn(
            "task3_mutation_probe",
            read_json(BASE_JSON)["animations"]["idle_loop"],
        )

    def test_adds_exactly_one_full_character_region_attachment(self) -> None:
        base = read_json(BASE_JSON)
        skeleton, _ = build_single_switch_skeleton()
        self.assertEqual(len(skeleton["bones"]), len(base["bones"]) + 1)
        self.assertEqual(len(skeleton["slots"]), len(base["slots"]) + 1)

        death_bones = [
            bone for bone in skeleton["bones"] if bone["name"] == "death_fall_root"
        ]
        self.assertEqual(len(death_bones), 1)
        self.assertEqual(death_bones[0]["parent"], "root")

        fall_slots = [
            slot for slot in skeleton["slots"] if slot["name"] == "death_fall"
        ]
        self.assertEqual(fall_slots, [{"name": "death_fall", "bone": "death_fall_root"}])

        attachment = skeleton["skins"][0]["attachments"]["death_fall"]
        self.assertEqual(list(attachment), ["death_fall_prone"])
        self.assertEqual(
            attachment["death_fall_prone"],
            {
                "type": "region",
                "path": "death_fall_prone",
                "width": 1024,
                "height": 1536,
            },
        )

    def test_anchor_transform_is_consumed_directly(self) -> None:
        anchors = read_json(ANCHORS)
        skeleton, manifest = build_single_switch_skeleton()
        transform = anchors["transform"]
        sprite = anchors["reference"]["probe_spine_sprite"]
        self.assertEqual(manifest["alignment_transform"], transform)
        self.assertEqual(manifest["alignment_probe_spine_sprite"], sprite)

        angle = math.radians(transform["rotation_degrees"])
        center_x = 512.0
        center_y = 768.0
        transformed_center_x = transform["scale"] * (
            math.cos(angle) * center_x - math.sin(angle) * center_y
        ) + transform["translation_x"]
        transformed_center_y = transform["scale"] * (
            math.sin(angle) * center_x + math.cos(angle) * center_y
        ) + transform["translation_y"]
        probe_scale = sprite["uniform_scale"]
        expected = {
            "x": (transformed_center_x - sprite["position_x"]) / probe_scale,
            "y": -(transformed_center_y - sprite["position_y"]) / probe_scale,
            "rotation": -transform["rotation_degrees"],
            "scaleX": transform["scale"] / probe_scale,
            "scaleY": transform["scale"] / probe_scale,
        }
        bone = next(
            bone for bone in skeleton["bones"] if bone["name"] == "death_fall_root"
        )
        for name, value in expected.items():
            self.assertAlmostEqual(bone[name], value, places=9)

    def test_standing_art_hides_when_fall_attachment_appears(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        die_slots = skeleton["animations"]["die"]["slots"]
        standing_slots = [
            slot["name"]
            for slot in skeleton["slots"]
            if slot.get("attachment") is not None
        ]
        for slot_name in standing_slots:
            self.assertEqual(
                die_slots[slot_name],
                {"attachment": [{"time": 0.30, "name": None}]},
            )
        self.assertEqual(
            die_slots["death_fall"],
            {
                "attachment": [
                    {"time": 0.0, "name": None},
                    {"time": 0.30, "name": "death_fall_prone"},
                ]
            },
        )

    def test_root_does_not_translate_before_switch(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        root_timeline = skeleton["animations"]["die"]["bones"].get("root", {})
        early_translation = [
            key
            for key in root_timeline.get("translate", [])
            if key["time"] < 0.30
        ]
        self.assertEqual(early_translation, [])

    def test_release_and_lean_use_the_fixed_standing_bone_values(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        die_bones = skeleton["animations"]["die"]["bones"]
        self.assertEqual(
            die_bones["hips"],
            {
                "translate": [
                    {"time": 0.0, "x": 0.0, "y": 0.0},
                    {"time": 0.12, "x": 2.0, "y": -3.0},
                    {"time": 0.30, "x": 12.0, "y": -18.0},
                ],
                "rotate": [
                    {"time": 0.0, "value": 0.0},
                    {"time": 0.12, "value": -2.0},
                    {"time": 0.30, "value": -8.0},
                ],
            },
        )
        self.assertEqual(
            die_bones["torso"],
            {
                "rotate": [
                    {"time": 0.0, "value": 0.0},
                    {"time": 0.12, "value": -4.0},
                    {"time": 0.30, "value": -20.0},
                ]
            },
        )

    def test_fall_timeline_uses_fixed_times_and_spine_beziers(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        fall_root = skeleton["animations"]["die"]["bones"]["death_fall_root"]
        for timeline_name in ("translate", "rotate"):
            timeline = fall_root[timeline_name]
            self.assertEqual([key["time"] for key in timeline], PHASE_TIMES)
            for key in timeline[:-1]:
                self.assertIsInstance(key.get("curve"), list)
                self.assertEqual(
                    len(key["curve"]),
                    8 if timeline_name == "translate" else 4,
                )

    def test_downward_key_spacing_accelerates_through_contact(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        timeline = skeleton["animations"]["die"]["bones"]["death_fall_root"][
            "translate"
        ]
        fall_keys = [keyed_at(timeline, time) for time in PHASE_TIMES[:4]]
        downward_displacements = [
            previous["y"] - current["y"]
            for previous, current in zip(fall_keys, fall_keys[1:])
        ]
        self.assertTrue(all(distance > 0 for distance in downward_displacements))
        self.assertLess(downward_displacements[0], downward_displacements[1])
        self.assertLess(downward_displacements[1], downward_displacements[2])

    def test_contact_has_exactly_one_small_rebound(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        timeline = skeleton["animations"]["die"]["bones"]["death_fall_root"][
            "translate"
        ]
        contact_keys = [keyed_at(timeline, time) for time in PHASE_TIMES[3:]]
        vertical_moves = [
            current["y"] - previous["y"]
            for previous, current in zip(contact_keys, contact_keys[1:])
        ]
        upward_moves = [distance for distance in vertical_moves if distance > 0]
        self.assertEqual(len(upward_moves), 1)
        self.assertGreaterEqual(upward_moves[0], 3.0)
        self.assertLessEqual(upward_moves[0], 6.0)
        self.assertLess(vertical_moves[1], 0)
        self.assertEqual(vertical_moves[2], 0)

    def test_final_transform_matches_hold_transform(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        fall_root = skeleton["animations"]["die"]["bones"]["death_fall_root"]
        for timeline_name in ("translate", "rotate"):
            timeline = fall_root[timeline_name]
            hold = keyed_at(timeline, 0.82)
            final = keyed_at(timeline, 0.95)
            self.assertEqual(
                value_without_curve_or_time(hold),
                value_without_curve_or_time(final),
            )

    def test_die_has_no_rgba_crossfade_timeline(self) -> None:
        skeleton, _ = build_single_switch_skeleton()
        die = skeleton["animations"]["die"]
        self.assertNotIn("rgba", str(die).lower())
        self.assertNotIn("rgb", str(die).lower())
        self.assertNotIn("alpha", str(die).lower())
        for slot in die["slots"].values():
            self.assertEqual(set(slot), {"attachment"})

    def test_runtime_project_writes_required_outputs(self) -> None:
        expected = [
            "animation/images/fall_prone_candidate_v1.png",
            "animation/images/rig_sheet.png",
            "animation/togawasakiko_v2.atlas",
            "animation/togawasakiko_v2.spine-json",
            "animation/togawasakiko_v2_skel_data.tres",
            "bin/spine_godot_extension.gdextension",
            "inspect_spine.gd",
            "preview_driver.gd",
            "preview_scene.tscn",
            "project.godot",
            "single_switch_manifest.json",
        ]
        self.assertEqual(self.runtime_root, RUNTIME_ROOT)
        for relative_path in expected:
            self.assertTrue((RUNTIME_ROOT / relative_path).is_file(), relative_path)

        runtime_skeleton = read_json(
            RUNTIME_ROOT / "animation/togawasakiko_v2.spine-json"
        )
        built_skeleton, built_manifest = build_single_switch_skeleton()
        self.assertEqual(runtime_skeleton, built_skeleton)
        self.assertEqual(
            read_json(RUNTIME_ROOT / "single_switch_manifest.json"),
            built_manifest,
        )
        self.assertEqual(sha256(BASE_JSON), EXPECTED_BASE_SHA256)

    def test_runtime_copies_assets_and_extension(self) -> None:
        runtime_candidate = RUNTIME_ROOT / "animation/images/fall_prone_candidate_v1.png"
        self.assertEqual(sha256(runtime_candidate), sha256(CANDIDATE))
        with Image.open(runtime_candidate) as image:
            self.assertEqual(image.mode, "RGBA")
            self.assertEqual(image.size, (1024, 1536))

        self.assertTrue(
            (
                RUNTIME_ROOT
                / "bin/macos/libspine_godot.macos.editor.framework/libspine_godot.macos.editor"
            ).is_file()
        )

    def test_atlas_has_a_correct_second_page_for_fall_attachment(self) -> None:
        atlas = (RUNTIME_ROOT / "animation/togawasakiko_v2.atlas").read_text()
        second_page = atlas.split("\n\nimages/fall_prone_candidate_v1.png\n", 1)
        self.assertEqual(len(second_page), 2)
        fall_page = second_page[1]
        self.assertIn("size: 1024,1536\n", fall_page)
        self.assertIn("format: RGBA8888\n", fall_page)
        self.assertIn("filter: Linear,Linear\n", fall_page)
        self.assertIn("repeat: none\n", fall_page)
        self.assertIn("death_fall_prone\n", fall_page)
        self.assertIn("  size: 1024, 1536\n", fall_page)
        self.assertIn("  orig: 1024, 1536\n", fall_page)

    def test_preview_plays_die_once_and_quits_after_point_nine_five(self) -> None:
        driver = (RUNTIME_ROOT / "preview_driver.gd").read_text()
        self.assertEqual(driver.count('set_animation", "die", false, 0'), 1)
        self.assertIn("const DURATION_SECONDS := 0.95", driver)
        self.assertIn("elapsed >= DURATION_SECONDS", driver)
        self.assertEqual(driver.count("get_tree().quit(0)"), 1)

    def test_runtime_inspector_declares_every_action(self) -> None:
        inspector = (RUNTIME_ROOT / "inspect_spine.gd").read_text()
        for animation_name in (*NON_DEATH_ANIMATIONS, "die"):
            self.assertIn(f'"{animation_name}"', inspector)
        self.assertIn("SINGLE_SWITCH_ALL_ACTIONS_LOAD_OK", inspector)


if __name__ == "__main__":
    unittest.main()
