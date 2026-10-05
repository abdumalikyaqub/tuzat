using System.Diagnostics;
using Tuzat.Core;

namespace Tuzat.App;

internal sealed class TrayApp : ApplicationContext
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tuzat", "settings.json");

    private readonly Icon _activeIcon = TrayIcons.Create(paused: false);
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _autostartItem;

    public TrayApp()
    {
        _autostartItem = new ToolStripMenuItem("Автозапуск", null, (_, _) => ToggleAutostart()) { Checked = Autostart.IsEnabled };
        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[]
        {
            _autostartItem,
            new ToolStripSeparator(),
            new ToolStripMenuItem("Открыть настройки", null, (_, _) => OpenSettings()),
            new ToolStripMenuItem("Перечитать настройки", null, (_, _) => ApplySettings()),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Выход", null, (_, _) => ExitThread()),
        });
        _tray = new NotifyIcon { Icon = _activeIcon, Text = "Tuzat", ContextMenuStrip = menu, Visible = true };

        ApplySettings();
    }

    private void ApplySettings()
    {
        var errors = SettingsLoader.LoadOrCreate(SettingsPath).Errors;
        if (errors.Count == 0) return;
        _tray.ShowBalloonTip(10_000, "Tuzat: настройки", string.Join(Environment.NewLine, errors), ToolTipIcon.Warning);
    }

    private void ToggleAutostart()
    {
        try
        {
            Autostart.Set(!_autostartItem.Checked);
            _autostartItem.Checked = Autostart.IsEnabled;
        }
        catch (Exception e) when (e is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            _tray.ShowBalloonTip(5_000, "Tuzat", $"Не удалось изменить автозапуск: {e.Message}", ToolTipIcon.Error);
        }
    }

    private static void OpenSettings()
    {
        SettingsLoader.LoadOrCreate(SettingsPath);
        using var _ = Process.Start("notepad.exe", $"\"{SettingsPath}\"");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
            _activeIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
