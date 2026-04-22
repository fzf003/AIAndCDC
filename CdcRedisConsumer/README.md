# CdcRedisConsumer

Redis Stream CDC 消费服务。

## 项目定位

- 当前职责：作为唯一的 Redis Stream 消费实现，独立处理 CDC 消息。
- 推荐运行方式：与 `CdcHttpServer` 分开启动，各自承担单一职责。
- 文档约定：中文为主，文本文件统一使用 UTF-8。

## 配置

复制 `appsettings.example.json` 为本地 `appsettings.json`，或通过环境变量覆盖：

- `Redis:Url`
- `Redis:StreamKey`
- `Redis:ConsumerGroup`
- `Redis:ConsumerName`

## 启动

```powershell
cd CdcRedisConsumer
dotnet run
```

如果未配置 Redis 或 Stream Key，服务会在启动阶段明确报错。
