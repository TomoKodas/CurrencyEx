using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.DataAccess.Persistence;
using CurrencyExchange.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.DataAccess;

public class EfExchangeRateProvider : IExchangeRateProvider
{
    private readonly CurrencyExchangeDbContext _dbContext;

    public EfExchangeRateProvider(CurrencyExchangeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ExchangeRate> GetRequiredRateAsync(
        Currency currency,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currency);

        var entity = await _dbContext.ExchangeRates
            .AsNoTracking()
            .SingleOrDefaultAsync(rate => rate.CurrencyIsoCode == currency.IsoCode, cancellationToken)
            ?? throw new UnsupportedCurrencyException(currency.IsoCode);

        return new ExchangeRate(
            Currency.From(entity.CurrencyIsoCode),
            Currency.From(entity.BaseCurrencyIsoCode),
            entity.UnitAmount,
            entity.BaseCurrencyAmount);
    }
}
