using PlayFab;
using PlayFab.ClientModels;
using ShopOwnerSimulator.Client.Models;

namespace ShopOwnerSimulator.Client.Services;

public class PlayFabService
{
    private readonly string _titleId;
    private string? _sessionTicket;
    private string? _playFabId;

    public PlayFabService(IConfiguration configuration)
    {
        _titleId = configuration["PlayFab:TitleId"] ?? string.Empty;
        
        if (!string.IsNullOrEmpty(_titleId))
        {
            PlayFabSettings.staticSettings.TitleId = _titleId;
        }

        Console.WriteLine($"PlayFab Client SDK configured: TitleId={_titleId}");
    }

    public string? PlayFabId => _playFabId;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionTicket);

    // 계정 생성 (CustomId 사용)
    public async Task<bool> CreateAccountAsync(string username)
    {
        try
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = username,
                CreateAccount = true,
                TitleId = _titleId
            };

            var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Account creation failed: {result.Error.GenerateErrorReport()}");
                return false;
            }

            _playFabId = result.Result.PlayFabId;
            _sessionTicket = result.Result.SessionTicket;
            
            // 초기 사용자 데이터 저장
            await SaveUserDataAsync(new User 
            { 
                Username = username, 
                PlayFabId = _playFabId,
                Level = 1,
                Gold = 1000
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateAccountAsync error: {ex.Message}");
            return false;
        }
    }

    // 로그인 (CustomId 사용)
    public async Task<bool> LoginAsync(string username)
    {
        try
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = username,
                CreateAccount = false,
                TitleId = _titleId
            };

            var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Login failed: {result.Error.GenerateErrorReport()}");
                return false;
            }

            _playFabId = result.Result.PlayFabId;
            _sessionTicket = result.Result.SessionTicket;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginAsync error: {ex.Message}");
            return false;
        }
    }

    // 사용자 데이터 저장
    public async Task<bool> SaveUserDataAsync(User user)
    {
        try
        {
            if (string.IsNullOrEmpty(_playFabId))
            {
                Console.WriteLine("Not authenticated");
                return false;
            }

            var data = new Dictionary<string, string>
            {
                { "Username", user.Username ?? string.Empty },
                { "Level", user.Level.ToString() },
                { "Gold", user.Gold.ToString() },
                { "FullData", System.Text.Json.JsonSerializer.Serialize(user) }
            };

            var request = new UpdateUserDataRequest
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            var result = await PlayFabClientAPI.UpdateUserDataAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Save data failed: {result.Error.GenerateErrorReport()}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveUserDataAsync error: {ex.Message}");
            return false;
        }
    }

    // 사용자 데이터 로드
    public async Task<User?> LoadUserDataAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_playFabId))
            {
                Console.WriteLine("Not authenticated");
                return null;
            }

            var request = new GetUserDataRequest
            {
                PlayFabId = _playFabId,
                Keys = new List<string> { "FullData" }
            };

            var result = await PlayFabClientAPI.GetUserDataAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Load data failed: {result.Error.GenerateErrorReport()}");
                return null;
            }

            if (result.Result.Data != null && 
                result.Result.Data.TryGetValue("FullData", out var entry) && 
                !string.IsNullOrEmpty(entry.Value))
            {
                var user = System.Text.Json.JsonSerializer.Deserialize<User>(entry.Value);
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadUserDataAsync error: {ex.Message}");
            return null;
        }
    }

    // 통계 업데이트 (리더보드용)
    public async Task<bool> UpdatePlayerStatisticsAsync(int gold)
    {
        try
        {
            if (string.IsNullOrEmpty(_playFabId))
            {
                Console.WriteLine("Not authenticated");
                return false;
            }

            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "TotalGold",
                        Value = gold
                    }
                }
            };

            var result = await PlayFabClientAPI.UpdatePlayerStatisticsAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Update statistics failed: {result.Error.GenerateErrorReport()}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdatePlayerStatisticsAsync error: {ex.Message}");
            return false;
        }
    }

    // 리더보드 조회
    public async Task<List<(string DisplayName, int Gold)>> GetLeaderboardAsync(int maxResults = 10)
    {
        var resultList = new List<(string, int)>();
        
        try
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = "TotalGold",
                StartPosition = 0,
                MaxResultsCount = maxResults
            };

            var result = await PlayFabClientAPI.GetLeaderboardAsync(request);
            
            if (result.Error != null)
            {
                Console.WriteLine($"Get leaderboard failed: {result.Error.GenerateErrorReport()}");
                return resultList;
            }

            foreach (var entry in result.Result.Leaderboard)
            {
                var displayName = entry.DisplayName ?? "Unknown";
                var statValue = entry.StatValue;
                resultList.Add((displayName, statValue));
            }

            return resultList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetLeaderboardAsync error: {ex.Message}");
            return resultList;
        }
    }

    // 로그아웃
    public void Logout()
    {
        _playFabId = null;
        _sessionTicket = null;
    }
}
