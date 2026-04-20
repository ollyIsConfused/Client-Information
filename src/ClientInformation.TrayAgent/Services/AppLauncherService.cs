using System.Diagnostics;
using System.Runtime.InteropServices;
using ClientInformation.Data.Services;
using ClientInformation.Shared.Constants;

namespace ClientInformation.TrayAgent.Services;

public class AppLauncherService
{
    private readonly LoggingService _logger;

    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
    private const int SW_RESTORE = 9;

    public AppLauncherService(LoggingService logger)
    {
        _logger = logger;
    }

    private string GetAppExePath()
        => Path.Combine(AppContext.BaseDirectory, AppConstants.AppExecutableName);

    /// <summary>Startet die App oder bringt sie in den Vordergrund.</summary>
    public void OpenOrFocus()
    {
        if (ProcessMonitorService.IsMainAppRunning())
        {
            BringToFront();
            return;
        }
        Launch();
    }

    /// <summary>Startet die App (kein Fokus-Versuch falls schon läuft).</summary>
    public void Launch()
    {
        if (ProcessMonitorService.IsMainAppRunning())
        {
            _logger.Info("App läuft bereits — kein neuer Start.");
            return;
        }

        var exe = GetAppExePath();
        if (!File.Exists(exe))
        {
            _logger.Error($"App-Executable nicht gefunden: {exe}");
            MessageBox.Show(
                $"ClientInformation.App.exe nicht gefunden:\n{exe}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            Process.Start(exe);
            _logger.Info($"App gestartet: {exe}");
        }
        catch (Exception ex)
        {
            _logger.Error("Fehler beim Starten der App", ex);
        }
    }

    /// <summary>Beendet die App und startet sie neu.</summary>
    public void Restart()
    {
        var processes = Process.GetProcessesByName(AppConstants.AppProcessName);
        foreach (var p in processes)
        {
            try
            {
                p.Kill();
                p.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Beenden für Neustart", ex);
            }
        }

        _logger.Info("App-Prozess für Neustart beendet.");
        Task.Delay(600).ContinueWith(_ => Launch());
    }

    /// <summary>Beendet die App-Prozesse hart.</summary>
    public void KillApp()
    {
        var processes = Process.GetProcessesByName(AppConstants.AppProcessName);
        foreach (var p in processes)
        {
            try { p.Kill(); }
            catch (Exception ex) { _logger.Error("Fehler beim Beenden der App", ex); }
        }
        _logger.Info("App-Prozess beendet.");
    }

    private void BringToFront()
    {
        var processes = Process.GetProcessesByName(AppConstants.AppProcessName);
        if (processes.Length == 0) return;

        var hwnd = processes[0].MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;

        if (IsIconic(hwnd)) ShowWindow(hwnd, SW_RESTORE);
        SetForegroundWindow(hwnd);
    }
}
