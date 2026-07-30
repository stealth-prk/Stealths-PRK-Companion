# PRK Companion

A deliberately small, standalone web companion for Anarchy Online / Project Rubi-Ka.

It provides an always-on-top browser window for AO resources without injecting into, reading from, or automating the game client.

## Features

- Global backtick / `` ` `` hotkey to show/hide the overlay
- Full-screen, borderless, always-on-top overlay
- Semi-transparent dark-teal OEM+ UI inspired by the PRK GUI skin
- Quick links for Auno, TinkerTools, PRKTools, AO-Universe, and PRK resources
- Persistent site tabs: a visited resource stays loaded when you switch away
- Native basic calculator for AO math
- Web Browser tab with URL entry and DuckDuckGo search
- Saved settings for opacity, global hotkey (Backtick, F1, F8, or F10), and overlay scale (80%–120%)
- Normal URL bar for any other AO resource

## Download

Download `PRK-Companion-win-x64.zip` from the latest GitHub release, extract it, and double-click `PRK-Companion.exe`.

The release is self-contained, so end users do not need the .NET SDK. Microsoft Edge WebView2 Runtime is required for the embedded websites and is already included with current Windows 10 and Windows 11 installations.

## Run from source

On Windows, install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and the [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/), then run:

```powershell
dotnet run --project .\PRK-Companion\PRK-Companion.csproj
```

The app needs no access to AO itself. Press `` ` `` to bring it up, select a bookmark, then press `` ` `` or `Esc` to return to AO.

## Create the Windows app

Run this once from the project folder on a Windows x64 machine with the .NET 8 SDK:

```powershell
.\Publish-PRK-Companion.ps1
```

It creates `Release\PRK-Companion\PRK-Companion.exe`: a self-contained Windows app that runs by double-clicking the `.exe`, with no `dotnet run` or SDK required for the person using it. Microsoft Edge WebView2 Runtime is still required for the embedded sites.

Maintainers can publish a GitHub release by pushing a version tag:

```powershell
git tag v0.24.0
git push origin v0.24.0
```

The included GitHub Actions workflow builds the self-contained Windows app, packages `PRK-Companion-win-x64.zip`, attaches it to the release, and also keeps it as a workflow artifact.

## Intentional boundaries

This project will stay a simple desktop browser companion:

- No AO client injection
- No memory reading
- No automated gameplay or input
- No changes to the Project Rubi-Ka GUI mod
