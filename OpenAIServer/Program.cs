using AspNetCoreRateLimit;
using OpenAI.Examples;

var _configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddRazorPages();

// 1) Register your services
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiServiceVectorStore>();

// Add IP Rate Limiting
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add the rate limit middleware
builder.Services.AddOptions();
builder.Services.AddMemoryCache();

// Add Rate Limiting policies
builder.Services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));

var app = builder.Build();

// 2) (Optional) redirect to HTTPS
app.UseHttpsRedirection();

// 3) Routing must come before MapControllers
app.UseRouting();

// 4) auth goes here
//app.UseAuthorization();

// 5) Map API controllers
app.MapControllers();

// 6) (Optional) static pages, razor, etc.
// Enable IP rate limiting
app.UseIpRateLimiting();
//app.MapStaticAssets();
//app.MapRazorPages().WithStaticAssets();

app.Run();
