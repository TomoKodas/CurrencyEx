using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.DataAccess.Tests;

public class EfExchangeRateProviderTests
{
    [Fact]
    public async Task GetRequiredRateAsync_WhenCurrencyIsSeeded_ReturnsSeededRate()
    {
        await using var dbContext = TestData.CreateDbContext();
        var provider = new EfExchangeRateProvider(dbContext);

        var rate = await provider.GetRequiredRateAsync(Currency.From("EUR"));

        Assert.Equal(Currency.From("EUR"), rate.Currency);
        Assert.Equal(Currency.From("DKK"), rate.BaseCurrency);
        Assert.Equal(100, rate.UnitAmount);
        Assert.Equal(743.94m, rate.BaseCurrencyAmount);
    }

    [Fact]
    public async Task GetRequiredRateAsync_WhenCurrencyIsBase_ReturnsIdentityRate()
    {
        await using var dbContext = TestData.CreateDbContext();
        var provider = new EfExchangeRateProvider(dbContext);

        var rate = await provider.GetRequiredRateAsync(Currency.From("DKK"));

        Assert.Equal(rate.Currency, rate.BaseCurrency);
        Assert.Equal(rate.UnitAmount, rate.BaseCurrencyAmount);
    }

    [Fact]
    public async Task GetRequiredRateAsync_WhenCurrencyIsUnavailable_ThrowsUnsupportedCurrencyException()
    {
        await using var dbContext = TestData.CreateDbContext();
        var provider = new EfExchangeRateProvider(dbContext);

        await Assert.ThrowsAsync<UnsupportedCurrencyException>(() =>
            provider.GetRequiredRateAsync(Currency.From("AAA")));
    }
}
