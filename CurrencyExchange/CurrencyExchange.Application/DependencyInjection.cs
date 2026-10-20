using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrencyExchangeApplication(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();

        return services;
    }
}
