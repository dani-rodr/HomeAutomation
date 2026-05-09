#!/usr/bin/env bash
set -euo pipefail

REPO_URL="${REPO_URL:-https://github.com/dani-rodr/HomeAutomation.git}"
REPO_DIR="${REPO_DIR:-/opt/homeautomation/HomeAutomation}"
APP_USER="homeautomation"

msg() {
  printf '\n[%s] %s\n' "$(date +%H:%M:%S)" "$*"
}

install_dotnet() {
  if command -v dotnet >/dev/null 2>&1; then
    return
  fi

  msg "Installing .NET 10 SDK"
  curl -fsSL https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -o /tmp/packages-microsoft-prod.deb
  dpkg -i /tmp/packages-microsoft-prod.deb >/dev/null
  apt-get update
  apt-get install -y dotnet-sdk-10.0
}

install_node() {
  if command -v node >/dev/null 2>&1 && [[ "$(node --version 2>/dev/null || true)" == v24* ]]; then
    return
  fi

  msg "Installing Node.js 24"
  curl -fsSL https://deb.nodesource.com/setup_24.x | bash -
  apt-get install -y nodejs
}

install_uv() {
  if command -v uv >/dev/null 2>&1; then
    return
  fi

  msg "Installing uv"
  curl -LsSf https://astral.sh/uv/install.sh | env UV_INSTALL_DIR=/usr/local/bin sh
}

ensure_user() {
  if ! id -u "${APP_USER}" >/dev/null 2>&1; then
    msg "Creating ${APP_USER} user"
    useradd --create-home --shell /bin/bash "${APP_USER}"
  fi

  usermod -aG sudo "${APP_USER}"
}

clone_repo() {
  mkdir -p "$(dirname "${REPO_DIR}")"
  if [[ ! -e "${REPO_DIR}" ]]; then
    msg "Cloning repository"
    git clone "${REPO_URL}" "${REPO_DIR}"
  elif [[ ! -d "${REPO_DIR}/.git" ]]; then
    printf 'Refusing to continue: %s exists but is not a git checkout.\n' "${REPO_DIR}" >&2
    exit 1
  fi

  chown -R "${APP_USER}:${APP_USER}" "$(dirname "${REPO_DIR}")"
}

restore_dotnet_tools() {
  if [[ -f "${REPO_DIR}/.config/dotnet-tools.json" ]]; then
    msg "Restoring local .NET tools"
    runuser -u "${APP_USER}" -- env HOME="/home/${APP_USER}" dotnet tool restore --tool-manifest "${REPO_DIR}/.config/dotnet-tools.json"
  fi
}

install_opencode_bits() {
  msg "Installing OpenCode and plugins"
  npm install -g \
    opencode-ai@1.14.41 \
    opencode-codex-usage \
    @tarquinen/opencode-dcp@latest \
    @rama_nigg/open-cursor@latest
}

configure_console_autologin() {
  rm -rf /etc/systemd/system/getty@tty1.service.d

  mkdir -p /etc/systemd/system/console-getty.service.d
  cat > /etc/systemd/system/console-getty.service.d/autologin.conf <<'EOF'
[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin root --noclear --keep-baud console 115200,38400,9600 $TERM
EOF

  mkdir -p /etc/systemd/system/container-getty@1.service.d
  cat > /etc/systemd/system/container-getty@1.service.d/autologin.conf <<'EOF'
[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin root --noclear /dev/tty1 linux
EOF
}

install_files() {
  install -m 0755 /root/homeautomation-init /usr/local/bin/homeautomation-init
  install -m 0755 /root/homeautomation-opencode-start /usr/local/bin/homeautomation-opencode-start
  install -m 0644 /root/homeautomation-opencode.service /etc/systemd/system/homeautomation-opencode.service
  mkdir -p "/home/${APP_USER}/.config/opencode" "/home/${APP_USER}/.local/share/opencode"
  chown -R "${APP_USER}:${APP_USER}" "/home/${APP_USER}"
  configure_console_autologin
  systemctl daemon-reload
  systemctl reset-failed console-getty.service container-getty@1.service container-getty@2.service >/dev/null 2>&1 || true
  systemctl restart console-getty.service container-getty@1.service >/dev/null 2>&1 || true
}

main() {
  export DEBIAN_FRONTEND=noninteractive

  msg "Updating apt metadata"
  apt-get update
  apt-get install -y \
    apt-transport-https \
    build-essential \
    ca-certificates \
    curl \
    git \
    gh \
    gnupg \
    jq \
    lsb-release \
    openssh-client \
    rsync \
    sudo \
    unzip \
    xdg-utils \
    zip

  install_dotnet
  install_node
  install_uv
  ensure_user
  clone_repo
  install_opencode_bits
  restore_dotnet_tools
  install_files

  msg "Base installation complete"
}

main "$@"
