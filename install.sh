#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────
# install.sh — Install the rwl CLI tool
#
# Usage:
#   curl -fsSL https://raw.githubusercontent.com/.../install.sh | bash
#   # or
#   bash install.sh [--prefix /usr/local]
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

echo -e "${CYAN}${BOLD}Installing rwl CLI...${RESET}"

# Find source directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC="$SCRIPT_DIR/bin/rwl"

if [[ ! -f "$SRC" ]]; then
  echo -e "${RED}Error: bin/rwl not found at $SCRIPT_DIR${RESET}"
  echo "Run this script from the ImInDanger repository root."
  exit 1
fi

# Install binary
DEST="$PREFIX/bin/rwl"
echo -e "  Installing to ${BOLD}$DEST${RESET}"

if [[ -w "$PREFIX/bin" ]]; then
  cp "$SRC" "$DEST"
  chmod +x "$DEST"
else
  echo -e "  ${DIM}(requires sudo)${RESET}"
  sudo cp "$SRC" "$DEST"
  sudo chmod +x "$DEST"
fi

# Set RWL_HOME hint
echo ""
echo -e "${GREEN}${BOLD}✅ rwl installed successfully!${RESET}"
echo ""
echo -e "  ${DIM}To use, add the ImInDanger repo as RWL_HOME:${RESET}"
echo ""
echo -e "  ${CYAN}export RWL_HOME=\"$SCRIPT_DIR\"${RESET}"
echo ""
echo -e "  ${DIM}Add to your shell profile (~/.bashrc, ~/.zshrc):${RESET}"
echo ""
echo -e "  ${CYAN}echo 'export RWL_HOME=\"$SCRIPT_DIR\"' >> ~/.zshrc${RESET}"
echo ""
echo -e "  ${DIM}Or symlink instead of copying:${RESET}"
echo ""
echo -e "  ${CYAN}ln -sf \"$SRC\" \"$PREFIX/bin/rwl\"${RESET}"
echo ""
echo -e "  ${DIM}Then run:${RESET}"
echo -e "  ${BOLD}rwl help${RESET}"
echo ""
