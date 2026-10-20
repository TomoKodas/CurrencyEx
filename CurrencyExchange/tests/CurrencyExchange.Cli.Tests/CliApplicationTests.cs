using System.Globalization;

namespace CurrencyExchange.Cli.Tests;

public class CliApplicationTests
{
    [Theory]
    [InlineData("EUR/DKK", "1", "1 EUR = 7.4394 DKK")]
    [InlineData("EUR/DKK", "1000", "1000 EUR = 7439.4 DKK")]
    [InlineData("EUR/USD", "100", "100 EUR = 112.1895 USD")]
    [InlineData("DKK/EUR", "743.94", "743.94 DKK = 100 EUR")]
    [InlineData("EUR/EUR", "12.34567", "12.34567 EUR = 12.34567 EUR")]
    [InlineData("EUR/DKK", "-0", "0 EUR = 0 DKK")]
    [InlineData("JPY/DKK", "0.00004", "0.00004 JPY = 0 DKK")]
    [InlineData(
        "EUR/DKK",
        "1000000000000000000000000000",
        "1000000000000000000000000000 EUR = 7439400000000000000000000000 DKK")]
    [InlineData(
        "GBP/EUR",
        "9300000000000000000000000000",
        "9300000000000000000000000000 GBP = 10661484797161061375917412694 EUR")]
    [InlineData("USD/NOK", "7", "7 USD = 59.2062 NOK")]
    public async Task RunAsync_WhenPairIsSupported_WritesExchangedAmount(
        string pair,
        string amount,
        string expectedOutput)
    {
        var result = await RunAsync(pair, amount);

        Assert.Equal(CliApplication.Success, result.ExitCode);
        Assert.Equal(expectedOutput, result.Output.Trim());
        Assert.Empty(result.Error);
    }

    [Theory]
    [InlineData("XYZ/DKK")]
    [InlineData("DKK/XYZ")]
    [InlineData("XYZ/XYZ")]
    public async Task RunAsync_WhenCurrencyIsUnknown_WritesErrorMessage(string pair)
    {
        var result = await RunAsync(pair, "1");

        Assert.Equal(CliApplication.UserError, result.ExitCode);
        Assert.Equal("Unsupported currency: XYZ.", result.Error.Trim());
        Assert.Empty(result.Output);
    }

    [Theory]
    [InlineData("EURDKK 1")]
    [InlineData("EU/DKK 1")]
    [InlineData("EUR/DKK -1")]
    [InlineData("EUR/DKK abc")]
    [InlineData("EUR/USD 1,000")]
    [InlineData("EUR/DKK 20000000000000000000000000000")]
    public async Task RunAsync_WhenInputIsInvalid_WritesErrorMessage(string input)
    {
        var result = await RunAsync(input.Split(' '));

        Assert.Equal(CliApplication.UserError, result.ExitCode);
        Assert.NotEmpty(result.Error.Trim());
        Assert.Empty(result.Output);
    }

    [Fact]
    public async Task RunAsync_WhenCurrentCultureUsesDecimalComma_WritesInvariantOutput()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");

        try
        {
            var result = await RunAsync("EUR/DKK", "1,5");

            Assert.Equal(CliApplication.Success, result.ExitCode);
            Assert.Equal("1.5 EUR = 11.1591 DKK", result.Output.Trim());
            Assert.Empty(result.Error);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("", "")]
    public async Task RunAsync_WhenNoArgumentsAreGiven_PromptsOnErrorStreamAndReadsInput(params string[] args)
    {
        var result = await RunWithInputAsync(args, "EUR/DKK 1");

        Assert.Equal(CliApplication.Success, result.ExitCode);
        Assert.Equal("1 EUR = 7.4394 DKK", result.Output.Trim());
        Assert.StartsWith("Enter currency pair and amount", result.Error);
    }

    [Fact]
    public async Task RunAsync_WhenPipedInputStartsWithByteOrderMark_IgnoresIt()
    {
        var result = await RunWithInputAsync([], "﻿EUR/DKK 1");

        Assert.Equal(CliApplication.Success, result.ExitCode);
        Assert.Equal("1 EUR = 7.4394 DKK", result.Output.Trim());
    }

    private static Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] args) =>
        RunWithInputAsync(args, string.Empty);

    private static async Task<(int ExitCode, string Output, string Error)> RunWithInputAsync(
        string[] args,
        string input)
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(args, new StringReader(input), output, error);

        return (exitCode, output.ToString(), error.ToString());
    }
}
