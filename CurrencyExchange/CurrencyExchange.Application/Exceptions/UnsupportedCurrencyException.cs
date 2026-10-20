using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Application.Exceptions;

public class UnsupportedCurrencyException : CurrencyExchangeException
{
    public UnsupportedCurrencyException(string currencyIsoCode)
        : base($"Unsupported currency: {currencyIsoCode}.")
    {
    }
}
