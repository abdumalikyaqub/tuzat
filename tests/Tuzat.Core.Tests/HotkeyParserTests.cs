namespace Tuzat.Core.Tests;

public class HotkeyParserTests
{
    [Theory]
    [InlineData("Pause", HotkeyModifiers.None, VirtualKey.Pause)]
    [InlineData("Shift+Pause", HotkeyModifiers.Shift, VirtualKey.Pause)]
    [InlineData(" shift + pause ", HotkeyModifiers.Shift, VirtualKey.Pause)]
    [InlineData("Ctrl+Alt+F12", HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 0x7B)]
    [InlineData("Control+Shift+K", HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, (int)'K')]
    [InlineData("Win+Space", HotkeyModifiers.Win, VirtualKey.Space)]
    [InlineData("ScrollLock", HotkeyModifiers.None, VirtualKey.Scroll)]
    [InlineData("F24", HotkeyModifiers.None, 0x87)]
    public void Parses_valid_hotkeys(string text, HotkeyModifiers modifiers, int key)
    {
        Assert.True(HotkeyParser.TryParse(text, out var hotkey, out var error), error);
        Assert.Equal(new Hotkey(modifiers, key), hotkey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Shift")]
    [InlineData("Shift+")]
    [InlineData("Shift+Shift+Pause")]
    [InlineData("Foo")]
    [InlineData("Pause+F12")]
    [InlineData("Ctrl+Pause")]
    [InlineData("A")]
    [InlineData("Shift+A")]
    [InlineData("Space")]
    public void Rejects_invalid_hotkeys(string text)
    {
        Assert.False(HotkeyParser.TryParse(text, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void Rejects_null() => Assert.False(HotkeyParser.TryParse(null, out _, out _));
}
