#!/usr/bin/env bash
# Installs an osb release on a Raspberry Pi (or any Debian-based machine with systemd).
# deploy.ps1 / deploy.sh run this over SSH; a copy is kept on the Pi for rollbacks:
#
#   sudo bash pi-install.sh install <osb-bundle.tar.gz> [label]
#   sudo /opt/osb/pi-install.sh rollback [--restore-db]
#
# The first install provisions the Pi: missing packages, an unprivileged "osb" user, the
# systemd service and /etc/osb/osb.env. Every install backs up the SQLite database, unpacks
# into a new folder under /opt/osb/releases, points /opt/osb/current at it, restarts the
# service and waits for /healthz. If the new release doesn't come up, the previous release
# and the database from before the deploy are put back.
set -euo pipefail
export LC_ALL=C

APP=osb
APP_ROOT=/opt/$APP
RELEASES=$APP_ROOT/releases
CURRENT=$APP_ROOT/current
ENV_FILE=/etc/$APP/$APP.env
UNIT_FILE=/etc/systemd/system/$APP.service
DEFAULT_DB=/var/lib/$APP/$APP.db   # ConnectionStrings__Osb in osb.service
DB_FILE=$DEFAULT_DB                # replaced by db_file once the env file exists
BACKUPS=/var/backups/$APP          # one database copy per release, named like the release
KEEP_RELEASES=${KEEP_RELEASES:-3}

stage=
new_env_file=

log()  { echo "==> $*"; }
warn() { echo "warning: $*" >&2; }
die()  { echo "error: $*" >&2; exit 1; }

# Runtime identifier (as in "dotnet publish -r") this machine can execute.
machine_rid() {
    if [[ $(uname -m) == armv6* ]]; then
        echo "an ARMv6 CPU (.NET needs ARMv7 or newer: Pi 2, 3, 4, 5 or Zero 2 W)"
        return
    fi
    # dpkg reports the userland, which is what matters: 32-bit Raspberry Pi OS on a Pi 4/5
    # runs a 64-bit kernel, so uname -m alone would wrongly say aarch64.
    case $(dpkg --print-architecture 2>/dev/null || uname -m) in
        arm64 | aarch64) echo linux-arm64 ;;
        armhf | armv7l)  echo linux-arm ;;
        amd64 | x86_64)  echo linux-x64 ;;
        *)               echo "an unsupported architecture ($(uname -m))" ;;
    esac
}

# Runtime identifier a published app was built for, read from the ELF header (e_machine).
binary_rid() {
    case $(od -An -t u2 -j 18 -N 2 "$1" | tr -d ' ') in
        183) echo linux-arm64 ;;
        40)  echo linux-arm ;;
        62)  echo linux-x64 ;;
        *)   echo unknown ;;
    esac
}

