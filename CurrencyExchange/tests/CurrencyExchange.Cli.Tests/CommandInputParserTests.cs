namespace CurrencyExchange.Cli.Tests;

public class CommandInputParserTests
{
    [Theory]
    [InlineData("USD/DKK 1", "USD/DKK", 1)]
    [InlineData("  EUR/USD   12.5  ", "EUR/USD", 12.5)]
    [InlineData("EUR/USD 12,5", "EUR/USD", 12.5)]
    [InlineData("EUR/USD\t12.5", "EUR/USD", 12.5)]
    [InlineData("﻿EUR/USD 12.5", "EUR/USD", 12.5)]
    public void TryParse_WhenInputIsValid_ReturnsPairAndAmount(
        string input,
        string expectedPair,
        decimal expectedAmount)
    {
        var parsed = CommandInputParser.TryParse(
            input,
            out var pair,
            out var amount);

        Assert.True(parsed);
        Assert.Equal(expectedPair, pair);
        Assert.Equal(expectedAmount, amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("USD/DKK")]
    [InlineData("USD/DKK one")]
    [InlineData("USD/DKK 1 extra")]
    [InlineData("USD/DKK 1,000")]
    [InlineData("USD/DKK 1,000.50")]
    [InlineData("USD/DKK 1.000,50")]
    public void TryParse_WhenInputIsInvalid_ReturnsFalse(string input)
    {
        var parsed = CommandInputParser.TryParse(input, out _, out _);

        Assert.False(parsed);
    }
}
