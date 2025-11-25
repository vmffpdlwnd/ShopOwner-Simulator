using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopOwnerSimulator.Client;
using ShopOwnerSimulator.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API 서버 주소 설정 (개발 환경에서는 localhost:5000)
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5000/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
