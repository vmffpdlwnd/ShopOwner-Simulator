// Services/Implementations/StorageService.cs
using System.Text.Json;

namespace ShopOwnerSimulator.Services.Implementations;

public class StorageService : IStorageService
{
    private readonly Dictionary<string, string> _cache = new();
    private const string StoragePrefix = "shop_simulator_";

    public async Task<T> GetAsync<T>(string key)
    {
        var fullKey = $"{StoragePrefix}{key}";
        
        if (_cache.TryGetValue(fullKey, out var value))
        {
            return JsonSerializer.Deserialize<T>(value);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var fullKey = $"{StoragePrefix}{key}";
        var json = JsonSerializer.Serialize(value);
        _cache[fullKey] = json;
    }

    public async Task RemoveAsync(string key)
    {
        var fullKey = $"{StoragePrefix}{key}";
        _cache.Remove(fullKey);
    }

    public async Task ClearAsync()
    {
        _cache.Clear();
    }
}