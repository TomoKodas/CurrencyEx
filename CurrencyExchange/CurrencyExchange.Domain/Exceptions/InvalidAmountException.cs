namespace CurrencyExchange.Domain.Exceptions;

public sealed class InvalidAmountException : CurrencyExchangeException
{
    public const string NegativeAmountMessage = "Amount cannot be negative.";

    public InvalidAmountException(string message)
        : base(message)
    {
    }

    public InvalidAmountException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
