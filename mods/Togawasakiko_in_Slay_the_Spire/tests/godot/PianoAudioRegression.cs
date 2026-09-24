using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using Togawasakiko_in_Slay_the_Spire;

internal static class PianoAudioRegression
{
    private const string AudioPath = "res://piano_fixture.tres";
    private const string PlayerName = "TogawasakikoShadowQuestionRoomMusic";
    private static NRunMusicController? _music;
    private static int _resumes;

    private static bool MusicPath(ref string __result) { __result = AudioPath; return false; }
    private static bool Portrait() => false;
    private static bool MusicController(ref NRunMusicController? __result) { __result = _music; return false; }
    private static bool StopRunMusic() => false;
    private static bool ResumeRunMusic() { _resumes++; return false; }

    internal static async Task Run(Node host)
    {
        TestMode.TurnOnInternal();
        typeof(ModManager).GetProperty(nameof(ModManager.State))!.SetValue(null, ModManagerState.Skipped);
        ModelDb.Init();
        Assembly assembly = typeof(TogawasakikoMod).Assembly;
        foreach (Type type in assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AbstractModel))))
            ModelDb.Inject(type);
        TestLocalization.Load("eng");

        Type support = assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
        var harmony = new Harmony("sakiko.native-piano-event-regression");
        harmony.Patch(AccessTools.Method(support, "GetShadowQuestionRoomEventMusicPath"),
            prefix: new HarmonyMethod(typeof(PianoAudioRegression), nameof(MusicPath)));
        harmony.Patch(AccessTools.Method(support, "TrySetCurrentEventPortrait"),
            prefix: new HarmonyMethod(typeof(PianoAudioRegression), nameof(Portrait)));
        _music = new NRunMusicController();
        harmony.Patch(AccessTools.PropertyGetter(typeof(NRunMusicController), nameof(NRunMusicController.Instance)),
            prefix: new HarmonyMethod(typeof(PianoAudioRegression), nameof(MusicController)));
        harmony.Patch(AccessTools.Method(typeof(NRunMusicController), nameof(NRunMusicController.StopMusic)),
            prefix: new HarmonyMethod(typeof(PianoAudioRegression), nameof(StopRunMusic)));
        harmony.Patch(AccessTools.Method(typeof(NRunMusicController), nameof(NRunMusicController.UpdateMusic)),
            prefix: new HarmonyMethod(typeof(PianoAudioRegression), nameof(ResumeRunMusic)));
        ulong? previousIdentity = LocalContext.NetId;
        var nodes = new List<Control>();
        try
        {
            Type character = assembly.GetType("Togawasakiko_in_Slay_the_Spire.Togawasakiko", true)!;
            Player player = Player.CreateForNewRun(ModelDb.GetById<CharacterModel>(ModelDb.GetId(character)), UnlockState.all, 1);
            RunState.CreateForTest([player], seed: "PIANOLIFECYCLE");
            LocalContext.NetId = player.NetId;

            for (int cycle = 0; cycle < 2; cycle++)
            {
                AudioStream stream = ResourceLoader.Load<AudioStream>(AudioPath);
                PreloadManager.Cache.SetAsset(AudioPath, stream);
                (EventModel model, Control node) = await Begin(host, player, nodes);
                var audio = host.GetTree().Root.FindChildren(PlayerName, "AudioStreamPlayer", true, false)
                    .OfType<AudioStreamPlayer>().Single();
                if (!audio.Playing || !ReferenceEquals(stream, audio.Stream)) throw new Exception("Piano did not start its fresh stream.");
                int resumes = _resumes;
                if (cycle == 1)
                {
                    await model.CurrentOptions.Single(o => o.TextKey == "PLAY_1_STOP").Chosen();
                    model.EnsureCleanup();
                    if (_resumes != resumes + 1) throw new Exception("Normal event finish must restore run music exactly once.");
                }
                else
                {
                    // Save-and-quit removes the scene without calling OnEventFinished.
                    host.RemoveChild(node);
                    if (audio.Playing) throw new Exception("Piano playback survived event node removal.");
                    if (_resumes != resumes) throw new Exception("Scene removal restarted run music.");
                }
                if (audio.GetParent() != node) throw new Exception("Piano player escaped the native event node and is owned by the scene-tree root.");
                node.Free();
                await Frames(host);
                if (GodotObject.IsInstanceValid(audio)) throw new Exception("Event disposal retained its audio player.");
                await host.ToSignal(host.GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
                PreloadManager.Cache.UnloadAssets([AudioPath]);
                await Frames(host);
                if (GodotObject.IsInstanceValid(stream)) throw new Exception("Cache did not dispose the old piano stream.");
                GD.Print($"PASS piano native event ownership and unload/reload cycle {cycle + 1}");
            }

            PreloadManager.Cache.SetAsset(AudioPath, ResourceLoader.Load<AudioStream>(AudioPath));
            (EventModel oldEvent, Control oldNode) = await Begin(host, player, nodes);
            oldNode.Free();
            await Frames(host);
            (EventModel nextEvent, Control nextNode) = await Begin(host, player, nodes);
            AudioStreamPlayer nextAudio = nextNode.GetNode<AudioStreamPlayer>(PlayerName);
            int beforeOldCleanup = _resumes;
            oldEvent.EnsureCleanup();
            if (!nextAudio.Playing || _resumes != beforeOldCleanup)
                throw new Exception("A stale event cleanup changed the new event's music.");
            nextEvent.EnsureCleanup();
            if (_resumes != beforeOldCleanup + 1) throw new Exception("The new event did not clean up its own music.");
            nextNode.Free();
            GD.Print("PASS stale piano cleanup cannot stop a newer event");
        }
        finally
        {
            foreach (Control node in nodes.Where(GodotObject.IsInstanceValid)) node.Free();
            // Also clean the legacy root-level player when testing unfixed production.
            foreach (AudioStreamPlayer audio in host.GetTree().Root.FindChildren(PlayerName, "AudioStreamPlayer", true, false).OfType<AudioStreamPlayer>())
            {
                audio.Stop();
                audio.Stream = null;
                audio.Free();
            }
            await host.ToSignal(host.GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
            PreloadManager.Cache.UnloadAssets([AudioPath]);
            await Frames(host);
            _music.Free();
            _music = null;
            harmony.UnpatchAll(harmony.Id);
            LocalContext.NetId = previousIdentity;
        }
    }

    private static async Task<(EventModel, Control)> Begin(Node host, Player player, List<Control> nodes)
    {
        Type eventType = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.UnattendedPiano", true)!;
        EventModel model = ModelDb.GetById<EventModel>(ModelDb.GetId(eventType)).ToMutable();
        await model.BeginEvent(player, false);
        var node = new Control { Name = "PianoEventFixture" };
        nodes.Add(node);
        host.AddChild(node);
        model.SetNode(node);
        await model.CurrentOptions.Single(option => option.TextKey == "PLAY").Chosen();
        return (model, node);
    }

    private static async Task Frames(Node host)
    {
        await host.ToSignal(host.GetTree(), SceneTree.SignalName.ProcessFrame);
        await host.ToSignal(host.GetTree(), SceneTree.SignalName.ProcessFrame);
    }
}
