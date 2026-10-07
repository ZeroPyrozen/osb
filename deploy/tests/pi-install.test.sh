#!/usr/bin/env bash
# Tests for deploy/pi-install.sh, without root, systemd or a Pi. The script's functions run against
# a temporary folder, with stand-ins for systemctl, curl, journalctl and the first-time setup
# (packages and the osb user).
#
#   bash deploy/tests/pi-install.test.sh
#
# CI runs this on Linux. It also runs in Git Bash on Windows, except the install and rollback runs,
# which need real symbolic links.

here=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# Everything above the root check is definitions; leave out the part that runs a command.
sed '/^\[\[ \$EUID -eq 0 \]\]/,$d' "$here/../pi-install.sh" > "$work/functions.sh"
# shellcheck source=/dev/null
source "$work/functions.sh"   # also turns on set -euo pipefail, like the real script

fails=0
check() {   # check <what> <expected> <actual>
    if [[ $2 == "$3" ]]; then
        echo "PASS  $1"
    else
        echo "FAIL  $1: expected <$2>, got <$3>"
        fails=$((fails + 1))
    fi
}
exists() { if [[ -e $1 ]]; then echo yes; else echo no; fi; }

if [[ $(uname -s) == MINGW* || $(uname -s) == MSYS* ]]; then
    # Git Bash can't set permission bits in its temp folder, and they aren't what's tested here.
    install() { if [[ $1 == -d ]]; then shift; [[ $1 == -m ]] && shift 2; mkdir -p "$@"; else command install "$@"; fi; }
fi

ENV_FILE=$work/osb.env

# --- health_url: the health check address, from ASPNETCORE_URLS ------------------
t() { printf '%b' "$2" > "$ENV_FILE"; check "health_url: $1" "$3" "$(health_url)"; }
t "the env file template"   'ASPNETCORE_URLS=http://0.0.0.0:5000\n'                          http://127.0.0.1:5000/healthz
t "the Pi's port"           'ASPNETCORE_URLS=http://0.0.0.0:44335\n'                         http://127.0.0.1:44335/healthz
t "quoted, several urls"    'ASPNETCORE_URLS="http://127.0.0.1:8080;https://0.0.0.0:8443"\n' http://127.0.0.1:8080/healthz
t "https wildcard"          'ASPNETCORE_URLS=https://+:5001\n'                               https://127.0.0.1:5001/healthz
t "no port, http"           'ASPNETCORE_URLS=http://localhost\n'                             http://127.0.0.1:80/healthz
t "no port, https"          "ASPNETCORE_URLS='https://example.org'\n"                        https://127.0.0.1:443/healthz
t "ipv6 any"                'ASPNETCORE_URLS=http://[::]:5000\n'                             http://127.0.0.1:5000/healthz
t "CRLF line ending"        'ASPNETCORE_URLS=http://0.0.0.0:5050\r\n'                        http://127.0.0.1:5050/healthz
t "commented out"           '#ASPNETCORE_URLS=http://0.0.0.0:9999\n'                         http://127.0.0.1:5000/healthz
t "the last one wins"       'ASPNETCORE_URLS=http://0.0.0.0:1111\nASPNETCORE_URLS=http://0.0.0.0:2222\n' http://127.0.0.1:2222/healthz
t "spaces around ="         '  ASPNETCORE_URLS = http://0.0.0.0:5000\n'                      http://127.0.0.1:5000/healthz
rm -f "$ENV_FILE"
check "health_url: no env file (must not abort)" http://127.0.0.1:5000/healthz "$(health_url)"

# --- db_file: the database path, from ConnectionStrings__Osb ----------------------
DEFAULT_DB=/var/lib/osb/osb.db
d() { printf '%b' "$2" > "$ENV_FILE"; check "db_file: $1" "$3" "$(db_file)"; }
d "not set"             '# nothing here\n'                                                  /var/lib/osb/osb.db
d "commented out"       '#ConnectionStrings__Osb="Data Source=/x/y.db"\n'                  /var/lib/osb/osb.db
d "quoted absolute"     'ConnectionStrings__Osb="Data Source=/mnt/usb/osb.db"\n'           /mnt/usb/osb.db
d "more options"        'ConnectionStrings__Osb="Data Source=/srv/osb.db;Cache=Shared"\n'  /srv/osb.db
d "Filename, spaces"    "ConnectionStrings__Osb='Filename=/srv/a b/osb.db'\n"               "/srv/a b/osb.db"
d "DataSource, lower"   'ConnectionStrings__Osb=datasource=/srv/x.db\n'                    /srv/x.db
d "relative path"       'ConnectionStrings__Osb="Data Source=App_Data/osb.db"\n'           /var/lib/osb/osb.db
d "CRLF and spaces"     'ConnectionStrings__Osb = "Data Source = /srv/osb.db "\r\n'        /srv/osb.db
rm -f "$ENV_FILE"
check "db_file: no env file (must not abort)" /var/lib/osb/osb.db "$(db_file)"

