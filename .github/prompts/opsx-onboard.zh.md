---
description: Guided onboarding - walk through a complete OpenSpec workflow cycle with narration
---

Guide the user through their first complete OpenSpec workflow cycle. This is a teaching experience—you'll do real work in their codebase while explaining each step.

---

## 预检

Before starting, check if the OpenSpec CLI is installed:

```bash
# Unix/macOS
openspec --version 2>&1 || echo "CLI_NOT_INSTALLED"
# Windows (PowerShell)
# if (Get-Command openspec -ErrorAction SilentlyContinue) { openspec --version } else { echo "CLI_NOT_INSTALLED" }
```

**如果 CLI 未安装：**
> OpenSpec CLI is not installed. Install it first, then come back to `/opsx:onboard`.

Stop here if not installed.

---

## Phase 1: Welcome

Display:

```
## Welcome to OpenSpec!

I'll walk you through a complete change cycle—from idea to implementation—using a real task in your codebase. Along the way, you'll learn the workflow by doing it.

**What we'll do:**
1. Pick a small, real task in your codebase
2. Explore the problem briefly
3. Create a change (the container for our work)
4. Build the artifacts: proposal → specs → design → tasks
5. Implement the tasks
6. Archive the completed change

**Time:** ~15-20 minutes

Let's start by finding something to work on.
```

---

## Phase 2: Task Selection

### Codebase Analysis

Scan the codebase for small improvement opportunities. Look for:

1. **TODO/FIXME comments** - Search for `TODO`, `FIXME`, `HACK`, `XXX` in code files
2. **Missing error handling** - `catch` blocks that swallow errors, risky operations without try-catch
3. **Functions without tests** - Cross-reference `src/` with test directories
4. **Type issues** - `any` types in TypeScript files (`: any`, `as any`)
5. **Debug artifacts** - `console.log`, `console.debug`, `debugger` statements in non-debug code
6. **Missing validation** - User input handlers without validation

Also check recent git activity:
```bash
# Unix/macOS
git log --oneline -10 2>/dev/null || echo "No git history"
# Windows (PowerShell)
# git log --oneline -10 2>$null; if ($LASTEXITCODE -ne 0) { echo "No git history" }
```

### Present Suggestions

From your analysis, present 3-4 specific suggestions:

```
## 任务建议

Based on scanning your codebase, here are some good starter tasks:

**1. [Most promising task]**
   Location: `src/path/to/file.ts:42`
   Scope: ~1-2 files, ~20-30 lines
   Why it's good: [brief reason]

**2. [Second task]**
   Location: `src/another/file.ts`
   Scope: ~1 file, ~15 lines
   Why it's good: [brief reason]

**3. [Third task]**
   Location: [location]
   Scope: [estimate]
   Why it's good: [brief reason]

**4. Something else?**
   Tell me what you'd like to work on.

Which task interests you? (Pick a number or describe your own)
```

**如果未找到：** Fall back to asking what the user wants to build:
> I didn't find obvious quick wins in your codebase. What's something small you've been meaning to add or fix?

### Scope Guardrail

If the user picks or describes something too large (major feature, multi-day work):

```
That's a valuable task, but it's probably larger than ideal for your first OpenSpec run-through.

For learning the workflow, smaller is better—it lets you see the full cycle without getting stuck in implementation details.

**选项：**
1. **Slice it smaller** - What's the smallest useful piece of [their task]? Maybe just [specific slice]?
2. **Pick something else** - One of the other suggestions, or a different small task?
3. **Do it anyway** - If you really want to tackle this, we can. Just know it'll take longer.

What would you prefer?
```

Let the user override if they insist—this is a soft guardrail.

---

## Phase 3: Explore Demo

Once a task is selected, briefly demonstrate explore mode:

```
Before we create a change, let me quickly show you **explore mode**—it's how you think through problems before committing to a direction.
```

Spend 1-2 minutes investigating the relevant code:
- Read the file(s) involved
- Draw a quick ASCII diagram if it helps
- Note any considerations

```
## 快速探索

[Your brief analysis—what you found, any considerations]

┌─────────────────────────────────────────┐
│   [Optional: ASCII diagram if helpful]  │
└─────────────────────────────────────────┘

Explore mode (`/opsx:explore`) is for this kind of thinking—investigating before implementing. You can use it anytime you need to think through a problem.

Now let's create a change to hold our work.
```

