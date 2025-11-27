// Services/Implementations/PlayFabService.cs
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class PlayFabService : IPlayFabService
{
    private readonly HttpClient _httpClient;
    private readonly string? _titleId;
    private readonly string? _secretKey;
    private readonly bool _enabled;

    public PlayFabService(string? titleId, string? secretKey)
    {
        _titleId = titleId;
        _secretKey = secretKey;
        _httpClient = new HttpClient();

        // Consider PlayFab disabled when configuration contains placeholders or is empty
        _enabled = !string.IsNullOrWhiteSpace(_titleId)
                   && !string.IsNullOrWhiteSpace(_secretKey)
                   && !_titleId!.Contains("${")
                   && !_titleId!.Contains("your");

        if (!_enabled)
        {
            Console.Error.WriteLine("PlayFabService: PlayFab disabled due to missing or placeholder configuration.");
        }
    }

    private async Task<T> CallPlayFabApiAsync<T>(string functionName, object payload)
    {
        try
        {
            if (!_enabled)
            {
                Console.Error.WriteLine($"PlayFabService: Skipping API call '{functionName}' because PlayFab is disabled.");
                return default!;
            }

            var apiRoot = $"https://{_titleId}.playfabapi.com";
            var url = $"{apiRoot}/Server/{functionName}";
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-SecretKey", _secretKey);

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(jsonResponse);

            if (result == null)
                return default!;

            // Data is nullable in ApiResponse<T>; callers expect T or will handle nulls.
            return result.Data!;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"PlayFab API Error: {ex.Message}");
            throw;
        }
    }

    public async Task<Player> GetPlayerAsync(string playerId)
    {
        if (!_enabled)
        {
            // Return a local stub player for offline/local development
            return await Task.FromResult(new Player
            {
                Id = string.IsNullOrWhiteSpace(playerId) ? Guid.NewGuid().ToString() : playerId,
                Username = "LocalPlayer",
                Level = 1,
                Experience = 0,
                Gold = 1000,
                Crystal = 0,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            });
        }

        var response = await CallPlayFabApiAsync<Player>("GetUserData", new { PlayFabId = playerId });
        return response ?? throw new Exception("Player not found");
    }

    public async Task<Player> CreatePlayerAsync(string username)
    {
        var payload = new
        {
            DisplayName = username,
            TitleId = _titleId
        };
        if (!_enabled)
        {
            return await Task.FromResult(new Player
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Level = 1,
                Experience = 0,
                Gold = 1000,
                Crystal = 0,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            });
        }

        var response = await CallPlayFabApiAsync<Player>("CreateOpenIdConnection", payload);
        return response;
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
        if (!_enabled)
            return await Task.FromResult(true);

        var payload = new
        {
            PlayFabId = player.Id,
            Data = new
            {
                Username = player.Username,
                Level = player.Level,
                Experience = player.Experience,
                Gold = player.Gold
            }
        };

        await CallPlayFabApiAsync<object>("UpdateUserData", payload);
        return true;
    }

    public async Task<bool> UpdatePlayerGoldAsync(string playerId, long goldAmount)
    {
        if (!_enabled)
            return await Task.FromResult(true);

        var payload = new
        {
            PlayFabId = playerId,
            ChangeValue = goldAmount
        };

        await CallPlayFabApiAsync<object>("AddUserVirtualCurrency", payload);
        return true;
    }

        public async Task<List<Mercenary>> GetMercenariesAsync(string playerId)
    {
            if (!_enabled)
            {
                return await Task.FromResult(new List<Mercenary>());
            }

            var payload = new { PlayFabId = playerId };
            var response = await CallPlayFabApiAsync<List<Mercenary>>("GetUserData", payload);
            return response ?? new List<Mercenary>();
    }

    public async Task<bool> UpdateMercenaryAsync(Mercenary mercenary)
    {
        if (!_enabled)
            return await Task.FromResult(true);

        var payload = new
        {
            PlayFabId = mercenary.PlayerId,
            MercenaryId = mercenary.Id,
            Data = new
            {
                Level = mercenary.Level,
                Experience = mercenary.Experience,
                Stats = mercenary.Stats
            }
        };

        await CallPlayFabApiAsync<object>("UpdateUserData", payload);
        return true;
    }

    public async Task<List<InventoryItem>> GetInventoryAsync(string playerId)
    {
        if (!_enabled)
        {
            return await Task.FromResult(new List<InventoryItem>());
        }

        var payload = new { PlayFabId = playerId };
        var response = await CallPlayFabApiAsync<List<InventoryItem>>("GetUserInventory", payload);
        return response ?? new List<InventoryItem>();
    }

    public async Task<bool> UpdateInventoryAsync(string playerId, InventoryItem item)
    {
        if (!_enabled)
            return await Task.FromResult(true);

        var payload = new
        {
            PlayFabId = playerId,
            ItemId = item.Id,
            Data = new
            {
                Quantity = item.Quantity,
                IsEquipped = item.IsEquipped
            }
        };

        await CallPlayFabApiAsync<object>("UpdateUserData", payload);
        return true;
    }

    private class ApiResponse<T>
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public T? Data { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public int Code { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}