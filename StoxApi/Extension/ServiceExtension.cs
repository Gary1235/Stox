

namespace StoxApi.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 在這裡統一註冊你的服務
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFinnhubService, FinnhubService>();
            services.AddHttpClient<FinnhubService>();
            return services;
        }
    }
}