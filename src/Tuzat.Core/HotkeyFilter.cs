namespace Tuzat.Core;

public enum KeyDecision { Pass, Swallow, Trigger }

// Fires a hotkey once per press and hides its auto-repeat and key-up from other apps.
public sealed class HotkeyFilter
{
    private volatile HotkeyBindings? _bindings;
    private int _heldKey = -1;

    public HotkeyBindings? Bindings
    {
        get => _bindings;
        set => _bindings = value;
    }

    public KeyDecision OnKeyDown(int key, HotkeyModifiers modifiers, out HotkeyAction action)
    {
        action = default;
        if (key == _heldKey) return KeyDecision.Swallow;
        if (_bindings?.Match(key, modifiers) is not { } match) return KeyDecision.Pass;

        _heldKey = key;
        action = match;
        return KeyDecision.Trigger;
    }

    public bool OnKeyUp(int key)
    {
        if (key != _heldKey) return false;
        _heldKey = -1;
        return true;
    }

    public void Reset() => _heldKey = -1;
}
