#!/usr/bin/env bash
# Ralph Wiggum Loop — Post-Iteration Check
# Run after each loop iteration to validate results
#
# Usage: bash hooks/post-iteration.sh [ITERATION_NUMBER]

set -euo pipefail

ITERATION="${1:-?}"
TASKS_FILE="./TASKS.md"
PROGRESS_FILE="./PROGRESS.md"
CONFIG_FILE="./LOOP_CONFIG.md"

echo "📋 Post-Iteration Check (Iteration $ITERATION)"
echo ""

# ── Check task state consistency ──
echo "── Task State ──"
if [[ -f "$TASKS_FILE" ]]; then
  pending=$(grep -c '\[ \]' "$TASKS_FILE" 2>/dev/null || echo 0)
  in_progress=$(grep -c '\[~\]' "$TASKS_FILE" 2>/dev/null || echo 0)
  done=$(grep -c '\[x\]' "$TASKS_FILE" 2>/dev/null || echo 0)
  failed=$(grep -c '\[!\]' "$TASKS_FILE" 2>/dev/null || echo 0)
  echo "   Pending: $pending | In Progress: $in_progress | Done: $done | Failed: $failed"

  if [[ "$in_progress" -gt 1 ]]; then
    echo "   ⚠️  Multiple tasks in-progress — should be at most 1"
  fi
else
  echo "   ⚠️  TASKS.md not found"
fi

echo ""

# ── Check progress file ──
echo "── Progress Log ──"
if [[ -f "$PROGRESS_FILE" ]]; then
  entries=$(grep -c '^## Iteration' "$PROGRESS_FILE" 2>/dev/null || echo 0)
  echo "   Entries: $entries"

  lines=$(wc -l < "$PROGRESS_FILE")
  if [[ "$lines" -gt 500 ]]; then
    echo "   ⚠️  PROGRESS.md is $lines lines — consider compacting"
  fi
else
  echo "   ⚠️  PROGRESS.md not found"
fi

echo ""

# ── Run convergence check ──
CONVERGENCE_SCRIPT=".github/skills/convergence-detector/check-convergence.sh"
if [[ -f "$CONVERGENCE_SCRIPT" ]]; then
  bash "$CONVERGENCE_SCRIPT" --mode all
else
  echo "⚠️  Convergence detector not found"
fi

echo ""

# ── Run validation commands from config ──
echo "── Validation Commands ──"
if [[ -f "$CONFIG_FILE" ]]; then
  # Extract uncommented commands from the Validation Commands section
  commands=$(sed -n '/## Validation Commands/,/^## /{/^```/,/^```/{ /^```/d; /^#/d; /^$/d; p; }}' "$CONFIG_FILE" 2>/dev/null || true)

  if [[ -n "$commands" ]]; then
    while IFS= read -r cmd; do
      cmd=$(echo "$cmd" | xargs)
      [[ -z "$cmd" ]] && continue
      echo "   Running: $cmd"
      if eval "$cmd" 2>&1 | tail -5; then
        echo "   ✅ Passed"
      else
        echo "   ❌ Failed"
      fi
    done <<< "$commands"
  else
    echo "   No validation commands configured"
  fi
else
  echo "   No LOOP_CONFIG.md found"
fi

echo ""
echo "✅ Post-iteration check complete"
