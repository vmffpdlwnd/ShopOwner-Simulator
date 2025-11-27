// Services/Implementations/ExchangeService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IInventoryService _inventoryService;

    private readonly List<ExchangeOrder> _orders = new();
    private readonly List<Transaction> _transactions = new();

    private readonly Dictionary<string, long> _itemPrices = new()
    {
        { "material_ore", 100 },
        { "material_wood", 80 },
        { "material_herb", 120 },
        { "equipment_sword", 500 },
        { "equipment_armor", 400 }
    };

    public ExchangeService(
        IStateService stateService,
        IStorageService storage,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _storage = storage;
        _inventoryService = inventoryService;
    }

    public async Task<List<ExchangeOrder>> GetOrdersAsync(string itemTemplateId)
    {
        return _orders
            .Where(o => o.ItemTemplateId == itemTemplateId && o.Status == OrderStatus.Active)
            .ToList();
    }

    public async Task<List<ExchangeOrder>> GetMyOrdersAsync(string playerId)
    {
        return _orders.Where(o => o.SellerId == playerId).ToList();
    }

    public async Task<ExchangeListResponse> ListOrderAsync(ExchangeListRequest request)
    {
        // Verify item exists in inventory
        var inventoryItem = _stateService.Inventory.FirstOrDefault(
            i => i.ItemTemplateId == request.ItemTemplateId);

        if (inventoryItem == null || inventoryItem.Quantity < request.Quantity)
            throw new Exception("Insufficient inventory");

        // Create order
        var order = new ExchangeOrder
        {
            Id = Guid.NewGuid().ToString(),
            SellerId = _stateService.CurrentPlayer.Id,
            ItemTemplateId = request.ItemTemplateId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Remaining = request.Quantity,
            ListedTime = DateTime.UtcNow,
            Status = OrderStatus.Active
        };

        // Remove from inventory
        await _inventoryService.RemoveItemAsync(
            _stateService.CurrentPlayer.Id,
            request.ItemTemplateId,
            request.Quantity);

        _orders.Add(order);
        await _storage.SetAsync($"exchange_order_{order.Id}", order);

        return new ExchangeListResponse
        {
            OrderId = order.Id,
            ListedTime = order.ListedTime
        };
    }

    public async Task<Transaction> BuyAsync(ExchangeBuyRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == request.OrderId);
        if (order == null || order.Status != OrderStatus.Active)
            throw new Exception("Order not available");

        if (order.Remaining < request.Quantity)
            throw new Exception("Insufficient quantity");

        var totalCost = order.UnitPrice * request.Quantity;
        if (_stateService.CurrentPlayer.Gold < totalCost)
            throw new Exception("Insufficient gold");

        // Deduct gold from buyer
        _stateService.CurrentPlayer.Gold -= totalCost;

        // Add item to buyer inventory
        await _inventoryService.AddItemAsync(
            _stateService.CurrentPlayer.Id,
            order.ItemTemplateId,
            request.Quantity);

        // Add gold to seller
        var seller = _stateService.Mercenaries.FirstOrDefault(m => m.PlayerId == order.SellerId);
        if (seller != null)
        {
            // Get seller's player data and update gold
            // This is simplified - in real implementation, fetch seller player data
        }

        // Update order
        order.Remaining -= request.Quantity;
        if (order.Remaining == 0)
        {
            order.Status = OrderStatus.Completed;
        }

        // Create transaction record
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            BuyerId = _stateService.CurrentPlayer.Id,
            SellerId = order.SellerId,
            OrderId = order.Id,
            ItemTemplateId = order.ItemTemplateId,
            Quantity = request.Quantity,
            UnitPrice = order.UnitPrice,
            TotalGold = totalCost,
            TransactionTime = DateTime.UtcNow,
            Type = TransactionType.Exchange
        };

        _transactions.Add(transaction);
        await _storage.SetAsync($"transaction_{transaction.Id}", transaction);

        return transaction;
    }

    public Task<bool> CancelOrderAsync(string orderId)
    {
        throw new NotImplementedException();
    }

    public Task<List<ItemTemplate>> GetAvailableItemsAsync()
    {
        throw new NotImplementedException();
    }
}