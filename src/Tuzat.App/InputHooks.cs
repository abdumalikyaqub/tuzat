using System.ComponentModel;
using System.Runtime.InteropServices;
using Tuzat.Core;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

// A dedicated thread: Windows silently removes a low-level hook whose thread is busy, and the UI thread waits often.
internal sealed class InputHooks : IDisposable
{
    public const nuint InjectedMarker = 0x545A5401;

    private const uint WmReinstall = WM_APP + 1;
    private const uint WmClear = WM_APP + 2;

    private readonly Action<HotkeyAction, KeyStroke[]> _onHotkey;
    private readonly WordBuffer _buffer = new();
    private readonly HotkeyFilter _filter = new();
    private readonly HookProc _keyboardProc;
    private readonly HookProc _mouseProc;
    private Thread? _thread;
    private uint _threadId;
    private nint _keyboardHook;
    private nint _mouseHook;
    private volatile bool _paused;

    // onHotkey runs on the hook thread and must return quickly.
    public InputHooks(Action<HotkeyAction, KeyStroke[]> onHotkey)
    {
        _onHotkey = onHotkey;
        _keyboardProc = KeyboardProc;
        _mouseProc = MouseProc;
    }

    public bool Paused
    {
        get => _paused;
        set
        {
            _paused = value;
            Post(WmClear);
        }
    }

    public void SetBindings(HotkeyBindings? bindings) => _filter.Bindings = bindings;

    public void Start()
    {
        int error = 0;
        using var ready = new ManualResetEventSlim();
        _thread = new Thread(() =>
        {
            _threadId = GetCurrentThreadId();
            PeekMessage(out _, 0, WM_USER, WM_USER, PM_NOREMOVE); // creates the message queue before anyone posts to it
            error = Install();
            ready.Set();
            if (error == 0) RunLoop();
        })
        { IsBackground = true, Name = "Tuzat hooks" };
        _thread.Start();
        ready.Wait();

        if (error != 0) throw new Win32Exception(error);
    }

    public void Reinstall() => Post(WmReinstall);

    public void Dispose()
    {
        if (_thread is null) return;
        Post(WM_QUIT);
        _thread.Join(TimeSpan.FromSeconds(1));
        _thread = null;
    }

    private void Post(uint message)
    {
        if (_threadId != 0) PostThreadMessage(_threadId, message, 0, 0);
    }

    private void RunLoop()
    {
        while (GetMessage(out var message, 0, 0, 0) > 0)
        {
            if (message.message == WmReinstall)
            {
                Uninstall();
                _filter.Reset();
                _buffer.Clear();
                Install();
            }
            else if (message.message == WmClear)
            {
                _buffer.Clear();
                _filter.Reset();
            }
        }

        Uninstall();
    }

    private int Install()
    {
        nint module = GetModuleHandle(null);
        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(_keyboardProc), module, 0);
        if (_keyboardHook == 0) return Marshal.GetLastPInvokeError();

        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(_mouseProc), module, 0);
        if (_mouseHook != 0) return 0;

        int error = Marshal.GetLastPInvokeError();
        Uninstall();
        return error;
    }

    private void Uninstall()
    {
        if (_keyboardHook != 0) UnhookWindowsHookEx(_keyboardHook);
        if (_mouseHook != 0) UnhookWindowsHookEx(_mouseHook);
        _keyboardHook = 0;
        _mouseHook = 0;
    }

    private unsafe nint KeyboardProc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && HandleKey((int)wParam, *(KBDLLHOOKSTRUCT*)lParam)) return 1;
        return CallNextHookEx(0, nCode, wParam, lParam);
    }

    // Returns true when the key must be hidden from other apps.
    private bool HandleKey(int message, KBDLLHOOKSTRUCT key)
    {
        if (key.dwExtraInfo == InjectedMarker || _paused) return false;

        int virtualKey = (int)key.vkCode;
        if (message is WM_KEYUP or WM_SYSKEYUP) return _filter.OnKeyUp(virtualKey);

        var modifiers = CurrentModifiers();
        switch (_filter.OnKeyDown(virtualKey, modifiers, out var action))
        {
            case KeyDecision.Trigger:
                _onHotkey(action, _buffer.Snapshot(GetForegroundWindow()));
                return true;
            case KeyDecision.Swallow:
                return true;
        }

        var stroke = new KeyStroke(
            virtualKey,
            (ushort)key.scanCode,
            (key.flags & LLKHF_EXTENDED) != 0,
            modifiers.HasFlag(HotkeyModifiers.Shift));
        _buffer.OnKeyDown(stroke, (modifiers & ~HotkeyModifiers.Shift) != 0, GetForegroundWindow());
        return false;
    }

    private nint MouseProc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && (int)wParam is WM_LBUTTONDOWN or WM_RBUTTONDOWN or WM_MBUTTONDOWN or WM_XBUTTONDOWN)
        {
            _buffer.Clear();
        }

        return CallNextHookEx(0, nCode, wParam, lParam);
    }

    private static HotkeyModifiers CurrentModifiers()
    {
        var modifiers = HotkeyModifiers.None;
        if (IsDown(VirtualKey.Shift)) modifiers |= HotkeyModifiers.Shift;
        if (IsDown(VirtualKey.Control)) modifiers |= HotkeyModifiers.Ctrl;
        if (IsDown(VirtualKey.Menu)) modifiers |= HotkeyModifiers.Alt;
        if (IsDown(VirtualKey.LWin) || IsDown(VirtualKey.RWin)) modifiers |= HotkeyModifiers.Win;
        return modifiers;
    }

    private static bool IsDown(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
}
