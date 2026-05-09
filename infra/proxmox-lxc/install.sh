#!/usr/bin/env bash
set -euo pipefail

DEFAULT_HOSTNAME="homeautomation-dev"
DEFAULT_STORAGE="local-lvm"
DEFAULT_TEMPLATE_STORAGE="local"
DEFAULT_BRIDGE="vmbr0"
DEFAULT_DISK_GB=20
DEFAULT_MEMORY_MB=2048
DEFAULT_SWAP_MB=2048
DEFAULT_CORES=2
RAW_BASE_URL="https://raw.githubusercontent.com/dani-rodr/HomeAutomation/main/infra/proxmox-lxc"

msg() {
  printf '\n[%s] %s\n' "$(date +%H:%M:%S)" "$*"
}

fail() {
  printf '\nERROR: %s\n' "$*" >&2
  exit 1
}

require_root() {
  [[ ${EUID} -eq 0 ]] || fail "Run this script as root on the Proxmox host."
}

require_proxmox() {
  command -v pct >/dev/null 2>&1 || fail "pct is not available. Run this on a Proxmox host."
  command -v pveversion >/dev/null 2>&1 || fail "pveversion is not available. Run this on a Proxmox host."
  command -v pvesm >/dev/null 2>&1 || fail "pvesm is not available. Run this on a Proxmox host."
  command -v pveam >/dev/null 2>&1 || fail "pveam is not available. Run this on a Proxmox host."
}

pick_storage() {
  local target="$1"
  if pvesm status --enabled 1 --content rootdir 2>/dev/null | awk '{print $1}' | grep -Fxq "$target"; then
    printf '%s\n' "$target"
    return
  fi

  pvesm status --enabled 1 --content rootdir 2>/dev/null | awk 'NR>1 {print $1; exit}'
}

pick_template_volid() {
  local existing
  existing="$(pveam list "${DEFAULT_TEMPLATE_STORAGE}" 2>/dev/null | awk '/debian-13-standard/ {print $2; exit}')"
  if [[ -n "${existing}" ]]; then
    printf '%s:%s\n' "${DEFAULT_TEMPLATE_STORAGE}" "${existing}"
    return
  fi

  msg "Updating template index"
  pveam update >/dev/null

  local template_name
  template_name="$(pveam available --section system 2>/dev/null | awk '/debian-13-standard/ {print $2; exit}')"
  if [[ -z "${template_name}" ]]; then
    template_name="$(pveam available --section system 2>/dev/null | awk '/debian-12-standard/ {print $2; exit}')"
  fi
  [[ -n "${template_name}" ]] || fail "Could not find a Debian LXC template."

  msg "Downloading ${template_name} to ${DEFAULT_TEMPLATE_STORAGE}"
  pveam download "${DEFAULT_TEMPLATE_STORAGE}" "${template_name}" >/dev/null
  printf '%s:%s\n' "${DEFAULT_TEMPLATE_STORAGE}" "${template_name}"
}

fetch_companion() {
  local name="$1"
  local target="$2"
  local local_dir
  local_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

  if [[ -f "${local_dir}/${name}" ]]; then
    cp "${local_dir}/${name}" "${target}"
    return
  fi

  curl -fsSL "${RAW_BASE_URL}/${name}" -o "${target}"
}

main() {
  require_root
  require_proxmox

  local ctid storage template_volid tmpdir ip_line
  ctid="$(pvesh get /cluster/nextid)"
  storage="$(pick_storage "${DEFAULT_STORAGE}")"
  [[ -n "${storage}" ]] || fail "Could not find a storage that supports LXC root disks."
  template_volid="$(pick_template_volid)"

  msg "Reviewing default settings"
  printf '  CT ID:      %s\n' "${ctid}"
  printf '  Hostname:   %s\n' "${DEFAULT_HOSTNAME}"
  printf '  Template:   %s\n' "${template_volid}"
  printf '  Storage:    %s\n' "${storage}"
  printf '  Disk:       %s GB\n' "${DEFAULT_DISK_GB}"
  printf '  CPU cores:  %s\n' "${DEFAULT_CORES}"
  printf '  Memory:     %s MB\n' "${DEFAULT_MEMORY_MB}"
  printf '  Swap:       %s MB\n' "${DEFAULT_SWAP_MB}"
  printf '  Network:    DHCP on %s\n' "${DEFAULT_BRIDGE}"
  printf '  Unprivileged: yes\n'

  read -r -p "Continue with these defaults? [Y/n] " confirm
  confirm="${confirm:-Y}"
  [[ "${confirm}" =~ ^[Yy]$ ]] || fail "Aborted by user."

  msg "Creating LXC ${ctid}"
  local -a pct_create_args=(
    --arch amd64
    --cores "${DEFAULT_CORES}"
    --hostname "${DEFAULT_HOSTNAME}"
    --memory "${DEFAULT_MEMORY_MB}"
    --swap "${DEFAULT_SWAP_MB}"
    --net0 "name=eth0,bridge=${DEFAULT_BRIDGE},ip=dhcp"
    --onboot 1
    --ostype debian
    --rootfs "${storage}:${DEFAULT_DISK_GB}"
    --start 1
    --tags developer\;opencode\;homeautomation
    --unprivileged 1
    --features keyctl=1
  )

  if [[ -s /root/.ssh/authorized_keys ]]; then
    pct_create_args+=(--ssh-public-keys /root/.ssh/authorized_keys)
  fi

  pct create "${ctid}" "${template_volid}" "${pct_create_args[@]}"

  pct start "${ctid}" >/dev/null 2>&1 || true
  sleep 5

  tmpdir="$(mktemp -d)"
  trap 'rm -rf "${tmpdir}"' EXIT

  fetch_companion "setup-container.sh" "${tmpdir}/setup-container.sh"
  fetch_companion "homeautomation-init" "${tmpdir}/homeautomation-init"
  fetch_companion "homeautomation-opencode-start" "${tmpdir}/homeautomation-opencode-start"
  fetch_companion "homeautomation-opencode.service" "${tmpdir}/homeautomation-opencode.service"

  pct push "${ctid}" "${tmpdir}/setup-container.sh" /root/setup-container.sh -perms 755
  pct push "${ctid}" "${tmpdir}/homeautomation-init" /root/homeautomation-init -perms 755
  pct push "${ctid}" "${tmpdir}/homeautomation-opencode-start" /root/homeautomation-opencode-start -perms 755
  pct push "${ctid}" "${tmpdir}/homeautomation-opencode.service" /root/homeautomation-opencode.service -perms 644

  msg "Installing development environment inside the LXC"
  pct exec "${ctid}" -- env \
    REPO_URL="https://github.com/dani-rodr/HomeAutomation.git" \
    REPO_DIR="/opt/homeautomation/HomeAutomation" \
    bash /root/setup-container.sh

  msg "Running initial application setup inside the LXC"
  pct exec "${ctid}" -- /usr/local/bin/homeautomation-init

  ip_line="$(pct exec "${ctid}" -- hostname -I 2>/dev/null | awk '{print $1}')"
  msg "LXC ${ctid} is ready"
  printf '  Enter shell: pct enter %s\n' "${ctid}"
  printf '  Re-run setup: pct exec %s -- homeautomation-init\n' "${ctid}"
  if [[ -n "${ip_line}" ]]; then
    printf '  OpenCode Web: http://%s:4096\n' "${ip_line}"
  fi
}

main "$@"
