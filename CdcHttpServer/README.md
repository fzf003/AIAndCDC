# CdcHttpServer

CDC HTTP 接收与查询服务。

## 项目定位

- 推荐主线：`CdcHttpServer + CdcWebUIClient`
- 当前职责：
  - 接收 Debezium 通过 HTTP 推送的 CDC 事件
  - 在内存中维护最近事件
  - 提供查询接口给前端使用
- 不再承担 Redis Stream 消费职责，相关工作已收敛到 `CdcRedisConsumer`

## 启动

```powershell
cd CdcHttpServer
dotnet run
```

默认监听地址：

- `http://localhost:8889`

## 配置

默认配置位于：

- `appsettings.json`
- `appsettings.Development.json`

当前支持的关键配置：

- `Server:Urls`
- `EventStore:Capacity`
- `Diagnostics:LogRawEventPayload`

## 主要接口

- `POST /cdc`
  - 接收 Debezium 推送的 CDC 事件
- `GET /api/events?page=1&size=100`
  - 返回分页事件列表
- `GET /api/events/{id}`
  - 返回单条事件详情
- `GET /api/stats`
  - 返回事件统计信息
- `GET /health`
  - 返回服务状态与关键检查项

## Debezium 配置

请确保 `debezium/application.properties` 中包含：

```properties
debezium.sink.type=http
debezium.sink.http.url=http://host.docker.internal:8889/cdc
```

## 说明

- 当前事件查询使用内存存储，适合本地调试与界面展示。
- Redis Stream 消费请使用独立的 `CdcRedisConsumer` 服务。
