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
string Normalize(string? v, string envName)
{
    if (string.IsNullOrWhiteSpace(v)) return Environment.GetEnvironmentVariable(envName) ?? string.Empty;
    // If the value is a placeholder like ${PLAYFAB_TITLE_ID}, treat it as empty
    if (v.Contains("${") && v.Contains("}")) return Environment.GetEnvironmentVariable(envName) ?? string.Empty;
    return v;
}

var playFabTitleId = Normalize(config["PlayFab:TitleId"], "PLAYFAB_TITLE_ID");
var playFabSecretKey = Normalize(config["PlayFab:SecretKey"], "PLAYFAB_SECRET_KEY");

if (string.IsNullOrWhiteSpace(playFabTitleId)) playFabTitleId = null;
if (string.IsNullOrWhiteSpace(playFabSecretKey)) playFabSecretKey = null;

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
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IMercenaryService, MercenaryService>();
builder.Services.AddScoped<IPersonalShopService, PersonalShopService>();

// Global State
builder.Services.AddScoped<GameState>();
builder.Services.AddScoped<InventoryState>();
builder.Services.AddScoped<MercenaryState>();
builder.Services.AddScoped<TimerState>();

// Logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// NOTE: Do not auto-initialize GameState here. Initialization occurs after user chooses
// to Login or Play as Guest so we can support an optional (ephemeral) guest mode.
await host.RunAsync();