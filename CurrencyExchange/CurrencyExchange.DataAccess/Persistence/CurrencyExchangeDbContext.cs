using CurrencyExchange.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.DataAccess.Persistence;

public class CurrencyExchangeDbContext : DbContext
{
    public CurrencyExchangeDbContext(DbContextOptions<CurrencyExchangeDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExchangeRateEntity> ExchangeRates => Set<ExchangeRateEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRateEntity>(entity =>
        {
            entity.HasKey(x => x.CurrencyIsoCode);

            entity.Property(x => x.CurrencyIsoCode)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(x => x.BaseCurrencyIsoCode)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(x => x.UnitAmount)
                .IsRequired()
                .HasPrecision(18, 6);

            entity.Property(x => x.BaseCurrencyAmount)
                .IsRequired()
                .HasPrecision(18, 6);

            entity.HasData(ExchangeRateSeedData.Load());
        });
    }
}
