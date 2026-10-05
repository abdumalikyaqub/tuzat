using System.ComponentModel;
using Tuzat.Core;

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
                Localized.Text(
                    $"Cannot install the keyboard hook: {e.Message} (code {e.NativeErrorCode}).",
                    $"Не удалось установить перехват клавиатуры: {e.Message} (код {e.NativeErrorCode})."),
                "Tuzat",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
