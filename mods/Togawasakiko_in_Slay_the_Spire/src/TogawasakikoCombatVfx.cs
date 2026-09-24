using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Togawasakiko_in_Slay_the_Spire;

internal static class TogawasakikoCombatVfx
{
    internal const string ThornScenePath =
        "res://scenes/vfx/togawasakiko/thorn_restraint.tscn";

    // The runtime thorn and the current pointing animation both contact at 0.31s.
    private const float StandardContactTime = 0.31f;
    private static bool _missingRuntimeNodeLogged;

    internal static AttackCommand WithSingleTargetThorns(
        this AttackCommand command,
        Creature target,
        float standardAttackerAnimationDelay)
    {
        return command.WithTargetThorns(
            new[] { target },
            standardAttackerAnimationDelay);
    }

    internal static AttackCommand WithTargetThorns(
        this AttackCommand command,
        IEnumerable<Creature> targets,
        float standardAttackerAnimationDelay)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(targets);
        Creature[] targetSnapshot = targets
            .Where(target => target != null)
            .Distinct()
            .ToArray();

        float fastAttackerAnimationDelay = MathF.Min(
            standardAttackerAnimationDelay * 0.5f,
            0.25f);
        float fastWaitBeforeHit = MathF.Max(
            0f,
            (StandardContactTime * 0.5f) - fastAttackerAnimationDelay);
        float standardWaitBeforeHit = MathF.Max(
            0f,
            StandardContactTime - standardAttackerAnimationDelay);

        return command
            .WithAttackerFx(() => CreateThornNodeGroup(targetSnapshot))
            .WithWaitBeforeHit(fastWaitBeforeHit, standardWaitBeforeHit);
    }

    private static Node2D CreateThornNodeGroup(IEnumerable<Creature> targets)
    {
        PackedScene? thornScene = GetThornScene();
        if (thornScene == null)
        {
            return CreateSelfFreeingNoop(
                $"Cannot load thorn VFX scene: {ThornScenePath}");
        }

        Node2D? rootThorn = null;

        foreach (Creature target in targets.Where(target => target.IsAlive))
        {
            var targetNode = target.GetCreatureNode();
            if (targetNode == null)
            {
                continue;
            }

            Vector2 groundAnchor = targetNode.GetBottomOfHitbox();
            Node2D thornNode = thornScene.Instantiate<Node2D>(
                PackedScene.GenEditState.Disabled);
            thornNode.TreeEntered += () => thornNode.GlobalPosition = groundAnchor;
            if (rootThorn == null)
            {
                rootThorn = thornNode;
            }
            else
            {
                rootThorn.AddChild(thornNode);
            }
        }

        return rootThorn ?? CreateSelfFreeingNoop(
            "Cannot spawn thorn VFX because no target has a runtime creature node.");
    }

    private static PackedScene? GetThornScene()
    {
        return PreloadManager.Cache.GetScene(ThornScenePath);
    }

    private static Node2D CreateSelfFreeingNoop(string warning)
    {
        if (!_missingRuntimeNodeLogged)
        {
            _missingRuntimeNodeLogged = true;
            ModSupport.LogWarn(warning);
        }

        var noop = new Node2D
        {
            Name = "MissingTogawasakikoThornVfx"
        };
        noop.TreeEntered += noop.QueueFree;
        return noop;
    }
}
