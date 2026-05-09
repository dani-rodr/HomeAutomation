# Proxmox LXC Installer

This directory contains a Proxmox-host installer for a dedicated LXC that runs OpenCode Web for working on the `HomeAutomation` repository.

## What It Creates

- A Debian 13 LXC named `homeautomation-dev`
- Conservative defaults for limited hardware:
  - 2 CPU cores
  - 2048 MB RAM
  - 2048 MB swap
  - 20 GB disk on `local-lvm`
- A `homeautomation` user inside the LXC
- A clone of `https://github.com/dani-rodr/HomeAutomation.git`
- OpenCode Web bound to `0.0.0.0:4096`
- Local .NET tools restored from `.config/dotnet-tools.json`, including `nd-codegen`

## Install From the Proxmox Host

```bash
bash -c "$(curl -fsSL https://raw.githubusercontent.com/dani-rodr/HomeAutomation/master/infra/proxmox-lxc/install.sh)"
```

The installer creates the LXC, installs the required toolchain, then drops into the initial application setup.

## First-Run Setup

The first-run wizard asks for:

- OpenCode username and password
- Root console password for Proxmox web console logins
- Git user name and email
- Home Assistant MCP URL
- Optional provider API keys
- Optional GitHub token for `gh auth`

After setup, the wizard enables and starts the systemd service.

## Access

Open from your phone or browser:

```text
http://<lxc-ip>:4096
```

For the Proxmox Console button, log in as `root` with the root console password you set during the first-run wizard.

## Useful Commands

From the Proxmox host:

```bash
pct enter <ctid>
pct exec <ctid> -- homeautomation-init
pct exec <ctid> -- systemctl status homeautomation-opencode --no-pager
```

Inside the LXC:

```bash
sudo -u homeautomation -H bash
cd /opt/homeautomation/HomeAutomation
dotnet build
dotnet test
nd-codegen
```
