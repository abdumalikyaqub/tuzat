using System.Text;

namespace Tuzat.Core.Tests;

public class SettingsLoaderTests
{
    private static readonly TuzatSettings Defaults = TuzatSettings.Default;

    [Fact]
    public void Default_json_matches_default_settings()
    {
        var result = SettingsLoader.Parse(SettingsLoader.DefaultJson);

        Assert.Equal(Defaults, result.Settings);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    public void Missing_fields_use_defaults_without_errors(string json)
    {
        var result = SettingsLoader.Parse(json);

        Assert.Equal(Defaults, result.Settings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Valid_custom_settings_are_read()
    {
        var result = SettingsLoader.Parse("""
            {
              "fixLastWord": "F12",
              "fixSelection": "Ctrl+Alt+F12",
              "layouts": ["00000409", "00000428"],
              "clipboardRestoreDelayMs": 500
            }
            """);

        Assert.Empty(result.Errors);
        Assert.Equal(new Hotkey(HotkeyModifiers.None, 0x7B), result.Settings.FixLastWord);
        Assert.Equal(new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 0x7B), result.Settings.FixSelection);
        Assert.Equal("00000428", result.Settings.LayoutB);
        Assert.Equal(500, result.Settings.ClipboardRestoreDelayMs);
    }

    [Fact]
    public void Comments_and_trailing_commas_are_accepted()
    {
        var result = SettingsLoader.Parse("""
            {
              // edited by hand
              "fixLastWord": "F12",
            }
            """);

        Assert.Empty(result.Errors);
        Assert.Equal(new Hotkey(HotkeyModifiers.None, 0x7B), result.Settings.FixLastWord);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("[]")]
    [InlineData("""{ "clipboardRestoreDelayMs": "abc" }""")]
    public void Invalid_json_uses_all_defaults_with_one_error(string json)
    {
        var result = SettingsLoader.Parse(json);

        Assert.Equal(Defaults, result.Settings);
        Assert.Contains("JSON", Assert.Single(result.Errors));
    }

    [Fact]
    public void Invalid_field_uses_its_default_and_names_the_field()
    {
        var result = SettingsLoader.Parse("""{ "fixLastWord": "F12", "fixSelection": "Foo" }""");

        Assert.Equal(new Hotkey(HotkeyModifiers.None, 0x7B), result.Settings.FixLastWord);
        Assert.Equal(Defaults.FixSelection, result.Settings.FixSelection);
        Assert.Contains("fixSelection", Assert.Single(result.Errors));
    }

    [Fact]
    public void Equal_hotkeys_reset_both_to_defaults()
    {
        var result = SettingsLoader.Parse("""{ "fixLastWord": "F12", "fixSelection": "F12" }""");

        Assert.Equal(Defaults.FixLastWord, result.Settings.FixLastWord);
        Assert.Equal(Defaults.FixSelection, result.Settings.FixSelection);
        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData("""{ "layouts": ["00000409"] }""")]
    [InlineData("""{ "layouts": ["00000409", "00000409"] }""")]
    [InlineData("""{ "layouts": ["0409", "0419"] }""")]
    [InlineData("""{ "layouts": ["00000409", "0000041G"] }""")]
    [InlineData("""{ "layouts": ["00000409", null] }""")]
    public void Invalid_layouts_use_defaults(string json)
    {
        var result = SettingsLoader.Parse(json);

        Assert.Equal((Defaults.LayoutA, Defaults.LayoutB), (result.Settings.LayoutA, result.Settings.LayoutB));
        Assert.Contains("layouts", Assert.Single(result.Errors));
    }

    [Fact]
    public void Layout_ids_are_upper_cased() =>
        Assert.Equal("0000041F", SettingsLoader.Parse("""{ "layouts": ["00000409", "0000041f"] }""").Settings.LayoutB);

    [Theory]
    [InlineData(49)]
    [InlineData(2001)]
    public void Delay_out_of_range_uses_default(int delay)
    {
        var result = SettingsLoader.Parse($$"""{ "clipboardRestoreDelayMs": {{delay}} }""");

        Assert.Equal(Defaults.ClipboardRestoreDelayMs, result.Settings.ClipboardRestoreDelayMs);
        Assert.Contains("clipboardRestoreDelayMs", Assert.Single(result.Errors));
    }

    [Fact]
    public void LoadOrCreate_writes_default_file_when_missing()
    {
        var directory = CreateTempDirectoryPath();
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var result = SettingsLoader.LoadOrCreate(path);

            Assert.Equal(Defaults, result.Settings);
            Assert.Empty(result.Errors);
            Assert.Equal(SettingsLoader.DefaultJson, File.ReadAllText(path));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void LoadOrCreate_reads_file_with_utf8_bom()
    {
        var directory = CreateTempDirectoryPath();
        var path = Path.Combine(directory, "settings.json");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(path, """{ "fixLastWord": "F12" }""", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            var result = SettingsLoader.LoadOrCreate(path);

            Assert.Empty(result.Errors);
            Assert.Equal(new Hotkey(HotkeyModifiers.None, 0x7B), result.Settings.FixLastWord);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTempDirectoryPath() =>
        Path.Combine(Path.GetTempPath(), "tuzat-tests-" + Guid.NewGuid().ToString("N"));
}
