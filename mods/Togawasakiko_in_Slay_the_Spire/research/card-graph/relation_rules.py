"""Conservative, explainable candidate edges. No text-to-mechanic inference."""
from copy import deepcopy
import json
from workbench_model import (validate_card, validate_fact, require, digest, card_universe, endpoint_hashes)


def load_profiles(path,baseline):
    data=json.loads(path.read_text())
    require(data.get('schema_version')==1 and data.get('source_revision')==baseline['meta']['source_revision'],'机制档案基线不匹配')
    require(set(data['cards'])=={n['id'] for n in baseline['nodes'] if n['status']=='implemented'},'机制档案必须覆盖全部已实现卡')
    for profile in data['cards'].values():
        require(isinstance(profile['limitations'],list) and bool(profile['evidence']),'档案缺少边界或证据')
        for fact in profile['facts']: validate_fact(fact)
        require(all(e in baseline['evidence'] for e in profile['evidence']),'未知档案证据')
    return {**data,'revision':digest(data)}


def declared_facts(card):
    facts=deepcopy(card.get('mechanics',[]))
    def add(action,resource,variant,condition):
        facts.append(dict(action=action,resource=resource,scope='self',timing='after_play',variant=variant,
                          amount='',limit=None,condition=condition,evidence=[]))
    for variant,key in [('base','keywords'),('upgraded','upgraded_keywords')]:
        keywords=card.get(key,[])
        if 'Unplayable' not in keywords:
            if card.get('song'): add('produce','song_played',variant,'同一持有者实际打出歌曲')
            if card.get('type')=='Attack':
                add('produce','attack_played',variant,'实际打出攻击牌')
                add('read','strength',variant,'仅正常攻击伤害；直接无属性伤害需人工排除')
            if 'Exhaust' in keywords: add('produce','card_exhausted',variant,'实际打出后消耗')
        if 'Ethereal' in keywords: add('produce','card_exhausted',variant,'回合结束留在手中因虚无消耗；并未打出')
    return facts


def suggest_relations(draft_id,card,baseline,workspace,profiles):
    validate_card(card)
    cards=card_universe(baseline,workspace,draft_id,card)
    hashes=endpoint_hashes(baseline,workspace,draft_id,card)
    facts={i:profiles['cards'][i]['facts'] if i in profiles['cards'] else declared_facts(c) for i,c in cards.items()}
    suggestions=[];seen=set()
    def emit(source,target,kind,rule,condition,reason,evidence=()):
        if source==target or draft_id not in (source,target): return
        signature=[source,target,rule,condition];identifier='suggested:'+digest(signature)[:24]
        if identifier in seen:return
        seen.add(identifier)
        refs=list(dict.fromkeys([e for e in evidence if e in baseline['evidence']]+[draft_id]))
        suggestions.append(dict(id=identifier,source=source,target=target,kind=kind,mechanism=rule,
            rule_id=rule,condition=condition,reason=reason,evidence=refs,status='proposal',suggested=True,
            endpoint_hashes={i:hashes[i] for i in (source,target)},profile_revision=profiles['revision'],self_reviewed=False))
    for other in cards:
        if other==draft_id:continue
        for source,target in [(draft_id,other),(other,draft_id)]:
            for a in facts[source]:
                for b in facts[target]:
                    scope_match=a['scope']==b['scope'] or a['scope']=='all_enemies' and b['scope']=='enemy'
                    if a['resource']!=b['resource'] or not scope_match:continue
                    supply=a['action']=='produce' and b['action'] in ('read','consume')
                    tradeoff=a['action']=='consume' and b['action']=='read' and a['resource']=='pressure'
                    if not(supply or tradeoff):continue
                    timing=f"来源 {a['variant']}/{a['timing']}：{a['condition']}；接收 {b['variant']}/{b['timing']}：{b['condition']}。"
                    if a['scope'] in ('enemy','all_enemies'):timing+='必须是同一受影响敌人；压力由全队共享。'
                    else:timing+='必须是同一持有者，不能把队友计数混在一起。'
                    if a['timing']=='enemy_hit' and b['timing']=='enemy_turn_start':timing+='受击后才产压，不能赶上当前敌方回合开始的检查。'
                    rule='song-count' if a['resource']=='song_played' and target=='TwoMoonsDeepIntoTheForest' else ('tradeoff:' if tradeoff else 'resource:')+a['resource']
                    emit(source,target,'tradeoff' if tradeoff else 'supply',rule,timing,
                         '显式资源或事件匹配；这只是候选关系，需确认数值、存活、牌序及触发额度。',a['evidence']+b['evidence'])
            candidate=cards[target]
            if candidate.get('song'):
                common=candidate.get('rarity') in ('Common','Uncommon')
                for source_id,kind,rule,condition in [
                    ('RehearsalOrder','sequence','song-tutor','歌曲原实例在抽牌堆，且品质为普通或非凡。'),
                    ('UnfinishedScore','sequence','song-topdeck','歌曲原实例在弃牌堆，置顶后仍需抽到；不保证Ave Mujica打出它。'),
                    ('Compose','generation','song-generation','纳入真实歌曲池后才是随机候选；本战0费，不代表升级。'),
                    ('TheWholeBlueWorld','generation','song-generation','纳入真实歌曲池后才是随机候选；基础-1费、升级免费，不代表升级实例。'),
                    ('InYourBlueEyes','replay','song-replay','可打出的歌曲原实例在消耗堆；升级另选弃牌堆；打出两次、重复支付代价，遵循原去向。')]:
                    if source!=source_id or source_id=='RehearsalOrder' and not common:continue
                    if source_id=='InYourBlueEyes' and (target=='ImprisonedXii' or 'Unplayable' in candidate.get('keywords',[]) and 'Unplayable' in candidate.get('upgraded_keywords',[])):continue
                    emit(source,target,kind,rule,condition,'候选资格不保证随机命中；提案仍未实现。',profiles['cards'][source]['evidence'])
            if candidate.get('type')=='Attack' and any(f['action']=='replay' and f['resource']=='attack_played' for f in facts[source]):
                emit(source,target,'replay','attack-replay','同一持有者、本回合攻击重放能力生效；原牌完整结算及代价重复。','重放实际攻击牌，不是增加攻击段数。')
            if any(f['action']=='sequence' and f['resource']=='card_type_order' for f in facts[source]) and any(f['action']=='replay' and f['resource']=='attack_played' for f in facts[target]):
                emit(source,target,'conflict','alternation-replay','交错序列和攻击重放同时生效；相邻两次攻击均算出牌。','重复同类型可能在第二次结算后强制结束自己的回合。')
    limitations=['建议不是战斗模拟或平衡评分；没有连边不代表没有配合。',
                 '自由文本和自定义词条不会自动解释；未编码的牌组、事件、遗物和原版卡效果需要人工连边。']
    if not card['mechanics']:limitations.append('当前草案没有显式机制，仅按类型、Song及已知词条分析。')
    if card['notes']:limitations.append('设计备注仅供人工阅读，不参与机制推断。')
    return {'suggestions':suggestions,'limitations':limitations,'endpoint_hashes':hashes,'profile_revision':profiles['revision']}
