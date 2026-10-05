using System.Runtime.InteropServices;
using Tuzat.Core;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal sealed class InputSender
{
    private const ushort LeftShiftScanCode = 0x2A;

    // Unassigned key: pressing it stops a lone Alt or Win release from opening a menu.
    private const int MenuMaskKey = 0xE8;

    private static readonly int[] Modifiers =
    [
        VirtualKey.LShift, VirtualKey.RShift, VirtualKey.LControl, VirtualKey.RControl,
        VirtualKey.LMenu, VirtualKey.RMenu, VirtualKey.LWin, VirtualKey.RWin,
    ];

    private readonly List<INPUT> _batch = [];

    // Held hotkey modifiers would otherwise turn Ctrl+C into Ctrl+Shift+C or change replayed letters.
    public bool ReleaseModifiers()
    {
        bool masked = false;
        foreach (var key in Modifiers)
        {
            if ((GetAsyncKeyState(key) & 0x8000) == 0) continue;
            if (!masked && key is VirtualKey.LMenu or VirtualKey.RMenu or VirtualKey.LWin or VirtualKey.RWin)
            {
                AddKey(MenuMaskKey, up: false);
                AddKey(MenuMaskKey, up: true);
                masked = true;
            }
            AddKey(key, up: true);
        }

        return Flush();
    }

    public bool Backspace(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddKey(VirtualKey.Back, up: false);
            AddKey(VirtualKey.Back, up: true);
        }

        return Flush();
    }

    // Scan codes are physical positions, so the new layout produces the right characters.
    public bool Replay(IReadOnlyList<KeyStroke> strokes)
    {
        foreach (var stroke in strokes)
        {
            if (stroke.Shift) AddScan(LeftShiftScanCode, extended: false, up: false);
            AddScan(stroke.ScanCode, stroke.IsExtended, up: false);
            AddScan(stroke.ScanCode, stroke.IsExtended, up: true);
            if (stroke.Shift) AddScan(LeftShiftScanCode, extended: false, up: true);
        }

        return Flush();
    }

    public bool Chord(int modifier, int key)
    {
        AddKey(modifier, up: false);
        AddKey(key, up: false);
        AddKey(key, up: true);
        AddKey(modifier, up: true);
        return Flush();
    }

    private void AddKey(int virtualKey, bool up) =>
        _batch.Add(Keyboard(
            (ushort)virtualKey,
            (ushort)MapVirtualKey((uint)virtualKey, MAPVK_VK_TO_VSC),
            (up ? KEYEVENTF_KEYUP : 0) | (KeyClassifier.IsExtendedKey(virtualKey) ? KEYEVENTF_EXTENDEDKEY : 0)));

    private void AddScan(ushort scanCode, bool extended, bool up) =>
        _batch.Add(Keyboard(
            0,
            scanCode,
            KEYEVENTF_SCANCODE | (extended ? KEYEVENTF_EXTENDEDKEY : 0) | (up ? KEYEVENTF_KEYUP : 0)));

    private static INPUT Keyboard(ushort virtualKey, ushort scanCode, uint flags) => new()
    {
        type = INPUT_KEYBOARD,
        u = new InputUnion
        {
            ki = new KEYBDINPUT { wVk = virtualKey, wScan = scanCode, dwFlags = flags, dwExtraInfo = InputHooks.InjectedMarker },
        },
    };

    // One SendInput call per step, so the user's own typing cannot land in the middle.
    private unsafe bool Flush()
    {
        if (_batch.Count == 0) return true;

        var inputs = CollectionsMarshal.AsSpan(_batch);
        uint sent;
        fixed (INPUT* pointer = inputs) sent = SendInput((uint)inputs.Length, pointer, sizeof(INPUT));
        bool complete = sent == inputs.Length;
        _batch.Clear();
        return complete;
    }
}
