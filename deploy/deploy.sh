#!/usr/bin/env bash
# Builds osb and deploys it to a Raspberry Pi over SSH: the bash twin of deploy.ps1, for
# Linux, macOS, WSL and Git Bash. See deploy/README.md.
#
#   ./deploy/deploy.sh pi@raspberrypi.local              # 64-bit Raspberry Pi OS
#   ./deploy/deploy.sh pi@raspberrypi.local linux-arm    # 32-bit Raspberry Pi OS
#   ./deploy/deploy.sh pi@raspberrypi.local rollback     # back to the previous release
#   ./deploy/deploy.sh pi@raspberrypi.local rollback --restore-db
#                                                        # ...and its database (see README)
#
# Set SSH_PORT to use a port other than 22.
set -euo pipefail

target=${1:?usage: deploy.sh user@host [linux-arm64|linux-arm|linux-x64|rollback [--restore-db]]}
mode=${2:-linux-arm64}
port=${SSH_PORT:-22}
repo=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)

case $mode in
    rollback)
        case ${3:-} in
            '') exec ssh -p "$port" -t "$target" 'sudo /opt/osb/pi-install.sh rollback' ;;
            --restore-db) exec ssh -p "$port" -t "$target" 'sudo /opt/osb/pi-install.sh rollback --restore-db' ;;
            *) echo "unknown rollback option '$3'" >&2; exit 2 ;;
        esac ;;
    linux-arm64 | linux-arm | linux-x64) rid=$mode ;;
    *) echo "unknown runtime '$mode'" >&2; exit 2 ;;
esac

# Short commit hash for the release folder name, so you can tell what is deployed.
label=$(git -C "$repo" rev-parse --short HEAD 2>/dev/null) || label=local
if [[ $label != local && -n $(git -C "$repo" status --porcelain 2>/dev/null) ]]; then
    label+=-dirty
fi

out=$repo/bin/deploy/$rid
rm -rf "$out"
mkdir -p "$out/bundle/deploy"

echo "==> Publishing self-contained $rid build ($label)"
dotnet publish "$repo/osb.csproj" -c Release -r "$rid" --self-contained -o "$out/bundle/app" -nologo

echo "==> Packing"
cp "$repo"/deploy/{pi-install.sh,osb.service,osb.env.example} "$out/bundle/deploy/"
cp "$repo/deploy/pi-install.sh" "$out/osb-pi-install.sh"
COPYFILE_DISABLE=1 tar -czf "$out/osb-bundle.tar.gz" -C "$out/bundle" app deploy

echo "==> Uploading to $target"
(cd "$out" && scp -P "$port" osb-bundle.tar.gz osb-pi-install.sh "$target:")

echo "==> Installing on $target"
ssh -p "$port" -t "$target" "sed -i 's/\r\$//' ~/osb-pi-install.sh && sudo bash ~/osb-pi-install.sh install ~/osb-bundle.tar.gz $label; rc=\$?; rm -f ~/osb-bundle.tar.gz ~/osb-pi-install.sh; exit \$rc"

echo "==> Deployed $label. Browse to http://${target#*@}:5000 (or the port set in /etc/osb/osb.env)."
