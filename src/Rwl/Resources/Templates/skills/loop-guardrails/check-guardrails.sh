#!/usr/bin/env bash
# Ralph Wiggum Loop — Guardrail Enforcement
# Validates changes against configured safety limits

set -euo pipefail

CONFIG_FILE="./LOOP_CONFIG.md"
VERBOSE=false
EXIT_CODE=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --config)  CONFIG_FILE="$2"; shift 2 ;;
    --verbose) VERBOSE=true; shift ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

log() {
  [[ "$VERBOSE" == "true" ]] && echo "   $1"
}

echo "🛡️  Guardrail Check"
echo ""

# ── Check restricted paths ──
echo "── Path Restrictions ──"

RESTRICTED_PATTERNS=(".env" ".env.*" "*.lock" "package-lock.json")

if [[ -f "$CONFIG_FILE" ]]; then
  while IFS= read -r pattern; do
    pattern=$(echo "$pattern" | sed 's/^#\s*//' | xargs)
    [[ -n "$pattern" ]] && RESTRICTED_PATTERNS+=("$pattern")
  done < <(sed -n '/## Restricted Paths/,/```/{/```/d;p}' "$CONFIG_FILE" 2>/dev/null \
    | grep -v '^$' | grep -v '^```' | grep '^#' || true)
fi

if command -v git &>/dev/null && git rev-parse --is-inside-work-tree &>/dev/null 2>&1; then
  changed_files=$(git diff --cached --name-only 2>/dev/null || git diff --name-only 2>/dev/null || true)

  if [[ -n "$changed_files" ]]; then
    violations=0
    while IFS= read -r file; do
      for pattern in "${RESTRICTED_PATTERNS[@]}"; do
        case "$file" in
          $pattern)
            echo "   🔴 VIOLATION: Modified restricted file: $file (matches: $pattern)"
            violations=$((violations + 1))
            EXIT_CODE=2
            ;;
        esac
      done
    done <<< "$changed_files"

    if [[ $violations -eq 0 ]]; then
      echo "   🟢 OK — no restricted files modified"
    fi
  else
    echo "   🟢 OK — no changes detected"
  fi
else
  echo "   ⚪ Not in a git repo (skipping path check)"
fi

echo ""

# ── Check diff size ──
echo "── Diff Size ──"

MAX_DIFF_LINES=200

if command -v git &>/dev/null && git rev-parse --is-inside-work-tree &>/dev/null 2>&1; then
  diff_lines=$(git diff --stat 2>/dev/null | tail -1 | grep -oP '\d+ insertion' | grep -oP '\d+' || echo 0)
  del_lines=$(git diff --stat 2>/dev/null | tail -1 | grep -oP '\d+ deletion' | grep -oP '\d+' || echo 0)
  total_changes=$(( ${diff_lines:-0} + ${del_lines:-0} ))

  if [[ "$total_changes" -gt "$MAX_DIFF_LINES" ]]; then
    echo "   🟡 WARNING — $total_changes lines changed (limit: $MAX_DIFF_LINES)"
    echo "   Consider breaking this into smaller tasks"
    [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
  else
    echo "   🟢 OK — $total_changes lines changed (limit: $MAX_DIFF_LINES)"
  fi
else
  echo "   ⚪ Not in a git repo (skipping diff check)"
fi

echo ""

# ── Check for test deletions ──
echo "── Test Protection ──"

if command -v git &>/dev/null && git rev-parse --is-inside-work-tree &>/dev/null 2>&1; then
  deleted_tests=$(git diff --name-only --diff-filter=D 2>/dev/null \
    | grep -iE '\.(test|spec)\.' || true)

  if [[ -n "$deleted_tests" ]]; then
    echo "   🔴 VIOLATION: Test files deleted!"
    echo "$deleted_tests" | sed 's/^/      /'
    EXIT_CODE=2
  else
    echo "   🟢 OK — no test files deleted"
  fi
else
  echo "   ⚪ Not in a git repo (skipping test check)"
fi

echo ""

# ── Check stop conditions ──
echo "── Stop Conditions ──"

if [[ -f ".loop-stop" ]]; then
  echo "   🛑 .loop-stop file exists — loop should halt"
  EXIT_CODE=2
else
  log "No .loop-stop file"
fi

if [[ -f "./TASKS.md" ]] && grep -q '\[STOP\]' "./TASKS.md" 2>/dev/null; then
  echo "   🛑 [STOP] flag found in TASKS.md — loop should halt"
  EXIT_CODE=2
else
  log "No [STOP] flag in TASKS.md"
fi

echo "   🟢 OK — no emergency stop conditions"

echo ""
case $EXIT_CODE in
  0) echo "✅ All guardrails passed" ;;
  1) echo "⚠️  Warnings detected (review recommended)" ;;
  2) echo "🛑 Guardrail violations detected (action required)" ;;
esac

exit $EXIT_CODE
