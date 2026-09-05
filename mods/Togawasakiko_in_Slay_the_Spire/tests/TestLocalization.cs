using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MegaCrit.Sts2.Core.Localization;
using Togawasakiko_in_Slay_the_Spire;

internal static class TestLocalization
{
    internal static string RepoRoot { get; } = FindRoot();

    internal static void Load(string language)
    {
        // Bypass only the native filesystem/settings constructor. Use the game's
        // actual tables and formatters with current mod localization dictionaries.
        var manager = (LocManager)RuntimeHelpers.GetUninitializedObject(typeof(LocManager));
        var tables = new Dictionary<string, LocTable>();
        string directory = Path.Combine(RepoRoot, "references/pck-extract/sts2-main/localization", language);
        foreach (string path in Directory.EnumerateFiles(directory, "*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            tables[name] = new LocTable(name, JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!);
        }
        Type support = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
        foreach ((string table, string field) in new[] { ("cards", "CardLocEntries"), ("powers", "PowerLocEntries"), ("characters", "CharacterLocEntries") })
        {
            var entries = (IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>)support
                .GetField(field, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            tables[table].MergeWith(new Dictionary<string, string>(entries[language]));
        }
        typeof(LocManager).GetField("_tables", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, tables);
        typeof(LocManager).GetProperty(nameof(LocManager.Instance))!.SetValue(null, manager);
        typeof(LocManager).GetProperty(nameof(LocManager.Language))!.SetValue(manager, language);
        typeof(LocManager).GetProperty(nameof(LocManager.CultureInfo))!.SetValue(manager, CultureInfo.GetCultureInfo(language == "zhs" ? "zh-CN" : "en"));
        typeof(LocManager).GetProperty(nameof(LocManager.StringComparer))!.SetValue(manager, StringComparer.Ordinal);
        typeof(LocManager).GetMethod("LoadLocFormatters", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(manager, null);
    }

    private static string FindRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "local/game-path.txt"))) return dir.FullName;
        throw new DirectoryNotFoundException("Run tests from a built project inside the STS2 workspace.");
    }
}
