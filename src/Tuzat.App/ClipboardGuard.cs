using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal sealed record ClipboardItem(uint Format, byte[] Data);

internal sealed record ClipboardSnapshot(IReadOnlyList<ClipboardItem> Items);

internal sealed class ClipboardGuard : IDisposable
{
    private const int OpenAttempts = 5;
    private const int OpenRetryDelayMs = 20;

    private static readonly byte[] DwordZero = new byte[4];

    // Win+V history, cloud sync and clipboard managers skip content that carries these formats.
    private static readonly uint[] PrivacyFormats =
    [
        RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing"),
        RegisterClipboardFormat("CanIncludeInClipboardHistory"),
        RegisterClipboardFormat("CanUploadToCloudClipboard"),
    ];

    // SetClipboardData fails after EmptyClipboard unless the clipboard was opened with an owner window.
    private readonly NativeWindow _owner = new();

    public ClipboardGuard() => _owner.CreateHandle(new CreateParams { Parent = HWND_MESSAGE });

    public uint SequenceNumber => GetClipboardSequenceNumber();

    public bool TrySave([NotNullWhen(true)] out ClipboardSnapshot? snapshot)
    {
        snapshot = null;
        if (!TryOpen()) return false;
        try
        {
            var items = new List<ClipboardItem>();
            for (uint format = EnumClipboardFormats(0); format != 0; format = EnumClipboardFormats(format))
            {
                if (!IsGlobalMemoryFormat(format) || PrivacyFormats.Contains(format)) continue;
                if (ReadGlobal(GetClipboardData(format)) is { } data) items.Add(new ClipboardItem(format, data));
            }

            snapshot = new ClipboardSnapshot(items);
            return true;
        }
        finally
        {
            CloseClipboard();
        }
    }

    public bool TryReadText([NotNullWhen(true)] out string? text)
    {
        text = null;
        if (!TryOpen()) return false;
        try
        {
            nint handle = GetClipboardData(CF_UNICODETEXT);
            if (handle == 0) return false;
            nint pointer = GlobalLock(handle);
            if (pointer == 0) return false;
            try
            {
                string raw = Marshal.PtrToStringUni(pointer, (int)(GlobalSize(handle) / 2));
                int end = raw.IndexOf('\0');
                text = end >= 0 ? raw[..end] : raw;
                return true;
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    public bool TrySetText(string text) =>
        TryReplace([new ClipboardItem(CF_UNICODETEXT, Encoding.Unicode.GetBytes(text + '\0'))]);

    public bool TryRestore(ClipboardSnapshot snapshot) => TryReplace(snapshot.Items);

    public void Dispose() => _owner.DestroyHandle();

    private bool TryReplace(IReadOnlyList<ClipboardItem> items)
    {
        if (!TryOpen()) return false;
        try
        {
            if (!EmptyClipboard()) return false;
            bool complete = true;
            foreach (var item in items) complete &= TryWrite(item.Format, item.Data);
            foreach (var format in PrivacyFormats) TryWrite(format, DwordZero);
            return complete;
        }
        finally
        {
            CloseClipboard();
        }
    }

    private bool TryOpen()
    {
        for (int attempt = 1; ; attempt++)
        {
            if (OpenClipboard(_owner.Handle)) return true;
            if (attempt == OpenAttempts) return false;
            ThreadWait.Sleep(OpenRetryDelayMs);
        }
    }

    // GDI handles and owner-managed formats cannot be copied as plain memory; CF_DIB still keeps images.
    private static bool IsGlobalMemoryFormat(uint format) => format switch
    {
        2 or 3 or 9 or 14 => false,       // CF_BITMAP, CF_METAFILEPICT, CF_PALETTE, CF_ENHMETAFILE
        >= 0x80 and <= 0x8E => false,     // CF_OWNERDISPLAY and CF_DSP* formats
        >= 0x200 and <= 0x3FF => false,   // CF_PRIVATEFIRST..CF_GDIOBJLAST
        _ => true,
    };

    private static byte[]? ReadGlobal(nint handle)
    {
        if (handle == 0) return null;
        int size = (int)GlobalSize(handle);
        if (size == 0) return null;
        nint pointer = GlobalLock(handle);
        if (pointer == 0) return null;
        try
        {
            var data = new byte[size];
            Marshal.Copy(pointer, data, 0, size);
            return data;
        }
        finally
        {
            GlobalUnlock(handle);
        }
    }

    private static bool TryWrite(uint format, byte[] data)
    {
        nint handle = GlobalAlloc(GMEM_MOVEABLE, (nuint)Math.Max(data.Length, 1));
        if (handle == 0) return false;
        nint pointer = GlobalLock(handle);
        if (pointer == 0)
        {
            GlobalFree(handle);
            return false;
        }

        Marshal.Copy(data, 0, pointer, data.Length);
        GlobalUnlock(handle);
        if (SetClipboardData(format, handle) != 0) return true; // the clipboard now owns the memory

        GlobalFree(handle);
        return false;
    }
}
