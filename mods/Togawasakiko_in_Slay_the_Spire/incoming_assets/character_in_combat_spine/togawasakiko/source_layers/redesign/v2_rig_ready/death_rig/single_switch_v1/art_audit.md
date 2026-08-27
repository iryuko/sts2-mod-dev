# Single-Switch Fall/Prone Art Audit

## Reference Authority

- `reference_lock/blueprint_idle_locked.png` is the exclusive authority for Sakiko's identity, face, costume, proportions, palette, materials, and painted finish.
- `death_rig/blueprints/death_pose_01_kneel_candidate_v1.png`, `death_pose_02_brace_candidate_v2.png`, and `death_pose_03_prone_candidate_v1.png` were used only for gravity, limb-contact intent, and the ground relationship. They do not override the locked idle reference for identity or styling.

## Candidate Inspection

Candidate: `fall_prone_candidate_v1.png`

| Field | Result | Evidence |
| --- | --- | --- |
| Face | PASS | The downward, right-facing partial profile is intentionally obscured by the prone pose while retaining the locked reference's facial construction. |
| Silver-blue hair | PASS | Cold silver-blue hair, tied rear hair, and trailing strands match the locked reference's color family and construction. |
| Costume construction | PASS | Dark red puff sleeve, black corset, black-and-ivory layered skirt, restrained gold trim, black gloves, and boots are present. |
| Limb length | PASS | Both arms and legs read as continuous, proportionate limbs without added limbs or visible joint breaks. |
| Right-facing orientation | PASS | Head, hands, and body read toward screen right; the face is directed down and away from camera. |
| Halo | PASS | The restrained gold thorn halo remains present behind the head. |
| Transparent edges | PASS | RGBA alpha extrema are `(0, 255)`; the opaque silhouette is bounded at `(23, 693, 1005, 977)`, leaving clear transparent canvas around it. |
| Fall readability | PASS | Arms extend forward, torso is committed low, hair trails back, and the body reads as the final moment of a forward collapse. |
| Prone credibility | PASS | The near-horizontal, face-down silhouette rests coherently with forearms and body aligned for a prone hold. |

## Result

All required visual acceptance fields pass. The asset is suitable as a single attachment that can serve both the forward-fall transition and the prone hold. No PNG regeneration was needed.

## Runtime Motion Audit

### Rejected V1

`single_switch_death_v1_20260827.mp4` was rejected after runtime inspection.
The fall-root X keys did not compensate for the attachment rotation, so the
boot contact drifted left while the body rotated. The 768px preview also
clipped both ends of the prone silhouette.

### Grounded V2

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
| Switch pop | PENDING USER ACCEPTANCE | The design intentionally uses one hard attachment switch at `0.30s`; normal-speed acceptance remains subjective. |
