## Why

在 P0 完成主线和契约统一后，项目仍存在运行治理层面的明显问题：

- Web 服务与 Redis 消费职责耦合在同一进程入口。
- 配置来源分散且存在硬编码。
- 日志与错误响应缺乏统一规范。
- 健康检查无法真实反映依赖状态。

这些问题不一定立刻阻止开发，但会在联调、部署、排障时持续放大成本。

## What Changes

- 拆清 `CdcHttpServer` 与 `CdcRedisConsumer` 的运行职责。
- 将关键运行参数统一纳入配置体系。
- 用结构化日志替换临时文件追加写入。
- 统一 API 错误响应与健康检查输出。

## Dependencies

本变更依赖：

- `p0-unify-api-and-ui-mainline`

因为 P1 需要建立在稳定主线和稳定接口之上，避免重复收口。

## Capabilities

### New Capabilities

- `cdc-service-boundary`: 明确运行时职责边界
- `runtime-config-and-observability`: 统一配置、日志、错误响应与健康检查

## Impact

- **CdcHttpServer**: 从“混合运行器”收敛为明确的 HTTP 服务。
- **CdcRedisConsumer**: 作为独立后台消费者承担 Redis Stream 消费职责。
- **运行配置**: 进入 `appsettings` 与环境变量覆盖体系。
- **日志与健康检查**: 更适合后续部署与排障。
