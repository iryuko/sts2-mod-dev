import hashlib
import json
import math
import os
import shutil
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parent
REPO_ROOT = next(parent for parent in ROOT.parents if (parent / ".git").exists())
MAIN_WORKSPACE = Path("/Users/user/Desktop/sts2-mod-dev")
BASE_PROJECT = MAIN_WORKSPACE / "local/spine-proof-togawasakiko-v2"
BASE_JSON = BASE_PROJECT / "animation/togawasakiko_v2.spine-json"
BASE_ATLAS = BASE_PROJECT / "animation/togawasakiko_v2.atlas"
BASE_RIG_SHEET = BASE_PROJECT / "animation/images/rig_sheet.png"
EXPECTED_BASE_SHA256 = "2752c01bc1592508c90d2ce2ae3b58a55763306008d0712cc720d406a0405537"
GODOT = MAIN_WORKSPACE / "local/tools/godot-4.5.1/Godot.app/Contents/MacOS/Godot"

CANDIDATE = ROOT / "fall_prone_candidate_v1.png"
ANCHORS = ROOT / "fall_prone_anchors.json"
OVERLAY = ROOT / "qa/switch_alignment_overlay.png"
SCRATCH_ROOT = REPO_ROOT / "local/tmp/task2-switch-alignment-probe"
PROBE_IMAGE = SCRATCH_ROOT / "standing_at_switch_probe.png"
CANVAS_SIZE = (1024, 1536)
PROBE_TIME_SECONDS = 0.30

# Measured on the rendered probe and accepted candidate in 1024x1536 pixels.
STANDING_AT_SWITCH = {
    "head_center": {"x": 700.0, "y": 335.0},
    "hip_center": {"x": 548.0, "y": 675.0},
}
FALL_ATTACHMENT = {
    "head_center": {"x": 845.0, "y": 846.0},
    "hip_center": {"x": 545.0, "y": 856.0},
}


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compute_similarity_transform(source: dict, target: dict) -> dict[str, float]:
    source_dx = source["hip_center"]["x"] - source["head_center"]["x"]
    source_dy = source["hip_center"]["y"] - source["head_center"]["y"]
    target_dx = target["hip_center"]["x"] - target["head_center"]["x"]
    target_dy = target["hip_center"]["y"] - target["head_center"]["y"]
    source_length = math.hypot(source_dx, source_dy)
    target_length = math.hypot(target_dx, target_dy)
    if source_length == 0:
        raise ValueError("Source landmarks must not overlap")
    scale = target_length / source_length
    rotation_radians = math.atan2(target_dy, target_dx) - math.atan2(
        source_dy, source_dx
    )
    rotation = math.degrees(rotation_radians)
    cosine = math.cos(rotation_radians)
    sine = math.sin(rotation_radians)
    source_head = source["head_center"]
    target_head = target["head_center"]
    transformed_head_x = scale * (
        cosine * source_head["x"] - sine * source_head["y"]
    )
    transformed_head_y = scale * (
        sine * source_head["x"] + cosine * source_head["y"]
    )
    return {
        "scale": scale,
        "rotation_degrees": rotation,
        "translation_x": target_head["x"] - transformed_head_x,
        "translation_y": target_head["y"] - transformed_head_y,
    }


def transform_point(point: dict, transform: dict) -> dict[str, float]:
    angle = math.radians(transform["rotation_degrees"])
    cosine = math.cos(angle)
    sine = math.sin(angle)
    scale = transform["scale"]
    return {
        "x": scale * (cosine * point["x"] - sine * point["y"])
        + transform["translation_x"],
        "y": scale * (sine * point["x"] + cosine * point["y"])
        + transform["translation_y"],
    }


