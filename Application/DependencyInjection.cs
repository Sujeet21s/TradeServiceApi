using TradeServiceApi.Endpoints.ServiceEndpointHandler;
using TradeServiceApi.Repositories;
using TradeServiceApi.Services;

namespace TradeServiceApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
                                        IConfiguration configuration)
        {
            services.AddScoped<ServiceEndpointHandler>();
            services.AddSingleton<IInMemoryRepository, InMemoryRepository>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
