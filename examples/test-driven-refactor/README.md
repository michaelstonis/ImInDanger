# Example: Test-Driven Refactoring

This example demonstrates using the Ralph Wiggum Loop to refactor a `UserService` class
to use dependency injection, guided by existing tests.

## Why This Works Well

- **Clear success criteria:** existing tests define correct behavior
- **Bounded tasks:** each step touches 1-2 files
- **Dependency ordering:** interface → implementation → consumer → tests
- **Verification at each step:** `tsc --noEmit` and `npm test` validate progress

## How to Use

1. Copy `TASKS.md` from this directory to your project root
2. Edit the file paths and descriptions to match your project
3. Run `bash .github/skills/task-state-manager/init-state.sh` to create state files
4. Run `bash .github/skills/loop-runner/run-loop.sh`

## Expected Behavior

The loop will:
1. Create the interface (compiles ✅)
2. Create the Postgres implementation (compiles ✅)
3. Create the mock (compiles ✅)
4. Refactor UserService constructor (compiles ✅)
5. Update tests to use mock (tests pass ✅)
6. Update app bootstrap (builds ✅)
7. Run final smoke test (everything passes ✅)

Each step leaves the codebase in a working state.
