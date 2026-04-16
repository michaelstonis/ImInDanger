<div align="center">

# 🔄 Ralph Wiggum Loop

```
                                        @@@@@@@@@@@@@@@@@
                                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                            @@@@  @@@@@@   @@@ @@@  @@ @@@@ @@@@@@@@@@
                        @@@@@  @@@@@@@   @@  @@@   @@  @@ @@  @@ @@@@@@@@@
                     @@@@@  @@@@ @@@   @@  @@@    @@   @@  @@  @@@ @@@@@@@@@@
                  @@@@@@  @@@  @@@   @@@  @@     @@    @    @@   @@  @@@@@@@@@@@
                @@@@@@  @@@   @@    @@   @@     @@    @@     @@   @@   @@@ @@@@@@@
              @@@ @@   @@   @@    @@    @@      @@    @@     @@    @@    @@  @@ @@@@@
            @@@ @@   @@@   @@    @@    @@      @@     @@      @@    @@    @@  @@@  @@@@
           @@  @@   @@    @@    @@     @@      @@     @@      @@     @@    @@   @@  @@@@@
         @@@  @@   @@    @@     @     @@       @      @@      @@      @@    @@   @@   @@@@@
        @@   @@   @@    @@     @@     @@       @      @        @@      @     @    @@   @@ @@@
       @@   @    @@    @@@    @@      @@                                                @@  @@
      @@   @@   @@     @@     @@                                                         @@   @
     @@   @@   @@     @@     @@             @@@@@@@@@@@@@                 @@@@@@@@@@@    @@
     @   @@    @@     @@     @@           @@@           @@@             @@           @@   @@
    @@   @@   @@     @@                  @@               @@          @@              @@  @@
    @@  @@    @@     @@                 @@                 @@         @@               @@ @@
    @   @@   @@      @@                 @@     @@           @         @        @@@      @ @@
        @            @                  @@    @@@@          @         @@       @@@     @@ @@
       @@@@@@@                          @@     @@          @@          @               @  @@
      @@@    @@@                         @@                @            @@           @@@  @@
     @@                                   @@@            @@    @@@@@@@@  @@@@      @@@    @
    @@   @@@@                               @@@@      @@@           @@@@@@  @@@@@@       @@
     @  @ @                                     @@@@@                    @@               @
     @@    @                                            @@                @@              @@
      @@@                                              @@  @@@@@          @@               @@@
        @@@@    @                                      @@@@@   @@@       @@                  @@
          @@@@@@@                                    @@@@       @@@@@@@@@@                    @@
             @@                                    @@@         @@@@                            @@
              @@                                  @@         @@@                               @@
               @@@                        @@     @@         @@  @@@                           @@
                 @@                       @@    @@         @@@@@@@@@@@                     @@@@
                  @@@                    @@@@@@@@@         @@@       @@               @@@@@@
                 @@ @@@@                @@      @                   @@@@@@@@@@@@@@@@@@@
                @@     @@@                      @                @@@@  @@@       @@
               @@        @@@@@                  @               @        @@      @
               @            @@@@@@              @                         @@   @@
              @@                @@@@@@@@@   @@@@@                      @ @@  @@@@
              @@@@                    @@@@@@@@@@@@@@                 @@@@@@ @@  @@@@
            @@@@@@@                       @@       @@@@             @@ @@@ @@      @@@
         @@@      @@@                    @@           @@@@          @@@    @@    @@@ @@
        @           @@@@                @@               @@@@       @@      @@ @@@    @@
       @@             @@@@@           @@@                  @@@      @@       @@@@@@   @@
       @                 @@@@@       @@@                     @@@    @@     @@@    @@   @@@
      @@                    @@@@@   @@                         @@  @@     @@       @@@ @@@@
       @                         @@@@                           @@@@@@@ @@@          @@@@ @@
       @@                      @@@@                             @@@   @@@             @@@  @@
       @@                                                        @@                    @    @@
       @@@                                                       @@                          @@
      @@@@                                                      @@@                           @@
    @@   @@                                                    @@@                            @@@
    @     @@                                                 @@@                               @@@
   @       @@@                                              @@@                                 @@@@
  @@        @@@                                           @@@                                   @@@@
```

### *"I'm in danger!"*

**An iterative, fresh-context agent loop pattern for GitHub Copilot CLI**

