using Microsoft.Extensions.DependencyInjection;

namespace AJP.ElasticBand
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddElasticBand(this IServiceCollection services)
        {
            services
                .AddHttpClient()
                .AddSingleton<IElasticQueryBuilder, ElasticQueryBuilder>()
                .AddSingleton<IElasticBand, ElasticBand>();

            return services;
        }        
    }
}
