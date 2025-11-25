using ShopOwnerSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on the port provided by the environment
var portEnv = Environment.GetEnvironmentVariable("PORT");
var port = 5000;
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var p))
{
    port = p;
}
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(port);
});

// Add services to the container - API only
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Blazor client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5100", 
            "https://localhost:5101",
            "https://*.pages.dev"  // Cloudflare Pages
        )
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Register services
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<PlayFabService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<LambdaService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check endpoint
app.MapGet("/health", () => Results.Text("OK"));

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();