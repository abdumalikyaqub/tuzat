namespace Tuzat.Core.Tests;

public class TextConverterTests
{
    private static readonly LayoutMap Map = TestLayouts.EnRu;

    [Theory]
    [InlineData("ghbdtn", LayoutSide.A, "привет")]
    [InlineData("руддщ", LayoutSide.B, "hello")]
    [InlineData("Ghbdtn", LayoutSide.A, "Привет")]
    [InlineData("z ,ele ljvf", LayoutSide.A, "я буду дома")]
    [InlineData("ю.", LayoutSide.B, "./")]
    public void Convert_maps_characters(string text, LayoutSide source, string expected) =>
        Assert.Equal(expected, TextConverter.Convert(text, Map, source));

    [Fact]
    public void Convert_keeps_characters_of_the_other_side() =>
        Assert.Equal("привет мир", TextConverter.Convert("ghbdtn мир", Map, LayoutSide.A));

    [Fact]
    public void Convert_keeps_line_breaks_digits_and_emoji() =>
        Assert.Equal("привет\r\n123 😀", TextConverter.Convert("ghbdtn\r\n123 😀", Map, LayoutSide.A));

    [Fact]
    public void Convert_empty_text() => Assert.Equal("", TextConverter.Convert("", Map, LayoutSide.A));

    [Theory]
    [InlineData("ghbdtn", LayoutSide.A)]
    [InlineData("руддщ", LayoutSide.B)]
    [InlineData("ghbdtn мир", LayoutSide.A)]
    [InlineData("hi привет", LayoutSide.B)]
    public void DetectSource_uses_majority(string text, LayoutSide expected) =>
        Assert.Equal(expected, TextConverter.DetectSource(text, Map, currentSide: expected.Opposite()));

    [Theory]
    [InlineData("", LayoutSide.A)]
    [InlineData("", LayoutSide.B)]
    [InlineData("123 !", LayoutSide.A)]
    [InlineData(".", LayoutSide.B)]
    [InlineData("ab фы", LayoutSide.B)]
    public void DetectSource_uses_current_side_on_tie(string text, LayoutSide current) =>
        Assert.Equal(current, TextConverter.DetectSource(text, Map, current));
}
