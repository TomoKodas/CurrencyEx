using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Models;

public sealed record Money
{
    public Currency Currency { get; }
    public decimal Amount { get; }

    public Money(Currency currency, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (amount < 0)
            throw new InvalidAmountException("Money amount cannot be negative.");

        Currency = currency;
        Amount = amount;
    }
}
