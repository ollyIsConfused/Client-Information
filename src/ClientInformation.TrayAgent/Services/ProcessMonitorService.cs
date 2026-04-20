using System.Diagnostics;
using ClientInformation.Data.Services;
using ClientInformation.Shared.Constants;

namespace ClientInformation.TrayAgent.Services;

public class ProcessMonitorService : IDisposable
{
    public event Action<bool>? StatusChanged;

    private readonly LoggingService _logger;
    private readonly System.Threading.Timer _timer;
    private bool _lastKnownState;

    public ProcessMonitorService(LoggingService logger)
    {
        _logger = logger;
        _timer  = new System.Threading.Timer(Check, null, Timeout.Infinite, Timeout.Infinite);
    }

    public static bool IsMainAppRunning()
        => Process.GetProcessesByName(AppConstants.AppProcessName).Length > 0;

    public void Start() => _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(10));
    public void Stop()  => _timer.Change(Timeout.Infinite, Timeout.Infinite);

    private void Check(object? state)
    {
        var running = IsMainAppRunning();
        if (running == _lastKnownState) return;

        _lastKnownState = running;
        _logger.Info($"App-Status geändert: {(running ? "läuft" : "gestoppt")}");
        StatusChanged?.Invoke(running);
    }

    public void Dispose() => _timer.Dispose();
}
