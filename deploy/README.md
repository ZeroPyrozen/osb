# Deploying to a Raspberry Pi

One command builds the site on your PC, copies it to the Pi over SSH and (re)starts it as a
systemd service. The Pi doesn't need .NET installed: the build is self-contained.

## What you need

- A Raspberry Pi with an ARMv7 or newer CPU (Pi 2, 3, 4, 5, Zero 2 W) running
  Raspberry Pi OS (or another Debian-based OS) with SSH enabled. The Pi 1 and the original
  Zero/Zero W (ARMv6) can't run .NET.
- On your PC: the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), and an SSH
  client and `tar`, both of which ship with Windows 10/11, macOS and Linux.
- A Pi user that can run `sudo`. The default Raspberry Pi OS user can.

Set up SSH key login once, so deploys don't ask for your password. In PowerShell:

```powershell
ssh-keygen -t ed25519   # skip if you already have ~/.ssh/id_ed25519
type $env:USERPROFILE\.ssh\id_ed25519.pub | ssh pi@raspberrypi.local "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"
```

(On Linux or macOS: `ssh-copy-id pi@raspberrypi.local`.)

## Deploy

From the repository root:

```powershell
.\deploy\deploy.ps1 -SshHost pi@raspberrypi.local
```

or, from bash (Linux, macOS, WSL, Git Bash):

```bash
./deploy/deploy.sh pi@raspberrypi.local
```

Use `-Runtime linux-arm` (bash: `./deploy/deploy.sh pi@raspberrypi.local linux-arm`) if the Pi
runs **32-bit** Raspberry Pi OS. If you pick the wrong one, the deploy stops and tells you which
to use. Pass `-Port` (bash: `SSH_PORT=...`) if SSH isn't on port 22.

The first deploy also sets the Pi up: it installs missing packages (ICU, curl), creates an
unprivileged `osb` user, installs and enables the `osb` systemd service, and creates
`/etc/osb/osb.env` for your settings. Fill in the secrets there (see the next section).

Every deploy:

1. publishes a self-contained build for the Pi and packs it into one archive;
2. uploads it with `scp` and runs `pi-install.sh` on the Pi with `sudo`;
3. unpacks it into a new folder under `/opt/osb/releases/`;
4. stops the service and backs up the database to `/var/backups/osb/<release>/`;
5. points `/opt/osb/current` at the new release and starts the service, which applies any
   new database migrations;
6. waits for `/healthz` to answer. If it doesn't, the previous release **and the database from
   before the deploy** are restored automatically, and the deploy fails, showing the service log;
7. keeps the last 3 releases (with their database backups) and deletes older ones.

Afterwards the site is at `http://raspberrypi.local:5000`.

## Settings and secrets

