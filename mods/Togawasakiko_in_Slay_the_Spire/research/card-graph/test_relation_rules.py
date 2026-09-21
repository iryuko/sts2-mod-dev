import json
from pathlib import Path
import unittest
from test_workbench_model import card
from workbench_model import empty_workspace
from relation_rules import load_profiles, suggest_relations

ROOT=Path(__file__).parent


def fact(action,resource,scope='self',timing='on_play',condition='同一持有者实际结算'):
    return dict(action=action,resource=resource,scope=scope,timing=timing,variant='both',amount='',limit=None,condition=condition,evidence=[])


class RulesTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.graph=json.loads((ROOT/'graph.json').read_text());cls.profiles=load_profiles(ROOT/'mechanic-profiles.json',cls.graph)
    def analyze(self, **changes):
        return suggest_relations('draft:test',card(**changes),self.graph,empty_workspace(self.graph),self.profiles)
    def test_full_baseline_has_evidenced_profiles(self):
        self.assertEqual(len(self.profiles['cards']),72)
        self.assertTrue(all(p['evidence'] for p in self.profiles['cards'].values()))
    def test_no_tag_guessing_or_self_edges(self):
        result=self.analyze(type='Skill',base='压力很大',notes='Song')
        self.assertEqual(result['suggestions'],[])
        self.assertTrue(result['limitations'])
    def test_song_count_respects_unplayable_and_tutor_rarity(self):
        self.assertFalse(any(e['rule_id']=='song-count' for e in self.analyze()['suggestions']))
        result=self.analyze(song=True,rarity='Rare')
        self.assertTrue(any(e['target']=='TwoMoonsDeepIntoTheForest' for e in result['suggestions']))
        self.assertFalse(any(e['source']=='RehearsalOrder' for e in result['suggestions']))
        result=self.analyze(song=True,keywords=['Unplayable'],upgraded_keywords=['Unplayable'])
        self.assertFalse(any(e['rule_id']=='song-count' for e in result['suggestions']))
    def test_discard_exhaust_play_are_distinct(self):
        result=self.analyze(mechanics=[fact('read','card_discarded')])
        self.assertTrue(any(e['source']=='Completeness' for e in result['suggestions']))
        result=self.analyze(mechanics=[fact('produce','card_exhausted')])
        self.assertTrue(any(e['target']=='LingeringResonance' for e in result['suggestions']))
        self.assertFalse(any(e['target']=='BackstageSupport' for e in result['suggestions']))
    def test_generated_exhaustion_is_conditional_and_face_timing_visible(self):
        result=self.analyze(mechanics=[fact('read','card_exhausted')])
        edge=next(e for e in result['suggestions'] if e['source']=='Housewarming')
        self.assertIn('生成实例',edge['condition'])
        result=self.analyze(mechanics=[fact('read','pressure','enemy','enemy_turn_start')])
        edge=next(e for e in result['suggestions'] if e['source']=='Face')
        self.assertIn('不能赶上',edge['condition'])
    def test_exhaust_upgrade_removal_separate_variants(self):
        result=self.analyze(keywords=['Exhaust'],upgraded_keywords=[])
        edge=next(e for e in result['suggestions'] if e['target']=='LingeringResonance')
        self.assertIn('base',edge['condition']);self.assertNotIn('upgraded',edge['condition'])


if __name__=='__main__': unittest.main()
