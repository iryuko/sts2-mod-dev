using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal static class ModSupport
{
    private static readonly HashSet<string> StarterDeckEntries = new(StringComparer.Ordinal)
    {
        "STRIKE_TOGAWASAKIKO",
        "DEFEND_TOGAWASAKIKO",
        "SLANDER",
        "UNENDURABLE"
    };

    private static readonly HashSet<string> SongCardEntries = new(StringComparer.Ordinal)
    {
        "AVE_MUJICA",
        "A_WONDERFUL_WORLD_YET_NOWHERE_TO_BE_FOUND",
        "ANGLES",
        "ETHER",
        "GEORGETTE_ME_GEORGETTE_YOU",
        "SYMBOL_I",
        "SYMBOL_II",
        "SYMBOL_III",
        "SYMBOL_IV",
        "CRUCIFIX_X",
        "FACE",
        "MUSIC_OF_THE_CELESTIAL_SPHERE",
        "KILL_KISS",
        "BLACK_BIRTHDAY",
        "TREASURE_PLEASURE",
        "CHOIR_S_CHOIR",
        "IMPRISONED_XII",
        "GOD_YOU_FOOL",
        "MASQUERADE_RHAPSODY_REQUEST",
        "S_THE_WAY",
        "TWO_MOONS_DEEP_INTO_THE_FOREST",
        "SOPHIE"
    };

    private static AudioStream? _shadowEventMusicStream;
    private static AudioStreamPlayer? _shadowEventMusicPlayer;
    private static readonly FieldInfo? CardEnergyCostLocalModifiersField =
        AccessTools.Field(typeof(CardEnergyCost), "_localModifiers");
    private static readonly AccessTools.FieldRef<RunState, HashSet<ModelId>?> VisitedEventIdsRef =
        AccessTools.FieldRefAccess<RunState, HashSet<ModelId>?>("_visitedEventIds");

    public static bool IsBaseGameCharacter(CharacterModel? character)
    {
        return character is Ironclad or Silent or Regent or Defect or Necrobinder or Deprived;
    }

    public static void LogInfo(string message)
    {
        string line = "[Togawasakiko] " + message;
        Console.WriteLine(line);
        Log.Info(line);
    }

    public static void LogWarn(string message)
    {
        string line = "[Togawasakiko] " + message;
        Console.WriteLine(line);
        Log.Warn(line);
    }

    public static void LogError(string message)
    {
        string line = "[Togawasakiko] " + message;
        Console.WriteLine(line);
        Log.Error(line);
    }

    public static HoverTip CreatePowerHoverTip<TPower>() where TPower : PowerModel
    {
        TPower power = ModelDb.Power<TPower>();
        return new HoverTip(power.Title, power.Description, power.Icon);
    }

    public static IHoverTip CreateCardHoverTip<TCard>() where TCard : CardModel
    {
        return HoverTipFactory.FromCard(ModelDb.Card<TCard>(), false);
    }

    public static bool CardMentionsPressure(CardModel card)
    {
        string descriptionKey = card.Id.Entry + ".description";
        return CardLocEntries.Values.Any(languageEntries =>
            languageEntries.TryGetValue(descriptionKey, out string? description)
            && (description.Contains("Pressure", StringComparison.OrdinalIgnoreCase)
                || description.Contains("压力", StringComparison.Ordinal)));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> CharacterLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["TOGAWASAKIKO.title"] = "Togawa Sakiko",
                ["TOGAWASAKIKO.titleObject"] = "Togawa Sakiko",
                ["TOGAWASAKIKO.description"] = "A control-oriented character who builds Pressure and converts it into temporary tools.",
                ["TOGAWASAKIKO.pronounSubject"] = "she",
                ["TOGAWASAKIKO.pronounObject"] = "her",
                ["TOGAWASAKIKO.pronounPossessive"] = "hers",
                ["TOGAWASAKIKO.possessiveAdjective"] = "her",
                ["TOGAWASAKIKO.unlockText"] = "This character is currently injected by the mod at runtime.",
                ["TOGAWASAKIKO.cardsModifierTitle"] = "Togawa Sakiko Cards",
                ["TOGAWASAKIKO.cardsModifierDescription"] = "Uses Togawa Sakiko's starter deck and pressure cards.",
                ["TOGAWASAKIKO.eventDeathPreventionLine"] = "Not yet."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["TOGAWASAKIKO.title"] = "丰川祥子",
                ["TOGAWASAKIKO.titleObject"] = "丰川祥子",
                ["TOGAWASAKIKO.description"] = "丰川财团的继承人，世界的神明\n将本次Ave Mujica的演出舞台选在一座高塔，她会带来震撼的表演",
                ["TOGAWASAKIKO.pronounSubject"] = "她",
                ["TOGAWASAKIKO.pronounObject"] = "她",
                ["TOGAWASAKIKO.pronounPossessive"] = "她的",
                ["TOGAWASAKIKO.possessiveAdjective"] = "她的",
                ["TOGAWASAKIKO.unlockText"] = "该角色当前通过 mod 在运行时注入到选人界面。",
                ["TOGAWASAKIKO.cardsModifierTitle"] = "丰川祥子卡池",
                ["TOGAWASAKIKO.cardsModifierDescription"] = "使用丰川祥子的起始牌组与压力体系卡牌。",
                ["TOGAWASAKIKO.eventDeathPreventionLine"] = "还没到这一步。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> CardLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["STRIKE_TOGAWASAKIKO.title"] = "Strike",
                ["STRIKE_TOGAWASAKIKO.description"] = "Deal {Damage:diff()} damage.",
                ["DEFEND_TOGAWASAKIKO.title"] = "Defend",
                ["DEFEND_TOGAWASAKIKO.description"] = "Gain {Block:diff()} [gold]Block[/gold].",
                ["SLANDER.title"] = "Slander",
                ["SLANDER.description"] = "Deal {Damage:diff()} damage. Deal [blue]2[/blue] more for each [b][gold]Pressure[/gold][/b] on the target.",
                ["UNENDURABLE.title"] = "Unendurable",
                ["UNENDURABLE.description"] = "Gain {Block:diff()} [gold]Block[/gold]. Apply {PressureAmount:diff()} [b][gold]Pressure[/gold][/b].",
                ["COMPOSE.title"] = "Compose",
                ["COMPOSE.description"] = "[b]Exhaust.[/b] Add a random [b][gold]Song[/gold][/b] card to your hand. It costs [blue]0[/blue] this combat.",
                ["I_HAVE_ASCENDED.title"] = "I Have Ascended",
                ["I_HAVE_ASCENDED.description"] = "Add [blue]1[/blue] {IfUpgraded:show:upgraded |}Apotheosis to your {IfUpgraded:show:hand|discard pile}.",
                ["THRILLED.title"] = "Thrilled",
                ["THRILLED.description"] = "Gain {Energy:energyIcons()}. Discard [blue]1[/blue] [b][gold]Song[/gold][/b] card.",
                ["COMPLETENESS.title"] = "Completeness",
                ["COMPLETENESS.description"] = "{IfUpgraded:show:[gold]Retain[/gold].\n|}Discard any number of cards. Draw that many cards. Then apply Pressure to all enemies equal to [blue]2[/blue] times the number discarded.",
                ["COMPLETENESS_SELECT_ANY.prompt"] = "Discard any number of cards.",
                ["PERK_UP.title"] = "Perk Up",
                ["PERK_UP.description"] = "Add a random [gold]Colorless[/gold] card to your hand. {IfUpgraded:show:Draw [blue]1[/blue] card.|}",
                ["SPEAK.title"] = "Speak!",
                ["SPEAK.description"] = "Add [blue]1[/blue] [blue]0[/blue]-cost [gold]Persona Dissociation[/gold] to your hand.",
                ["RESTORATION_OF_POWER.title"] = "Restoration of Power",
                ["RESTORATION_OF_POWER.description"] = "Add {Cards:diff()} random pressure-generated {IfUpgraded:show:cards|card} to your hand.",
                ["SHE_IS_RADIANT.title"] = "She Is Radiant",
                ["SHE_IS_RADIANT.description"] = "Remove all current Pressure from all enemies. Gain [blue]3[/blue] [gold]Strength[/gold].",
                ["PUT_ON_YOUR_MASK.title"] = "Put On Your Mask",
                ["PUT_ON_YOUR_MASK.description"] = "Apply {WeakAmount:diff()} [gold]Weak[/gold]. If the target already has Weak, gain [blue]1[/blue] [gold]Face[/gold].",
                ["SEVER_THE_PAST.title"] = "Sever the Past",
                ["SEVER_THE_PAST.description"] = "Deal {Damage:diff()} damage. Shuffle your discard pile into your draw pile.",
                ["ANSWER_ME.title"] = "Answer Me",
                ["ANSWER_ME.description"] = "Enemies with less than [blue]5[/blue] Pressure gain [blue]7[/blue] Pressure. Enemies with at least [blue]5[/blue] Pressure lose [blue]1[/blue] [gold]Strength[/gold].",
                ["NOTEBOOK.title"] = "Notebook",
                ["NOTEBOOK.description"] = "Convert all [b][gold]Social Withdrawal[/gold][/b] on the target into that much [b][gold]Pressure[/gold][/b].",
                ["LEAVE_IT_TO_ME.title"] = "Leave It to Me",
                ["LEAVE_IT_TO_ME.description"] = "Deal {Damage:diff()} damage. Remove up to [blue]7[/blue] Pressure from the target. Heal [blue]5[/blue] HP. If it still has Pressure, apply [blue]1[/blue] [gold]Weak[/gold].",
                ["FRAGILITY.title"] = "Fragility",
                ["FRAGILITY.description"] = "Gain {Block:diff()} [gold]Block[/gold]. Gain [blue]1[/blue] [gold]Face[/gold]. Heal {Heal:diff()} HP.",
                ["DAWN_OF_DESPAIR.title"] = "Dawn of Despair",
                ["DAWN_OF_DESPAIR.description"] = "Deal {Damage:diff()} damage {HitCount:diff()} times. Then apply [gold]Despair Echo[/gold] this turn.",
                ["SO_MANY_MAGGOTS.title"] = "So Many Maggots",
                ["SO_MANY_MAGGOTS.description"] = "Choose a player. Discard [blue]1[/blue] card. Remove all negative powers from them.",
                ["BAIL_MONEY.title"] = "Bail Money",
                ["BAIL_MONEY.description"] = "Deal {Damage:diff()} damage. Target loses [blue]1[/blue] [gold]Dexterity[/gold]. Lose [gold]10[/gold] Gold.",
                ["WEIGHTLIFTING_CHAMPION.title"] = "Weightlifting Champion",
                ["WEIGHTLIFTING_CHAMPION.description"] = "Lose {HpLoss:diff()} HP. Gain [blue]1[/blue] [gold]Strength[/gold] and [blue]1[/blue] [gold]Dexterity[/gold].",
                ["HOUSEWARMING.title"] = "Housewarming",
                ["HOUSEWARMING.description"] = "Add [blue]1[/blue] {IfUpgraded:show:upgraded |}[gold]Barking Barking Barking[/gold] with [gold]Ethereal[/gold] and [gold]Exhaust[/gold] to your hand.",
                ["SHADOW_OF_THE_PAST_I.title"] = "Shadow of the Past I",
                ["SHADOW_OF_THE_PAST_I.description"] = "[gold]Unplayable.[/gold] After [blue]2[/blue] combats, remove this from your [gold]Deck[/gold] and gain [blue]7[/blue] Max HP.",
                ["SHADOW_OF_THE_PAST_II.title"] = "Shadow of the Past II",
                ["SHADOW_OF_THE_PAST_II.description"] = "[gold]Unplayable.[/gold] After [blue]2[/blue] combats, remove this from your [gold]Deck[/gold], then remove [blue]1[/blue] starter Strike and [blue]1[/blue] starter Defend from your [gold]Deck[/gold].",
                ["SHADOW_OF_THE_PAST_I_I.title"] = "Shadow of the Past II",
                ["SHADOW_OF_THE_PAST_I_I.description"] = "[gold]Unplayable.[/gold] After [blue]2[/blue] combats, remove this from your [gold]Deck[/gold], then remove [blue]1[/blue] starter Strike and [blue]1[/blue] starter Defend from your [gold]Deck[/gold].",
                ["SHADOW_OF_THE_PAST_III.title"] = "Shadow of the Past III",
                ["SHADOW_OF_THE_PAST_III.description"] = "[gold]Unplayable.[/gold] After [blue]2[/blue] combats, remove this from your [gold]Deck[/gold] and upgrade your starter relic.",
                ["SHADOW_OF_THE_PAST_II_I.title"] = "Shadow of the Past III",
                ["SHADOW_OF_THE_PAST_II_I.description"] = "[gold]Unplayable.[/gold] After [blue]2[/blue] combats, remove this from your [gold]Deck[/gold] and upgrade your starter relic.",
                ["BARKING_BARKING_BARKING.title"] = "Barking Barking Barking",
                ["BARKING_BARKING_BARKING.description"] = "Deal {Damage:diff()} damage. Gain {Regen:diff()} [gold]Regen[/gold].",
                ["PULLMAN_CRASH.title"] = "Pullman Crash",
                ["PULLMAN_CRASH.description"] = "Deal {Damage:diff()} damage. If the target has more than [blue]8[/blue] Pressure, apply [blue]1[/blue] [gold]Vulnerable[/gold].",
                ["FINAL_CURTAIN.title"] = "Final Curtain",
                ["FINAL_CURTAIN.description"] = "Deal {Damage:diff()} damage to all enemies. Repeat this once for each current enemy.",
                ["BLADE_THROUGH_THE_HEART.title"] = "Blade Through the Heart",
                ["BLADE_THROUGH_THE_HEART.description"] = "Apply [blue]2[/blue] [gold]Vulnerable[/gold], [blue]2[/blue] [gold]Weak[/gold], and [red]-1[/red] [gold]Dexterity[/gold] to all enemies. Then deal {Damage:diff()} damage to all enemies.",
                ["AVE_MUJICA.title"] = "Ave Mujica",
                ["AVE_MUJICA.description"] = "At the start of your turn, after drawing, add a random pressure-generated card to your hand, then automatically play the top card of your draw pile.",
                ["A_WONDERFUL_WORLD_YET_NOWHERE_TO_BE_FOUND.title"] = "A Wonderful World, Yet Nowhere to Be Found",
                ["A_WONDERFUL_WORLD_YET_NOWHERE_TO_BE_FOUND.description"] = "Gain [gold]Mirror Flower, Water Moon[/gold]. This turn cycle, the total damage you take cannot exceed {DamageCap:diff()}. Excess damage becomes [blue]0[/blue], but the hit still counts as a damage event.",
                ["ANGLES.title"] = "Angles",
                ["ANGLES.description"] = "If every living enemy with an intent is attacking, apply [blue]1[/blue] [gold]Inferiority[/gold] to all enemies. Otherwise, apply [blue]1[/blue] [gold]Vulnerable[/gold] to all enemies.",
                ["ETHER.title"] = "Ether",
                ["ETHER.description"] = "Deal {Damage:diff()} damage {HitCount:diff()} times. Apply {PressureAmount:diff()} [b][gold]Pressure[/gold][/b].",
                ["GEORGETTE_ME_GEORGETTE_YOU.title"] = "Georgette Me, Georgette You",
                ["GEORGETTE_ME_GEORGETTE_YOU.description"] = "Choose an enemy. If your current HP is greater than or equal to theirs, apply {PressureAmount:diff()} [b][gold]Pressure[/gold][/b]. Otherwise, deal {Damage:diff()} damage.",
                ["SYMBOL_I.title"] = "Symbol I",
                ["SYMBOL_I.description"] = "Deal {Damage:diff()} damage [blue]3[/blue] times. Each hit targets a random enemy. Gain [gold]Symbol I[/gold] this turn.",
                ["SYMBOL_II.title"] = "Symbol II",
                ["SYMBOL_II.description"] = "Deal {Damage:diff()} damage to all enemies. Apply [blue]1[/blue] [gold]Inferiority[/gold] to each. Gain [gold]Symbol II[/gold] this turn.",
                ["SYMBOL_III.title"] = "Symbol III",
                ["SYMBOL_III.description"] = "Gain {Block:diff()} [gold]Block[/gold]. Permanently increase this card's [gold]Block[/gold] by {Increase:diff()}. Gain [gold]Symbol III[/gold] this turn.",
                ["SYMBOL_IV.title"] = "Symbol IV",
                ["SYMBOL_IV.description"] = "Gain [gold]Symbol IV[/gold].",
                ["CRUCIFIX_X.title"] = "Crucifix X",
                ["CRUCIFIX_X.description"] = "Deal {Damage:diff()} damage X times. If all enemies have at least [blue]4[/blue] Pressure, deal [blue]2[/blue] extra hits.",
                ["FACE.title"] = "Face",
                ["FACE.description"] = "Gain {Block:diff()} [gold]Block[/gold]. Until your next turn starts, whenever a monster hits you, apply [blue]3[/blue] [b][gold]Pressure[/gold][/b] to it.",
                ["SAKI_MOVE_PLZ.title"] = "Saki, Move Plz",
                ["SAKI_MOVE_PLZ.description"] = "Deal {Damage:diff()} damage. If the last card you played this turn was a [b][gold]Song[/gold][/b], apply [blue]1[/blue] [gold]Vulnerable[/gold].",
                ["MUSIC_OF_THE_CELESTIAL_SPHERE.title"] = "Music of the Celestial Sphere",
                ["MUSIC_OF_THE_CELESTIAL_SPHERE.description"] = "For each enemy, lose [blue]1[/blue] [gold]Strength[/gold] for every {PressureDivisor:diff()} Pressure.",
                ["KILL_KISS.title"] = "KillKiss",
                ["KILL_KISS.description"] = "At the start of the enemy turn, deal {IfUpgraded:show:damage equal to its Max HP|[blue]25[/blue] damage} to each enemy whose Pressure is greater than half its current HP.",
                ["KILLKISS.title"] = "KillKiss",
                ["KILLKISS.description"] = "At the start of the enemy turn, deal {IfUpgraded:show:damage equal to its Max HP|[blue]25[/blue] damage} to each enemy whose Pressure is greater than half its current HP.",
                ["PERSONA_DISSOCIATION.title"] = "Persona Dissociation",
                ["PERSONA_DISSOCIATION.description"] = "Apply [blue]1[/blue] Persona Dissociation.",
                ["SOCIAL_WITHDRAWAL.title"] = "Social Withdrawal",
                ["SOCIAL_WITHDRAWAL.description"] = "Apply 3 Social Withdrawal. Exhaust.",
                ["ALL_YOU_THINK_ABOUT_IS_YOURSELF.title"] = "All You Think About Is Yourself",
                ["ALL_YOU_THINK_ABOUT_IS_YOURSELF.description"] = "Deal 9 damage. Stun. Ethereal. Exhaust.",
                ["OVERWORK_ANXIETY.title"] = "Overwork Anxiety",
                ["OVERWORK_ANXIETY.description"] = "Draw 1 card. Exhaust.",
                ["BLACK_BIRTHDAY.title"] = "Black Birthday",
                ["BLACK_BIRTHDAY.description"] = "Gain {Energy:energyIcons()}. If the target has more than [blue]5[/blue] Pressure, gain {BonusEnergy:energyIcons()}.",
                ["TREASURE_PLEASURE.title"] = "Treasure Pleasure",
                ["TREASURE_PLEASURE.description"] = "Apply [blue]1[/blue] Persona Dissociation to yourself. Gain [b][gold]Magnetic Force - Hell Wargod[/gold][/b] this turn.",
                ["INNOCENCE.title"] = "Innocence",
                ["INNOCENCE.description"] = "At the start of your turn, apply {SocialWithdrawalAmount:diff()} [b][gold]Social Withdrawal[/gold][/b] to all enemies.",
                ["CHOIR_S_CHOIR.title"] = "Choir 'S' Choir",
                ["CHOIR_S_CHOIR.description"] = "Discard [blue]1[/blue] card. Then play all cards in your [gold]Exhaust Pile[/gold] for free. Choose random legal targets.",
                ["IMPRISONED_XII.title"] = "Imprisoned XII",
                ["IMPRISONED_XII.description"] = "[gold]Unplayable.[/gold] Whenever this is added to your hand, draw {Cards:diff()} {Cards:plural:card|cards}.",
                ["GOD_YOU_FOOL.title"] = "God, You Fool",
                ["GOD_YOU_FOOL.description"] = "Lose {HpLoss:diff()} HP. Draw [blue]2[/blue] cards.",
                ["MASQUERADE_RHAPSODY_REQUEST.title"] = "Mas?uerade Rhapsody Re?uest",
                ["MASQUERADE_RHAPSODY_REQUEST.description"] = "{IfUpgraded:show:|[gold]Exhaust[/gold].\n}Apply Pressure equal to half your missing HP, rounded down.",
                ["S_THE_WAY.title"] = "'S/' The Way",
                ["S_THE_WAY.description"] = "Deal {Damage:diff()} damage. If this kills, draw [blue]1[/blue] card. Otherwise, lose [blue]4[/blue] HP, and you and the target lose [blue]1[/blue] Dexterity this turn.",
                ["TWO_MOONS_DEEP_INTO_THE_FOREST.title"] = "Two Moons Deep Into The Forest",
                ["TWO_MOONS_DEEP_INTO_THE_FOREST.description"] = "Deal {Damage:diff()} damage. This combat, costs {Energy:energyIcons()} less for each [b][gold]Song[/gold][/b] you've played.",
                ["SOPHIE.title"] = "Sophie",
                ["SOPHIE.description"] = "Gain {Block:diff()} [gold]Block[/gold]. Apply [blue]1[/blue] Inferiority. Gain [blue]1[/blue] [gold]Weak[/gold] this turn."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["STRIKE_TOGAWASAKIKO.title"] = "打击",
                ["STRIKE_TOGAWASAKIKO.description"] = "造成{Damage:diff()}点伤害。",
                ["DEFEND_TOGAWASAKIKO.title"] = "防御",
                ["DEFEND_TOGAWASAKIKO.description"] = "获得{Block:diff()}点[gold]格挡[/gold]。",
                ["SLANDER.title"] = "中伤",
                ["SLANDER.description"] = "造成{Damage:diff()}点伤害。目标每有[b][blue]1[/blue][/b]层[b][gold]压力[/gold][/b]，额外造成[blue]2[/blue]点伤害。",
                ["UNENDURABLE.title"] = "难熬",
                ["UNENDURABLE.description"] = "获得{Block:diff()}点[gold]格挡[/gold]。给予目标{PressureAmount:diff()}层[b][gold]压力[/gold][/b]。",
                ["COMPOSE.title"] = "谱曲",
                ["COMPOSE.description"] = "[b]消耗。[/b]随机将1张[b][gold]歌曲[/gold][/b]牌加入手牌。该牌本场战斗费用变为[blue]0[/blue]。",
                ["I_HAVE_ASCENDED.title"] = "我已成神",
                ["I_HAVE_ASCENDED.description"] = "将[blue]1[/blue]张{IfUpgraded:show:升级后的|}【神化】加入{IfUpgraded:show:手牌|弃牌堆}。",
                ["THRILLED.title"] = "兴奋不已",
                ["THRILLED.description"] = "获得{Energy:energyIcons()}。丢弃[blue]1[/blue]张[b][gold]歌曲[/gold][/b]牌。",
                ["COMPLETENESS.title"] = "完美无缺",
                ["COMPLETENESS.description"] = "{IfUpgraded:show:[gold]保留[/gold]。\n|}丢弃任意数量的牌。抽等量的牌。然后使所有敌人获得等同于弃牌数量[blue]2[/blue]倍的[b][gold]压力[/gold][/b]。",
                ["COMPLETENESS_SELECT_ANY.prompt"] = "选择任意张牌来丢弃。",
                ["PERK_UP.title"] = "抖擞精神",
                ["PERK_UP.description"] = "随机将[blue]1[/blue]张[gold]无色牌[/gold]加入手牌。{IfUpgraded:show:抽[blue]1[/blue]张牌。|}",
                ["SPEAK.title"] = "说话！",
                ["SPEAK.description"] = "将[blue]1[/blue]张[blue]0[/blue]费【人格解离】加入手牌。",
                ["RESTORATION_OF_POWER.title"] = "复权",
                ["RESTORATION_OF_POWER.description"] = "随机将{Cards:diff()}张压力衍生牌加入手牌。",
                ["SHE_IS_RADIANT.title"] = "她在发光",
                ["SHE_IS_RADIANT.description"] = "移除所有敌人当前拥有的[b][gold]压力[/gold][/b]。你获得[blue]3[/blue]点[gold]力量[/gold]。",
                ["PUT_ON_YOUR_MASK.title"] = "面具戴好",
                ["PUT_ON_YOUR_MASK.description"] = "使目标获得{WeakAmount:diff()}层[gold]虚弱[/gold]。若其在打出前已经拥有虚弱，你获得[blue]1[/blue]层【颜】。",
                ["SEVER_THE_PAST.title"] = "斩断过去",
                ["SEVER_THE_PAST.description"] = "造成{Damage:diff()}点伤害。将你的弃牌堆洗回抽牌堆。",
                ["ANSWER_ME.title"] = "作出回答",
                ["ANSWER_ME.description"] = "使压力小于[blue]5[/blue]的敌人获得[blue]7[/blue]层压力。使压力大于等于[blue]5[/blue]的敌人失去[blue]1[/blue]点力量。",
                ["NOTEBOOK.title"] = "日记本",
                ["NOTEBOOK.description"] = "将目标身上的全部[b][gold]自闭[/gold][/b]转化为等量的[b][gold]压力[/gold][/b]。",
                ["LEAVE_IT_TO_ME.title"] = "交给我吧",
                ["LEAVE_IT_TO_ME.description"] = "造成{Damage:diff()}点伤害。移除目标至多[blue]7[/blue]层压力。回复[blue]5[/blue]点生命。若其仍有压力，使其获得[blue]1[/blue]层[gold]虚弱[/gold]。",
                ["FRAGILITY.title"] = "脆弱",
                ["FRAGILITY.description"] = "获得{Block:diff()}点[gold]格挡[/gold]。获得[blue]1[/blue]层【颜】。回复{Heal:diff()}点生命值。",
                ["DAWN_OF_DESPAIR.title"] = "绝望伊始",
                ["DAWN_OF_DESPAIR.description"] = "造成{Damage:diff()}点伤害{HitCount:diff()}次。然后使目标本回合获得【绝望回响】。",
                ["SO_MANY_MAGGOTS.title"] = "好多蛆",
                ["SO_MANY_MAGGOTS.description"] = "选择1名玩家。丢弃[blue]1[/blue]张牌。移除其身上的所有负面Power。",
                ["BAIL_MONEY.title"] = "保释金",
                ["BAIL_MONEY.description"] = "造成{Damage:diff()}点伤害。使目标失去[blue]1[/blue]点[gold]敏捷[/gold]。然后失去[gold]10[/gold]金币。",
                ["WEIGHTLIFTING_CHAMPION.title"] = "举重冠军",
                ["WEIGHTLIFTING_CHAMPION.description"] = "失去{HpLoss:diff()}点生命。获得[blue]1[/blue]点[gold]力量[/gold]和[blue]1[/blue]点[gold]敏捷[/gold]。",
                ["HOUSEWARMING.title"] = "乔迁",
                ["HOUSEWARMING.description"] = "将[blue]1[/blue]张带有[gold]虚无[/gold]和[gold]消耗[/gold]的{IfUpgraded:show:升级后的|}【大狗大狗叫叫叫】加入手牌。",
                ["SHADOW_OF_THE_PAST_I.title"] = "往日之影1",
                ["SHADOW_OF_THE_PAST_I.description"] = "[gold]无法打出。[/gold]完成[blue]2[/blue]次战斗后，将其从[gold]牌组[/gold]移除，并增加[blue]7[/blue]点最大生命值。",
                ["SHADOW_OF_THE_PAST_II.title"] = "往日之影2",
                ["SHADOW_OF_THE_PAST_II.description"] = "[gold]无法打出。[/gold]完成[blue]2[/blue]次战斗后，将其从[gold]牌组[/gold]移除，并移除[blue]1[/blue]张起始打击和[blue]1[/blue]张起始防御。",
                ["SHADOW_OF_THE_PAST_I_I.title"] = "往日之影2",
                ["SHADOW_OF_THE_PAST_I_I.description"] = "[gold]无法打出。[/gold]完成[blue]2[/blue]次战斗后，将其从[gold]牌组[/gold]移除，并移除[blue]1[/blue]张起始打击和[blue]1[/blue]张起始防御。",
                ["SHADOW_OF_THE_PAST_III.title"] = "往日之影3",
                ["SHADOW_OF_THE_PAST_III.description"] = "[gold]无法打出。[/gold]完成[blue]2[/blue]次战斗后，将其从[gold]牌组[/gold]移除，并升级你的起始遗物。",
                ["SHADOW_OF_THE_PAST_II_I.title"] = "往日之影3",
                ["SHADOW_OF_THE_PAST_II_I.description"] = "[gold]无法打出。[/gold]完成[blue]2[/blue]次战斗后，将其从[gold]牌组[/gold]移除，并升级你的起始遗物。",
                ["BARKING_BARKING_BARKING.title"] = "大狗大狗叫叫叫",
                ["BARKING_BARKING_BARKING.description"] = "造成{Damage:diff()}点伤害。获得{Regen:diff()}层[gold]回复[/gold]。",
                ["PULLMAN_CRASH.title"] = "普尔曼冲击",
                ["PULLMAN_CRASH.description"] = "造成{Damage:diff()}点伤害。若目标的压力层数大于[blue]8[/blue]，使其获得[blue]1[/blue]层[gold]易伤[/gold]。",
                ["FINAL_CURTAIN.title"] = "谢幕",
                ["FINAL_CURTAIN.description"] = "对所有敌人造成{Damage:diff()}点伤害。重复次数等于当前敌人总数。",
                ["BLADE_THROUGH_THE_HEART.title"] = "利刃穿心",
                ["BLADE_THROUGH_THE_HEART.description"] = "使所有敌人获得[blue]2[/blue]层[gold]易伤[/gold]和[blue]2[/blue]层[gold]虚弱[/gold]，并失去[blue]1[/blue]点[gold]敏捷[/gold]。然后对所有敌人造成{Damage:diff()}点伤害。",
                ["AVE_MUJICA.title"] = "Ave Mujica",
                ["AVE_MUJICA.description"] = "在你的回合开始时，于常规抽牌之后，随机将1张压力衍生牌加入手牌，然后自动打出抽牌堆顶的第1张牌。",
                ["A_WONDERFUL_WORLD_YET_NOWHERE_TO_BE_FOUND.title"] = "A Wonderful World, Yet Nowhere to Be Found",
                ["A_WONDERFUL_WORLD_YET_NOWHERE_TO_BE_FOUND.description"] = "获得【镜花水月】。在本轮回内，你受到的伤害总量不会超过{DamageCap:diff()}点。超出的部分改为[blue]0[/blue]，但该次受伤事件仍然视为发生。",
                ["ANGLES.title"] = "Angles",
                ["ANGLES.description"] = "若所有存活且当前有意图的敌人都在攻击，则使所有敌人各获得[blue]1[/blue]层【自卑】。否则，使所有敌人各获得[blue]1[/blue]层[gold]易伤[/gold]。",
                ["ETHER.title"] = "以太",
                ["ETHER.description"] = "造成{Damage:diff()}点伤害{HitCount:diff()}次。给予目标{PressureAmount:diff()}层[b][gold]压力[/gold][/b]。",
                ["GEORGETTE_ME_GEORGETTE_YOU.title"] = "Georgette Me, Georgette You",
                ["GEORGETTE_ME_GEORGETTE_YOU.description"] = "选择1名敌人。若你的当前生命值大于等于其当前生命值，则施加{PressureAmount:diff()}层[b][gold]压力[/gold][/b]；否则造成{Damage:diff()}点伤害。",
                ["SYMBOL_I.title"] = "Symbol I",
                ["SYMBOL_I.description"] = "造成{Damage:diff()}点伤害[blue]3[/blue]次，每次随机选择目标。你在本回合获得【Symbol I】。",
                ["SYMBOL_II.title"] = "Symbol II",
                ["SYMBOL_II.description"] = "对所有敌人造成{Damage:diff()}点伤害，并使其各获得[blue]1[/blue]层【自卑】。你在本回合获得【Symbol II】。",
                ["SYMBOL_III.title"] = "Symbol III",
                ["SYMBOL_III.description"] = "获得{Block:diff()}点格挡。每打出一次，这张牌在本局游戏中的格挡值永久增加{Increase:diff()}点。你在本回合获得【Symbol III】。",
                ["SYMBOL_IV.title"] = "Symbol IV",
                ["SYMBOL_IV.description"] = "获得【Symbol IV】。",
                ["CRUCIFIX_X.title"] = "十字架X",
                ["CRUCIFIX_X.description"] = "造成{Damage:diff()}点伤害X次。若所有敌人都至少有[b][blue]4[/blue][/b]层[b][gold]压力[/gold][/b]，额外造成[blue]2[/blue]次伤害。",
                ["FACE.title"] = "颜",
                ["FACE.description"] = "获得{Block:diff()}点[gold]格挡[/gold]。直到你的下个回合开始前，每当怪物攻击命中你时，无论是否被格挡，都给予其[blue]3[/blue]层[b][gold]压力[/gold][/b]。",
                ["SAKI_MOVE_PLZ.title"] = "祥，移动",
                ["SAKI_MOVE_PLZ.description"] = "造成{Damage:diff()}点伤害。若你本回合打出的上一张牌是[b][gold]歌曲[/gold][/b]牌，使目标获得[blue]1[/blue]层[gold]易伤[/gold]。",
                ["MUSIC_OF_THE_CELESTIAL_SPHERE.title"] = "天穹之乐",
                ["MUSIC_OF_THE_CELESTIAL_SPHERE.description"] = "使所有敌人每有{PressureDivisor:diff()}层[b][gold]压力[/gold][/b]便失去[blue]1[/blue]点[gold]力量[/gold]。",
                ["KILL_KISS.title"] = "KillKiss",
                ["KILL_KISS.description"] = "怪物回合开始时，若其压力层数严格大于其当前剩余生命值的一半，则对其造成{IfUpgraded:show:等同于其最大生命值的伤害|[blue]25[/blue]点伤害}。",
                ["KILLKISS.title"] = "KillKiss",
                ["KILLKISS.description"] = "怪物回合开始时，若其压力层数严格大于其当前剩余生命值的一半，则对其造成{IfUpgraded:show:等同于其最大生命值的伤害|[blue]25[/blue]点伤害}。",
                ["PERSONA_DISSOCIATION.title"] = "人格解离",
                ["PERSONA_DISSOCIATION.description"] = "施加1层人格解离",
                ["SOCIAL_WITHDRAWAL.title"] = "自闭",
                ["SOCIAL_WITHDRAWAL.description"] = "施加3层自闭。消耗。",
                ["ALL_YOU_THINK_ABOUT_IS_YOURSELF.title"] = "满脑子都想着自己",
                ["ALL_YOU_THINK_ABOUT_IS_YOURSELF.description"] = "造成9点伤害。晕眩。虚无。消耗。",
                ["OVERWORK_ANXIETY.title"] = "过劳焦虑",
                ["OVERWORK_ANXIETY.description"] = "抽1张牌。消耗。",
                ["BLACK_BIRTHDAY.title"] = "黑色生日",
                ["BLACK_BIRTHDAY.description"] = "获得{Energy:energyIcons()}。若目标的压力层数大于[blue]5[/blue]，再获得{BonusEnergy:energyIcons()}。",
                ["TREASURE_PLEASURE.title"] = "Treasure Pleasure",
                ["TREASURE_PLEASURE.description"] = "你获得[blue]1[/blue]层人格解离。获得[b][gold]磁场力量-地狱战神[/gold][/b]直到本回合结束。",
                ["INNOCENCE.title"] = "天真",
                ["INNOCENCE.description"] = "在你的回合开始时，使所有敌人获得{SocialWithdrawalAmount:diff()}层[b][gold]自闭[/gold][/b]。",
                ["CHOIR_S_CHOIR.title"] = "Choir 'S' Choir",
                ["CHOIR_S_CHOIR.description"] = "丢弃[blue]1[/blue]张牌。然后免费打出你[gold]消耗牌堆[/gold]中的所有牌。若需要目标，则随机选择合法目标。",
                ["IMPRISONED_XII.title"] = "被囚禁的12",
                ["IMPRISONED_XII.description"] = "[gold]无法打出。[/gold]每当这张牌加入你的手牌时，抽{Cards:diff()}张牌。",
                ["GOD_YOU_FOOL.title"] = "神明，笨蛋",
                ["GOD_YOU_FOOL.description"] = "失去{HpLoss:diff()}点生命。抽[blue]2[/blue]张牌。",
                ["MASQUERADE_RHAPSODY_REQUEST.title"] = "Mas?uerade Rhapsody Re?uest",
                ["MASQUERADE_RHAPSODY_REQUEST.description"] = "{IfUpgraded:show:|[gold]消耗[/gold]。\n}给予目标等同于你已损失生命值一半、向下取整的[b][gold]压力[/gold][/b]。",
                ["S_THE_WAY.title"] = "'S/' The Way",
                ["S_THE_WAY.description"] = "造成{Damage:diff()}点伤害。若击杀目标，抽[blue]1[/blue]张牌。否则，你失去[blue]4[/blue]点生命，且你与目标在本回合各失去[blue]1[/blue]点敏捷。",
                ["TWO_MOONS_DEEP_INTO_THE_FOREST.title"] = "两轮月亮 深入森林之中",
                ["TWO_MOONS_DEEP_INTO_THE_FOREST.description"] = "造成{Damage:diff()}点伤害。本场战斗中，你每打出[blue]1[/blue]张[b][gold]歌曲[/gold][/b]牌，此牌费用减少{Energy:energyIcons()}。",
                ["SOPHIE.title"] = "Sophie",
                ["SOPHIE.description"] = "获得{Block:diff()}点[gold]格挡[/gold]。使目标获得[blue]1[/blue]层自卑。你在本回合获得[blue]1[/blue]层[gold]虚弱[/gold]。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> PowerLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["PRESSURE_POWER.title"] = "Pressure",
                ["PRESSURE_POWER.description"] = "他们感觉喘不过气...",
                ["SAKIKO_DESPAIR_ECHO_POWER.title"] = "Despair Echo",
                ["SAKIKO_DESPAIR_ECHO_POWER.description"] = "This turn, whenever this creature takes damage, it gains [blue]3[/blue] Pressure per stack.",
                ["PERSONA_DISSOCIATION_POWER.title"] = "Persona Dissociation",
                ["PERSONA_DISSOCIATION_POWER.description"] = "Each stack doubles the next damage this creature would take before block is applied. Lose 1 stack each time it happens.",
                ["MAGNETIC_FORCE_HELL_WARGOD_POWER.title"] = "Magnetic Force - Hell Wargod",
                ["MAGNETIC_FORCE_HELL_WARGOD_POWER.description"] = "This turn, each Attack you play is played an extra time.",
                ["SOCIAL_WITHDRAWAL_POWER.title"] = "Social Withdrawal",
                ["SOCIAL_WITHDRAWAL_POWER.description"] = "At the end of this creature's turn, it takes 7 damage, then loses 1 stack.",
                ["INFERIORITY_POWER.title"] = "Inferiority",
                ["INFERIORITY_POWER.description"] = "Each stack lasts 1 turn. When applied, convert 1 Pressure into Overwork Anxiety. While it lasts, whenever this creature takes damage, it loses 1 Strength. At the end of its turn, lose 1 stack.",
                ["AVE_MUJICA_POWER.title"] = "Ave Mujica",
                ["AVE_MUJICA_POWER.description"] = "At the start of your turn, after drawing, add a random pressure-generated card to your hand, then automatically play the top card of your draw pile.",
                ["MIRROR_FLOWER_WATER_MOON_POWER.title"] = "Mirror Flower, Water Moon",
                ["MIRROR_FLOWER_WATER_MOON_POWER.description"] = "This turn cycle, the total HP damage you take cannot exceed the shown amount. Excess damage becomes 0, but the hit still counts as a damage event.",
                ["FACE_REACTION_POWER.title"] = "Face",
                ["FACE_REACTION_POWER.description"] = "Until your next turn starts, whenever a monster hits you, apply [blue]3[/blue] Pressure per stack to it, even if the hit is fully blocked.",
                ["INNOCENCE_POWER.title"] = "Innocence",
                ["INNOCENCE_POWER.description"] = "At the start of your turn, apply this many stacks of [gold]Social Withdrawal[/gold] to all enemies.",
                ["SYMBOL_I_POWER.title"] = "Symbol I",
                ["SYMBOL_I_POWER.description"] = "This turn-only Symbol marker counts as 1 Symbol kind at end of turn. Additional stacks do not increase the number of Symbol kinds.",
                ["SYMBOL_II_POWER.title"] = "Symbol II",
                ["SYMBOL_II_POWER.description"] = "This turn-only Symbol marker counts as 1 Symbol kind at end of turn. Additional stacks do not increase the number of Symbol kinds.",
                ["SYMBOL_III_POWER.title"] = "Symbol III",
                ["SYMBOL_III_POWER.description"] = "This turn-only Symbol marker counts as 1 Symbol kind at end of turn. Additional stacks do not increase the number of Symbol kinds.",
                ["SYMBOL_IV_POWER.title"] = "Symbol IV",
                ["SYMBOL_IV_POWER.description"] = "At the start of your turn, draw [blue]1[/blue] extra card per stack. It still counts as only 1 Symbol kind at end of turn.",
                ["KILL_KISS_POWER.title"] = "KillKiss",
                ["KILL_KISS_POWER.description"] = "At the start of the enemy turn, deal damage to enemies whose Pressure exceeds half their current HP.",
                ["KILL_KISS_PLUS_POWER.title"] = "KillKiss+",
                ["KILL_KISS_PLUS_POWER.description"] = "At the start of the enemy turn, deal damage equal to Max HP to enemies whose Pressure exceeds half their current HP.",
                ["KILLKISS_POWER.title"] = "KillKiss",
                ["KILLKISS_POWER.description"] = "At the start of the enemy turn, deal damage to enemies whose Pressure exceeds half their current HP.",
                ["THE_WAY_TEMPORARY_DEXTERITY_LOSS_POWER.title"] = "Temporary Dexterity Loss",
                ["THE_WAY_TEMPORARY_DEXTERITY_LOSS_POWER.description"] = "Lose 1 Dexterity this turn.",
                ["TOGAWASAKIKO_COMBAT_WATCHER_POWER.title"] = "Pressure Watcher",
                ["TOGAWASAKIKO_COMBAT_WATCHER_POWER.description"] = "Hidden implementation power for pressure generation."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["PRESSURE_POWER.title"] = "压力",
                ["PRESSURE_POWER.description"] = "他们感觉喘不过气...",
                ["SAKIKO_DESPAIR_ECHO_POWER.title"] = "绝望回响",
                ["SAKIKO_DESPAIR_ECHO_POWER.description"] = "本回合内，每当该生物受到伤害时，都会按层数额外获得每层[blue]3[/blue]层压力。",
                ["PERSONA_DISSOCIATION_POWER.title"] = "人格解离",
                ["PERSONA_DISSOCIATION_POWER.description"] = "每层都会让下一次受到的伤害在结算格挡前先翻倍。每次触发后失去1层。",
                ["MAGNETIC_FORCE_HELL_WARGOD_POWER.title"] = "磁场力量-地狱战神",
                ["MAGNETIC_FORCE_HELL_WARGOD_POWER.description"] = "本回合中，你打出的每一张攻击牌都会额外结算1次。",
                ["SOCIAL_WITHDRAWAL_POWER.title"] = "自闭",
                ["SOCIAL_WITHDRAWAL_POWER.description"] = "在该生物回合结束时，受到7点伤害，然后失去1层。",
                ["INFERIORITY_POWER.title"] = "自卑",
                ["INFERIORITY_POWER.description"] = "每层持续1个回合。施加时，把1层压力兑换为过劳焦虑。持续期间内，每次受到伤害时失去1点力量。在该生物回合结束时，失去1层。",
                ["AVE_MUJICA_POWER.title"] = "Ave Mujica",
                ["AVE_MUJICA_POWER.description"] = "在你的回合开始时，于常规抽牌之后，随机将1张压力衍生牌加入手牌，然后自动打出抽牌堆顶的第1张牌。",
                ["MIRROR_FLOWER_WATER_MOON_POWER.title"] = "镜花水月",
                ["MIRROR_FLOWER_WATER_MOON_POWER.description"] = "在本轮回内，你受到的生命伤害总量不会超过显示数值。超出的部分改为0，但该次受伤事件仍然视为发生。",
                ["FACE_REACTION_POWER.title"] = "颜",
                ["FACE_REACTION_POWER.description"] = "直到你的下个回合开始前，每当怪物攻击命中你时，无论是否被格挡，都给予其每层[blue]3[/blue]层压力。",
                ["INNOCENCE_POWER.title"] = "天真",
                ["INNOCENCE_POWER.description"] = "在你的回合开始时，使所有敌人获得等同于该层数的[b][gold]自闭[/gold][/b]。",
                ["SYMBOL_I_POWER.title"] = "Symbol I",
                ["SYMBOL_I_POWER.description"] = "本回合标志物。回合结束时只按是否拥有该种 Symbol 计数；同种额外层数不会增加 Symbol 种类数。",
                ["SYMBOL_II_POWER.title"] = "Symbol II",
                ["SYMBOL_II_POWER.description"] = "本回合标志物。回合结束时只按是否拥有该种 Symbol 计数；同种额外层数不会增加 Symbol 种类数。",
                ["SYMBOL_III_POWER.title"] = "Symbol III",
                ["SYMBOL_III_POWER.description"] = "本回合标志物。回合结束时只按是否拥有该种 Symbol 计数；同种额外层数不会增加 Symbol 种类数。",
                ["SYMBOL_IV_POWER.title"] = "Symbol IV",
                ["SYMBOL_IV_POWER.description"] = "在你的回合开始时，每层额外抽[blue]1[/blue]张牌。回合结束统计 Symbol 时，无论几层都只算[blue]1[/blue]种。",
                ["KILL_KISS_POWER.title"] = "KillKiss",
                ["KILL_KISS_POWER.description"] = "怪物回合开始时，对压力层数严格大于其当前剩余生命值一半的敌人造成伤害。",
                ["KILL_KISS_PLUS_POWER.title"] = "KillKiss+",
                ["KILL_KISS_PLUS_POWER.description"] = "怪物回合开始时，对压力层数严格大于其当前剩余生命值一半的敌人造成等同于最大生命值的伤害。",
                ["KILLKISS_POWER.title"] = "KillKiss",
                ["KILLKISS_POWER.description"] = "怪物回合开始时，对压力层数严格大于其当前剩余生命值一半的敌人造成伤害。",
                ["THE_WAY_TEMPORARY_DEXTERITY_LOSS_POWER.title"] = "临时敏捷下降",
                ["THE_WAY_TEMPORARY_DEXTERITY_LOSS_POWER.description"] = "本回合失去1点敏捷。",
                ["TOGAWASAKIKO_COMBAT_WATCHER_POWER.title"] = "压力监听",
                ["TOGAWASAKIKO_COMBAT_WATCHER_POWER.description"] = "用于压力衍生牌生成的隐藏实现 power。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> RelicLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["DOLL_MASK.title"] = "Doll Mask",
                ["DOLL_MASK.description"] = "At the start of your turn, apply [b][blue]1[/blue][/b] [b][gold]Pressure[/gold][/b] to all enemies.",
                ["DOLL_MASK.flavor"] = "A quiet ignition point for the pressure system.",
                ["UPGRADED_DOLL_MASK.title"] = "Dollmusk Plus",
                ["UPGRADED_DOLL_MASK.description"] = "At the start of your turn, apply [b][blue]3[/blue][/b] [b][gold]Pressure[/gold][/b] to all enemies.",
                ["UPGRADED_DOLL_MASK.flavor"] = "A mask that no longer bothers to whisper.",
                ["PIANO_OF_MOM.title"] = "Piano of Mom",
                ["PIANO_OF_MOM.description"] = "At the start of each combat, add 1 random upgraded Song card to your hand.",
                ["PIANO_OF_MOM.flavor"] = "The first sound that taught her what silence costs.",
                ["BEST_COMPANION.title"] = "Best Companion(?)",
                ["BEST_COMPANION.description"] = "Upon pickup, add [gold]Barking Barking Barking[/gold] to your deck.",
                ["BEST_COMPANION.flavor"] = "A relic stub for Togawa Teiji's future ancient event.",
                ["BLACK_LIMOUSINE.title"] = "Black Limousine",
                ["BLACK_LIMOUSINE.description"] = "Upon pickup, add [gold]Pullman Crash[/gold] to your deck.",
                ["BLACK_LIMOUSINE.flavor"] = "A polished promise of impact."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["DOLL_MASK.title"] = "人偶的假面",
                ["DOLL_MASK.description"] = "在你的回合开始时，给予所有敌人[b][blue]1[/blue][/b]层[b][gold]压力[/gold][/b]。",
                ["DOLL_MASK.flavor"] = "压力体系的最小点火器。",
                ["UPGRADED_DOLL_MASK.title"] = "人偶的假面Plus",
                ["UPGRADED_DOLL_MASK.description"] = "在你的回合开始时，给予所有敌人[b][blue]3[/blue][/b]层[b][gold]压力[/gold][/b]。",
                ["UPGRADED_DOLL_MASK.flavor"] = "它不再掩饰任何恶意。",
                ["PIANO_OF_MOM.title"] = "妈妈的钢琴",
                ["PIANO_OF_MOM.description"] = "每场战斗开始时，随机将1张升级后的歌曲牌加入手牌。",
                ["PIANO_OF_MOM.flavor"] = "最初教会她沉默代价的声音。",
                ["BEST_COMPANION.title"] = "最好的伙伴(?",
                ["BEST_COMPANION.description"] = "获得时，将[gold]大狗大狗叫叫叫[/gold]加入你的牌组。",
                ["BEST_COMPANION.flavor"] = "用于丰川定治先古之民事件的 relic 占位实现。",
                ["BLACK_LIMOUSINE.title"] = "黑色高级车",
                ["BLACK_LIMOUSINE.description"] = "获得时，将[gold]普尔曼冲击[/gold]加入你的牌组。",
                ["BLACK_LIMOUSINE.flavor"] = "一辆足以撞碎体面的车。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AncientLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["TOGAWA_TEIJI.title"] = "Togawa Teiji",
                ["TOGAWA_TEIJI.epithet"] = "Head of House Togawa",
                ["TOGAWA_TEIJI.pages.INITIAL.description"] = "Togawa Teiji watches in silence, then gestures for you to keep moving.",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-0.char"] = "Grandfather, you truly are everywhere.",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-0.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-1.ancient"] = "A sharp tongue alone cannot carry the weight of other people's lives.",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-1.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-2.char"] = "...",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-2.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-3.ancient"] = "Sigh... take these, and keep going.",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-0.char"] = "Grandfather, you truly are everywhere.",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-0.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-1.ancient"] = "A sharp tongue alone cannot carry the weight of other people's lives.",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-1.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-2.char"] = "...",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-2.next"] = "Continue",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-3.ancient"] = "Sigh... take these, and keep going.",
                ["TOGAWA_TEIJI.pages.INITIAL.options.CONTINUE_PERFORMING.title"] = "Keep Performing",
                ["TOGAWA_TEIJI.pages.INITIAL.options.CONTINUE_PERFORMING.description"] = "Gain [gold]1000[/gold] Gold."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["TOGAWA_TEIJI.title"] = "丰川定治",
                ["TOGAWA_TEIJI.epithet"] = "家主",
                ["TOGAWA_TEIJI.pages.INITIAL.description"] = "丰川定治沉默地注视着你，随后示意你继续前进。",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-0.char"] = "爷爷你可真是无处不在",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-0.next"] = "继续",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-1.ancient"] = "光靠口舌之快可不能背负其他人的人生",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-1.next"] = "继续",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-2.char"] = "……",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-2.next"] = "继续",
                ["TOGAWA_TEIJI.talk.firstVisitEver.0-3.ancient"] = "唉……拿上这些，继续走吧",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-0.char"] = "爷爷你可真是无处不在",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-0.next"] = "继续",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-1.ancient"] = "光靠口舌之快可不能背负其他人的人生",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-1.next"] = "继续",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-2.char"] = "……",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-2.next"] = "继续",
                ["TOGAWA_TEIJI.talk.TOGAWASAKIKO.0-3.ancient"] = "唉……拿上这些，继续走吧",
                ["TOGAWA_TEIJI.pages.INITIAL.options.CONTINUE_PERFORMING.title"] = "继续演出吧",
                ["TOGAWA_TEIJI.pages.INITIAL.options.CONTINUE_PERFORMING.description"] = "获得[gold]1000[/gold]金币。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> CardLibraryLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["POOL_TOGAWASAKIKO_TIP"] = "Togawa Sakiko cards."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["POOL_TOGAWASAKIKO_TIP"] = "丰川祥子的卡牌。"
            }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> EventLocEntries =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["eng"] = new Dictionary<string, string>
            {
                ["UNATTENDED_PIANO.title"] = "Unattended Piano",
                ["UNATTENDED_PIANO.pages.INITIAL.description"] = "You step into an abandoned music classroom...\n\nAn apparently soulless piano sits beside the window...",
                ["UNATTENDED_PIANO.pages.INITIAL.options.LEAVE.title"] = "Pay It No Mind",
                ["UNATTENDED_PIANO.pages.INITIAL.options.LEAVE.description"] = "Turn away from the room and take a sip of red tea. Heal [blue]12[/blue] HP.",
                ["UNATTENDED_PIANO.pages.INITIAL.options.PLAY.title"] = "A Formless Hand",
                ["UNATTENDED_PIANO.pages.INITIAL.options.PLAY.description"] = "Sit down and begin to play.",
                ["UNATTENDED_PIANO.pages.PLAY_1.description"] = "You feel an electric current running through your fingertips.\n\nWill you keep playing?",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.CONTINUE.title"] = "Continue Playing",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.CONTINUE.description"] = "Lose [red]6[/red] HP. Gain a random [gold]Shadow of the Past[/gold].",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.STOP.title"] = "Stop Playing",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.STOP.description"] = "Leave the classroom.",
                ["UNATTENDED_PIANO.pages.PLAY_2.description"] = "A faint sorrow wells up inside your heart.\n\nBlood trickles from your fingers.",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.CONTINUE.title"] = "Continue Playing",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.CONTINUE.description"] = "Lose [red]6[/red] HP. Gain a different random [gold]Shadow of the Past[/gold].",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.STOP.title"] = "Stop Playing",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.STOP.description"] = "Leave the classroom.",
                ["UNATTENDED_PIANO.pages.PLAY_3.description"] = "Scenes from long ago replay in your mind... every beautiful memory returns at once.\n\nYou cry aloud.\n\nBlood pours from your fingers.",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.CONTINUE.title"] = "Continue Playing",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.CONTINUE.description"] = "Lose [red]6[/red] HP. Gain the final [gold]Shadow of the Past[/gold].",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.STOP.title"] = "Stop Playing",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.STOP.description"] = "Leave the classroom.",
                ["UNATTENDED_PIANO.pages.LEAVE.description"] = "You turn away from the piano, lift the cup on the nearby desk, and take a quiet sip of tea before leaving.",
                ["UNATTENDED_PIANO.pages.STOP.description"] = "You stop playing and leave the classroom without looking back.",
                ["UNATTENDED_PIANO.pages.FINAL.description"] = "You slowly rise to your feet and stagger out of the classroom.",
                ["UNATTENDED_PIANO.pages.FINAL.options.LEAVE.title"] = "Leave",
                ["UNATTENDED_PIANO.pages.FINAL.options.LEAVE.description"] = "Step away from the piano.",
                ["UNATTENDED_PIANO.pages.FINAL_EXIT.description"] = "You slowly rise to your feet and stagger out of the classroom."
            },
            ["zhs"] = new Dictionary<string, string>
            {
                ["UNATTENDED_PIANO.title"] = "无人问津的钢琴",
                ["UNATTENDED_PIANO.pages.INITIAL.description"] = "你走进了一间音乐教室……\n\n靠窗处摆放着一架失去了灵魂的钢琴……",
                ["UNATTENDED_PIANO.pages.INITIAL.options.LEAVE.title"] = "不以为意",
                ["UNATTENDED_PIANO.pages.INITIAL.options.LEAVE.description"] = "转身离开房间，喝一口红茶吧。恢复[blue]12[/blue]点生命值。",
                ["UNATTENDED_PIANO.pages.INITIAL.options.PLAY.title"] = "无形之手",
                ["UNATTENDED_PIANO.pages.INITIAL.options.PLAY.description"] = "坐上座位开始弹琴。",
                ["UNATTENDED_PIANO.pages.PLAY_1.description"] = "你感受到电流从指尖传来。\n\n是否继续弹奏？",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.CONTINUE.title"] = "继续",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.CONTINUE.description"] = "失去[red]6[/red]点生命值。随机获得[gold]1[/gold]张[gold]往日之影[/gold]。",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.STOP.title"] = "停止弹琴",
                ["UNATTENDED_PIANO.pages.PLAY_1.options.STOP.description"] = "离开房间。",
                ["UNATTENDED_PIANO.pages.PLAY_2.description"] = "若有若无的悲凉在你心中涌起。\n\n手指流出鲜血。",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.CONTINUE.title"] = "继续",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.CONTINUE.description"] = "失去[red]6[/red]点生命值。随机获得[gold]1[/gold]张不同的[gold]往日之影[/gold]。",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.STOP.title"] = "停止弹琴",
                ["UNATTENDED_PIANO.pages.PLAY_2.options.STOP.description"] = "离开房间。",
                ["UNATTENDED_PIANO.pages.PLAY_3.description"] = "往日种种……一切的美好都在你脑中来回播放。\n\n你放声大哭。\n\n手指已血流如注。",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.CONTINUE.title"] = "继续",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.CONTINUE.description"] = "失去[red]6[/red]点生命值。获得最后[gold]1[/gold]张[gold]往日之影[/gold]。",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.STOP.title"] = "停止弹琴",
                ["UNATTENDED_PIANO.pages.PLAY_3.options.STOP.description"] = "离开房间。",
                ["UNATTENDED_PIANO.pages.LEAVE.description"] = "你转身离开了钢琴，顺手端起桌边的红茶轻轻抿了一口，然后离开房间。",
                ["UNATTENDED_PIANO.pages.STOP.description"] = "你停下了弹奏，沉默地离开了房间。",
                ["UNATTENDED_PIANO.pages.FINAL.description"] = "你缓慢起身，蹒跚离开教室",
                ["UNATTENDED_PIANO.pages.FINAL.options.LEAVE.title"] = "起身离开",
                ["UNATTENDED_PIANO.pages.FINAL.options.LEAVE.description"] = "离开教室。",
                ["UNATTENDED_PIANO.pages.FINAL_EXIT.description"] = "你缓慢起身，蹒跚离开教室"
            }
        };

    public static void EnsureLocalizationOverrides()
    {
        try
        {
            bool changed = false;
            changed |= UpsertLocalizationTable("eng", "characters", CharacterLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "characters", CharacterLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "cards", CardLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "cards", CardLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "powers", PowerLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "powers", PowerLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "relics", RelicLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "relics", RelicLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "ancients", AncientLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "ancients", AncientLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "events", EventLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "events", EventLocEntries["zhs"]);
            changed |= UpsertLocalizationTable("eng", "card_library", CardLibraryLocEntries["eng"]);
            changed |= UpsertLocalizationTable("zhs", "card_library", CardLibraryLocEntries["zhs"]);

            if (changed)
            {
                MegaCrit.Sts2.Core.Localization.LocManager? locManager =
                    MegaCrit.Sts2.Core.Localization.LocManager.Instance;
                if (locManager != null)
                {
                    locManager.SetLanguage(locManager.Language);
                }
            }
        }
        catch (Exception ex)
        {
            LogWarn("Failed to prepare localization overrides: " + ex);
        }
    }

    private static bool UpsertLocalizationTable(
        string language,
        string tableName,
        IReadOnlyDictionary<string, string> requiredEntries)
    {
        string languageDir = Path.Combine(OS.GetUserDataDir(), "localization_override", language);
        Directory.CreateDirectory(languageDir);

        string tablePath = Path.Combine(languageDir, $"{tableName}.json");
        Dictionary<string, string> currentEntries = LoadLocalizationTable(tablePath);
        bool changed = false;

        foreach ((string key, string value) in requiredEntries)
        {
            if (currentEntries.TryGetValue(key, out string? currentValue) && currentValue == value)
            {
                continue;
            }

            currentEntries[key] = value;
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        File.WriteAllText(tablePath, JsonSerializer.Serialize(currentEntries, JsonOptions) + System.Environment.NewLine);
        LogInfo("Updated localization override: " + tablePath);
        return true;
    }

    private static Dictionary<string, string> LoadLocalizationTable(string tablePath)
    {
        if (!File.Exists(tablePath))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(tablePath)) ??
                   new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public static IEnumerable<Creature> GetEnemyCreatures(Creature creature)
    {
        CombatState? combatState = creature.CombatState;
        return combatState == null
            ? Enumerable.Empty<Creature>()
            : combatState.Creatures.Where(other => other.IsMonster && other.IsAlive);
    }

    public static IEnumerable<Creature> GetLivingEnemiesWithIntent(Creature creature)
    {
        return GetEnemyCreatures(creature).Where(enemy => enemy.Monster?.NextMove?.Intents.Count > 0);
    }

    public static bool DoesCurrentMoveContainAttackIntent(Creature enemy)
    {
        return enemy.Monster?.NextMove?.Intents.Any(intent => intent is AttackIntent) ?? false;
    }

    public static bool AllLivingEnemiesWithIntentAttack(Creature? creature)
    {
        if (creature == null)
        {
            return false;
        }

        List<Creature> enemiesWithIntent = GetLivingEnemiesWithIntent(creature).ToList();
        return enemiesWithIntent.Count > 0 && enemiesWithIntent.All(DoesCurrentMoveContainAttackIntent);
    }

    public static T? GetPower<T>(Creature creature)
        where T : PowerModel
    {
        return creature.Powers.OfType<T>().FirstOrDefault();
    }

    public static int GetPressure(Creature creature)
    {
        return GetPower<PressurePower>(creature)?.Amount ?? 0;
    }

    public static async Task ApplyPressure(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<PressurePower>(target, amount, applier, cardSource, false);
    }

    public static async Task<bool> TryConsumePressure(
        Creature target,
        int amount,
        Creature? applier,
        CardModel? cardSource)
    {
        PressurePower? pressure = GetPower<PressurePower>(target);
        if (pressure == null || pressure.Amount < amount)
        {
            return false;
        }

        await PowerCmd.ModifyAmount(pressure, -amount, applier, cardSource, true);
        return true;
    }

    public static async Task GiveGeneratedCardToPlayer<T>(Player recipient)
        where T : CardModel
    {
        if (recipient.Creature?.CombatState == null)
        {
            return;
        }

        CardModel card = recipient.Creature.CombatState.CreateCard<T>(recipient);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true, CardPilePosition.Random);
    }

    public static async Task<CardModel?> AddSpecificCardToCombatPile<T>(
        Player recipient,
        PileType pileType,
        AbstractModel source,
        bool upgraded = false)
        where T : CardModel
    {
        if (recipient.Creature?.CombatState == null)
        {
            return null;
        }

        CardModel card = recipient.Creature.CombatState.CreateCard<T>(recipient);
        if (upgraded)
        {
            CardCmd.Upgrade(card, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.None);
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, pileType, true, CardPilePosition.Random);
        return card;
    }

    public static async Task ShuffleDiscardPileIntoDrawPile(PlayerChoiceContext choiceContext, Player player, AbstractModel source)
    {
        if (player.Creature?.CombatState == null)
        {
            return;
        }

        PlayerCombatState? combatState = player.PlayerCombatState;
        if (combatState == null)
        {
            return;
        }

        List<CardModel> discardCards = combatState.DiscardPile.Cards.ToList();
        if (discardCards.Count == 0)
        {
            return;
        }

        await CardPileCmd.Add(discardCards, PileType.Draw, CardPilePosition.Random, source, false);
        await CardPileCmd.Shuffle(choiceContext, player);
    }

    public static T? AddSpecificCardToDeck<T>(Player recipient, bool silent = false)
        where T : CardModel
    {
        if (recipient.Deck == null)
        {
            return null;
        }

        T card = (T)ModelDb.Card<T>().ToMutable();
        card.Owner = recipient;
        recipient.Deck.AddInternal(card, recipient.Deck.Cards.Count, silent);
        SaveManager.Instance?.Progress?.MarkCardAsSeen(card.Id);
        return card;
    }

    public static CardModel? AddSpecificCardToDeck(Player recipient, CardModel canonical, bool silent = false)
    {
        if (recipient.Deck == null)
        {
            return null;
        }

        CardModel card = canonical.ToMutable();
        card.Owner = recipient;
        recipient.Deck.AddInternal(card, recipient.Deck.Cards.Count, silent);
        SaveManager.Instance?.Progress?.MarkCardAsSeen(card.Id);
        return card;
    }

    public static async Task ApplyEventHpLoss(Creature target, decimal amount)
    {
        if (amount <= 0 || !target.IsAlive)
        {
            return;
        }

        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount, DamageProps.nonCardHpLoss, null, null);
    }

    public static bool IsSongCard(CardModel? card)
    {
        return card != null && SongCardEntries.Contains(card.Id.Entry);
    }

    public static bool IsStarterDeckCard(CardModel? card)
    {
        return card != null && StarterDeckEntries.Contains(card.Id.Entry);
    }

    public static bool HasVisitedEvent(RunState runState, EventModel eventModel)
    {
        HashSet<ModelId>? visitedEventIds = VisitedEventIdsRef(runState);
        if (visitedEventIds == null)
        {
            return false;
        }

        ModelId modelId = eventModel.CanonicalInstance?.Id ?? eventModel.Id;
        return visitedEventIds.Contains(modelId);
    }

    public static void MarkEventVisited(RunState runState, EventModel eventModel)
    {
        HashSet<ModelId>? visitedEventIds = VisitedEventIdsRef(runState);
        if (visitedEventIds == null)
        {
            return;
        }

        ModelId modelId = eventModel.CanonicalInstance?.Id ?? eventModel.Id;
        visitedEventIds.Add(modelId);
    }

    public static bool HasCardLocalization(CardModel? card)
    {
        if (card == null)
        {
            return false;
        }

        string titleKey = card.Id.Entry + ".title";
        string descriptionKey = card.Id.Entry + ".description";
        return CardLocEntries["eng"].ContainsKey(titleKey)
            && CardLocEntries["eng"].ContainsKey(descriptionKey);
    }

    public static IReadOnlyList<CardModel> GetSongPoolCanonicals()
    {
        return ModelDb.CardPool<TogawasakikoCardPool>()
            .AllCards
            .Where(card => IsSongCard(card) && HasCardLocalization(card))
            .ToList();
    }

    public static IReadOnlyList<CardModel> GetColorlessPoolCanonicals()
    {
        return ModelDb.AllCardPools
            .Where(pool => pool.IsColorless)
            .SelectMany(pool => pool.AllCards)
            .Distinct()
            .ToList();
    }

    public static PlayerChoiceContext? CreateHookChoiceContext(
        AbstractModel source,
        CombatState combatState,
        Player? preferredPlayer = null,
        GameActionType gameActionType = GameActionType.Combat)
    {
        ulong? localPlayerId = LocalContext.NetId;
        if (!localPlayerId.HasValue)
        {
            if (combatState.Players.Count > 1)
            {
                LogWarn($"CreateHookChoiceContext could not resolve LocalContext.NetId for source {source.GetType().Name} in multiplayer combat; refusing ambiguous fallback.");
                return null;
            }

            Player? fallbackPlayer = preferredPlayer ?? combatState.Players.FirstOrDefault();
            if (fallbackPlayer == null)
            {
                return null;
            }

            localPlayerId = fallbackPlayer.NetId;
        }

        return new HookPlayerChoiceContext(source, localPlayerId.Value, combatState, gameActionType);
    }

    public static PlayerChoiceContext? CreateBestEffortCombatChoiceContext(AbstractModel source, Player? preferredPlayer)
    {
        CombatState? combatState = preferredPlayer?.Creature?.CombatState;
        if (combatState == null)
        {
            return null;
        }

        PlayerChoiceContext? hookContext = CreateHookChoiceContext(source, combatState, preferredPlayer);
        if (hookContext != null)
        {
            return hookContext;
        }

        if (combatState.Players.Count <= 1)
        {
            return CreateDetachedChoiceContext(source);
        }

        LogWarn($"CreateBestEffortCombatChoiceContext failed for source {source.GetType().Name}; skipping detached fallback in multiplayer.");
        return null;
    }

    public static IReadOnlyList<CardModel> GetPressureGeneratedPoolCanonicals()
    {
        return new CardModel[]
        {
            ModelDb.Card<PersonaDissociation>(),
            ModelDb.Card<SocialWithdrawal>(),
            ModelDb.Card<AllYouThinkAboutIsYourself>(),
            ModelDb.Card<OverworkAnxiety>()
        }
            .Where(HasCardLocalization)
            .ToArray();
    }

    public static IReadOnlyList<PowerModel> GetNegativePowers(Creature creature)
    {
        return creature.Powers
            .Where(power => power.Type == PowerType.Debuff)
            .ToList();
    }

    public static int CountOwnedSymbolKinds(Creature creature)
    {
        int count = 0;
        if (GetPower<SymbolIPower>(creature)?.Amount > 0)
        {
            count++;
        }

        if (GetPower<SymbolIIPower>(creature)?.Amount > 0)
        {
            count++;
        }

        if (GetPower<SymbolIIIPower>(creature)?.Amount > 0)
        {
            count++;
        }

        if (GetPower<SymbolIVPower>(creature)?.Amount > 0)
        {
            count++;
        }

        return count;
    }

    public static async Task RemoveTemporarySymbolPowers(PlayerChoiceContext choiceContext, Creature creature)
    {
        foreach (PowerModel? power in new PowerModel?[]
                 {
                     GetPower<SymbolIPower>(creature),
                     GetPower<SymbolIIPower>(creature),
                     GetPower<SymbolIIIPower>(creature)
                 })
        {
            if (power != null)
            {
                await PowerCmd.Remove(power);
            }
        }
    }

    public static CardModel? CreateRandomCardFromCanonicalPool(Player recipient, IReadOnlyList<CardModel> canonicals)
    {
        CombatState? combatState = recipient.Creature?.CombatState;
        if (combatState == null || canonicals.Count == 0)
        {
            return null;
        }

        return CardFactory.GetDistinctForCombat(
            recipient,
            canonicals,
            1,
            recipient.RunState.Rng.CombatCardGeneration).FirstOrDefault();
    }

    public static async Task<CardModel?> GiveRandomSongCardToPlayer(Player recipient, bool setCostToZeroThisCombat)
    {
        CardModel? card = CreateRandomCardFromCanonicalPool(recipient, GetSongPoolCanonicals());
        if (card == null)
        {
            return null;
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true, CardPilePosition.Random);
        if (setCostToZeroThisCombat)
        {
            card.EnergyCost.SetThisCombat(0, true);
        }

        return card;
    }

    public static async Task<CardModel?> GiveRandomUpgradedSongCardToPlayer(Player recipient, AbstractModel source)
    {
        CardModel? card = CreateRandomCardFromCanonicalPool(recipient, GetSongPoolCanonicals());
        if (card == null)
        {
            return null;
        }

        CardCmd.Upgrade(card, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true, CardPilePosition.Random);
        return card;
    }

    public static async Task<CardModel?> GiveRandomPressureGeneratedCardToPlayer(Player recipient)
    {
        CardModel? card = CreateRandomCardFromCanonicalPool(recipient, GetPressureGeneratedPoolCanonicals());
        if (card == null)
        {
            return null;
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true, CardPilePosition.Random);
        return card;
    }

    public static async Task<CardModel?> GiveRandomColorlessCardToPlayer(Player recipient)
    {
        CardModel? card = CardFactory.GetDistinctForCombat(
            recipient,
            ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(
                recipient.UnlockState,
                recipient.RunState.CardMultiplayerConstraint),
            1,
            recipient.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card == null)
        {
            return null;
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
        return card;
    }

    public static IEnumerable<Creature> GetOtherEnemyCreatures(Creature creature)
    {
        return GetEnemyCreatures(creature).Where(other => other != creature);
    }

    public static bool DoAllLivingEnemiesHavePressure(Creature owner, int minimumAmount)
    {
        List<Creature> enemies = GetEnemyCreatures(owner).ToList();
        return enemies.Count > 0 && enemies.All(enemy => GetPressure(enemy) >= minimumAmount);
    }

    public static int GetLostHp(Creature creature)
    {
        return Math.Max(0, creature.MaxHp - creature.CurrentHp);
    }

    public static CardModel? GetPreviouslyPlayedCardThisTurn(Player player, CardModel currentCard)
    {
        TogawasakikoCombatWatcherPower? watcher = player.Creature == null
            ? null
            : GetPower<TogawasakikoCombatWatcherPower>(player.Creature);

        CardModel? previousCard = watcher?.LastPlayedCardThisTurn;
        return ReferenceEquals(previousCard, currentCard) ? null : previousCard;
    }

    internal static void ClearLocalCostModifiers(CardModel card)
    {
        if (CardEnergyCostLocalModifiersField?.GetValue(card.EnergyCost) is IList localModifiers && localModifiers.Count > 0)
        {
            localModifiers.Clear();
            card.InvokeEnergyCostChanged();
        }
    }

    public static void ClearPersistedTwoMoonsCostModifiers(Player player)
    {
        foreach (TwoMoonsDeepIntoTheForest card in (player.Deck?.Cards ?? Array.Empty<CardModel>()).OfType<TwoMoonsDeepIntoTheForest>())
        {
            ClearLocalCostModifiers(card);
        }
    }

    public static async Task TryGenerateInferiorityPressureCard(
        Creature target,
        Creature? applier,
        CardModel? cardSource)
    {
        Player? recipient = cardSource?.Owner ?? applier?.Player;
        if (recipient == null)
        {
            return;
        }

        if (await TryConsumePressure(target, 1, applier, cardSource))
        {
            await GiveGeneratedCardToPlayer<OverworkAnxiety>(recipient);
        }
    }

    public static IReadOnlyList<CardModel> GetAllCombatCards(Player player)
    {
        if (player.Creature?.CombatState == null)
        {
            return Array.Empty<CardModel>();
        }

        PlayerCombatState? combatState = player.PlayerCombatState;
        if (combatState == null)
        {
            return Array.Empty<CardModel>();
        }

        return combatState.Hand.Cards
            .Concat(combatState.DrawPile.Cards)
            .Concat(combatState.DiscardPile.Cards)
            .Concat(combatState.ExhaustPile.Cards)
            .Concat(combatState.PlayPile.Cards)
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<CardModel> GetAllPlayerCards(Player player)
    {
        IEnumerable<CardModel> deckCards = player.Deck?.Cards ?? Array.Empty<CardModel>();
        return deckCards
            .Concat(GetAllCombatCards(player))
            .Distinct()
            .ToList();
    }

    public static async Task RemoveStarterStrikeAndDefendFromDeck(Player player)
    {
        if (player.Deck == null)
        {
            return;
        }

        StrikeTogawasakiko? strike = player.Deck.Cards.OfType<StrikeTogawasakiko>().FirstOrDefault();
        if (strike != null)
        {
            await CardPileCmd.RemoveFromDeck(strike, false);
        }

        DefendTogawasakiko? defend = player.Deck.Cards.OfType<DefendTogawasakiko>().FirstOrDefault();
        if (defend != null)
        {
            await CardPileCmd.RemoveFromDeck(defend, false);
        }
    }

    public static async Task TryUpgradeDollMask(Player player)
    {
        DollMask? currentRelic = player.Relics.OfType<DollMask>().FirstOrDefault();
        if (currentRelic == null)
        {
            return;
        }

        await RelicCmd.Replace(currentRelic, ModelDb.Relic<UpgradedDollMask>().ToMutable());
    }

    public static PlayerChoiceContext CreateDetachedChoiceContext(AbstractModel? lastInvolvedModel = null)
    {
        DetachedPlayerChoiceContext context = new();
        if (lastInvolvedModel != null)
        {
            context.PushModel(lastInvolvedModel);
        }

        return context;
    }

    public static Creature? GetRandomAutoplayTarget(CardModel card)
    {
        if (card.Owner?.Creature?.CombatState == null)
        {
            return null;
        }

        return card.TargetType switch
        {
            TargetType.AnyEnemy or TargetType.RandomEnemy => GetRandomEnemy(card.Owner.Creature),
            TargetType.Self or TargetType.AnyPlayer or TargetType.AnyAlly => card.Owner.Creature,
            _ => null
        };
    }

    public static bool TryResolveAutoplayTarget(CardModel card, out Creature? autoplayTarget)
    {
        autoplayTarget = GetRandomAutoplayTarget(card);

        return card.TargetType switch
        {
            TargetType.AnyEnemy or TargetType.RandomEnemy => autoplayTarget != null,
            TargetType.Self or TargetType.AnyPlayer or TargetType.AnyAlly => autoplayTarget != null,
            TargetType.None or TargetType.AllEnemies or TargetType.AllAllies => true,
            _ => autoplayTarget != null
        };
    }

    public static bool TryResolveReplayTarget(CardPlay cardPlay, out Creature? replayTarget)
    {
        replayTarget = null;
        CardModel? card = cardPlay.Card;
        Creature? ownerCreature = card?.Owner?.Creature;
        if (card == null || ownerCreature?.CombatState == null)
        {
            return false;
        }

        switch (card.TargetType)
        {
            case TargetType.AnyEnemy:
            case TargetType.RandomEnemy:
                replayTarget = cardPlay.Target != null && cardPlay.Target.IsAlive
                    ? cardPlay.Target
                    : GetRandomEnemy(ownerCreature);
                return replayTarget != null;
            case TargetType.AllEnemies:
                return GetEnemyCreatures(ownerCreature).Any();
            case TargetType.Self:
            case TargetType.AnyPlayer:
            case TargetType.AnyAlly:
                replayTarget = ownerCreature;
                return true;
            case TargetType.None:
            case TargetType.AllAllies:
                return true;
            default:
                replayTarget = GetRandomAutoplayTarget(card);
                return replayTarget != null || card.TargetType == TargetType.None;
        }
    }

    public static Creature? GetRandomEnemy(Creature ownerCreature)
    {
        CombatState? combatState = ownerCreature.CombatState;
        if (combatState == null)
        {
            return null;
        }

        List<Creature> enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
        {
            return null;
        }

        Creature? randomEnemy = ownerCreature.Player?.RunState?.Rng?.CombatTargets?.NextItem(enemies);
        if (randomEnemy != null)
        {
            return randomEnemy;
        }

        int playerCount = ownerCreature.Player?.RunState?.Players?.Count ?? combatState.Players.Count;
        if (playerCount > 1)
        {
            LogWarn("GetRandomEnemy could not resolve CombatTargets RNG in multiplayer combat; refusing deterministic fallback to first enemy.");
            return null;
        }

        return enemies[0];
    }

    private sealed class DetachedPlayerChoiceContext : PlayerChoiceContext
    {
        public override Task SignalPlayerChoiceBegun(PlayerChoiceOptions options)
        {
            return Task.CompletedTask;
        }

        public override Task SignalPlayerChoiceEnded()
        {
            return Task.CompletedTask;
        }
    }

    public static string GetNormalAttackPlaceholderPortraitPath()
    {
        return GetBasicPortraitPath("slander.png");
    }

    public static string GetNormalSkillPlaceholderPortraitPath()
    {
        return GetBasicPortraitPath("unendurable.png");
    }

    public static string GetBasicPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/basic/{fileName}";
    }

    public static string GetNormalCommonPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/normal/common/{fileName}";
    }

    public static string GetNormalUncommonPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/normal/uncommon/{fileName}";
    }

    public static string GetNormalRarePortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/normal/rare/{fileName}";
    }

    public static string GetEventGrantedPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/event_granted/{fileName}";
    }

    public static string GetRelicGrantedPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/relic_granted/{fileName}";
    }

    public static string GetGeneratedPortraitPath(string fileName)
    {
        return $"res://mod_assets/cards/generated_pressure/{fileName}";
    }

    public static string GetShadowQuestionRoomEventMainPath(string fileName)
    {
        return fileName switch
        {
            "start.png" => "res://images/events/unattended_piano.png",
            "shadow_piano.png" => "res://images/events/unattended_piano_shadow.png",
            _ => $"res://images/events/{fileName}"
        };
    }

    public static string GetShadowQuestionRoomEventMusicPath()
    {
        const string eventPath = "res://audio/music/events/unattended_piano.mp3";
        if (ResourceLoader.Exists(eventPath))
        {
            return eventPath;
        }

        const string trackPath = "res://audio/music/tracks/unattended_piano.mp3";
        return ResourceLoader.Exists(trackPath) ? trackPath : string.Empty;
    }

    public static void TrySetCurrentEventPortrait(string portraitPath)
    {
        if (string.IsNullOrWhiteSpace(portraitPath))
        {
            return;
        }

        CompressedTexture2D? portrait = PreloadManager.Cache.GetCompressedTexture2D(portraitPath)
            ?? ResourceLoader.Load<CompressedTexture2D>(portraitPath);
        if (portrait == null)
        {
            LogWarn("Event portrait resource missing: " + portraitPath);
            return;
        }

        NEventRoom.Instance?.SetPortrait(portrait);
    }

    public static void TryPlayShadowQuestionRoomEventMusic()
    {
        string musicPath = GetShadowQuestionRoomEventMusicPath();
        if (string.IsNullOrWhiteSpace(musicPath))
        {
            LogWarn("Shadow question-room event music asset is not present yet; skipping playback.");
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            LogWarn("Shadow question-room event music skipped because SceneTree root is unavailable.");
            return;
        }

        AudioStream? stream = _shadowEventMusicStream;
        if (stream == null || stream.ResourcePath != musicPath)
        {
            stream = GD.Load<AudioStream>(musicPath);
            _shadowEventMusicStream = stream;
        }

        if (stream == null)
        {
            LogWarn("Failed to load shadow question-room event music: " + musicPath);
            return;
        }

        if (!GodotObject.IsInstanceValid(_shadowEventMusicPlayer))
        {
            _shadowEventMusicPlayer = new AudioStreamPlayer
            {
                Name = "TogawasakikoShadowQuestionRoomMusic",
                Bus = "Master"
            };
            tree.Root.AddChild(_shadowEventMusicPlayer);
        }

        NRunMusicController.Instance?.StopMusic();
        _shadowEventMusicPlayer.Stop();
        _shadowEventMusicPlayer.Stream = stream;
        _shadowEventMusicPlayer.Play();
    }

    public static void StopShadowQuestionRoomEventMusic()
    {
        if (GodotObject.IsInstanceValid(_shadowEventMusicPlayer))
        {
            _shadowEventMusicPlayer.Stop();
        }

        NRunMusicController.Instance?.UpdateMusic();
    }

    internal sealed partial class CharacterSelectInjector : Node
    {
        private readonly HashSet<ulong> _patchedScreens = new();
        private readonly HashSet<ulong> _foundScreenLoggedScreens = new();
        private readonly HashSet<ulong> _missingContainerLoggedScreens = new();

        public override void _EnterTree()
        {
            SetProcess(true);
            LogInfo("CharacterSelectInjector entered tree.");
        }

        public override void _Process(double delta)
        {
            if (GetTree().Root == null)
            {
                return;
            }

            if (FindCharacterSelectScreen(GetTree().Root) is not { } screen)
            {
                return;
            }

            ulong screenId = screen.GetInstanceId();
            if (_foundScreenLoggedScreens.Add(screenId))
            {
                LogInfo("Found character select screen: " + screen.GetPath());
            }

            if (_patchedScreens.Contains(screenId))
            {
                return;
            }

            if (!TryInjectButton(screen, screenId))
            {
                return;
            }

            _patchedScreens.Add(screenId);
        }

        private static NCharacterSelectScreen? FindCharacterSelectScreen(Node root)
        {
            if (root.GetNodeOrNull<NCharacterSelectScreen>("Submenus/CharacterSelectScreen") is { } directMatch)
            {
                return directMatch;
            }

            if (root is NCharacterSelectScreen screen)
            {
                return screen;
            }

            foreach (Node child in root.GetChildren())
            {
                if (FindCharacterSelectScreen(child) is { } match)
                {
                    return match;
                }
            }

            return null;
        }

        private bool TryInjectButton(NCharacterSelectScreen screen, ulong screenId)
        {
            Control? buttonContainer = screen.GetNodeOrNull<Control>("CharSelectButtons/ButtonContainer")
                ?? screen.GetNodeOrNull<Control>("LeftContainer/CharSelectButtons/ButtonContainer");
            if (buttonContainer == null)
            {
                if (_missingContainerLoggedScreens.Add(screenId))
                {
                    LogWarn("Character select button container not found on screen: " + screen.GetPath());
                }

                return false;
            }

            string buttonName = $"{ModelDb.Character<Togawasakiko>().Id.Entry}_button";
            if (buttonContainer.GetChildren().OfType<NCharacterSelectButton>().Any(button => button.Name == buttonName))
            {
                LogInfo("Character-select button already present.");
                return true;
            }

            PackedScene? buttonScene = ResourceLoader.Load<PackedScene>("res://scenes/screens/char_select/char_select_button.tscn");
            if (buttonScene == null)
            {
                LogError("Failed to load character select button scene.");
                return false;
            }

            NCharacterSelectButton button = buttonScene.Instantiate<NCharacterSelectButton>();
            button.Name = buttonName;
            buttonContainer.AddChild(button);
            button.Init(ModelDb.Character<Togawasakiko>(), screen);
            button.DebugUnlock();
            button.UnlockIfPossible();
            button.Reset();
            ForceUnlockButton(button, ModelDb.Character<Togawasakiko>());
            LogInfo("Injected runtime character-select button.");
            return true;
        }

        private static void ForceUnlockButton(NCharacterSelectButton button, Togawasakiko character)
        {
            Type buttonType = button.GetType();
            buttonType.GetField("_isLocked", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(button, false);

            if (buttonType.GetField("_icon", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(button) is TextureRect icon)
            {
                icon.Texture = character.CharacterSelectIcon;
            }

            if (buttonType.GetField("_lock", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(button) is CanvasItem lockNode)
            {
                lockNode.Visible = false;
            }
        }
    }
}
