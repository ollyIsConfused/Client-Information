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

    // HKLM statt HKCU: Der Eintrag gilt maschinenweit für jeden Benutzer, der sich
    // anmeldet — nicht nur für das Konto, unter dem er gesetzt wurde. Schreibzugriff
    // auf HKLM erfordert Administrator-Rechte.
    public bool IsEnabled()
    {
        using var key = Registry.LocalMachine.OpenSubKey(AppConstants.RegistryKeyPath, writable: false);
        return key?.GetValue(AppConstants.AutostartValueName) is not null;
    }

    public void Enable()
    {
        var exe = Path.Combine(AppContext.BaseDirectory, "ClientInformation.TrayAgent.exe");
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(AppConstants.RegistryKeyPath, writable: true);
            key?.SetValue(AppConstants.AutostartValueName, $"\"{exe}\"");
            _logger.Info($"Autostart aktiviert (maschinenweit, alle Benutzer): {exe}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error("Autostart konnte nicht aktiviert werden — Admin-Rechte erforderlich", ex);
            ShowAdminRequiredMessage();
        }
    }

    public void Disable()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(AppConstants.RegistryKeyPath, writable: true);
            key?.DeleteValue(AppConstants.AutostartValueName, throwOnMissingValue: false);
            _logger.Info("Autostart deaktiviert (maschinenweit).");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error("Autostart konnte nicht deaktiviert werden — Admin-Rechte erforderlich", ex);
            ShowAdminRequiredMessage();
        }
    }

    private static void ShowAdminRequiredMessage()
        => MessageBox.Show(
            "Der Autostart gilt für alle Benutzer dieses Notebooks und kann daher nur mit " +
            "Administrator-Rechten geändert werden.\n\nBitte den TrayAgent einmalig per " +
            "Rechtsklick → \"Als Administrator ausführen\" starten, oder den Installer erneut ausführen.",
            "Administrator-Rechte erforderlich",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
}
