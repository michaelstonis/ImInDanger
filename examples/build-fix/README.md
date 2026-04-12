# Example: Build Fix

This example demonstrates using the Ralph Wiggum Loop to fix a broken CI build
after a TypeScript version upgrade.

## Why This Works Well

- **Error messages are specific:** compiler errors include file, line, and error code
- **Errors feed back as data:** each failed iteration captures the exact error output
- **Independent fixes:** most type errors can be fixed file-by-file
- **Clear completion:** zero type errors = done

## How to Use

1. Capture your current build errors: `npx tsc --noEmit > .loop-logs/build-errors.txt 2>&1`
2. Copy `TASKS.md` from this directory to your project root
3. Edit the tasks to match your actual errors
4. Run `bash .github/skills/task-state-manager/init-state.sh` to create state files
5. Run `bash .github/skills/loop-runner/run-loop.sh`

## Expected Behavior

The loop will:
1. Fix null check errors in UserController
2. Update deprecated API calls in AuthService
3. Fix module resolution in tsconfig.json
4. Fix enum usage patterns
5. Run final build verification

Each iteration targets a specific error category, making progress measurable.

## Tips

- Sort tasks by error count (most errors first) for maximum early progress
- Use `grep -c "TSNNNN"` in validation to track specific error codes
- If an error fix introduces new errors, the next iteration will catch them
