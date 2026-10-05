namespace Tuzat.Core;

public static class TextConverter
{
    // Characters present on both sides (like '.') or on neither side say nothing about the source.
    public static LayoutSide DetectSource(string text, LayoutMap map, LayoutSide currentSide)
    {
        int a = 0, b = 0;
        foreach (var c in text)
        {
            bool onA = map.IsOnSideA(c), onB = map.IsOnSideB(c);
            if (onA && !onB) a++;
            else if (onB && !onA) b++;
        }

        return a > b ? LayoutSide.A : b > a ? LayoutSide.B : currentSide;
    }

    public static string Convert(string text, LayoutMap map, LayoutSide source)
    {
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (map.TryMap(chars[i], source, out var mapped)) chars[i] = mapped;
        }

        return new string(chars);
    }
}
