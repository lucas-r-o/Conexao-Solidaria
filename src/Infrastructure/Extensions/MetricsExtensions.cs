using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Infrastructure.Extensions
{
    public static class MetricsExtensions
    {
        public static void AddMetrics(this WebApplication builder)
        {
            builder.UseHttpMetrics();
            builder.MapMetrics();
        }
    }
}
