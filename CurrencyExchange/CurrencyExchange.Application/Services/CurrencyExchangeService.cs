using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    private const int ResultDecimalPlaces = 4;

    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyExchangeService(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider
            ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
    }

    public async Task<Money> ExchangeAsync(
        CurrencyPair currencyPair,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currencyPair);

        if (amount < 0)
            throw new InvalidAmountException(InvalidAmountException.NegativeAmountMessage);

        var sourceRate = await _exchangeRateProvider.GetRequiredRateAsync(
            currencyPair.FromCurrency,
            cancellationToken);

        if (currencyPair.FromCurrency == currencyPair.ToCurrency)
            return new Money(currencyPair.ToCurrency, amount);

        var targetRate = await _exchangeRateProvider.GetRequiredRateAsync(
            currencyPair.ToCurrency,
            cancellationToken);

        var exchangedAmount = sourceRate.ConvertTo(targetRate, amount);

        return CreateRoundedMoney(currencyPair.ToCurrency, exchangedAmount);
    }

    private static Money CreateRoundedMoney(Currency currency, decimal amount)
    {
        var roundedAmount = decimal.Round(
            amount,
            ResultDecimalPlaces,
            MidpointRounding.ToEven);

        return new Money(currency, roundedAmount);
    }
}
