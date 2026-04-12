# Ralph Wiggum Loop — Prompt Template

> Use this as a base prompt when running the loop manually or customizing the loop runner.

You are executing an iteration of the Ralph Wiggum Loop.

## Your Instructions

1. **Read state files:**
   - `TASKS.md` — find the next pending task (marked `[ ]`) or retry a failed task (marked `[!]`)
   - `PROGRESS.md` — review what previous iterations accomplished and any errors encountered
   - `LOOP_CONFIG.md` — check safety limits and validation commands

2. **Execute ONE task:**
   - Pick the first available task (respecting dependency order)
   - Make the minimum changes needed to complete it
   - Stay within the allowed paths from LOOP_CONFIG.md

3. **Validate your work:**
   - Run the validation commands from LOOP_CONFIG.md
   - If validation fails, record the error — do NOT delete or weaken tests

4. **Update state files:**
   - Mark the task `[x]` (done) or `[!]` (failed) in TASKS.md
   - Append an iteration entry to PROGRESS.md with:
     - Task description
     - Status (✅ / ❌ / ⚠️)
     - Files changed
     - Validation results
     - Notes for future iterations

5. **Stop conditions:**
   - If all tasks are `[x]`, note completion in PROGRESS.md
   - If you detect the same error 3+ times in PROGRESS.md, add `[STOP]` to TASKS.md

## Rules

- ONE task only — never attempt multiple tasks
- Failures are data — record full error output, don't hide problems
- Never delete tests or weaken assertions
- Never add bulk lint suppressions or `any` types
- If a task is too large, break it into subtasks in TASKS.md and do the first one
