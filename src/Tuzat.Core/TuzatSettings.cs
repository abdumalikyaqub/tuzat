namespace Tuzat.Core;

public sealed record TuzatSettings(
    Hotkey FixLastWord,
    Hotkey FixSelection,
    string LayoutA,
    string LayoutB,
    int ClipboardRestoreDelayMs)
{
    public const int MinRestoreDelayMs = 50;
    public const int MaxRestoreDelayMs = 2000;

    public static TuzatSettings Default { get; } = new(
        new Hotkey(HotkeyModifiers.None, VirtualKey.Pause),
        new Hotkey(HotkeyModifiers.Shift, VirtualKey.Pause),
        "00000409",
        "00000419",
        200);

    public HotkeyBindings Bindings => new(FixLastWord, FixSelection);
}
