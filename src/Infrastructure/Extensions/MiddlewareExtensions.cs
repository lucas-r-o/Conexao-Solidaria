using Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void AddErrorMiddleware(this WebApplication? app) => app?.UseMiddleware<ErrorMiddleware>();
    }
}
