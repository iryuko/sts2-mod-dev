import copy
import unittest

from workbench_model import (empty_card, empty_workspace, validate_card, mechanism_hash,
                             reduce_command, compose_graph, endpoint_hashes, ValidationError)

BASE = {'meta': {'source_revision': 'test'}, 'nodes': [
    {'id':'Real','name':'真实牌','cost':1,'type':'Skill','base':'获得3格挡','status':'implemented'}],
    'edges': [], 'evidence': {}, 'counts': {'implemented':1,'proposals':0}}
PROFILES = {'revision':'test-profiles'}


def card(**changes):
    return dict(empty_card(), **({'name':'测试','cost':1,'type':'Attack','rarity':'Common',
                                'source_pool':'main','base':'造成6伤害'} | changes))


class ModelTests(unittest.TestCase):
    def setUp(self):
        self.state = empty_workspace(BASE)

    def cmd(self, **command):
        self.state, result = reduce_command(self.state, command, BASE, PROFILES)
        return result

    def test_snapshot_withdraw_archive_and_duplicate_names(self):
        first = self.cmd(action='create', card=card())['id']
        second = self.cmd(action='create', card=card())['id']
        self.assertNotEqual(first, second)
        self.cmd(action='publish', id=first)
        self.cmd(action='update', id=first, card=card(base='造成9伤害'))
        self.assertEqual(self.state['entries'][first]['published']['card']['base'], '造成6伤害')
        before = copy.deepcopy(BASE)
        graph = compose_graph(BASE, self.state, PROFILES)
        self.assertEqual(graph['nodes'][-1]['base'], '造成6伤害')
        self.assertEqual(BASE, before)
        self.cmd(action='archive', id=first)
        self.cmd(action='restore', id=first)
        self.assertIsNone(self.state['entries'][first]['published'])
        self.assertEqual(self.state['entries'][first]['working']['base'], '造成9伤害')

    def test_validation_and_independent_upgrade_keywords(self):
        validate_card(empty_card())
        validate_card(card(cost='X', keywords=['Exhaust'], upgraded_keywords=[]), publishing=True)
        for invalid in [card(cost=True), card(cost=-1), card(cost=100), card(cost=float('nan')),
                        card(type='magic'), card(unknown=2), card(mechanics=[{}])]:
            with self.subTest(invalid=invalid), self.assertRaises(ValidationError): validate_card(invalid)
        with self.assertRaises(ValidationError): validate_card(empty_card(), publishing=True)
        with self.assertRaises(ValidationError): self.cmd(action='update', id='Real', card=card())
        with self.assertRaises(ValidationError): self.cmd(action='create', card=card(image_id='missing'))

    def test_unknown_explicit_card_reference_can_save_but_not_publish(self):
        identifier=self.cmd(action='create',card=card(base='参照 [[card:missing]]'))['id']
        with self.assertRaises(ValidationError): self.cmd(action='publish',id=identifier)
        self.cmd(action='update',id=identifier,card=card(base='参照 [[card:Real]]'))
        self.cmd(action='publish',id=identifier)

    def test_hash_ignores_name_image_whitespace_not_effects(self):
        self.assertEqual(mechanism_hash(card()), mechanism_hash(card(name='新名', image_id='art', base='  造成6伤害 \n')))
        for changes in [{'cost':2}, {'base':'造成7伤害'}, {'upgraded_keywords':['Retain']}, {'song':True}]:
            self.assertNotEqual(mechanism_hash(card()), mechanism_hash(card(**changes)))

    def test_cross_proposal_reviews_become_stale_not_deleted(self):
        a = self.cmd(action='create', card=card())['id']
        b = self.cmd(action='create', card=card())['id']
        self.cmd(action='publish', id=b)
        relation = {'source':a,'target':b,'kind':'supply','mechanism':'配合',
                    'condition':'同一持有者','reason':'人工判断','evidence':[], 'self_reviewed':False}
        hashes = endpoint_hashes(BASE, self.state, a, self.state['entries'][a]['working'])
        expected = {k:hashes[k] for k in [a,b]}
        self.cmd(action='review', id=a, relation=relation, decision='accepted', expected_endpoint_hashes=expected)
        self.cmd(action='publish', id=a)
        self.assertEqual(len(compose_graph(BASE,self.state,PROFILES)['edges']),1)
        self.assertEqual(compose_graph(BASE,self.state,PROFILES)['counts']['edges'],1)
        self.cmd(action='update', id=b, card=card(base='变了'))
        self.assertEqual(len(compose_graph(BASE,self.state,PROFILES)['edges']),1)
        self.cmd(action='publish', id=b)
        self.assertEqual(len(compose_graph(BASE,self.state,PROFILES)['edges']),0)
        self.assertEqual(len(self.state['entries'][a]['reviews']),1)
        with self.assertRaises(ValidationError): self.cmd(action='publish',id=a)

    def test_review_rejects_hidden_drafts_self_without_review_and_bad_hash(self):
        a=self.cmd(action='create',card=card())['id']
        b=self.cmd(action='create',card=card())['id']
        for target in [b,a,'missing']:
            with self.assertRaises(ValidationError):
                self.cmd(action='review',id=a,relation={'source':a,'target':target,'kind':'supply',
                    'mechanism':'m','condition':'c','reason':'r'},decision='accepted',expected_endpoint_hashes={})

    def test_missing_endpoint_review_can_be_rejected_and_owner_republished(self):
        a=self.cmd(action='create',card=card())['id'];b=self.cmd(action='create',card=card())['id']
        self.cmd(action='publish',id=b)
        hashes=endpoint_hashes(BASE,self.state,a,card())
        relation={'source':a,'target':b,'kind':'supply','mechanism':'m','condition':'c','reason':'r','evidence':[]}
        self.cmd(action='review',id=a,relation=relation,decision='accepted',expected_endpoint_hashes={k:hashes[k] for k in [a,b]})
        self.cmd(action='publish',id=a);self.cmd(action='archive',id=b)
        review=next(iter(self.state['entries'][a]['reviews'].values()))
        self.cmd(action='review',id=a,relation=review,decision='rejected',expected_endpoint_hashes={a:hashes[a]})
        self.cmd(action='publish',id=a)
        self.assertEqual(compose_graph(BASE,self.state,PROFILES)['edges'],[])


if __name__ == '__main__': unittest.main()
