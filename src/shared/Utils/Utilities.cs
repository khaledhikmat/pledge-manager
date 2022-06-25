using System.Globalization;

namespace pledgemanager.shared.Utils;

public class Utilities
{
    public static string FormatAsUSD(object value)
    {
        return ((double)value).ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    }
}