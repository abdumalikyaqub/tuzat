using System.Diagnostics;
using Microsoft.Win32;
using Tuzat.Core;

namespace Tuzat.App;

internal sealed class TrayApp : ApplicationContext
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tuzat", "settings.json");

    private readonly WindowsFormsSynchronizationContext _ui = new();
    private readonly Icon _activeIcon = TrayIcons.Create(paused: false);
    private readonly Icon _pausedIcon = TrayIcons.Create(paused: true);
    private readonly InputSender _input = new();
    private readonly ClipboardGuard _clipboard = new();
    private readonly InputHooks _hooks;
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _autostartItem;
    private TuzatSettings _settings = TuzatSettings.Default;
    private LayoutSwitcher? _layouts;

    public TrayApp()
    {
        // Hooks start first: if they fail, no tray icon is left behind.
        _hooks = new InputHooks((action, strokes) => _ui.Post(_ => Run(action, strokes), null));
        _hooks.Start();

        _pauseItem = new ToolStripMenuItem("Пауза", null, (_, _) => SetPaused(!_hooks.Paused));
        _autostartItem = new ToolStripMenuItem("Автозапуск", null, (_, _) => ToggleAutostart()) { Checked = Autostart.IsEnabled };
        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[]
        {
            _pauseItem,
            _autostartItem,
            new ToolStripSeparator(),
            new ToolStripMenuItem("Открыть настройки", null, (_, _) => OpenSettings()),
            new ToolStripMenuItem("Перечитать настройки", null, (_, _) => ApplySettings()),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Выход", null, (_, _) => ExitThread()),
        });
        _tray = new NotifyIcon { Icon = _activeIcon, Text = "Tuzat", ContextMenuStrip = menu, Visible = true };

        ApplySettings();

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    private void Run(HotkeyAction action, KeyStroke[] strokes)
    {
        if (_layouts is null) return;

        switch (action)
        {
            case HotkeyAction.FixLastWord:
                FixLastWordAction.Run(strokes, _layouts, _input);
                break;
            case HotkeyAction.FixSelection:
                FixSelectionAction.Run(_layouts, _input, _clipboard, _settings.ClipboardRestoreDelayMs);
                break;
        }
    }

    private void ApplySettings()
    {
        var result = SettingsLoader.LoadOrCreate(SettingsPath);
        var errors = result.Errors.ToList();
        _settings = result.Settings;
        _layouts = LayoutSwitcher.TryCreate(_settings.LayoutA, _settings.LayoutB, out var layoutError);
        if (layoutError is not null) errors.Add(layoutError);

        _hooks.SetBindings(_layouts is null ? null : _settings.Bindings);
        if (errors.Count > 0)
        {
            _tray.ShowBalloonTip(10_000, "Tuzat: настройки", string.Join(Environment.NewLine, errors), ToolTipIcon.Warning);
        }
    }

    private void SetPaused(bool paused)
    {
        _hooks.Paused = paused;
        _pauseItem.Checked = paused;
        _tray.Icon = paused ? _pausedIcon : _activeIcon;
        _tray.Text = paused ? "Tuzat — пауза" : "Tuzat";
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

    // Windows can drop low-level hooks after sleep or a locked session.
    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume) _hooks.Reinstall();
    }

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionUnlock) _hooks.Reinstall();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _hooks.Dispose();
            _clipboard.Dispose();
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
            _activeIcon.Dispose();
            _pausedIcon.Dispose();
            _ui.Dispose();
        }

        base.Dispose(disposing);
    }
}
