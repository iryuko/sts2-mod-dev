# Single-Switch Fall/Prone Art Audit

## Reference Authority

- `reference_lock/blueprint_idle_locked.png` is the exclusive authority for Sakiko's identity, face, costume, proportions, palette, materials, and painted finish.
- `death_rig/blueprints/death_pose_01_kneel_candidate_v1.png`, `death_pose_02_brace_candidate_v2.png`, and `death_pose_03_prone_candidate_v1.png` were used only for gravity, limb-contact intent, and the ground relationship. They do not override the locked idle reference for identity or styling.

## Candidate Inspection

Candidate: `fall_prone_candidate_v2_clean.png`

| Field | Result | Evidence |
| --- | --- | --- |
| Face | PASS | The face is mostly hidden by the terminal face-down pose; the visible profile follows the locked idle construction instead of the degraded V1 face. |
| Silver-blue hair | PASS | Cold silver-blue hair, tied rear hair, and trailing strands follow the locked reference's color family and rendering. |
| Costume construction | PASS | Dark red puff sleeves, black corset, black-and-ivory layered skirt, restrained gold trim, black gloves, and boots are preserved from the locked idle design. |
| Limb length | PASS | Both arms and legs read as continuous, proportionate limbs without added limbs or visible joint breaks. |
| Right-facing orientation | PASS | Head, hands, and body read toward screen right; the face is directed down and away from camera. |
| Halo | PASS | The restrained gold thorn halo remains present behind the head. |
| Transparent edges | PASS | The image is true RGBA with alpha extrema `(0, 255)`. Its connected silhouette is bounded at `(11, 800, 1018, 1065)` with no isolated visible speckles. |
| Fall readability | PASS | The body is already fully prone; it is intentionally not used as a rotating in-between fall frame. |
| Prone credibility | PASS | The near-horizontal, face-down silhouette rests coherently with forearms and body aligned for a prone hold. |

## Result

The V1 attachment was rejected for noisy rendering and style drift. V2 was redrawn from the locked idle reference, then converted from a neutral checkerboard RGB render to a clean single-component alpha silhouette. It is used only as the terminal prone attachment.

## Runtime Motion Audit

### Rejected V1

`single_switch_death_v1_20260827.mp4` was rejected after runtime inspection.
The fall-root X keys did not compensate for the attachment rotation, so the
boot contact drifted left while the body rotated. The 768px preview also
clipped both ends of the prone silhouette.

### Rejected Grounded V2

Evidence:

- `single_switch_death_v2_grounded_20260827.mp4`
- `single_switch_death_v2_grounded_contact_20260827.png`
- `single_switch_death_v2_grounding_grid.png`

| Field | Result | Evidence |
| --- | --- | --- |
| Forward-loss readability | PASS | The torso commits forward before the rapid rotation and descent. |
| Acceleration | PASS | Vertical displacement increases through the `0.68s` contact key. |
| Contact rebound | PASS | One restrained 4px rebound occurs at `0.74s`, followed by settlement. |
| Prone grounding | PASS | The boot contact is solved from the attachment toe point and remains on the same X line through the rotation. Frames 40-56 hold the foreground left edge at `x=244px`; the standing left boot is approximately `x=260px`. |
| Final framing | PASS | The preview is widened to 1280px without changing character scale; the final foreground spans `x=244..1150px`. |
| Face, costume, proportions, palette, materials | PASS | The accepted attachment is rigidly transformed; no warp, nonuniform scale, or RGBA crossfade is used. |
| Switch pop | REJECTED | The attachment remained visible while rotating from `0.30s` through `0.68s`, making the character read as a rigid paper cutout. |

### Instant V4 (`0.21s`)

Evidence:

- `single_switch_death_v4_instant_021s_aligned_20260827.mp4`
- `single_switch_death_v4_instant_021s_aligned_contact_20260827.png`

| Field | Result | Evidence |
| --- | --- | --- |
| Total duration | PASS | Godot records 13 frames at 60 FPS, yielding `0.216667s`; the authored animation duration is `0.21s`. |
| Instant collapse | PASS | Standing release occupies `0.00..0.075s`; the prone attachment appears fully grounded at `0.075s`. |
| Paper-cutout avoidance | PASS | The prone attachment uses its final rotation on its first visible frame. No visible full-character rotation sweep remains. |
| Impact settle | PASS | Contact occurs at `0.095s`, followed by one 4px rebound at `0.125s` and a settled hold from `0.155s`. |
| Foot alignment | PASS | Measured preview coordinates are standing left boot `x=255` and prone left toe `x=256`, a 1px difference. |
| Final framing | PASS | The final foreground ends at `y=982` on a 1024px-high preview, leaving a 42px safety margin. |
| Asset noise | PASS | The V2 alpha contract requires one connected visible silhouette; 115 detached extraction fragments totaling 338 pixels were removed before runtime use. |
