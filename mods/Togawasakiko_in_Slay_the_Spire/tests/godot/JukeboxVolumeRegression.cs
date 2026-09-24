using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Test;
using MegaCrit.Sts2.Core.TestSupport;
using Togawasakiko_in_Slay_the_Spire;

internal static class JukeboxVolumeRegression
{
    private const string AudioPath = "res://piano_fixture.tres";
    private static NAudioManager? _audio;

    private static bool AudioInstance(ref NAudioManager? __result)
    {
        __result = _audio;
        return false;
    }

    internal static async Task Run(Node host)
    {
        Assembly assembly = typeof(TogawasakikoMod).Assembly;
        Type overlayType = assembly.GetType("Togawasakiko_in_Slay_the_Spire.JukeboxOverlay", true)!;
        Type injectorType = assembly.GetType("Togawasakiko_in_Slay_the_Spire.JukeboxRunInjector", true)!;
        Type trackType = assembly.GetType("Togawasakiko_in_Slay_the_Spire.JukeboxTrackInfo", true)!;
        object track = Activator.CreateInstance(trackType, "fixture", "Fixture", AudioPath)!;
        MethodInfo play = AccessTools.Method(overlayType, "PlayCustomTrack");
        MethodInfo select = AccessTools.Method(overlayType, "OnTrackSelected");
        MethodInfo stop = AccessTools.Method(overlayType, "StopSharedPlaybackAndRestoreRunMusic");
        MethodInfo runExit = AccessTools.Method(injectorType, "StopPlaybackForRunExit");
        MethodInfo sliderChanged = AccessTools.Method(typeof(NBgmVolumeSlider), "OnValueChanged");
        FieldInfo sharedPlayer = AccessTools.Field(overlayType, "_sharedPlayer");
        FieldInfo streamCache = AccessTools.Field(overlayType, "_streamCache");

        // Only the external FMOD endpoint is replaced. The native slider, settings,
        // NAudioManager volume conversion and production Jukebox calls remain real.
        using var proxyScript = new GDScript
        {
            SourceCode = """
                extends Node
                var bgm_volume: float = -1.0
                var bgm_writes: int = 0
                var master_volume: float = -1.0
                var sfx_volume: float = -1.0
                var ambience_volume: float = -1.0
                func set_bgm_volume(value: float) -> void:
                    bgm_volume = value
                    bgm_writes += 1
                func set_master_volume(value: float) -> void:
                    master_volume = value
                func set_sfx_volume(value: float) -> void:
                    sfx_volume = value
                func set_ambience_volume(value: float) -> void:
                    ambience_volume = value
                """
        };
        Equal(Error.Ok, proxyScript.Reload());
        using var proxy = new Node();
        proxy.SetScript(proxyScript);
        using var audio = new NAudioManager();
        AccessTools.Field(typeof(NAudioManager), "_audioNode").SetValue(audio, proxy);
        using var slider = new NBgmVolumeSlider();
        _audio = audio;

        FieldInfo mockSave = AccessTools.Field(typeof(SaveManager), "_mockInstance");
        object? previousSave = mockSave.GetValue(null);
        bool previousTestMode = TestMode.IsOn;
        var save = new SaveManager(new MockGodotFileIo("user://jukebox-volume-regression"));
        save.InitSettingsDataForTest();
        SaveManager.MockInstanceForTesting(save);
        TestMode.IsOn = false;

        int addedBus = -1;
        if (AudioServer.GetBusIndex("Bgm") < 0 && AudioServer.GetBusIndex("Music") < 0)
        {
            addedBus = AudioServer.BusCount;
            AudioServer.AddBus();
            AudioServer.SetBusName(addedBus, "Bgm");
        }

        var harmony = new Harmony("sakiko.jukebox-volume-regression");
        var failures = new List<Exception>();
        var fixtureStreams = new HashSet<AudioStream>();
        int passed = 0;
        try
        {
            harmony.Patch(AccessTools.PropertyGetter(typeof(NAudioManager), nameof(NAudioManager.Instance)),
                prefix: new HarmonyMethod(typeof(JukeboxVolumeRegression), nameof(AudioInstance)));
            // Absent on the RED baseline; load the actual production patch once implemented.
            Type? volumePatch = assembly.GetType("Togawasakiko_in_Slay_the_Spire.JukeboxBgmVolumePatch");
            if (volumePatch != null) harmony.CreateClassProcessor(volumePatch).Patch();

            void ChangeSlider(double percent) => sliderChanged.Invoke(slider, [percent]);
            double EffectiveBgm() => proxy.Get("bgm_volume").AsDouble();
            int BgmWrites() => proxy.Get("bgm_writes").AsInt32();
            AudioStreamPlayer Player() => (AudioStreamPlayer)sharedPlayer.GetValue(null)!;
            void Start(Control overlay)
            {
                play.Invoke(overlay, [track]);
                Equal(true, Player().Playing);
                Equal("Master", Player().Bus.ToString());
                Near(0.6, save.SettingsSave.VolumeBgm);
                Near(0.0, EffectiveBgm());
            }

            void Check(string name, Action<Control> body)
            {
                stop.Invoke(null, null);
                ChangeSlider(60);
                using var overlay = (Control)Activator.CreateInstance(overlayType, nonPublic: true)!;
                try
                {
                    body(overlay);
                    passed++;
                    GD.Print("PASS " + name);
                }
                catch (Exception exception)
                {
                    failures.Add(new Exception(name, exception));
                    GD.Print("FAIL " + name + ": " + exception);
                }
                finally
                {
                    try
                    {
                        stop.Invoke(null, null);
                    }
                    finally
                    {
                        var cache = (Dictionary<string, AudioStream>)streamCache.GetValue(overlay)!;
                        if (cache.Remove(AudioPath, out AudioStream? stream)) fixtureStreams.Add(stream);
                        if (GodotObject.IsInstanceValid(overlay)) overlay.Free();
                    }
                }
            }

            Check("Jukebox Off preserves the native BGM slider and squared-volume mapping", _ =>
            {
                int writes = BgmWrites();
                ChangeSlider(30);
                Near(0.3, save.SettingsSave.VolumeBgm);
                Near(0.09, EffectiveBgm());
                Equal(writes + 1, BgmWrites());
            });

            Check("Jukebox keeps BGM muted immediately across native slider changes without altering preferences", overlay =>
            {
                Start(overlay);
                foreach (double percent in new[] { 25.0, 0.0, 100.0, 72.0 })
                {
                    int writes = BgmWrites();
                    ChangeSlider(percent);
                    Near(percent / 100.0, save.SettingsSave.VolumeBgm);
                    Near(0.0, EffectiveBgm());
                    Equal(writes + 1, BgmWrites());
                    Equal(true, Player().Playing);
                }
            });

            Check("Jukebox Off restores the latest BGM preference and releases the mute", overlay =>
            {
                Start(overlay);
                ChangeSlider(35);
                select.Invoke(overlay, [0L]);
                Near(0.35, save.SettingsSave.VolumeBgm);
                Near(0.1225, EffectiveBgm());
                Equal(false, Player().Playing);
                Equal(null, Player().Stream);
                ChangeSlider(80);
                Near(0.64, EffectiveBgm());
                int writes = BgmWrites();
                select.Invoke(overlay, [0L]);
                Equal(writes, BgmWrites());
            });

            Check("Jukebox run exit restores the latest preference across repeated playback cycles", overlay =>
            {
                for (int cycle = 0; cycle < 2; cycle++)
                {
                    ChangeSlider(60);
                    Start(overlay);
                    ChangeSlider(72);
                    runExit.Invoke(null, null);
                    Near(0.72, save.SettingsSave.VolumeBgm);
                    Near(0.5184, EffectiveBgm());
                    Equal(false, Player().Playing);
                    Equal(null, Player().Stream);
                    int writes = BgmWrites();
                    runExit.Invoke(null, null);
                    Equal(writes, BgmWrites());
                    ChangeSlider(20);
                    Near(0.04, EffectiveBgm());
                }
            });

            Check("Jukebox BGM mute leaves native master, SFX and ambience volumes independent", overlay =>
            {
                Start(overlay);
                audio.SetMasterVol(0.7f);
                audio.SetSfxVol(0.3f);
                audio.SetAmbienceVol(0.4f);
                Near(0.49, proxy.Get("master_volume").AsDouble());
                Near(0.09, proxy.Get("sfx_volume").AsDouble());
                Near(0.16, proxy.Get("ambience_volume").AsDouble());
                Near(0.0, EffectiveBgm());
                Near(0.6, save.SettingsSave.VolumeBgm);
            });
        }
        finally
        {
            stop.Invoke(null, null);
            if (sharedPlayer.GetValue(null) is AudioStreamPlayer player && GodotObject.IsInstanceValid(player))
                player.Free();
            sharedPlayer.SetValue(null, null);
            harmony.UnpatchAll(harmony.Id);
            _audio = null;
            if (GodotObject.IsInstanceValid(slider)) slider.Free();
            if (GodotObject.IsInstanceValid(audio)) audio.Free();
            if (GodotObject.IsInstanceValid(proxy)) proxy.Free();
            mockSave.SetValue(null, previousSave);
            TestMode.IsOn = previousTestMode;
            if (addedBus >= 0) AudioServer.RemoveBus(addedBus);
            // Stop queues native playback cleanup; wait for ownership, not a fixed delay.
            long deadline = System.Environment.TickCount64 + 2000;
            foreach (AudioStream stream in fixtureStreams.Where(GodotObject.IsInstanceValid))
            {
                while (stream.GetReferenceCount() > 1)
                {
                    if (System.Environment.TickCount64 >= deadline)
                        throw new Exception($"Stopped fixture audio retained {stream.GetReferenceCount()} native references.");
                    await host.ToSignal(host.GetTree(), SceneTree.SignalName.ProcessFrame);
                }
                stream.Dispose();
            }
            fixtureStreams.Clear();
        }

        GD.Print($"RESULT Jukebox volume: {passed} passed, {failures.Count} failed");
        if (failures.Count > 0) throw new AggregateException(failures);
    }

    private static void Near(double expected, double actual)
    {
        if (Math.Abs(expected - actual) > 0.00001)
            throw new Exception($"expected {expected}, got {actual}");
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }
}
