import copy
import hashlib
import json
import math
import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parent
REPO_ROOT = next(parent for parent in ROOT.parents if (parent / ".git").exists())
MAIN_WORKSPACE = Path("/Users/user/Desktop/sts2-mod-dev")

BASE_PROJECT = MAIN_WORKSPACE / "local/spine-proof-togawasakiko-v2"
BASE_JSON = BASE_PROJECT / "animation/togawasakiko_v2.spine-json"
BASE_ATLAS = BASE_PROJECT / "animation/togawasakiko_v2.atlas"
BASE_RIG_SHEET = BASE_PROJECT / "animation/images/rig_sheet.png"
BASE_SKELETON_RESOURCE = (
    BASE_PROJECT / "animation/togawasakiko_v2_skel_data.tres"
)
EXTENSION_SOURCE = (
    MAIN_WORKSPACE / "local/tools/spine-godot-4.2-4.5.1/package/bin"
)
EXPECTED_BASE_SHA256 = (
    "2752c01bc1592508c90d2ce2ae3b58a55763306008d0712cc720d406a0405537"
)

CANDIDATE = ROOT / "fall_prone_candidate_v3_idle_locked.png"
ANCHORS = ROOT / "fall_prone_anchors.json"
RUNTIME_ROOT = REPO_ROOT / "local/spine-proof-togawasakiko-death-single-switch-v1"

NON_DEATH_ANIMATIONS = (
    "idle_loop",
    "attack",
    "cast",
    "hurt",
    "relaxed_loop",
)
RUNTIME_ACTIONS = (*NON_DEATH_ANIMATIONS, "die")
PHASE_TIMES = (0.075, 0.095, 0.125, 0.155, 0.21)
FOOT_CONTACT_SOURCE = (26.0, 961.0)
FOOT_ALIGNMENT_CORRECTION_X = -57.0
GROUND_Y = -358.0


def read_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _verify_inputs() -> dict:
    actual_hash = sha256(BASE_JSON)
    if actual_hash != EXPECTED_BASE_SHA256:
        raise RuntimeError(
            "Canonical V10 hash changed: "
            f"expected {EXPECTED_BASE_SHA256}, got {actual_hash}"
        )
    if not CANDIDATE.is_file():
        raise FileNotFoundError(f"Missing accepted fall attachment: {CANDIDATE}")
    anchors = read_json(ANCHORS)
    if anchors["reference"]["canonical_v10_sha256"] != EXPECTED_BASE_SHA256:
        raise RuntimeError("Anchor manifest does not reference the locked canonical V10")
    return anchors


def _fall_root_setup(anchors: dict) -> dict[str, float | str]:
    transform = anchors["transform"]
    reference = anchors["reference"]
    sprite = reference["probe_spine_sprite"]
    source_center_x = reference["canvas_width"] / 2.0
    source_center_y = reference["canvas_height"] / 2.0
    angle = math.radians(transform["rotation_degrees"])
    cosine = math.cos(angle)
    sine = math.sin(angle)
    transformed_center_x = transform["scale"] * (
        cosine * source_center_x - sine * source_center_y
    ) + transform["translation_x"]
    transformed_center_y = transform["scale"] * (
        sine * source_center_x + cosine * source_center_y
    ) + transform["translation_y"]
    probe_scale = sprite["uniform_scale"]
    if probe_scale == 0:
        raise ValueError("Anchor probe scale must be non-zero")

    setup_scale = transform["scale"] / probe_scale
    return {
        "name": "death_fall_root",
        "parent": "root",
        "x": (transformed_center_x - sprite["position_x"]) / probe_scale,
        "y": -(transformed_center_y - sprite["position_y"]) / probe_scale,
        "rotation": -transform["rotation_degrees"],
        "scaleX": setup_scale,
        "scaleY": setup_scale,
    }


