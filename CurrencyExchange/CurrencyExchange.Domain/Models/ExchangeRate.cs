using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Models;

public sealed record ExchangeRate
{
    private const string AmountTooLargeMessage = "Amount is too large to exchange.";

    public Currency Currency { get; }
    public Currency BaseCurrency { get; }
    public decimal UnitAmount { get; }
    public decimal BaseCurrencyAmount { get; }

    private decimal BaseCurrencyAmountPerUnit => BaseCurrencyAmount / UnitAmount;

    public ExchangeRate(
        Currency currency,
        Currency baseCurrency,
        decimal unitAmount,
        decimal baseCurrencyAmount)
    {
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(baseCurrency);
        ValidateUnitAmount(unitAmount);
        ValidateBaseCurrencyAmount(baseCurrencyAmount);

        if (currency == baseCurrency && unitAmount != baseCurrencyAmount)
        {
            throw new InvalidExchangeRateException(
                "An identity exchange rate must have equal unit and base-currency amounts.");
        }

        Currency = currency;
        BaseCurrency = baseCurrency;
        UnitAmount = unitAmount;
        BaseCurrencyAmount = baseCurrencyAmount;
    }

    public static void ValidateUnitAmount(decimal unitAmount)
    {
        if (unitAmount <= 0)
        {
            throw new InvalidExchangeRateException(
                "Unit amount must be greater than zero.");
        }
    }

    public static void ValidateBaseCurrencyAmount(decimal baseCurrencyAmount)
    {
        if (baseCurrencyAmount <= 0)
        {
            throw new InvalidExchangeRateException(
                "Base currency amount must be greater than zero.");
        }
    }

    public decimal ConvertTo(ExchangeRate targetRate, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(targetRate);

        if (amount < 0)
            throw new InvalidAmountException(InvalidAmountException.NegativeAmountMessage);

        if (BaseCurrency != targetRate.BaseCurrency)
        {
            throw new IncompatibleBaseCurrencyException(
                BaseCurrency.IsoCode,
                targetRate.BaseCurrency.IsoCode);
        }

        // dividing first is only used when that intermediate is too large for a decimal.
        try
        {
            return amount * BaseCurrencyAmountPerUnit / targetRate.BaseCurrencyAmountPerUnit;
        }
        catch (OverflowException)
        {
            return ConvertDividingFirst(targetRate, amount);
        }
    }

    private decimal ConvertDividingFirst(ExchangeRate targetRate, decimal amount)
    {
        try
        {
            return amount / targetRate.BaseCurrencyAmountPerUnit * BaseCurrencyAmountPerUnit;
        }
        catch (OverflowException exception)
        {
            throw new InvalidAmountException(AmountTooLargeMessage, exception);
        }
    }
}
