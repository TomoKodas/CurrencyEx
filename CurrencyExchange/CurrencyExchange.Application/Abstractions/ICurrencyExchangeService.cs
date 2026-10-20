using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Abstractions;

public interface ICurrencyExchangeService
{
    Task<Money> ExchangeAsync(
        CurrencyPair currencyPair,
        decimal amount,
        CancellationToken cancellationToken = default);
}
