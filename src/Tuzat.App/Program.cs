using System.ComponentModel;

namespace Tuzat.App;

internal static class Program
{
    public const string MutexName = "Tuzat.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
        if (!createdNew) return;

        CrashLog.Install();
        ApplicationConfiguration.Initialize();
        try
        {
            using var app = new TrayApp();
            Application.Run(app);
        }
        catch (Win32Exception e)
        {
            MessageBox.Show(
                $"Не удалось установить перехват клавиатуры: {e.Message} (код {e.NativeErrorCode}).",
                "Tuzat",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
