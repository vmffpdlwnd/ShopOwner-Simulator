// PlayFabService - SDK-based implementation (server-side account creation + server APIs)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlayFab;
using ClientModels = PlayFab.ClientModels;
using ServerModels = PlayFab.ServerModels;
using AdminModels = PlayFab.AdminModels;

namespace ShopOwnerSimulator.Services;

public class PlayFabService
{
	private readonly string _titleId;
	private readonly string _secretKey;

	public PlayFabService(IConfiguration configuration)
	{
		_titleId = configuration["PlayFab:TitleId"]?.ToUpper() ?? string.Empty;
		_secretKey = configuration["PlayFab:SecretKey"] ?? string.Empty;

		if (!string.IsNullOrEmpty(_titleId) && string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
			PlayFabSettings.staticSettings.TitleId = _titleId;
		if (!string.IsNullOrEmpty(_secretKey))
			PlayFabSettings.staticSettings.DeveloperSecretKey = _secretKey;

		Console.WriteLine($"PlayFab SDK configured: TitleId={_titleId}");
	}

	// Client-side login WITHOUT creating accounts (since client creation may be disabled)
	public async Task<string?> AuthenticatePlayerAsync(string username)
	{
		try
		{
			var req = new ClientModels.LoginWithCustomIDRequest { CustomId = username, CreateAccount = false, TitleId = _titleId };
			var res = await PlayFabClientAPI.LoginWithCustomIDAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return null;
			}

			return res.Result?.PlayFabId;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"AuthenticatePlayerAsync error: {ex.Message}");
			return null;
		}
	}

	// Server-side account creation using PlayFab Client API but executed from server where DeveloperSecretKey is set.
	// This keeps control on the server and avoids relying on client-side CreateAccount.
	public async Task<string?> CreateAccountServerAsync(string username, string password)
	{
		try
		{
			var req = new ClientModels.RegisterPlayFabUserRequest
			{
				Username = username,
				Password = password,
				Email = $"{username}@game.local",
				TitleId = _titleId
			};

			var res = await PlayFabClientAPI.RegisterPlayFabUserAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return null;
			}

			return res.Result?.PlayFabId;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"CreateAccountServerAsync error: {ex.Message}");
			return null;
		}
	}

	public async Task<bool> SaveUserDataAsync(string playFabId, User user)
	{
		try
		{
			var data = new Dictionary<string, string>
			{
				{ "Level", user.Level.ToString() },
				{ "Gold", user.Gold.ToString() },
				{ "Username", user.Username ?? string.Empty },
				{ "FullData", System.Text.Json.JsonSerializer.Serialize(user) }
			};

			var req = new ServerModels.UpdateUserDataRequest
			{
				PlayFabId = playFabId,
				Data = data,
				Permission = ServerModels.UserDataPermission.Public
			};

			var res = await PlayFabServerAPI.UpdateUserDataAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
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

	public async Task<User?> LoadUserDataAsync(string playFabId)
	{
		try
		{
			var req = new ServerModels.GetUserDataRequest { PlayFabId = playFabId, Keys = new List<string> { "FullData" } };
			var res = await PlayFabServerAPI.GetUserDataAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return null;
			}

			if (res.Result.Data != null && res.Result.Data.TryGetValue("FullData", out var entry) && entry.Value != null)
			{
				try
				{
					var user = System.Text.Json.JsonSerializer.Deserialize<User>(entry.Value);
					return user;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"LoadUserDataAsync deserialize error: {ex.Message}");
				}
			}

			return null;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"LoadUserDataAsync error: {ex.Message}");
			return null;
		}
	}

	public async Task<bool> UpdatePlayerStatisticsAsync(string playFabId, int gold)
	{
		try
		{
			var req = new ServerModels.UpdatePlayerStatisticsRequest
			{
				PlayFabId = playFabId,
				Statistics = new List<ServerModels.StatisticUpdate> { new ServerModels.StatisticUpdate { StatisticName = "TotalGold", Value = gold } }
			};

			var res = await PlayFabServerAPI.UpdatePlayerStatisticsAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
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

	public async Task<List<(string PlayFabId, string DisplayName, int Statistic)>> GetLeaderboardAsync(int maxResults = 10)
	{
		var resultList = new List<(string, string, int)>();
		try
		{
			var req = new ServerModels.GetLeaderboardRequest { StatisticName = "TotalGold", StartPosition = 0, MaxResultsCount = maxResults };
			var res = await PlayFabServerAPI.GetLeaderboardAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return resultList;
			}

			foreach (var entry in res.Result.Leaderboard)
			{
				var pid = entry.PlayFabId ?? string.Empty;
				var display = entry.DisplayName ?? "Unknown";
				var stat = entry.StatValue;
				resultList.Add((pid, display, stat));
			}

			return resultList;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"GetLeaderboardAsync error: {ex.Message}");
			return resultList;
		}
	}

	// Compatibility wrappers expected by other services
	public Task<string?> CreateAccountAsync(string username, string password)
	{
		return CreateAccountServerAsync(username, password);
	}

	public async Task<string?> AuthenticateWithPasswordAsync(string username, string password)
	{
		try
		{
			var req = new ClientModels.LoginWithPlayFabRequest { Username = username, Password = password, TitleId = _titleId };
			var res = await PlayFabClientAPI.LoginWithPlayFabAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return null;
			}

			return res.Result?.PlayFabId;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"AuthenticateWithPasswordAsync error: {ex.Message}");
			return null;
		}
	}

	public async Task<string?> CheckUserExistsByUsernameAsync(string username)
	{
		try
		{
			var req = new AdminModels.LookupUserAccountInfoRequest { Username = username };
			var res = await PlayFabAdminAPI.GetUserAccountInfoAsync(req).ConfigureAwait(false);
			if (res.Error != null)
			{
				Console.WriteLine(res.Error.GenerateErrorReport());
				return null;
			}

			return res.Result?.UserInfo?.PlayFabId;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"CheckUserExistsByUsernameAsync error: {ex.Message}");
			return null;
		}
	}
}
