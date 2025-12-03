// Services/Interfaces/IStateService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IStateService
{
    event Action OnStateChanged;
    
    Player CurrentPlayer { get; set; }
    List<Mercenary> Mercenaries { get; set; }
    List<InventoryItem> Inventory { get; set; }
    List<Transaction> Transactions { get; set; }
    
    Task LoadPlayerStateAsync(string playerId);
    Task SavePlayerStateAsync();
    Task PersistLocalAsync();
    void NotifyStateChanged();
}