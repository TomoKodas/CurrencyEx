using CurrencyExchange.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.DataAccess.Tests;

internal static class TestData
{
    public static CurrencyExchangeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CurrencyExchangeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new CurrencyExchangeDbContext(options);
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}
