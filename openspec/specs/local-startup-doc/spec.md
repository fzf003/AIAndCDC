## ADDED Requirements

### Requirement: 项目必须提供一套中文为主的推荐启动文档

项目 SHALL 提供面向本地开发与排障的一套中文为主启动文档。

#### Scenario: 新成员首次启动项目

- **WHEN** 新成员阅读项目文档
- **THEN** 能按单一路径完成本地启动与基础验证

#### Scenario: 遇到 CDC 链路问题

- **WHEN** 开发者排查 Debezium、Redis Stream Key 或 API 异常
- **THEN** 文档应提供对应检查步骤与常见问题说明
