namespace Tuzat.Core;

public sealed class WordBuffer
{
    public const int MaxLength = 64;

    private readonly List<KeyStroke> _strokes = new(MaxLength + 1);
    private nint _window;

    public void OnKeyDown(KeyStroke stroke, bool hasCommandModifier, nint window)
    {
        if (window != _window)
        {
            _strokes.Clear();
            _window = window;
        }

        var kind = KeyClassifier.Classify(stroke.VirtualKey);
        if (kind is KeyKind.Modifier or KeyKind.Ignore) return;
        if (hasCommandModifier)
        {
            _strokes.Clear();
            return;
        }

        switch (kind)
        {
            case KeyKind.Printable:
                if (_strokes.Count > 0 && _strokes[^1].VirtualKey == VirtualKey.Space) _strokes.Clear();
                Add(stroke);
                break;
            case KeyKind.Space:
                Add(stroke);
                break;
            case KeyKind.Backspace:
                if (_strokes.Count > 0) _strokes.RemoveAt(_strokes.Count - 1);
                break;
            default:
                _strokes.Clear();
                break;
        }
    }

    public KeyStroke[] Snapshot() => _strokes.ToArray();

    // The hotkey itself never reaches OnKeyDown, so the window check must also run here.
    public KeyStroke[] Snapshot(nint window)
    {
        if (window != _window)
        {
            _strokes.Clear();
            _window = window;
        }

        return Snapshot();
    }

    public void Clear() => _strokes.Clear();

    // A run longer than MaxLength is not a word, so a partial replay would be wrong.
    private void Add(KeyStroke stroke)
    {
        _strokes.Add(stroke);
        if (_strokes.Count > MaxLength) _strokes.Clear();
    }
}
