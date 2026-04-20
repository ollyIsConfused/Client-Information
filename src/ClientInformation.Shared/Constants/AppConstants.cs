namespace ClientInformation.Shared.Constants;

public static class AppConstants
{
    public const string AppName           = "ClientInformation";
    public const string AppDisplayName    = "Client Information";
    public const string AppVersion        = "1.0.0";

    public const string AppMutexName      = "ClientInformation.App.SingleInstance";
    public const string TrayMutexName     = "ClientInformation.TrayAgent.SingleInstance";

    public const string AppProcessName    = "ClientInformation.App";
    public const string AppExecutableName = "ClientInformation.App.exe";

    public const string RegistryKeyPath   = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    public const string AutostartValueName = "ClientInformationTrayAgent";
}
