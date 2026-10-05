using System.Globalization;

namespace Tuzat.Core.Tests;

public class LocalizedTests
{
    [Theory]
    [InlineData("ru-RU", "ру")]
    [InlineData("ru", "ру")]
    [InlineData("en-US", "en")]
    [InlineData("uz-Latn-UZ", "en")]
    [InlineData("tg-Cyrl-TJ", "en")]
    public void Picks_text_by_ui_culture(string culture, string expected)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            Assert.Equal(expected, Localized.Text("en", "ру"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }
}
