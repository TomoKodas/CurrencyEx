using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Constructor_WhenAmountIsZero_CreatesMoney()
    {
        var money = new Money(Currency.From("EUR"), 0);

        Assert.Equal(0, money.Amount);
    }

    [Fact]
    public void Constructor_WhenAmountIsNegative_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() =>
            new Money(Currency.From("EUR"), -0.01m));
    }
}
