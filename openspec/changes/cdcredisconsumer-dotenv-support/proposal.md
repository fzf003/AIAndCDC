## Why

CdcRedisConsumer 当前依赖 `appsettings.json` 中的 `${VAR}` 占位符来引用环境变量，但程序启动时不会自动加载 `.env` 文件。这导致在非 Docker 环境下运行时，Redis 连接字符串等配置无法正确解析，消费者启动失败。需要让程序在启动时自动读取项目目录下的 `.env` 文件，将环境变量注入配置系统，实现无 Docker 的本地运行支持。

## What Changes

- 新增 `DotEnvLoader` 类，自动解析 `.env` 文件并注入进程环境变量（不引入第三方包）
- 新增 `ConfigurationExtensions` 扩展方法，支持 `appsettings.json` 中的 `${VAR}` 占位符替换
- 修改 `Program.cs`，在 Host 启动前调用 `.env` 加载
- 修改 `RedisStreamConsumer.cs`，使用扩展方法读取带占位符的配置值
- `appsettings.json` 保持现有 `${VAR}` 模板语法不变

## Capabilities

### New Capabilities
- `cdcredisconsumer-dotenv-support`: CdcRedisConsumer 的 .env 文件自动加载与配置占位符解析能力

### Modified Capabilities
- 无现有 spec 需要修改

## Impact

- **CdcRedisConsumer/Program.cs**: 启动时增加 `.env` 加载调用
- **CdcRedisConsumer/RedisStreamConsumer.cs**: 配置读取方式改为占位符解析
- **新增文件**: `DotEnvLoader.cs`、`ConfigurationExtensions.cs`
- **无外部依赖变更**: 纯手写实现，零第三方 NuGet 包
- **仅影响 CdcRedisConsumer 项目**，不波及其他服务
