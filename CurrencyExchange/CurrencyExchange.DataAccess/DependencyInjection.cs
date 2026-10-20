using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.DataAccess;

public static class DependencyInjection
{
    private const string DatabaseNamePrefix = "CurrencyExchangeDb";

    public static IServiceCollection AddCurrencyExchangeDataAccess(this IServiceCollection services)
    {
        var databaseName = $"{DatabaseNamePrefix}-{Guid.NewGuid():N}";

        services.AddDbContext<CurrencyExchangeDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IExchangeRateProvider, EfExchangeRateProvider>();

        return services;
    }

    public static async Task InitializeCurrencyExchangeDataAccessAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var dbContext = services.GetRequiredService<CurrencyExchangeDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
