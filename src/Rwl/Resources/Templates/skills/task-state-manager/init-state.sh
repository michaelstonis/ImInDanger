#!/usr/bin/env bash
# Initialize Ralph Wiggum Loop state files
# Creates TASKS.md, PROGRESS.md, and LOOP_CONFIG.md templates

set -euo pipefail

FORCE=false
[[ "${1:-}" == "--force" ]] && FORCE=true

create_file() {
  local file="$1"
  local content="$2"

  if [[ -f "$file" ]] && [[ "$FORCE" != "true" ]]; then
    echo "⏭️  Skipping $file (already exists, use --force to overwrite)"
    return
  fi

  echo "$content" > "$file"
  echo "✅ Created $file"
}

# ── TASKS.md ──
create_file "TASKS.md" '# Tasks

## Objective
<!-- Describe the overall goal of this loop run -->

## Success Criteria
<!-- How to verify ALL work is complete -->
<!-- e.g., "All tests pass, build succeeds, no lint errors" -->

## Task List

### 1. [Task Title]
- **Status:** [ ]
- **Files:** `path/to/file`
- **Description:** [What to do — be specific enough for a fresh agent]
- **Validation:** `[command to verify success]`
- **Notes:** [Any gotchas or context]

### 2. [Task Title]
- **Status:** [ ]
- **Depends on:** Task 1
- **Files:** `path/to/file`
- **Description:** [...]
- **Validation:** `[command to verify success]`

<!-- Add more tasks as needed -->
'

# ── PROGRESS.md ──
create_file "PROGRESS.md" '# Progress Log

> Ralph Wiggum Loop — Iteration History
> Generated: '"$(date -u +%Y-%m-%dT%H:%M:%SZ)"'

---

<!-- Entries are appended by the loop orchestrator. Do not edit above this line. -->
'

# ── LOOP_CONFIG.md ──
create_file "LOOP_CONFIG.md" '# Loop Configuration

## Safety Limits
- max_iterations: 20
- timeout_minutes: 10
- auto_review_interval: 5

## Validation Commands
<!-- Commands run after each task to verify correctness -->
```
# Uncomment and customize:
# npm test
# npm run build
# npm run lint
# npx tsc --noEmit
```

## Allowed Paths
<!-- Restrict which paths the agent can modify (glob patterns) -->
<!-- Leave empty to allow all paths in the project -->
```
# src/**
# tests/**
# *.config.*
```

## Restricted Paths
<!-- Paths the agent must NEVER modify -->
```
# .env
# .env.*
# *.lock
# package-lock.json
# node_modules/**
```

## Stop Conditions
<!-- Additional conditions that should halt the loop -->
- All tasks in TASKS.md marked [x]
- Same error occurring 3+ consecutive times
- [STOP] flag in TASKS.md
- .loop-stop file exists in project root

## Notes
<!-- Any additional context for the loop agent -->
'

echo ""
echo "🚌 Ralph Wiggum Loop state files initialized!"
echo "   Next steps:"
echo "   1. Edit TASKS.md to add your tasks"
echo "   2. Review LOOP_CONFIG.md settings"
echo "   3. Run: bash .github/skills/loop-runner/run-loop.sh"