Secrets never go in `appsettings.json` (it's in a public repo). On the Pi, they live in
`/etc/osb/osb.env`, which only root can read:

```bash
sudo nano /etc/osb/osb.env      # e.g. API__ClientSecret=...
sudo systemctl restart osb
```

Use `__` where `appsettings.json` has nesting: `API:ClientSecret` becomes `API__ClientSecret`.
Deploys never overwrite this file. [`osb.env.example`](osb.env.example) lists the useful
settings.

Showcase submissions are reviewed by the osu! accounts in `Showcase__Reviewers`, a list of osu!
user IDs separated by commas (your ID is the number in your osu! profile's address). After a
restart, reviewers find "Review queue" in their account menu and Edit on every storyboard page;
they don't need to log in again.

osu! login sends players back to `API__RedirectURL`, which must match one of the
"Application Callback URLs" of your osu! OAuth application exactly. That field takes several
URLs separated by commas, so one application can serve production and testing. To test login on
your network, add `http://raspberrypi.local:5000/auth/authorized` there, set the same value as
`API__RedirectURL`, and always open the site at that address: the login cookies only work on
the host name that the callback uses.

For local development, use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
instead:

```powershell
dotnet user-secrets set "API:ClientID" "<your osu! OAuth application's client ID>"
dotnet user-secrets set "API:ClientSecret" "<your osu! OAuth client secret>"
```

## The database

The site keeps its data in one SQLite file, `/var/lib/osb/osb.db`: the showcase (storyboards,
storyboarders, tags), the storyboards members submit and their reviews, and the progress of
learners who log in with osu!. Nothing needs setting up. On its first start the app creates the database and fills in the showcase from the data
bundled with the build, and later releases update the schema (EF Core migrations) when they
start.

Deploys never replace the database. Before each one, `pi-install.sh` copies it to
`/var/backups/osb/<release>/` with the service stopped, so the copy is consistent. A deploy
that fails its health check puts that copy back along with the previous release, since the
failed release may already have migrated the database.

A **manual rollback keeps the current database**, so nothing done on the site since the deploy,
such as learners' progress or reviews, is lost. Migrations normally only add tables and columns, which older releases simply ignore. If a
release changed the database in a way the previous one can't handle, also put back the
database from before that release was deployed: add `-RestoreDb` (bash: `rollback
--restore-db`). Everything changed since that deploy is then lost, so the database as it was
just before the rollback is saved in `/var/backups/osb/before-rollback/` first.

To download a copy, take one of the backups (they're consistent, because the service was
stopped while they were made):

```bash
ssh -t pi@raspberrypi.local 'sudo ls /var/backups/osb'
ssh -t pi@raspberrypi.local 'sudo install -m 600 -o $USER /var/backups/osb/<release>/osb.db ~/osb-backup.db'
scp pi@raspberrypi.local:osb-backup.db .
```

## Day-to-day

| Task | Command |
| --- | --- |
| Roll back to the previous release | `.\deploy\deploy.ps1 -SshHost pi@raspberrypi.local -Rollback`, or on the Pi: `sudo /opt/osb/pi-install.sh rollback` |
| Roll back, database included | Add `-RestoreDb`, or on the Pi: `sudo /opt/osb/pi-install.sh rollback --restore-db` |
| Follow the logs | `ssh pi@raspberrypi.local journalctl -u osb -f` |
| Status | `ssh pi@raspberrypi.local systemctl status osb` |
| Restart after editing settings | `ssh pi@raspberrypi.local sudo systemctl restart osb` |
| See what's deployed | `ssh pi@raspberrypi.local readlink /opt/osb/current` |

## Making it public (HTTPS)

Kestrel listens on port 5000 over plain HTTP. To serve `storyboarder.xyz` over HTTPS, put one of
these in front of it on the Pi, then set `ASPNETCORE_URLS=http://127.0.0.1:5000` in
`/etc/osb/osb.env` so port 5000 is no longer reachable from outside:

- **nginx + Let's Encrypt**: see [`nginx-osb.conf.example`](nginx-osb.conf.example). This needs
  ports 80 and 443 forwarded from your router to the Pi.
- **Cloudflare Tunnel** (`cloudflared`) or **Caddy** work too, without nginx config. A tunnel also
  avoids opening router ports.

The app already trusts `X-Forwarded-For` and `X-Forwarded-Proto` from a proxy on the same machine,
so generated links, HTTPS redirection and HSTS work behind any of them. If the proxy runs on a
*different* machine, add its IP to `ForwardedHeadersOptions.KnownProxies` in `Program.cs`.

Remember to point the osu! OAuth application's callback URL (`API__RedirectURL`) at the public
address.

## What lives where on the Pi

| Path | Contents |
| --- | --- |
| `/opt/osb/releases/<timestamp>-<commit>/` | One folder per deploy, owned by root, read-only to the app |
| `/opt/osb/current` | Symlink to the release that's running |
| `/opt/osb/pi-install.sh` | Copy of the install script, for rollbacks |
| `/etc/osb/osb.env` | Settings and secrets (root only) |
| `/etc/systemd/system/osb.service` | The service definition, from [`osb.service`](osb.service). Replaced on every deploy, so use `sudo systemctl edit osb` for local changes |
| `/var/lib/osb/osb.db` | The SQLite database. Never touched by deploys |
| `/var/lib/osb/keys/` | ASP.NET Core data-protection keys (keep login cookies valid across restarts) |
| `/var/backups/osb/<release>/` | The database as it was just before each kept release was deployed |

## Troubleshooting

- **"this build is for linux-arm64, but this machine needs linux-arm"**: the Pi runs 32-bit
  Raspberry Pi OS. Deploy with `-Runtime linux-arm`.
- **`sudo` asks for a password**: type it when prompted (the script keeps the terminal
  interactive), or allow passwordless sudo for your user.
- **The service won't start**: `journalctl -u osb -n 100` shows why. Settings problems are
  usually in `/etc/osb/osb.env`.
- **The site works on the Pi but other devices time out**: a firewall on the Pi is blocking the
  port. With `ufw`, allow your local network, for example
  `sudo ufw allow from 192.168.1.0/24 to any port 5000 proto tcp`.
- **Low on memory** (a 512 MB Zero 2 W, or 1 GB boards): uncomment `DOTNET_gcServer=0` in
  `/etc/osb/osb.env`.
- **Port 80 without a proxy**: run `sudo systemctl edit osb` and add `AmbientCapabilities=CAP_NET_BIND_SERVICE`
  under `[Service]`, then set `ASPNETCORE_URLS=http://0.0.0.0:80`. A reverse proxy is the
  better option for a public site.
