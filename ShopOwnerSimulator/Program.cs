using ShopOwnerSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on the port provided by the environment (App Runner sets PORT),
// fallback to 8080 for local runs.
var portEnv = Environment.GetEnvironmentVariable("PORT");
var port = 8080;
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var p))
{
    port = p;
}
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(port);
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSingleton<DataService>();
// Register PlayFabService (SDK-based) so IConfiguration is injected into its constructor
builder.Services.AddSingleton<PlayFabService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<LambdaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


// Expose a lightweight health endpoint before HTTPS redirection so platform health checks
// (which usually use plain HTTP) aren't redirected to HTTPS and can succeed.
app.MapGet("/health", () => Results.Text("OK"));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();