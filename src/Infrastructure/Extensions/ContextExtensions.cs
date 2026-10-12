using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class ContextExtensions
    {
        private const string CONEXAO_SOLIDARIA = "CONEXAO_SOLIDARIA";
        public static void AddContexts(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(CONEXAO_SOLIDARIA) ?? throw new ArgumentNullException(nameof(CONEXAO_SOLIDARIA));

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        }
    }
}
