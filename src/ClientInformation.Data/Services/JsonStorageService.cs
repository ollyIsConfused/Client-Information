using System.Text.Json;

namespace ClientInformation.Data.Services;

public class JsonStorageService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task SaveAsync<T>(string filePath, T data)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(data, Options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}
