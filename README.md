# AIAndCDC

以 CDC 为主线的本地演示与开发项目，推荐运行路径为：

- `CdcHttpServer`：HTTP 接收与查询服务
- `CdcWebUIClient`：前端界面
- `CdcRedisConsumer`：独立 Redis Stream 消费服务

## 项目职责

- `CdcHttpServer`
  - 接收 Debezium 通过 HTTP 推送的 CDC 事件，并提供查询接口
- `CdcWebUIClient`
  - 展示 CDC 事件列表、详情与统计信息
- `CdcRedisConsumer`
  - 独立消费 Redis Stream 中的 CDC 消息
- `CdcWebUI`
  - 旧版原型，不再作为推荐入口

## 推荐启动流程

1. 启动 `CdcHttpServer`

```powershell
cd CdcHttpServer
dotnet run
```

2. 启动 `CdcWebUIClient`

```powershell
cd CdcWebUIClient
deno task dev
```

3. 如需 Redis Stream 消费，再启动 `CdcRedisConsumer`

```powershell
cd CdcRedisConsumer
dotnet run
```

## 本地验证

- HTTP 服务健康检查

```powershell
curl http://localhost:8889/health
```

- 事件列表接口

```powershell
curl "http://localhost:8889/api/events?page=1&size=10"
```

## 常见排查

- Debezium HTTP Sink 无法投递
  - 检查 `debezium/application.properties` 中的 `debezium.sink.http.url`
  - 确认 `CdcHttpServer` 已启动并监听 `8889`

- Redis 消费器启动失败
  - 检查 `CdcRedisConsumer/appsettings.json` 或环境变量中的 `Redis:Url` 与 `Redis:StreamKey`
  - 确认 Redis 可连通，且 Stream Key 与 Debezium 真实写入的一致

- 前端没有数据显示
  - 先访问 `http://localhost:8889/api/events?page=1&size=10`
  - 若接口正常，再检查前端开发服务器代理配置

## 自动化验证

新增测试项目：

- `AIAndCDC.Tests`

运行方式：

```powershell
dotnet test AIAndCDC.Tests/AIAndCDC.Tests.csproj
```

说明：

- HTTP 接口测试无需额外依赖
- Redis 消费器集成测试默认连接 `localhost:6379`
