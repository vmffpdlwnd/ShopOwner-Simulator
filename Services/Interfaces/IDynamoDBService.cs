using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IDynamoDBService
{
    // PersonalShopListing
    Task<bool> SavePersonalShopListingAsync(PersonalShopListing listing);
    Task<PersonalShopListing> GetPersonalShopListingAsync(string listingId);
    Task<List<PersonalShopListing>> GetPlayerListingsAsync(string playerId);
    Task<bool> DeletePersonalShopListingAsync(string listingId);

    // ExchangeOrder
    Task<bool> SaveExchangeOrderAsync(ExchangeOrder order);
    Task<ExchangeOrder> GetExchangeOrderAsync(string orderId);
    Task<List<ExchangeOrder>> GetExchangeOrdersByItemAsync(string itemTemplateId);
    Task<List<ExchangeOrder>> GetPlayerOrdersAsync(string playerId);
    Task<bool> DeleteExchangeOrderAsync(string orderId);

    // Transaction
    Task<bool> SaveTransactionAsync(Transaction transaction);
    Task<List<Transaction>> GetPlayerTransactionsAsync(string playerId);

    // DungeonProgress
    Task<bool> SaveDungeonProgressAsync(DungeonProgress progress);
    Task<DungeonProgress> GetDungeonProgressAsync(string progressId);
    Task<List<DungeonProgress>> GetMercenaryProgressAsync(string mercenaryId);
    Task<bool> DeleteDungeonProgressAsync(string progressId);
}