using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Domain.Tests;

public class CurrencyTests
{
    [Theory]
    [InlineData("eur", "EUR")]
    [InlineData(" USD ", "USD")]
    public void From_WhenCodeIsValid_NormalizesCode(
        string input,
        string expectedCode)
    {
        var currency = Currency.From(input);

        Assert.Equal(expectedCode, currency.IsoCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void From_WhenCodeIsInvalid_ThrowsInvalidCurrencyException(
        string input)
    {
        Assert.Throws<InvalidCurrencyException>(() => Currency.From(input));
    }

    [Fact]
    public void Equality_WhenCodesHaveDifferentCasing_ReturnsTrue()
    {
        var first = Currency.From("eur");
        var second = Currency.From("EUR");

        Assert.Equal(first, second);
    }
}
