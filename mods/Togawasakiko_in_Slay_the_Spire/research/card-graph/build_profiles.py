"""Rebuild reviewed facts, not a natural-language mechanic parser.

Assignments below use the pinned catalogs/source and keep their conditional notes.
Unrepresented effects remain explicit limitations, never inferred from free text.
"""
import json
from pathlib import Path

ROOT=Path(__file__).parent


def build():
    graph=json.loads((ROOT/'graph.json').read_text())
    nodes={n['id']:n for n in graph['nodes'] if n['status']=='implemented'}
    profiles={i:{'facts':[], 'limitations':['档案仅覆盖显式列出的机制，不是完整战斗模拟。']+n.get('notes',[]),
                 'evidence':[n['source']+'#'+i]} for i,n in nodes.items()}

    def add(ids,action,resource,scope='self',timing='on_play',condition='无额外条件',variant='both',limit=None,amount=''):
        for identifier in ids.split():
            node=nodes[identifier]
            profiles[identifier]['facts'].append(dict(action=action,resource=resource,scope=scope,timing=timing,
                variant=variant,amount=amount,limit=limit,condition=condition+'；原效果：'+node['base']+' 升级：'+node['upgrade'],
                evidence=profiles[identifier]['evidence']))

    add('DefendTogawasakiko Unendurable Fragility Face Sophie SymbolIii UnfinishedScore FollowingPhrase ComposedResponse','produce','block')
    add('Unendurable Ether GeorgetteMeGeorgetteYou Notebook MasqueradeRhapsodyRequest Unmask','produce','pressure','enemy',condition='施压分支成立、目标存活；共享敌方压力未被花掉')
    add('Completeness AnswerMe','produce','pressure','all_enemies',condition='施压分支实际成立，非任意分支都产压')
    add('Slander Curseslander BlackBirthday PullmanCrash','read','pressure','enemy',condition='读取同一目标剩余压力；阈值见原效果')
    add('CrucifixX MusicOfTheCelestialSphere','read','pressure','all_enemies',condition='逐敌检查；十字架要求所有存活检查对象都达门槛')
    add('KillKiss','read','pressure','enemy','enemy_turn_start','敌方回合开始前已生效，且压力严格大于目标当前HP一半')
    add('SheIsRadiant','consume','pressure','all_enemies')
    add('ComposedResponse LeaveItToMe','consume','pressure','enemy')
    add('PutOnYourMask SakiMovePlz BladeThroughTheHeart BailMoney MusicOfTheCelestialSphere SymbolIi Sophie Angles STheWay AnswerMe',
        'consume','pressure','enemy',condition='Debuff确实增加且压力足够时watcher才兑换；多人共享压力、唯一收益归属')
    add('PutOnYourMask Fragility Face','produce','pressure','enemy','enemy_hit','先有颜，再受到怪物来源正TotalDamage；下次自身回合开始移除')
    add('DawnOfDespair','produce','despair_echo','enemy',condition='在本卡攻击完成之后才施加；玩家侧回合末失效')
    add('DawnOfDespair','read','damage_received','enemy','after_play','先施加回响，再有后续正TotalDamage；本卡初始攻击不享受刚施加回响')
    add('Angles Sophie SymbolIi','produce','inferiority','enemy',condition='自卑分支实际施加，后续每个正TotalDamage降低1力量')
    add('Angles Sophie SymbolIi','read','damage_received','enemy','after_play','先成功施加自卑，再有正TotalDamage；包括被完全格挡的正总伤害')
    add('SheIsRadiant WeightliftingChampion','produce','strength')
    add('WeightliftingChampion','produce','dexterity')
    add('LeaveItToMe Fragility','produce','heal',condition='自身实际回复生命')
    add('BarkingBarkingBarking','produce','heal',timing='own_turn_end',condition='获得回复能力后，按原版回复时点治疗；不是立即治疗')
    add('WeightliftingChampion GodYouFool STheWay TheWholeBlueWorld','produce','hp_loss',condition='实际扣除生命、玩家存活；可能受损失上限影响')
    add('MasqueradeRhapsodyRequest','read','hp_loss',condition='后续施放时缺失生命仍存在，期间未治回')
    add('GeorgetteMeGeorgetteYou','read','heal',condition='治疗后当前HP满足与目标HP的比较，才进入施压分支')
    add('Completeness Thrilled SoManyMaggots ChoirSChoir','produce','card_discarded',condition='自己的牌确实从手中弃置；无可选牌不产生事件')
    add('Speak RestorationOfPower Curseslander','produce','pressure_token',condition='生成手牌成功；随机生成不保证具体卡名')
    add('AveMujica','produce','pressure_token',timing='after_draw',condition='自身回合常规抽牌完成后触发能力')
    add('UnspokenWords Divine','consume','pressure_token',condition='选择自己手中GeneratedPressureCard并确实消耗，不打出其效果')
    add('UnspokenWords Divine','produce','card_exhausted',condition='手中压力衍生牌实际被主动消耗；不同于打出')
    add('Housewarming','produce','card_exhausted',timing='after_play',condition='只在生成实例大狗后续打出消耗或回合末虚无消耗时；乔迁本体不消耗')
    add('LingeringResonance','read','card_exhausted',timing='after_play',condition='自己的本回合前2次实际消耗，能力先已生效；虚无计入',limit=2)
    add('LingeringResonance','produce','pressure','all_enemies','after_play','能力先已生效，自己的前2次消耗触发',limit=2)
    add('BackstageSupport','read','token_played',timing='after_play',condition='自己的前2次GeneratedPressureCard实际打出；主动消耗不算打出',limit=2)
    add('BackstageSupport','produce','block',timing='after_play',condition='能力已生效、自己的前2次压力衍生牌打出；Unpowered格挡不吃敏捷',limit=2)
    add('TwoMoonsDeepIntoTheForest','read','song_played',timing='after_play',condition='只统计同一持有者本场战斗的实际歌曲出牌')
    add('FollowingPhrase SakiMovePlz','read','song_played',condition='同一持有者上一张歌曲；小节按开始历史，祥移动按完成历史，嵌套时不同')
    add('TreasurePleasure','replay','attack_played',timing='after_play',condition='本回合、自己攻击牌完整结算后；再次支付效果代价')
    add('OctagramDance','sequence','card_type_order',condition='本回合连续同类型时在第二张结算后结束自己的回合，队友不结束')
    add('InYourBlueEyes ChoirSChoir','replay','exhaust_pile_access',condition='合法实例在消耗堆快照、排除当前打出的实例；碧眼只选Song且打出两次')
    add('InYourBlueEyes','replay','discard_pile_access',variant='upgraded',condition='升级碧眼另从弃牌堆快照选择Song，遵循原牌结算去向')
    add('UnfinishedScore','read','discard_pile_access',condition='歌曲实际在弃牌堆，置顶不等于直接入手或打出')
    add('SeverThePast','produce','draw_pile_access',condition='弃牌非空才合并洗牌；会破坏既有置顶顺序')
    add('RehearsalOrder','read','draw_pile_access',condition='抽牌堆现有普通或非凡Song，排除稀有，不生成')
    add('Compose TheWholeBlueWorld','generate','song_pool_access',condition='仅歌曲池随机候选，不保证抽中；非永久入组')
    exhaust='IHaveAscended Speak RestorationOfPower WeightliftingChampion PullmanCrash Fragility PersonaDissociation SocialWithdrawal AllYouThinkAboutIsYourself OverworkAnxiety Unmask RehearsalOrder BlackBirthday Divine OctagramDance'
    add(exhaust,'produce','card_exhausted',timing='after_play',condition='自己实际打出并按原牌消耗；虚无牌还可未打出而消耗')
    add('MasqueradeRhapsodyRequest TreasurePleasure','produce','card_exhausted',timing='after_play',variant='base',condition='只有未升级版带消耗；升级后移除')
    for identifier,node in nodes.items():
        if node['type']=='Attack':
            add(identifier,'read','strength',condition='正常攻击伤害走力量修正；多段逐段计算，目标需存活')
            add(identifier,'produce','attack_played',timing='after_play',condition='实际打出而非单次攻击段数')
            add(identifier,'produce','damage_received','enemy',condition='敌人确实受到正TotalDamage，独立命中逐次结算')
        if node['song'] and identifier!='ImprisonedXii':
            add(identifier,'produce','song_played',timing='after_play',condition='同一持有者实际打出；不是生成/抽取/消耗')
        if node['pool']=='token': add(identifier,'produce','token_played',timing='after_play',condition='压力衍生牌实际打出，不只是主动消耗')
        if not profiles[identifier]['facts']:
            profiles[identifier]['limitations'].append('此牌特殊效果暂未编码到有限规则，请保留人工连边。')
    return {'schema_version':1,'source_revision':graph['meta']['source_revision'],'cards':profiles}


if __name__=='__main__':
    (ROOT/'mechanic-profiles.json').write_text(json.dumps(build(),ensure_ascii=False,indent=2)+'\n')
