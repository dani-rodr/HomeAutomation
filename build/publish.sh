#!/bin/bash
set -euo pipefail

# Script root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/../src/HomeAutomation"
PROJECT_PATH="$PROJECT_DIR/HomeAutomation.csproj"

APPSETTINGS="$PROJECT_DIR/appsettings.json"
APPSETTINGS_DEV="$PROJECT_DIR/appsettings.Development.json"

# Load settings from JSON
ip="192.168.0.134"
# ip=$(jq -r '.HomeAssistant.Host' "$APPSETTINGS")
port=$(jq -r '.HomeAssistant.Port' "$APPSETTINGS")
token=$(jq -r '.HomeAssistant.Token' "$APPSETTINGS_DEV")

# CHANGE ME
slug='c6a2317c_netdaemon6'
version="${slug##*_}"
addon_json="{\"addon\": \"$slug\"}"

ha_call() {
    local service=$1
    local payload=$2

    echo "Calling Home Assistant service: $service"

    response=$(curl -s -w "\n%{http_code}" -X POST \
    -H "Authorization: Bearer $token" \
    -H "Content-Type: application/json" \
    -d "$payload" \
    "http://$ip:$port/api/services/$service")

    # Split response and status code
    body=$(echo "$response" | sed '$d')
    status=$(echo "$response" | tail -n1)

    echo "HTTP $status"

}

# ha_call "light/toggle" "{\"entity_id\": \"light.sala_lights_group\"}"
dotnet publish -c Release "$PROJECT_PATH"
publish_dir=$(find "$PROJECT_DIR/bin/Release" -type d -path "*/net*/publish" | head -n 1)

# Rsync with change detection
echo "Deploying via rsync..."
CHANGES=$(rsync -az --delete --itemize-changes -e "ssh -o StrictHostKeyChecking=no" \
  "$publish_dir"/ root@"$ip":/config/"$version"/ | tee /dev/stderr)

if [ -n "$CHANGES" ]; then
  echo "Changes detected. Restarting addon..."

  ha_call "hassio/addon_restart" "$addon_json"
else
  echo "No changes detected. Skipping addon restart."
fi