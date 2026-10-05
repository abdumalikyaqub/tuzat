using Tuzat.Core;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal static class LayoutMapBuilder
{
    // Keeps dead-key state intact (Windows 10 1607+), so building the map never breaks the user's next accent.
    private const uint DoNotChangeKeyboardState = 0x4;

    // Number row, three letter rows and the extra key next to left Shift (OEM_102).
    private static readonly ushort[] ScanCodes =
        [0x29, .. Range(0x02, 0x0D), .. Range(0x10, 0x1B), 0x2B, .. Range(0x1E, 0x28), .. Range(0x2C, 0x35), 0x56];

    // Unshifted keys go first, so lower-case pairs win over shifted ones in LayoutMap.
    public static LayoutMap Build(nint layoutA, nint layoutB)
    {
        var pairs = new List<(char, char)>();
        foreach (bool shift in new[] { false, true })
        {
            foreach (var scanCode in ScanCodes)
            {
                if (TryGetChar(layoutA, scanCode, shift, out var a) && TryGetChar(layoutB, scanCode, shift, out var b))
                {
                    pairs.Add((a, b));
                }
            }
        }

        return new LayoutMap(pairs);
    }

    private static unsafe bool TryGetChar(nint layout, ushort scanCode, bool shift, out char result)
    {
        result = default;
        uint virtualKey = MapVirtualKeyEx(scanCode, MAPVK_VSC_TO_VK, layout);
        if (virtualKey == 0) return false;

        byte* state = stackalloc byte[256];
        new Span<byte>(state, 256).Clear();
        if (shift) state[VirtualKey.Shift] = 0x80;

        char* buffer = stackalloc char[4];
        if (ToUnicodeEx(virtualKey, scanCode, state, buffer, 4, DoNotChangeKeyboardState, layout) != 1) return false;

        result = buffer[0];
        return !char.IsControl(result);
    }

    private static IEnumerable<ushort> Range(int first, int last) =>
        Enumerable.Range(first, last - first + 1).Select(i => (ushort)i);
}
