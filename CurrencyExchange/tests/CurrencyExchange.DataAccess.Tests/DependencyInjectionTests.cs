using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.DataAccess.Entities;
using CurrencyExchange.DataAccess.Persistence;
using CurrencyExchange.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.DataAccess.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddCurrencyExchangeDataAccess_RegistersEfExchangeRateProvider()
    {
        using var services = new ServiceCollection()
            .AddCurrencyExchangeDataAccess()
            .BuildServiceProvider();
        using var scope = services.CreateScope();

        var provider = scope.ServiceProvider.GetRequiredService<IExchangeRateProvider>();

        Assert.IsType<EfExchangeRateProvider>(provider);
    }

    [Fact]
    public async Task AddCurrencyExchangeDataAccess_WhenCalledForTwoContainers_UsesSeparateDatabases()
    {
        using var first = new ServiceCollection().AddCurrencyExchangeDataAccess().BuildServiceProvider();
        using var second = new ServiceCollection().AddCurrencyExchangeDataAccess().BuildServiceProvider();

        await using (var scope = first.CreateAsyncScope())
        {
            await scope.ServiceProvider.InitializeCurrencyExchangeDataAccessAsync();
            var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyExchangeDbContext>();
            dbContext.ExchangeRates.Add(new ExchangeRateEntity
            {
                CurrencyIsoCode = "PLN",
                BaseCurrencyIsoCode = "DKK",
                UnitAmount = 100,
                BaseCurrencyAmount = 170m
            });
            await dbContext.SaveChangesAsync();
        }

        await using var secondScope = second.CreateAsyncScope();
        await secondScope.ServiceProvider.InitializeCurrencyExchangeDataAccessAsync();
        var secondProvider = secondScope.ServiceProvider.GetRequiredService<IExchangeRateProvider>();

        await Assert.ThrowsAsync<UnsupportedCurrencyException>(() =>
            secondProvider.GetRequiredRateAsync(Currency.From("PLN")));
    }
}
