---
name: convergence-detector
description: >
  Detects convergence issues in Ralph Wiggum Loop iterations: oscillation, stagnation,
  metric gaming, and context overflow. Use this skill when checking loop health or when
  the loop seems stuck.
---

# Convergence Detector Skill

This skill analyzes Ralph Wiggum Loop state files to detect pathological loop behaviors.

## When to Use

Run the convergence detector:
- After every N iterations (configured via `auto_review_interval`)
- When the loop seems stuck
- When errors are repeating
- Before deciding whether to continue or stop the loop

## Detection Modes

### 1. Oscillation Detection 🔄

**What:** Agent alternates between two states — fix A breaks B, fix B re-breaks A.

**Check command:**
```bash
bash .github/skills/convergence-detector/check-convergence.sh --mode oscillation
```

**How it works:**
- Examines the last N iteration entries in `PROGRESS.md`
- Checks if the same files are being modified repeatedly
- Checks if error patterns alternate between two states

### 2. Stagnation Detection 🧊

**What:** Agent makes no meaningful progress across iterations.

**Check command:**
```bash
bash .github/skills/convergence-detector/check-convergence.sh --mode stagnation
```

**How it works:**
- Checks if any task has been `[~]` for 3+ iterations
- Checks if `PROGRESS.md` entries show "no changes"
- Compares task completion rate across recent iterations

### 3. Metric Gaming Detection 🎮

**What:** Agent manipulates success criteria instead of doing real work.

**Check command:**
```bash
bash .github/skills/convergence-detector/check-convergence.sh --mode gaming
```

**How it works:**
- Checks git diff for deleted test files
- Checks for weakened assertions (`.toBe` → `.toBeTruthy`)
- Checks for added `@ts-ignore`, `eslint-disable`, or `any` type casts
- Checks for deleted or commented-out test cases

### 4. Full Health Check

**Check command:**
```bash
bash .github/skills/convergence-detector/check-convergence.sh --mode all
```

Runs all detection modes and produces a combined health report.

## Interpreting Results

| Exit Code | Meaning |
|-----------|---------|
| 0 | Healthy — no issues detected |
| 1 | Warning — potential issues found |
| 2 | Critical — pathological behavior detected, stop recommended |

## Integration

The convergence detector is automatically invoked by `run-loop.sh` when `--auto-review`
is enabled. You can also invoke it manually at any time to check loop health.
