# Stealth's PRK Companion

> A clean, fullscreen web companion for **Anarchy Online** and **Project Rubi-Ka**.

PRK Companion puts the AO sites and quick math you actually use behind one hotkey—no Alt-Tab loop, no client injection, and no interaction with the game itself.

## Download and run

1. Download [`PRK-Companion-win-x64.zip`](https://github.com/stealth-prk/PRK-Companion/releases/latest) from the latest release.
2. Extract the ZIP anywhere you like.
3. Double-click `PRK-Companion.exe`.

## Screenshots

### AO resources without leaving the game

![PRK Companion resource overlay with Auno open](screenshots/resource-overlay.png)

### Tune the overlay to your setup

![PRK Companion settings for opacity, hotkey, and overlay scale](screenshots/settings.png)

## What’s inside

- **Global hotkey overlay** — Backtick/tilde by default, with F1, F8, and F10 options.
- **Persistent resource tabs** — once a site is loaded, switching away and back does not reload it.
- **AO quick links** — Auno, TinkerTools, PRKTools, Faffy’s PRK Guide, the PRK Portal, Bug Report, and AO-Universe resources.
- **AO-Universe dropdown** — home, implants, buffing, pocket bosses, dyna-camps, and the master blitz list in one compact menu.
- **Web Browser** — enter a URL directly or search the web from the overlay.
- **Calculator** — normal keyboard support, chained formulas, Enter to calculate, and a visible calculation history.
- **Settings that stick** — overlay opacity, hotkey, and 80%–120% overlay scale are saved between sessions.

## Quick controls

| Action | Control |
| --- | --- |
| Show or hide the overlay | Assigned global hotkey |
| Return to AO | Assigned hotkey or `Esc` |
| Close PRK Companion completely | `QUIT` |
| Open overlay options | `SETTINGS` |

## Deliberately simple and AO-safe

PRK Companion is only a Windows web companion. It does **not** inject into the AO client, read memory, automate gameplay, send input to AO, or modify the Project Rubi-Ka GUI mod.

## For contributors

To run from source, install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then run this from the project folder:

```powershell
dotnet run
```

Pushing a version tag triggers GitHub Actions to build the self-contained Windows ZIP release.
