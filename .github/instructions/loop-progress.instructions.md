---
applyTo: "PROGRESS.md"
---

# Progress File Instructions

This is a Ralph Wiggum Loop progress log. When editing this file:

1. **Append only** — never modify or delete existing entries
2. **Use the standard format** for each iteration entry:

```markdown
## Iteration N — [ISO 8601 timestamp]

**Task:** [task description from TASKS.md]
**Status:** ✅ Success | ❌ Failed | ⚠️ Partial | ⏰ Timed out
**Changes:**
- [list files modified/created/deleted]
**Validation:** [test/build/lint results]
**Notes:** [observations for future iterations]
```

3. **Include full error output** when a task fails — this is how the next iteration learns what went wrong
4. **Be concise but complete** — future iterations need enough context to continue
5. **Never edit historical entries** — they are the audit trail
