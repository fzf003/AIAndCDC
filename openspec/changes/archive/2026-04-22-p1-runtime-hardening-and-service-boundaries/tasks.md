## 1. 服务职责拆分

- [x] 1.1 从 `CdcHttpServer` 入口中移除直接启动 Redis 消费的逻辑
- [x] 1.2 确认 `CdcRedisConsumer` 作为唯一 Redis Stream 消费实现
- [x] 1.3 如存在必要后台任务，改为 Hosted Service 方式实现

## 2. 配置治理

- [x] 2.1 统一端口、Redis、EventStore、日志等配置项来源
- [x] 2.2 补充示例配置文件或开发环境配置文件
- [x] 2.3 清理代码与文档中的机器专属硬编码路径

## 3. 观测与错误处理

- [x] 3.1 用 `ILogger` 替换直接文件追加写入
- [x] 3.2 统一 API 错误响应为结构化输出
- [x] 3.3 改造 `/health`，使其能反映核心依赖状态

## 4. 验证

- [x] 4.1 验证 `CdcHttpServer` 可独立启动并提供查询接口
- [x] 4.2 验证 `CdcRedisConsumer` 可独立消费消息
- [x] 4.3 验证配置覆盖、日志输出和健康检查行为正常