def _foot_locked_translation_x(
    fall_root: dict[str, float | str],
    rotation_offset: float,
) -> float:
    local_x = FOOT_CONTACT_SOURCE[0] - 512.0
    local_y = 768.0 - FOOT_CONTACT_SOURCE[1]
    setup_rotation = float(fall_root["rotation"])
    setup_scale = float(fall_root["scaleX"])

    def rotated_x(rotation_degrees: float) -> float:
        angle = math.radians(rotation_degrees)
        return setup_scale * (
            math.cos(angle) * local_x - math.sin(angle) * local_y
        )

    switch_foot_x = float(fall_root["x"]) + rotated_x(setup_rotation)
    return (
        switch_foot_x
        - float(fall_root["x"])
        - rotated_x(setup_rotation + rotation_offset)
    )


def _curve_1d(
    start: tuple[float, float],
    end: tuple[float, float],
    value_controls: tuple[float, float],
) -> list[float]:
    start_time, start_value = start
    end_time, end_value = end
    duration = end_time - start_time
    change = end_value - start_value
    return [
        start_time + duration * 0.25,
        start_value + change * value_controls[0],
        start_time + duration * 0.75,
        start_value + change * value_controls[1],
    ]


def _translate_timeline(
    points: tuple[tuple[float, float, float], ...],
) -> list[dict]:
    timeline = []
    for index, (time, x, y) in enumerate(points):
        key = {"time": time, "x": x, "y": y}
        if index < len(points) - 1:
            next_time, next_x, next_y = points[index + 1]
            controls = (0.08, 0.58)
            key["curve"] = [
                *_curve_1d((time, x), (next_time, next_x), controls),
                *_curve_1d((time, y), (next_time, next_y), controls),
            ]
        timeline.append(key)
    return timeline


def _rotate_timeline(points: tuple[tuple[float, float], ...]) -> list[dict]:
    timeline = []
    for index, (time, value) in enumerate(points):
        key = {"time": time, "value": value}
        if index < len(points) - 1:
            controls = (0.08, 0.58)
            key["curve"] = _curve_1d(
                (time, value),
                points[index + 1],
                controls,
            )
        timeline.append(key)
    return timeline


def build_single_switch_skeleton() -> tuple[dict, dict]:
    anchors = _verify_inputs()
    base = read_json(BASE_JSON)
    return apply_single_switch_death(base, anchors)


