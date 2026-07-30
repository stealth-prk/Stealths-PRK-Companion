$ErrorActionPreference = 'Stop'

$releaseDirectory = Join-Path $PSScriptRoot 'Release\PRK-Companion'

dotnet publish "$PSScriptRoot\PRK-Companion.csproj" `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  --output $releaseDirectory

Write-Host "`nRelease build complete: $releaseDirectory\PRK-Companion.exe" -ForegroundColor Cyan
