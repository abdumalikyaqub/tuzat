using Tuzat.Core;

namespace Tuzat.App;

internal static class FixSelectionAction
{
    private const int KeyC = 0x43;
    private const int KeyV = 0x56;
    private const int CopyTimeoutMs = 300;

    public static void Run(LayoutSwitcher layouts, InputSender input, ClipboardGuard clipboard, int restoreDelayMs)
    {
        if (FocusTracker.Current() is not { } focus || !input.ReleaseModifiers()) return;
        if (!clipboard.TrySave(out var saved)) return;

        uint before = clipboard.SequenceNumber;
        if (!input.Chord(VirtualKey.Control, KeyC)
            || !WaitForChange(clipboard, before)
            || !clipboard.TryReadText(out var text)
            || text.Length == 0)
        {
            if (clipboard.SequenceNumber != before) clipboard.TryRestore(saved);
            return;
        }

        var source = TextConverter.DetectSource(text, layouts.Map, layouts.SideOf(focus));
        if (!clipboard.TrySetText(TextConverter.Convert(text, layouts.Map, source)))
        {
            clipboard.TryRestore(saved);
            return;
        }

        // The target app reads the clipboard asynchronously; restoring too early pastes the old content.
        bool pasted = input.Chord(VirtualKey.Control, KeyV);
        if (pasted) ThreadWait.Sleep(restoreDelayMs);
        clipboard.TryRestore(saved);
        if (pasted) layouts.SwitchTo(focus, source.Opposite());
    }

    private static bool WaitForChange(ClipboardGuard clipboard, uint before)
    {
        long deadline = Environment.TickCount64 + CopyTimeoutMs;
        while (clipboard.SequenceNumber == before)
        {
            if (Environment.TickCount64 >= deadline) return false;
            ThreadWait.Sleep(10);
        }

        return true;
    }
}
