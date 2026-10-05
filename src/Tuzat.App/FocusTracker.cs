using System.Runtime.InteropServices;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal readonly record struct FocusTarget(nint Window, uint ThreadId);

internal static class FocusTracker
{
    // The focused child window may belong to another thread than the top-level window.
    public static FocusTarget? Current()
    {
        nint foreground = GetForegroundWindow();
        if (foreground == 0) return null;

        uint thread = GetWindowThreadProcessId(foreground, 0);
        var info = new GUITHREADINFO { cbSize = Marshal.SizeOf<GUITHREADINFO>() };
        nint window = GetGUIThreadInfo(thread, ref info) && info.hwndFocus != 0 ? info.hwndFocus : foreground;
        return new FocusTarget(window, GetWindowThreadProcessId(window, 0));
    }
}
