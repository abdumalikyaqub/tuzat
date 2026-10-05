# Architecture

This document is for contributors. It explains how Tuzat works and why the Win32 parts look the way they do.

## Projects

| Project | Target | Role |
|---|---|---|
| `src/Tuzat.Core` | `net10.0` | All logic that does not need Win32: key classification, the word buffer, the layout character map, text conversion, hotkeys, settings, localization. Fully unit-tested. |
| `src/Tuzat.App` | `net10.0-windows` | A thin WinForms tray app: keyboard and mouse hooks, `SendInput`, layout switching, clipboard. |
| `tests/Tuzat.Core.Tests` | `net10.0` | xUnit tests for `Tuzat.Core`. |
| `installer/Tuzat.iss` | Inno Setup 6 | Per-user installer. |

## Fix the last word (`Pause`)

```
key press ─► InputHooks (hook thread)
              ├─ HotkeyFilter: is it the hotkey?  ── no ─► WordBuffer.OnKeyDown
              └─ yes: swallow it, Snapshot(foreground window) ─► post to UI thread
UI thread ─► FixLastWordAction
              1. release held modifiers
              2. send N × Backspace
              3. LayoutSwitcher.SwitchTo(other layout) and wait until it is active
              4. InputSender.Replay: send the same scan codes again
```

The buffer stores **scan codes**, not characters. Scan codes are physical key positions, so after the layout switch Windows produces the right characters by itself. No character table is needed for this path, and it works for any layout pair.

The buffer is not cleared after the fix, so a second `Pause` converts the word back.

## Fix the selection (`Shift+Pause`)

```
UI thread ─► FixSelectionAction
              1. release held modifiers
              2. ClipboardGuard.TrySave: copy every clipboard format that is plain memory
              3. send Ctrl+C and wait until the clipboard sequence number changes
              4. TextConverter: detect the source layout by majority, map each character via LayoutMap
              5. put the result on the clipboard (marked as excluded from history), send Ctrl+V
              6. wait clipboardRestoreDelayMs, restore the saved clipboard, switch the layout
```

`LayoutMapBuilder` builds the character table from the two installed layouts with `ToUnicodeEx` (flag `0x4`, so dead-key state is not changed). The first pair for a character wins, so the table stays one-to-one.

## Threading

- **Hooks run on their own thread** with a message loop. Windows silently removes a low-level hook if its thread does not answer in time, and the UI thread often waits (for the layout switch, for the clipboard).
- **Actions run on the UI thread** (STA, needed for the clipboard). They are posted from the hook thread, so they run one at a time.
- **`ThreadWait.Sleep`** is used instead of `Thread.Sleep` on the UI thread. It still processes sent messages. Without it, another app's `Ctrl+C` would hang while Tuzat owns the clipboard.

## Key decisions

| Decision | Reason |
|---|---|
| Tuzat marks its own `SendInput` events with `dwExtraInfo = 0x545A5401` | The hook ignores only its own events. Using `LLKHF_INJECTED` would also ignore on-screen keyboards and other tools and break the buffer. |
| The hotkey snapshot checks the foreground window | If a dialog takes focus after typing, `Pause` must not retype the word into the dialog. |
| Right Ctrl, right Alt and Win are released with `KEYEVENTF_EXTENDEDKEY` | Without the flag, Windows applies the release to the left key and the right key stays down. |
| Alt and Win releases are preceded by the unassigned key `0xE8` | A lone Alt or Win release would open a menu or the Start menu. |
| Clipboard content is marked with `ExcludeClipboardContentFromMonitorProcessing`, `CanIncludeInClipboardHistory = 0`, `CanUploadToCloudClipboard = 0` | The temporary converted text must not appear in Win+V history, cloud sync or clipboard managers. |
| Unknown keys reset the buffer | They may move the caret or type text that cannot be replayed. |
| Two UI languages through `Localized.Text` | Two languages do not justify resource files. English is the fallback. |

## Privacy rules

- Key presses live only in memory, at most 64, for the current word.
- Typed or converted text is never written to disk, logs or notifications.
- There is no network code.

## Testing

`Tuzat.Core` is covered by unit tests (`dotnet test`). The Win32 layer is checked by hand with the checklist in [CONTRIBUTING.md](../CONTRIBUTING.md#manual-test-checklist).
