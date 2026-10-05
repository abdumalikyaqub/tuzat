namespace Tuzat.Core;

[Flags]
public enum HotkeyModifiers { None = 0, Ctrl = 1, Alt = 2, Shift = 4, Win = 8 }

public enum HotkeyAction { FixLastWord, FixSelection }

public readonly record struct Hotkey(HotkeyModifiers Modifiers, int Key)
{
    public bool Matches(int key, HotkeyModifiers modifiers) => key == Key && modifiers == Modifiers;
}

public sealed record HotkeyBindings(Hotkey FixLastWord, Hotkey FixSelection)
{
    public HotkeyAction? Match(int key, HotkeyModifiers modifiers) =>
        FixLastWord.Matches(key, modifiers) ? HotkeyAction.FixLastWord
        : FixSelection.Matches(key, modifiers) ? HotkeyAction.FixSelection
        : null;
}
