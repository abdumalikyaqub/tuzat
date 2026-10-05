using Tuzat.Core;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal sealed class LayoutSwitcher
{
    private const int SwitchTimeoutMs = 200;

    private readonly nint _a;
    private readonly nint _b;

    private LayoutSwitcher(nint a, nint b) { _a = a; _b = b; }

    public static LayoutSwitcher? TryCreate(string klidA, string klidB, out string? error)
    {
        nint? a = FindInstalled(klidA);
        nint? b = FindInstalled(klidB);
        if (a is null || b is null)
        {
            error = $"раскладка {(a is null ? klidA : klidB)} не установлена в Windows, горячие клавиши отключены";
            return null;
        }

        error = null;
        return new LayoutSwitcher(a.Value, b.Value);
    }

    // A layout outside the pair counts as B, so the target becomes A.
    public LayoutSide SideOf(FocusTarget focus) =>
        GetKeyboardLayout(focus.ThreadId) == _a ? LayoutSide.A : LayoutSide.B;

    public void SwitchTo(FocusTarget focus, LayoutSide side)
    {
        nint target = side == LayoutSide.A ? _a : _b;
        if (GetKeyboardLayout(focus.ThreadId) == target) return;
        if (!PostMessage(focus.Window, WM_INPUTLANGCHANGEREQUEST, 0, target)) return;

        // The switch is asynchronous; replayed keys must not arrive before it.
        long deadline = Environment.TickCount64 + SwitchTimeoutMs;
        while (GetKeyboardLayout(focus.ThreadId) != target && Environment.TickCount64 < deadline)
        {
            ThreadWait.Sleep(10);
        }
    }

    // HKL values do not map to KLID strings directly, so each layout is activated briefly on this thread to read its name.
    private static unsafe nint? FindInstalled(string klid)
    {
        int count = GetKeyboardLayoutList(0, null);
        var layouts = new nint[count];
        fixed (nint* list = layouts) count = GetKeyboardLayoutList(count, list);

        nint original = GetKeyboardLayout(0);
        char* name = stackalloc char[KL_NAMELENGTH];
        try
        {
            for (int i = 0; i < count; i++)
            {
                if (ActivateKeyboardLayout(layouts[i], 0) == 0 || !GetKeyboardLayoutName(name)) continue;
                if (new string(name).Equals(klid, StringComparison.OrdinalIgnoreCase)) return layouts[i];
            }

            return null;
        }
        finally
        {
            ActivateKeyboardLayout(original, 0);
        }
    }
}
