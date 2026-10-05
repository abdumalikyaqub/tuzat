namespace Tuzat.Core.Tests;

public class KeyClassifierTests
{
    [Theory]
    [InlineData(0x41)] // A
    [InlineData(0x5A)] // Z
    [InlineData(0x30)] // 0
    [InlineData(0x39)] // 9
    [InlineData(0xBA)] // OEM_1 (;)
    [InlineData(0xBC)] // OEM_COMMA
    [InlineData(0xBF)] // OEM_2 (/)
    [InlineData(0xC0)] // OEM_3 (`)
    [InlineData(0xDB)] // OEM_4 ([)
    [InlineData(0xDE)] // OEM_7 (')
    [InlineData(0xE2)] // OEM_102
    public void Printable_keys(int key) => Assert.Equal(KeyKind.Printable, KeyClassifier.Classify(key));

    [Fact]
    public void Space_key() => Assert.Equal(KeyKind.Space, KeyClassifier.Classify(VirtualKey.Space));

    [Fact]
    public void Backspace_key() => Assert.Equal(KeyKind.Backspace, KeyClassifier.Classify(VirtualKey.Back));

    [Theory]
    [InlineData(VirtualKey.Return)]
    [InlineData(VirtualKey.Tab)]
    [InlineData(VirtualKey.Escape)]
    [InlineData(VirtualKey.Insert)]
    [InlineData(VirtualKey.F1)]
    [InlineData(0x25)] // Left
    [InlineData(0x24)] // Home
    [InlineData(0x2E)] // Delete
    [InlineData(0x87)] // F24
    [InlineData(0x60)] // Numpad 0
    [InlineData(0x5D)] // Apps
    [InlineData(0xE7)] // VK_PACKET from IME and other tools
    public void Reset_keys(int key) => Assert.Equal(KeyKind.Reset, KeyClassifier.Classify(key));

    [Theory]
    [InlineData(VirtualKey.Shift)]
    [InlineData(VirtualKey.Control)]
    [InlineData(VirtualKey.Menu)]
    [InlineData(VirtualKey.Capital)]
    [InlineData(VirtualKey.LShift)]
    [InlineData(VirtualKey.RMenu)]
    [InlineData(VirtualKey.LWin)]
    [InlineData(VirtualKey.RWin)]
    public void Modifier_keys(int key) => Assert.Equal(KeyKind.Modifier, KeyClassifier.Classify(key));

    [Theory]
    [InlineData(VirtualKey.RControl)]
    [InlineData(VirtualKey.RMenu)]
    [InlineData(VirtualKey.LWin)]
    [InlineData(VirtualKey.RWin)]
    public void Right_side_modifiers_are_extended_keys(int key) => Assert.True(KeyClassifier.IsExtendedKey(key));

    [Theory]
    [InlineData(VirtualKey.LShift)]
    [InlineData(VirtualKey.RShift)]
    [InlineData(VirtualKey.LControl)]
    [InlineData(VirtualKey.LMenu)]
    [InlineData(VirtualKey.Back)]
    [InlineData(0x43)] // C
    public void Other_keys_are_not_extended(int key) => Assert.False(KeyClassifier.IsExtendedKey(key));

    [Theory]
    [InlineData(VirtualKey.Pause)]
    [InlineData(VirtualKey.Snapshot)]
    [InlineData(VirtualKey.NumLock)]
    [InlineData(VirtualKey.Scroll)]
    [InlineData(0xAD)] // Volume mute
    [InlineData(0xB3)] // Media play/pause
    public void Ignored_keys(int key) => Assert.Equal(KeyKind.Ignore, KeyClassifier.Classify(key));
}
