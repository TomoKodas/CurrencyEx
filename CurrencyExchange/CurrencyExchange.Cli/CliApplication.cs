using System.Globalization;
using CurrencyExchange.Application;
using CurrencyExchange.Application.Abstractions;
using CurrencyExchange.DataAccess;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.Cli;

public static class CliApplication
{
    public const int Success = 0;
    public const int UnexpectedError = 1;
    public const int UserError = 2;

    private const string UsageMessage =
        "Enter a currency pair and amount, for example: USD/DKK 1.5 " +
        "(decimal separator '.' or ','; thousands separators and ambiguous " +
        "amounts such as 1,000 are not supported)";

    private const string PromptMessage = "Enter currency pair and amount (for example USD/DKK 1): ";

    private const string AmountFormat = "0.############################";

    public static async Task<int> RunAsync(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        try
        {
            var commandText = string.Join(' ', args);
            if (string.IsNullOrWhiteSpace(commandText))
                commandText = await PromptAsync(input, error);

            if (!CommandInputParser.TryParse(commandText, out var currencyPairText, out var amount))
            {
                await error.WriteLineAsync(UsageMessage);
                return UserError;
            }

            var currencyPair = CurrencyPair.From(currencyPairText);

            await using var services = new ServiceCollection()
                .AddCurrencyExchangeApplication()
                .AddCurrencyExchangeDataAccess()
                .BuildServiceProvider();
            await using var scope = services.CreateAsyncScope();
            await scope.ServiceProvider.InitializeCurrencyExchangeDataAccessAsync();

            var result = await scope.ServiceProvider
                .GetRequiredService<ICurrencyExchangeService>()
                .ExchangeAsync(currencyPair, amount);

            await output.WriteLineAsync(
                $"{FormatAmount(amount)} {currencyPair.FromCurrency.IsoCode} = " +
                $"{FormatAmount(result.Amount)} {result.Currency.IsoCode}");

            return Success;
        }
        catch (CurrencyExchangeException exception)
        {
            await error.WriteLineAsync(exception.Message);
            return UserError;
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync(exception.ToString());
            return UnexpectedError;
        }
    }

    // The prompt goes to stderr so stdout only ever contains the result.
    private static async Task<string> PromptAsync(TextReader input, TextWriter error)
    {
        await error.WriteAsync(PromptMessage);
        await error.FlushAsync();

        return await input.ReadLineAsync() ?? string.Empty;
    }

    private static string FormatAmount(decimal amount) =>
        amount.ToString(AmountFormat, CultureInfo.InvariantCulture);
}
