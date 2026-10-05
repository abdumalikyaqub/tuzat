using System.Text.Json;

namespace Tuzat.Core;

public sealed record SettingsLoadResult(TuzatSettings Settings, IReadOnlyList<string> Errors);

public static class SettingsLoader
{
    public const string DefaultJson = """
        {
          "fixLastWord": "Pause",
          "fixSelection": "Shift+Pause",
          "layouts": ["00000409", "00000419"],
          "clipboardRestoreDelayMs": 200
        }
        """;

    // Users edit the file in Notepad, so comments and trailing commas are tolerated.
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static SettingsLoadResult LoadOrCreate(string path)
    {
        try
        {
            if (File.Exists(path)) return Parse(File.ReadAllText(path));

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, DefaultJson);
            return new SettingsLoadResult(TuzatSettings.Default, []);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return new SettingsLoadResult(
                TuzatSettings.Default,
                [Localized.Text($"cannot read {path}: {e.Message}", $"не удалось прочитать {path}: {e.Message}")]);
        }
    }

    public static SettingsLoadResult Parse(string json)
    {
        SettingsFile? file;
        try
        {
            file = JsonSerializer.Deserialize<SettingsFile>(json, Options);
        }
        catch (JsonException e)
        {
            return new SettingsLoadResult(
                TuzatSettings.Default,
                [Localized.Text(
                    $"invalid JSON (line {e.LineNumber + 1}), default values are used",
                    $"неверный JSON (строка {e.LineNumber + 1}), используются значения по умолчанию")]);
        }

        file ??= new SettingsFile();
        var defaults = TuzatSettings.Default;
        var errors = new List<string>();

        var fixLastWord = ReadHotkey(file.FixLastWord, "fixLastWord", defaults.FixLastWord, errors);
        var fixSelection = ReadHotkey(file.FixSelection, "fixSelection", defaults.FixSelection, errors);
        if (fixLastWord == fixSelection)
        {
            errors.Add(Localized.Text(
                "fixLastWord and fixSelection are the same, default values are used",
                "fixLastWord и fixSelection совпадают, используются значения по умолчанию"));
            fixLastWord = defaults.FixLastWord;
            fixSelection = defaults.FixSelection;
        }

        var (layoutA, layoutB) = ReadLayouts(file.Layouts, defaults, errors);
        var delay = ReadDelay(file.ClipboardRestoreDelayMs, defaults.ClipboardRestoreDelayMs, errors);

        return new SettingsLoadResult(new TuzatSettings(fixLastWord, fixSelection, layoutA, layoutB, delay), errors);
    }

    private static Hotkey ReadHotkey(string? text, string field, Hotkey fallback, List<string> errors)
    {
        if (text is null) return fallback;
        if (HotkeyParser.TryParse(text, out var hotkey, out var error)) return hotkey;

        errors.Add($"{field}: {error}");
        return fallback;
    }

    private static (string, string) ReadLayouts(string?[]? layouts, TuzatSettings defaults, List<string> errors)
    {
        if (layouts is null) return (defaults.LayoutA, defaults.LayoutB);
        if (layouts is [{ } a, { } b] && IsKlid(a) && IsKlid(b) && !a.Equals(b, StringComparison.OrdinalIgnoreCase))
        {
            return (a.ToUpperInvariant(), b.ToUpperInvariant());
        }

        errors.Add(Localized.Text(
            "layouts: two different layout IDs (KLID) are required, for example \"00000409\"",
            "layouts: нужны две разные раскладки в формате KLID, например \"00000409\""));
        return (defaults.LayoutA, defaults.LayoutB);
    }

    private static bool IsKlid(string value) => value.Length == 8 && value.All(char.IsAsciiHexDigit);

    private static int ReadDelay(int? value, int fallback, List<string> errors)
    {
        if (value is null) return fallback;
        if (value is >= TuzatSettings.MinRestoreDelayMs and <= TuzatSettings.MaxRestoreDelayMs) return value.Value;

        const int min = TuzatSettings.MinRestoreDelayMs, max = TuzatSettings.MaxRestoreDelayMs;
        errors.Add(Localized.Text(
            $"clipboardRestoreDelayMs: a number from {min} to {max} is required",
            $"clipboardRestoreDelayMs: нужно число от {min} до {max}"));
        return fallback;
    }

    private sealed class SettingsFile
    {
        public string? FixLastWord { get; set; }
        public string? FixSelection { get; set; }
        public string?[]? Layouts { get; set; }
        public int? ClipboardRestoreDelayMs { get; set; }
    }
}