**暂停** - Wait for user acknowledgment before proceeding.

---

## Phase 4: Create the Change

**解释：**
```
## 创建变更

A "change" in OpenSpec is a container for all the thinking and planning around a piece of work. It lives in `openspec/changes/<name>/` and holds your artifacts—proposal, specs, design, tasks.

Let me create one for our task.
```

**执行：** Create the change with a derived kebab-case name:
```bash
openspec new change "<derived-name>"
```

**展示：**
```
Created: `openspec/changes/<name>/`

The folder structure:
```
openspec/changes/<name>/
├── proposal.md    ← Why we're doing this (empty, we'll fill it)
├── design.md      ← How we'll build it (empty)
├── specs/         ← Detailed requirements (empty)
└── tasks.md       ← Implementation checklist (empty)
```

Now let's fill in the first artifact—the proposal.
```

---

## Phase 5: Proposal

**解释：**
```
## 提案

The proposal captures **why** we're making this change and **what** it involves at a high level. It's the "elevator pitch" for the work.

I'll draft one based on our task.
```

**执行：** Draft the proposal content (don't save yet):

```
Here's a draft proposal:

---

## 背景

[1-2 sentences explaining the problem/opportunity]

## 变更内容

[Bullet points of what will be different]

## 能力

### 新增能力
- `<capability-name>`: [brief description]

### 修改的能力
<!-- If modifying existing behavior -->

## 影响

- `src/path/to/file.ts`: [what changes]
- [other files if applicable]

---

Does this capture the intent? I can adjust before we save it.
```

**暂停** - Wait for user approval/feedback.

After approval, save the proposal:
```bash
openspec instructions proposal --change "<name>" --json
```
Then write the content to `openspec/changes/<name>/proposal.md`.

```
Proposal saved. This is your "why" document—you can always come back and refine it as understanding evolves.

Next up: specs.
```

---

## Phase 6: Specs

**解释：**
```
## 规范

Specs define **what** we're building in precise, testable terms. They use a requirement/scenario format that makes expected behavior crystal clear.

For a small task like this, we might only need one spec file.
```

**执行：** Create the spec file:
```bash
# Unix/macOS
mkdir -p openspec/changes/<name>/specs/<capability-name>
# Windows (PowerShell)
# New-Item -ItemType Directory -Force -Path "openspec/changes/<name>/specs/<capability-name>"
```

Draft the spec content:

```
Here's the spec:

---

## 新增需求

### 需求： <Name>

<Description of what the system should do>

#### 场景： <Scenario name>

- **当** <trigger condition>
- **则** <expected outcome>
- **且** <additional outcome if needed>

---

This format—WHEN/THEN/AND—makes requirements testable. You can literally read them as test cases.
```

Save to `openspec/changes/<name>/specs/<capability>/spec.md`.

---

## Phase 7: Design

**解释：**
```
## 设计

The design captures **how** we'll build it—technical decisions, tradeoffs, approach.

For small changes, this might be brief. That's fine—not every change needs deep design discussion.
```

**执行：** Draft design.md:

```
Here's the design:

---

## 上下文

[Brief context about the current state]

## 目标 / 非目标

**目标：**
- [What we're trying to achieve]

**非目标：**
- [What's explicitly out of scope]

## 决策

### Decision 1: [Key decision]

[Explanation of approach and rationale]

---

For a small task, this captures the key decisions without over-engineering.
```

Save to `openspec/changes/<name>/design.md`.

---

## Phase 8: Tasks

**解释：**
```
## 任务

Finally, we break the work into implementation tasks—checkboxes that drive the apply phase.

These should be small, clear, and in logical order.
```

**执行：** Generate tasks based on specs and design:

```
Here are the implementation tasks:

---

## 1. [Category or file]

- [ ] 1.1 [Specific task]
- [ ] 1.2 [Specific task]

## 2. Verify

- [ ] 2.1 [Verification step]

---

Each checkbox becomes a unit of work in the apply phase. Ready to implement?
```

**暂停** - Wait for user to confirm they're ready to implement.

Save to `openspec/changes/<name>/tasks.md`.

---

## Phase 9: Apply (Implementation)

**解释：**
```
## Implementation

Now we implement each task, checking them off as we go. I'll announce each one and occasionally note how the specs/design informed the approach.
```

**执行：** For each task:

1. Announce: "正在处理任务 N: [description]"
2. Implement the change in the codebase
3. Reference specs/design naturally: "The spec says X, so I'm doing Y"
4. Mark complete in tasks.md: `- [ ]` → `- [x]`
5. Brief status: "✓ Task N complete"

Keep narration light—don't over-explain every line of code.

After all tasks:

```
## 实施完成

All tasks done:
- [x] Task 1
- [x] Task 2
- [x] ...

The change is implemented! One more step—let's archive it.
```

---

## Phase 10: Archive

**解释：**
```
## 归档

When a change is complete, we archive it. This moves it from `openspec/changes/` to `openspec/changes/archive/YYYY-MM-DD-<name>/`.

Archived changes become your project's decision history—you can always find them later to understand why something was built a certain way.
```

**执行：**
```bash
openspec archive "<name>"
```

**展示：**
```
Archived to: `openspec/changes/archive/YYYY-MM-DD-<name>/`

The change is now part of your project's history. The code is in your codebase, the decision record is preserved.
```

---

## Phase 11: Recap & Next Steps

```
## 恭喜！

You just completed a full OpenSpec cycle:

1. **Explore** - Thought through the problem
2. **New** - Created a change container
3. **Proposal** - Captured WHY
4. **Specs** - Defined WHAT in detail
5. **Design** - Decided HOW
6. **Tasks** - Broke it into steps
7. **Apply** - Implemented the work
8. **Archive** - Preserved the record

This same rhythm works for any size change—a small fix or a major feature.

---

## 命令参考

**核心工作流：**

 | Command           | What it does                               |
 |-------------------|--------------------------------------------|
 | `/opsx:propose` | Create a change and generate all artifacts |
 | `/opsx:explore` | Think through problems before/during work  |
 | `/opsx:apply`   | 执行任务 from a change              |
 | `/opsx:archive` | Archive a completed change                 |

**额外命令：**

 | Command            | What it does                                             |
 |--------------------|----------------------------------------------------------|
 | `/opsx:new`      | Start a new change, step through artifacts one at a time |
 | `/opsx:continue` | Continue working on an existing change                   |
 | `/opsx:ff`       | Fast-forward: create all artifacts at once               |
 | `/opsx:verify`   | Verify implementation matches artifacts                  |

---

## 下一步？

Try `/opsx:propose` on something you actually want to build. You've got the rhythm now!
```

---

## 优雅退出处理

### User wants to stop mid-way

If the user says they need to stop, want to pause, or seem disengaged:

```
No problem! Your change is saved at `openspec/changes/<name>/`.

To pick up where we left off later:
- `/opsx:continue <name>` - Resume artifact creation
- `/opsx:apply <name>` - Jump to implementation (if tasks exist)

The work won't be lost. Come back whenever you're ready.
```

Exit gracefully without pressure.

### User just wants command reference

If the user says they just want to see the commands or skip the tutorial:

```
## OpenSpec 快速参考

**核心工作流：**

 | Command                  | What it does                               |
 |--------------------------|--------------------------------------------|
 | `/opsx:propose <name>` | Create a change and generate all artifacts |
 | `/opsx:explore`        | Think through problems (no code changes)   |
 | `/opsx:apply <name>`   | 执行任务                            |
 | `/opsx:archive <name>` | Archive when done                          |

**额外命令：**

 | Command                   | What it does                        |
 |---------------------------|-------------------------------------|
 | `/opsx:new <name>`      | Start a new change, step by step    |
 | `/opsx:continue <name>` | Continue an existing change         |
 | `/opsx:ff <name>`       | Fast-forward: all artifacts at once |
 | `/opsx:verify <name>`   | Verify implementation               |

Try `/opsx:propose` to start your first change.
```

Exit gracefully.

---

## 约束规则

- **Follow the EXPLAIN → DO → SHOW → PAUSE pattern** at key transitions (after explore, after proposal draft, after tasks, after archive)
- **Keep narration light** during implementation—teach without lecturing
- **Don't skip phases** even if the change is small—the goal is teaching the workflow
- **Pause for acknowledgment** at marked points, but don't over-pause
- **Handle exits gracefully**—never pressure the user to continue
- **Use real codebase tasks**—don't simulate or use fake examples
- **Adjust scope gently**—guide toward smaller tasks but respect user choice
