#!/usr/bin/env bash
# Ralph Wiggum Loop — Convergence Detection
# Checks for oscillation, stagnation, and metric gaming

set -euo pipefail

MODE="all"
PROGRESS_FILE="./PROGRESS.md"
TASKS_FILE="./TASKS.md"
THRESHOLD=3  # Number of iterations to check
EXIT_CODE=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --mode)     MODE="$2"; shift 2 ;;
    --progress) PROGRESS_FILE="$2"; shift 2 ;;
    --tasks)    TASKS_FILE="$2"; shift 2 ;;
    --threshold) THRESHOLD="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

echo "🔍 Convergence Check (mode: $MODE)"
echo "   Progress: $PROGRESS_FILE"
echo "   Tasks:    $TASKS_FILE"
echo ""

# ── Oscillation Detection ──
check_oscillation() {
  echo "── Oscillation Detection 🔄 ──"

  if [[ ! -f "$PROGRESS_FILE" ]]; then
    echo "   ⚪ No progress file found (nothing to check)"
    return
  fi

  local fail_count
  fail_count=$(grep -c '❌ Failed' "$PROGRESS_FILE" 2>/dev/null || echo 0)

  if [[ "$fail_count" -lt "$THRESHOLD" ]]; then
    echo "   🟢 OK — fewer than $THRESHOLD failures recorded"
    return
  fi

  # Check if recent failures are on the same task
  local unique_failed_tasks
  unique_failed_tasks=$(grep -B2 '❌ Failed' "$PROGRESS_FILE" \
    | grep -oP '(?<=\*\*Task:\*\*\s).+' \
    | tail -"$THRESHOLD" \
    | sort -u \
    | wc -l)

  if [[ "$unique_failed_tasks" -le 1 ]] && [[ "$fail_count" -ge "$THRESHOLD" ]]; then
    echo "   🔴 CRITICAL — Same task failing $THRESHOLD+ consecutive times!"
    echo "   Recommendation: Stop the loop. This task needs human review."
    EXIT_CODE=2
  else
    echo "   🟢 OK — failures are on different tasks"
  fi
}

# ── Stagnation Detection ──
check_stagnation() {
  echo "── Stagnation Detection 🧊 ──"

  if [[ ! -f "$TASKS_FILE" ]]; then
    echo "   ⚪ No tasks file found"
    return
  fi

  # Check for tasks stuck in-progress
  local in_progress
  in_progress=$(grep -c '\[~\]' "$TASKS_FILE" 2>/dev/null || echo 0)

  if [[ "$in_progress" -gt 0 ]]; then
    echo "   🟡 WARNING — $in_progress task(s) stuck in-progress [~]"
    [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
  fi

  # Check completion rate
  local total_tasks done_tasks
  total_tasks=$(grep -cP '^\s*-\s*\*\*Status:\*\*' "$TASKS_FILE" 2>/dev/null || echo 0)
  done_tasks=$(grep -c '\[x\]' "$TASKS_FILE" 2>/dev/null || echo 0)

  if [[ "$total_tasks" -gt 0 ]]; then
    local percent=$(( (done_tasks * 100) / total_tasks ))
    echo "   📊 Progress: $done_tasks / $total_tasks tasks complete ($percent%)"
  fi

  # Check if last N entries show minimal changes
  if [[ -f "$PROGRESS_FILE" ]]; then
    local no_change_count
    no_change_count=$(tail -50 "$PROGRESS_FILE" | grep -ic 'no changes\|no modifications\|nothing to do' 2>/dev/null || echo 0)
    if [[ "$no_change_count" -ge 2 ]]; then
      echo "   🟡 WARNING — Recent iterations show minimal/no changes"
      [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
    else
      echo "   🟢 OK — recent iterations are making changes"
    fi
  fi
}

# ── Metric Gaming Detection ──
check_gaming() {
  echo "── Metric Gaming Detection 🎮 ──"

  if ! command -v git &>/dev/null || ! git rev-parse --is-inside-work-tree &>/dev/null 2>&1; then
    echo "   ⚪ Not in a git repo (skipping diff-based checks)"
    return
  fi

  local issues=0

  # Check for deleted test files
  local deleted_tests
  deleted_tests=$(git diff --name-only --diff-filter=D 2>/dev/null \
    | grep -iE '(test|spec|__tests__)' || true)
  if [[ -n "$deleted_tests" ]]; then
    echo "   🔴 CRITICAL — Test files deleted:"
    echo "$deleted_tests" | sed 's/^/      /'
    issues=$((issues + 1))
    EXIT_CODE=2
  fi

  # Check for ts-ignore / eslint-disable additions
  local suppression_adds
  suppression_adds=$(git diff 2>/dev/null \
    | grep -c '^\+.*\(@ts-ignore\|eslint-disable\|@ts-expect-error\|# type: ignore\|# noqa\)' \
    || echo 0)
  if [[ "$suppression_adds" -gt 2 ]]; then
    echo "   🟡 WARNING — $suppression_adds lint/type suppression(s) added"
    [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
    issues=$((issues + 1))
  fi

  # Check for 'any' type additions (TypeScript)
  local any_adds
  any_adds=$(git diff 2>/dev/null \
    | grep -c '^\+.*:\s*any\b' || echo 0)
  if [[ "$any_adds" -gt 3 ]]; then
    echo "   🟡 WARNING — $any_adds 'any' type annotations added"
    [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
    issues=$((issues + 1))
  fi

  if [[ "$issues" -eq 0 ]]; then
    echo "   🟢 OK — no metric gaming detected"
  fi
}

# ── Progress File Size Check ──
check_progress_size() {
  echo "── Progress File Health 📚 ──"

  if [[ ! -f "$PROGRESS_FILE" ]]; then
    echo "   ⚪ No progress file found"
    return
  fi

  local lines
  lines=$(wc -l < "$PROGRESS_FILE")

  if [[ "$lines" -gt 500 ]]; then
    echo "   🟡 WARNING — PROGRESS.md has $lines lines (recommend compacting)"
    [[ $EXIT_CODE -lt 1 ]] && EXIT_CODE=1
  elif [[ "$lines" -gt 200 ]]; then
    echo "   🟡 NOTE — PROGRESS.md has $lines lines (consider compacting soon)"
  else
    echo "   🟢 OK — PROGRESS.md is $lines lines"
  fi
}

# ── Run checks ──
case "$MODE" in
  oscillation)  check_oscillation ;;
  stagnation)   check_stagnation ;;
  gaming)       check_gaming ;;
  all)
    check_oscillation
    echo ""
    check_stagnation
    echo ""
    check_gaming
    echo ""
    check_progress_size
    ;;
  *) echo "Unknown mode: $MODE"; exit 1 ;;
esac

echo ""
case $EXIT_CODE in
  0) echo "✅ Overall: Healthy" ;;
  1) echo "⚠️  Overall: Warnings detected (review recommended)" ;;
  2) echo "🛑 Overall: Critical issues detected (stop recommended)" ;;
esac

exit $EXIT_CODE
