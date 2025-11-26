// Services/Interfaces/ITimerService.cs
namespace ShopOwnerSimulator.Services;

public interface ITimerService
{
    void StartTimer(string timerId, DateTime endTime, Action onComplete);
    void StopTimer(string timerId);
    bool IsTimerRunning(string timerId);
    TimeSpan GetRemainingTime(string timerId);
}