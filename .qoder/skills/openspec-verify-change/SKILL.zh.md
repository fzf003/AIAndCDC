---
name: openspec-verify-change
description: Verify implementation matches change artifacts. 当用户想要在归档前验证实现是否完整、正确和一致时使用.
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.3.0"
---

Verify that an implementation matches the change artifacts (specs, tasks, design).

**输入**: Optionally specify a change name. If omitted, check if it can be inferred from conversation context. If vague or ambiguous you MUST prompt for available changes.

**步骤**

1. **如果未提供变更名称，提示选择**

   Run `openspec list --json` to get available changes. Use the **AskUserQuestion tool** to let the user select.

   Show changes that have implementation tasks (tasks artifact exists).
   Include the schema used for each change if available.
   Mark changes with incomplete tasks as "(In Progress)".

   **重要**: Do NOT guess or auto-select a change. Always let the user choose.

2. **检查状态以了解模式**
   ```bash
   openspec status --change "<name>" --json
   ```
   Parse the JSON to understand:
   - `schemaName`: The workflow being used (e.g., "spec-driven")
   - Which artifacts exist for this change

3. **Get the change directory and load artifacts**

   ```bash
   openspec instructions apply --change "<name>" --json
   ```

   This returns the change directory and context files. Read all available artifacts from `contextFiles`.

4. **Initialize verification report structure**

   Create a report structure with three dimensions:
   - **Completeness**: Track tasks and spec coverage
   - **Correctness**: Track requirement implementation and scenario coverage
   - **Coherence**: Track design adherence and pattern consistency

   Each dimension can have 严重, 警告, or 建议 issues.

5. **Verify Completeness**

   **Task Completion**:
   - If tasks.md exists in contextFiles, read it
   - Parse checkboxes: `- [ ]` (incomplete) vs `- [x]` (complete)
   - Count complete vs total tasks
   - If incomplete tasks exist:
     - Add 严重 issue for each incomplete task
     - Recommendation: "完成任务： <description>" or "如果已实现则标记为完成"

   **Spec Coverage**:
   - If delta specs exist in `openspec/changes/<name>/specs/`:
     - Extract all requirements (marked with "### 需求：")
     - For each requirement:
       - Search codebase for keywords related to the requirement
       - Assess if implementation likely exists
     - If requirements appear unimplemented:
       - Add 严重 issue: "未找到需求： <requirement name>"
       - Recommendation: "实现需求 X: <description>"

6. **Verify Correctness**

   **Requirement Implementation Mapping**:
   - For each requirement from delta specs:
     - Search codebase for implementation evidence
     - If found, note file paths and line ranges
     - Assess if implementation matches requirement intent
     - If divergence detected:
       - Add 警告: "实现可能与规范有偏差： <details>"
       - Recommendation: "审查 <file>:<lines> 对照需求 X"

   **Scenario Coverage**:
   - For each scenario in delta specs (marked with "#### 场景："):
     - Check if conditions are handled in code
     - Check if tests exist covering the scenario
     - If scenario appears uncovered:
       - Add 警告: "场景未覆盖： <scenario name>"
       - Recommendation: "为场景添加测试或实现： <description>"

7. **Verify Coherence**

   **Design Adherence**:
   - If design.md exists in contextFiles:
     - Extract key decisions (look for sections like "Decision:", "Approach:", "Architecture:")
     - Verify implementation follows those decisions
     - If contradiction detected:
       - Add 警告: "未遵循设计决策： <decision>"
       - Recommendation: "更新实现或修改 design.md 以符合实际"
   - If no design.md: Skip design adherence check, note "No design.md to verify against"

   **Code Pattern Consistency**:
   - 审查 new code for consistency with project patterns
   - Check file naming, directory structure, coding style
   - If significant deviations found:
     - Add 建议: "代码模式偏差： <details>"
     - Recommendation: "考虑遵循项目模式： <example>"

8. **Generate Verification Report**

   **Summary Scorecard**:
   ```
   ## 验证报告: <change-name>

   ### 摘要
   | Dimension    | Status           |
   |--------------|------------------|
   | Completeness | X/Y tasks, N reqs|
   | Correctness  | M/N reqs covered |
   | Coherence    | Followed/Issues  |
   ```

   **Issues by Priority**:

   1. **严重** (归档前必须修复):
      - Incomplete tasks
      - Missing requirement implementations
      - Each with specific, actionable recommendation

   2. **警告** (应该修复):
      - Spec/design divergences
      - Missing scenario coverage
      - Each with specific recommendation

   3. **建议** (可选修复):
      - Pattern inconsistencies
      - Minor improvements
      - Each with specific recommendation

   **Final Assessment**:
   - If 严重 issues: "X critical issue(s) found. Fix before archiving."
   - If only warnings: "No critical issues. Y warning(s) to consider. Ready for archive (with noted improvements)."
   - If all clear: "All checks passed. Ready for archive."

**验证启发式**

- **Completeness**: Focus on objective checklist items (checkboxes, requirements list)
- **Correctness**: Use keyword search, file path analysis, reasonable inference - don't require perfect certainty
- **Coherence**: Look for glaring inconsistencies, don't nitpick style
- **False Positives**: When uncertain, prefer 建议 over 警告, 警告 over 严重
- **Actionability**: Every issue must have a specific recommendation with file/line references where applicable

**优雅降级**

- If only tasks.md exists: verify task completion only, skip spec/design checks
- If tasks + specs exist: verify completeness and correctness, skip design
- If full artifacts: verify all three dimensions
- Always note which checks were skipped and why

**输出格式**

Use clear markdown with:
- Table for summary scorecard
- Grouped lists for issues (严重/警告/建议)
- Code references in format: `file.ts:123`
- Specific, actionable recommendations
- No vague suggestions like "consider reviewing"
