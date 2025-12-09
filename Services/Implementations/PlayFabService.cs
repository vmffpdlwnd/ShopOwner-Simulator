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
    private string _sessionTicket;

    public PlayFabService(string titleId, string secretKey)
    {
        _titleId = titleId ?? "EC0FE";
        _secretKey = secretKey ?? "5W7MEBQ5SFA68FA35KPTG6W4JT5I67AHEQDK8TCCM4TJ3TH8U8";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-PlayFabSDK", "CSharpSDK-1.0");
    }

    // 로그인 (아이디/비밀번호)
    public async Task<string> LoginWithEmailAsync(string email, string password)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/LoginWithEmailAddress";
        var payload = new
        {
            TitleId = _titleId,
            Email = email,
            Password = password
        };

        try
        {
            var response = await PostAsync<LoginResult>(url, payload, false);
            _sessionTicket = response.SessionTicket;
            return response.PlayFabId;
        }
        catch (Exception ex)
        {
            throw new Exception("로그인 실패", ex);
        }
    }

    // 회원가입
    public async Task<string> RegisterWithEmailAsync(string email, string password, string username)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/RegisterPlayFabUser";
        var payload = new
        {
            TitleId = _titleId,
            Email = email,
            Password = password,
            Username = username,
            RequireBothUsernameAndEmail = false
        };

        try
        {
            var response = await PostAsync<LoginResult>(url, payload, false);
            _sessionTicket = response.SessionTicket;
            return response.PlayFabId;
        }
        catch (PlayFabApiException pex)
        {
            // Handle common PlayFab errors with user-friendly messages
            if (string.Equals(pex.Error, "UsernameNotAvailable", StringComparison.OrdinalIgnoreCase) || pex.ErrorCode == 1009)
            {
                throw new Exception("사용자 이름이 이미 사용 중입니다.", pex);
            }
            if (string.Equals(pex.Error, "InvalidParams", StringComparison.OrdinalIgnoreCase) || pex.ErrorCode == 1000)
            {
                throw new Exception($"회원가입 실패: {pex.PlayFabMessage}", pex);
            }

            throw new Exception($"회원가입 실패: {pex.Message}", pex);
        }
        catch (Exception ex)
        {
            throw new Exception($"회원가입 실패: {ex.Message}", ex);
        }
    }

    // 게스트 로그인
    public async Task<string> LoginAsGuestAsync(string deviceId)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/LoginWithCustomID";
        var payload = new
        {
            TitleId = _titleId,
            CustomId = deviceId,
            CreateAccount = true
        };

        try
        {
            var response = await PostAsync<LoginResult>(url, payload, false);
            _sessionTicket = response.SessionTicket;
            return response.PlayFabId;
        }
        catch (Exception ex)
        {
            // Preserve inner exception so caller can inspect PlayFab API error payload
            throw new Exception("게스트 로그인 실패", ex);
        }
    }

    public async Task<Player> GetPlayerAsync(string playerId)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/GetUserData";
        
        try
        {
            var response = await PostAsync<UserDataResponse>(url, new { }, true);
            
            if (response?.Data == null)
            {
                return CreateDefaultPlayer(playerId);
            }

            return new Player
            {
                Id = playerId,
                Username = response.Data.ContainsKey("Username") ? response.Data["Username"].Value : "플레이어",
                Level = response.Data.ContainsKey("Level") ? int.Parse(response.Data["Level"].Value) : 1,
                Gold = response.Data.ContainsKey("Gold") ? long.Parse(response.Data["Gold"].Value) : 10000,
                Experience = response.Data.ContainsKey("Experience") ? long.Parse(response.Data["Experience"].Value) : 0,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
        }
        catch
        {
            return CreateDefaultPlayer(playerId);
        }
    }

    public async Task<Player> CreatePlayerAsync(string username)
    {
        var playerId = Guid.NewGuid().ToString();
        var player = CreateDefaultPlayer(playerId);
        player.Username = username;
        
        await UpdatePlayerAsync(player);
        return player;
    }

    public async Task<bool> UpdatePlayerAsync(Player player)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/UpdateUserData";
        var payload = new
        {
            Data = new Dictionary<string, string>
            {
                { "Username", player.Username },
                { "Level", player.Level.ToString() },
                { "Gold", player.Gold.ToString() },
                { "Experience", player.Experience.ToString() }
            }
        };

        try
        {
            await PostAsync<object>(url, payload, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"플레이어 업데이트 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePlayerGoldAsync(string playerId, long goldAmount)
    {
        var player = await GetPlayerAsync(playerId);
        player.Gold += goldAmount;
        return await UpdatePlayerAsync(player);
    }

    public async Task<List<Mercenary>> GetMercenariesAsync(string playerId)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/GetUserData";
        
        try
        {
            var response = await PostAsync<UserDataResponse>(url, new { }, true);
            
            if (response?.Data != null && response.Data.ContainsKey("Mercenaries"))
            {
                var json = response.Data["Mercenaries"].Value;
                return JsonSerializer.Deserialize<List<Mercenary>>(json) ?? new List<Mercenary>();
            }

            return new List<Mercenary>();
        }
        catch
        {
            return new List<Mercenary>();
        }
    }

    public async Task<bool> UpdateMercenaryAsync(Mercenary mercenary)
    {
        var mercenaries = await GetMercenariesAsync(mercenary.PlayerId);
        var existing = mercenaries.FirstOrDefault(m => m.Id == mercenary.Id);
        
        if (existing != null)
        {
            mercenaries.Remove(existing);
        }
        mercenaries.Add(mercenary);

        var url = $"https://{_titleId}.playfabapi.com/Client/UpdateUserData";
        var payload = new
        {
            Data = new Dictionary<string, string>
            {
                { "Mercenaries", JsonSerializer.Serialize(mercenaries) }
            }
        };

        try
        {
            await PostAsync<object>(url, payload, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<InventoryItem>> GetInventoryAsync(string playerId)
    {
        var url = $"https://{_titleId}.playfabapi.com/Client/GetUserData";
        
        try
        {
            var response = await PostAsync<UserDataResponse>(url, new { }, true);
            
            if (response?.Data != null && response.Data.ContainsKey("Inventory"))
            {
                var json = response.Data["Inventory"].Value;
                return JsonSerializer.Deserialize<List<InventoryItem>>(json) ?? new List<InventoryItem>();
            }

            return new List<InventoryItem>();
        }
        catch
        {
            return new List<InventoryItem>();
        }
    }

    public async Task<bool> UpdateInventoryAsync(string playerId, InventoryItem item)
    {
        var inventory = await GetInventoryAsync(playerId);
        var existing = inventory.FirstOrDefault(i => i.Id == item.Id);
        
        if (existing != null)
        {
            inventory.Remove(existing);
        }
        
        if (item.Quantity > 0)
        {
            inventory.Add(item);
        }

        var url = $"https://{_titleId}.playfabapi.com/Client/UpdateUserData";
        var payload = new
        {
            Data = new Dictionary<string, string>
            {
                { "Inventory", JsonSerializer.Serialize(inventory) }
            }
        };

        try
        {
            await PostAsync<object>(url, payload, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsCurrentUserAdminAsync()
    {
        try
        {
            Console.WriteLine("=== Admin Status Check Start ===");
            Console.WriteLine($"Session Ticket: {(_sessionTicket != null ? "EXISTS" : "NULL")}");
            
            var result = await ExecuteCloudScriptAsync<CloudScriptAdminStatusResult>("GetPlayerAdminStatus", null);
            
            Console.WriteLine($"Cloud Script Result: {(result != null ? "SUCCESS" : "NULL")}");
            Console.WriteLine($"IsAdmin Value: {result?.IsAdmin}");
            Console.WriteLine("=== Admin Status Check End ===");
            
            return result?.IsAdmin ?? false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"!!! Error checking admin status: {ex.Message}");
            Console.Error.WriteLine($"!!! Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"!!! Inner Exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }

        // Update the player's display name (what other players see). Requires auth.
        public async Task<bool> UpdateDisplayNameAsync(string displayName)
        {
            var url = $"https://{_titleId}.playfabapi.com/Client/UpdateUserTitleDisplayName";
            var payload = new
            {
                DisplayName = displayName
            };
    
            try
            {
                await PostAsync<object>(url, payload, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"DisplayName update failed: {ex.Message}");
                return false;
            }
        }
    
        public async Task<List<AdminPlayerInfo>> SearchPlayersAsync(string searchTerm)
        {
            try
            {
                var result = await ExecuteCloudScriptAsync<SearchPlayersResult>("SearchPlayers", new SearchPlayersRequest { SearchTerm = searchTerm });
                return result?.Players ?? new List<AdminPlayerInfo>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error searching players: {ex.Message}");
                throw;
            }
        }
    
        public async Task DeletePlayerAsync(string playFabId)
        {
            try
            {
                await ExecuteCloudScriptAsync<object>("DeletePlayer", new PlayerIdRequest { PlayFabId = playFabId });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting player {playFabId}: {ex.Message}");
                throw;
            }
        }
    
        public async Task ResetPlayerAsync(string playFabId)
        {
            try
            {
                await ExecuteCloudScriptAsync<object>("ResetPlayer", new PlayerIdRequest { PlayFabId = playFabId });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error resetting player {playFabId}: {ex.Message}");
                throw;
            }
        }
    
        private async Task<T> ExecuteCloudScriptAsync<T>(string functionName, object functionParameters)
        {
            var url = $"https://{_titleId}.playfabapi.com/Client/ExecuteCloudScript";
            var payload = new ExecuteCloudScriptRequest
            {
                FunctionName = functionName,
                FunctionParameter = functionParameters,
                GeneratePlayStreamEvent = true
            };
    
            try
            {
                Console.WriteLine($">>> Executing CloudScript: {functionName}");
                Console.WriteLine($">>> Parameters: {JsonSerializer.Serialize(functionParameters)}");
                
                var response = await PostAsync<ExecuteCloudScriptResult>(url, payload, true);
                
                Console.WriteLine($">>> Response received: {(response != null ? "SUCCESS" : "NULL")}");
                Console.WriteLine($">>> Function Result: {response?.FunctionResult}");
                Console.WriteLine($">>> Logs: {JsonSerializer.Serialize(response?.Logs)}");
                Console.WriteLine($">>> Error: {response?.Error}");
                
                if (response?.FunctionResult != null)
                {
                    // PlayFab returns FunctionResult as a JSON string, so we need to deserialize it again
                    var result = JsonSerializer.Deserialize<T>(response.FunctionResult.ToString(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Console.WriteLine($">>> Deserialized Result: {JsonSerializer.Serialize(result)}");
                    return result;
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"!!! CloudScript function '{functionName}' execution failed: {ex.Message}");
                throw new Exception($"CloudScript function '{functionName}' execution failed: {ex.Message}", ex);
            }
        }
    
        private Player CreateDefaultPlayer(string playerId)
        {
            return new Player
            {
                Id = playerId,
                Username = "플레이어",
                Level = 1,
                Experience = 0,
                Gold = 10000,
                Crystal = 0,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
        }
    
        private async Task<T> PostAsync<T>(string url, object payload, bool requireAuth)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
    
            _httpClient.DefaultRequestHeaders.Remove("X-Authorization");
            
            if (requireAuth && !string.IsNullOrEmpty(_sessionTicket))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Authorization", _sessionTicket);
            }
    
            var response = await _httpClient.PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();
    
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"PlayFab API 에러: {jsonResponse}");
    
                // Try to parse PlayFab error payload into a structured object
                try
                {
                    var error = JsonSerializer.Deserialize<PlayFabError>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
    
                    if (error != null)
                    {
                        throw new PlayFabApiException(error);
                    }
                }
                catch
                {
                    // If parsing fails, fall back to raw message
                }
    
                throw new Exception(jsonResponse);
            }
    
            var result = JsonSerializer.Deserialize<PlayFabResponse<T>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    
            return result.data;
        }
    
        private class PlayFabResponse<T>
        {
            public int code { get; set; }
            public string status { get; set; }
            public T data { get; set; }
        }
    
        private class LoginResult
        {
            public string PlayFabId { get; set; }
            public string SessionTicket { get; set; }
        }
    
        private class UserDataResponse
        {
            public Dictionary<string, UserDataRecord> Data { get; set; }
        }
    
        private class UserDataRecord
        {
            public string Value { get; set; }
        }
    
        private class PlayFabError
        {
            public int Code { get; set; }
            public string Status { get; set; }
            public string Error { get; set; }
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
            public Dictionary<string, string[]> ErrorDetails { get; set; }
        }
    
        private class PlayFabApiException : Exception
        {
            public string Status { get; }
            public string Error { get; }
            public int ErrorCode { get; }
            public string PlayFabMessage { get; }
            public Dictionary<string, string[]> ErrorDetails { get; }
    
            public PlayFabApiException(PlayFabError error)
                : base(error?.ErrorMessage ?? "PlayFab API error")
            {
                if (error == null) return;
    
                Status = error.Status;
                Error = error.Error;
                ErrorCode = error.ErrorCode;
                PlayFabMessage = error.ErrorMessage;
                ErrorDetails = error.ErrorDetails;
            }
        }
    
        // New DTOs for CloudScript execution
        private class ExecuteCloudScriptRequest
        {
            public string FunctionName { get; set; }
            public object FunctionParameter { get; set; }
            public bool GeneratePlayStreamEvent { get; set; }
        }
    
        private class ExecuteCloudScriptResult
        {
            public string FunctionName { get; set; }
            public object FunctionResult { get; set; } // Can be any JSON object/string
            public PlayStreamEvent[] Logs { get; set; }
        }
    
        private class PlayStreamEvent
        {
            public string EventName { get; set; }
            public object Body { get; set; }
            public string Timestamp { get; set; }
        }
    
        private class CloudScriptAdminStatusResult
        {
            public bool IsAdmin { get; set; }
        }
    
        private class SearchPlayersRequest
        {
            public string SearchTerm { get; set; }
        }
    
        private class SearchPlayersResult
        {
            public List<AdminPlayerInfo> Players { get; set; }
        }
    
        private class PlayerIdRequest
        {
            public string PlayFabId { get; set; }
        }}