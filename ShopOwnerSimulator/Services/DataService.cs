// Services/DataService.cs
using System.Text.Json;
using System.IO;
using ShopOwnerSimulator.Models;
using Microsoft.AspNetCore.Hosting;

namespace ShopOwnerSimulator.Services;

public class DataService
{
    private const string DATA_FOLDER = "GameData";
    private const string USER_FILE_NAME = "user_data.json";
    private readonly string _dataPath;
    private readonly string _userFilePath;

    // DataService now reads DATA_DIR environment variable (if set) to determine
    // where runtime data should be stored. If DATA_DIR is not set, it falls back
    // to the application's content root + /GameData. This makes paths portable
    // across machines and containers.
    public DataService(IWebHostEnvironment env)
    {
        var dataDirEnv = Environment.GetEnvironmentVariable("DATA_DIR");
        if (!string.IsNullOrEmpty(dataDirEnv))
        {
            // If provided path is relative, treat it relative to content root.
            _dataPath = Path.IsPathRooted(dataDirEnv) ? dataDirEnv : Path.Combine(env.ContentRootPath, dataDirEnv);
        }
        else
        {
            _dataPath = Path.Combine(env.ContentRootPath, DATA_FOLDER);
        }

        _userFilePath = Path.Combine(_dataPath, USER_FILE_NAME);

        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }
    }

    public void SaveUser(User user)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(user, options);
            File.WriteAllText(_userFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"유저 데이터 저장 실패: {ex.Message}");
        }
    }

    public User? LoadUser()
    {
        try
        {
            if (!File.Exists(_userFilePath)) return null;
            string json = File.ReadAllText(_userFilePath);
            return JsonSerializer.Deserialize<User>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"유저 데이터 로드 실패: {ex.Message}");
            return null;
        }
    }

    public bool HasSavedGame() => File.Exists(_userFilePath);

    public void DeleteSavedGame()
    {
        try
        {
            if (File.Exists(_userFilePath)) File.Delete(_userFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"게임 데이터 삭제 실패: {ex.Message}");
        }
    }
}