def build_probe_skeleton() -> dict:
    actual_hash = sha256(BASE_JSON)
    if actual_hash != EXPECTED_BASE_SHA256:
        raise RuntimeError(
            f"Canonical V10 hash changed: expected {EXPECTED_BASE_SHA256}, got {actual_hash}"
        )
    skeleton = json.loads(BASE_JSON.read_text())
    skeleton["animations"]["task2_switch_probe"] = {
        "bones": {
            "hips": {
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
            "torso": {
                "rotate": [
                    {"time": 0.0, "value": 0.0},
                    {"time": 0.12, "value": -4.0},
                    {"time": 0.30, "value": -20.0},
                ]
            },
        }
    }
    return skeleton


def write_probe_project() -> None:
    if SCRATCH_ROOT.name != "task2-switch-alignment-probe":
        raise RuntimeError(f"Refusing to replace unexpected scratch path: {SCRATCH_ROOT}")
    shutil.rmtree(SCRATCH_ROOT, ignore_errors=True)
    (SCRATCH_ROOT / "animation/images").mkdir(parents=True)
    shutil.copytree(BASE_PROJECT / "bin", SCRATCH_ROOT / "bin")
    shutil.copy2(BASE_ATLAS, SCRATCH_ROOT / "animation/togawasakiko_v2.atlas")
    shutil.copy2(BASE_RIG_SHEET, SCRATCH_ROOT / "animation/images/rig_sheet.png")
    (SCRATCH_ROOT / "animation/togawasakiko_v2.spine-json").write_text(
        json.dumps(build_probe_skeleton(), separators=(",", ":"))
    )
    (SCRATCH_ROOT / "animation/togawasakiko_v2_skel_data.tres").write_text(
        """[gd_resource type="SpineSkeletonDataResource" load_steps=3 format=3]

[ext_resource type="SpineAtlasResource" path="res://animation/togawasakiko_v2.atlas" id="1_atlas"]
[ext_resource type="SpineSkeletonFileResource" path="res://animation/togawasakiko_v2.spine-json" id="2_skel"]

[resource]
atlas_res = ExtResource("1_atlas")
skeleton_file_res = ExtResource("2_skel")
default_mix = 0.0
config_version = 5
"""
    )
    (SCRATCH_ROOT / "project.godot").write_text(
        """config_version=5

[application]
config/name="Task 2 Switch Alignment Probe"
run/main_scene="res://probe_scene.tscn"
config/features=PackedStringArray("4.5", "Forward Plus")

[display]
window/size/viewport_width=1024
window/size/viewport_height=1536
window/size/window_width_override=512
window/size/window_height_override=768

[rendering]
renderer/rendering_method="gl_compatibility"
renderer/rendering_method.mobile="gl_compatibility"
environment/defaults/default_clear_color=Color(0, 0, 0, 0)
"""
    )
    (SCRATCH_ROOT / "probe_scene.tscn").write_text(
        """[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://capture_probe.gd" id="1_script"]
[ext_resource type="SpineSkeletonDataResource" path="res://animation/togawasakiko_v2_skel_data.tres" id="2_skel"]

[node name="Probe" type="Node2D"]
script = ExtResource("1_script")

[node name="RenderViewport" type="SubViewport" parent="."]
transparent_bg = true
size = Vector2i(1024, 1536)
render_target_update_mode = 4

[node name="Visuals" type="SpineSprite" parent="RenderViewport"]
position = Vector2(512, 686.666667)
scale = Vector2(0.773333333, 0.773333333)
skeleton_data_res = ExtResource("2_skel")
preview_skin = "default"
preview_animation = "task2_switch_probe"
"""
    )
    (SCRATCH_ROOT / "capture_probe.gd").write_text(
        """extends Node2D

@onready var render_viewport: SubViewport = $RenderViewport
@onready var visuals: Node = $RenderViewport/Visuals

func _ready() -> void:
	call_deferred("_capture")

func _capture() -> void:
	var state: Object = visuals.call("get_animation_state")
	var entry: Object = state.call("set_animation", "task2_switch_probe", false, 0)
	entry.call("set_track_time", 0.30)
	for _frame in 3:
		await get_tree().process_frame
		RenderingServer.force_draw()
	var image := render_viewport.get_texture().get_image()
	var output := ProjectSettings.globalize_path("res://standing_at_switch_probe.png")
	var error := image.save_png(output)
	if error != OK:
		push_error("Probe save failed: %s" % error)
		get_tree().quit(2)
		return
	print("TASK2_SWITCH_PROBE=", output)
	get_tree().quit(0)
"""
    )


def render_probe() -> None:
    write_probe_project()
    environment = os.environ.copy()
    environment["GODOT_USER_DATA_DIR"] = str(SCRATCH_ROOT / "user-data")
    import_result = subprocess.run(
        [str(GODOT), "--headless", "--editor", "--path", str(SCRATCH_ROOT), "--quit-after", "10"],
        env=environment,
        capture_output=True,
        text=True,
        timeout=60,
    )
    if import_result.returncode != 0:
        raise RuntimeError(f"Godot import failed:\n{import_result.stdout}\n{import_result.stderr}")
    render_result = subprocess.run(
        [str(GODOT), "--path", str(SCRATCH_ROOT), "--fixed-fps", "60"],
        env=environment,
        capture_output=True,
        text=True,
        timeout=60,
    )
    if render_result.returncode != 0 or not PROBE_IMAGE.exists():
        raise RuntimeError(f"Godot probe render failed:\n{render_result.stdout}\n{render_result.stderr}")
    with Image.open(PROBE_IMAGE) as probe:
        probe = probe.convert("RGBA")
        content_bbox = probe.getchannel("A").getbbox()
        if probe.size != CANVAS_SIZE:
            raise RuntimeError(
                f"Probe must be {CANVAS_SIZE[0]}x{CANVAS_SIZE[1]}, got {probe.size}"
            )
        if content_bbox is None:
            raise RuntimeError("Probe render is empty")
        left, top, right, bottom = content_bbox
        if left <= 0 or top <= 0 or right >= CANVAS_SIZE[0] or bottom >= CANVAS_SIZE[1]:
            raise RuntimeError(f"Probe character is cropped: content bbox {content_bbox}")


def transformed_image(image: Image.Image, transform: dict) -> Image.Image:
    angle = math.radians(transform["rotation_degrees"])
    cosine = math.cos(angle)
    sine = math.sin(angle)
    scale = transform["scale"]
    translation_x = transform["translation_x"]
    translation_y = transform["translation_y"]
    inverse = (
        cosine / scale,
        sine / scale,
        -(cosine * translation_x + sine * translation_y) / scale,
        -sine / scale,
        cosine / scale,
        (sine * translation_x - cosine * translation_y) / scale,
    )
    return image.transform(
        CANVAS_SIZE,
        Image.Transform.AFFINE,
        inverse,
        resample=Image.Resampling.BICUBIC,
        fillcolor=(0, 0, 0, 0),
    )


def build_alignment_preview() -> None:
    if set(STANDING_AT_SWITCH) != {"head_center", "hip_center"}:
        raise RuntimeError("Measure STANDING_AT_SWITCH landmarks before building the overlay")
    if set(FALL_ATTACHMENT) != {"head_center", "hip_center"}:
        raise RuntimeError("Measure FALL_ATTACHMENT landmarks before building the overlay")
    render_probe()
    reference = Image.open(PROBE_IMAGE).convert("RGBA")
    candidate = Image.open(CANDIDATE).convert("RGBA")
    if reference.size != CANVAS_SIZE or candidate.size != CANVAS_SIZE:
        raise ValueError("Both source images must use the 1024x1536 coordinate system")

    transform = compute_similarity_transform(FALL_ATTACHMENT, STANDING_AT_SWITCH)
    aligned_candidate = transformed_image(candidate, transform)
    candidate_opacity = aligned_candidate.getchannel("A").point(lambda alpha: alpha // 2)
    aligned_candidate.putalpha(candidate_opacity)
    overlay = Image.alpha_composite(reference, aligned_candidate)

    transformed_landmarks = {
        name: transform_point(point, transform)
        for name, point in FALL_ATTACHMENT.items()
    }
    landmark_errors = {
        name: math.hypot(
            transformed_landmarks[name]["x"] - STANDING_AT_SWITCH[name]["x"],
            transformed_landmarks[name]["y"] - STANDING_AT_SWITCH[name]["y"],
        )
        for name in transformed_landmarks
    }
    draw = ImageDraw.Draw(overlay)
    for name, point in STANDING_AT_SWITCH.items():
        x, y = point["x"], point["y"]
        color = (255, 211, 72, 255) if name == "head_center" else (55, 226, 255, 255)
        draw.ellipse((x - 8, y - 8, x + 8, y + 8), outline=color, width=3)
        draw.line((x - 12, y, x + 12, y), fill=color, width=2)
        draw.line((x, y - 12, x, y + 12), fill=color, width=2)

    manifest = {
        "standing_at_switch": STANDING_AT_SWITCH,
        "fall_attachment": FALL_ATTACHMENT,
        "transform": transform,
        "reference": {
            "canvas_width": CANVAS_SIZE[0],
            "canvas_height": CANVAS_SIZE[1],
            "canonical_v10_sha256": EXPECTED_BASE_SHA256,
            "probe_animation": "task2_switch_probe",
            "probe_time_seconds": PROBE_TIME_SECONDS,
            "probe_spine_sprite": {
                "position_x": 512.0,
                "position_y": 686.666667,
                "uniform_scale": 0.773333333,
            },
            "probe_pose": {
                "hips": {"x": 12.0, "y": -18.0, "rotation_degrees": -8.0},
                "torso": {"rotation_degrees": -20.0},
            },
            "rendered_probe_sha256": sha256(PROBE_IMAGE),
        },
        "qa": {
            "standing_probe_content_bbox": list(reference.getchannel("A").getbbox()),
            "transformed_candidate_content_bbox": list(
                aligned_candidate.getchannel("A").getbbox()
            ),
            "transformed_landmarks": transformed_landmarks,
            "landmark_error_pixels": landmark_errors,
            "max_landmark_error_pixels": max(landmark_errors.values()),
            "overlay_candidate_opacity": 0.5,
        },
    }
    ANCHORS.write_text(json.dumps(manifest, indent=2) + "\n")
    OVERLAY.parent.mkdir(parents=True, exist_ok=True)
    overlay.save(OVERLAY)


if __name__ == "__main__":
    if sys.argv[1:] == ["--render-probe-only"]:
        render_probe()
        print(PROBE_IMAGE)
    elif sys.argv[1:]:
        raise SystemExit("Usage: build_switch_alignment_preview.py [--render-probe-only]")
    else:
        build_alignment_preview()
