<#
.SYNOPSIS
    Builds osb and deploys it to a Raspberry Pi over SSH.

.DESCRIPTION
    Publishes a self-contained Linux build (the Pi needs no .NET install), uploads it with
    scp and installs it with deploy/pi-install.sh. The first deploy also provisions the Pi
    (system user, systemd service, /etc/osb/osb.env). Every deploy goes into a new release
    folder and is health-checked; if it doesn't come up, the previous release is restored.

    Needs the .NET 10 SDK, plus the OpenSSH client and tar that ship with Windows 10/11.

.EXAMPLE
    .\deploy\deploy.ps1 -SshHost pi@raspberrypi.local

    Deploys to a Pi running 64-bit Raspberry Pi OS.

.EXAMPLE
    .\deploy\deploy.ps1 -SshHost pi@raspberrypi.local -Runtime linux-arm

    Deploys to a Pi running 32-bit Raspberry Pi OS.

.EXAMPLE
    .\deploy\deploy.ps1 -SshHost pi@raspberrypi.local -Rollback

    Switches the Pi back to the previous release, keeping the current database.

.EXAMPLE
    .\deploy\deploy.ps1 -SshHost pi@raspberrypi.local -Rollback -RestoreDb

    Switches back and also restores the database backup taken before the current release
    was deployed. Changes made since then (e.g. learners' progress) are lost.
#>
[CmdletBinding()]
param(
    # SSH destination, e.g. pi@raspberrypi.local or pi@192.168.1.50
    [Parameter(Mandatory = $true)]
    [string] $SshHost,

    # linux-arm64 for 64-bit Raspberry Pi OS, linux-arm for 32-bit
    [ValidateSet('linux-arm64', 'linux-arm', 'linux-x64')]
    [string] $Runtime = 'linux-arm64',

    [int] $Port = 22,

    # Switch back to the previous release instead of deploying
    [switch] $Rollback,

    # With -Rollback: also restore the database from before the current release was deployed
    [switch] $RestoreDb
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

# Runs a native command and stops the script if it fails.
function Invoke-Native([string] $Exe, [string[]] $Arguments) {
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "'$Exe' failed with exit code $LASTEXITCODE." }
}

# Short commit hash for the release folder name, so you can tell what is deployed.
function Get-ReleaseLabel {
    $ErrorActionPreference = 'Continue'   # git writes to stderr outside a repo; not fatal here
    $sha = & git -C $repoRoot rev-parse --short HEAD 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $sha) { return 'local' }
    if (& git -C $repoRoot status --porcelain 2>$null) { return "$sha-dirty" }
    return "$sha"
}

foreach ($tool in 'ssh', 'scp', 'dotnet') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' was not found on PATH." }
}
# Prefer Windows' own tar: Git's GNU tar would treat "C:\..." as a remote host.
$tar = 'tar'
if ($env:SystemRoot -and (Test-Path (Join-Path $env:SystemRoot 'System32\tar.exe'))) {
    $tar = Join-Path $env:SystemRoot 'System32\tar.exe'
}

if ($RestoreDb -and -not $Rollback) { throw '-RestoreDb only works together with -Rollback.' }
if ($Rollback) {
    $command = 'sudo /opt/osb/pi-install.sh rollback'
    if ($RestoreDb) { $command += ' --restore-db' }
    Invoke-Native ssh @('-p', "$Port", '-t', $SshHost, $command)
    return
}

$label = Get-ReleaseLabel
$outDir = Join-Path $repoRoot "bin\deploy\$Runtime"
$bundleDir = Join-Path $outDir 'bundle'

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $bundleDir 'deploy') -Force | Out-Null

Write-Host "==> Publishing self-contained $Runtime build ($label)"
Invoke-Native dotnet @('publish', (Join-Path $repoRoot 'osb.csproj'), '-c', 'Release', '-r', $Runtime,
    '--self-contained', '-o', (Join-Path $bundleDir 'app'), '-nologo')

Write-Host '==> Packing'
foreach ($file in 'pi-install.sh', 'osb.service', 'osb.env.example') {
    Copy-Item (Join-Path $PSScriptRoot $file) (Join-Path $bundleDir 'deploy')
}
Copy-Item (Join-Path $PSScriptRoot 'pi-install.sh') (Join-Path $outDir 'osb-pi-install.sh')
Invoke-Native $tar @('-czf', (Join-Path $outDir 'osb-bundle.tar.gz'), '-C', $bundleDir, 'app', 'deploy')

Write-Host "==> Uploading to $SshHost"
Push-Location $outDir
try {
    # Plain file names keep drive letters out of scp's host:path parsing.
    Invoke-Native scp @('-P', "$Port", 'osb-bundle.tar.gz', 'osb-pi-install.sh', "${SshHost}:")
}
finally {
    Pop-Location
}

Write-Host "==> Installing on $SshHost"
$remote = 'sed -i ''s/\r$//'' ~/osb-pi-install.sh && sudo bash ~/osb-pi-install.sh install ~/osb-bundle.tar.gz ' +
    $label + '; rc=$?; rm -f ~/osb-bundle.tar.gz ~/osb-pi-install.sh; exit $rc'
Invoke-Native ssh @('-p', "$Port", '-t', $SshHost, $remote)

$piHost = $SshHost.Split('@')[-1]
Write-Host "==> Deployed $label. Browse to http://${piHost}:5000 (or the port set in /etc/osb/osb.env)."
