using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Linq;

namespace TextMate.Middlewares
{
    public class IpRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public IpRateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIP = GetClientIP(context);
            var cacheKey = $"ratelimit_{clientIP}";
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-1);

            // Get or create a list of request timestamps for this IP
            var requests = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return new ConcurrentQueue<DateTime>();
            });

            requests.Enqueue(now);

            // Determine limit based on path
            int limit = 10;

            // Remove timestamps outside the rolling 1-minute window
            while (requests.TryPeek(out var oldest) && oldest <= windowStart)
                requests.TryDequeue(out _);

            // remove timestamps outside window
            while (requests.Count > limit * 2 && requests.TryDequeue(out _)) { }

            int count = requests.Count;

            Console.WriteLine($"[RateLimit] {clientIP} on {context.Request.Path} | {count}/{limit} reqs/min");

            if (count > limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            await _next(context);
        }

        private string GetClientIP(HttpContext context)
        {
            // Try to get forwarded IP (Render, Cloudflare, Nginx, etc.)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Sometimes contains multiple IPs; take the first one
                return forwardedFor.Split(',').First().Trim();
            }

            // Fallback to direct connection IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

        public static class IpRateLimitingMiddlewareExtensions
        {
            public static IApplicationBuilder UseIpRateLimiting(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<IpRateLimitingMiddleware>();
            }
        }
    }

