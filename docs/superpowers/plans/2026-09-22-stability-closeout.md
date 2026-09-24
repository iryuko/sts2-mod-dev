# Stability Closeout Implementation Plan

> **For agentic workers:** Use systematic debugging and test-driven development for each independent fix. Continue in the existing isolated worktree; the user authorized implementing the remaining audit items together.

**Goal:** Close all four unresolved findings in the September 22 audit without reimplementing native game flows.

**Architecture:** Event presentation uses native LocalContext and event-owned Godot nodes. Audio volume suppression must not overwrite saved preferences. Shadow reconstruction preserves native serialization metadata. Existing card/resource fixes remain intact.

**Tech Stack:** C#/.NET 9, STS2 v0.107.1/59260271, Godot 4.5.1, existing managed and native regression hosts.

**Spec:** `docs/audits/card-stall-and-risk-audit-2026-09-22.md`, plus user's request to fix all listed remaining issues.

## Global Constraints

- Base: 0.2.5, branch `codex/card-stall-fix-20260922`.
- Preserve unrelated edits, approved art/Spine, PCK and accepted energy VFX limitation.
- No guessed APIs: use fresh decompilation of current references.
- Tests and builds are serialized; workers own disjoint source/test files.
- Installation/release are not implied by passing tests; keep their status explicit.

## Review Focus

- Remote piano choices must update remote gameplay without changing local presentation.
- Removing the event scene without finishing must stop audio without reviving run music.
- Delayed old-event cleanup must not stop a newer event's audio.
- BGM volume changes during jukebox playback must preserve the latest saved preference and stay muted until Off/run exit.
- Loaded Shadows across all owners must retain floor and combat metadata; test cost restrictions and first-hit lethal replays separately.

## Tasks

- [x] Piano: managed tests reproduced three presentation-isolation failures; real Godot reproduced playback surviving node removal. LocalContext and event-node ownership fixes passed the managed suite and native removal/finish/reload/stale-cleanup tests. Normal finish restores music once; tree exit only frees audio. Full UI and multiplayer transport remain live-test boundaries.
- [x] Jukebox: the native slider reproduced nonzero BGM output while custom playback was active. A narrow SetBgmVol argument filter preserves native settings/conversion; releasing the mute before restore preserves the newest preference. Five native cases pass, with no per-frame polling or changes to other channels.
- [x] Shadow: all three loaded Shadow types reproduced lost acquisition floors. The one-line metadata copy passed native serialization/reconstruction checks for three owners, floor 0/nonzero/null, repeated migration, and independent instances. Ave Mujica star-only/mixed restrictions and Magnetic Force first-hit lethal behavior now have passing edge coverage. Managed suite: 147 passed, 0 failed.
- [x] Integrate: 147 managed checks, 11 native resource/event/jukebox cases, 28 Python checks, Mac/Win v0.107.1 builds and diff checks pass. Native fixture cleanup now uses real Node.Free and reports no ERROR/leak warnings. Bounded cross-review found no important production regression. Audit and both workspace handoff pointers are updated; no install, push or release was performed.

## Validation Commands

September 24 follow-up: the user authorized pushing. Fast-forwarded to main
`b2f70818` to retain the merged card workbench, committed code as `f66d1c38`,
and opened [PR #10](https://github.com/iryuko/sts2-mod-dev/pull/10). Fresh managed,
Python and dual-reference builds passed. The native runner exposed a fixed-delay
fixture cleanup race; it now waits for native reference ownership with a bounded
timeout. Five consecutive native reruns passed without errors/leaks. No merge,
installation or Release was performed.

```bash
bash shared/scripts/test-togawasakiko-gameplay.sh
bash shared/scripts/test-togawasakiko-resources.sh
python3 -m unittest discover -s mods/Togawasakiko_in_Slay_the_Spire/tests -p 'test_*.py'
python3 mods/Togawasakiko_in_Slay_the_Spire/scripts/verify-api-compatibility.py --mac-reference-dir references/game-dlls/sts2/arm64 --windows-reference-dir /Users/user/Desktop/sts2-mod-dev/references/windows-sts2/2026-07-03/data_sts2_windows_x86_64
git diff --check
```

Runtime tests establish native logic/resource behavior; they do not certify Windows execution, multiplayer transport or complete interactive gameplay.
