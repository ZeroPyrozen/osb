<#
.SYNOPSIS
    Verifies the codebase contains no PHP.

.DESCRIPTION
    Fails (exit code 1) if the repository has any PHP source files (.php, .phtml, .phar, ...),
    Composer manifests (composer.json / composer.lock), or files containing a PHP open tag.
    Uses git's file list when available so ignored folders (bin, obj, node_modules) are skipped.

.EXAMPLE
    .\scripts\verify-no-php.ps1

    Checks the repository this script lives in.

.EXAMPLE
    .\scripts\verify-no-php.ps1 -Path C:\src\other-repo
#>
[CmdletBinding()]
param(
    [string]$Path = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path $Path).Path

# Prefer git's view of the repo (tracked + untracked, minus .gitignore); fall back to a plain walk.
$files = $null
if (Get-Command git -ErrorAction SilentlyContinue) {
    $files = git -C $root ls-files --cached --others --exclude-standard 2>$null
    if ($LASTEXITCODE -ne 0) { $files = $null }
}
if (-not $files) {
    $skip = '[\\/](\.git|bin|obj|node_modules)[\\/]'
    $files = Get-ChildItem -Path $root -Recurse -File |
        Where-Object { $_.FullName -notmatch $skip } |
        ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) }
}

$self = [IO.Path]::GetRelativePath($root, $PSCommandPath) -replace '\\', '/'
$phpExtensions = '\.(php\d?|phtml|phar|phps)$'
$phpTag = '<\?php'
$findings = @()
$files = @($files)
$total = $files.Count
$checked = 0

foreach ($file in $files) {
    $checked++
    # Redrawing the bar for every file slows the scan down, so update it every 20 files.
    if ($checked % 20 -eq 0 -or $checked -eq $total) {
        Write-Progress -Activity 'Scanning for PHP' -Status "$checked of $total files ($($findings.Count) found)" `
            -CurrentOperation $file -PercentComplete ([int]($checked * 100 / $total))
    }
    $relative = $file -replace '\\', '/'
    if ($relative -eq $self) { continue }
    $full = Join-Path $root $file
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { continue }

    $name = Split-Path -Leaf $relative
    if ($relative -match $phpExtensions) {
        $findings += "PHP file:          $relative"
    }
    elseif ($name -in 'composer.json', 'composer.lock') {
        $findings += "Composer manifest: $relative"
    }
    elseif (Select-String -LiteralPath $full -Pattern $phpTag -Quiet -ErrorAction SilentlyContinue) {
        $findings += "PHP open tag:      $relative"
    }
}

Write-Progress -Activity 'Scanning for PHP' -Completed

if ($findings.Count -gt 0) {
    Write-Host "PHP found in $root ($($findings.Count)):" -ForegroundColor Red
    $findings | ForEach-Object { Write-Host "  $_" }
    exit 1
}

Write-Host "No PHP found in $root ($total files checked)." -ForegroundColor Green
exit 0
