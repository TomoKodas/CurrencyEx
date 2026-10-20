namespace CurrencyExchange.Domain.Exceptions;

public abstract class CurrencyExchangeException : Exception
{
    protected CurrencyExchangeException(string message)
        : base(message)
    {
    }

    protected CurrencyExchangeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
