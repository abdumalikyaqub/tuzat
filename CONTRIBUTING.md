# Contributing to Tuzat

Thank you for helping. This guide explains how to report problems, suggest ideas and send code.

## Report a bug

Open a [bug report](https://github.com/abdumalikyaqub/tuzat/issues/new?template=bug_report.yml). Include:

- Tuzat version and Windows version.
- Your two keyboard layouts.
- The app where it happened (for example Chrome, Word, Windows Terminal).
- Steps, what you expected, and what happened.

Do not paste passwords or private text. A short made-up word like `ghbdtn` is enough.

Security problems go through a private report, not an issue. See [SECURITY.md](SECURITY.md).

## Suggest a feature

Open a [feature request](https://github.com/abdumalikyaqub/tuzat/issues/new?template=feature_request.yml). Describe the problem first, then your idea. Tuzat stays small on purpose: automatic correction and dictionaries are out of scope.

## Development setup

You need Windows 10 or 11 and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
dotnet build
dotnet test
dotnet run --project src/Tuzat.App
```

For the installer, install Inno Setup 6 (`winget install --id JRSoftware.InnoSetup --exact`) and run `./build.ps1`.

Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before you change the hooks, input or clipboard code.

## Code rules

- Logic that does not need Win32 goes to `Tuzat.Core` and gets xUnit tests. Write the test first.
- `Tuzat.App` stays a thin Win32 layer. Interop uses `LibraryImport`.
- The build treats warnings as errors.
- Comments are short, in English, and explain *why*, not *what*.
- Text shown to users goes through `Localized.Text("English", "Русский")`.
- Never write typed or converted text to disk, logs or notifications.

## Manual test checklist

The Win32 layer has no automated tests. Before a pull request that touches `Tuzat.App`, run Tuzat with `dotnet run --project src/Tuzat.App` and check:

1. Notepad: type `ghbdtn`, press `Pause` → `привет`. Press `Pause` again → `ghbdtn`.
2. Hold `Pause` for two seconds → only one conversion.
3. Type `ghbx`, `Backspace`, `dtn`, press `Pause` → `привет`.
4. Press `Pause` with nothing typed → only the layout switches.
5. Type a word, click somewhere with the mouse, press `Pause` → text does not change, only the layout switches.
6. Select `ghbdtn vbh` and press `Shift+Pause` → `привет мир`.
7. Select two lines with digits and an emoji and press `Shift+Pause` → line breaks, digits and emoji stay.
8. Copy an image, then use `Shift+Pause`, then paste → the image is still in the clipboard.
9. Open Win+V → the converted text is not in the history.
10. Repeat steps 1 and 6 in a browser, VS Code, Word, a messenger and Windows Terminal.
11. A window started as administrator → nothing happens, Tuzat does not crash.
12. After sleep and after locking and unlocking Windows → `Pause` still works.

## Pull requests

1. Fork the repository and create a branch from `main`.
2. Keep one change per pull request.
3. Use [Conventional Commits](https://www.conventionalcommits.org/) for commit messages, for example `fix: keep clipboard image after Shift+Pause`.
4. Make sure `dotnet build` and `dotnet test` pass.
5. Add a line to `CHANGELOG.md` under `Unreleased`.
6. Fill in the pull request template.

By contributing, you agree that your work is released under the [MIT License](LICENSE) and that you follow the [Code of Conduct](CODE_OF_CONDUCT.md).
