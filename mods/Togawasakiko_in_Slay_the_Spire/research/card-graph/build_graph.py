#!/usr/bin/env python3
"""Build an evidence-backed research graph; never modify gameplay source."""

import argparse
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess


HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
KINDS = {"supply", "payoff", "sequence", "replay", "tradeoff", "conflict", "generation"}


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def validate_coverage(cards, inventory):
    ids = [card["id"] for card in cards]
    duplicates = [key for key, value in Counter(ids).items() if value > 1]
    if duplicates:
        raise ValueError(f"duplicate card annotations: {duplicates}")
    missing = set(inventory) - set(ids)
    extra = set(ids) - set(inventory)
    if missing or extra:
        raise ValueError(f"Card coverage mismatch: missing={sorted(missing)}, extra={sorted(extra)}")


def normalize_edges(cards, relations):
    nodes = {card["id"]: card for card in cards}
    found = {}
    for original in relations:
        edge = dict(original)
        for endpoint in ("source", "target"):
            if edge.get(endpoint) not in nodes:
                raise ValueError(f"Unknown {endpoint}: {edge.get(endpoint)}")
        for required in ("mechanism", "condition", "reason", "evidence"):
            if not edge.get(required):
                raise ValueError(f"Missing edge {required}: {edge}")
        if edge.get("kind") not in KINDS:
            raise ValueError(f"Unknown edge kind: {edge.get('kind')}")
        if not isinstance(edge["evidence"], list):
            raise ValueError("Evidence must be a list")
        if edge["source"] == edge["target"] and edge.get("self_reviewed") is not True:
            raise ValueError(f"Self-edge needs a separately reviewed rule: {edge['source']}")
        edge["status"] = "proposal" if any(
            nodes[edge[key]]["status"] == "proposal" for key in ("source", "target")
        ) else "implemented"
        key = tuple(edge[field] for field in ("source", "target", "kind", "mechanism", "condition"))
        if key in found:
            found[key]["evidence"] = sorted(set(found[key]["evidence"] + edge["evidence"]))
        else:
            edge["id"] = "e-" + hashlib.sha256(json.dumps(key).encode()).hexdigest()[:14]
            found[key] = edge
    return sorted(found.values(), key=lambda e: e["id"])


def source_inventory(mod):
    """Read the explicitly supported local C# declarations, failing on drift.

    This is a source inventory adapter, not a C# parser or a gameplay interpreter.
    Effects/relationships stay human-reviewed in the catalog JSON files.
    """
    support = (mod / "src/ModSupport.cs").read_text()
    song_block = support.split("SongCardEntries =", 1)[1].split("};", 1)[0]
    songs = re.findall(r'"([A-Z0-9_]+)"', song_block)
    character = (mod / "src/Characters/Togawasakiko.cs").read_text()
    pool_block = character.split("internal sealed class TogawasakikoCardPool", 1)[1].split(
        "internal sealed class TogawasakikoPotionPool", 1)[0]
    main_pool = set(re.findall(r"ModelDb.Card<(\w+)>", pool_block))
    portrait_dirs = dict(re.findall(
        r'public static string (Get\w+PortraitPath)\(string fileName\)\s*\{\s*return \$"res://mod_assets/(.*?)/\{fileName\}";', support))
    inventory = {}
    for path in sorted((mod / "src/Cards").glob("*.cs")):
        text = path.read_text()
        declarations = list(re.finditer(r"^internal (sealed|abstract) class (\w+)\s*:\s*([^\n]+)", text, re.M))
        for i, declaration in enumerate(declarations):
            if declaration[1] != "sealed":
                continue
            name, parent = declaration[2], declaration[3]
            body = text[declaration.start():declarations[i + 1].start() if i + 1 < len(declarations) else len(text)]
            constructor = re.search(r": base\((\d+), CardType\.(\w+)(?:, CardRarity\.(\w+))?", body)
            if "ShadowOfThePastCard" in parent:
                cost, card_type, rarity, pool = 0, "Curse", "Event", "event"
            elif constructor:
                cost, card_type, rarity = int(constructor[1]), constructor[2], constructor[3]
                if "GeneratedPressureCard" in parent:
                    rarity, pool = "Token", "token"
                else:
                    pool = "main" if name in main_pool else "relic"
            else:
                raise ValueError(f"Unsupported constructor: {name}; review inventory adapter")
            if not rarity:
                raise ValueError(f"Missing rarity: {name}")
            song_entries = [entry for entry in songs if entry.replace("_", "").lower() == name.lower()]
            if len(song_entries) > 1:
                raise ValueError(f"Ambiguous Song identity: {name}")
            portrait = re.search(r'ModSupport\.(Get\w+PortraitPath)\("([^"]+)"\)', body)
            asset = None
            if portrait:
                asset = mod / "assets" / portrait_dirs[portrait[1]] / portrait[2]
            elif pool == "token":
                filename = re.search(r'TargetType\.\w+, "([^"]+\.png)"', body)
                asset = mod / "assets/cards/generated_pressure" / filename[1]
            if not asset or not asset.is_file():
                raise ValueError(f"Missing inspected portrait for {name}: {asset}")
            inventory[name] = {
                "type": card_type, "rarity": rarity, "pool": pool,
                "cost": "X" if re.search(r"new CardEnergyCost\(this,\s*0,\s*true\)", body) else cost,
                "song": bool(song_entries),
                "reward": pool == "main" and rarity in {"Common", "Uncommon", "Rare"},
                "source": path.relative_to(mod).as_posix(),
                "source_line": text[:declaration.start()].count("\n") + 1,
                "portrait": Path(os.path.relpath(asset, HERE)).as_posix(),
            }
    if sum(card["song"] for card in inventory.values()) != len(songs):
        raise ValueError("Song registry has an unmapped concrete card")
    if main_pool - inventory.keys():
        raise ValueError("Card pool contains an unparsed concrete card")
    return inventory


