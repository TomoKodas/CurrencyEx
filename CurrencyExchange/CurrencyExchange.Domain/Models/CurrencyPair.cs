using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Models;

public sealed record CurrencyPair
{
    public Currency FromCurrency { get; }
    public Currency ToCurrency { get; }

    private CurrencyPair(Currency fromCurrency, Currency toCurrency)
    {
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
    }

    public static CurrencyPair From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidCurrencyPairException("Currency pair is required.");

        var parts = value.Split('/', StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
        {
            throw new InvalidCurrencyPairException(
                "Currency pair must be in format EUR/DKK.");
        }

        return new CurrencyPair(
            Currency.From(parts[0]),
            Currency.From(parts[1]));
    }
}