# Idempotent: safe to run on every deploy. $1 = folder with the deploy/ files from the bundle.
provision() {
    local src=$1 need_icu=0 icu pkgs=()

    # The self-contained build brings its own .NET runtime, but the OS still has to provide
    # ICU (globalization), CA certificates (HTTPS calls to osu!) and curl (health check).
    ldconfig -p | grep 'libicuuc\.so' >/dev/null || need_icu=1
    command -v curl >/dev/null || pkgs+=(curl)
    [[ -e /etc/ssl/certs/ca-certificates.crt ]] || pkgs+=(ca-certificates)
    if ((need_icu)) || ((${#pkgs[@]})); then
        log "Installing missing packages"
        apt-get update -qq
        if ((need_icu)); then
            icu=$(apt-cache search --names-only '^libicu[0-9]+$' | awk '{print $1}' | sort -V | tail -n1)
            [[ -n $icu ]] || die "couldn't find an ICU package (libicuNN) to install"
            pkgs+=("$icu")
        fi
        DEBIAN_FRONTEND=noninteractive apt-get install -y -qq --no-install-recommends "${pkgs[@]}"
    fi

    if ! id -u "$APP" >/dev/null 2>&1; then
        log "Creating system user '$APP'"
        useradd --system --user-group --home-dir "/var/lib/$APP" --no-create-home \
            --shell /usr/sbin/nologin "$APP"
    fi
    install -d -m 755 "$APP_ROOT" "$RELEASES"
    install -d -m 750 "/etc/$APP"

    if [[ ! -e $ENV_FILE ]]; then
        log "Creating $ENV_FILE"
        install -m 600 "$src/osb.env.example" "$ENV_FILE"
        new_env_file=1
    fi

    if ! cmp -s "$src/osb.service" "$UNIT_FILE"; then
        log "Installing $UNIT_FILE"
        install -m 644 "$src/osb.service" "$UNIT_FILE"
        systemctl daemon-reload
    fi
    systemctl enable --quiet "$APP"

    install -m 755 "$src/pi-install.sh" "$APP_ROOT/pi-install.sh"
}

switch_to() {
    ln -sfn "$1" "$CURRENT.new"
    mv -Tf "$CURRENT.new" "$CURRENT"   # rename is atomic, so "current" is never missing
}

# Builds the health-check URL from the first ASPNETCORE_URLS entry in the env file.
health_url() {
    local urls first scheme port
    urls=$(sed -n 's/^[[:space:]]*ASPNETCORE_URLS[[:space:]]*=//p' "$ENV_FILE" 2>/dev/null |
        tail -n1 | tr -d "\"' \r") || true
    first=${urls%%;*}
    first=${first:-http://localhost:5000}
    scheme=${first%%://*}
    port=${first##*:}
    port=${port%%/*}
    if ! [[ $port =~ ^[0-9]+$ ]]; then
        port=80
        if [[ $scheme == https ]]; then port=443; fi
    fi
    echo "$scheme://127.0.0.1:$port/healthz"
}

# Restarts the service and waits for /healthz to answer.
restart_and_check() {
    local url
    url=$(health_url)
    systemctl restart "$APP" || return 1
    for _ in $(seq 1 30); do
        if curl -fsSk -o /dev/null --max-time 5 "$url" 2>/dev/null; then
            log "Health check OK: $url"
            return 0
        fi
        systemctl is-active --quiet "$APP" || break
        sleep 1
    done
    warn "health check failed: $url"
    return 1
}

# The database file: the Data Source of ConnectionStrings__Osb in the env file when that's an
# absolute path, otherwise the default from osb.service.
db_file() {
    local value
    value=$(sed -n 's/^[[:space:]]*ConnectionStrings__Osb[[:space:]]*=//p' "$ENV_FILE" 2>/dev/null |
        tail -n1 | tr -d "\"'\r") || true
    value=$(printf '%s\n' "$value" |
        sed -n 's/.*\(data \?source\|filename\)[[:space:]]*=[[:space:]]*\([^;]*\).*/\2/Ip')
    value=${value%"${value##*[![:space:]]}"}   # trim trailing spaces
    if [[ $value == /* ]]; then echo "$value"; else echo "$DEFAULT_DB"; fi
}

# Copies the SQLite database (with its -wal and -shm files, if any) into folder $1. Only call
# it while the service is stopped, so the copy can't catch a half-finished write.
backup_db() {
    local dest=$1 file
    [[ -f $DB_FILE ]] || return 0
    install -d -m 700 "$dest"
    for file in "$DB_FILE" "$DB_FILE-wal" "$DB_FILE-shm"; do
        if [[ -f $file ]]; then cp -p -- "$file" "$dest/"; fi
    done
}

# Puts a backup_db copy from folder $1 back in place. Only call it while the service is stopped.
restore_db() {
    local src=$1 file
    [[ -f $src/${DB_FILE##*/} ]] || return 1
    rm -f -- "$DB_FILE-wal" "$DB_FILE-shm"
    for file in "$src"/*; do cp -p -- "$file" "${DB_FILE%/*}/"; done
}

# Deletes all but the newest $KEEP_RELEASES releases (names start with a UTC timestamp), and
# the database backups of releases that are gone.
prune() {
    local current old all
    current=$(readlink -f "$CURRENT")
    shopt -s nullglob
    all=("$RELEASES"/*/)
    for old in "${all[@]:0:$((${#all[@]} > KEEP_RELEASES ? ${#all[@]} - KEEP_RELEASES : 0))}"; do
        old=${old%/}
        if [[ $old != "$current" ]]; then rm -rf -- "$old"; fi
    done
    for old in "$BACKUPS"/[0-9]*/; do
        old=${old%/}
        if [[ ! -d $RELEASES/${old##*/} ]]; then rm -rf -- "$old"; fi
    done
    shopt -u nullglob
}

cmd_install() {
    local bundle=${1:?usage: pi-install.sh install <osb-bundle.tar.gz> [label]}
    local label=${2:-manual}
    local release previous= want have backup
    label=${label//[^A-Za-z0-9._-]/}
    [[ -f $bundle ]] || die "bundle not found: $bundle"
    release=$RELEASES/$(date -u +%Y%m%d-%H%M%S)-$label
    backup=$BACKUPS/${release##*/}

    install -d -m 755 "$RELEASES"
    stage=$(mktemp -d "$RELEASES/.incoming.XXXXXX")
    log "Unpacking $(basename "$release")"
    tar -xzf "$bundle" -C "$stage" --no-same-owner --no-same-permissions
    if [[ ! -f $stage/app/$APP || ! -f $stage/deploy/osb.service ]]; then
        die "$bundle isn't an osb bundle (expected app/$APP and deploy/osb.service)"
    fi
    sed -i 's/\r$//' "$stage"/deploy/*   # tolerate CRLF from a Windows checkout

    want=$(machine_rid)
    [[ $want == linux-* ]] || die "this machine has $want"
    have=$(binary_rid "$stage/app/$APP")
    if [[ $have != "$want" ]]; then
        die "this build is for $have, but this machine needs $want. Deploy again with -Runtime $want (deploy.sh: pass it as the second argument)."
    fi

    provision "$stage/deploy"
    DB_FILE=$(db_file)

    chmod -R u=rwX,go=rX "$stage/app"
    chmod 755 "$stage/app/$APP"
    mv "$stage/app" "$release"

    if [[ -L $CURRENT ]]; then previous=$(readlink -f "$CURRENT"); fi
    # The new release migrates the database when it starts, so keep a copy to put back if it
    # fails. Stopping first keeps the copy consistent; the restart below starts it again.
    if [[ -f $DB_FILE ]]; then
        log "Backing up the database to $backup"
        systemctl stop "$APP" || true
        backup_db "$backup"
    fi
    switch_to "$release"
    if ! restart_and_check; then
        journalctl -u "$APP" -n 30 --no-pager || true
        if [[ -n $previous && -d $previous ]]; then
            log "Rolling back to $(basename "$previous")"
            systemctl stop "$APP" || true
            if [[ -d $backup ]]; then
                log "Restoring the database from before this deploy"
                restore_db "$backup" || warn "couldn't restore the database from $backup"
            fi
            switch_to "$previous"
            restart_and_check || warn "the previous release isn't healthy either"
            rm -rf -- "$release" "$backup"
        fi
        die "deploy failed (full log: journalctl -u $APP -n 200)"
    fi

    prune
    log "Now running $(basename "$release")"
    if [[ -n $new_env_file ]]; then
        echo
        echo "First install: put your settings and secrets (e.g. API__ClientSecret) in"
        echo "$ENV_FILE, then apply them with: sudo systemctl restart $APP"
    fi
}

# Switches to the previous release. The database is kept as it is (the app's migrations only
# add things, so older releases keep working with it) unless --restore-db is given: that puts
# back the copy taken just before the current release was deployed, losing later changes.
cmd_rollback() {
    local restore=0 current target= dir backup
    case ${1:-} in
        '') ;;
        --restore-db) restore=1 ;;
        *) die "unknown option '$1' (usage: pi-install.sh rollback [--restore-db])" ;;
    esac
    [[ -L $CURRENT ]] || die "nothing is installed yet"
    current=$(readlink -f "$CURRENT")
    shopt -s nullglob
    for dir in "$RELEASES"/*/; do
        dir=${dir%/}
        if [[ $dir < $current ]]; then target=$dir; fi   # newest release older than current
    done
    shopt -u nullglob
    [[ -n $target ]] || die "no older release to roll back to"
    DB_FILE=$(db_file)
    backup=$BACKUPS/${current##*/}
    if ((restore)) && [[ ! -f $backup/${DB_FILE##*/} ]]; then
        die "there's no database backup from before $(basename "$current") was deployed"
    fi

    log "Rolling back from $(basename "$current") to $(basename "$target")"
    if ((restore)); then
        systemctl stop "$APP" || true
        rm -rf -- "$BACKUPS/before-rollback"
        backup_db "$BACKUPS/before-rollback"
        log "Restoring the database from before $(basename "$current") was deployed"
        restore_db "$backup" || die "couldn't restore the database from $backup"
        log "The database as it was a moment ago is saved in $BACKUPS/before-rollback"
    fi
    switch_to "$target"
    restart_and_check || die "rolled back, but the service isn't healthy (see: journalctl -u $APP -n 50)"
    log "Now running $(basename "$target")"
}

[[ $EUID -eq 0 ]] || die "run this as root (sudo)"
exec 9>"/run/lock/$APP-deploy.lock"
flock -n 9 || die "another deploy is already running"
trap '[[ -n $stage ]] && rm -rf -- "$stage"' EXIT

case ${1:-} in
    install)  shift; cmd_install "$@" ;;
    rollback) shift; cmd_rollback "$@" ;;
    *)        echo "usage: $0 install <osb-bundle.tar.gz> [label] | rollback [--restore-db]" >&2; exit 2 ;;
esac
