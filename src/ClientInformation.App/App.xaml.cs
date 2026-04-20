using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using ClientInformation.Data.Services;
using ClientInformation.Shared.Constants;
using ClientInformation.Shared.Helpers;

namespace ClientInformation.App;

public partial class App : Application
{
    private static Mutex?  _mutex;
    private LoggingService? _logger;

    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
    private const int SW_RESTORE = 9;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, AppConstants.AppMutexName, out var isNewInstance);

        if (!isNewInstance)
        {
            // Bereits laufende Instanz in den Vordergrund holen
            BringExistingInstanceToFront();
            _mutex.Dispose();
            Shutdown();
            return;
        }

        AppPaths.EnsureDirectoriesExist();
        _logger = new LoggingService();
        _logger.Info("Haupt-App gestartet.");

        base.OnStartup(e);

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Info("Haupt-App beendet.");
        try   { _mutex?.ReleaseMutex(); }
        catch { /* Mutex war schon freigegeben */ }
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private static void BringExistingInstanceToFront()
    {
        // Desktop-Widget hat kein normales Fenster — nichts zu tun
    }
}

