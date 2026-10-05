# Security Policy

Tuzat uses a global keyboard hook, so security matters a lot for this project.

## Supported versions

Only the latest release gets security fixes.

## Report a vulnerability

Do **not** open a public issue. Use [GitHub private vulnerability reporting](https://github.com/abdumalikyaqub/tuzat/security/advisories/new) instead.

Please include the Tuzat version, the Windows version, the steps to reproduce, and the impact. You can expect a first answer within 7 days.

## What Tuzat does with your input

- It keeps at most 64 key presses of the current word in memory. It does not keep them anywhere else.
- It never writes typed or converted text to disk, logs or notifications. `crash.log` holds only the exception type and stack trace.
- It has no network code.
- `Shift+Pause` puts the converted text on the clipboard for a short time and marks it as excluded from clipboard history and cloud sync. After the paste, your previous clipboard content is restored.

Problems in these areas are in scope: anything that leaks typed text, sends input to the wrong window, or lets another process misuse Tuzat.
