---
name: loop-runner
description: >
  Runs the Ralph Wiggum Loop — an iterative fresh-context agent loop.
  Use this skill when asked to execute a loop, start iterating on tasks, or run
  the ralph wiggum pattern. Provides the loop execution script and instructions
  for invoking it.
---

# Loop Runner Skill

This skill provides the core loop execution mechanism for the Ralph Wiggum Loop pattern.

## How to Run the Loop

Execute the `run-loop.sh` script from this skill's directory:

```bash
bash .github/skills/loop-runner/run-loop.sh [OPTIONS]
```

### Options

| Flag | Description | Default |
|------|-------------|---------|
| `--max-iterations N` | Maximum loop iterations | 20 |
| `--timeout M` | Timeout per iteration in minutes | 10 |
| `--config PATH` | Path to LOOP_CONFIG.md | `./LOOP_CONFIG.md` |
| `--agent AGENT` | Agent to use per iteration | `ralph-wiggum-loop` |
| `--dry-run` | Preview what would run without executing | false |
| `--auto-review N` | Run loop-reviewer every N iterations | 5 |

### Prerequisites

Before running the loop, ensure:
1. `TASKS.md` exists in the project root with at least one pending task
2. `PROGRESS.md` exists (or will be created automatically)
3. `LOOP_CONFIG.md` exists with configuration (or defaults will be used)
4. The `copilot` CLI is installed and authenticated

### Example Usage

```bash
# Basic loop with defaults
bash .github/skills/loop-runner/run-loop.sh

# Limited iterations with auto-review
bash .github/skills/loop-runner/run-loop.sh --max-iterations 10 --auto-review 3

# Dry run to preview
bash .github/skills/loop-runner/run-loop.sh --dry-run
```

## How It Works

1. The script reads `LOOP_CONFIG.md` for configuration
2. Each iteration invokes `copilot` CLI with the `ralph-wiggum-loop` agent
3. The agent reads `TASKS.md` and `PROGRESS.md`, performs one task, and updates both files
4. The script checks stop conditions between iterations
5. Optionally runs the `loop-reviewer` agent for health checks
6. Continues until all tasks complete, max iterations reached, or a stop condition triggers
