using Tuzat.Core;

namespace Tuzat.App;

internal static class CrashLog
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tuzat", "crash.log");

    public static void Install()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Fail(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Fail(e.ExceptionObject as Exception);
    }

    private static void Fail(Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            // Only type and stack trace: an exception message could carry user text.
            File.AppendAllText(
                LogPath,
                $"{DateTime.Now:O} {exception?.GetType().FullName}{Environment.NewLine}{exception?.StackTrace}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // The message box below is still shown; a missing log must not hide the crash.
        }

        MessageBox.Show(
            Localized.Text(
                $"Tuzat stopped because of an error.{Environment.NewLine}Details: {LogPath}",
                $"Tuzat остановлен из-за ошибки.{Environment.NewLine}Подробности: {LogPath}"),
            "Tuzat",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        Environment.Exit(1);
    }
}
