#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
OpenSpec Markdown 文件中文翻译脚本
为 .github 和 .qoder 中的所有 .md 文件创建中文版本
"""

import os
import re
from pathlib import Path

# 基础路径
base_path = Path("d:/HarnessSpecProject/AIAndCDC")

# 翻译映射表
translations = {
    # 标题和章节
    "# Steps": "# 步骤",
    "## Steps": "## 步骤",
    "**Steps**": "**步骤**",
    "# Input": "# 输入",
    "**Input**": "**输入**",
    "# Output": "# 输出",
    "**Output**": "**输出**",
    "## Output": "## 输出",
    "## Guardrails": "## 约束规则",
    "**Guardrails**": "**约束规则**",
    "## The Stance": "## 工作立场",
    "## What You Might Do": "## 你可以做什么",
    "## OpenSpec Awareness": "## OpenSpec 感知",
    "## What You Don't Have To Do": "## 你不需要做的事",
    "## Ending Discovery": "## 结束探索",
    "## Handling Different Entry Points": "## 处理不同的入口点",
    "## Preflight": "## 预检",
    "## Command Reference": "## 命令参考",
    "## What's Next?": "## 下一步？",
    "## Graceful Exit Handling": "## 优雅退出处理",
    "## Context": "## 上下文",
    "## Goals / Non-Goals": "## 目标 / 非目标",
    "## Decisions": "## 决策",
    "## ADDED Requirements": "## 新增需求",
    "## MODIFIED Requirements": "## 修改的需求",
    "## REMOVED Requirements": "## 移除的需求",
    "## RENAMED Requirements": "## 重命名的需求",
    "### Requirement:": "### 需求：",
    "#### Scenario:": "#### 场景：",
    "**WHEN**": "**当**",
    "**THEN**": "**则**",
    "**AND**": "**且**",
    "**Goals:**": "**目标：**",
    "**Non-Goals:**": "**非目标：**",
    "## Summary": "## 摘要",
    "## Verification Report": "## 验证报告",
    "## Implementation Complete": "## 实施完成",
    "## Implementation Paused": "## 实施暂停",
    "## Archive Complete": "## 归档完成",
    "## Archive Failed": "## 归档失败",
    "## Specs Synced": "## 规范已同步",
    "## Task Suggestions": "## 任务建议",
    "## Quick Exploration": "## 快速探索",
    "## Creating a Change": "## 创建变更",
    "## The Proposal": "## 提案",
    "## Specs": "## 规范",
    "## Design": "## 设计",
    "## Tasks": "## 任务",
    "## Archiving": "## 归档",
    "## Congratulations!": "## 恭喜！",
    "## OpenSpec Quick Reference": "## OpenSpec 快速参考",
    "## Why": "## 背景",
    "## What Changes": "## 变更内容",
    "## Capabilities": "## 能力",
    "### New Capabilities": "### 新增能力",
    "### Modified Capabilities": "### 修改的能力",
    "## Impact": "## 影响",
    "## Issue Encountered": "## 遇到问题",
    
    # 强调和标记
    "**Important**": "**重要**",
    "**IMPORTANT**": "**重要**",
    "**IMPORTANT:**": "**重要：**",
    "**Note**": "**注意**",
    "**Warning**": "**警告**",
    "**PAUSE**": "**暂停**",
    "**EXPLAIN:**": "**解释：**",
    "**DO:**": "**执行：**",
    "**SHOW:**": "**展示：**",
    "**Options:**": "**选项：**",
    "**Final Assessment:**": "**最终评估：**",
    "**Verification Heuristics**": "**验证启发式**",
    "**Graceful Degradation**": "**优雅降级**",
    "**Output Format**": "**输出格式**",
    "**Fluid Workflow Integration**": "**流畅工作流集成**",
    "**Artifact Creation Guidelines**": "**制品创建指南**",
    "**Delta Spec Format Reference**": "**增量规范格式参考**",
    "**Key Principle: Intelligent Merging**": "**关键原则：智能合并**",
    "**Scope Guardrail**": "**范围约束**",
    "**If nothing found:**": "**如果未找到：**",
    "**If CLI not installed:**": "**如果 CLI 未安装：**",
    "**Core workflow:**": "**核心工作流：**",
    "**Additional commands:**": "**额外命令：**",
    
    # 常见术语
    "All tasks complete!": "所有任务完成！",
    "Task complete": "任务完成",
    "Working on task": "正在处理任务",
    "CRITICAL": "严重",
    "WARNING": "警告",
    "SUGGESTION": "建议",
    "Must fix before archive": "归档前必须修复",
    "Should fix": "应该修复",
    "Nice to fix": "可选修复",
    "Where to Capture": "记录位置",
    "Relevant artifact": "相关制品",
    
    # 步骤描述
    "Select the change": "选择变更",
    "Check status to understand the schema": "检查状态以了解模式",
    "Get apply instructions": "获取执行指令",
    "Read context files": "读取上下文文件",
    "Show current progress": "显示当前进度",
    "Implement tasks": "执行任务",
    "On completion or pause, show status": "完成或暂停时显示状态",
    "If no change name provided, prompt for selection": "如果未提供变更名称，提示选择",
    "Check artifact completion status": "检查制品完成状态",
    "Check task completion status": "检查任务完成状态",
    "Assess delta spec sync state": "评估增量规范同步状态",
    "Perform the archive": "执行归档",
    "Display summary": "显示摘要",
    "Find delta specs": "查找增量规范",
    "For each delta spec, apply changes to main specs": "对每个增量规范，应用更改到主规范",
    
    # 验证相关
    "Complete task:": "完成任务：",
    "Mark as done if already implemented": "如果已实现则标记为完成",
    "Requirement not found:": "未找到需求：",
    "Implement requirement": "实现需求",
    "Implementation may diverge from spec:": "实现可能与规范有偏差：",
    "Review": "审查",
    "against requirement": "对照需求",
    "Scenario not covered:": "场景未覆盖：",
    "Add test or implementation for scenario:": "为场景添加测试或实现：",
    "Design decision not followed:": "未遵循设计决策：",
    "Update implementation or revise design.md to match reality": "更新实现或修改 design.md 以符合实际",
    "Code pattern deviation:": "代码模式偏差：",
    "Consider following project pattern:": "考虑遵循项目模式：",
}

def translate_content(content):
    """翻译内容中的常见术语和标题"""
    result = content
    for eng, chn in translations.items():
        result = result.replace(eng, chn)
    return result

def translate_description(content):
    """翻译 YAML frontmatter 中的 description"""
    # 翻译常见描述
    desc_translations = {
        "Implement tasks from an OpenSpec change": "执行 OpenSpec 变更中的任务",
        "Archive a completed change in the experimental workflow": "在实验性工作流中归档已完成的变更",
        "Enter explore mode - a thinking partner for exploring ideas, investigating problems, and clarifying requirements": 
            "进入探索模式 - 作为思考伙伴帮助探索想法、调查问题和澄清需求",
        "Guided onboarding for OpenSpec - walk through a complete workflow cycle with narration and real codebase work": 
            "OpenSpec 引导式入门 - 通过讲解和真实代码库工作完成完整的工作流周期",
        "Propose a new change - create it and generate all artifacts in one step": 
            "提出新的变更提案 - 一步创建变更并生成所有制品",
        "Sync delta specs from a change to main specs": "将变更的增量规范同步到主规范",
        "Verify implementation matches change artifacts before archiving": "归档前验证实现是否与变更制品匹配",
        "Use when the user wants to start implementing, continue implementation, or work through tasks": 
            "当用户想要开始实施、继续实施或处理任务时使用",
        "Use when the user wants to finalize and archive a change after implementation is complete": 
            "当用户想要在实施完成后最终确定并归档变更时使用",
        "Use when the user wants to think through something before or during a change": 
            "当用户想要在变更之前或期间深入思考某事时使用",
        "Use when the user wants to walk through a complete workflow cycle with narration": 
            "当用户想要通过讲解完成完整的工作流周期时使用",
        "Use when the user wants to quickly describe what they want to build and get a complete proposal": 
            "当用户想要快速描述他们想要构建的内容并获得完整提案时使用",
        "Use when the user wants to update main specs with changes from a delta spec, without archiving the change": 
            "当用户想要在不归档变更的情况下将增量规范的更改更新到主规范时使用",
        "Use when the user wants to validate that implementation is complete, correct, and coherent before archiving": 
            "当用户想要在归档前验证实现是否完整、正确和一致时使用",
    }
    
    for eng, chn in desc_translations.items():
        content = content.replace(eng, chn)
    return content

def main():
    created_files = []
    
    # 遍历 .github 和 .qoder 目录
    for subdir in [".github", ".qoder"]:
        dir_path = base_path / subdir
        if not dir_path.exists():
            continue
            
        # 递归查找所有 .md 文件
        for md_file in dir_path.rglob("*.md"):
            # 跳过已存在的中文版本
            if md_file.name.endswith(".zh.md"):
                continue
                
            print(f"Processing: {md_file}")
            
            # 读取原始内容
            try:
                content = md_file.read_text(encoding='utf-8')
            except Exception as e:
                print(f"  Error reading {md_file}: {e}")
                continue
            
            # 翻译内容
            translated = translate_content(content)
            translated = translate_description(translated)
            
            # 生成中文文件名
            zh_file = md_file.with_suffix('').with_suffix('.zh.md')
            
            # 写入中文版本
            try:
                zh_file.write_text(translated, encoding='utf-8')
                created_files.append(str(zh_file))
                print(f"  Created: {zh_file}")
            except Exception as e:
                print(f"  Error writing {zh_file}: {e}")
    
    # 输出总结
    print("\n" + "="*50)
    print(f"翻译完成！共创建 {len(created_files)} 个中文文件")
    print("="*50)
    print("\n文件列表：")
    for f in created_files:
        print(f"  {f}")

if __name__ == "__main__":
    main()
