# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.9.0] - 2026-10-05

First public pre-release.

### Added

- `Pause` fixes the last word typed in the wrong layout and switches the layout. Press it again to undo.
- `Shift+Pause` fixes the selected text and restores the previous clipboard content.
- Any pair of installed keyboard layouts, English and Russian by default.
- Tray menu: Pause, Start with Windows, Open settings, Reload settings, Exit.
- `settings.json` with hotkeys, layout pair and clipboard restore delay; invalid values fall back to defaults with a notification.
- English and Russian UI, chosen from the Windows UI language.
- Per-user installer without admin rights.

[Unreleased]: https://github.com/abdumalikyaqub/tuzat/compare/v0.9.0...HEAD
[0.9.0]: https://github.com/abdumalikyaqub/tuzat/releases/tag/v0.9.0
