namespace Tuzat.Core.Tests;

public class LayoutMapTests
{
    [Fact]
    public void Maps_both_directions()
    {
        var map = new LayoutMap([('a', 'ф')]);

        Assert.True(map.IsOnSideA('a'));
        Assert.True(map.IsOnSideB('ф'));
        Assert.True(map.TryMap('a', LayoutSide.A, out var toB));
        Assert.Equal('ф', toB);
        Assert.True(map.TryMap('ф', LayoutSide.B, out var toA));
        Assert.Equal('a', toA);
    }

    [Fact]
    public void Identical_characters_are_excluded()
    {
        var map = new LayoutMap([('1', '1')]);

        Assert.False(map.IsOnSideA('1'));
        Assert.False(map.IsOnSideB('1'));
    }

    [Fact]
    public void First_pair_wins_on_side_A()
    {
        var map = new LayoutMap([('a', 'ф'), ('a', 'x')]);

        Assert.True(map.TryMap('a', LayoutSide.A, out var mapped));
        Assert.Equal('ф', mapped);
        Assert.False(map.IsOnSideB('x'));
    }

    [Fact]
    public void First_pair_wins_on_side_B()
    {
        var map = new LayoutMap([('a', 'ф'), ('b', 'ф')]);

        Assert.True(map.TryMap('ф', LayoutSide.B, out var mapped));
        Assert.Equal('a', mapped);
        Assert.False(map.IsOnSideA('b'));
    }

    [Fact]
    public void Unknown_character_is_not_mapped() => Assert.False(TestLayouts.EnRu.TryMap('5', LayoutSide.A, out _));

    [Fact]
    public void Opposite_side()
    {
        Assert.Equal(LayoutSide.B, LayoutSide.A.Opposite());
        Assert.Equal(LayoutSide.A, LayoutSide.B.Opposite());
    }
}
