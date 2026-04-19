## Why

当前 AIAndCDC 项目缺乏完整的 OpenSpec 配置文档，导致项目结构、模块依赖和技术栈信息分散在各个文件中。通过生成完整的 `.openspec.yaml` 配置和项目结构文档，可以：

1. **统一项目认知** - 为开发者和 AI 助手提供清晰的项目全景视图
2. **标准化项目元数据** - 建立规范的项目描述、模块划分和依赖关系
3. **提升协作效率** - 使新成员能快速理解项目架构和技术选型

## What Changes

- 创建完整的 `.openspec.yaml` 配置文件，包含：
  - 项目基本信息（名称、描述、版本）
  - 详细的模块定义（3 个 .NET 项目 + Debezium 配置）
  - 完整的技术栈和依赖关系
- 生成项目结构文档，展示目录层级和文件用途
- 更新 `openspec/config.yaml` 添加项目上下文

## Capabilities

### New Capabilities
- `openspec-project-config`: 完整的 OpenSpec 项目配置定义，包括项目元数据、模块结构和依赖关系
- `project-structure-doc`: 项目结构文档，详细描述每个目录和文件的用途

### Modified Capabilities
- （无现有 spec 需要修改）

## Impact

- **openspec/config.yaml** - 更新项目上下文配置
- **openspec/.openspec.yaml** - 新增项目级 OpenSpec 配置（如需要）
- **文档输出** - 生成项目结构说明文档
- **无代码变更** - 本次变更仅涉及配置和文档，不影响运行时行为
