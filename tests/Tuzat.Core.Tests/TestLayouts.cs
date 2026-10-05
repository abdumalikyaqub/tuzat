namespace Tuzat.Core.Tests;

// Same key positions as US English and Russian ЙЦУКЕН; unshifted pairs come first, as in LayoutMapBuilder.
internal static class TestLayouts
{
    private const string EnLower = "`qwertyuiop[]asdfghjkl;'zxcvbnm,./";
    private const string RuLower = "ёйцукенгшщзхъфывапролджэячсмитьбю.";
    private const string EnUpper = "~QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>?";
    private const string RuUpper = "ЁЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,";
    private const string EnShiftedDigits = "!@#$%^&*()";
    private const string RuShiftedDigits = "!\"№;%:?*()";

    public static LayoutMap EnRu { get; } = new(
        EnLower.Zip(RuLower).Concat(EnUpper.Zip(RuUpper)).Concat(EnShiftedDigits.Zip(RuShiftedDigits)));
}
