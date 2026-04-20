using ClientInformation.Data.Models;
using ClientInformation.Shared.Helpers;

namespace ClientInformation.Data.Services;

public class ClientDataService
{
    private readonly JsonStorageService _storage;

    public ClientDataService(JsonStorageService storage)
    {
        _storage = storage;
    }

    public async Task<List<ClientEntry>> GetAllAsync()
        => await _storage.LoadAsync<List<ClientEntry>>(AppPaths.ClientsFile) ?? [];

    public async Task SaveAllAsync(List<ClientEntry> clients)
        => await _storage.SaveAsync(AppPaths.ClientsFile, clients);

    public async Task UpsertAsync(ClientEntry client)
    {
        var clients = await GetAllAsync();
        var index = clients.FindIndex(c => c.Id == client.Id);
        if (index >= 0) clients[index] = client;
        else clients.Add(client);
        await SaveAllAsync(clients);
    }

    public async Task DeleteAsync(Guid id)
    {
        var clients = await GetAllAsync();
        clients.RemoveAll(c => c.Id == id);
        await SaveAllAsync(clients);
    }
}
