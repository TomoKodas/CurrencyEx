using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.Application.Abstractions;

public interface IExchangeRateProvider
{
    Task<ExchangeRate> GetRequiredRateAsync(
        Currency currency,
        CancellationToken cancellationToken = default);
}
