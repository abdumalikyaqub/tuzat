namespace Tuzat.Core;

public enum LayoutSide { A, B }

public static class LayoutSideExtensions
{
    public static LayoutSide Opposite(this LayoutSide side) => side == LayoutSide.A ? LayoutSide.B : LayoutSide.A;
}

public sealed class LayoutMap
{
    private readonly Dictionary<char, char> _aToB = [];
    private readonly Dictionary<char, char> _bToA = [];

    // The first pair for a character wins, so the map stays one-to-one.
    public LayoutMap(IEnumerable<(char A, char B)> pairs)
    {
        foreach (var (a, b) in pairs)
        {
            if (a == b || _aToB.ContainsKey(a) || _bToA.ContainsKey(b)) continue;
            _aToB[a] = b;
            _bToA[b] = a;
        }
    }

    public bool IsOnSideA(char c) => _aToB.ContainsKey(c);

    public bool IsOnSideB(char c) => _bToA.ContainsKey(c);

    public bool TryMap(char c, LayoutSide from, out char mapped) =>
        (from == LayoutSide.A ? _aToB : _bToA).TryGetValue(c, out mapped);
}
