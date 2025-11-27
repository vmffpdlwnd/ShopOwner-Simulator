// Services/Implementations/PlayFabService.cs
using System.Text;
using System.Text.Json;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class PlayFabService : IPlayFabService
{
    private readonly HttpClient _httpClient;
    private readonly string _titleId;
    private readonly string _secretKey;
    private const string PlayFabApiUrl = "https://yourtitleid.playfabapi.com";

    public PlayFabService(string titleId, string secretKey)
    {
        _titleId = titleId;
        _secretKey = secretKey;
        _httpClient = new HttpClient();
    }

    private async Task<T> CallPlayFabApiAsync<T>(string functionName, object payload)
    {
        try
        {
            var url = $"{PlayFabApiUrl}/Server/{functionName}";
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

        var response = await CallPlayFabApiAsync<Player>("CreateOpenIdConnection", payload);
        return response;
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
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
        var payload = new { PlayFabId = playerId };
        var response = await CallPlayFabApiAsync<List<Mercenary>>("GetUserData", payload);
        return response ?? new List<Mercenary>();
    }

    public async Task<bool> UpdateMercenaryAsync(Mercenary mercenary)
    {
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
        var payload = new { PlayFabId = playerId };
        var response = await CallPlayFabApiAsync<List<InventoryItem>>("GetUserInventory", payload);
        return response ?? new List<InventoryItem>();
    }

    public async Task<bool> UpdateInventoryAsync(string playerId, InventoryItem item)
    {
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