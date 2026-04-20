using ClientInformation.Shared.Helpers;

namespace ClientInformation.Data.Services;

public class LoggingService
{
    private static readonly object _lock = new();
    private readonly string _logFile;

    public LoggingService()
    {
        AppPaths.EnsureDirectoriesExist();
        _logFile = Path.Combine(AppPaths.LogsDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
    }

    public void Info(string message)    => Write(message, "INFO");
    public void Warning(string message) => Write(message, "WARN");

    public void Error(string message, Exception? ex = null)
        => Write(ex is not null ? $"{message} — {ex.GetType().Name}: {ex.Message}" : message, "ERROR");

    private void Write(string message, string level)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_lock)
        {
            try { File.AppendAllText(_logFile, entry + Environment.NewLine); }
            catch { /* best effort — nie abstürzen wegen Logging */ }
        }
    }
}
