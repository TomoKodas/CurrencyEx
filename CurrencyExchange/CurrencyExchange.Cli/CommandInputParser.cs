using System.Globalization;

namespace CurrencyExchange.Cli;

public static class CommandInputParser
{
    private const char ByteOrderMark = '﻿';
    private const char DecimalComma = ',';
    private const char DecimalPoint = '.';
    private const int ThousandsGroupSize = 3;

    public static bool TryParse(
        string input,
        out string currencyPair,
        out decimal amount)
    {
        currencyPair = string.Empty;
        amount = 0;

        var parts = input.TrimStart(ByteOrderMark).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
            return false;

        var amountText = parts[1];
        if (amountText.Contains(DecimalComma))
        {
            var digitsAfterComma = amountText.Length - amountText.LastIndexOf(DecimalComma) - 1;
            if (amountText.Contains(DecimalPoint) || digitsAfterComma == ThousandsGroupSize)
                return false;

            amountText = amountText.Replace(DecimalComma, DecimalPoint);
        }

        var numberStyles = NumberStyles.AllowLeadingSign |
                           NumberStyles.AllowDecimalPoint;

        if (!decimal.TryParse(
                amountText,
                numberStyles,
                CultureInfo.InvariantCulture,
                out amount))
        {
            return false;
        }

        currencyPair = parts[0];
        return true;
    }
}
