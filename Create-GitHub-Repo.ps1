$ErrorActionPreference = 'Stop'

$repository = 'stealth-prk/PRK-Companion'
$releaseTag = 'v0.24.0'

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'GitHub CLI is required. Install it with: winget install --id GitHub.cli'
}

gh auth status
if ($LASTEXITCODE -ne 0) {
    throw 'Sign in first with: gh auth login'
}

Set-Location $PSScriptRoot

if (-not (Test-Path '.git')) {
    git init -b main
    git config user.name 'Andy Ballacchino'
    git config user.email '165856874+stealth-prk@users.noreply.github.com'
    git add .
    git commit -m 'Initial PRK Companion release'
}

gh repo view $repository *> $null
if ($LASTEXITCODE -ne 0) {
    gh repo create $repository `
        --public `
        --description 'A clean Windows knowledge overlay and web companion for Anarchy Online / Project Rubi-Ka.' `
        --source . `
        --remote origin `
        --push
}
elseif (-not (git remote get-url origin 2>$null)) {
    git remote add origin "https://github.com/$repository.git"
    git push -u origin main
}

if (-not (git tag --list $releaseTag)) {
    git tag $releaseTag
    git push origin $releaseTag
}

Write-Host "`nRepository: https://github.com/$repository" -ForegroundColor Cyan
Write-Host "The $releaseTag tag was pushed. GitHub Actions is building the Windows release now." -ForegroundColor Cyan
