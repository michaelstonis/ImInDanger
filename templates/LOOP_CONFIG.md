# Loop Configuration

## Safety Limits
- max_iterations: 20
- timeout_minutes: 10
- auto_review_interval: 5

## Validation Commands
<!-- Uncomment and customize for your project -->
```
# npm test
# npm run build
# npm run lint
# npx tsc --noEmit
# python -m pytest
# go test ./...
# cargo test
```

## Allowed Paths
<!-- Glob patterns for files the loop agent may modify -->
<!-- Leave empty to allow all project files -->
```
# src/**
# lib/**
# tests/**
# test/**
```

## Restricted Paths
<!-- Files the agent must NEVER modify -->
```
.env
.env.*
*.lock
package-lock.json
yarn.lock
pnpm-lock.yaml
Cargo.lock
go.sum
node_modules/**
.git/**
```

## Stop Conditions
- All tasks in TASKS.md marked [x]
- Same error occurring 3+ consecutive times (oscillation)
- [STOP] flag present in TASKS.md
- `.loop-stop` file exists in project root

## Diff Limits
- max_lines_per_iteration: 200
- max_files_per_iteration: 10

## Notes
<!-- Additional context for the loop agent -->
<!-- Example: "This is a TypeScript project using React and Express" -->
<!-- Example: "Always run prettier after modifying .ts files" -->
