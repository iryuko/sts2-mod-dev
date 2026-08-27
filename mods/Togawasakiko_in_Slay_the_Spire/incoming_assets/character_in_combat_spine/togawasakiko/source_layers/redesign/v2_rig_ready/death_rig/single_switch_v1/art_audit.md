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
