## ADDED Requirements

### Requirement: 核心 CDC 链路必须具备基础自动化验证

系统 SHALL 为关键 CDC 解析、查询接口与 Redis 消费行为提供基础自动化测试。

#### Scenario: 验证 CDC 解析

- **WHEN** 使用代表性 Debezium 事件样本执行测试
- **THEN** 系统应能正确解析关键字段并保持契约稳定

#### Scenario: 验证查询接口

- **WHEN** 执行查询接口测试
- **THEN** 事件列表、事件详情与统计接口均应返回符合契约的结果

#### Scenario: 验证 Redis 消费器行为

- **WHEN** 执行 Redis 消费器测试
- **THEN** 处理器匹配与 ACK 行为应符合预期
