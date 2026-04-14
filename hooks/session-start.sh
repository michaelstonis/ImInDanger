#!/usr/bin/env bash
# session-start.sh — Ralph Wiggum Loop Plugin
# VS Code SessionStart hook: injects current loop state as context when a session begins.
#
# Output format (stdout, exit 0):
#   {"systemMessage": "<state summary>"}
# If TASKS.md is not present, exits 0 silently (not a loop project).

set -euo pipefail

WORKSPACE="${WORKSPACE_ROOT:-$(pwd)}"

# Bail out silently if this project doesn't use the Ralph Wiggum Loop pattern
[[ -f "$WORKSPACE/TASKS.md" ]] || exit 0

# ── Task counts ──────────────────────────────────────────────────────────────
tasks_content=$(cat "$WORKSPACE/TASKS.md" 2>/dev/null || true)

count_marker() {
  local pattern="$1"
  echo "$tasks_content" | grep -c "$pattern" 2>/dev/null || echo 0
}

done_count=$(count_marker '\[x\]')
in_prog=$(count_marker '\[~\]')
failed=$(count_marker '\[!\]')
pending=$(count_marker '\[ \]')
total=$((done_count + in_prog + failed + pending))

msg="🔄 Ralph Wiggum Loop — ${done_count}/${total} tasks done"
[[ "$in_prog"  -gt 0 ]] && msg="${msg}, ${in_prog} in-progress"
[[ "$failed"   -gt 0 ]] && msg="${msg}, ${failed} FAILED"
[[ "$pending"  -gt 0 ]] && msg="${msg}, ${pending} pending"
msg="${msg}."

# ── Last iteration entry ─────────────────────────────────────────────────────
if [[ -f "$WORKSPACE/PROGRESS.md" ]]; then
  last_iter=$(grep '^## Iteration' "$WORKSPACE/PROGRESS.md" | tail -1 || true)
  [[ -n "$last_iter" ]] && msg="${msg} Last recorded: ${last_iter}."
fi

# ── Emit JSON ────────────────────────────────────────────────────────────────
# Escape double-quotes for JSON string embedding
safe_msg="${msg//\"/\\\"}"
printf '{"systemMessage":"%s"}\n' "$safe_msg"
