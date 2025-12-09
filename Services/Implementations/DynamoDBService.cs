using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class DynamoDBService : IDynamoDBService
{
    private readonly AmazonDynamoDBClient _client;
    private const string PersonalShopTable = "PersonalShopListings";
    private const string ExchangeOrdersTable = "ExchangeOrders";
    private const string TransactionsTable = "Transactions";
    private const string DungeonProgressTable = "DungeonProgress";

    public DynamoDBService(string accessKey, string secretKey, string region)
    {
        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        _client = new AmazonDynamoDBClient(credentials, regionEndpoint);
    }

    // PersonalShopListing
    public async Task<bool> SavePersonalShopListingAsync(PersonalShopListing listing)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "ListingId", new AttributeValue { S = listing.Id } },
                { "PlayerId", new AttributeValue { S = listing.PlayerId } },
                { "ItemTemplateId", new AttributeValue { S = listing.ItemTemplateId } },
                { "Quantity", new AttributeValue { N = listing.Quantity.ToString() } },
                { "UnitPrice", new AttributeValue { N = listing.UnitPrice.ToString() } },
                { "ListedTime", new AttributeValue { S = listing.ListedTime.ToString("O") } },
                { "ExpireTime", new AttributeValue { S = listing.ExpireTime.ToString("O") } },
                { "Status", new AttributeValue { S = listing.Status.ToString() } },
                { "TotalGoldOnSale", new AttributeValue { N = listing.TotalGoldOnSale.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = PersonalShopTable,
                Item = item
            };

            await _client.PutItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"PersonalShopListing 저장 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<PersonalShopListing> GetPersonalShopListingAsync(string listingId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = PersonalShopTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ListingId", new AttributeValue { S = listingId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            return new PersonalShopListing
            {
                Id = response.Item["ListingId"].S,
                PlayerId = response.Item["PlayerId"].S,
                ItemTemplateId = response.Item["ItemTemplateId"].S,
                Quantity = int.Parse(response.Item["Quantity"].N),
                UnitPrice = long.Parse(response.Item["UnitPrice"].N),
                ListedTime = DateTime.Parse(response.Item["ListedTime"].S),
                ExpireTime = DateTime.Parse(response.Item["ExpireTime"].S),
                Status = Enum.Parse<ListingStatus>(response.Item["Status"].S),
                TotalGoldOnSale = long.Parse(response.Item["TotalGoldOnSale"].N)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"PersonalShopListing 조회 실패: {ex.Message}");
            return null;
        }
    }

    public async Task<List<PersonalShopListing>> GetPlayerListingsAsync(string playerId)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = PersonalShopTable,
                FilterExpression = "PlayerId = :playerId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":playerId", new AttributeValue { S = playerId } }
                }
            };

            var response = await _client.ScanAsync(request);
            var listings = new List<PersonalShopListing>();

            foreach (var item in response.Items)
            {
                listings.Add(new PersonalShopListing
                {
                    Id = item["ListingId"].S,
                    PlayerId = item["PlayerId"].S,
                    ItemTemplateId = item["ItemTemplateId"].S,
                    Quantity = int.Parse(item["Quantity"].N),
                    UnitPrice = long.Parse(item["UnitPrice"].N),
                    ListedTime = DateTime.Parse(item["ListedTime"].S),
                    ExpireTime = DateTime.Parse(item["ExpireTime"].S),
                    Status = Enum.Parse<ListingStatus>(item["Status"].S),
                    TotalGoldOnSale = long.Parse(item["TotalGoldOnSale"].N)
                });
            }

            return listings;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"플레이어 Listings 조회 실패: {ex.Message}");
            return new List<PersonalShopListing>();
        }
    }

    public async Task<bool> DeletePersonalShopListingAsync(string listingId)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = PersonalShopTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ListingId", new AttributeValue { S = listingId } }
                }
            };

            await _client.DeleteItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"PersonalShopListing 삭제 실패: {ex.Message}");
            return false;
        }
    }

    // ExchangeOrder
    public async Task<bool> SaveExchangeOrderAsync(ExchangeOrder order)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "OrderId", new AttributeValue { S = order.Id } },
                { "SellerId", new AttributeValue { S = order.SellerId } },
                { "ItemTemplateId", new AttributeValue { S = order.ItemTemplateId } },
                { "Quantity", new AttributeValue { N = order.Quantity.ToString() } },
                { "UnitPrice", new AttributeValue { N = order.UnitPrice.ToString() } },
                { "Remaining", new AttributeValue { N = order.Remaining.ToString() } },
                { "ListedTime", new AttributeValue { S = order.ListedTime.ToString("O") } },
                { "Status", new AttributeValue { S = order.Status.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = ExchangeOrdersTable,
                Item = item
            };

            await _client.PutItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ExchangeOrder 저장 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<ExchangeOrder> GetExchangeOrderAsync(string orderId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = ExchangeOrdersTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "OrderId", new AttributeValue { S = orderId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            return new ExchangeOrder
            {
                Id = response.Item["OrderId"].S,
                SellerId = response.Item["SellerId"].S,
                ItemTemplateId = response.Item["ItemTemplateId"].S,
                Quantity = int.Parse(response.Item["Quantity"].N),
                UnitPrice = long.Parse(response.Item["UnitPrice"].N),
                Remaining = int.Parse(response.Item["Remaining"].N),
                ListedTime = DateTime.Parse(response.Item["ListedTime"].S),
                Status = Enum.Parse<OrderStatus>(response.Item["Status"].S)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ExchangeOrder 조회 실패: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ExchangeOrder>> GetExchangeOrdersByItemAsync(string itemTemplateId)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = ExchangeOrdersTable,
                FilterExpression = "ItemTemplateId = :itemId AND #status = :active",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#status", "Status" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":itemId", new AttributeValue { S = itemTemplateId } },
                    { ":active", new AttributeValue { S = "Active" } }
                }
            };

            var response = await _client.ScanAsync(request);
            var orders = new List<ExchangeOrder>();

            foreach (var item in response.Items)
            {
                orders.Add(new ExchangeOrder
                {
                    Id = item["OrderId"].S,
                    SellerId = item["SellerId"].S,
                    ItemTemplateId = item["ItemTemplateId"].S,
                    Quantity = int.Parse(item["Quantity"].N),
                    UnitPrice = long.Parse(item["UnitPrice"].N),
                    Remaining = int.Parse(item["Remaining"].N),
                    ListedTime = DateTime.Parse(item["ListedTime"].S),
                    Status = Enum.Parse<OrderStatus>(item["Status"].S)
                });
            }

            return orders;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"아이템별 Orders 조회 실패: {ex.Message}");
            return new List<ExchangeOrder>();
        }
    }

    public async Task<List<ExchangeOrder>> GetPlayerOrdersAsync(string playerId)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = ExchangeOrdersTable,
                FilterExpression = "SellerId = :playerId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":playerId", new AttributeValue { S = playerId } }
                }
            };

            var response = await _client.ScanAsync(request);
            var orders = new List<ExchangeOrder>();

            foreach (var item in response.Items)
            {
                orders.Add(new ExchangeOrder
                {
                    Id = item["OrderId"].S,
                    SellerId = item["SellerId"].S,
                    ItemTemplateId = item["ItemTemplateId"].S,
                    Quantity = int.Parse(item["Quantity"].N),
                    UnitPrice = long.Parse(item["UnitPrice"].N),
                    Remaining = int.Parse(item["Remaining"].N),
                    ListedTime = DateTime.Parse(item["ListedTime"].S),
                    Status = Enum.Parse<OrderStatus>(item["Status"].S)
                });
            }

            return orders;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"플레이어 Orders 조회 실패: {ex.Message}");
            return new List<ExchangeOrder>();
        }
    }

    public async Task<bool> DeleteExchangeOrderAsync(string orderId)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = ExchangeOrdersTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "OrderId", new AttributeValue { S = orderId } }
                }
            };

            await _client.DeleteItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ExchangeOrder 삭제 실패: {ex.Message}");
            return false;
        }
    }

    // Transaction
    public async Task<bool> SaveTransactionAsync(Transaction transaction)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "TransactionId", new AttributeValue { S = transaction.Id } },
                { "BuyerId", new AttributeValue { S = transaction.BuyerId } },
                { "SellerId", new AttributeValue { S = transaction.SellerId } },
                { "OrderId", new AttributeValue { S = transaction.OrderId } },
                { "ItemTemplateId", new AttributeValue { S = transaction.ItemTemplateId } },
                { "Quantity", new AttributeValue { N = transaction.Quantity.ToString() } },
                { "UnitPrice", new AttributeValue { N = transaction.UnitPrice.ToString() } },
                { "TotalGold", new AttributeValue { N = transaction.TotalGold.ToString() } },
                { "TransactionTime", new AttributeValue { S = transaction.TransactionTime.ToString("O") } },
                { "Type", new AttributeValue { S = transaction.Type.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = TransactionsTable,
                Item = item
            };

            await _client.PutItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Transaction 저장 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Transaction>> GetPlayerTransactionsAsync(string playerId)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = TransactionsTable,
                FilterExpression = "BuyerId = :playerId OR SellerId = :playerId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":playerId", new AttributeValue { S = playerId } }
                }
            };

            var response = await _client.ScanAsync(request);
            var transactions = new List<Transaction>();

            foreach (var item in response.Items)
            {
                transactions.Add(new Transaction
                {
                    Id = item["TransactionId"].S,
                    BuyerId = item["BuyerId"].S,
                    SellerId = item["SellerId"].S,
                    OrderId = item["OrderId"].S,
                    ItemTemplateId = item["ItemTemplateId"].S,
                    Quantity = int.Parse(item["Quantity"].N),
                    UnitPrice = long.Parse(item["UnitPrice"].N),
                    TotalGold = long.Parse(item["TotalGold"].N),
                    TransactionTime = DateTime.Parse(item["TransactionTime"].S),
                    Type = Enum.Parse<TransactionType>(item["Type"].S)
                });
            }

            return transactions;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"플레이어 Transactions 조회 실패: {ex.Message}");
            return new List<Transaction>();
        }
    }

    // DungeonProgress
    public async Task<bool> SaveDungeonProgressAsync(DungeonProgress progress)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "ProgressId", new AttributeValue { S = progress.Id } },
                { "MercenaryId", new AttributeValue { S = progress.MercenaryId } },
                { "DungeonId", new AttributeValue { S = progress.DungeonId } },
                { "StartTime", new AttributeValue { S = progress.StartTime.ToString("O") } },
                { "EndTime", new AttributeValue { S = progress.EndTime.ToString("O") } },
                { "Status", new AttributeValue { S = progress.Status.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = DungeonProgressTable,
                Item = item
            };

            await _client.PutItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DungeonProgress 저장 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<DungeonProgress> GetDungeonProgressAsync(string progressId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = DungeonProgressTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ProgressId", new AttributeValue { S = progressId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            return new DungeonProgress
            {
                Id = response.Item["ProgressId"].S,
                MercenaryId = response.Item["MercenaryId"].S,
                DungeonId = response.Item["DungeonId"].S,
                StartTime = DateTime.Parse(response.Item["StartTime"].S),
                EndTime = DateTime.Parse(response.Item["EndTime"].S),
                Status = Enum.Parse<DungeonProgressStatus>(response.Item["Status"].S)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DungeonProgress 조회 실패: {ex.Message}");
            return null;
        }
    }

    public async Task<List<DungeonProgress>> GetMercenaryProgressAsync(string mercenaryId)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = DungeonProgressTable,
                FilterExpression = "MercenaryId = :mercId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":mercId", new AttributeValue { S = mercenaryId } }
                }
            };

            var response = await _client.ScanAsync(request);
            var progressList = new List<DungeonProgress>();

            foreach (var item in response.Items)
            {
                progressList.Add(new DungeonProgress
                {
                    Id = item["ProgressId"].S,
                    MercenaryId = item["MercenaryId"].S,
                    DungeonId = item["DungeonId"].S,
                    StartTime = DateTime.Parse(item["StartTime"].S),
                    EndTime = DateTime.Parse(item["EndTime"].S),
                    Status = Enum.Parse<DungeonProgressStatus>(item["Status"].S)
                });
            }

            return progressList;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"용병 Progress 조회 실패: {ex.Message}");
            return new List<DungeonProgress>();
        }
    }

    public async Task<bool> DeleteDungeonProgressAsync(string progressId)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = DungeonProgressTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ProgressId", new AttributeValue { S = progressId } }
                }
            };

            await _client.DeleteItemAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DungeonProgress 삭제 실패: {ex.Message}");
            return false;
        }
    }

    // Master Data
    public async Task<List<ItemTemplate>> GetAllItemTemplatesAsync()
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = "ItemTemplates"
            };

            var response = await _client.ScanAsync(request);
            var templates = new List<ItemTemplate>();

            foreach (var item in response.Items)
            {
                templates.Add(new ItemTemplate
                {
                    Id = item["ItemTemplateId"].S,
                    Name = item["Name"].S,
                    Type = Enum.Parse<ItemType>(item["Type"].S),
                    Rarity = Enum.Parse<ItemRarity>(item["Rarity"].S),
                    BasePrice = long.Parse(item["BasePrice"].N),
                    Description = item.ContainsKey("Description") ? item["Description"].S : ""
                });
            }

            return templates;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ItemTemplates 조회 실패: {ex.Message}");
            return new List<ItemTemplate>();
        }
    }

    public async Task<ItemTemplate> GetItemTemplateAsync(string itemTemplateId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = "ItemTemplates",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ItemTemplateId", new AttributeValue { S = itemTemplateId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            return new ItemTemplate
            {
                Id = response.Item["ItemTemplateId"].S,
                Name = response.Item["Name"].S,
                Type = Enum.Parse<ItemType>(response.Item["Type"].S),
                Rarity = Enum.Parse<ItemRarity>(response.Item["Rarity"].S),
                BasePrice = long.Parse(response.Item["BasePrice"].N),
                Description = response.Item.ContainsKey("Description") ? response.Item["Description"].S : ""
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ItemTemplate 조회 실패: {ex.Message}");
            return null;
        }
    }

    public async Task<List<dynamic>> GetAllRecipesAsync()
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = "Recipes"
            };

            var response = await _client.ScanAsync(request);
            var recipes = new List<dynamic>();

            foreach (var item in response.Items)
            {
                var requiredItems = new Dictionary<string, int>();
                if (item.ContainsKey("RequiredItems") && item["RequiredItems"].M != null)
                {
                    foreach (var req in item["RequiredItems"].M)
                    {
                        requiredItems[req.Key] = int.Parse(req.Value.N);
                    }
                }

                recipes.Add(new
                {
                    Id = item["RecipeId"].S,
                    Name = item["Name"].S,
                    OutputItem = item["OutputItem"].S,
                    OutputQuantity = int.Parse(item["OutputQuantity"].N),
                    RequiredItems = requiredItems
                });
            }

            return recipes;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Recipes 조회 실패: {ex.Message}");
            return new List<dynamic>();
        }
    }

    public async Task<dynamic> GetRecipeAsync(string recipeId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = "Recipes",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "RecipeId", new AttributeValue { S = recipeId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            var requiredItems = new Dictionary<string, int>();
            if (response.Item.ContainsKey("RequiredItems") && response.Item["RequiredItems"].M != null)
            {
                foreach (var req in response.Item["RequiredItems"].M)
                {
                    requiredItems[req.Key] = int.Parse(req.Value.N);
                }
            }

            return new
            {
                Id = response.Item["RecipeId"].S,
                Name = response.Item["Name"].S,
                OutputItem = response.Item["OutputItem"].S,
                OutputQuantity = int.Parse(response.Item["OutputQuantity"].N),
                RequiredItems = requiredItems
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Recipe 조회 실패: {ex.Message}");
            return null;
        }
    }

    public async Task<List<dynamic>> GetAllDungeonsAsync()
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = "Dungeons"
            };

            var response = await _client.ScanAsync(request);
            var dungeons = new List<dynamic>();

            foreach (var item in response.Items)
            {
                dungeons.Add(new
                {
                    Id = item["DungeonId"].S,
                    Name = item["Name"].S,
                    Level = int.Parse(item["Level"].N),
                    BaseRewardTime = int.Parse(item["BaseRewardTime"].N)
                });
            }

            return dungeons;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Dungeons 조회 실패: {ex.Message}");
            return new List<dynamic>();
        }
    }

    public async Task<dynamic> GetDungeonAsync(string dungeonId)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = "Dungeons",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "DungeonId", new AttributeValue { S = dungeonId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            if (!response.IsItemSet) return null;

            return new
            {
                Id = response.Item["DungeonId"].S,
                Name = response.Item["Name"].S,
                Level = int.Parse(response.Item["Level"].N),
                BaseRewardTime = int.Parse(response.Item["BaseRewardTime"].N)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Dungeon 조회 실패: {ex.Message}");
            return null;
        }
    }
}