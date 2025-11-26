using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopOwnerSimulator;
using ShopOwnerSimulator.Services.Interfaces;
using ShopOwnerSimulator.Services.Implementations;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register service implementations (stubs) using Type-based APIs to avoid ambiguous type conflicts
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IGameService), typeof(ShopOwnerSimulator.Services.Implementations.GameService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IDungeonService), typeof(ShopOwnerSimulator.Services.Implementations.DungeonService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.ICraftingService), typeof(ShopOwnerSimulator.Services.Implementations.CraftingService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IExchangeService), typeof(ShopOwnerSimulator.Services.Implementations.ExchangeService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IMercenaryService), typeof(ShopOwnerSimulator.Services.Implementations.MercenaryService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IInventoryService), typeof(ShopOwnerSimulator.Services.Implementations.InventoryService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IPersonalShopService), typeof(ShopOwnerSimulator.Services.Implementations.PersonalShopService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IPlayFabService), typeof(ShopOwnerSimulator.Services.Implementations.PlayFabService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IStorageService), typeof(ShopOwnerSimulator.Services.Implementations.StorageService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Interfaces.IStateService), typeof(ShopOwnerSimulator.Services.Implementations.StateService));
builder.Services.AddSingleton(typeof(ShopOwnerSimulator.Services.Implementations.TimerService));

await builder.Build().RunAsync();
