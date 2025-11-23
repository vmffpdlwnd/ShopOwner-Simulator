// Services/AuthService.cs
using System.Security.Cryptography;
using System.Text;

namespace ShopOwnerSimulator.Services;

public class AuthService
{
    private readonly PlayFabService _playFabService;
    private readonly DataService _dataService;

    public AuthService(PlayFabService playFabService, DataService dataService)
    {
        _playFabService = playFabService;
        _dataService = dataService;
    }

    public async Task<(bool Success, string Message, User? User)> LoginOrRegisterAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "ID와 비밀번호를 입력해주세요.", default(User?));

            var existingPlayFabId = await _playFabService.CheckUserExistsByUsernameAsync(username);

            if (existingPlayFabId != null)
            {
                var playFabId = await _playFabService.AuthenticateWithPasswordAsync(username, password);
                if (string.IsNullOrEmpty(playFabId)) return (false, "비밀번호가 일치하지 않습니다.", default(User?));

                var user = await _playFabService.LoadUserDataAsync(playFabId);
                if (user == null) return (false, "유저 데이터를 찾을 수 없습니다.", default(User?));

                user.PlayFabId = playFabId;
                user.IsLoggedIn = true;
                user.LastLoginAt = DateTime.UtcNow;

                try { _dataService.SaveUser(user); } catch { }

                return (true, $"로그인 성공! 환영합니다, {username}님!", user);
            }
            else
            {
                var newUser = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = username,
                    Level = 1,
                    Gold = 5000,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    Inventory = new Inventory { UserId = null },
                    Characters = new List<Character>(),
                    Shop = new Shop(),
                    IsLoggedIn = true,
                    PasswordHash = HashPassword(password)
                };

                newUser.Characters.Add(CreateStarterCharacter());

                var playFabId = await _playFabService.CreateAccountAsync(username, password);
                if (string.IsNullOrEmpty(playFabId)) return (false, "PlayFab 계정 생성에 실패했습니다.", default(User?));

                newUser.PlayFabId = playFabId;

                var saveSuccess = await _playFabService.SaveUserDataAsync(playFabId, newUser);
                if (!saveSuccess) return (false, "PlayFab 저장에 실패했습니다.", default(User?));

                try { _dataService.SaveUser(newUser); } catch { }

                return (true, $"회원가입 성공! 환영합니다, {username}님!", newUser);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"로그인/회원가입 오류: {ex.Message}");
            return (false, $"오류 발생: {ex.Message}", default(User?));
        }
    }

    public void Logout(User? user)
    {
        if (user != null)
        {
            user.IsLoggedIn = false;
            try { _dataService.SaveUser(user); } catch { }
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private Character CreateStarterCharacter()
    {
        return new Character
        {
            Name = "초급 전사",
            Job = CharacterJob.Warrior,
            Level = 1,
            MonthlySalary = 500,
            IsRentable = true,
            MaxHP = 100,
            HP = 100,
            Attack = 20,
            Defense = 15,
            Speed = 8
        };
    }
}
