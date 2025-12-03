// Services/Implementations/StorageService.cs
using System.Text.Json;
using Microsoft.JSInterop;

namespace ShopOwnerSimulator.Services.Implementations;

public class StorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, string> _memoryCache = new();
    private const string StoragePrefix = "shop_simulator_";

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private string FullKey(string key) => $"{StoragePrefix}{key}";

    public async Task<T> GetAsync<T>(string key)
    {
        var fullKey = FullKey(key);

        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("ShopOwnerSimulator.getLocalStorage", fullKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        catch
        {
            // Fall back to in-memory cache when JS interop isn't available
            if (_memoryCache.TryGetValue(fullKey, out var cached))
            {
                return JsonSerializer.Deserialize<T>(cached);
            }
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var fullKey = FullKey(key);
        var json = JsonSerializer.Serialize(value);

        try
        {
            await _jsRuntime.InvokeVoidAsync("ShopOwnerSimulator.setLocalStorage", fullKey, json);
        }
        catch
        {
            // Fall back to in-memory cache when JS interop isn't available
            _memoryCache[fullKey] = json;
        }
    }

    public async Task RemoveAsync(string key)
    {
        var fullKey = FullKey(key);

        try
        {
            await _jsRuntime.InvokeVoidAsync("ShopOwnerSimulator.removeLocalStorage", fullKey);
        }
        catch
        {
            _memoryCache.Remove(fullKey);
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("ShopOwnerSimulator.clearLocalStorage");
            _memoryCache.Clear();
        }
        catch
        {
            _memoryCache.Clear();
        }
    }
}