using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Domain.Tests;

public class ExchangeRateTests
{
    [Fact]
    public void ConvertTo_WhenTargetIsBaseCurrency_ReturnsBaseAmount()
    {
        var amount = CreateEuroRate().ConvertTo(CreateKronerRate(), 100);

        Assert.Equal(743.94m, amount);
    }

    [Fact]
    public void ConvertTo_WhenSourceIsBaseCurrency_ReturnsCurrencyAmount()
    {
        var amount = CreateKronerRate().ConvertTo(CreateEuroRate(), 743.94m);

        Assert.Equal(100m, amount);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(100, 0)]
    [InlineData(-1, 1)]
    public void Constructor_WhenRateIsNotPositive_ThrowsInvalidExchangeRateException(
        decimal unitAmount,
        decimal baseCurrencyAmount)
    {
        Assert.Throws<InvalidExchangeRateException>(() => new ExchangeRate(
            Currency.From("EUR"),
            Currency.From("DKK"),
            unitAmount,
            baseCurrencyAmount));
    }

    [Fact]
    public void Constructor_WhenIdentityRateAmountsDiffer_ThrowsInvalidExchangeRateException()
    {
        var kroner = Currency.From("DKK");

        Assert.Throws<InvalidExchangeRateException>(() =>
            new ExchangeRate(kroner, kroner, 100, 99));
    }

    [Fact]
    public void ConvertTo_WhenAmountIsNegative_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() =>
            CreateEuroRate().ConvertTo(CreateKronerRate(), -1));
    }

    [Fact]
    public void ConvertTo_WhenBaseCurrenciesMatch_ConvertsThroughBaseCurrency()
    {
        var dollarRate = new ExchangeRate(Currency.From("USD"), Currency.From("DKK"), 100, 663.11m);

        var amount = CreateEuroRate().ConvertTo(dollarRate, 663.11m);

        Assert.Equal(743.94m, amount);
    }

    [Fact]
    public void ConvertTo_WhenBaseCurrenciesDiffer_ThrowsIncompatibleBaseCurrencyException()
    {
        var dollarRate = new ExchangeRate(
            Currency.From("USD"),
            Currency.From("SEK"),
            100,
            984.11m);

        Assert.Throws<IncompatibleBaseCurrencyException>(() =>
            CreateEuroRate().ConvertTo(dollarRate, 1));
    }

    [Fact]
    public void ConvertTo_WhenResultFitsButAmountTimesUnitAmountWouldNot_ReturnsResult()
    {
        var amount = CreateKronerRate().ConvertTo(CreateEuroRate(), 7_439_400_000_000_000_000_000_000_000m);

        Assert.Equal(1_000_000_000_000_000_000_000_000_000m, amount);
    }

    [Fact]
    public void ConvertTo_WhenOnlyMultiplyingFirstOverflows_DividesFirst()
    {
        var poundRate = new ExchangeRate(Currency.From("GBP"), Currency.From("DKK"), 100, 852.85m);

        var amount = poundRate.ConvertTo(CreateEuroRate(), 9_300_000_000_000_000_000_000_000_000m);

        Assert.Equal(10_661_484_797_161_061_375_917_412_694m, amount);
    }

    [Fact]
    public void ConvertTo_WhenResultOverflows_ThrowsInvalidAmountException()
    {
        var exception = Assert.Throws<InvalidAmountException>(() =>
            CreateEuroRate().ConvertTo(CreateKronerRate(), decimal.MaxValue));

        Assert.IsType<OverflowException>(exception.InnerException);
    }

    private static ExchangeRate CreateEuroRate() =>
        new(Currency.From("EUR"), Currency.From("DKK"), 100, 743.94m);

    private static ExchangeRate CreateKronerRate() =>
        new(Currency.From("DKK"), Currency.From("DKK"), 100, 100);
}
