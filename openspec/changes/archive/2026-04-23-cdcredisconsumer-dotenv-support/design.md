## Context

CdcRedisConsumer 使用 .NET Generic Host，配置依赖 `appsettings.json`。当前 `appsettings.json` 使用了 shell 风格的 `${VAR}` 占位符（如 `"${REDIS_HOST}:${REDIS_PORT}"`），但 .NET 的 `IConfiguration` 系统**不支持值内联替换**。同时，项目目录下的 `.env` 文件（含 `REDIS_HOST`、`REDIS_PORT`、`DATABASE_NAME`）在程序启动时不会被自动加载，导致非 Docker 环境下配置解析失败。

## Goals / Non-Goals

**Goals:**
- 程序启动时自动读取 `CdcRedisConsumer/.env` 并注入进程环境变量
- 保留 `appsettings.json` 的 `${VAR}` 占位符语法，通过扩展方法实现运行时替换
- 零第三方依赖，纯手写实现
- 配置优先级：系统环境变量 > `.env` > `appsettings.json` 默认值

**Non-Goals:**
- 不修改 .env 文件格式（保持 `KEY=VALUE` 通用语法）
- 不改其他项目（CdcHttpServer、CdcWebUI 等）
- 不支持复杂 .env 语法（多行值、变量引用 `${OTHER}`）
- 不引入 Docker 支持

## Decisions

**1. 手写 DotEnvLoader 而非引入第三方包**
- 当前 `.env` 只包含简单 `KEY=VALUE` 和注释，20 行代码即可覆盖
- 理由：零依赖、可控、当前格式不需要 `dotenv.net` 的完整功能
- 替代方案：`dotenv.net` NuGet 包（rejected — 增加无必要的依赖）

**2. 保留 appsettings.json 的 ${VAR} 占位符**
- 用户明确表达"appsettings.json 里配置了环境变量"，占位符是意图的一部分
- 理由：直观表达"此处来自环境变量"，便于维护者理解
- 替代方案：改为标准 .NET 层级键（rejected — 改变现有配置习惯，且需要改 .env 为 `Redis__Host` 格式）

**3. 使用 IConfiguration 扩展方法做占位符替换**
- 在消费端（RedisStreamConsumer）读取配置时做替换，而非修改配置提供程序
- 理由：改动范围最小，只影响配置消费点，不侵入 .NET 配置系统
- 替代方案：自定义 ConfigurationProvider（rejected — 过度设计，增加维护成本）

**4. 仅读取项目目录下的 .env**
- 不向上递归查找根目录的 `.env`
- 理由：CdcRedisConsumer 应自给自足，避免和全局配置耦合

## Risks / Trade-offs

- **[Risk] .env 文件不存在时程序静默跳过** → 缓解：依赖 `appsettings.json` 中的默认值，若默认值含未替换占位符则 Redis 连接失败并显式报错
- **[Risk] 系统环境变量与 .env 冲突** → 缓解：系统变量优先级更高，这是预期行为（便于 CI/CD 覆盖）
- **[Trade-off] 占位符语法仅限 ${VAR}** → 接受：当前场景不需要嵌套引用或默认值语法
