## ADDED Requirements

### Requirement: Web 服务与 Redis 消费职责必须分离

系统 SHALL 将 HTTP 服务与 Redis Stream 消费职责明确拆分，避免在 Web 入口中混合运行后台消费逻辑。

#### Scenario: 启动 HTTP 服务

- **WHEN** 启动 `CdcHttpServer`
- **THEN** 服务仅负责 HTTP 接收、查询接口与自身依赖初始化

#### Scenario: 启动 Redis 消费器

- **WHEN** 启动 `CdcRedisConsumer`
- **THEN** 服务负责 Redis Stream 消费、处理与确认消息
