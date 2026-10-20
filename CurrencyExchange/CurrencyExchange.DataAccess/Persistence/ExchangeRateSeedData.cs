using System.Text.Json;
using System.Text.Json.Serialization;
using CurrencyExchange.DataAccess.Entities;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;

namespace CurrencyExchange.DataAccess.Persistence;

public static class ExchangeRateSeedData
{
    private const string DirectoryName = "Persistence";
    private const string FileName = "exchange-rates.json";

    private const string BaseCurrencyProperty = "baseCurrency";
    private const string UnitAmountProperty = "unitAmount";
    private const string RatesProperty = "rates";
    private const string CurrencyProperty = "currency";
    private const string BaseCurrencyAmountProperty = "baseCurrencyAmount";

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerOptions.Web)
        {
            AllowDuplicateProperties = false,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

    public static IReadOnlyList<ExchangeRateEntity> Load() =>
        Load(Path.Combine(AppContext.BaseDirectory, DirectoryName, FileName));

    public static IReadOnlyList<ExchangeRateEntity> Load(string filePath)
    {
        var file = Parse(filePath);
        var entries = file.Rates ?? [];

        var failures = new List<string>();
        var listed = new List<(string Path, Currency Currency, decimal BaseCurrencyAmount)>();

        var baseCurrency = ParseCurrency(BaseCurrencyProperty, file.BaseCurrency, failures);
        Validate(UnitAmountProperty, () => ExchangeRate.ValidateUnitAmount(file.UnitAmount), failures);

        if (baseCurrency is not null)
            listed.Add((BaseCurrencyProperty, baseCurrency, file.UnitAmount));

        if (entries.Count == 0)
            failures.Add($"'{RatesProperty}' must contain at least one rate.");

        for (var index = 0; index < entries.Count; index++)
        {
            var path = $"{RatesProperty}[{index}]";
            var entry = entries[index];
            if (entry is null)
            {
                failures.Add($"{path}: Rate is required.");
                continue;
            }

            var currency = ParseCurrency(path, entry.Currency, failures);
            Validate(path, () => ExchangeRate.ValidateBaseCurrencyAmount(entry.BaseCurrencyAmount), failures);

            if (currency is not null)
                listed.Add((path, currency, entry.BaseCurrencyAmount));
        }

        failures.AddRange(listed
            .GroupBy(rate => rate.Currency)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.IsoCode} is listed more than once " +
                             $"({string.Join(", ", group.Select(rate => rate.Path))}); " +
                             "the base currency is added automatically."));

        if (failures.Count > 0)
        {
            throw new InvalidDataException(
                $"{filePath} is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        }

        return listed
            .Select(rate => new ExchangeRateEntity
            {
                CurrencyIsoCode = rate.Currency.IsoCode,
                BaseCurrencyIsoCode = baseCurrency!.IsoCode,
                UnitAmount = file.UnitAmount,
                BaseCurrencyAmount = rate.BaseCurrencyAmount
            })
            .ToList();
    }

    private static SeedFile Parse(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var emptyFileException = new InvalidDataException($"{filePath} is empty.");
        if (string.IsNullOrWhiteSpace(json))
            throw emptyFileException;

        try
        {
            return JsonSerializer.Deserialize<SeedFile>(json, SerializerOptions) ?? throw emptyFileException;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"{filePath} is not valid JSON: {exception.Message}", exception);
        }
    }

    private static Currency? ParseCurrency(string path, string? isoCode, List<string> failures)
    {
        Currency? currency = null;
        Validate(path, () => currency = Currency.From(isoCode ?? string.Empty), failures);
        return currency;
    }

    private static void Validate(string path, Action validate, List<string> failures)
    {
        try
        {
            validate();
        }
        catch (CurrencyExchangeException exception)
        {
            failures.Add($"{path}: {exception.Message}");
        }
    }

    private sealed record SeedFile(
        [property: JsonPropertyName(BaseCurrencyProperty)] string? BaseCurrency,
        [property: JsonPropertyName(UnitAmountProperty)] decimal UnitAmount,
        [property: JsonPropertyName(RatesProperty)] List<SeedRate?>? Rates);

    private sealed record SeedRate(
        [property: JsonPropertyName(CurrencyProperty)] string? Currency,
        [property: JsonPropertyName(BaseCurrencyAmountProperty)] decimal BaseCurrencyAmount);
}
