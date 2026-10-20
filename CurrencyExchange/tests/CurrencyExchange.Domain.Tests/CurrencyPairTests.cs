using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Domain.Tests;

public class CurrencyPairTests
{
    [Fact]
    public void From_WhenPairIsValid_ReturnsNormalizedCurrencies()
    {
        var pair = CurrencyPair.From(" eur / usd ");

        Assert.Equal("EUR", pair.FromCurrency.IsoCode);
        Assert.Equal("USD", pair.ToCurrency.IsoCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EUR")]
    [InlineData("EUR/USD/DKK")]
    public void From_WhenPairFormatIsInvalid_ThrowsInvalidCurrencyPairException(
        string input)
    {
        Assert.Throws<InvalidCurrencyPairException>(() =>
            CurrencyPair.From(input));
    }

    [Theory]
    [InlineData("EU/USD")]
    [InlineData("EUR/U1D")]
    public void From_WhenCurrencyCodeIsInvalid_ThrowsInvalidCurrencyException(
        string input)
    {
        Assert.Throws<InvalidCurrencyException>(() => CurrencyPair.From(input));
    }
}