def apply_single_switch_death(base: dict, anchors: dict) -> tuple[dict, dict]:
    skeleton = copy.deepcopy(base)
    missing_animations = [
        name
        for name in (*NON_DEATH_ANIMATIONS, "die")
        if name not in base["animations"]
    ]
    if missing_animations:
        raise RuntimeError(f"Canonical V10 is missing animations: {missing_animations}")

    fall_root = _fall_root_setup(anchors)
    skeleton["bones"].append(fall_root)
    skeleton["slots"].append({"name": "death_fall", "bone": "death_fall_root"})
    skeleton["skins"][0]["attachments"]["death_fall"] = {
        "death_fall_prone": {
            "type": "region",
            "path": "death_fall_prone",
            "width": 1024,
            "height": 1536,
        }
    }

    standing_art_slots = [
        slot["name"]
        for slot in skeleton["slots"]
        if slot.get("attachment") is not None
    ]
    die_slots = {
        slot_name: {"attachment": [{"time": 0.075, "name": None}]}
        for slot_name in standing_art_slots
    }
    die_slots["death_fall"] = {
        "attachment": [
            {"time": 0.0, "name": None},
            {"time": 0.075, "name": "death_fall_prone"},
        ]
    }

    final_rotation = anchors["transform"]["rotation_degrees"]
    grounded_x = (
        _foot_locked_translation_x(fall_root, final_rotation)
        + FOOT_ALIGNMENT_CORRECTION_X
    )
    fall_translation_points = (
        (0.075, grounded_x, GROUND_Y + 8.0),
        (0.095, grounded_x, GROUND_Y),
        (0.125, grounded_x, GROUND_Y + 4.0),
        (0.155, grounded_x, GROUND_Y),
        (0.21, grounded_x, GROUND_Y),
    )
    fall_rotation_points = (
        (0.075, final_rotation),
        (0.095, final_rotation),
        (0.125, final_rotation),
        (0.155, final_rotation),
        (0.21, final_rotation),
    )
    die_bones = {
        "hips": {
            "translate": [
                {"time": 0.0, "x": 0.0, "y": 0.0},
                {"time": 0.035, "x": 2.0, "y": -3.0},
                {"time": 0.075, "x": 5.0, "y": -10.0},
            ],
            "rotate": [
                {"time": 0.0, "value": 0.0},
                {"time": 0.035, "value": -2.0},
                {"time": 0.075, "value": -6.0},
            ],
        },
        "torso": {
            "rotate": [
                {"time": 0.0, "value": 0.0},
                {"time": 0.035, "value": -4.0},
                {"time": 0.075, "value": -12.0},
            ]
        },
        "death_fall_root": {
            "translate": _translate_timeline(fall_translation_points),
            "rotate": _rotate_timeline(fall_rotation_points),
        },
    }
    skeleton["animations"]["die"] = {"bones": die_bones, "slots": die_slots}

    manifest = {
        "schema_version": 1,
        "canonical_v10_json": str(BASE_JSON),
        "canonical_v10_sha256": EXPECTED_BASE_SHA256,
        "candidate_png": str(CANDIDATE),
        "candidate_sha256": sha256(CANDIDATE),
        "anchors_json": str(ANCHORS),
        "alignment_transform": copy.deepcopy(anchors["transform"]),
        "alignment_probe_spine_sprite": copy.deepcopy(
            anchors["reference"]["probe_spine_sprite"]
        ),
        "duration_seconds": 0.21,
        "switch_time_seconds": 0.075,
        "contact_time_seconds": 0.095,
        "rebound_time_seconds": 0.125,
        "hold_time_seconds": 0.155,
        "attachment_switch_count": 1,
        "full_character_attachment_count": 1,
        "rgba_crossfade": False,
        "fall_root_phase_times": list(PHASE_TIMES),
        "fall_root_setup": copy.deepcopy(fall_root),
        "foot_contact_source": {
            "x": FOOT_CONTACT_SOURCE[0],
            "y": FOOT_CONTACT_SOURCE[1],
        },
        "foot_alignment_correction_x": FOOT_ALIGNMENT_CORRECTION_X,
        "ground_y": GROUND_Y,
        "grounded_fall_root_x": grounded_x,
        "runtime_actions": list(RUNTIME_ACTIONS),
        "atlas_pages": [
            {"path": "images/rig_sheet.png", "width": 3072, "height": 3072},
            {
                "path": "images/fall_prone_candidate_v3_idle_locked.png",
                "width": 1024,
                "height": 1536,
                "format": "RGBA8888",
            },
        ],
    }
    return skeleton, manifest


def _combined_atlas() -> str:
    fall_page = """images/fall_prone_candidate_v3_idle_locked.png
size: 1024,1536
format: RGBA8888
filter: Linear,Linear
repeat: none
death_fall_prone
  rotate: false
  xy: 0, 0
  size: 1024, 1536
  orig: 1024, 1536
  offset: 0, 0
  index: -1
"""
    return BASE_ATLAS.read_text(encoding="utf-8").rstrip() + "\n\n" + fall_page


def _project_godot() -> str:
    return """config_version=5

[application]
config/name="Togawasakiko Single-Switch Death V1"
run/main_scene="res://preview_scene.tscn"
config/features=PackedStringArray("4.5", "Forward Plus")

[display]
window/size/viewport_width=1280
window/size/viewport_height=1024
window/size/window_width_override=1280
window/size/window_height_override=1024

[rendering]
renderer/rendering_method="gl_compatibility"
renderer/rendering_method.mobile="gl_compatibility"
"""


def _preview_scene() -> str:
    return """[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://preview_driver.gd" id="1_script"]
[ext_resource type="SpineSkeletonDataResource" path="res://animation/togawasakiko_v2_skel_data.tres" id="2_skel"]

[node name="SingleSwitchDeathPreview" type="Node2D"]
script = ExtResource("1_script")

[node name="Background" type="ColorRect" parent="."]
offset_right = 1280.0
offset_bottom = 1024.0
color = Color(0.025, 0.028, 0.04, 1)
mouse_filter = 2

[node name="Visuals" type="SpineSprite" parent="."]
position = Vector2(384, 515)
scale = Vector2(0.58, 0.58)
skeleton_data_res = ExtResource("2_skel")
preview_skin = "default"
preview_animation = "die"

[node name="Caption" type="Label" parent="."]
offset_left = 24.0
offset_top = 20.0
offset_right = 1256.0
offset_bottom = 58.0
theme_override_colors/font_color = Color(0.86, 0.78, 0.58, 1)
theme_override_font_sizes/font_size = 24
text = "SINGLE-SWITCH DEATH V1"
horizontal_alignment = 1
"""


