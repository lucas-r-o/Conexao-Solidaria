using ApplicationServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationServices.Services
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICampaignService, CampaignService>();
        }
    }
}
