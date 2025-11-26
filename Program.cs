using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register placeholder services (implementations are stubs)
builder.Services.AddSingleton<Services.IGameService, Services.GameService>();
builder.Services.AddSingleton<Services.IMarketService, Services.MarketService>();
builder.Services.AddSingleton<Services.IMercenaryService, Services.MercenaryService>();
builder.Services.AddSingleton<Services.IEquipmentService, Services.EquipmentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
