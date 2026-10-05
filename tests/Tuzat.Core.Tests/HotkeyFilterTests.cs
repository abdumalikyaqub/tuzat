namespace Tuzat.Core.Tests;

public class HotkeyFilterTests
{
    private static HotkeyFilter CreateFilter() => new()
    {
        Bindings = new HotkeyBindings(
            new Hotkey(HotkeyModifiers.None, VirtualKey.Pause),
            new Hotkey(HotkeyModifiers.Shift, VirtualKey.Pause)),
    };

    [Theory]
    [InlineData(HotkeyModifiers.None, HotkeyAction.FixLastWord)]
    [InlineData(HotkeyModifiers.Shift, HotkeyAction.FixSelection)]
    public void Triggers_matching_hotkey(HotkeyModifiers modifiers, HotkeyAction expected)
    {
        Assert.Equal(KeyDecision.Trigger, CreateFilter().OnKeyDown(VirtualKey.Pause, modifiers, out var action));
        Assert.Equal(expected, action);
    }

    [Fact]
    public void Extra_modifier_does_not_match() =>
        Assert.Equal(KeyDecision.Pass, CreateFilter().OnKeyDown(VirtualKey.Pause, HotkeyModifiers.Ctrl, out _));

    [Fact]
    public void Other_key_passes() =>
        Assert.Equal(KeyDecision.Pass, CreateFilter().OnKeyDown('G', HotkeyModifiers.None, out _));

    [Fact]
    public void No_bindings_pass_everything() =>
        Assert.Equal(KeyDecision.Pass, new HotkeyFilter().OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _));

    [Fact]
    public void Repeated_key_down_is_swallowed_without_second_trigger()
    {
        var filter = CreateFilter();
        filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _);

        Assert.Equal(KeyDecision.Swallow, filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _));
    }

    [Fact]
    public void Key_up_of_triggered_key_is_swallowed_once()
    {
        var filter = CreateFilter();
        filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _);

        Assert.True(filter.OnKeyUp(VirtualKey.Pause));
        Assert.False(filter.OnKeyUp(VirtualKey.Pause));
    }

    [Fact]
    public void Key_up_of_other_key_passes()
    {
        var filter = CreateFilter();
        filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _);

        Assert.False(filter.OnKeyUp('G'));
    }

    [Fact]
    public void Next_press_after_key_up_triggers_again()
    {
        var filter = CreateFilter();
        filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _);
        filter.OnKeyUp(VirtualKey.Pause);

        Assert.Equal(KeyDecision.Trigger, filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _));
    }

    [Fact]
    public void Reset_forgets_held_key()
    {
        var filter = CreateFilter();
        filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _);
        filter.Reset();

        Assert.Equal(KeyDecision.Trigger, filter.OnKeyDown(VirtualKey.Pause, HotkeyModifiers.None, out _));
    }
}
