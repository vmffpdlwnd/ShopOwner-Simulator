// State/MercenaryState.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.State;

public class MercenaryState
{
    public event Action? OnMercenariesChanged;

    public List<Mercenary> Mercenaries { get; private set; } = new();
    public Mercenary? SelectedMercenary { get; set; }

    public void UpdateMercenaries(List<Mercenary> mercenaries)
    {
        Mercenaries = new List<Mercenary>(mercenaries);
        OnMercenariesChanged?.Invoke();
    }

    public void AddMercenary(Mercenary mercenary)
    {
        Mercenaries.Add(mercenary);
        OnMercenariesChanged?.Invoke();
    }

    public void UpdateMercenary(Mercenary mercenary)
    {
        var existing = Mercenaries.FirstOrDefault(m => m.Id == mercenary.Id);
        if (existing != null)
        {
            var index = Mercenaries.IndexOf(existing);
            Mercenaries[index] = mercenary;
            OnMercenariesChanged?.Invoke();
        }
    }

    public Mercenary? GetMercenary(string mercenaryId)
    {
        return Mercenaries.FirstOrDefault(m => m.Id == mercenaryId);
    }

    public List<Mercenary> GetActiveMercenaries()
    {
        return Mercenaries.Where(m => m.IsActive).ToList();
    }

    public void SelectMercenary(string mercenaryId)
    {
        SelectedMercenary = GetMercenary(mercenaryId);
        OnMercenariesChanged?.Invoke();
    }

    public void Clear()
    {
        Mercenaries.Clear();
        SelectedMercenary = null;
        OnMercenariesChanged?.Invoke();
    }
}
