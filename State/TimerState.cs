// State/TimerState.cs
namespace ShopOwnerSimulator.State;

public class TimerState
{
    public event Action OnTimerUpdated;

    private readonly Dictionary<string, DateTime> _timers = new();

    public void StartTimer(string timerId, DateTime endTime)
    {
        _timers[timerId] = endTime;
        OnTimerUpdated?.Invoke();
    }

    public void StopTimer(string timerId)
    {
        _timers.Remove(timerId);
        OnTimerUpdated?.Invoke();
    }

    public TimeSpan GetRemainingTime(string timerId)
    {
        if (_timers.TryGetValue(timerId, out var endTime))
        {
            var remaining = endTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        return TimeSpan.Zero;
    }

    public bool IsTimerActive(string timerId)
    {
        return _timers.ContainsKey(timerId);
    }

    public Dictionary<string, DateTime> GetAllTimers()
    {
        return new Dictionary<string, DateTime>(_timers);
    }

    public void Clear()
    {
        _timers.Clear();
        OnTimerUpdated?.Invoke();
    }
}