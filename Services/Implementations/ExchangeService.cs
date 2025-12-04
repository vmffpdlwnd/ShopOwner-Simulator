// Services/Implementations/ExchangeService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly IStateService _stateService;
    private readonly IDynamoDBService _dynamoDB;
    private readonly IInventoryService _inventoryService;

    public ExchangeService(
        IStateService stateService,
        IDynamoDBService dynamoDB,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _dynamoDB = dynamoDB;
        _inventoryService = inventoryService;
    }

    public async Task<List<ExchangeOrder>> GetOrdersAsync(string itemTemplateId)
    {
        return await _dynamoDB.GetExchangeOrdersByItemAsync(itemTemplateId);
    }

    public async Task<List<ExchangeOrder>> GetMyOrdersAsync(string playerId)
    {
        return await _dynamoDB.GetPlayerOrdersAsync(playerId);
    }

    public async Task<ExchangeListResponse> ListOrderAsync(ExchangeListRequest request)
    {
        // 인벤토리 확인
        var inventoryItem = _stateService.Inventory.FirstOrDefault(
            i => i.ItemTemplateId == request.ItemTemplateId);

        if (inventoryItem == null || inventoryItem.Quantity < request.Quantity)
            throw new Exception("인벤토리가 부족합니다");

        // 주문 생성
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

        // 인벤토리에서 제거
        await _inventoryService.RemoveItemAsync(
            _stateService.CurrentPlayer.Id,
            request.ItemTemplateId,
            request.Quantity);

        // DB에 저장
        await _dynamoDB.SaveExchangeOrderAsync(order);

        return new ExchangeListResponse
        {
            OrderId = order.Id,
            ListedTime = order.ListedTime
        };
    }

    public async Task<Transaction> BuyAsync(ExchangeBuyRequest request)
    {
        var order = await _dynamoDB.GetExchangeOrderAsync(request.OrderId);
        if (order == null || order.Status != OrderStatus.Active)
            throw new Exception("주문을 찾을 수 없습니다");

        if (order.Remaining < request.Quantity)
            throw new Exception("수량이 부족합니다");

        var totalCost = order.UnitPrice * request.Quantity;
        if (_stateService.CurrentPlayer.Gold < totalCost)
            throw new Exception("골드가 부족합니다");

        // 구매자 골드 차감
        _stateService.CurrentPlayer.Gold -= totalCost;

        // 구매자 인벤토리에 아이템 추가
        await _inventoryService.AddItemAsync(
            _stateService.CurrentPlayer.Id,
            order.ItemTemplateId,
            request.Quantity);

        // 주문 업데이트
        order.Remaining -= request.Quantity;
        if (order.Remaining == 0)
        {
            order.Status = OrderStatus.Completed;
        }
        await _dynamoDB.SaveExchangeOrderAsync(order);

        // 거래 기록 생성
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

        await _dynamoDB.SaveTransactionAsync(transaction);
        _stateService.NotifyStateChanged();

        return transaction;
    }

    public async Task<bool> CancelOrderAsync(string orderId)
    {
        var order = await _dynamoDB.GetExchangeOrderAsync(orderId);
        if (order == null || order.Status != OrderStatus.Active)
            return false;

        // 인벤토리에 반환
        await _inventoryService.AddItemAsync(
            order.SellerId,
            order.ItemTemplateId,
            order.Remaining);

        order.Status = OrderStatus.Cancelled;
        await _dynamoDB.SaveExchangeOrderAsync(order);

        return true;
    }

    public async Task<List<ItemTemplate>> GetAvailableItemsAsync()
    {
        // 임시 구현 - 나중에 DB에서 가져오기
        return new List<ItemTemplate>();
    }
}