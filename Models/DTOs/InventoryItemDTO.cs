// Models/DTOs/InventoryItemDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class InventoryItemDTO
{
    public string Id { get; set; }
    public string ItemTemplateId { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public bool IsEquipped { get; set; }
    public string EquippedMercenaryId { get; set; }
    public string ItemType { get; set; }
    public string Rarity { get; set; }
}