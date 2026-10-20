using CurrencyExchange.DataAccess.Entities;

namespace CurrencyExchange.DataAccess.Tests;

public class CurrencyExchangeDbContextTests
{
    [Fact]
    public void Model_ExchangeRateKey_IsCurrency()
    {
        using var dbContext = TestData.CreateDbContext();

        var entity = dbContext.Model.FindEntityType(typeof(ExchangeRateEntity));
        var keyProperties = entity!.FindPrimaryKey()!.Properties.Select(property => property.Name);

        Assert.Equal([nameof(ExchangeRateEntity.CurrencyIsoCode)], keyProperties);
    }
}
