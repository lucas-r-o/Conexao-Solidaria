using Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class BusExtensions
    {
        private const string SECTION_BUS = "Bus";
        private const string SUBSECTION_PORT = "Port";
        private const string SUBSECTION_HOST = "Host";
        private const string SUBSECTION_USER = "User";
        private const string SUBSECTION_PASSWORD = "Password";

        public static void AddBus(this IServiceCollection services, IConfiguration config)
        {
            var busSection = config.GetSection(SECTION_BUS);

            var host = busSection.GetRequiredSection(SUBSECTION_HOST)?.Value ?? throw new ArgumentNullException(nameof(SUBSECTION_HOST));
            var port = busSection.GetRequiredSection(SUBSECTION_PORT)?.Value ?? throw new ArgumentNullException(nameof(SUBSECTION_PORT));
            var user = busSection.GetRequiredSection(SUBSECTION_USER)?.Value ?? throw new ArgumentNullException(nameof(SUBSECTION_USER));
            var password = busSection.GetRequiredSection(SUBSECTION_PASSWORD)?.Value ?? throw new ArgumentNullException(nameof(SUBSECTION_PASSWORD));

            services.AddMassTransit(x =>
            {
                x.AddConsumer<ArrecadacaoAtualizadaConsumer>();
                x.AddConsumer<DoacaoRecusadaConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigurePublish(pipe =>
                    {
                        pipe.UseCircuitBreaker(cb =>
                        {
                            cb.TrackingPeriod = TimeSpan.FromSeconds(30);
                            cb.TripThreshold = 50; 
                            cb.ActiveThreshold = 5; 
                            cb.ResetInterval = TimeSpan.FromMinutes(1);
                        });
                        pipe.UseRetry(r =>
                        {
                            r.Intervals(
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3)
                            );
                        });

                        pipe.UseExecute(ctx =>
                        {
                            ctx.TimeToLive = TimeSpan.FromMinutes(10);
                            ctx.Durable = true;
                        });
                    });
                    cfg.UseJsonSerializer(); // força serializer padrão do MassTransit
                    cfg.ConfigureJsonSerializerOptions(options =>
                    {
                        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
                        return options;
                    });
                    cfg.Host(host, ushort.Parse(port), "/", h =>
                    {
                        h.Username(user);
                        h.Password(password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
