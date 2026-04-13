#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────
# install.sh — Build and install the rwl CLI tool (.NET single-file binary)
#
# Usage:
#   bash install.sh [--prefix /usr/local]
#
# Requirements: .NET SDK 10.0+
# ──────────────────────────────────────────────────────────────────────

set -euo pipefail

PREFIX="${1:-}"
[[ "$PREFIX" == "--prefix" ]] && PREFIX="${2:-/usr/local}" || PREFIX="${PREFIX:-/usr/local}"

BOLD='\033[1m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
RED='\033[0;31m'
DIM='\033[2m'
RESET='\033[0m'

echo -e "${CYAN}${BOLD}Building rwl CLI...${RESET}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/src/Rwl/Rwl.csproj"

if [[ ! -f "$PROJECT" ]]; then
  echo -e "${RED}Error: src/Rwl/Rwl.csproj not found at $SCRIPT_DIR${RESET}"
  echo "Run this script from the ImInDanger repository root."
  exit 1
fi

# Check for dotnet SDK
if ! command -v dotnet &>/dev/null; then
  echo -e "${RED}Error: .NET SDK not found. Install from https://dot.net/download${RESET}"
  exit 1
fi

# Detect runtime identifier
RID="$(dotnet --info 2>/dev/null | grep 'RID:' | awk '{print $2}' || echo "")"
if [[ -z "$RID" ]]; then
  case "$(uname -s)-$(uname -m)" in
    Darwin-arm64) RID="osx-arm64" ;;
    Darwin-x86_64) RID="osx-x64" ;;
    Linux-x86_64) RID="linux-x64" ;;
    Linux-aarch64) RID="linux-arm64" ;;
    *) echo -e "${RED}Unable to detect platform. Set RID manually.${RESET}"; exit 1 ;;
  esac
fi

echo -e "  ${DIM}Platform: $RID${RESET}"

# Build single-file binary
cd "$SCRIPT_DIR/src/Rwl"
dotnet publish -c Release -r "$RID" -q

BINARY="bin/Release/net10.0/$RID/publish/rwl"
if [[ ! -f "$BINARY" ]]; then
  echo -e "${RED}Build failed — binary not found${RESET}"
  exit 1
fi

# Install
DEST="$PREFIX/bin/rwl"
echo -e "  Installing to ${BOLD}$DEST${RESET}"

if [[ -w "$PREFIX/bin" ]]; then
  cp "$BINARY" "$DEST"
  chmod +x "$DEST"
else
  echo -e "  ${DIM}(requires sudo)${RESET}"
  sudo cp "$BINARY" "$DEST"
  sudo chmod +x "$DEST"
fi

echo ""
echo -e "${GREEN}${BOLD}✅ rwl installed successfully!${RESET}"
echo ""
echo -e "  ${DIM}Set RWL_HOME for component resolution:${RESET}"
echo ""
echo -e "  ${CYAN}export RWL_HOME=\"$SCRIPT_DIR\"${RESET}"
echo ""
echo -e "  ${DIM}Add to your shell profile:${RESET}"
echo -e "  ${CYAN}echo 'export RWL_HOME=\"$SCRIPT_DIR\"' >> ~/.zshrc${RESET}"
echo ""
echo -e "  ${DIM}Then run:${RESET}"
echo -e "  ${BOLD}rwl --help${RESET}"
echo ""
