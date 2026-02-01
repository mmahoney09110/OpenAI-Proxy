using Microsoft.AspNetCore.HttpOverrides;
using OpenAI.Examples;
using TextMate.Middlewares;

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

// Add memory cache for rate limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
// Rate Limiting implemented with IpRateLimitingMiddleware for:
// - Global: 100 requests/minute per IP
// - Authentication pages (/Identity): 5 requests/minute per IP
// - SMS webhooks (/sms): 10 requests/minute per IP
// (Static assets are not rate limited)
app.UseIpRateLimiting();

app.UseRouting();

app.MapControllers();

app.Run();
