"""Contract checks for the research graph, not a gameplay simulator."""

import copy
import importlib.util
import subprocess
import tempfile
import unittest
from pathlib import Path


HERE = Path(__file__).resolve().parent


class GraphContracts(unittest.TestCase):
    def setUp(self):
        script = HERE / "build_graph.py"
        self.assertTrue(script.exists(), "The graph builder must exist")
        spec = importlib.util.spec_from_file_location("build_graph", script)
        self.builder = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.builder)

    def sample(self):
        cards = [
            {"id": "Source", "name": "Source", "status": "implemented"},
            {"id": "Target", "name": "Target", "status": "implemented"},
            {"id": "Draft", "name": "Draft", "status": "proposal"},
        ]
        edge = {"source": "Source", "target": "Target", "kind": "supply",
                "mechanism": "pressure", "condition": "Same living enemy",
                "reason": "Supplies the resource", "evidence": ["src/Card.cs#Source"]}
        return cards, edge

    def test_dangling_endpoint_cannot_silently_enter_graph(self):
        cards, edge = self.sample()
        edge["target"] = "Missing"
        with self.assertRaisesRegex(ValueError, "Missing"):
            self.builder.normalize_edges(cards, [edge])

    def test_shared_tags_alone_do_not_create_edges(self):
        cards, _ = self.sample()
        cards[0]["tags"] = cards[1]["tags"] = ["song"]
        self.assertEqual([], self.builder.normalize_edges(cards, []))

    def test_directed_and_opposing_edges_are_preserved(self):
        cards, edge = self.sample()
        reverse = dict(edge, source="Target", target="Source")
        conflict = dict(edge, kind="conflict", reason="Also spends the resource")
        result = self.builder.normalize_edges(cards, [edge, reverse, conflict, copy.deepcopy(edge)])
        self.assertEqual(3, len(result))
        self.assertEqual({("Source", "Target", "supply"),
                          ("Target", "Source", "supply"),
                          ("Source", "Target", "conflict")},
                         {(e["source"], e["target"], e["kind"]) for e in result})

    def test_proposal_endpoint_marks_edge_unimplemented(self):
        cards, edge = self.sample()
        edge["target"] = "Draft"
        result = self.builder.normalize_edges(cards, [edge])
        self.assertEqual("proposal", result[0]["status"])

    def test_self_relations_require_explicit_review(self):
        cards, edge = self.sample()
        edge["target"] = edge["source"]
        with self.assertRaisesRegex(ValueError, "Self-edge"):
            self.builder.normalize_edges(cards, [edge])
        edge["self_reviewed"] = True
        self.assertEqual(1, len(self.builder.normalize_edges(cards, [edge])))

    def test_missing_condition_or_evidence_is_rejected(self):
        cards, edge = self.sample()
        for field in ("condition", "evidence"):
            invalid = dict(edge, **{field: ""})
            with self.assertRaises(ValueError, msg=field):
                self.builder.normalize_edges(cards, [invalid])

    def test_manifest_detects_missing_new_card_and_duplicate_annotations(self):
        with self.assertRaisesRegex(ValueError, "NewCard"):
            self.builder.validate_coverage([{"id": "OldCard"}], {"OldCard": {}, "NewCard": {}})
        with self.assertRaisesRegex(ValueError, "duplicate"):
            self.builder.validate_coverage([{"id": "OldCard"}, {"id": "OldCard"}], {"OldCard": {}})

    def test_checked_in_graph_covers_release_special_cards_and_counts(self):
        graph = self.builder.build(write=False)
        nodes = {n["id"]: n for n in graph["nodes"]}
        self.assertEqual(72, sum(n["status"] == "implemented" for n in nodes.values()))
        self.assertEqual(63, sum(n.get("pool") == "main" for n in nodes.values()))
        self.assertEqual(58, sum(n.get("reward") is True for n in nodes.values()))
        self.assertEqual(26, sum(n.get("song") is True for n in nodes.values()))
        self.assertEqual("token", nodes["OverworkAnxiety"]["pool"])
        self.assertEqual("event", nodes["ShadowOfThePastIII"]["pool"])
        self.assertEqual("relic", nodes["PullmanCrash"]["pool"])
        self.assertFalse(nodes["Compose"]["song"])
        self.assertFalse(nodes["SakiMovePlz"]["song"])

    def test_x_cost_is_not_the_numeric_constructor_placeholder(self):
        nodes = {n["id"]: n for n in self.builder.build(write=False)["nodes"]}
        self.assertEqual("X", nodes["CrucifixX"]["cost"])
        self.assertEqual(0, nodes["Completeness"]["cost"])

    def test_uncommitted_source_cannot_claim_a_pinned_baseline(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            src = root / "mod/src"
            src.mkdir(parents=True)
            (src / "Card.cs").write_text("original")
            subprocess.run(["git", "-C", directory, "add", "."], check=True)
            subprocess.run(["git", "-C", directory, "-c", "user.name=Graph Test",
                            "-c", "user.email=graph@example.invalid", "commit", "-qm", "baseline"], check=True)
            self.builder.validate_clean_source(root, "mod")
            (src / "Card.cs").write_text("changed")
            with self.assertRaisesRegex(ValueError, "Uncommitted"):
                self.builder.validate_clean_source(root, "mod")

    def test_generated_csharp_does_not_change_source_fingerprints(self):
        fingerprint = getattr(self.builder, "tracked_source_hashes", None)
        self.assertTrue(callable(fingerprint), "Fingerprint only Git-tracked C# source")
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            mod = root / "mod"
            src = mod / "src"
            src.mkdir(parents=True)
            (src / "Card.cs").write_text("original")
            (root / ".gitignore").write_text("**/obj/\n")
            subprocess.run(["git", "-C", directory, "add", "."], check=True)
            before = fingerprint(mod)
            self.assertEqual({"src/Card.cs"}, set(before))
            (src / "obj").mkdir()
            (src / "obj/AssemblyInfo.cs").write_text("generated")
            self.assertEqual(before, fingerprint(mod))
            (src / "Card.cs").write_text("changed")
            self.assertNotEqual(before, fingerprint(mod))

    def test_graph_retains_real_global_conflicts_and_old_card_loops(self):
        graph = self.builder.build(write=False)
        edges = {(e["source"], e["target"], e["kind"]) for e in graph["edges"]}
        for expected in [("Unendurable", "Slander", "supply"),
                         ("Innocence", "Notebook", "supply"),
                         ("SheIsRadiant", "KillKiss", "tradeoff"),
                         ("Face", "Slander", "supply"),
                         ("LingeringResonance", "MusicOfTheCelestialSphere", "supply"),
                         ("ChoirSChoir", "BarkingBarkingBarking", "replay"),
                         ("Angles", "DawnOfDespair", "payoff"),
                         ("TwoMoonsDeepIntoTheForest", "TwoMoonsDeepIntoTheForest", "payoff"),
                         ("TreasurePleasure", "OctagramDance", "conflict")]:
            self.assertIn(expected, edges)
        self.assertNotIn(("UnspokenWords", "BackstageSupport", "payoff"), edges)


if __name__ == "__main__":
    unittest.main()
