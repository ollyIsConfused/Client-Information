using ClientInformation.Data.Services;
using ClientInformation.Shared.Constants;
using Microsoft.Win32;

namespace ClientInformation.TrayAgent.Services;

public class AutostartService
{
    private readonly LoggingService _logger;

    public AutostartService(LoggingService logger)
    {
        _logger = logger;
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryKeyPath, writable: false);
        return key?.GetValue(AppConstants.AutostartValueName) is not null;
    }

    public void Enable()
    {
        var exe = Path.Combine(AppContext.BaseDirectory, "ClientInformation.TrayAgent.exe");
        using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryKeyPath, writable: true);
        key?.SetValue(AppConstants.AutostartValueName, $"\"{exe}\"");
        _logger.Info($"Autostart aktiviert: {exe}");
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryKeyPath, writable: true);
        key?.DeleteValue(AppConstants.AutostartValueName, throwOnMissingValue: false);
        _logger.Info("Autostart deaktiviert.");
    }
}