def _preview_driver() -> str:
    return """extends Node2D

const DURATION_SECONDS := 0.21

@onready var visuals: Node = $Visuals

var elapsed := 0.0

func _ready() -> void:
	var animation_state: Object = visuals.call("get_animation_state")
	animation_state.call("set_animation", "die", false, 0)

func _process(delta: float) -> void:
	elapsed += delta
	if elapsed >= DURATION_SECONDS:
		get_tree().quit(0)
"""


def _inspect_spine() -> str:
    actions = "\n".join(f'\t"{name}",' for name in RUNTIME_ACTIONS)
    return f"""extends SceneTree

const ANIMATIONS := [
{actions}
]

func _initialize() -> void:
\tvar scene := load("res://preview_scene.tscn")
\tif scene == null:
\t\tpush_error("preview_scene.tscn failed to load")
\t\tquit(2)
\t\treturn
\tvar instance: Node = scene.instantiate()
\troot.add_child(instance)
\tvar visuals: Node = instance.get_node("Visuals")
\tif visuals.get_class() != "SpineSprite":
\t\tpush_error("Visuals is not a SpineSprite")
\t\tquit(3)
\t\treturn
\tvar animation_state: Object = visuals.call("get_animation_state")
\tfor animation_name in ANIMATIONS:
\t\tvar entry: Object = animation_state.call("set_animation", animation_name, false, 0)
\t\tif entry == null:
\t\t\tpush_error("Missing animation: %s" % animation_name)
\t\t\tquit(4)
\t\t\treturn
\t\tprint("SINGLE_SWITCH_ACTION_OK=", animation_name)
\tprint("SINGLE_SWITCH_ALL_ACTIONS_LOAD_OK")
\tquit(0)
"""


def _write_text(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8")


def build_runtime_project() -> Path:
    expected_runtime_root = (
        REPO_ROOT / "local/spine-proof-togawasakiko-death-single-switch-v1"
    )
    if RUNTIME_ROOT != expected_runtime_root:
        raise RuntimeError(f"Refusing to replace unexpected runtime path: {RUNTIME_ROOT}")

    skeleton, manifest = build_single_switch_skeleton()
    shutil.rmtree(RUNTIME_ROOT, ignore_errors=True)
    images = RUNTIME_ROOT / "animation/images"
    images.mkdir(parents=True)
    shutil.copytree(EXTENSION_SOURCE, RUNTIME_ROOT / "bin")
    shutil.copy2(BASE_RIG_SHEET, images / "rig_sheet.png")
    shutil.copy2(CANDIDATE, images / "fall_prone_candidate_v3_idle_locked.png")
    shutil.copy2(
        BASE_SKELETON_RESOURCE,
        RUNTIME_ROOT / "animation/togawasakiko_v2_skel_data.tres",
    )

    _write_text(
        RUNTIME_ROOT / "animation/togawasakiko_v2.spine-json",
        json.dumps(skeleton, indent=2, ensure_ascii=True) + "\n",
    )
    _write_text(
        RUNTIME_ROOT / "single_switch_manifest.json",
        json.dumps(manifest, indent=2, ensure_ascii=True) + "\n",
    )
    _write_text(
        RUNTIME_ROOT / "animation/togawasakiko_v2.atlas",
        _combined_atlas(),
    )
    _write_text(RUNTIME_ROOT / "project.godot", _project_godot())
    _write_text(RUNTIME_ROOT / "preview_scene.tscn", _preview_scene())
    _write_text(RUNTIME_ROOT / "preview_driver.gd", _preview_driver())
    _write_text(RUNTIME_ROOT / "inspect_spine.gd", _inspect_spine())
    return RUNTIME_ROOT


if __name__ == "__main__":
    print(build_runtime_project())
