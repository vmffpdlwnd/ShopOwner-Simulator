using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopOwnerSimulator;
using ShopOwnerSimulator.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for general purposes
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// PlayFab client service
builder.Services.AddScoped<PlayFabClientService>();

// AWS Lambda service (게임 로직용)
builder.Services.AddScoped<LambdaService>();

await builder.Build().RunAsync();