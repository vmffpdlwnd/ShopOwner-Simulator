// Services/Implementations/TimerService.cs
namespace ShopOwnerSimulator.Services.Implementations;

public class TimerService : ITimerService
{
    private readonly Dictionary<string, TimerData> _timers = new();
    private CancellationTokenSource _cancellationTokenSource = new();

    private class TimerData
    {
        public DateTime EndTime { get; set; }
        public Action OnComplete { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }

    public void StartTimer(string timerId, DateTime endTime, Action onComplete)
    {
        if (_timers.ContainsKey(timerId))
        {
            StopTimer(timerId);
        }

        var cts = new CancellationTokenSource();
        var timerData = new TimerData
        {
            EndTime = endTime,
            OnComplete = onComplete,
            CancellationToken = cts
        };

        _timers[timerId] = timerData;

        // Blazor WebAssembly 호환 타이머
        Task.Run(async () =>
        {
            try
            {
                var remaining = endTime - DateTime.UtcNow;
                if (remaining.TotalMilliseconds <= 0)
                {
                    onComplete?.Invoke();
                    StopTimer(timerId);
                    return;
                }

                while (DateTime.UtcNow < endTime && !cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cts.Token);
                }

                if (!cts.Token.IsCancellationRequested)
                {
                    onComplete?.Invoke();
                    StopTimer(timerId);
                }
            }
            catch (TaskCanceledException)
            {
                // 타이머 취소됨
            }
        }, cts.Token);
    }

    public void StopTimer(string timerId)
    {
        if (_timers.TryGetValue(timerId, out var timerData))
        {
            timerData.CancellationToken?.Cancel();
            timerData.CancellationToken?.Dispose();
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