// Models/Entities/InventoryItem.cs
namespace ShopOwnerSimulator.Models.Entities;

public class InventoryItem
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string ItemTemplateId { get; set; }
    public int Quantity { get; set; }
    public bool IsEquipped { get; set; }
    public string EquippedMercenaryId { get; set; }
}