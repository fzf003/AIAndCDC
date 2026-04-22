## Context

当前 `CdcHttpServer` 入口中直接通过 `Task.Run` 启动 Redis 消费逻辑，这使得 Web 服务承担了额外的后台任务管理职责。与此同时，日志直接写文件、错误响应拼接字符串、健康检查只返回简单文本，这些都说明当前实现更偏向原型阶段。

P1 关注的是“运行治理”，即让系统更像一套可维护的服务，而不是继续把临时方案堆在一起。

## Goals / Non-Goals

**Goals**

- Web 服务与后台消费者职责明确。
- 配置来源统一、可覆盖、可说明。
- 日志与错误响应具备统一结构。
- 健康检查可以表达关键依赖状态。

**Non-Goals**

- 本阶段不扩展前端功能。
- 本阶段不引入持久化事件存储。
- 本阶段不删除所有历史实现，只收敛推荐运行方式。

## Decisions

### 1. `CdcHttpServer` 不再负责拉起 Redis 消费

Redis Stream 消费仅由 `CdcRedisConsumer` 承担。

如未来确实需要在 Web 进程内运行后台任务，也必须使用标准 Hosted Service，而不是入口处裸 `Task.Run`。

### 2. 配置集中管理

以下配置统一进入配置体系：

- 监听端口
- EventStore 容量
- Redis URL
- Redis Stream Key
- Consumer Group / Consumer Name
- 原始事件落盘开关与路径

优先顺序：

- `appsettings.json`
- `appsettings.Development.json`
- 环境变量覆盖

### 3. 日志标准化

默认使用 `ILogger` 输出结构化日志。

原始事件落盘仅作为可选诊断能力，不再在请求处理中无条件执行 `File.AppendAllText`。

### 4. 错误响应与健康检查标准化

- API 异常使用统一 Problem Details 风格输出。
- `/health` 至少区分“应用就绪”与“依赖可用性”。

## Risks / Trade-offs

- **风险：职责拆分后本地调试步骤变多**
  - 缓解：配套补齐推荐启动文档。
- **风险：日志结构调整影响旧的排查习惯**
  - 缓解：保留必要关键字段与原始事件可选落盘。
- **权衡：配置体系更规范，但初始接入成本略高**
  - 接受：这是从原型走向稳定服务的必要成本。
