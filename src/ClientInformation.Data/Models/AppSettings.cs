namespace ClientInformation.Data.Models;

public class AppSettings
{
    public bool     StartMainAppWithWindows { get; set; } = false;
    public bool     MinimizeToTray          { get; set; } = true;
    public string   LastStatusMessage       { get; set; } = string.Empty;
    public DateTime LastRunUtc              { get; set; } = DateTime.UtcNow;

    // EPM-Konfiguration
    public string EpmCoreHost    { get; set; } = "WU-EPMCORE.wu.ssn";
    public int    EpmCorePort    { get; set; } = 443;
    public string EpmServiceName { get; set; } = "CBA8";
}
