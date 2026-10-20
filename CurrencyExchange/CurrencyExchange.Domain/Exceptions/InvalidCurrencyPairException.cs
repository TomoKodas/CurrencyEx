namespace CurrencyExchange.Domain.Exceptions;

public class InvalidCurrencyPairException : CurrencyExchangeException
{
    public InvalidCurrencyPairException(string message)
        : base(message)
    {
    }
}
