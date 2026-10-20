using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Tests;

internal sealed class FakeExchangeRateProvider : IExchangeRateProvider
{
    private readonly Dictionary<string, ExchangeRate> _rates =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> AvailableCurrencyCodes => _rates.Keys;

    public void AddRate(
        string currencyCode,
        string baseCurrencyCode,
        decimal unitAmount,
        decimal baseCurrencyAmount)
    {
        var rate = new ExchangeRate(
            Currency.From(currencyCode),
            Currency.From(baseCurrencyCode),
            unitAmount,
            baseCurrencyAmount);

        _rates[rate.Currency.IsoCode] = rate;
    }

    public Task<ExchangeRate> GetRequiredRateAsync(
        Currency currency,
        CancellationToken cancellationToken = default)
    {
        if (!_rates.TryGetValue(currency.IsoCode, out var rate))
            throw new UnsupportedCurrencyException(currency.IsoCode);

        return Task.FromResult(rate);
    }
}
