using System.Globalization;

namespace Tuzat.Core;

// Two languages do not justify resource files; English is the fallback for every other culture.
public static class Localized
{
    public static string Text(string english, string russian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? russian : english;
}