[![Pattern](https://img.shields.io/badge/pattern-Ralph_Wiggum_Loop-blue)](#)
[![Platform](https://img.shields.io/badge/platform-GitHub_Copilot_CLI-green)](#)
[![License](https://img.shields.io/badge/license-MIT-yellow)](#license)

*Complete complex coding tasks reliably by running an AI agent in a loop where each iteration starts fresh, reads persistent state from disk, completes one bounded task, and writes results back.*

</div>

---

## Table of Contents

- [Overview](#overview)
- [Why This Works](#why-this-works)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [CLI Reference (`rwl`)](#cli-reference-rwl)
- [VS Code Agent Plugin](#vs-code-agent-plugin)
  - [Installation](#plugin-installation)
  - [What the Plugin Provides](#what-the-plugin-provides)
  - [Plugin Hooks](#plugin-hooks)
- [Components](#components)
  - [Agents](#agents)
  - [Skills](#skills)
  - [Instructions](#instructions)
  - [Templates](#templates)
  - [Hooks](#hooks)
- [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Failure Modes & Detection](#failure-modes--detection)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)
- [Attribution](#attribution)
- [License](#license)

---

## Overview

The **Ralph Wiggum Loop** is an agent design pattern created by [Geoffrey Huntley](https://ghuntley.com/loop/) that turns the biggest weakness of large language models — context window degradation — into a feature.

Instead of letting an AI agent run until its context fills up and it starts making mistakes, the loop:

1. **Starts the agent with a fresh context**
2. **Reads task state from persistent files** (`TASKS.md`, `PROGRESS.md`)
3. **Executes exactly ONE bounded task**
4. **Writes results back to disk**
5. **Terminates and restarts** — fresh context, no accumulated confusion

```
┌──────────────────────────────────────────────────────┐
│                  RALPH WIGGUM LOOP                   │
│                                                      │
│    ┌───────────┐     ┌──────────┐     ┌──────────┐   │
│    │  Read     │────▶│ Execute  │────▶│  Write   │   │
│    │  State    │     │ ONE Task │     │  Results │   │
│    └───────────┘     └──────────┘     └──────────┘   │
│         ▲                                   │        │
│         │           ┌──────────┐            │        │
│         └───────────│  Fresh   │◀───────────┘        │
│                     │  Context │                     │
│                     └──────────┘                     │
│                                                      │
│    State Files (persistent on disk):                 │
│    📋 TASKS.md    📊 PROGRESS.md    ⚙️ LOOP_CONFIG    │
└──────────────────────────────────────────────────────┘
```

This repository provides the pattern as a complete set of **reusable components** for GitHub Copilot CLI and VS Code Agents.

## Why This Works

### The Context Degradation Problem

LLMs have a well-documented performance curve within their context window:

```
Performance
    ▲
100%│████████████
    │            ████
    │                ████
    │                    ████
    │                        ████
    │                            ████  ← "Dumb Zone"
 0% │────────────────────────────────▶ Context Fill %
    0%         40%        70%       100%
```

At ~60-70% context utilization, models enter a "dumb zone" where they:
- Forget earlier instructions
- Contradict previous decisions
- Make increasingly poor choices
- Hallucinate solutions

### The Fresh Context Solution

The Ralph Wiggum Loop sidesteps this entirely:

| Traditional Approach | Ralph Wiggum Loop |
|---------------------|-------------------|
| One long session | Many short sessions |
| Context accumulates until degraded | Context resets every iteration |
| Agent "forgets" early decisions | Agent re-reads decisions from files |
| Errors compound silently | Errors recorded and fed back |
| Hard to resume after crash | Natural crash recovery (state on disk) |
| Unclear progress | Progress tracked in PROGRESS.md |

### Key Insight: Failures Are Data

When an iteration fails, the full error output is written to `PROGRESS.md`. The next iteration reads this and course-corrects. This is the pattern's superpower — errors don't corrupt context, they enrich it.

---

## Architecture

```
ImInDanger/
├── .github/
│   ├── agents/                         # Custom agent profiles
│   │   ├── ralph-wiggum-loop.agent.md  # Main orchestrator
│   │   ├── loop-planner.agent.md       # Task decomposition
│   │   └── loop-reviewer.agent.md      # Health analysis
│   ├── skills/                         # Reusable skills
│   │   ├── loop-runner/                # Main loop execution
│   │   │   ├── SKILL.md
│   │   │   └── run-loop.sh
│   │   ├── task-state-manager/         # State file management
│   │   │   ├── SKILL.md
│   │   │   └── init-state.sh
│   │   ├── convergence-detector/       # Pathology detection
│   │   │   ├── SKILL.md
│   │   │   └── check-convergence.sh
│   │   └── loop-guardrails/            # Safety enforcement
│   │       ├── SKILL.md
│   │       └── check-guardrails.sh
│   ├── instructions/                   # Path-specific instructions
│   │   ├── loop-tasks.instructions.md
│   │   └── loop-progress.instructions.md
│   └── copilot-instructions.md         # Repository-wide instructions
├── templates/                          # Starter templates
│   ├── TASKS.md
│   ├── PROGRESS.md
│   ├── PROMPT.md
│   └── LOOP_CONFIG.md
├── hooks/                              # Git hooks
│   ├── pre-commit
│   ├── post-iteration.sh
│   └── setup-hooks.sh
├── examples/                           # Example configurations
│   ├── test-driven-refactor/
│   │   ├── TASKS.md
│   │   └── README.md
│   └── build-fix/
│       ├── TASKS.md
│       └── README.md
├── AGENTS.md                           # Root agent instructions
└── README.md                           # This file
```

---

## Quick Start

### Option A: Using the `rwl` CLI (Recommended)

```bash
# Clone this repo
git clone https://github.com/mstonis/ImInDanger.git

# Install the CLI
cd ImInDanger && make link
# — or —
bash install.sh

# Navigate to your project and initialize
cd /path/to/your/project
rwl init
```

The `rwl init` wizard will:
- Detect your project type (Node, Go, Python, .NET, Rust, etc.)
- Copy agents, skills, instructions, and templates into your project
- Auto-configure validation commands and allowed paths
- Optionally install git hooks for safety guardrails

Then create your task plan:

```bash
rwl plan          # Interactive task planning wizard
rwl doctor        # Verify everything is set up correctly
rwl status        # View the dashboard
```

### Option B: Manual Setup

Copy the components into your repository:

```bash
# Clone or copy the components
cp -r .github/agents/ /path/to/your/project/.github/agents/
cp -r .github/skills/ /path/to/your/project/.github/skills/
cp -r .github/instructions/ /path/to/your/project/.github/instructions/
cp .github/copilot-instructions.md /path/to/your/project/.github/
cp AGENTS.md /path/to/your/project/
```

### 2. Initialize State Files

```bash
bash .github/skills/task-state-manager/init-state.sh
```

This creates `TASKS.md`, `PROGRESS.md`, and `LOOP_CONFIG.md` from templates.

### 3. Define Your Tasks

Edit `TASKS.md` with your objective and task list:

```markdown
## Objective
Refactor authentication module to use JWT tokens

## Task List

### 1. Create JWT utility module
- **Status:** [ ]
- **Files:** `src/utils/jwt.ts`
- **Description:** Create JWT sign/verify functions using jsonwebtoken
- **Validation:** `npx tsc --noEmit`

### 2. Update auth middleware
- **Status:** [ ]
- **Depends on:** Task 1
- **Files:** `src/middleware/auth.ts`
- **Description:** Replace session-based auth with JWT verification
- **Validation:** `npm test -- --grep "auth"`
```

### 4. Configure the Loop

Edit `LOOP_CONFIG.md`:

```markdown
## Safety Limits
- max_iterations: 15
- timeout_minutes: 10

## Validation Commands
npm test
npm run build

## Allowed Paths
src/**
tests/**
```

### 5. Run the Loop

**Option A — Using the skill script:**

```bash
bash .github/skills/loop-runner/run-loop.sh
```

**Option B — Using Copilot CLI directly:**

```bash
# Single iteration
copilot --agent=ralph-wiggum-loop

# Manual loop
while true; do
  copilot --agent=ralph-wiggum-loop --prompt "Execute one task from TASKS.md"
  # Check if all tasks are done
  grep -q '\[ \]' TASKS.md || break
done
```

**Option C — Using VS Code:**

Open the Copilot Chat panel and type:

```
@ralph-wiggum-loop Execute the next pending task from TASKS.md
```

### 6. Install Safety Hooks (Optional)

```bash
bash hooks/setup-hooks.sh
```

---

## CLI Reference (`rwl`)

The `rwl` CLI is a .NET single-file binary built with [Spectre.Console](https://spectreconsole.net/) for rich terminal UI. It provides interactive wizards, dashboards, and management commands for the Ralph Wiggum Loop.

**Requirements:** .NET 10.0 runtime (for dotnet tool install) or .NET SDK 10.0+ (for building from source).

### Installation

```bash
# Option 1: Install as a global .NET tool (recommended)
dotnet tool install -g rwl

# Option 2: Build & install self-contained binary (no runtime needed)
make install

# Option 3: Build & symlink for development
make link

# Option 4: Direct install script
bash install.sh --prefix /usr/local
```

To update an existing installation:

```bash
dotnet tool update -g rwl
```

To uninstall:

```bash
dotnet tool uninstall -g rwl
```

> **Note:** When installed as a dotnet tool, `rwl` uses embedded resources for templates — no `RWL_HOME` needed.
> If building from source, set `RWL_HOME` to the repo root so `rwl` can find source templates:
>
> ```bash
> export RWL_HOME="/path/to/ImInDanger"
> ```

### Building the NuGet Package

```bash
# Create the .nupkg
make pack

# Install locally from the built package
make install-tool

# Push to NuGet.org
dotnet nuget push src/Rwl/bin/Release/rwl.2.0.0.nupkg \
  -s https://api.nuget.org/v3/index.json -k YOUR_API_KEY
```

### Commands

| Command | Description |
|---------|-------------|
| `rwl init` | Interactive setup wizard — adds loop components to your project |
| `rwl doctor` | Verifies all components are installed and configured correctly |
| `rwl status` | Dashboard showing task progress, iteration history, and config |
| `rwl plan` | Guided task creation — builds a complete TASKS.md with objectives |
| `rwl add-task` | Add a single task to an existing TASKS.md |
| `rwl run` | Start the loop (pass-through to `run-loop.sh`) |
| `rwl run-one` | Execute a single iteration |
| `rwl stop` | Create a stop flag to halt the loop after the current iteration |
| `rwl compact [N]` | Compact PROGRESS.md, keeping last N iterations (default: 5) |
| `rwl reset` | Reset loop state for a fresh run |
| `rwl health` | Run convergence and guardrail checks |
| `rwl --help` | Show usage information |
| `rwl --version` | Print version |

### `rwl init` — Setup Wizard

The init wizard detects your project type and configures everything automatically:

```
$ cd my-node-project
$ rwl init

   ╔═══════════════════════════════════════════════╗
   ║         🚌  Ralph Wiggum Loop  🚌            ║
   ║           "I'm in danger!"                    ║
   ╚═══════════════════════════════════════════════╝

  Detected: node project
  Source:   /path/to/ImInDanger

  Install Mode:
    1) Full install (recommended)
    2) Custom — choose components
    3) Minimal — agents + templates only

  ▸ Choice [1/2/3]: 1
  ▸ Also install git hooks? [y/N]: y

  Installing components...
    ✅ .github/agents/ralph-wiggum-loop.agent.md
    ✅ .github/agents/loop-planner.agent.md
    ✅ .github/agents/loop-reviewer.agent.md
    ✅ .github/skills/loop-runner/
    ...
    ✅ LOOP_CONFIG.md (auto-configured)

  🎉 Done! 15 files installed.
```

### `rwl plan` — Task Planning

Creates a complete TASKS.md interactively:

```bash
$ rwl plan
# Prompts for: objective, success criteria, validation command
# Then loops: add tasks with titles, descriptions, files, dependencies
# Outputs a fully-formed TASKS.md ready for the loop
```

### `rwl status` — Dashboard

```
🚌 Ralph Wiggum Loop — Status Dashboard

Tasks
─────
  [████████████░░░░░░░░░░░░░░░░░░] 40%  (4/10)

  Pending       [ ]  4
  In Progress   [~]  1
  Done          [x]  4
  Failed        [!]  1

Progress
────────
  Iterations: 6  (4✓ 2✗)
  File size:  142 lines

Config
──────
  Max iterations: 25  Timeout: 10m
```

### `rwl doctor` — Health Check

Verifies all components are properly installed:

```
$ rwl doctor

  Git
  ───
    ✅ Git repository

  Agents
  ──────
    ✅ ralph-wiggum-loop
    ✅ loop-planner
    ✅ loop-reviewer

  Skills
  ──────
    ✅ loop-runner (executable ✓)
    ...

  State Files
  ───────────
    ✅ TASKS.md
    ✅ PROGRESS.md
    ✅ LOOP_CONFIG.md

  0 issue(s), 0 warning(s)
```

---

## VS Code Agent Plugin

The Ralph Wiggum Loop ships as a **VS Code Copilot Plugin** that bundles all agents, skills, and hooks into an installable package. Once installed, the agents and skills are available directly in the Copilot Chat interface.

### Plugin Installation

**Prerequisites:**

- VS Code with GitHub Copilot extension
- Enable plugins in VS Code settings:

  ```json
  // .vscode/settings.json
  "chat.plugins.enabled": true
  ```

**Option 1 — Install via Git URL (recommended)**

1. Open VS Code Command Palette (`Cmd+Shift+P` / `Ctrl+Shift+P`)
2. Run: **GitHub Copilot: Install Plugin From Source**
3. Enter: `https://github.com/mstonis/ImInDanger`
4. The plugin will be cloned and registered automatically

**Option 2 — Local development / sideload**

```jsonc
// .vscode/settings.json (or User settings)
"chat.pluginLocations": [
  "/path/to/ImInDanger"
]
```

**Option 3 — Marketplace (github/awesome-copilot)**

```bash
copilot plugin install ralph-wiggum-loop@awesome-copilot
```

### What the Plugin Provides

| Component | Invoke |
|-----------|--------|
| `@ralph-wiggum-loop` agent | `@ralph-wiggum-loop run one iteration` |
| `@loop-planner` agent | `@loop-planner break down my tasks` |
| `@loop-reviewer` agent | `@loop-reviewer check loop health` |
| `loop-runner` skill | `/loop-runner` |
| `task-state-manager` skill | `/task-state-manager` |
| `convergence-detector` skill | `/convergence-detector` |
| `loop-guardrails` skill | `/loop-guardrails` |

#### Agent Handoffs

Agents wire together with one-click handoffs:

- **`@loop-planner`** → after planning, offers "▶️ Start Loop" → hands off to `@ralph-wiggum-loop`
- **`@ralph-wiggum-loop`** → after executing, offers "🔍 Review Loop Health" → hands off to `@loop-reviewer`
- **`@loop-reviewer`** → after reviewing, offers "▶️ Continue Loop" or "📋 Replan Tasks"

### Plugin Hooks

The plugin installs two VS Code lifecycle hooks automatically:

| Hook | Trigger | Effect |
|------|---------|--------|
| `session-start.sh` | Start of every Copilot session | Injects TASKS.md/PROGRESS.md state summary as context |
| `session-stop.sh` | End of every Copilot session | Warns if tasks are still `[~]` in-progress |

**Session Start Output Example:**

```
🔄 Ralph Wiggum Loop — 4/7 tasks done, 1 in-progress, 2 pending. Last recorded: ## Iteration 5 — 2025-01-15T10:30:00Z.
```

This message is injected as a system-level context message so the agent always starts with situational awareness.

**Plugin Manifest:** `.github/plugin/plugin.json`

---

## Components

### Agents

#### `ralph-wiggum-loop` — Main Orchestrator

The core agent profile. Each invocation:
1. Reads `TASKS.md`, `PROGRESS.md`, `LOOP_CONFIG.md`
2. Identifies the next pending or failed task
3. Executes exactly ONE task
4. Validates using the configured commands
5. Updates state files with results

**Invoke:** `@ralph-wiggum-loop`, `/agent ralph-wiggum-loop`, or `copilot --agent=ralph-wiggum-loop`

#### `loop-planner` — Task Decomposition

Converts a high-level objective into a structured task list suitable for the loop pattern.

**Invoke:** `@loop-planner "Refactor the auth module to use dependency injection"`

The planner will:
- Break the objective into bounded, independently-completable tasks
- Order tasks by dependencies
- Add validation commands to each task
- Write the result to `TASKS.md`

#### `loop-reviewer` — Health Analysis

Analyzes loop health by examining `PROGRESS.md` and `TASKS.md`. Detects:
- Oscillation (fix A breaks B, fix B re-breaks A)
- Stagnation (no progress across iterations)
- Metric gaming (tests deleted, assertions weakened)
- Context overflow (state files too large)

**Invoke:** `@loop-reviewer "Analyze the current loop health"`

### Skills

#### `loop-runner`

The main execution skill. Provides a bash script (`run-loop.sh`) that:
- Runs the agent loop with configurable iteration limits
- Loads settings from `LOOP_CONFIG.md`
- Handles stop conditions (all done, stop flag, timeout, oscillation)
- Triggers automatic health reviews
- Logs each iteration

```bash
# Basic usage
bash .github/skills/loop-runner/run-loop.sh

# Custom limits
bash .github/skills/loop-runner/run-loop.sh --max-iterations 30 --timeout 15
```

#### `task-state-manager`

Initializes the persistent state files from templates:

```bash
# Create state files (won't overwrite existing)
bash .github/skills/task-state-manager/init-state.sh

# Force overwrite
bash .github/skills/task-state-manager/init-state.sh --force
```

#### `convergence-detector`

Analyzes loop progress for pathological behaviors:

```bash
# Check all modes
bash .github/skills/convergence-detector/check-convergence.sh --mode all

# Check specific mode
bash .github/skills/convergence-detector/check-convergence.sh --mode oscillation
bash .github/skills/convergence-detector/check-convergence.sh --mode stagnation
bash .github/skills/convergence-detector/check-convergence.sh --mode gaming
```

Exit codes: `0` = healthy, `1` = warning, `2` = critical

#### `loop-guardrails`

Pre-commit safety validation:

```bash
bash .github/skills/loop-guardrails/check-guardrails.sh
```

Checks:
- ✅ Changes are within allowed paths
- ✅ Diff size within limits
- ✅ No test files deleted
- ✅ No test assertions weakened
- ✅ No stop flags present

### Instructions

| File | Scope | Purpose |
|------|-------|---------|
| `.github/copilot-instructions.md` | All files | Repository-wide loop context |
| `AGENTS.md` | Repository root | Primary agent instructions |
| `.github/instructions/loop-tasks.instructions.md` | `TASKS.md` | Task file editing rules |
| `.github/instructions/loop-progress.instructions.md` | `PROGRESS.md` | Progress log rules |

### Templates

| Template | Purpose |
|----------|---------|
| `templates/TASKS.md` | Task list with status markers |
| `templates/PROGRESS.md` | Append-only iteration log |
| `templates/LOOP_CONFIG.md` | Loop configuration and limits |
| `templates/PROMPT.md` | Base prompt for manual usage |

### Hooks

| Hook | Purpose |
|------|---------|
| `hooks/pre-commit` | Blocks commits that violate guardrails |
| `hooks/post-iteration.sh` | Validates state after each iteration |
| `hooks/setup-hooks.sh` | Installs hooks into `.git/hooks/` |

---

## Configuration

### `LOOP_CONFIG.md` Reference

```markdown
## Safety Limits
- max_iterations: 20        # Maximum loop iterations
- timeout_minutes: 10        # Per-iteration timeout
- auto_review_interval: 5    # Run health review every N iterations

## Validation Commands
npm test                      # Commands run after each task
npm run build

## Allowed Paths
src/**                        # Glob patterns for modifiable files
tests/**

## Restricted Paths
.env                          # Files agent must NEVER modify
*.lock
node_modules/**

## Diff Limits
- max_lines_per_iteration: 200   # Maximum lines changed per iteration
- max_files_per_iteration: 10    # Maximum files changed per iteration
```

### State File Markers

| Marker | Meaning |
|--------|---------|
| `[ ]` | Pending — not yet started |
| `[~]` | In progress — currently being worked on |
| `[x]` | Done — completed successfully |
| `[!]` | Failed — attempted but unsuccessful |
| `[STOP]` | Emergency stop — halts the loop immediately |

---

## Usage Examples

### Test-Driven Refactoring

The ideal use case: existing tests define correct behavior, and you're restructuring code.

```bash
# 1. Plan the tasks
copilot --agent=loop-planner --prompt \
  "Refactor UserService to use dependency injection. Tests are in tests/services/"

# 2. Review the generated TASKS.md
cat TASKS.md

# 3. Run the loop
bash .github/skills/loop-runner/run-loop.sh

# 4. Check health if needed
copilot --agent=loop-reviewer --prompt "How is the loop doing?"
```

See [`examples/test-driven-refactor/`](examples/test-driven-refactor/) for a complete example.

### Fixing a Broken Build

When CI is red and you have specific compiler/type errors:

```bash
# 1. Capture current errors
npx tsc --noEmit 2>&1 > .loop-logs/build-errors.txt

# 2. Plan tasks from the errors
copilot --agent=loop-planner --prompt \
  "Fix the TypeScript build errors in .loop-logs/build-errors.txt"

# 3. Run the loop
bash .github/skills/loop-runner/run-loop.sh
```

See [`examples/build-fix/`](examples/build-fix/) for a complete example.

### Large-Scale Cleanup

Removing deprecated APIs, updating import paths across hundreds of files:

```bash
# 1. Generate task list
copilot --agent=loop-planner --prompt \
  "Replace all usage of deprecated api.v1.* with api.v2.* across the codebase"

# 2. Set high iteration limit
# Edit LOOP_CONFIG.md: max_iterations: 100

# 3. Run
bash .github/skills/loop-runner/run-loop.sh
```

---

## Failure Modes & Detection

The loop can go wrong in specific, detectable ways. The `convergence-detector` and `loop-reviewer` watch for these:

### 1. Oscillation 🔄

**What:** Fix A breaks B. Fix B re-breaks A. Infinite loop.

**Detection:** Same task toggling between `[x]` and `[!]` across iterations, or the same error message appearing 3+ times.

**Resolution:** Stop the loop, add explicit instructions or constraints to `TASKS.md`.

### 2. Stagnation 🧊

**What:** No tasks change status across multiple iterations.

**Detection:** No status changes for 3+ consecutive iterations.

**Resolution:** Review `PROGRESS.md` for what the agent is attempting. Simplify the current task or add more context.

### 3. Metric Gaming 🎰

**What:** Agent deletes tests, weakens assertions, adds `@ts-ignore`, or uses `any` types to make checks pass.

**Detection:** Git diff analysis for deleted test files, removed `assert`/`expect` calls, or added suppressions.

**Resolution:** The guardrails pre-commit hook blocks these automatically. If detected in review, revert and add explicit anti-gaming instructions.

### 4. Context Overflow 📦

**What:** `PROGRESS.md` grows so large that reading it consumes too much of the context window.

**Detection:** Progress file exceeds 500 lines.

**Resolution:** Compact `PROGRESS.md` — keep the last 5 iterations and a summary of earlier ones.

### 5. Hallucination Drift 🌫️

**What:** Agent builds on false assumptions that become entrenched in state files.

**Detection:** Manual review reveals statements in `PROGRESS.md` that don't match reality.

**Resolution:** Stop the loop, correct the state files, add explicit constraints.

---

## Best Practices

### ✅ DO

- **Start with tests.** The loop works best when success/failure is objectively measurable.
- **Keep tasks small.** Each task should touch 1-3 files maximum.
- **Be specific in task descriptions.** Write them as if for a developer who has never seen the codebase.
- **Include validation commands.** Every task should have a command that verifies success.
- **Use the planner agent.** Let it decompose objectives — it understands the task size constraints.
- **Review `PROGRESS.md` periodically.** It's your window into what the agent is actually doing.
- **Install the pre-commit hook.** It's your last line of defense against metric gaming.

### ❌ DON'T

- **Don't use it for exploratory design.** The pattern needs clear objectives.
- **Don't skip validation commands.** Without verification, the loop can't self-correct.
- **Don't set iteration limits too high initially.** Start with 10-15 and increase if needed.
- **Don't mix unrelated objectives.** One loop run = one coherent goal.
- **Don't edit `PROGRESS.md`** during a run — it's append-only.
- **Don't use it for ambiguous requirements.** "Make the code better" will not converge.

### When to Use (And When Not To)

| Great Fit ✅ | Poor Fit ❌ |
|-------------|------------|
| Test-driven refactoring | Exploratory prototyping |
| Fixing specific build errors | "Make it faster" (vague) |
| Large-scale code migration | Novel architecture design |
| API version upgrades | Greenfield development |
| Dependency injection retrofitting | UX/UI design decisions |
| Linter/formatter adoption | Requirements gathering |

---

## Troubleshooting

### The loop isn't making progress

1. Check `PROGRESS.md` — what is the agent attempting?
2. Run `bash .github/skills/convergence-detector/check-convergence.sh --mode stagnation`
3. Simplify the current task in `TASKS.md`
4. Add more explicit context or constraints

### The loop is oscillating

1. Run `bash .github/skills/convergence-detector/check-convergence.sh --mode oscillation`
2. Look for the conflicting changes in `PROGRESS.md`
3. Add a constraint to `TASKS.md` that breaks the cycle
4. Consider splitting the task differently

### The agent is gaming metrics

1. Run `bash .github/skills/convergence-detector/check-convergence.sh --mode gaming`
2. Check `git diff` for deleted test files or weakened assertions
3. Revert the gaming changes
4. Add explicit anti-gaming notes to the task description
5. Ensure the pre-commit hook is installed

### State files are too large

1. Compact `PROGRESS.md`: keep summary + last 5 iterations
2. Remove completed tasks from `TASKS.md` (keep a summary count)
3. Run `bash .github/skills/task-state-manager/init-state.sh` to reset if starting fresh

### Loop won't start

1. Verify `TASKS.md` exists and has pending tasks (`[ ]`)
2. Check `LOOP_CONFIG.md` exists
3. Ensure scripts are executable: `chmod +x .github/skills/*//*.sh`
4. Check for a stale `.loop-stop` file: `rm -f .loop-stop`

---

## How It Works — Deep Dive

### The Context Window Problem

Every LLM has a context window — the maximum amount of text it can process at once. As a conversation grows, the model's ability to use information from earlier in the context degrades:

```
Iteration 1:  [System Prompt | Task State | Work] ← clean, focused
Iteration 15: [System Prompt | Turn 1 | Turn 2 | ... | Turn 15 | Work] ← degraded
```

By iteration 15 of a traditional session, the model has accumulated 14 turns of conversation history. It's spending most of its context "remembering" old work instead of focusing on the current task.

### The Fresh Context Fix

The Ralph Wiggum Loop eliminates this entirely:

```
Every iteration: [System Prompt | Read TASKS.md | Read PROGRESS.md | Work] ← always clean
```

The agent reads the same state files every time, but its context is always fresh. There's no accumulated conversation history weighing it down.

### State Machine

```
START ──▶ READ_STATE ──▶ SELECT_TASK ──▶ EXECUTE ──▶ VALIDATE
  ▲                                                      │
  │           ┌──── STOP (all done) ◀──── YES ────┐      │
  │           │                                    │      ▼
  └───────────┴──── WRITE_STATE ◀─── CHECK_DONE ──┘  RECORD
                         │                              ▲
                         └──────────────────────────────┘
```

### Why "Ralph Wiggum"?

The name comes from the Simpsons character Ralph Wiggum. In a famous scene, Ralph sits on a school bus as it drives into danger, cheerfully announcing *"I'm in danger!"* The AI agent, like Ralph, happily enters each iteration without awareness of what happened before — and that naivety is exactly what makes the pattern work. Each iteration is a fresh start, unburdened by accumulated context.

---

## Adding to an Existing Project

The fastest way is `rwl init`:

```bash
cd /path/to/your/project
rwl init        # interactive wizard
rwl plan        # create tasks
rwl doctor      # verify setup
```

### Manual Setup — Minimal

The minimum you need is:

1. `.github/agents/ralph-wiggum-loop.agent.md` — the main agent
2. `TASKS.md` — your task list
3. `PROGRESS.md` — iteration log (can start empty)

Everything else (skills, hooks, templates, guardrails) enhances the pattern but isn't strictly required.

### Manual Setup — Recommended

For production use, also add:

- `.github/agents/loop-reviewer.agent.md` — health monitoring
- `.github/skills/convergence-detector/` — automated pathology detection
- `.github/skills/loop-guardrails/` — safety enforcement
- `hooks/pre-commit` — blocks dangerous commits
- `LOOP_CONFIG.md` — configuration

### Manual Setup — Full

Copy the entire `.github/agents/`, `.github/skills/`, `.github/instructions/` directories plus `AGENTS.md` and the hook scripts. Or just use `rwl init` with the "Full install" option.

---

## Attribution

The **Ralph Wiggum Loop** pattern was created by **[Geoffrey Huntley](https://ghuntley.com/loop/)**. This implementation is inspired by:

- [ghuntley.com/loop](https://ghuntley.com/loop/) — Original pattern description and philosophy
- [beuke.org/ralph-wiggum-loop](https://beuke.org/ralph-wiggum-loop/) — Comprehensive technical reference
- [agentpatterns.ai](https://agentpatterns.ai/agent-design/ralph-wiggum-loop/) — Fresh-Context Iteration Pattern
- [dwmkerr.com](https://dwmkerr.com/ralph-wiggum-loop/) — Real-world usage (451 files, 58K lines)

The canonical loop is beautifully simple:

```bash
while :; do cat PROMPT.md | claude-code; done
```

This project extends that simplicity into a full component system for GitHub Copilot CLI.

---

## License

MIT License. See [LICENSE](LICENSE) for details.

The Ralph Wiggum Loop pattern itself is an open community pattern. This implementation
is an independent adaptation for the GitHub Copilot CLI platform.
