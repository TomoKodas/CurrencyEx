namespace CurrencyExchange.DataAccess.Entities;

public class ExchangeRateEntity
{
    public string CurrencyIsoCode { get; set; } = string.Empty;

    public string BaseCurrencyIsoCode { get; set; } = string.Empty;

    public decimal UnitAmount { get; set; }

    public decimal BaseCurrencyAmount { get; set; }
}