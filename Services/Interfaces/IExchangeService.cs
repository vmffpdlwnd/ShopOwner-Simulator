// Services/Interfaces/IExchangeService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IExchangeService
{
    Task<List<ExchangeOrder>> GetOrdersAsync(string itemTemplateId);
    Task<List<ExchangeOrder>> GetMyOrdersAsync(string playerId);
    Task<ExchangeListResponse> ListOrderAsync(ExchangeListRequest request);
    Task<Transaction> BuyAsync(ExchangeBuyRequest request);
    Task<bool> CancelOrderAsync(string orderId);
    Task<List<ItemTemplate>> GetAvailableItemsAsync();
}