## Context

AIAndCDC 是一个基于 Debezium 的 Change Data Capture (CDC) 数据变更捕获平台。当前项目包含：

- **3 个 .NET 8.0 项目**：CdcHttpServer、CdcRedisConsumer、CdcWebUI
- **Debezium Server 2.7.4.Final**：CDC 核心引擎
- **多种 Sink 支持**：HTTP、Redis Stream、NATS（已废弃）
- **监控体系**：Prometheus + Grafana

项目元数据分散在多个配置文件中，缺乏统一的项目级配置文档。

## Goals / Non-Goals

**Goals:**
- 创建标准化的 `.openspec.yaml` 项目配置文件
- 完整描述项目模块、依赖关系和技术栈
- 生成清晰的项目结构文档
- 更新 `openspec/config.yaml` 添加项目上下文

**Non-Goals:**
- 不修改任何业务代码
- 不更改项目架构或技术选型
- 不引入新的依赖或功能

## Decisions

### 1. 配置格式选择
**决策**: 使用 YAML 格式创建 `.openspec.yaml`

**理由**:
- YAML 具有良好的可读性和层级表达能力
- 与现有 `openspec/config.yaml` 保持一致
- 支持注释，便于维护

### 2. 模块划分策略
**决策**: 按功能模块划分，每个 .NET 项目作为一个独立模块

**模块定义**:
| 模块名 | 类型 | 描述 |
|--------|------|------|
| cdc-http-server | service | HTTP Webhook 接收服务 |
| cdc-redis-consumer | worker | Redis Stream 消费者 |
| cdc-web-ui | web | Web 监控界面 |
| debezium-server | infrastructure | CDC 核心引擎配置 |

### 3. 依赖关系描述
**决策**: 区分内部模块依赖和外部服务依赖

**内部依赖**: 模块间的调用关系
**外部依赖**: SQL Server、Redis、NATS 等基础设施

### 4. 文档输出位置
**决策**: 
- `.openspec.yaml` → 项目根目录
- 项目结构文档 → `openspec/changes/generate-openspec-config/project-structure.md`
- 更新后的 `config.yaml` → `openspec/config.yaml`

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| 配置与实际代码不同步 | 在文档中注明"以实际代码为准"，定期 review |
| 模块边界模糊 | 参考项目 README 和代码结构，保持与现有认知一致 |
| 依赖版本信息过时 | 从 `.csproj` 和 `docker-compose.yml` 中提取实际版本 |

## Migration Plan

**部署步骤**:
1. 创建 `.openspec.yaml` 文件
2. 更新 `openspec/config.yaml`
3. 生成项目结构文档
4. 验证配置完整性

**回滚策略**:
- 所有变更均为新增文件，可直接删除
- 原始 `config.yaml` 可通过 git 恢复

## Open Questions

- 是否需要定义 `.openspec.yaml` 的 JSON Schema 进行校验？
- 项目版本号如何管理（当前未明确版本）？
