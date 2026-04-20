namespace ClientInformation.Shared.Models;

public class AppStatusInfo
{
    public bool     IsMainAppRunning { get; set; }
    public string   StatusMessage    { get; set; } = string.Empty;
    public DateTime LastChecked      { get; set; } = DateTime.UtcNow;
}
