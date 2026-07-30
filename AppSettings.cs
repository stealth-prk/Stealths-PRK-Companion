using System.Text.Json;

namespace PrkCompanion;

internal sealed class AppSettings
{
    public double Opacity { get; set; } = 0.97;
    public uint HotkeyVirtualKey { get; set; } = 0xC0;
    public string HotkeyLabel { get; set; } = "`";
    public int DeckScale { get; set; } = 100;

    private static string PathToSettings => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PRK Companion", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            var json = File.ReadAllText(PathToSettings);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(PathToSettings)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(PathToSettings, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public AppSettings Copy() => new()
    {
        Opacity = Opacity,
        HotkeyVirtualKey = HotkeyVirtualKey,
        HotkeyLabel = HotkeyLabel,
        DeckScale = DeckScale
    };
}
