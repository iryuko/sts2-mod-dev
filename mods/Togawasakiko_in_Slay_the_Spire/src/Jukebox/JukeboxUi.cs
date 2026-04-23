using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class JukeboxTrackInfo
{
    public JukeboxTrackInfo(string id, string displayName, string resourcePath)
    {
        Id = id;
        DisplayName = displayName;
        ResourcePath = resourcePath;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string ResourcePath { get; }
}

internal sealed partial class JukeboxRunInjector : Node
{
    private static readonly HashSet<ulong> PatchedGlobalUis = new();
    private static JukeboxOverlay? _currentOverlay;
    private static NRun? _latestRun;

    public void ResetCurrentOverlayToOff()
    {
        ResetCurrentOverlayToOffStatic();
    }

    public static void RegisterRun(NRun run)
    {
        _latestRun = run;
        ModSupport.LogInfo("Registered current run for jukebox overlay v7: " + run.GetPath());
    }

    public static void ResetCurrentOverlayToOffStatic()
    {
        if (_currentOverlay == null || !GodotObject.IsInstanceValid(_currentOverlay))
        {
            return;
        }

        _currentOverlay.ResetToOffForCombat();
    }

    public static bool TryInjectIntoGlobalUi(Control globalUi)
    {
        if (!ShouldDisplayForCurrentRun())
        {
            return false;
        }

        ulong globalUiId = globalUi.GetInstanceId();
        if (PatchedGlobalUis.Contains(globalUiId))
        {
            return true;
        }

        if (globalUi.GetNodeOrNull<JukeboxOverlay>("TogawasakikoJukeboxOverlay") is { } existingOverlay)
        {
            _currentOverlay = existingOverlay;
            PatchedGlobalUis.Add(globalUiId);
            return true;
        }

        JukeboxOverlay overlay = new()
        {
            Name = "TogawasakikoJukeboxOverlay"
        };
        overlay.InitializeForInjection(globalUi);
        globalUi.AddChild(overlay);
        _currentOverlay = overlay;
        PatchedGlobalUis.Add(globalUiId);
        ModSupport.LogInfo(
            $"Injected jukebox overlay v7 into: {globalUi.GetPath()} insideTree={overlay.IsInsideTree()} visible={overlay.Visible} pos={overlay.Position} size={overlay.Size}");
        return true;
    }

    private static bool ShouldDisplayForCurrentRun()
    {
        RunState? runState = AccessTools.Property(typeof(RunManager), "State")?.GetValue(RunManager.Instance) as RunState;
        Player? player = TogawasakikoMod.GetLocalPlayer(runState);
        return player?.Character is Togawasakiko;
    }
}

internal sealed partial class JukeboxOverlay : Control
{
    private const string TrackDirectory = "res://audio/music/tracks";
    private const string OffOptionLabel = "Off (null)";
    private const float ClosedWidth = 164.0f;
    private const float ClosedHeight = 52.0f;
    private const float ExpandedWidth = 344.0f;
    private const float ExpandedHeight = 228.0f;
    private const float RightMargin = 28.0f;
    private const float TopMargin = 220.0f;

    private static readonly Dictionary<string, int> PreferredTrackOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["togawasakiko_title_theme"] = 0,
        ["ave_mujica"] = 1,
        ["killkiss"] = 2,
        ["music_of_the_celestial_sphere"] = 3,
        ["choir_s_choir"] = 4
    };

    private readonly Dictionary<string, AudioStream> _streamCache = new(StringComparer.Ordinal);
    private readonly List<JukeboxTrackInfo> _tracks = [];

    private AudioStreamPlayer? _player;
    private Button? _toggleButton;
    private PanelContainer? _panel;
    private OptionButton? _trackSelector;
    private Godot.Range? _progressSlider;
    private Godot.Range? _volumeSlider;
    private Label? _statusLabel;

    private bool _expanded;
    private bool _built;
    private bool _loggedPlacement;
    private bool _suppressVolumeSignal;
    private bool _suppressTrackSignal;
    private bool _suppressProgressSignal;
    private string? _activeTrackPath;
    private int _bgmBusIndex = -1;
    private float _savedBgmBusVolumeDb;
    private bool _isBgmBusMutedForCustomTrack;

    public void InitializeForInjection(Control host)
    {
        TopLevel = true;
        Visible = true;
        ProcessMode = ProcessModeEnum.Always;
        SetProcess(true);
        EnsureUiBuilt();
        Size = new Vector2(ClosedWidth, ClosedHeight);
        Position = new Vector2(1480.0f, TopMargin);
        UpdatePlacement(host.GetViewportRect().Size);
        ModSupport.LogInfo(
            $"Jukebox initialize v7 host={host.GetPath()} viewport=({host.GetViewportRect().Size.X:0.0},{host.GetViewportRect().Size.Y:0.0}) pos={Position} size={Size}");
    }

    public override void _EnterTree()
    {
        ModSupport.LogInfo("Jukebox overlay _EnterTree v7.");
    }

    public override void _Ready()
    {
        EnsureUiBuilt();
        SetProcess(true);
        UpdatePlacement();
        ModSupport.LogInfo("Jukebox overlay _Ready v7.");
    }

    public override void _Process(double delta)
    {
        UpdatePlacement();
        UpdateProgressUi();
    }

    public override void _ExitTree()
    {
        RestoreBgmBusIfNeeded();
        _player?.Stop();
    }

    private void EnsureUiBuilt()
    {
        if (_built)
        {
            return;
        }

        BuildUi();
        RefreshTrackList();
        SetExpanded(false);
        _built = true;
    }

    private void BuildUi()
    {
        Visible = true;
        AnchorLeft = 0.0f;
        AnchorRight = 0.0f;
        AnchorTop = 0.0f;
        AnchorBottom = 0.0f;
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 100;

        AttachFreshPlayer();

        VBoxContainer root = new()
        {
            Name = "Root",
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill,
            MouseFilter = MouseFilterEnum.Pass
        };
        AddChild(root);

        _toggleButton = new Button
        {
            Name = "ToggleButton",
            Text = "BGM",
            CustomMinimumSize = new Vector2(ClosedWidth, ClosedHeight),
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            MouseFilter = MouseFilterEnum.Stop
        };
        _toggleButton.AddThemeStyleboxOverride("normal", CreateBox(new Color(0.09f, 0.12f, 0.16f, 0.96f), new Color(0.88f, 0.74f, 0.3f, 1.0f), 2, 10));
        _toggleButton.AddThemeStyleboxOverride("hover", CreateBox(new Color(0.13f, 0.17f, 0.24f, 1.0f), new Color(0.96f, 0.82f, 0.38f, 1.0f), 2, 10));
        _toggleButton.AddThemeStyleboxOverride("pressed", CreateBox(new Color(0.16f, 0.2f, 0.28f, 1.0f), new Color(0.96f, 0.82f, 0.38f, 1.0f), 2, 10));
        _toggleButton.AddThemeColorOverride("font_color", new Color(0.97f, 0.94f, 0.84f, 1.0f));
        _toggleButton.Pressed += ToggleExpanded;
        root.AddChild(_toggleButton);

        _panel = new PanelContainer
        {
            Name = "Panel",
            Visible = false,
            CustomMinimumSize = new Vector2(ExpandedWidth, 0),
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            MouseFilter = MouseFilterEnum.Stop
        };
        _panel.AddThemeStyleboxOverride("panel", CreateBox(new Color(0.05f, 0.07f, 0.1f, 0.96f), new Color(0.56f, 0.78f, 0.94f, 1.0f), 2, 12));
        root.AddChild(_panel);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        _panel.AddChild(margin);

        VBoxContainer content = new();
        content.AddThemeConstantOverride("separation", 10);
        margin.AddChild(content);

        Label header = new()
        {
            Text = "BGM Selection",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        content.AddChild(header);

        HBoxContainer trackRow = new();
        trackRow.AddThemeConstantOverride("separation", 8);
        content.AddChild(trackRow);

        Label trackLabel = new()
        {
            Text = "Track",
            CustomMinimumSize = new Vector2(48, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        trackRow.AddChild(trackLabel);

        _trackSelector = new OptionButton
        {
            Name = "TrackSelector",
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        _trackSelector.ItemSelected += OnTrackSelected;
        trackRow.AddChild(_trackSelector);

        HBoxContainer progressRow = new();
        progressRow.AddThemeConstantOverride("separation", 8);
        content.AddChild(progressRow);

        Label progressLabel = new()
        {
            Text = "Seek",
            CustomMinimumSize = new Vector2(48, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        progressRow.AddChild(progressLabel);

        _progressSlider = CreateSlider("ProgressSlider", 0, 100, 0, OnProgressChanged);
        if (_progressSlider != null)
        {
            progressRow.AddChild(_progressSlider);
        }

        HBoxContainer volumeRow = new();
        volumeRow.AddThemeConstantOverride("separation", 8);
        content.AddChild(volumeRow);

        Label volumeLabel = new()
        {
            Text = "Volume",
            CustomMinimumSize = new Vector2(48, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        volumeRow.AddChild(volumeLabel);

        _volumeSlider = CreateSlider("VolumeSlider", 0, 100, 100, OnVolumeChanged);
        if (_volumeSlider != null)
        {
            volumeRow.AddChild(_volumeSlider);
        }

        _statusLabel = new Label
        {
            Name = "StatusLabel",
            Text = "No tracks detected.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        content.AddChild(_statusLabel);
    }

    private void ToggleExpanded()
    {
        SetExpanded(!_expanded);
    }

    private void SetExpanded(bool expanded)
    {
        _expanded = expanded;
        if (_panel != null)
        {
            _panel.Visible = expanded;
        }

        if (_toggleButton != null)
        {
            _toggleButton.Text = expanded ? "BGM Close" : "BGM";
        }

        UpdatePlacement();
    }

    private void RefreshTrackList()
    {
        _tracks.Clear();
        _tracks.AddRange(LoadTracks());

        if (_trackSelector == null)
        {
            return;
        }

        _suppressTrackSignal = true;
        _trackSelector.Clear();
        _trackSelector.AddItem(OffOptionLabel);

        foreach (JukeboxTrackInfo track in _tracks)
        {
            _trackSelector.AddItem(track.DisplayName);
        }

        _trackSelector.Select(0);
        _trackSelector.Disabled = _tracks.Count == 0;
        _suppressTrackSignal = false;
        SetVolumeValue(100);
        SetVolumeEnabled(_tracks.Count > 0);

        SetStatus(_tracks.Count == 0
            ? "No runtime tracks in res://audio/music/tracks."
            : "Select a track or turn BGM off.");
        SetProgressEnabled(false);
    }

    private IEnumerable<JukeboxTrackInfo> LoadTracks()
    {
        DirAccess? dir = DirAccess.Open(TrackDirectory);
        if (dir == null)
        {
            yield break;
        }

        List<JukeboxTrackInfo> discovered = [];
        dir.ListDirBegin();
        try
        {
            while (true)
            {
                string fileName = dir.GetNext();
                if (string.IsNullOrEmpty(fileName))
                {
                    break;
                }

                if (dir.CurrentIsDir() || fileName.StartsWith(".", StringComparison.Ordinal))
                {
                    continue;
                }

                string extension = Path.GetExtension(fileName);
                if (!extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string id = Path.GetFileNameWithoutExtension(fileName);
                string resourcePath = $"{TrackDirectory}/{fileName}";
                discovered.Add(new JukeboxTrackInfo(id, ToDisplayName(id), resourcePath));
            }
        }
        finally
        {
            dir.ListDirEnd();
        }

        foreach (JukeboxTrackInfo track in discovered
                     .OrderBy(track => PreferredTrackOrder.GetValueOrDefault(track.Id, 1000))
                     .ThenBy(track => track.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            yield return track;
        }
    }

    private void OnTrackSelected(long index)
    {
        if (_suppressTrackSignal)
        {
            return;
        }

        if (index <= 0)
        {
            StopCustomTrackAndResumeRunMusic();
            return;
        }

        int trackIndex = (int)index - 1;
        if (trackIndex < 0 || trackIndex >= _tracks.Count)
        {
            return;
        }

        PlayCustomTrack(_tracks[trackIndex]);
    }

    private void PlayCustomTrack(JukeboxTrackInfo track)
    {
        if (_player == null)
        {
            return;
        }

        AudioStream? stream = LoadTrack(track.ResourcePath);
        if (stream == null)
        {
            SetStatus($"Failed to load {track.ResourcePath}.");
            return;
        }

        MuteBgmBusForCustomTrackIfNeeded();
        _player.Stop();
        _player.Stream = stream;
        _player.Bus = ResolveAudioBus();
        _player.Play();
        _activeTrackPath = track.ResourcePath;

        SetProgressEnabled(true);
        SetVolumeEnabled(true);
        ApplyVolumeSliderToPlayer();
        SetStatus($"{track.DisplayName} playing.");
    }

    private void StopCustomTrackAndResumeRunMusic()
    {
        _activeTrackPath = null;
        StopPlayerOnly();
        RestoreBgmBusIfNeeded();

        SetProgressValue(0);
        SetProgressEnabled(false);
        SetVolumeEnabled(_tracks.Count > 0);

        SetStatus(_tracks.Count == 0
            ? "No runtime tracks in res://audio/music/tracks."
            : "Run music restored.");
    }

    private void StopPlayerOnly()
    {
        if (_player == null)
        {
            return;
        }

        _player.Stop();
        _player.Stream = null;
        SetProgressValue(0);
        SetProgressEnabled(false);
    }

    public void ResetToOffForCombat()
    {
        if (_trackSelector != null)
        {
            _suppressTrackSignal = true;
            _trackSelector.Select(0);
            _suppressTrackSignal = false;
        }

        StopCustomTrackAndResumeRunMusic();
    }

    private void UpdatePlacement()
    {
        if (!IsInsideTree() || GetViewport() == null)
        {
            return;
        }

        float overlayWidth = _expanded ? ExpandedWidth : ClosedWidth;
        float overlayHeight = _expanded ? ExpandedHeight : ClosedHeight;
        Vector2 viewportSize = GetViewportRect().Size;
        if (viewportSize.X <= 0 || viewportSize.Y <= 0)
        {
            return;
        }

        float left = Math.Max(12.0f, viewportSize.X - overlayWidth - RightMargin);
        float top = Math.Max(12.0f, TopMargin);

        Position = new Vector2(left, top);
        Size = new Vector2(overlayWidth, overlayHeight);

        if (!_loggedPlacement)
        {
            _loggedPlacement = true;
            ModSupport.LogInfo(
                $"Jukebox placement v7 overlay=({left:0.0},{top:0.0},{overlayWidth:0.0},{overlayHeight:0.0}) viewport=({viewportSize.X:0.0},{viewportSize.Y:0.0})");
        }
    }

    private void UpdatePlacement(Vector2 viewportSize)
    {
        float overlayWidth = _expanded ? ExpandedWidth : ClosedWidth;
        float overlayHeight = _expanded ? ExpandedHeight : ClosedHeight;
        if (viewportSize.X <= 0 || viewportSize.Y <= 0)
        {
            return;
        }

        float left = Math.Max(12.0f, viewportSize.X - overlayWidth - RightMargin);
        float top = Math.Max(12.0f, TopMargin);

        Position = new Vector2(left, top);
        Size = new Vector2(overlayWidth, overlayHeight);

        if (!_loggedPlacement)
        {
            _loggedPlacement = true;
            ModSupport.LogInfo(
                $"Jukebox placement v7 overlay=({left:0.0},{top:0.0},{overlayWidth:0.0},{overlayHeight:0.0}) viewport=({viewportSize.X:0.0},{viewportSize.Y:0.0}) pre-ready");
        }
    }

    private Godot.Range CreateSlider(string name, double minValue, double maxValue, double initialValue, Action<double> valueChangedHandler)
    {
        PackedScene? volumeSliderScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/volume_slider.tscn");
        Godot.Range? slider = volumeSliderScene?.Instantiate() as Godot.Range;
        if (slider == null)
        {
            slider = new HSlider();
            ModSupport.LogWarn($"Jukebox slider '{name}' fell back to HSlider; volume_slider.tscn did not load.");
        }

        slider.Name = name;
        slider.SizeFlagsHorizontal = SizeFlags.Fill;
        slider.CustomMinimumSize = new Vector2(220, 40);
        slider.MinValue = minValue;
        slider.MaxValue = maxValue;
        slider.Step = 0.1;
        slider.Value = initialValue;
        slider.MouseFilter = MouseFilterEnum.Stop;
        slider.FocusMode = FocusModeEnum.All;
        slider.GuiInput += OnSliderGuiInput;
        slider.ValueChanged += value => valueChangedHandler(value);
        return slider;
    }

    private static StyleBoxFlat CreateBox(Color bgColor, Color borderColor, int borderWidth, int cornerRadius)
    {
        StyleBoxFlat box = new()
        {
            BgColor = bgColor,
            BorderColor = borderColor,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = cornerRadius,
            CornerRadiusTopRight = cornerRadius,
            CornerRadiusBottomRight = cornerRadius,
            CornerRadiusBottomLeft = cornerRadius
        };
        return box;
    }

    private AudioStream? LoadTrack(string resourcePath)
    {
        if (_streamCache.TryGetValue(resourcePath, out AudioStream? cached) && cached != null)
        {
            return cached;
        }

        AudioStream? stream = GD.Load<AudioStream>(resourcePath);
        if (stream != null)
        {
            _streamCache[resourcePath] = stream;
        }
        else
        {
            ModSupport.LogWarn("Jukebox failed to load track: " + resourcePath);
        }

        return stream;
    }

    private void AttachFreshPlayer()
    {
        if (_player != null)
        {
            _player.Finished -= OnTrackFinished;
            _player.Stop();
            RemoveChild(_player);
            _player.QueueFree();
        }

        _player = new AudioStreamPlayer
        {
            Name = "JukeboxAudioPlayer",
            Bus = ResolveAudioBus()
        };
        _player.Finished += OnTrackFinished;
        AddChild(_player);
    }

    private void UpdateProgressUi()
    {
        if (_player?.Stream == null || _progressSlider == null || _statusLabel == null)
        {
            return;
        }

        double length = _player.Stream.GetLength();
        double position = _player.GetPlaybackPosition();
        if (length <= 0)
        {
            return;
        }

        if (!_progressSlider.HasFocus())
        {
            SetProgressValue(position / length * 100.0);
        }

        string displayName = _tracks.FirstOrDefault(track => track.ResourcePath == _activeTrackPath)?.DisplayName
            ?? Path.GetFileNameWithoutExtension(_activeTrackPath ?? "custom_track");
        _statusLabel.Text = $"{displayName} playing.";
    }

    private void OnProgressChanged(double value)
    {
        if (_suppressProgressSignal || _player?.Stream == null)
        {
            return;
        }

        double length = _player.Stream.GetLength();
        if (length <= 0)
        {
            return;
        }

        double seekPosition = Math.Clamp(value / 100.0, 0.0, 1.0) * length;
        _player.Seek((float)seekPosition);
    }

    private void OnSliderGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            AcceptEvent();
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion && (mouseMotion.ButtonMask & MouseButtonMask.Left) != 0)
        {
            AcceptEvent();
        }
    }

    private void OnVolumeChanged(double value)
    {
        if (_suppressVolumeSignal)
        {
            return;
        }

        ApplyVolumeSliderToPlayer();
    }

    private void OnTrackFinished()
    {
        if (_player == null || string.IsNullOrEmpty(_activeTrackPath))
        {
            return;
        }

        _player.Play();
    }

    private void SetProgressEnabled(bool enabled)
    {
        if (_progressSlider == null)
        {
            return;
        }

        _progressSlider.MouseFilter = enabled ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        _progressSlider.FocusMode = enabled ? FocusModeEnum.All : FocusModeEnum.None;
    }

    private void SetVolumeEnabled(bool enabled)
    {
        if (_volumeSlider == null)
        {
            return;
        }

        _volumeSlider.MouseFilter = enabled ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        _volumeSlider.FocusMode = enabled ? FocusModeEnum.All : FocusModeEnum.None;
    }

    private void SetProgressValue(double value)
    {
        if (_progressSlider == null)
        {
            return;
        }

        _suppressProgressSignal = true;
        _progressSlider.Value = value;
        _suppressProgressSignal = false;
    }

    private void SetVolumeValue(double value)
    {
        if (_volumeSlider == null)
        {
            return;
        }

        _suppressVolumeSignal = true;
        _volumeSlider.Value = value;
        _suppressVolumeSignal = false;
    }

    private void SetStatus(string text)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = text;
        }
    }

    private void ApplyVolumeSliderToPlayer()
    {
        if (_player == null || _volumeSlider == null)
        {
            return;
        }

        float linear = (float)Math.Clamp(_volumeSlider.Value / 100.0, 0.0, 1.0);
        _player.VolumeDb = linear <= 0.0001f ? -80.0f : Mathf.LinearToDb(linear);
    }

    private void MuteBgmBusForCustomTrackIfNeeded()
    {
        int bgmBusIndex = ResolveBgmBusIndex();
        if (bgmBusIndex < 0)
        {
            return;
        }

        if (_isBgmBusMutedForCustomTrack && _bgmBusIndex == bgmBusIndex)
        {
            return;
        }

        _bgmBusIndex = bgmBusIndex;
        _savedBgmBusVolumeDb = AudioServer.GetBusVolumeDb(bgmBusIndex);
        AudioServer.SetBusVolumeDb(bgmBusIndex, -80.0f);
        _isBgmBusMutedForCustomTrack = true;
    }

    private void RestoreBgmBusIfNeeded()
    {
        if (!_isBgmBusMutedForCustomTrack)
        {
            return;
        }

        if (_bgmBusIndex >= 0 && _bgmBusIndex < AudioServer.BusCount)
        {
            AudioServer.SetBusVolumeDb(_bgmBusIndex, _savedBgmBusVolumeDb);
        }

        _bgmBusIndex = -1;
        _savedBgmBusVolumeDb = 0.0f;
        _isBgmBusMutedForCustomTrack = false;
    }

    private static int ResolveBgmBusIndex()
    {
        int bgmBusIndex = AudioServer.GetBusIndex("Bgm");
        if (bgmBusIndex >= 0)
        {
            return bgmBusIndex;
        }

        return AudioServer.GetBusIndex("Music");
    }

    private static string ResolveAudioBus()
    {
        return AudioServer.GetBusIndex("Master") >= 0 ? "Master" : "Bgm";
    }

    private static string ToDisplayName(string id)
    {
        string[] parts = id.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0
            ? id
            : string.Join(" ", parts.Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

}
