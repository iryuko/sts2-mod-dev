# Workspace cleanup and main-menu mod error audit (2026-07-26)

## Result

- The main-menu red `MODDED_WARNING` did not come from a failed Togawasakiko initializer.
- A normal Steam launch loaded the Togawasakiko DLL and PCK, ran its initializer, and logged `Finished mod initialization`.
- The red state was caused by the disabled Watcher mod's old dependency schema.
- The workspace cleanup released about 5.37 GiB (5.76 GB) without removing current source, research evidence, source assets, or required reference data.

## Red warning mechanism

The current reference assembly is STS2 `v0.107.1` (`commit 59260271`).

`ModManifest.ReadFromStream(...)` still accepts an old dependency array such as:

```json
"dependencies": ["BaseLib"]
```

It migrates that entry in memory to a `ModDependency`, but also adds `MOD_ERROR.MIGRATION_REQUIRED` to the mod's error list.

`NDebugInfoLabelManager.UpdateText(...)` computes the main-menu error state across every detected mod:

- a failed mod counts as an error;
- any mod with a non-empty `errors` list also counts;
- disabled mods are not excluded.

The installed Watcher manifest used the old dependency array. It was disabled, but its migration error still made the global warning red. Togawasakiko itself only emitted a warning for an omitted minimum game version and completed initialization.

## Corrections

- Migrated the workspace and installed Watcher manifests to the current dependency object:

```json
{
  "id": "BaseLib",
  "min_version": null
}
```

- Added `"min_game_version": "0.107.1"` to the Togawasakiko source and PCK manifests.
- Updated `shared/scripts/build-mod.sh` so the generated external loader manifest preserves `min_game_version`.
- Rebuilt and installed Togawasakiko through the standard scripts.

Verification:

- release and installed DLL/PCK/manifest hashes match;
- a second Steam launch produced no mod-loader or mod-initialization `ERROR` lines before shutdown;
- no old dependency migration error or missing minimum-version warning remained;
- the main-menu status returned to the normal non-error color.

On normal process shutdown, Godot still prints generic renderer/resource leak
diagnostics. Those lines appear after Steamworks and FMOD have shut down and are
not part of the mod-loader error state described here.

## Removed content

The following were inspected before removal:

- superseded Togawasakiko release zips and matching hash sidecars;
- the unreferenced `2026-04-10` local test package;
- top-level `out`, which duplicated the retained `local/tmp/sts2-arm64.il` content plus one tool warning;
- downloaded Godot and Spine extension archives whose extracted tools remain present and are the paths used by scripts;
- target-mod `.build`, `src/bin`, `src/obj`, and `pack/.godot` build/import caches;
- `.godot` caches inside local Spine proof projects, while retaining every project source and asset;
- Finder `.DS_Store`, R `.Rhistory`, and the empty top-level `12` file.

One documented July 4 compatibility package remains as a regression baseline:

- `Togawasakiko_in_Slay_the_Spire-v0.2.1-20260704-v0107-api-sync-no-energy-scene.zip`

The current unpacked release directory also remains intact.

## Explicitly preserved

- `references/pck-extract/sts2-main/.godot/imported`
  - This is not disposable in the current workspace: asset export manifests and Spine research refer directly to the imported `.ctex` / `.spatlas` files.
- `references/game-dlls` and `references/windows-sts2`
  - Required for current Mac/Win API comparison.
- `local/model-cache/segment-anything`
  - Referenced by the current combat-art layer extraction script.
- `local/tools/spine-runtimes-4.2`, the Spine proof projects, and combat-art source layers.
- The referenced T5 multiplayer regression package under `local/releases`.
- All source code, documentation, current release artifacts, incoming assets, and original art references.
- `.git`
  - `git count-objects` reported no garbage; aggressive history cleanup would discard useful repository history for little justified benefit.
