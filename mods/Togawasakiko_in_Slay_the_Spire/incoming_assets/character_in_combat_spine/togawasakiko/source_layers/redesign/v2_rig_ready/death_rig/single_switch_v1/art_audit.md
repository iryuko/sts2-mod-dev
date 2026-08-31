# Single-Switch Fall/Prone Art Audit

## Reference Authority

- `reference_lock/blueprint_idle_locked.png` is the exclusive authority for Sakiko's identity, face, costume, proportions, palette, materials, and painted finish.
- `death_rig/blueprints/death_pose_01_kneel_candidate_v1.png`, `death_pose_02_brace_candidate_v2.png`, and `death_pose_03_prone_candidate_v1.png` were used only for gravity, limb-contact intent, and the ground relationship. They do not override the locked idle reference for identity or styling.

## Candidate Inspection

Candidate: `fall_prone_candidate_v3_idle_locked.png`

| Field | Result | Evidence |
| --- | --- | --- |
| Face | PASS | The accepted side-prone profile preserves the locked idle face construction, small head scale, and closed-eye terminal expression. |
| Silver-blue hair | PASS | Cold silver-blue hair, tied rear hair, and the long grounded hair mass follow the locked reference's color family and rendering. |
| Costume construction | PASS | Dark red puff sleeves, black corset, black-and-ivory layered skirt, restrained gold trim, black gloves, and boots are preserved from the locked idle design. |
| Limb length | PASS | Both arms and legs read as continuous, proportionate limbs without added limbs or visible joint breaks. |
| Right-facing orientation | PASS | Head and upper body remain directed toward screen right in the accepted side-prone pose. |
| Halo | PASS | The restrained gold thorn halo remains present behind the head. |
| Transparent edges | PASS | The selected RGB render was converted to true RGBA with alpha extrema `(0, 255)`. Its single connected silhouette is bounded at `(26, 682, 996, 1078)` with no checkerboard remnants or detached visible speckles. |
| Fall readability | PASS | The body is already fully prone; it is intentionally not used as a rotating in-between fall frame. |
| Prone credibility | PASS | The near-horizontal side-prone silhouette rests coherently on the ground with relaxed limbs, settled skirt layers, and grounded hair. |

## Result

The V1 attachment was rejected for noisy rendering and style drift. V2 fixed the extraction path but was superseded by the user-selected V3 art. V3 was generated directly from the locked idle reference, selected without further pose edits, then converted from a neutral checkerboard RGB render to a clean single-component alpha silhouette. It is used only as the terminal prone attachment.

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

### Idle-Locked V5 (`0.21s`)

Evidence:

- `single_switch_death_v5_idle_locked_021s_20260831.mp4`

| Field | Result | Evidence |
| --- | --- | --- |
| Selected art | PASS | The user explicitly selected the side-prone V3 image; no later face-down variant is used. |
| Total duration | PASS | Godot records 13 frames at 60 FPS, yielding `0.216667s`; authored duration remains `0.21s`. |
| Foot alignment | PASS | Measured preview coordinates are standing left boot `x=252` and prone left toe `x=253`, a 1px difference. |
| Final framing | PASS | The settled silhouette spans `x=253..1156` and ends at `y=995` on the `1280x1024` preview, leaving safe right and lower margins. |
| Runtime loading | PASS | Godot 4.5.1 loads `idle_loop`, `attack`, `cast`, `hurt`, `relaxed_loop`, and `die` from the rebuilt isolated project. |
