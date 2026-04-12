#!/usr/bin/env bash
# Ralph Wiggum Loop — Hook Setup
# Installs git hooks for the loop pattern

set -euo pipefail

HOOKS_DIR="$(git rev-parse --git-dir 2>/dev/null)/hooks"

if [[ ! -d "$HOOKS_DIR" ]]; then
  echo "❌ Not in a git repository. Initialize git first: git init"
  exit 1
fi

echo "🔧 Installing Ralph Wiggum Loop git hooks..."
echo ""

# Install pre-commit hook
PRE_COMMIT_SRC="hooks/pre-commit"
PRE_COMMIT_DST="$HOOKS_DIR/pre-commit"

if [[ -f "$PRE_COMMIT_SRC" ]]; then
  if [[ -f "$PRE_COMMIT_DST" ]]; then
    echo "⚠️  Pre-commit hook already exists at $PRE_COMMIT_DST"
    read -rp "   Overwrite? (y/N) " confirm
    if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
      echo "   Skipped."
    else
      cp "$PRE_COMMIT_SRC" "$PRE_COMMIT_DST"
      chmod +x "$PRE_COMMIT_DST"
      echo "   ✅ Pre-commit hook installed"
    fi
  else
    cp "$PRE_COMMIT_SRC" "$PRE_COMMIT_DST"
    chmod +x "$PRE_COMMIT_DST"
    echo "✅ Pre-commit hook installed"
  fi
else
  echo "❌ Pre-commit hook source not found at $PRE_COMMIT_SRC"
fi

echo ""
echo "🎉 Hook setup complete!"
echo ""
echo "Installed hooks:"
echo "  - pre-commit: Validates changes against loop guardrails"
echo ""
echo "To remove hooks, delete them from $HOOKS_DIR/"
