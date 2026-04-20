using ClientInformation.Data.Models;
using ClientInformation.Data.Services;
using ClientInformation.Shared.Constants;
using ClientInformation.Shared.Helpers;
using ClientInformation.TrayAgent.Services;

namespace ClientInformation.TrayAgent;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon          _trayIcon;
    private readonly AppLauncherService  _launcher;
    private readonly ProcessMonitorService _monitor;
    private readonly AutostartService    _autostart;
    private readonly SettingsService     _settings;
    private readonly LoggingService      _logger;

    private ToolStripMenuItem _statusItem    = null!;
    private ToolStripMenuItem _autostartItem = null!;
    private AppSettings       _currentSettings = new();

    public TrayApplicationContext()
    {
        AppPaths.EnsureDirectoriesExist();

        var storage  = new JsonStorageService();
        _logger    = new LoggingService();
        _settings  = new SettingsService(storage);
        _launcher  = new AppLauncherService(_logger);
        _monitor   = new ProcessMonitorService(_logger);
        _autostart = new AutostartService(_logger);

        _trayIcon = new NotifyIcon
        {
            Text    = AppConstants.AppDisplayName,
            Icon    = LoadIcon(),
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        _trayIcon.DoubleClick += (_, _) => _launcher.OpenOrFocus();

        _monitor.StatusChanged += OnStatusChanged;
        _monitor.Start();

        // Settings asynchron laden und ggf. App starten
        _ = LoadSettingsAndApplyAsync();

        _logger.Info("TrayAgent gestartet.");
    }

    // ── Icon ──────────────────────────────────────────────────────────────────

    private static Icon LoadIcon()
    {
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "Client.ico"),
            Path.Combine(AppContext.BaseDirectory, "Client.ico")
        };
        foreach (var p in paths)
            if (File.Exists(p)) return new Icon(p);

        return SystemIcons.Application;
    }

    // ── Kontextmenü ───────────────────────────────────────────────────────────

    private ContextMenuStrip BuildContextMenu()
    {
        _statusItem    = new ToolStripMenuItem("Status: Unbekannt") { Enabled = false };
        _autostartItem = new ToolStripMenuItem("Autostart aktivieren", null, OnToggleAutostart);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Öffnen / Fokussieren", null, (_, _) => _launcher.OpenOrFocus());
        menu.Items.Add("Starten",               null, (_, _) => _launcher.Launch());
        menu.Items.Add("Neu starten",           null, (_, _) => _launcher.Restart());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_autostartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("App beenden",           null, (_, _) => _launcher.KillApp());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("TrayAgent beenden",     null, OnExit);

        menu.Opening += (_, _) => RefreshMenuState();
        return menu;
    }

    private void RefreshMenuState()
    {
        var running = ProcessMonitorService.IsMainAppRunning();

        _statusItem.Text    = running ? "● App läuft" : "○ App gestoppt";
        _autostartItem.Text = _autostart.IsEnabled()
            ? "Autostart deaktivieren"
            : "Autostart aktivieren";
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void OnStatusChanged(bool isRunning)
    {
        // Thread-sicherer Aufruf, da Timer-Thread
        _trayIcon.Text = isRunning
            ? $"{AppConstants.AppDisplayName} — läuft"
            : $"{AppConstants.AppDisplayName} — gestoppt";
    }

    private void OnToggleAutostart(object? sender, EventArgs e)
    {
        if (_autostart.IsEnabled()) _autostart.Disable();
        else                        _autostart.Enable();
    }

    private async Task LoadSettingsAndApplyAsync()
    {
        try
        {
            _currentSettings = await _settings.LoadAsync();
            if (_currentSettings.StartMainAppWithWindows)
            {
                _logger.Info("StartMainAppWithWindows aktiv — starte App.");
                _launcher.Launch();
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Fehler beim Laden der Settings", ex);
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _logger.Info("TrayAgent wird beendet.");
        _monitor.Stop();
        _trayIcon.Visible = false;
        Application.Exit();
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _monitor.Dispose();
            _trayIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
