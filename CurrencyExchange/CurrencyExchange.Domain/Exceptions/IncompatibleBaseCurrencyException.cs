namespace CurrencyExchange.Domain.Exceptions;

public sealed class IncompatibleBaseCurrencyException : CurrencyExchangeException
{
    public IncompatibleBaseCurrencyException(
        string sourceBaseCurrencyIsoCode,
        string targetBaseCurrencyIsoCode)
        : base(
            $"Currencies use incompatible base currencies: " +
            $"{sourceBaseCurrencyIsoCode} and {targetBaseCurrencyIsoCode}.")
    {
    }
}
