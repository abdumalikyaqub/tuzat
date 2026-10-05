using Tuzat.Core;

namespace Tuzat.App;

internal static class FixLastWordAction
{
    // The buffer is not cleared afterwards, so a second press converts the word back.
    public static void Run(KeyStroke[] strokes, LayoutSwitcher layouts, InputSender input)
    {
        if (FocusTracker.Current() is not { } focus || !input.ReleaseModifiers()) return;

        var target = layouts.SideOf(focus).Opposite();
        if (strokes.Length == 0)
        {
            layouts.SwitchTo(focus, target);
            return;
        }

        if (!input.Backspace(strokes.Length)) return;
        layouts.SwitchTo(focus, target);
        input.Replay(strokes);
    }
}
