namespace CurrencyExchange.Domain.Exceptions;

public sealed class InvalidCurrencyException : CurrencyExchangeException
{
    public InvalidCurrencyException(string message)
        : base(message)
    {
    }
}
