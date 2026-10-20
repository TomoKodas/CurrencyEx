namespace CurrencyExchange.Domain.Exceptions;

public class InvalidExchangeRateException : CurrencyExchangeException
{
    public InvalidExchangeRateException(string message)
        : base(message)
    {
    }
}
