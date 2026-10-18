using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class CorsExtensions
    {
        private const string CorsPolicyName = "AllowFront";
        private const string AllowedOriginsConfigurationKey = "Cors:AllowedOrigins";

        public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection(AllowedOriginsConfigurationKey)
                .GetChildren()
                .Select(section => section.Value)
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => NormalizeOrigin(origin!))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (allowedOrigins.Length == 0)
                throw new InvalidOperationException($"A configuração '{AllowedOriginsConfigurationKey}' deve conter pelo menos uma origem válida.");

            services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
        }

        private static string NormalizeOrigin(string origin)
        {
            origin = origin.Trim();

            if (origin == "*" || origin.Contains('*'))
                throw new InvalidOperationException($"A origem CORS '{origin}' é inválida. Não é permitido usar curingas com credenciais habilitadas.");

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrWhiteSpace(uri.Host) ||
                !string.IsNullOrEmpty(uri.UserInfo) ||
                !string.IsNullOrEmpty(uri.Query) ||
                !string.IsNullOrEmpty(uri.Fragment) ||
                uri.AbsolutePath != "/")
            {
                throw new InvalidOperationException(
                    $"A origem CORS '{origin}' é inválida. Informe uma URL absoluta HTTP ou HTTPS sem caminho, query ou fragmento.");
            }

            return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }
    }
}
