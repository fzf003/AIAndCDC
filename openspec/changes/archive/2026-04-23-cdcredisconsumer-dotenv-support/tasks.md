## 1. 基础设施

- [x] 1.1 创建 `DotEnvLoader.cs`，实现 `.env` 文件解析（支持注释和空行过滤，注入进程环境变量）
- [x] 1.2 创建 `ConfigurationExtensions.cs`，实现 `ResolveEnv(this IConfiguration, string)` 扩展方法（正则替换 `${VAR}` 占位符）

## 2. 集成与适配

- [x] 2.1 修改 `Program.cs`，在 `CreateHostBuilder` 之前调用 `DotEnvLoader.Load()`
- [x] 2.2 修改 `RedisStreamConsumer.cs`，将 `config["Redis:Url"]`、`config["Redis:StreamKey"]`、`config["Redis:ConsumerGroup"]`、`config["Redis:ConsumerName"]` 改为使用 `ResolveEnv()` 扩展方法读取

## 3. 验证

- [x] 3.1 编译 CdcRedisConsumer 项目，确保无编译错误
- [x] 3.2 本地运行测试：验证 `.env` 存在时占位符正确替换（Redis 连接字符串解析为实际值）
- [x] 3.3 验证配置优先级：系统环境变量 > `.env` > `appsettings.json`
- [x] 3.4 验证边界：`.env` 不存在时程序正常启动（依赖 `appsettings.json` 默认值）
