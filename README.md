# Currency Exchange

A small command-line tool that converts an amount from one currency to another, using fixed exchange rates against Danish kroner.

Supported currencies: DKK, EUR, USD, GBP, SEK, NOK, CHF, JPY.

## Running it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). Nothing else to set up.

```sh
dotnet build CurrencyExchange/CurrencyExchange.slnx
dotnet test CurrencyExchange/CurrencyExchange.slnx

dotnet run --project CurrencyExchange/CurrencyExchange.Cli -- EUR/USD 100
# 100 EUR = 112.1895 USD
```

If you leave out the arguments, it asks you for them.

Use `.` for decimals. A `,` works too (`1,5`), but `1,000` is rejected because it could mean either one thousand or one.

Exit codes:

| Code | Meaning |
|---|---|
| `0` | Success |
| `1` | Unexpected error |
| `2` | Invalid input, e.g. unknown currency or bad amount |

## How it works

Every rate says how many kroner it costs to buy 100 units of a currency. To convert between two currencies, the amount goes through kroner: for example, EUR to USD means EUR to DKK, then DKK to USD.

The rates live in `exchange-rates.json`, which is loaded into an in-memory database at startup. You can change them without recompiling.

Results are rounded to 4 decimal places. Converting a currency to itself returns the amount unchanged.

Invalid input, such as an unknown currency, a negative amount or a badly formatted number, gives a clear error message instead of a crash.

## Project layout

- **Domain**: currencies, rates and the conversion math
- **Application**: the exchange use case
- **DataAccess**: the rates file and the in-memory database
- **Cli**: reads the input and prints the result
- **tests**: unit tests for each project
