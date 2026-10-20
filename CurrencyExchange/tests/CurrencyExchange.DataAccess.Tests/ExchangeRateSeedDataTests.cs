using System.Text.Json;
using CurrencyExchange.DataAccess.Entities;
using CurrencyExchange.DataAccess.Persistence;

namespace CurrencyExchange.DataAccess.Tests;

public class ExchangeRateSeedDataTests
{
    [Fact]
    public void Load_WhenFileIsValid_AddsBaseCurrencyIdentityRateAndNormalizesCodes()
    {
        var rates = LoadJson("""
            { "baseCurrency": "dkk", "unitAmount": 100,
              "rates": [ { "currency": " eur ", "baseCurrencyAmount": 743.94 } ] }
            """);

        Assert.Collection(
            rates,
            rate =>
            {
                Assert.Equal("DKK", rate.CurrencyIsoCode);
                Assert.Equal(100m, rate.BaseCurrencyAmount);
            },
            rate =>
            {
                Assert.Equal("EUR", rate.CurrencyIsoCode);
                Assert.Equal("DKK", rate.BaseCurrencyIsoCode);
                Assert.Equal(743.94m, rate.BaseCurrencyAmount);
            });
    }

    [Fact]
    public void Load_WhenShippedFileIsUsed_ContainsBaseAndAllExerciseCurrencies()
    {
        var codes = ExchangeRateSeedData.Load().Select(rate => rate.CurrencyIsoCode);

        Assert.Equal(["DKK", "EUR", "USD", "GBP", "SEK", "NOK", "CHF", "JPY"], codes);
    }

    [Theory]
    [InlineData("""{ "currency": "EUR", "baseCurrencyAmount": 743.94 }, { "currency": "eur", "baseCurrencyAmount": 750 }""", "EUR is listed more than once")]
    [InlineData("""{ "currency": "DKK", "baseCurrencyAmount": 100 }""", "DKK is listed more than once")]
    [InlineData("""{ "currency": "EURO", "baseCurrencyAmount": 743.94 }""", "rates[0]: Currency code must contain exactly three ASCII letters.")]
    [InlineData("""{ "baseCurrencyAmount": 743.94 }""", "rates[0]: Currency code is required.")]
    [InlineData("""{ "currency": "EUR", "baseCurrencyAmount": 0 }""", "rates[0]: Base currency amount must be greater than zero.")]
    [InlineData("", "'rates' must contain at least one rate.")]
    public void Load_WhenRateIsInvalid_ThrowsInvalidDataExceptionDescribingProblem(
        string ratesJson,
        string expectedMessage)
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson(
            $$"""{ "baseCurrency": "DKK", "unitAmount": 100, "rates": [ {{ratesJson}} ] }"""));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void Load_WhenBaseCurrencyIsInvalid_ReportsItOnceAndStillChecksRates()
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson("""
            { "baseCurrency": "DKKK", "unitAmount": 100,
              "rates": [ { "currency": "EUR", "baseCurrencyAmount": 0 },
                         { "currency": "EUR", "baseCurrencyAmount": 743.94 } ] }
            """));

        Assert.Contains("baseCurrency: Currency code must contain exactly three ASCII letters.", exception.Message);
        Assert.Contains("rates[0]: Base currency amount must be greater than zero.", exception.Message);
        Assert.Contains("EUR is listed more than once (rates[0], rates[1])", exception.Message);
        Assert.DoesNotContain("rates[1]:", exception.Message);
    }

    [Fact]
    public void Load_WhenTopLevelValuesAreMissing_ReportsThemWithoutBlamingRates()
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson("""
            { "rates": [ { "currency": "EUR", "baseCurrencyAmount": 743.94 } ] }
            """));

        Assert.Contains("baseCurrency: Currency code is required.", exception.Message);
        Assert.Contains("unitAmount: Unit amount must be greater than zero.", exception.Message);
        Assert.DoesNotContain("rates[0]", exception.Message);
    }

    [Fact]
    public void Load_WhenRateHasSeveralProblems_ReportsEachOfThem()
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson("""
            { "baseCurrency": "DKK", "unitAmount": 100,
              "rates": [ { "currency": "EURO", "baseCurrencyAmount": 0 }, null ] }
            """));

        Assert.Contains("rates[0]: Currency code must contain exactly three ASCII letters.", exception.Message);
        Assert.Contains("rates[0]: Base currency amount must be greater than zero.", exception.Message);
        Assert.Contains("rates[1]: Rate is required.", exception.Message);
    }

    [Fact]
    public void Load_WhenFileHasCommentsAndTrailingCommas_LoadsRates()
    {
        var rates = LoadJson("""
            {
              // Rates as of 2026-10-20
              "baseCurrency": "DKK",
              "unitAmount": 100, /* per 100 units */
              "rates": [
                { "currency": "EUR", "baseCurrencyAmount": 743.94, },
              ],
            }
            """);

        Assert.Equal(["DKK", "EUR"], rates.Select(rate => rate.CurrencyIsoCode));
    }

    [Theory]
    [InlineData("""{ "baseCurrency": "DKK", "unitAmount": 100, "unitAmount": 1, "rates": [ { "currency": "EUR", "baseCurrencyAmount": 743.94 } ] }""")]
    [InlineData("""{ "baseCurrency": "DKK", "unitAmount": 100, "UnitAmount": 1, "rates": [ { "currency": "EUR", "baseCurrencyAmount": 743.94 } ] }""")]
    [InlineData("""{ "baseCurrency": "DKK", "unitAmount": 100, "rates": [ { "currency": "EUR", "baseCurrencyAmount": 743.94, "baseCurrencyAmount": 1 } ] }""")]
    [InlineData("""{ "baseCurrency": "DKK", "unitAmount": 100, "rates": [ { "currency": "EUR", "baseCurrencyAmount": 743.94 } ], "rates": [] }""")]
    public void Load_WhenPropertyIsRepeated_ThrowsInvalidDataException(string json)
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson(json));

        Assert.Contains("is not valid JSON", exception.Message);
        Assert.IsType<JsonException>(exception.InnerException);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \r\n ")]
    [InlineData("﻿")]
    [InlineData("null")]
    public void Load_WhenFileIsEmpty_ThrowsInvalidDataExceptionNamingFile(string json)
    {
        var exception = Assert.Throws<InvalidDataException>(() => LoadJson(json));

        Assert.EndsWith(" is empty.", exception.Message);
    }

    private static IReadOnlyList<ExchangeRateEntity> LoadJson(string json)
    {
        var filePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(filePath, json);
            return ExchangeRateSeedData.Load(filePath);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
