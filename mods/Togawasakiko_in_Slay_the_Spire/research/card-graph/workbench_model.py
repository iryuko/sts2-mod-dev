"""Pure draft validation and research publication; never modifies game data."""
from copy import deepcopy
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
import re
import uuid

KINDS = ('supply', 'payoff', 'sequence', 'replay', 'generation', 'tradeoff', 'conflict')
RESOURCES = ('pressure', 'block', 'hp_loss', 'heal', 'strength', 'dexterity', 'inferiority',
             'despair_echo', 'damage_received', 'pressure_token', 'card_discarded', 'card_exhausted',
             'token_played', 'song_played', 'attack_played', 'card_type_order', 'song_pool_access',
             'exhaust_pile_access', 'discard_pile_access', 'draw_pile_access')
TIMINGS = ('on_play', 'after_play', 'after_draw', 'enemy_hit', 'own_turn_start', 'own_turn_end',
           'enemy_turn_start', 'continuous')


class ValidationError(ValueError):
    def __init__(self, message, code='validation', fields=None):
        super().__init__(message); self.code = code; self.fields = fields or []


class RevisionConflict(ValidationError): pass
class CorruptWorkspace(ValidationError): pass
class WorkspaceLocked(ValidationError): pass


def require(condition, message, field=''):
    if not condition: raise ValidationError(message, fields=[field] if field else [])


def now(): return datetime.now(timezone.utc).isoformat()


def digest(value):
    return hashlib.sha256(json.dumps(value, ensure_ascii=False, sort_keys=True, allow_nan=False,
                                     separators=(',', ':')).encode()).hexdigest()


def empty_card():
    return dict(name='', cost=None, upgraded_cost=None, type=None, rarity=None, source_pool=None,
                song=False, keywords=[], upgraded_keywords=[], base='', upgrade='',
                upgrade_mode='unchanged', notes='', image_id=None, portrait_source_id=None, mechanics=[])


def text(value, maximum, field, required=False):
    require(isinstance(value, str) and len(value) <= maximum, f'{field} 文本无效或过长', field)
    require(not required or bool(value.strip()), f'{field} 不能为空', field)


def validate_fact(fact):
    keys={'action','resource','scope','timing','variant','amount','limit','condition','evidence'}
    require(isinstance(fact,dict) and set(fact)==keys, '机制字段不完整')
    for key, allowed in [('action',('produce','consume','read','trigger','generate','replay','sequence')),
                         ('resource',RESOURCES),('scope',('self','enemy','all_enemies','ally')),
                         ('timing',TIMINGS),('variant',('base','upgraded','both'))]:
        require(fact[key] in allowed, f'未知机制 {key}')
    text(fact['amount'],200,'amount');text(fact['condition'],2000,'condition',True)
    require(fact['limit'] is None or type(fact['limit']) is int and 0<=fact['limit']<=999, '次数无效')
    require(isinstance(fact['evidence'],list) and len(fact['evidence'])<=32, '证据无效')
    for item in fact['evidence']: text(item,500,'evidence')
    return fact


def validate_card(card, *, publishing=False):
    require(isinstance(card,dict) and set(card)==set(empty_card()), '卡牌字段不完整或含未知字段')
    for key, size in [('name',200),('base',8000),('upgrade',8000),('notes',12000)]:
        text(card[key],size,key,publishing and key in ('name','base'))
    for key in ('cost','upgraded_cost'):
        v=card[key]
        require(v is None or v=='X' or type(v) is int and 0<=v<=99,'费用应为0至99或X',key)
    require(not publishing or card['cost'] is not None, '基础费用未填写','cost')
    for key, values in [('type',('Attack','Skill','Power','Curse')),
                        ('rarity',('Basic','Common','Uncommon','Rare','Ancient','Token','Event')),
                        ('source_pool',('main','token','event','relic','special'))]:
        require(card[key] in values or card[key] is None and not publishing, f'{key} 未选择或无效',key)
    require(type(card['song']) is bool,'Song应为布尔值')
    require(card['upgrade_mode'] in ('unchanged','delta','full'),'升级模式无效')
    for key in ('keywords','upgraded_keywords'):
        require(isinstance(card[key],list) and len(card[key])<=32,'词条列表无效',key)
        for item in card[key]: text(item,64,key,True)
    for key in ('image_id','portrait_source_id'):
        require(card[key] is None or isinstance(card[key],str) and 0<len(card[key])<=200,'图片引用无效',key)
    require(isinstance(card['mechanics'],list) and len(card['mechanics'])<=100,'机制列表无效')
    for fact in card['mechanics']: validate_fact(fact)
    return deepcopy(card)


def mechanism_hash(card):
    def normalize(value):
        if isinstance(value,str): return ' '.join(value.split())
        if isinstance(value,list): return [normalize(v) for v in value]
        if isinstance(value,dict): return {k:normalize(v) for k,v in value.items()}
        return value
    return digest(normalize({k:card.get(k) for k in empty_card() if k not in ('name','image_id','portrait_source_id')}))


