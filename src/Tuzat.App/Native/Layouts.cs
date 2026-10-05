using System.Runtime.InteropServices;

namespace Tuzat.App.Native;

internal static partial class NativeMethods
{
    public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
    public const int KL_NAMELENGTH = 9;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public uint flags;
        public nint hwndActive;
        public nint hwndFocus;
        public nint hwndCapture;
        public nint hwndMenuOwner;
        public nint hwndMoveSize;
        public nint hwndCaret;
        public RECT rcCaret;
    }

    [LibraryImport("user32.dll")]
    public static partial uint GetWindowThreadProcessId(nint hWnd, nint lpdwProcessId);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO pgui);

    [LibraryImport("user32.dll")]
    public static partial nint GetKeyboardLayout(uint idThread);

    [LibraryImport("user32.dll")]
    public static unsafe partial int GetKeyboardLayoutList(int nBuff, nint* lpList);

    [LibraryImport("user32.dll")]
    public static partial nint ActivateKeyboardLayout(nint hkl, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "GetKeyboardLayoutNameW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool GetKeyboardLayoutName(char* pwszKLID);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(nint hWnd, uint msg, nuint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "MapVirtualKeyExW")]
    public static partial uint MapVirtualKeyEx(uint uCode, uint uMapType, nint dwhkl);

    [LibraryImport("user32.dll")]
    public static unsafe partial int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte* lpKeyState, char* pwszBuff, int cchBuff, uint wFlags, nint dwhkl);
}
