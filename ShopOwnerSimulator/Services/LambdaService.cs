// Services/LambdaService.cs
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ShopOwnerSimulator.Models;

namespace ShopOwnerSimulator.Services;

public class LambdaService
{
    private readonly string _lambdaUrl = "https://lnf7r5m4ew5s2p3ilsrc2okthu0pztob.lambda-url.ap-northeast-2.on.aws/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<LambdaService> _logger;

    public LambdaService(ILogger<LambdaService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

    /// <summary>
    /// Lambda에서 유저 정보 조회
    /// </summary>
    public async Task<LambdaUserResponse?> GetUserInfoAsync(string userId)
    {
        try
        {
            var payload = new { UserId = userId };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_lambdaUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<LambdaUserResponse>(responseBody);
            
            if (result != null && result.Success)
            {
                _logger.LogInformation($"Lambda 조회 성공: {userId}");
                return result;
            }
            else
            {
                _logger.LogWarning($"Lambda 조회 실패: {result?.Message}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lambda 호출 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Lambda에 유저 데이터 저장
    /// </summary>
    public async Task<bool> SaveUserDataAsync(User user)
    {
        try
        {
            var payload = new
            {
                UserId = user.UserId,
                Username = user.Username,
                Gold = user.Gold,
                Level = user.Level,
                Inventory = new
                {
                    Wood = user.Inventory.Wood,
                    Iron = user.Inventory.Iron,
                    Stone = user.Inventory.Stone,
                    Crystal = user.Inventory.Crystal
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_lambdaUrl + "save", content);
            var success = response.IsSuccessStatusCode;

            if (success)
            {
                _logger.LogInformation($"Lambda 저장 성공: {user.UserId}");
            }
            else
            {
                _logger.LogWarning($"Lambda 저장 실패: {response.StatusCode}");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lambda 저장 오류: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lambda 함수 헬스 체크
    /// </summary>
    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_lambdaUrl + "health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            _logger.LogWarning("Lambda 헬스 체크 실패");
            return false;
        }
    }
}

// Lambda 응답 모델
public class LambdaUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public LambdaUserData? Data { get; set; }
}

public class LambdaUserData
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalGold { get; set; }
    public int Level { get; set; }
}