def empty_workspace(baseline):
    return {'schema_version':1,'revision':0,'baseline_revision':baseline['meta']['source_revision'],
            'entries':{},'images':{}}


def card_universe(baseline, state, draft_id=None, card=None):
    result={n['id']:n for n in baseline['nodes']}
    result.update({i:e['published']['card'] for i,e in state['entries'].items() if e['published'] and not e['archived']})
    if draft_id and card is not None: result[draft_id]=card
    return result


def endpoint_hashes(baseline, state, draft_id=None, card=None):
    return {i:mechanism_hash(c) for i,c in card_universe(baseline,state,draft_id,card).items()}


def review_current(review, hashes, profiles):
    return (review.get('profile_revision')==profiles.get('revision','none')
            and all(hashes.get(i)==review.get('endpoint_hashes',{}).get(i)
                    and (i in hashes or review.get('decision')=='rejected' and i in review.get('endpoint_hashes',{}))
                    for i in (review['source'],review['target'])))


def preview_relations(analysis, entry, profiles):
    reviews=entry['reviews'];hashes=analysis['endpoint_hashes']
    active={i:r for i,r in reviews.items() if review_current(r,hashes,profiles)}
    suggested=[deepcopy(r) for r in analysis['suggestions'] if r['id'] not in active]
    accepted=[{**deepcopy(r),'status':'proposal'} for r in active.values() if r['decision']=='accepted']
    return suggested+accepted


def validate_references(card, state, baseline):
    require(card['image_id'] is None or card['image_id'] in state['images'],'找不到上传图片')
    ids={n['id'] for n in baseline['nodes']}
    require(card['portrait_source_id'] is None or card['portrait_source_id'] in ids,'找不到原卡卡图')


def reduce_command(state, command, baseline, profiles=None):
    profiles=profiles or {}; result={}; current=deepcopy(state)
    require(isinstance(command,dict),'命令必须为对象')
    action=command.get('action')
    allowed={'create':{'card','origin_id'},'update':{'id','card'},'review':{'id','relation','decision','expected_endpoint_hashes'},
             'publish':{'id'},'withdraw':{'id'},'archive':{'id'},'restore':{'id'}}
    require(isinstance(action,str) and action in allowed,'未知命令')
    require(not(set(command)-allowed[action]-{'action'}),'命令含未知字段')
    if action in ('create','update'):
        card=validate_card(command.get('card'));validate_references(card,current,baseline)
    if action=='create':
        require(len(current['entries'])<500,'草稿已达500张上限')
        origin=command.get('origin_id')
        require(origin is None or origin in card_universe(baseline,current) or origin in current['entries'],'原卡不存在')
        identifier=f'draft:{uuid.uuid4()}'
        current['entries'][identifier]={'id':identifier,'origin_id':origin,'archived':False,'working':card,
                                       'reviews':{},'published':None,'created_at':now(),'updated_at':now()}
        result['id']=identifier
    else:
        identifier=command.get('id');require(isinstance(identifier,str) and identifier in current['entries'],'找不到草稿')
        entry=current['entries'][identifier]
        require(not entry['archived'] or action=='restore','草稿已归档')
        if action=='update': entry['working']=card
        elif action=='review':
            relation=command.get('relation');require(isinstance(relation,dict),'关系必须为对象')
            keys={'id','source','target','kind','mechanism','condition','reason','evidence','rule_id','self_reviewed',
                  'endpoint_hashes','profile_revision','decision'}
            require(not(set(relation)-keys),'关系含未知字段')
            source,target=relation.get('source'),relation.get('target')
            hashes=endpoint_hashes(baseline,current,identifier,entry['working'])
            prior=entry['reviews'].get(relation.get('id'))
            retiring=(command.get('decision')=='rejected' and prior
                      and (source,target)==(prior['source'],prior['target']))
            require(isinstance(source,str) and isinstance(target,str) and (source in hashes and target in hashes or retiring)
                    and identifier in (source,target),'关系端点必须包含当前草稿及已纳入卡牌')
            require(source!=target or relation.get('self_reviewed') is True,'自环需要明确审核')
            require(relation.get('kind') in KINDS,'未知关系类型')
            for key in ('mechanism','condition','reason'): text(relation.get(key),4000,key,True)
            expected={i:hashes[i] for i in (source,target) if i in hashes}
            if command.get('expected_endpoint_hashes')!=expected:
                raise RevisionConflict('卡牌机制已变化，请重新分析','endpoint_conflict')
            require(command.get('decision') in ('accepted','rejected'),'审核状态无效')
            evidence=relation.get('evidence',[])
            require(isinstance(evidence,list) and len(evidence)<=32,'证据列表无效')
            for ref in evidence: require(isinstance(ref,str) and (ref in baseline.get('evidence',{}) or ref in hashes or retiring and ref in current['entries']),'未知证据引用')
            rid=relation.get('id') or f'manual:{uuid.uuid4()}'
            require(isinstance(rid,str) and len(rid)<=200,'关系ID无效')
            require(sum(len(e['reviews']) for e in current['entries'].values())<10000 or rid in entry['reviews'],'关系数量达到上限')
            entry['reviews'][rid]={**deepcopy(relation),'id':rid,'decision':command['decision'],
                                   'endpoint_hashes':{i:hashes.get(i) for i in (source,target)},'profile_revision':profiles.get('revision','none'),
                                   'evidence':evidence,'self_reviewed':relation.get('self_reviewed') is True}
        elif action=='publish':
            validate_card(entry['working'],publishing=True)
            known={n['id'] for n in baseline['nodes']}|set(current['entries'])
            for key in ('base','upgrade','notes'):
                refs=re.findall(r'\[\[card:([^\]\r\n]+)\]\]',entry['working'][key])
                require(all(ref in known for ref in refs),'卡文包含未知卡牌引用',key)
            hashes=endpoint_hashes(baseline,current,identifier,entry['working'])
            accepted=[r for r in entry['reviews'].values() if r['decision']=='accepted']
            require(all(review_current(r,hashes,profiles) for r in accepted),'已确认关系需要重新审核','reviews')
            entry['published']={'card':deepcopy(entry['working']),'relations':deepcopy(accepted),
                                'mechanism_hash':mechanism_hash(entry['working']),'published_at':now()}
        elif action in ('withdraw','archive'):
            entry['published']=None
            if action=='archive': entry['archived']=True
        elif action=='restore': entry['archived']=False
        entry['updated_at']=now()
    current['revision']+=1
    return current,result


