namespace Tuzat.Core.Tests;

public class WordBufferTests
{
    private const nint Window = 1;

    private static KeyStroke Key(int virtualKey, bool shift = false) => new(virtualKey, (ushort)virtualKey, false, shift);

    private static WordBuffer Type(params int[] keys)
    {
        var buffer = new WordBuffer();
        foreach (var key in keys) buffer.OnKeyDown(Key(key), hasCommandModifier: false, Window);
        return buffer;
    }

    private static int[] Keys(WordBuffer buffer) => buffer.Snapshot().Select(s => s.VirtualKey).ToArray();

    [Fact]
    public void Printable_keys_are_collected() => Assert.Equal(new int[] { 'G', 'H', 'B' }, Keys(Type('G', 'H', 'B')));

    [Fact]
    public void Snapshot_does_not_clear_the_buffer()
    {
        var buffer = Type('G', 'H');
        buffer.Snapshot();
        Assert.Equal(new int[] { 'G', 'H' }, Keys(buffer));
    }

    [Fact]
    public void Trailing_spaces_are_kept() =>
        Assert.Equal(new int[] { 'H', 'I', VirtualKey.Space, VirtualKey.Space }, Keys(Type('H', 'I', VirtualKey.Space, VirtualKey.Space)));

    [Fact]
    public void Printable_after_space_starts_new_word() =>
        Assert.Equal(new int[] { 'X' }, Keys(Type('H', 'I', VirtualKey.Space, 'X')));

    [Fact]
    public void Backspace_inside_word_keeps_only_final_text() =>
        Assert.Equal(new int[] { 'G', 'H', 'B', 'D' }, Keys(Type('G', 'H', 'B', 'X', VirtualKey.Back, 'D')));

    [Fact]
    public void Backspace_on_empty_buffer_does_nothing() =>
        Assert.Equal(new int[] { 'A' }, Keys(Type(VirtualKey.Back, 'A')));

    [Theory]
    [InlineData(VirtualKey.Return)]
    [InlineData(VirtualKey.Tab)]
    [InlineData(VirtualKey.Escape)]
    [InlineData(VirtualKey.F1)]
    [InlineData(0x25)] // Left
    [InlineData(0x2E)] // Delete
    public void Reset_keys_clear_the_buffer(int key) => Assert.Empty(Type('G', 'H', key).Snapshot());

    [Fact]
    public void Key_with_command_modifier_clears_the_buffer()
    {
        var buffer = Type('G', 'H');
        buffer.OnKeyDown(Key('C'), hasCommandModifier: true, Window);
        Assert.Empty(buffer.Snapshot());
    }

    [Fact]
    public void Modifier_and_ignored_keys_do_not_change_the_buffer() =>
        Assert.Equal(new int[] { 'G' }, Keys(Type('G', VirtualKey.Shift, VirtualKey.Capital, VirtualKey.Pause)));

    [Fact]
    public void Shift_state_is_stored()
    {
        var buffer = new WordBuffer();
        buffer.OnKeyDown(Key('G', shift: true), false, Window);
        Assert.True(Assert.Single(buffer.Snapshot()).Shift);
    }

    [Fact]
    public void Window_change_starts_new_buffer()
    {
        var buffer = Type('G', 'H');
        buffer.OnKeyDown(Key('X'), false, window: 2);
        Assert.Equal(new int[] { 'X' }, Keys(buffer));
    }

    [Fact]
    public void Buffer_holds_max_length() =>
        Assert.Equal(WordBuffer.MaxLength, Type(Enumerable.Repeat((int)'G', WordBuffer.MaxLength).ToArray()).Snapshot().Length);

    [Fact]
    public void Overflow_clears_the_buffer() =>
        Assert.Empty(Type(Enumerable.Repeat((int)'G', WordBuffer.MaxLength + 1).ToArray()).Snapshot());

    [Fact]
    public void Clear_empties_the_buffer()
    {
        var buffer = Type('G', 'H');
        buffer.Clear();
        Assert.Empty(buffer.Snapshot());
    }
}
