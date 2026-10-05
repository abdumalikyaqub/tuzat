namespace Tuzat.Core;

public enum KeyKind { Printable, Space, Backspace, Reset, Modifier, Ignore }

public static class KeyClassifier
{
    // Unknown keys reset the buffer: they may move the caret or type something we cannot replay.
    public static KeyKind Classify(int virtualKey) => virtualKey switch
    {
        VirtualKey.Space => KeyKind.Space,
        VirtualKey.Back => KeyKind.Backspace,
        >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A => KeyKind.Printable,
        >= 0xBA and <= 0xC0 or >= 0xDB and <= 0xDF or 0xE2 => KeyKind.Printable,
        VirtualKey.Shift or VirtualKey.Control or VirtualKey.Menu or VirtualKey.Capital
            or >= VirtualKey.LShift and <= VirtualKey.RMenu
            or VirtualKey.LWin or VirtualKey.RWin => KeyKind.Modifier,
        VirtualKey.Pause or VirtualKey.Snapshot or VirtualKey.NumLock or VirtualKey.Scroll
            or >= 0xAD and <= 0xB7 or 0xFF => KeyKind.Ignore,
        _ => KeyKind.Reset,
    };

    // Without the extended flag, an injected right Ctrl or Alt release is applied to the left key.
    public static bool IsExtendedKey(int virtualKey) =>
        virtualKey is VirtualKey.RControl or VirtualKey.RMenu or VirtualKey.LWin or VirtualKey.RWin;
}
