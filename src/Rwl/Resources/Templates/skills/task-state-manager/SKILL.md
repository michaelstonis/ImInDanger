---
name: task-state-manager
description: >
  Manages Ralph Wiggum Loop state files (TASKS.md, PROGRESS.md, LOOP_CONFIG.md).
  Use this skill when asked to initialize loop state, create task files, reset progress,
  compact progress logs, or manage the loop's persistent disk state.
---

# Task State Manager Skill

This skill manages the persistent state files used by the Ralph Wiggum Loop.

## State Files

The loop uses three core state files, all in the project root:

| File | Purpose | Created by |
|------|---------|------------|
| `TASKS.md` | Task list with status markers | User or loop-planner agent |
| `PROGRESS.md` | Append-only iteration log | Loop orchestrator |
| `LOOP_CONFIG.md` | Loop configuration | User or init-state.sh |

## Operations

### Initialize State Files

Run `init-state.sh` to create template state files:

```bash
bash .github/skills/task-state-manager/init-state.sh [--force]
```

Use `--force` to overwrite existing files.

### Task Status Markers

Tasks in `TASKS.md` use these status markers:

| Marker | Meaning |
|--------|---------|
| `[ ]` | Pending — not yet started |
| `[~]` | In progress — being worked on |
| `[x]` | Done — completed successfully |
| `[!]` | Failed — attempted but unsuccessful |
| `[STOP]` | Stop flag — halt the loop |

### Updating Tasks

When marking a task complete:
```markdown
### 1. Implement user model
- **Status:** [x]
- **Completed:** 2024-01-15T10:30:00Z
```

When marking a task failed:
```markdown
### 3. Fix auth middleware
- **Status:** [!]
- **Error:** TypeError: Cannot read property 'token' of undefined
- **Attempted:** 2024-01-15T11:00:00Z
```

### Compacting Progress

If `PROGRESS.md` grows too large (>500 lines), compact it:

1. Keep the last 5 iteration entries in full
2. Summarize earlier entries into a "Historical Summary" section
3. Preserve total iteration count and overall statistics

### Resetting State

To reset for a new loop run:
1. Keep `TASKS.md` but reset all statuses to `[ ]`
2. Archive `PROGRESS.md` to `PROGRESS.{timestamp}.md`
3. Keep `LOOP_CONFIG.md` as-is

## Important

- Never delete `PROGRESS.md` — it's the audit trail
- Always append to `PROGRESS.md`, never overwrite existing entries
- `TASKS.md` is the source of truth for what work remains
- `LOOP_CONFIG.md` controls safety limits — don't weaken them during a loop
