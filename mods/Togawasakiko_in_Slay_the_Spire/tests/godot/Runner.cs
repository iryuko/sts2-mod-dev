using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using Togawasakiko_in_Slay_the_Spire;

public partial class Runner : Node
{
    private const string ScenePath = "res://scenes/vfx/togawasakiko/thorn_restraint.tscn";

    public override async void _Ready()
    {
        try
        {
            Type type = typeof(TogawasakikoMod).Assembly.GetType(
                "Togawasakiko_in_Slay_the_Spire.TogawasakikoCombatVfx", true)!;
            MethodInfo lookup = type.GetMethod("GetThornScene", BindingFlags.NonPublic | BindingFlags.Static)!;
            PackedScene? previous = null;

            // Use the real native cache unload, including its deferred Dispose.
            // A tiny scene isolates resource ownership from combat and Spine.
            for (int cycle = 0; cycle < 3; cycle++)
            {
                PackedScene current = ResourceLoader.Load<PackedScene>(ScenePath);
                PreloadManager.Cache.SetAsset(ScenePath, current);
                var actual = (PackedScene)lookup.Invoke(null, null)!;
                if (!GodotObject.IsInstanceValid(actual) || !ReferenceEquals(current, actual))
                    throw new Exception($"Cycle {cycle}: thorn lookup retained the disposed scene from the previous run.");
                if (ReferenceEquals(previous, actual))
                    throw new Exception($"Cycle {cycle}: unloaded scene was reused.");
                Node2D node = actual.Instantiate<Node2D>();
                node.Free();

                PreloadManager.Cache.UnloadAssets(new[] { ScenePath });
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (GodotObject.IsInstanceValid(current))
                    throw new Exception("The native cache did not dispose its unloaded scene.");
                previous = current;
                GD.Print($"PASS thorn VFX native unload/reload cycle {cycle + 1}");
            }

            await PianoAudioRegression.Run(this);
            await JukeboxVolumeRegression.Run(this);
            GD.Print("RESULT native resource and event regression passed");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GD.PushError("FAIL resource lifecycle: " + exception);
            GetTree().Quit(1);
        }
    }

}