def as_node(identifier, card):
    portrait=f'/api/images/{card["image_id"]}' if card.get('image_id') else (
        f'/api/portraits/{card["portrait_source_id"]}' if card.get('portrait_source_id') else None)
    return {**deepcopy(card),'id':identifier,'name':card.get('name') or '未命名草案','status':'proposal',
            'pool':'proposal','portrait':portrait,'notes':[card['notes']] if card.get('notes') else [],
            'upgrade':card.get('upgrade') or '不变'}


def compose_graph(baseline, state, profiles=None, preview=None):
    profiles=profiles or {}; graph=deepcopy(baseline)
    hashes=endpoint_hashes(baseline,state)
    for identifier,entry in state['entries'].items():
        if entry['archived'] or not entry['published']: continue
        graph['nodes'].append(as_node(identifier,entry['published']['card']))
        for review in entry['published']['relations']:
            if review_current(review,hashes,profiles): graph['edges'].append({**deepcopy(review),'status':'proposal'})
    if preview:
        identifier,card=preview['draft_id'],preview['card']
        graph['nodes']=[n for n in graph['nodes'] if n['id']!=identifier]+[as_node(identifier,card)]
        graph['edges']=[e for e in graph['edges'] if identifier not in (e['source'],e['target'])]
        graph['edges']+=deepcopy(preview.get('relations',[]))
    graph['counts']={**graph.get('counts',{}),'implemented':sum(n.get('status')=='implemented' for n in graph['nodes']),
                     'proposals':sum(n.get('status')=='proposal' for n in graph['nodes']),
                     'edges':len(graph['edges']),'kinds':dict(Counter(e['kind'] for e in graph['edges']))}
    return graph


def validate_workspace(state, baseline):
    try:
        require(isinstance(state,dict) and set(state)==set(empty_workspace(baseline)),'工作区结构无效')
        require(type(state['schema_version']) is int and state['schema_version']==1,'工作区版本不支持')
        require(type(state['revision']) is int and state['revision']>=0,'revision无效')
        require(state['baseline_revision']==baseline['meta']['source_revision'],'基线版本不匹配')
        require(isinstance(state['entries'],dict) and len(state['entries'])<=500,'草稿列表无效')
        require(isinstance(state['images'],dict),'图片列表无效')
        for identifier,entry in state['entries'].items():
            require(re.fullmatch(r'draft:[0-9a-f-]{36}',identifier) and entry['id']==identifier,'草稿ID无效')
            require(type(entry['archived']) is bool and isinstance(entry['reviews'],dict),'草稿状态无效')
            validate_card(entry['working']);validate_references(entry['working'],state,baseline)
            if entry['published']:
                validate_card(entry['published']['card'],publishing=True)
                require(isinstance(entry['published']['relations'],list),'发布关系无效')
            for review in list(entry['reviews'].values())+(entry['published']['relations'] if entry['published'] else []):
                require(all(k in review for k in ('source','target','id','endpoint_hashes','profile_revision','decision')),'审核记录无效')
    except (ValidationError,KeyError,TypeError,AttributeError) as error:
        raise CorruptWorkspace('工作区损坏或版本不兼容，请从备份恢复','corrupt_workspace') from error
    return state
