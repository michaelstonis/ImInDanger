---
name: loop-guardrails
description: >
  Enforces safety guardrails for the Ralph Wiggum Loop: iteration limits, path restrictions,
  diff size limits, and validation gates. Use this skill when checking if a loop operation
  is within safety bounds or when configuring loop safety parameters.
allowed-tools: shell
---

# Loop Guardrails Skill

This skill enforces safety boundaries for Ralph Wiggum Loop operations.

## Guardrail Types

### 1. Iteration Limits
- **Max iterations:** Configurable in `LOOP_CONFIG.md` (default: 20)
- **Timeout per iteration:** Configurable (default: 10 minutes)
- Hard-stop at max iterations prevents runaway loops

### 2. Path Restrictions
- **Allowed paths:** Glob patterns in `LOOP_CONFIG.md` restricting where the agent can write
- **Restricted paths:** Files the agent must never modify (e.g., `.env`, lock files)
- Violations are caught pre-commit

### 3. Diff Size Limits
- **Max lines changed per iteration:** Configurable (default: 200)
- Prevents sweeping changes that are hard to review
- Large diffs suggest the task should be decomposed further

### 4. Validation Gates
- **Required commands:** Tests, builds, lints that must pass after each iteration
- If validation fails, the task is marked `[!]` (failed) in `TASKS.md`
- Prevents broken states from persisting

### 5. Anti-Gaming Rules
- Test files cannot be deleted
- Lint suppression comments cannot be added in bulk
- Type safety cannot be weakened (no bulk `any` casts)

## Check Command

```bash
bash .github/skills/loop-guardrails/check-guardrails.sh
```

This validates the current state of changes against all configured guardrails.

## Integration

### Pre-commit Hook
Install the pre-commit hook to automatically validate guardrails:

```bash
bash hooks/setup-hooks.sh
```

This copies the guardrail check into `.git/hooks/pre-commit`.

### Manual Check
Run at any time to verify the current changes are within bounds:

```bash
bash .github/skills/loop-guardrails/check-guardrails.sh --verbose
```

## Configuration

All guardrail settings live in `LOOP_CONFIG.md`. The skill reads this file to determine limits.

### Emergency Stop

To immediately halt the loop, create a `.loop-stop` file:

```bash
touch .loop-stop
```

The loop runner checks for this file between iterations.
