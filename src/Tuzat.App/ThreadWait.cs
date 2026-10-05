using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

internal static class ThreadWait
{
    // Thread.Sleep would block cross-thread SendMessage calls, such as clipboard owner notifications.
    public static void Sleep(int milliseconds)
    {
        long deadline = Environment.TickCount64 + milliseconds;
        for (long left = milliseconds; left > 0; left = deadline - Environment.TickCount64)
        {
            MsgWaitForMultipleObjectsEx(0, 0, (uint)left, QS_SENDMESSAGE, 0);
            PeekMessage(out _, 0, 0, 0, PM_NOREMOVE | PM_QS_SENDMESSAGE);
        }
    }
}