def expand_families(nodes, families):
    by_id = {node["id"]: node for node in nodes}

    def select(selector):
        if isinstance(selector, list):
            missing = set(selector) - by_id.keys()
            if missing:
                raise ValueError(f"Unknown family cards: {sorted(missing)}")
            return selector
        return [n["id"] for n in nodes if n["status"] == "implemented"
                and all(n.get(key) == value for key, value in selector.items())]

    edges = []
    for family in families:
        for source in select(family["from"]):
            for target in select(family["to"]):
                if source == target or [source, target] in family.get("except", []):
                    continue
                edge = {key: value for key, value in family.items() if key not in {"from", "to", "except", "id"}}
                edge.update(source=source, target=target, family=family["id"])
                edges.append(edge)
    return edges


def resolve_evidence(reference, mod, revision):
    if reference.startswith("proposal:"):
        return {"reference": reference, "proposal": True}
    path, separator, anchor = reference.partition("#")
    target = mod / path
    if not separator or not target.is_file():
        raise ValueError(f"Missing evidence file/anchor: {reference}")
    lines = target.read_text().splitlines()
    matches = [i + 1 for i, line in enumerate(lines) if anchor in line]
    if not matches:
        raise ValueError(f"Missing evidence symbol: {reference}")
    declarations = [i for i in matches if re.search(r"\b(class|public|private|protected|internal)\b", lines[i - 1])]
    line = (declarations or matches)[0]
    return {"reference": reference, "path": path, "line": line,
            "url": f"https://github.com/iryuko/sts2-mod-dev/blob/{revision}/mods/Togawasakiko_in_Slay_the_Spire/{path}#L{line}"}


def validate_clean_source(source_root, mod_directory):
    status = subprocess.check_output(
        ["git", "-C", str(source_root), "status", "--porcelain", "--untracked-files=all",
         "--", f"{mod_directory}/src"], text=True)
    if status.strip():
        raise ValueError("Uncommitted gameplay source: choose a clean, reviewed baseline")


def tracked_source_hashes(mod):
    paths = subprocess.check_output(
        ["git", "-C", str(mod), "ls-files", "-z", "--", "src"], text=True).split("\0")
    return {name: hashlib.sha256((mod / name).read_bytes()).hexdigest()
            for name in sorted(paths) if name.endswith(".cs")}


def build(write=True, source_root=None):
    config = read_json(HERE / "graph-config.json")
    source_root = Path(source_root) if source_root else REPO / config["source_worktree"]
    tree = subprocess.check_output(["git", "-C", str(source_root), "rev-parse", "HEAD^{tree}"], text=True).strip()
    if tree != config["source_tree"]:
        raise ValueError("Source tree changed: review cards and update graph-config.json before rebuilding")
    validate_clean_source(source_root, config["mod_directory"])
    mod = source_root / config["mod_directory"]
    inventory = source_inventory(mod)
    catalogs = [read_json(HERE / name) for name in ("catalog-core.json", "catalog-songs.json")]
    cards = [card for catalog in catalogs for card in catalog["cards"]]
    validate_coverage(cards, inventory)
    nodes = []
    for card in cards:
        if card["source"] != inventory[card["id"]]["source"]:
            raise ValueError(f"Wrong source annotation: {card['id']}")
        nodes.append({**card, **inventory[card["id"]], "status": "implemented"})
    proposals = read_json(HERE / "proposals.json")
    for card in proposals["cards"]:
        if card["id"] in inventory or not card["id"].startswith("proposal:"):
            raise ValueError(f"Proposal needs a distinct identifier: {card['id']}")
        nodes.append({**card, "status": "proposal", "pool": "proposal", "reward": False, "song": False})
    relations = [edge for catalog in catalogs for edge in catalog["relations"]]
    families = read_json(HERE / "relation-families.json")
    relations += expand_families(nodes, families["families"])
    relations += families.get("reviewed_relations", [])
    relations += proposals["relations"]
    edges = normalize_edges(nodes, relations)
    references = {ref for edge in edges for ref in edge["evidence"]}
    references |= {f"{n['source']}#{n['id']}" for n in nodes if n["status"] == "implemented"}
    evidence = {ref: resolve_evidence(ref, mod, config["source_revision"]) for ref in sorted(references)}
    hashes = tracked_source_hashes(mod)
    result = {"meta": config, "source_hashes": hashes,
              "nodes": sorted(nodes, key=lambda n: n["id"]), "edges": edges, "evidence": evidence,
              "counts": {"implemented": len(inventory), "proposals": len(proposals["cards"]),
                         "edges": len(edges), "kinds": dict(Counter(e["kind"] for e in edges))}}
    if write:
        data = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
        (HERE / "graph.json").write_text(data, encoding="utf-8")
        (HERE / "graph-data.js").write_text("window.CARD_GRAPH = " + data.rstrip() + ";\n", encoding="utf-8")
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-root", type=Path)
    parser.add_argument("--check", action="store_true", help="Reject stale checked-in graph output")
    args = parser.parse_args()
    graph = build(write=not args.check, source_root=args.source_root)
    if args.check:
        if graph != read_json(HERE / "graph.json"):
            raise SystemExit("Stale graph.json; rebuild after reviewing annotations")
        expected = "window.CARD_GRAPH = " + json.dumps(graph, ensure_ascii=False, indent=2) + ";\n"
        if (HERE / "graph-data.js").read_text() != expected:
            raise SystemExit("Stale graph-data.js; rebuild")
    print(json.dumps(graph["counts"], ensure_ascii=False))


if __name__ == "__main__":
    main()
