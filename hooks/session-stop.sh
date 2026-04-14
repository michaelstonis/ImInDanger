#!/usr/bin/env bash
# session-stop.sh — Ralph Wiggum Loop Plugin
# VS Code Stop hook: warns if tasks are still in-progress when the session ends.
#
# Output format (stdout, exit 0):
#   {"systemMessage": "<warning>"} — when in-progress tasks are detected
#   (empty)                        — when no warning is needed

set -euo pipefail

WORKSPACE="${WORKSPACE_ROOT:-$(pwd)}"

[[ -f "$WORKSPACE/TASKS.md" ]] || exit 0

in_prog=$(grep -c '\[~\]' "$WORKSPACE/TASKS.md" 2>/dev/null || echo 0)

if [[ "$in_prog" -gt 0 ]]; then
  noun=$([[ "$in_prog" -eq 1 ]] && echo "task" || echo "tasks")
  printf '{"systemMessage":"⚠️ Ralph Wiggum Loop: %d in-progress %s detected. Make sure PROGRESS.md and TASKS.md are updated before ending this session."}\n' \
    "$in_prog" "$noun"
fi

exit 0
