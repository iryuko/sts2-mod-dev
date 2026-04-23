using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
internal static class CharacterSelectAudioPatches
{
    private const string TogawasakikoSelectAudioPath = "res://audio/sfx/character_select/togawasakiko_select.ogg";

    private static AudioStream? _cachedSelectAudio;
    private static AudioStreamPlayer? _player;

    [HarmonyPostfix]
    private static void PlayTogawasakikoSelectAudio(CharacterModel characterModel)
    {
        if (characterModel is not Togawasakiko)
        {
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            ModSupport.LogWarn("Character select audio skipped because SceneTree root is unavailable.");
            return;
        }

        AudioStream? stream = _cachedSelectAudio ??= GD.Load<AudioStream>(TogawasakikoSelectAudioPath);
        if (stream == null)
        {
            ModSupport.LogWarn("Failed to load character select audio: " + TogawasakikoSelectAudioPath);
            return;
        }

        if (!GodotObject.IsInstanceValid(_player))
        {
            _player = new AudioStreamPlayer
            {
                Name = "TogawasakikoCharacterSelectAudio",
                Bus = "Master"
            };
            tree.Root.AddChild(_player);
        }

        _player.Stop();
        _player.Stream = stream;
        _player.Play();
    }
}
