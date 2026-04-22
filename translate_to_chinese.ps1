# OpenSpec Markdown 文件中文翻译脚本
# 此脚本为 .github 和 .qoder 中的所有 .md 文件创建中文版本

$basePath = "d:\HarnessSpecProject\AIAndCDC"

# 定义翻译函数
function Translate-Content {
    param([string]$content, [string]$fileType)
    
    # 保留 YAML frontmatter 的 key，翻译 value
    $content = $content -replace '(?m)^(name:\s*)(.+)$', '${1}$2'
    $content = $content -replace '(?m)^(description:\s*)(.+)$', { 
        $desc = $_.Groups[2].Value
        $translated = switch -Regex ($desc) {
            'Implement tasks from an OpenSpec change' { '执行 OpenSpec 变更中的任务' }
            'Archive a completed change' { '归档已完成的变更' }
            'Enter explore mode' { '进入探索模式' }
            'Guided onboarding' { 'OpenSpec 引导式入门' }
            'Propose a new change' { '提出新的变更提案' }
            'Sync delta specs' { '同步增量规范到主规范' }
            'Verify implementation matches' { '验证实现是否与变更制品匹配' }
            default { $desc }
        }
        "description: $translated"
    }
    
    # 翻译常见标题和术语
    $translations = @{
        '# Steps' = '# 步骤'
        '## Steps' = '## 步骤'
        '**Steps**' = '**步骤**'
        '# Input' = '# 输入'
        '**Input**' = '**输入**'
        '# Output' = '# 输出'
        '**Output**' = '**输出**'
        '## Output' = '## 输出'
        '## Guardrails' = '## 约束规则'
        '**Guardrails**' = '**约束规则**'
        '## The Stance' = '## 工作立场'
        '## What You Might Do' = '## 你可以做什么'
        '## OpenSpec Awareness' = '## OpenSpec 感知'
        '## What You Don''t Have To Do' = '## 你不需要做的事'
        '## Ending Discovery' = '## 结束探索'
        '## Handling Different Entry Points' = '## 处理不同的入口点'
        '## Preflight' = '## 预检'
        '## Phase ' = '## 阶段 '
        '## Command Reference' = '## 命令参考'
        '## What''s Next?' = '## 下一步？'
        '## Graceful Exit Handling' = '## 优雅退出处理'
        '## Context' = '## 上下文'
        '## Goals / Non-Goals' = '## 目标 / 非目标'
        '## Decisions' = '## 决策'
        '## ADDED Requirements' = '## 新增需求'
        '## MODIFIED Requirements' = '## 修改的需求'
        '## REMOVED Requirements' = '## 移除的需求'
        '## RENAMED Requirements' = '## 重命名的需求'
        '### Requirement:' = '### 需求：'
        '#### Scenario:' = '#### 场景：'
        '**WHEN**' = '**当**'
        '**THEN**' = '**则**'
        '**AND**' = '**且**'
        '**Goals:**' = '**目标：**'
        '**Non-Goals:**' = '**非目标：**'
        '## Summary' = '## 摘要'
        '## Verification Report' = '## 验证报告'
        '## Implementation Complete' = '## 实施完成'
        '## Implementation Paused' = '## 实施暂停'
        '## Archive Complete' = '## 归档完成'
        '## Archive Failed' = '## 归档失败'
        '## Specs Synced' = '## 规范已同步'
        '## Task Suggestions' = '## 任务建议'
        '## Quick Exploration' = '## 快速探索'
        '## Creating a Change' = '## 创建变更'
        '## The Proposal' = '## 提案'
        '## Specs' = '## 规范'
        '## Design' = '## 设计'
        '## Tasks' = '## 任务'
        '## Archiving' = '## 归档'
        '## Congratulations!' = '## 恭喜！'
        '## OpenSpec Quick Reference' = '## OpenSpec 快速参考'
        '## Why' = '## 背景'
        '## What Changes' = '## 变更内容'
        '## Capabilities' = '## 能力'
        '### New Capabilities' = '### 新增能力'
        '### Modified Capabilities' = '### 修改的能力'
        '## Impact' = '## 影响'
        '**Important**' = '**重要**'
        '**IMPORTANT**' = '**重要**'
        '**Note**' = '**注意**'
        '**Warning**' = '**警告**'
        '**PAUSE**' = '**暂停**'
        '**EXPLAIN:**' = '**解释：**'
        '**DO:**' = '**执行：**'
        '**SHOW:**' = '**展示：**'
        '## 1\.' = '## 1.'
        '## 2\.' = '## 2.'
        '## 3\.' = '## 3.'
        '## 4\.' = '## 4.'
        '## 5\.' = '## 5.'
        '## 6\.' = '## 6.'
        '## 7\.' = '## 7.'
        '## 8\.' = '## 8.'
        '## 9\.' = '## 9.'
        '## 10\.' = '## 10.'
        '## 11\.' = '## 11.'
        '## Issue Encountered' = '## 遇到问题'
        '**Options:**' = '**选项：**'
        '**Final Assessment:**' = '**最终评估：**'
        '**Verification Heuristics**' = '**验证启发式**'
        '**Graceful Degradation**' = '**优雅降级**'
        '**Output Format**' = '**输出格式**'
        '**Fluid Workflow Integration**' = '**流畅工作流集成**'
        '**Artifact Creation Guidelines**' = '**制品创建指南**'
        '**Delta Spec Format Reference**' = '**增量规范格式参考**'
        '**Key Principle: Intelligent Merging**' = '**关键原则：智能合并**'
        '**Scope Guardrail**' = '**范围约束**'
        '**If nothing found:**' = '**如果未找到：**'
        '**If CLI not installed:**' = '**如果 CLI 未安装：**'
        '**Core workflow:**' = '**核心工作流：**'
        '**Additional commands:**' = '**额外命令：**'
        '**New requirement discovered**' = '**发现新需求**'
        '**Requirement changed**' = '**需求变更**'
        '**Design decision made**' = '**做出设计决策**'
        '**Scope changed**' = '**范围变更**'
        '**New work identified**' = '**识别到新工作**'
        '**Assumption invalidated**' = '**假设被否定**'
        'Where to Capture' = '记录位置'
        'Relevant artifact' = '相关制品'
        'All tasks complete!' = '所有任务完成！'
        'Task complete' = '任务完成'
        'Working on task' = '正在处理任务'
        'CRITICAL' = '严重'
        'WARNING' = '警告'
        'SUGGESTION' = '建议'
        'Must fix before archive' = '归档前必须修复'
        'Should fix' = '应该修复'
        'Nice to fix' = '可选修复'
        'Select the change' = '选择变更'
        'Check status to understand the schema' = '检查状态以了解模式'
        'Get apply instructions' = '获取执行指令'
        'Read context files' = '读取上下文文件'
        'Show current progress' = '显示当前进度'
        'Implement tasks' = '执行任务'
        'On completion or pause, show status' = '完成或暂停时显示状态'
        'If no change name provided, prompt for selection' = '如果未提供变更名称，提示选择'
        'Check artifact completion status' = '检查制品完成状态'
        'Check task completion status' = '检查任务完成状态'
        'Assess delta spec sync state' = '评估增量规范同步状态'
        'Perform the archive' = '执行归档'
        'Display summary' = '显示摘要'
        'Find delta specs' = '查找增量规范'
        'For each delta spec, apply changes to main specs' = '对每个增量规范，应用更改到主规范'
        'Complete task:' = '完成任务：'
        'Mark as done if already implemented' = '如果已实现则标记为完成'
        'Requirement not found:' = '未找到需求：'
        'Implement requirement' = '实现需求'
        'Implementation may diverge from spec:' = '实现可能与规范有偏差：'
        'Review' = '审查'
        'against requirement' = '对照需求'
        'Scenario not covered:' = '场景未覆盖：'
        'Add test or implementation for scenario:' = '为场景添加测试或实现：'
        'Design decision not followed:' = '未遵循设计决策：'
        'Update implementation or revise design.md to match reality' = '更新实现或修改 design.md 以符合实际'
        'Code pattern deviation:' = '代码模式偏差：'
        'Consider following project pattern:' = '考虑遵循项目模式：'
    }
    
    foreach ($key in $translations.Keys) {
        $content = $content -replace [regex]::Escape($key), $translations[$key]
    }
    
    return $content
}

# 获取所有 .md 文件
$mdFiles = Get-ChildItem -Path "$basePath\.github","$basePath\.qoder" -Recurse -Filter "*.md"

$createdFiles = @()

foreach ($file in $mdFiles) {
    # 读取原始内容
    $originalContent = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # 翻译内容
    $translatedContent = Translate-Content -content $originalContent -fileType $file.Extension
    
    # 生成中文文件名
    $zhFileName = $file.FullName -replace '\.md$', '.zh.md'
    
    # 写入中文版本
    $translatedContent | Out-File -FilePath $zhFileName -Encoding UTF8
    
    $createdFiles += $zhFileName
    Write-Host "Created: $zhFileName" -ForegroundColor Green
}

Write-Host "`n=== 翻译完成 ===" -ForegroundColor Cyan
Write-Host "共创建 $($createdFiles.Count) 个中文文件" -ForegroundColor Cyan
Write-Host "`n文件列表：" -ForegroundColor Yellow
$createdFiles | ForEach-Object { Write-Host "  $_" }
