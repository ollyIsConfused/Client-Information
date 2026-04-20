namespace ClientInformation.Shared.Helpers;

public static class AppPaths
{
    public static string AppDataRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClientInformation");

    public static string SettingsFile  => Path.Combine(AppDataRoot, "settings.json");
    public static string ClientsFile   => Path.Combine(AppDataRoot, "clients.json");
    public static string LogsDirectory => Path.Combine(AppDataRoot, "logs");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(LogsDirectory);
    }
}
