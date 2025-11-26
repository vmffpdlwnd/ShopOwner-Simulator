// Services/Implementations/TimerService.cs
namespace ShopOwnerSimulator.Services.Implementations;

public class TimerService : ITimerService
{
    private readonly Dictionary<string, TimerData> _timers = new();

    private class TimerData
    {
        public DateTime EndTime { get; set; }
        public Action OnComplete { get; set; }
        public Timer Timer { get; set; }
    }

    public void StartTimer(string timerId, DateTime endTime, Action onComplete)
    {
        if (_timers.ContainsKey(timerId))
        {
            StopTimer(timerId);
        }

        var timerData = new TimerData
        {
            EndTime = endTime,
            OnComplete = onComplete
        };

        var remaining = endTime - DateTime.UtcNow;
        if (remaining.TotalMilliseconds <= 0)
        {
            onComplete?.Invoke();
            return;
        }

        timerData.Timer = new Timer(_ =>
        {
            if (DateTime.UtcNow >= endTime)
            {
                onComplete?.Invoke();
                StopTimer(timerId);
            }
        }, null, (long)remaining.TotalMilliseconds, Timeout.Infinite);

        _timers[timerId] = timerData;
    }

    public void StopTimer(string timerId)
    {
        if (_timers.TryGetValue(timerId, out var timerData))
        {
            timerData.Timer?.Dispose();
            _timers.Remove(timerId);
        }
    }

    public bool IsTimerRunning(string timerId)
    {
        return _timers.ContainsKey(timerId);
    }

    public TimeSpan GetRemainingTime(string timerId)
    {
        if (_timers.TryGetValue(timerId, out var timerData))
        {
            var remaining = timerData.EndTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        return TimeSpan.Zero;
    }
}