using System.Globalization;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Tests;

public class CurrencyExchangeServiceTests
{
    private const decimal UnitAmount = 100m;

    [Fact]
    public async Task ExchangeAsync_WhenTargetIsBaseCurrency_UsesSourceRate()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("EUR", "DKK", UnitAmount, 743.94m);
        rates.AddRate("DKK", "DKK", UnitAmount, UnitAmount);
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From("EUR/DKK"),
            100);

        Assert.Equal(743.94m, result.Amount);
        Assert.Equal(Currency.From("DKK"), result.Currency);
    }

    [Fact]
    public async Task ExchangeAsync_WhenSourceIsBaseCurrency_UsesTargetRate()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("DKK", "DKK", UnitAmount, UnitAmount);
        rates.AddRate("EUR", "DKK", UnitAmount, 743.94m);
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From("DKK/EUR"),
            743.94m);

        Assert.Equal(100m, result.Amount);
        Assert.Equal(Currency.From("EUR"), result.Currency);
    }

    [Fact]
    public async Task ExchangeAsync_WhenCurrenciesAreNotBase_UsesBothRates()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("EUR", "DKK", UnitAmount, 743.94m);
        rates.AddRate("USD", "DKK", UnitAmount, 663.11m);
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From("EUR/USD"),
            100);

        Assert.Equal(112.1895m, result.Amount);
        Assert.Equal(Currency.From("USD"), result.Currency);
    }

    [Fact]
    public async Task ExchangeAsync_WhenCurrenciesAreEqual_ReturnsOriginalAmount()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("EUR", "DKK", UnitAmount, 743.94m);
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From("EUR/EUR"),
            12.34567m);

        Assert.Equal(12.34567m, result.Amount);
        Assert.Equal(Currency.From("EUR"), result.Currency);
    }

    [Fact]
    public async Task ExchangeAsync_WhenSourceCurrencyIsUnavailable_ThrowsUnsupportedCurrencyException()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("DKK", "DKK", UnitAmount, UnitAmount);
        var service = new CurrencyExchangeService(rates);

        await Assert.ThrowsAsync<UnsupportedCurrencyException>(() =>
            service.ExchangeAsync(CurrencyPair.From("AAA/DKK"), 1));
    }

    [Fact]
    public async Task ExchangeAsync_WhenTargetCurrencyIsUnavailable_ThrowsUnsupportedCurrencyException()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("DKK", "DKK", UnitAmount, UnitAmount);
        var service = new CurrencyExchangeService(rates);

        await Assert.ThrowsAsync<UnsupportedCurrencyException>(() =>
            service.ExchangeAsync(CurrencyPair.From("DKK/AAA"), 1));
    }

    [Fact]
    public async Task ExchangeAsync_WhenAmountIsNegative_ThrowsInvalidAmountException()
    {
        var service = new CurrencyExchangeService(
            new FakeExchangeRateProvider());

        await Assert.ThrowsAsync<InvalidAmountException>(() =>
            service.ExchangeAsync(CurrencyPair.From("EUR/DKK"), -1));
    }

    [Fact]
    public async Task ExchangeAsync_WhenBaseCurrenciesDiffer_ThrowsIncompatibleBaseCurrencyException()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("EUR", "DKK", UnitAmount, 743.94m);
        rates.AddRate("USD", "SEK", UnitAmount, 984.11m);
        var service = new CurrencyExchangeService(rates);

        await Assert.ThrowsAsync<IncompatibleBaseCurrencyException>(() =>
            service.ExchangeAsync(CurrencyPair.From("EUR/USD"), 1));
    }

    [Fact]
    public async Task ExchangeAsync_WhenResultOverflows_ThrowsInvalidAmountException()
    {
        var service = new CurrencyExchangeService(CreateExerciseRates());

        await Assert.ThrowsAsync<InvalidAmountException>(() =>
            service.ExchangeAsync(CurrencyPair.From("EUR/JPY"), decimal.MaxValue));
    }

    [Fact]
    public async Task ExchangeAsync_WhenResultHasMoreThanFourDecimals_RoundsToFourDecimals()
    {
        var rates = new FakeExchangeRateProvider();
        rates.AddRate("EUR", "DKK", UnitAmount, 123.445m);
        rates.AddRate("DKK", "DKK", UnitAmount, UnitAmount);
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From("EUR/DKK"),
            1);

        Assert.Equal(1.2344m, result.Amount);
    }

    [Theory]
    [MemberData(nameof(AllSupportedCurrencyPairs))]
    public async Task ExchangeAsync_WhenPairIsSupported_CalculatesExpectedAmount(
        string sourceCode,
        string targetCode,
        decimal expectedAmount)
    {
        var rates = CreateExerciseRates();
        var service = new CurrencyExchangeService(rates);

        var result = await service.ExchangeAsync(
            CurrencyPair.From($"{sourceCode}/{targetCode}"),
            123.45m);

        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(targetCode, result.Currency.IsoCode);
    }

    [Theory]
    [InlineData("EUR", "DKK", "7439.40")]
    [InlineData("DKK", "EUR", "134.4194")]
    [InlineData("EUR", "USD", "1121.8953")]
    [InlineData("USD", "EUR", "891.3488")]
    [InlineData("GBP", "JPY", "142760.2946")]
    [InlineData("JPY", "GBP", "7.0047")]
    [InlineData("SEK", "NOK", "970.6633")]
    [InlineData("NOK", "CHF", "114.6903")]
    [InlineData("CHF", "SEK", "8982.6544")]
    [InlineData("DKK", "JPY", "16739.2032")]
    [InlineData("JPY", "DKK", "59.7400")]
    [InlineData("USD", "GBP", "777.5224")]
    public async Task ExchangeAsync_WhenExchangingThousandUnits_ReturnsKnownAmount(
        string sourceCode,
        string targetCode,
        string expectedAmount)
    {
        var service = new CurrencyExchangeService(CreateExerciseRates());

        var result = await service.ExchangeAsync(
            CurrencyPair.From($"{sourceCode}/{targetCode}"),
            1000);

        Assert.Equal(
            decimal.Parse(expectedAmount, CultureInfo.InvariantCulture),
            result.Amount);
    }

    public static IEnumerable<object[]> AllSupportedCurrencyPairs()
    {
        const decimal amount = 123.45m;

        foreach (var source in ExerciseRates)
        {
            foreach (var target in ExerciseRates)
            {
                var expected = source.Key == target.Key
                    ? amount
                    : decimal.Round(
                        amount * source.Value / target.Value,
                        4,
                        MidpointRounding.ToEven);

                yield return [source.Key, target.Key, expected];
            }
        }
    }

    private static FakeExchangeRateProvider CreateExerciseRates()
    {
        var rates = new FakeExchangeRateProvider();

        foreach (var rate in ExerciseRates)
            rates.AddRate(rate.Key, "DKK", UnitAmount, rate.Value);

        return rates;
    }

    private static readonly IReadOnlyDictionary<string, decimal> ExerciseRates =
        new Dictionary<string, decimal>
        {
            ["DKK"] = 100m,
            ["EUR"] = 743.94m,
            ["USD"] = 663.11m,
            ["GBP"] = 852.85m,
            ["SEK"] = 76.10m,
            ["NOK"] = 78.40m,
            ["CHF"] = 683.58m,
            ["JPY"] = 5.9740m
        };
}
