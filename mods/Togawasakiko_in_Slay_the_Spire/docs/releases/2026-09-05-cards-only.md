# 2026-09-05 0.2.2

## Source Scope

- Thirteen approved cards: Unfinished Score, Following Phrase, Unmask, Rehearsal Order, Backstage Support, Unspoken Words, Lingering Resonance, Until Next Act, Composed Response, Octagram Dance, DIVINE, In Your Blue Eyes, and The Whole Blue World.
- Includes their powers, registration, localization, card portraits, native replay/payment patches and regression tests. Retains the recent Pressure redemption and Two Moons card-interaction fixes.
- Based on remote commit `8b5e17ee6fe9d6b98352ba7ac7415d65ac2d2b5a`. Existing animation source history is inherited, not resubmitted.
- No new animation, thorn VFX, creature scene, event-room, entry-point or unrelated research changes. Shared working files, branch and index are untouched.
- The earlier local 121-file release candidate is superseded and must not be uploaded.

## Verification

- The isolated card-only source snapshot builds with zero errors and four nullable warnings in unchanged baseline helper code.
- All 107 gameplay regression checks pass against the actual STS2 v0.107.1 assemblies.
- Tests cover card costs, native upgrade/downgrade and descriptions, keywords, generation, Pressure, replay and per-player end-turn boundaries. They do not prove live multiplayer transport, room transitions, reward timing or in-game visuals.
- Eight portraits and three power aliases are included. Five bridge-card portraits and the power aliases still reuse existing art.
- Git source publication does not itself mean the binary Release has been published. The packaged animation baseline must be resolved independently; release title and body remain date and version only.

## 2026-09-06 Local Post-release Art Hotfix

- The five bridge cards above originally shared the `basic/unendurable.png` placeholder in the published `0.2.2` source state.
- The follow-up assigns five dedicated `1000x760` portraits and is promoted as the separate `0.2.3` release rather than replacing `0.2.2`.
- In-game visual framing remains unverified.
