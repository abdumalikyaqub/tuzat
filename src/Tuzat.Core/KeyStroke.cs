namespace Tuzat.Core;

public readonly record struct KeyStroke(int VirtualKey, ushort ScanCode, bool IsExtended, bool Shift);
