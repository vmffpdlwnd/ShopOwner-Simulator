using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopOwnerSimulator;
using ShopOwnerSimulator.Services;
using ShopOwnerSimulator.Services.Implementations;
using ShopOwnerSimulator.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root component
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuration
var config = builder.Configuration;

// HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// PlayFab Configuration
var playFabTitleId = config["PlayFab:TitleId"] ?? Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID");
var playFabSecretKey = config["PlayFab:SecretKey"] ?? Environment.GetEnvironmentVariable("PLAYFAB_SECRET_KEY");

// Services - Interfaces & Implementations
builder.Services.AddScoped<IPlayFabService>(sp => 
    new PlayFabService(playFabTitleId, playFabSecretKey));

builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<ITimerService, TimerService>();

// Game Services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IDungeonService, DungeonService>();
builder.Services.AddScoped<ICraftingService, CraftingService>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IMercenaryService, MercenaryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPersonalShopService, PersonalShopService>();

// Global State
builder.Services.AddScoped<GameState>();
builder.Services.AddScoped<InventoryState>();
builder.Services.AddScoped<MercenaryState>();
builder.Services.AddScoped<TimerState>();

// Logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// Initialize app state on startup
var gameState = host.Services.GetRequiredService<GameState>();
await gameState.InitializeAsync();

await host.RunAsync();