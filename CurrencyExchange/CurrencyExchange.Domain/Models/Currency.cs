using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Models;

public sealed record Currency
{
    public string IsoCode { get; }

    private Currency(string isoCode)
    {
        IsoCode = isoCode;
    }

    public static Currency From(string isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
            throw new InvalidCurrencyException("Currency code is required.");

        var normalizedIsoCode = isoCode.Trim().ToUpperInvariant();

        if (normalizedIsoCode.Length != 3 ||
            normalizedIsoCode.Any(character => character is < 'A' or > 'Z'))
        {
            throw new InvalidCurrencyException(
                "Currency code must contain exactly three ASCII letters.");
        }

        return new Currency(normalizedIsoCode);
    }
}
