using System.Diagnostics.CodeAnalysis;

namespace Tuzat.Core;

public static class HotkeyParser
{
    private static readonly Dictionary<string, HotkeyModifiers> ModifierNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ctrl"] = HotkeyModifiers.Ctrl,
        ["Control"] = HotkeyModifiers.Ctrl,
        ["Alt"] = HotkeyModifiers.Alt,
        ["Shift"] = HotkeyModifiers.Shift,
        ["Win"] = HotkeyModifiers.Win,
    };

    private static readonly Dictionary<string, int> KeyNames = BuildKeyNames();

    public static bool TryParse(string? text, out Hotkey hotkey, [NotNullWhen(false)] out string? error)
    {
        hotkey = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "пустое значение";
            return false;
        }

        var modifiers = HotkeyModifiers.None;
        int? key = null;
        foreach (var rawPart in text.Split('+'))
        {
            var part = rawPart.Trim();
            if (ModifierNames.TryGetValue(part, out var modifier))
            {
                if (modifiers.HasFlag(modifier))
                {
                    error = $"модификатор {part} указан дважды";
                    return false;
                }
                modifiers |= modifier;
            }
            else if (KeyNames.TryGetValue(part, out var code))
            {
                if (key is not null)
                {
                    error = "указано больше одной основной клавиши";
                    return false;
                }
                key = code;
            }
            else
            {
                error = $"неизвестная клавиша \"{part}\"";
                return false;
            }
        }

        if (key is not { } finalKey)
        {
            error = "нет основной клавиши";
            return false;
        }

        // Windows reports Ctrl+Pause as VK_CANCEL, so this hotkey would never fire.
        if (finalKey == VirtualKey.Pause && modifiers.HasFlag(HotkeyModifiers.Ctrl))
        {
            error = "Ctrl+Pause не поддерживается";
            return false;
        }

        if (KeyClassifier.Classify(finalKey) is KeyKind.Printable or KeyKind.Space
            && (modifiers & (HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Win)) == 0)
        {
            error = "клавиша без Ctrl, Alt или Win мешает набору текста";
            return false;
        }

        hotkey = new Hotkey(modifiers, finalKey);
        error = null;
        return true;
    }

    private static Dictionary<string, int> BuildKeyNames()
    {
        var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pause"] = VirtualKey.Pause,
            ["ScrollLock"] = VirtualKey.Scroll,
            ["Insert"] = VirtualKey.Insert,
            ["Space"] = VirtualKey.Space,
        };
        for (int i = 1; i <= 24; i++) names[$"F{i}"] = VirtualKey.F1 + i - 1;
        for (char c = 'A'; c <= 'Z'; c++) names[c.ToString()] = c;
        for (char c = '0'; c <= '9'; c++) names[c.ToString()] = c;
        return names;
    }
}
