# Repository Instructions — Ralph Wiggum Loop

This repository implements the **Ralph Wiggum Loop** pattern — an iterative, fresh-context
agent loop for completing complex coding tasks with AI agents.

## Project Structure

- `.github/agents/` — Custom agent profiles for the loop
- `.github/skills/` — Reusable skills (loop runner, state management, convergence detection, guardrails)
- `templates/` — Template files for new loop runs
- `hooks/` — Git hooks for safety enforcement
- `examples/` — Example loop configurations

## When Working in This Repository

1. **State files are sacred.** `TASKS.md`, `PROGRESS.md`, and `LOOP_CONFIG.md` are the source of truth during a loop run. Always read them before acting.

2. **One task per iteration.** The Ralph Wiggum Loop pattern requires exactly one bounded task per agent invocation. Never attempt multiple tasks.

3. **Failures are data.** When something fails, record the full error in `PROGRESS.md`. The next iteration will read it and course-correct.

4. **Never game metrics.** Do not delete tests, weaken assertions, add bulk lint suppressions, or use `any` types to make checks pass.

5. **Respect guardrails.** Check `LOOP_CONFIG.md` for iteration limits, path restrictions, and validation commands before making changes.

## Coding Standards

- Use clear, descriptive commit messages
- Scripts use `bash` with `set -euo pipefail`
- Markdown files follow GitHub Flavored Markdown
- Agent/skill files use YAML frontmatter format
