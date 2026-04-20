using ClientInformation.Data.Models;
using ClientInformation.Shared.Helpers;

namespace ClientInformation.Data.Services;

public class SettingsService
{
    private readonly JsonStorageService _storage;

    public SettingsService(JsonStorageService storage)
    {
        _storage = storage;
    }

    public async Task<AppSettings> LoadAsync()
        => await _storage.LoadAsync<AppSettings>(AppPaths.SettingsFile) ?? new AppSettings();

    public async Task SaveAsync(AppSettings settings)
    {
        settings.LastRunUtc = DateTime.UtcNow;
        await _storage.SaveAsync(AppPaths.SettingsFile, settings);
    }
}
