# AGENTS.md — Ralph Wiggum Loop

## Project Context

This repository provides the **Ralph Wiggum Loop** pattern as a set of reusable components
for GitHub Copilot CLI and VS Code Agents.

The Ralph Wiggum Loop is an iterative agent pattern where:
1. An AI agent reads persistent task state from disk
2. Plans and executes exactly ONE bounded task
3. Writes results back to disk
4. Restarts with a fresh context window
5. Repeats until all tasks are complete or a stop condition is met

## Key Principles

- **Fresh context each iteration** — prevents context degradation
- **State lives in files** — `TASKS.md`, `PROGRESS.md`, `LOOP_CONFIG.md`
- **One task per loop** — bounded, verifiable units of work
- **Failures are data** — errors feed back as input for next iteration
- **Natural recovery** — disk state reflects last successful write

## Available Agents

| Agent | Purpose |
|-------|---------|
| `ralph-wiggum-loop` | Main orchestrator — reads state, executes one task, writes results |
| `loop-planner` | Decomposes objectives into bounded, loop-compatible tasks |
| `loop-reviewer` | Analyzes loop health, detects pathologies, recommends actions |

## Available Skills

| Skill | Purpose |
|-------|---------|
| `loop-runner` | Shell script to run the iterative loop |
| `task-state-manager` | Initialize and manage state files |
| `convergence-detector` | Detect oscillation, stagnation, metric gaming |
| `loop-guardrails` | Enforce safety limits on changes |

## Working with State Files

### TASKS.md
Contains the task list. Status markers:
- `[ ]` pending, `[~]` in-progress, `[x]` done, `[!]` failed, `[STOP]` halt

### PROGRESS.md
Append-only iteration log. Never edit existing entries.

### LOOP_CONFIG.md
Configuration: iteration limits, timeouts, path restrictions, validation commands.

## Rules

1. Always read state files before acting
2. Complete exactly ONE task per iteration
3. Record all results (success and failure) in PROGRESS.md
4. Never delete or weaken tests
5. Respect path restrictions in LOOP_CONFIG.md
6. Stop when all tasks are done or a stop condition triggers
