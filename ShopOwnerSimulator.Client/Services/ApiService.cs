using System.Net.Http.Json;
using ShopOwnerSimulator.Client.Models;

namespace ShopOwnerSimulator.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<User?> CreateNewUserAsync(string username)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/user/new", username);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>("api/user/current");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user: {ex.Message}");
                return null;
            }
        }

        public async Task<object?> GetUserInfoAsync(string userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<object>($"api/game/user/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info: {ex.Message}");
                return null;
            }
        }
    }
}