# --- machine and build architectures ----------------------------------------------
# A Linux executable is recognised by its ELF header: e_machine, at byte 18.
elf() { printf '\x7fELF\x02\x01\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x02\x00'"$2" > "$1"; }
elf "$work/arm64" '\xb7\x00'; elf "$work/arm" '\x28\x00'; elf "$work/x64" '\x3e\x00'
check "binary_rid: 64-bit ARM build" linux-arm64 "$(binary_rid "$work/arm64")"
check "binary_rid: 32-bit ARM build" linux-arm   "$(binary_rid "$work/arm")"
check "binary_rid: x64 build"        linux-x64   "$(binary_rid "$work/x64")"
check "binary_rid: not an executable" unknown    "$(binary_rid "$here/../osb.service")"
rid=$(machine_rid)
check "machine_rid: names a Linux runtime" yes "$([[ $rid == linux-* ]] && echo yes || echo "no ($rid)")"

# --- release labels can't escape the releases folder ------------------------------
label='76cb5ec-dirty; rm -rf /'; label=${label//[^A-Za-z0-9._-]/}
check "labels are sanitised" 76cb5ec-dirtyrm-rf "$label"

# --- backup_db and restore_db -----------------------------------------------------
mkdir -p "$work/data"
DB_FILE=$work/data/osb.db
backup_db "$work/backups/none"
check "backup_db: no database, no backup" no "$(exists "$work/backups/none")"
echo v1 > "$DB_FILE"; echo wal1 > "$DB_FILE-wal"
backup_db "$work/backups/r1"
check "backup_db: copies the database and its wal" "osb.db osb.db-wal" "$(cd "$work/backups/r1" && echo *)"
echo v2 > "$DB_FILE"; rm "$DB_FILE-wal"; echo shm2 > "$DB_FILE-shm"
restore_db "$work/backups/r1"
check "restore_db: the database is back"  v1   "$(cat "$DB_FILE")"
check "restore_db: its wal is back"       wal1 "$(cat "$DB_FILE-wal")"
check "restore_db: a stale shm is removed" no  "$(exists "$DB_FILE-shm")"
rc=0; restore_db "$work/backups/missing" || rc=$?
check "restore_db: fails without a backup" 1 "$rc"

# --- prune: keeps the newest KEEP_RELEASES, and never the current one ----------------
RELEASES=$work/prune/releases
BACKUPS=$work/prune/backups
mkdir -p "$RELEASES"/{20261001-100000-a,20261002-100000-b,20261003-100000-c,20261004-100000-d,20261005-100000-e,.incoming.x}
mkdir -p "$BACKUPS"/{20261001-100000-a,20261003-100000-c,20261005-100000-e,before-rollback}
CURRENT=$RELEASES/20261005-100000-e
KEEP_RELEASES=3 prune
check "prune: keeps the newest 3" "20261003-100000-c 20261004-100000-d 20261005-100000-e" "$(cd "$RELEASES" && echo */ | tr -d /)"
check "prune: ignores unpacking folders" yes "$(exists "$RELEASES/.incoming.x")"
check "prune: drops backups of pruned releases, keeps before-rollback" "20261003-100000-c 20261005-100000-e before-rollback" "$(cd "$BACKUPS" && echo */ | tr -d /)"
mkdir -p "$RELEASES"/{20261006-100000-f,20261007-100000-g}
CURRENT=$RELEASES/20261003-100000-c   # after a rollback, current is older than the newest release
KEEP_RELEASES=2 prune
check "prune: spares an older current release" "20261003-100000-c 20261006-100000-f 20261007-100000-g" "$(cd "$RELEASES" && echo */ | tr -d /)"

out=$( (cmd_rollback --bogus) 2>&1 ) || true
check "rollback: rejects unknown options" yes "$([[ $out == *"unknown option '--bogus'"* ]] && echo yes || echo no)"

# --- install and rollback, end to end ------------------------------------------------
touch "$work/link-target"
ln -s "$work/link-target" "$work/link-probe" 2>/dev/null || true
if [[ ! -L $work/link-probe ]]; then
    echo "SKIP  install and rollback runs: this system can't make symbolic links"
else
    site=$work/pi
    APP_ROOT=$site/opt/osb RELEASES=$site/opt/osb/releases CURRENT=$site/opt/osb/current
    ENV_FILE=$site/etc/osb.env BACKUPS=$site/backups DEFAULT_DB=$site/var/lib/osb/osb.db KEEP_RELEASES=3
    mkdir -p "$site/etc" "$site/var/lib/osb"
    echo 'ASPNETCORE_URLS=http://0.0.0.0:44335' > "$ENV_FILE"

    # Stand-ins. A release with a .broken file fails its health check, and half-migrates the
    # database when it starts, like a release whose migration crashes the app.
    provision() { :; }
    machine_rid() { echo linux-x64; }
    journalctl() { :; }
    sleep() { :; }
    # Release names start with the time; one second apart per deploy keeps them in order. The
    # count lives in a file because the script calls date in a subshell.
    date() {
        local n=$(( $(cat "$site/clock" 2>/dev/null || echo 0) + 1 ))
        echo "$n" > "$site/clock"
        printf '20261007-1000%02d\n' "$n"
    }
    systemctl() {
        echo "$*" >> "$site/systemctl.log"
        if [[ $1 == restart && -e $CURRENT/.broken ]]; then echo half-migrated >> "$DB_FILE"; fi
        return 0
    }
    curl() { [[ ! -e $CURRENT/.broken ]]; }

    bundle() {   # bundle <name> [arm64|broken|empty]
        local dir=$work/bundles/$1
        mkdir -p "$dir/app" "$dir/deploy"
        cp "$here/../osb.service" "$here/../osb.env.example" "$here/../pi-install.sh" "$dir/deploy/"
        case ${2:-} in
            arm64) elf "$dir/app/osb" '\xb7\x00' ;;
            empty) rm -rf "$dir/app" "$dir/deploy"; mkdir -p "$dir/app" ;;
            *) elf "$dir/app/osb" '\x3e\x00' ;;
        esac
        if [[ ${2:-} == broken ]]; then touch "$dir/app/.broken"; fi
        tar -czf "$work/bundles/$1.tar.gz" -C "$dir" .
        echo "$work/bundles/$1.tar.gz"
    }
    install_bundle() { (cmd_install "$@") 2>&1; }   # a subshell, so a failed deploy's exit stays in it
    running() { basename "$(readlink -f "$CURRENT")"; }
    db() { cat "$site/var/lib/osb/osb.db" 2>/dev/null; }
    releases() { (cd "$RELEASES" && echo */ | tr -d /); }

    install_bundle "$(bundle one)" one > /dev/null || true
    check "install: the first release is running" 20261007-100001-one "$(running)"
    check "install: no database yet, so no backup" no "$(exists "$BACKUPS/20261007-100001-one")"

    echo "learners' progress" > "$site/var/lib/osb/osb.db"
    install_bundle "$(bundle two)" two > /dev/null || true
    check "install: the second release is running" 20261007-100002-two "$(running)"
    check "install: the database was backed up first" "learners' progress" "$(cat "$BACKUPS/20261007-100002-two/osb.db")"

    rc=0; out=$(install_bundle "$(bundle three broken)" three) || rc=$?
    check "failed deploy: reports the failure" yes "$([[ $rc != 0 && $out == *"deploy failed"* ]] && echo yes || echo no)"
    check "failed deploy: the previous release runs again" 20261007-100002-two "$(running)"
    check "failed deploy: the database from before is back" "learners' progress" "$(db)"
    check "failed deploy: its release and backup are gone" "no no" "$(exists "$RELEASES/20261007-100003-three") $(exists "$BACKUPS/20261007-100003-three")"

    rc=0; out=$(install_bundle "$(bundle four arm64)" four) || rc=$?
    check "wrong architecture: refused" yes "$([[ $rc != 0 && $out == *"this build is for linux-arm64, but this machine needs linux-x64"* ]] && echo yes || echo no)"
    rc=0; out=$(install_bundle "$(bundle five empty)" five) || rc=$?
    check "not a bundle: refused" yes "$([[ $rc != 0 && $out == *"isn't an osb bundle"* ]] && echo yes || echo no)"
    check "refused deploys change nothing" "20261007-100002-two" "$(running)"

    install_bundle "$(bundle six)" six > /dev/null || true
    install_bundle "$(bundle seven)" seven > /dev/null || true
    check "install: keeps the newest three releases" "20261007-100002-two 20261007-100006-six 20261007-100007-seven" "$(releases)"

    echo "progress made with seven" > "$site/var/lib/osb/osb.db"
    (cmd_rollback) > /dev/null 2>&1 || true
    check "rollback: back to the previous release" 20261007-100006-six "$(running)"
    check "rollback: keeps the current database" "progress made with seven" "$(db)"

    install_bundle "$(bundle eight)" eight > /dev/null || true
    echo "progress made with eight" > "$site/var/lib/osb/osb.db"
    (cmd_rollback --restore-db) > /dev/null 2>&1 || true
    check "rollback --restore-db: back to the previous release" 20261007-100007-seven "$(running)"
    check "rollback --restore-db: the database from before eight" "progress made with seven" "$(db)"
    check "rollback --restore-db: keeps what it replaced" "progress made with eight" "$(cat "$BACKUPS/before-rollback/osb.db")"

    rm -rf "$BACKUPS/20261007-100007-seven"
    rc=0; out=$( (cmd_rollback --restore-db) 2>&1 ) || rc=$?
    check "rollback --restore-db: refused without a backup" yes "$([[ $rc != 0 && $out == *"no database backup from before"* ]] && echo yes || echo no)"
fi

echo
echo "$fails failure(s)"
exit "$fails"
