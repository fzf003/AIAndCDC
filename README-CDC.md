# CDC (Change Data Capture) 完整配置文档

**最后更新**: 2026-04-05  
**环境**: SQL Server 2019 + Debezium Server 2.7.4.Final  
**支持**: HTTP / Redis Sink（NATS 已废弃）

---

## ⚠️ 重要变更（2026-04-05）

1. **Debezium 升级**: 2.6 → 2.7.4.Final
2. **Sink 变更**: NATS → Redis Stream（官方不维护 NATS Sink）
3. **Redis Stream Key 命名规则**:
   - 实际格式：`<topic.prefix>.<database>.<schema>.<table>`
   - 示例：`product.${DATABASE_NAME}.dbo.Product`
   - **不是** `cdc:dbo.Product`（Redis Stream 不使用 Pub/Sub）

---

## 📋 目录

1. [架构概述](#架构概述)
2. [快速开始](#快速开始)
3. [HTTP Sink 配置](#http-sink-配置)
4. [Redis Sink 配置](#redis-sink-配置)
5. [NATS Sink 配置](#nats-sink-配置)
6. [多表多主题配置](#多表多主题配置)
7. [消费者程序](#消费者程序)
8. [测试指南](#测试指南)
9. [故障排查](#故障排查)

---

## 架构概述

### 数据流

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  SQL Server     │     │  Debezium Server │     │    Sink         │
│  ${SQL_SERVER}  │ ──→ │  (Docker)        │ ──→ │  HTTP/Redis/NATS│
│  :1433          │     │  :8080           │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
       ↓                        ↓                        ↓
  事务日志                 捕获变更                  消费者程序
  (CDC 捕获)                (JSON)                 (C#/.NET/Python)
```

### 三种 Sink 对比

| 特性 | HTTP | Redis | NATS |
|------|------|-------|------|
| **内置支持** | ✅ 是 | ✅ 是 | ❌ 需自定义镜像 |
| **额外依赖** | ❌ 无 | ✅ Redis | ✅ NATS |
| **消息持久化** | ❌ 否 | ✅ 可选 | ✅ JetStream |
| **Pub/Sub** | ❌ 否 | ✅ 是 | ✅ 是 |
| **延迟** | 毫秒级 | 亚毫秒级 | 亚毫秒级 |
| **复杂度** | ⭐ 简单 | ⭐⭐ 中等 | ⭐⭐⭐ 较高 |
| **推荐场景** | 测试/开发 | 生产环境 | 高并发场景 |

---

## 快速开始

### 1. 启动 Debezium

```powershell
cd E:\github\ProActor\demo\demo\CdcService
docker compose up -d debezium
```

### 2. 查看日志

```powershell
docker logs debezium --tail 50 --follow
```

**预期输出**:
```
Starting streaming
Connected metrics set to 'true'
```

### 3. 启动消费者

```powershell
cd CdcHttpServer
& 'C:\Program Files\dotnet\dotnet.exe' run
```

---

## HTTP Sink 配置

### 配置文件

**`debezium/application.properties`**:

```properties
# ======================
# SQL Server 连接
# ======================
debezium.source.connector.class=io.debezium.connector.sqlserver.SqlServerConnector
debezium.source.database.hostname=${SQL_SERVER_HOST}
debezium.source.database.port=1433
debezium.source.database.user=${DATABASE_USER}
debezium.source.database.password=${DATABASE_PASSWORD}
debezium.source.database.names=${DATABASE_NAME}
debezium.source.database.server.name=product
debezium.source.topic.prefix=product
debezium.source.database.encrypt=true
debezium.source.database.trustServerCertificate=true

# 监听表
debezium.source.table.include.list=dbo.Product

# ======================
# Snapshot 配置
# ======================
debezium.source.snapshot.mode=initial
debezium.source.snapshot.locking.mode=none

# ======================
# 持久化（必须）
# ======================
debezium.source.schema.history.internal=io.debezium.storage.file.history.FileSchemaHistory
debezium.source.schema.history.internal.file.filename=/debezium/data/schema_history.dat
debezium.source.offset.storage.file.filename=/debezium/data/offsets.dat
debezium.source.offset.flush.interval.ms=1000

# ======================
# HTTP Sink（核心）
# ======================
debezium.sink.type=http
debezium.sink.http.url=http://host.docker.internal:8889/cdc

# ======================
# JSON 输出
# ======================
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

### C# 消费者

**`CdcHttpServer/Program.cs`**:

```csharp
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:8889");
var app = builder.Build();

Console.WriteLine("=== CDC HTTP Server ===");
Console.WriteLine("Listening on http://*:8889/cdc");
Console.WriteLine();

app.MapPost("/cdc", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📨 {json[..100]}...");
    
    // 解析并处理
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    
    var table = root.GetProperty("source").GetProperty("table").GetString();
    var op = root.GetProperty("op").GetString();
    
    Console.WriteLine($"  Table: {table}, Op: {op}");
    
    return Results.Ok();
});

app.Run();
```

### 运行

```powershell
# 启动消费者
cd CdcHttpServer
& 'C:\Program Files\dotnet\dotnet.exe' run

# 测试 SQL
INSERT INTO dbo.Product (Sku, Price, ProductName) 
VALUES ('TEST-001', 99.99, '测试商品');
```

---

## Redis Sink 配置

### ⚠️ 重要：Redis Stream 机制

Debezium Redis Sink 使用 **Redis Stream**（`XADD`），**不是 Pub/Sub**！

**Stream Key 命名规则**：
```
<topic.prefix>.<database>.<schema>.<table>
```

示例：
- 配置：`topic.prefix=product`, `database=${DATABASE_NAME}`, `table=dbo.Product`
- 实际 Stream Key：`product.${DATABASE_NAME}.dbo.Product`

**验证命令**：
```powershell
# 读取所有消息
redis-cli XRANGE product.${DATABASE_NAME}.dbo.Product - +

# 实时消费（阻塞）
redis-cli XREAD BLOCK 0 STREAMS product.${DATABASE_NAME}.dbo.Product $

# 查看 Stream 信息
redis-cli XINFO STREAM product.${DATABASE_NAME}.dbo.Product
```

**❌ 错误**：`redis-cli SUBSCRIBE ...`（Pub/Sub 收不到消息）

### 前置条件

```powershell
# 启动 Redis（Docker）
docker run -d --name redis -p 6379:6379 redis:7

# 或使用现有 Redis
# 确保 ${REDIS_HOST}:${REDIS_PORT} 可访问
```

### 配置文件

**`debezium/application-redis.properties`**:

```properties
# SQL Server 配置（同上，省略）
debezium.source.connector.class=io.debezium.connector.sqlserver.SqlServerConnector
debezium.source.database.hostname=${SQL_SERVER_HOST}
# ... 其他 SQL Server 配置 ...

# ======================
# Redis Sink（核心）
# ======================
debezium.sink.type=redis
debezium.sink.redis.server.urls=redis://${REDIS_HOST}:${REDIS_PORT}
debezium.sink.redis.topic.prefix=cdc

# 可选：Stream 模式（持久化）
# debezium.sink.redis.stream.key=cdc-stream
# debezium.sink.redis.stream.max.len=10000

# ======================
# JSON 输出
# ======================
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

### 切换配置

```powershell
cd debezium
copy application.properties application-http.properties.bak
copy application-redis.properties application.properties
docker compose restart debezium
```

### C# Redis Stream 消费者（工程级）

**项目结构**：
```
CdcRedisConsumer/
├── Models/
│   └── CdcMessage.cs          # CDC 消息模型
├── Handlers/
│   └── CdcHandlers.cs         # 业务处理器
├── Services/
│   └── RedisStreamConsumer.cs # Stream 消费者服务
├── Program.cs
├── appsettings.json
└── CdcRedisConsumer.csproj
```

**核心代码**（`Services/RedisStreamConsumer.cs`）：
```csharp
/// <summary>
/// Redis Stream 消费者服务
/// </summary>
public class RedisStreamConsumer : BackgroundService
{
    private readonly string _streamKey; // product.${DATABASE_NAME}.dbo.Product
    private readonly string _consumerGroup;
    private readonly string _consumerName;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync("${REDIS_HOST}:${REDIS_PORT}");
        var db = redis.GetDatabase();

        // 确保消费者组存在
        await EnsureConsumerGroupAsync(db);

        // 消费消息
        while (!stoppingToken.IsCancellationRequested)
        {
            // 读取消息（阻塞 5 秒）
            var entries = await db.StreamReadGroupAsync(
                _streamKey,
                _consumerGroup,
                _consumerName,
                StreamPosition.NewMessages,
                count: 10);

            foreach (var entry in entries)
            {
                await ProcessEntryAsync(db, entry, stoppingToken);
            }
        }
    }

    private async Task ProcessEntryAsync(IDatabase db, StreamEntry entry, CancellationToken ct)
    {
        // 解析 JSON payload
        var jsonPayload = entry.Values[1].Value.ToString();
        var message = JsonSerializer.Deserialize<CdcMessage>(jsonPayload);

        // 查找匹配的处理器
        var handler = FindHandler(message.Source.Table);
        await handler.HandleAsync(message, ct);

        // 确认消息（ACK）
        await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
    }
}
```

**配置**（`appsettings.json`）：
```json
{
  "Redis": {
    "Url": "${REDIS_HOST}:${REDIS_PORT}",
    "StreamKey": "product.${DATABASE_NAME}.dbo.Product",
    "ConsumerGroup": "cdc-consumers",
    "ConsumerName": "consumer-1"
  }
}
```

**运行**：
```powershell
cd CdcRedisConsumer
dotnet run
```

**特性**：
- ✅ 使用 Redis Stream（`XREADGROUP`），不是 Pub/Sub
- ✅ 消费者组模式，支持多实例并行消费
- ✅ 消息 ACK 确认，失败自动重试
- ✅ 处理器模式，按表名分发业务逻辑
- ✅ 生产环境可用

---

### 简单测试命令

```powershell
# 查看 Stream 信息
redis-cli XINFO STREAM product.${DATABASE_NAME}.dbo.Product

# 读取所有消息
redis-cli XRANGE product.${DATABASE_NAME}.dbo.Product - +

# 执行 SQL 测试
# INSERT INTO dbo.Product ...
```

---

## NATS Sink 配置

### ⚠️ 重要说明

Debezium Server 2.6 **官方镜像没有内置 NATS Sink**，需要自定义镜像。

### 方案 A: 自定义镜像（生产推荐）

**1. 创建 Dockerfile**

**`Dockerfile-nats`**:
```dockerfile
FROM quay.io/debezium/server:2.6

USER root
RUN microdnf install -y wget && \
    wget -q https://github.com/debezium/debezium-server-nats/releases/download/v2.6.0/debezium-server-nats-2.6.0.tar.gz && \
    tar -xzf debezium-server-nats-2.6.0.tar.gz -C /debezium/lib/ && \
    rm debezium-server-nats-2.6.0.tar.gz && \
    microdnf clean all

USER 1001
```

**2. 构建镜像**

```powershell
docker build -t debezium-nats:2.6 -f Dockerfile-nats .
```

**3. 修改 docker-compose.yml**

```yaml
debezium:
  image: debezium-nats:2.6  # 使用自定义镜像
  # ... 其他配置不变
```

**4. 配置文件**

**`debezium/application-nats.properties`**:

```properties
# SQL Server 配置（省略）
# ...

# ======================
# NATS Sink（核心）
# ======================
debezium.sink.type=nats
debezium.sink.nats.serverUrls=nats://${NATS_HOST}:${NATS_PORT}
debezium.sink.nats.subjectPrefix=cdc
debezium.sink.nats.deliverySubject=cdc.dbo.Product

# ======================
# JSON 输出
# ======================
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

### 方案 B: HTTP + NATS 桥接（简单）

**无需自定义镜像**，用 C# 程序桥接：

```
Debezium (HTTP) → C# Bridge → NATS
```

**`CdcNatsBridge/Program.cs`**:

```csharp
using System.Text;
using NATS.Client;
using NATS.Client.JetStream;

// HTTP 服务器
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:8889");
var app = builder.Build();

// NATS 连接
var natsOpts = ConnectionFactory.GetDefaultOptions();
natsOpts.Url = "nats://${NATS_HOST}:${NATS_PORT}";
using var conn = new ConnectionFactory().CreateConnection(natsOpts);
var js = conn.CreateJetStreamContext();

Console.WriteLine("=== CDC HTTP → NATS Bridge ===");
Console.WriteLine("HTTP: http://*:8889/cdc");
Console.WriteLine("NATS: nats://${NATS_HOST}:${NATS_PORT}");
Console.WriteLine();

app.MapPost("/cdc", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    var data = Encoding.UTF8.GetBytes(json);
    
    // 转发到 NATS
    await js.PublishAsync("cdc.dbo.Product", data);
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ➡️ Forwarded to NATS");
    
    return Results.Ok();
});

app.Run();
```

### C# NATS 消费者

```csharp
using NATS.Client;
using NATS.Client.JetStream;

var opts = ConnectionFactory.GetDefaultOptions();
opts.Url = "nats://${NATS_HOST}:${NATS_PORT}";

using var conn = new ConnectionFactory().CreateConnection(opts);
var js = conn.CreateJetStreamContext();

// 订阅主题
using var sub = js.SubscribeSync("cdc.>");

Console.WriteLine("=== CDC NATS Consumer ===");
Console.WriteLine("Subscribed to: cdc.>");
Console.WriteLine();

while (true)
{
    try
    {
        var msg = sub.NextMessage(5000);
        var json = Encoding.UTF8.GetString(msg.Data);
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📨 {msg.Subject}");
        Console.WriteLine($"  {json[..100]}...");
    }
    catch (NATSTimeoutException) { }
}
```

---

## 多表多主题配置

### 1. Debezium 多表配置

```properties
# 监听多个表
debezium.source.table.include.list=dbo.Product,dbo.Customer,dbo.Order

# 或监听整个 schema
# debezium.source.table.include.list=dbo.*

# 排除系统表
debezium.source.table.exclude.list=dbo.__EFMigrationsHistory,dbo.endpointTable
```

### 2. 多主题映射

#### HTTP Sink（单端点）

所有表发送到同一个端点，通过 JSON 中的 `source.table` 区分：

```properties
debezium.sink.type=http
debezium.sink.http.url=http://host.docker.internal:8889/cdc
```

**JSON 示例**:
```json
{
  "source": { "table": "Product" },
  "op": "c",
  "after": { ... }
}
```

#### Redis Sink（多主题）

```properties
debezium.sink.type=redis
debezium.sink.redis.server.urls=redis://${REDIS_HOST}:${REDIS_PORT}
debezium.sink.redis.topic.prefix=cdc
```

**实际主题**:
- `cdc:dbo.Product`
- `cdc:dbo.Customer`
- `cdc:dbo.Order`

#### NATS Sink（多主题）

```properties
debezium.sink.type=nats
debezium.sink.nats.serverUrls=nats://${NATS_HOST}:${NATS_PORT}
debezium.sink.nats.subjectPrefix=cdc
```

**实际主题**:
- `cdc.dbo.Product`
- `cdc.dbo.Customer`
- `cdc.dbo.Order`

### 3. C# 多表消费者

**`Program.cs`**:

```csharp
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:8889");
var app = builder.Build();

Console.WriteLine("=== CDC Multi-Table Consumer ===");
Console.WriteLine("Tables: Product, Customer, Order");
Console.WriteLine();

app.MapPost("/cdc", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    
    // 获取表名和操作
    string? table = root.GetProperty("source").GetProperty("table").GetString();
    string? op = root.GetProperty("op").GetString();
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📨 {table} - {GetOpIcon(op)}");
    
    // 分发处理
    switch (table)
    {
        case "Product":
            await HandleProduct(root);
            break;
        case "Customer":
            await HandleCustomer(root);
            break;
        case "Order":
            await HandleOrder(root);
            break;
    }
    
    return Results.Ok();
});

async Task HandleProduct(JsonElement root)
{
    var after = root.GetProperty("after");
    if (after.TryGetProperty("Sku", out var sku))
        Console.WriteLine($"  SKU: {sku.GetString()}");
    if (after.TryGetProperty("Price", out var price))
        Console.WriteLine($"  Price: ¥{price.GetDecimal()}");
}

async Task HandleCustomer(JsonElement root)
{
    var after = root.GetProperty("after");
    if (after.TryGetProperty("Name", out var name))
        Console.WriteLine($"  Name: {name.GetString()}");
}

async Task HandleOrder(JsonElement root)
{
    var after = root.GetProperty("after");
    if (after.TryGetProperty("OrderId", out var orderId))
        Console.WriteLine($"  OrderId: {orderId.GetInt32()}");
}

string GetOpIcon(string? op) => op?.ToLower() switch
{
    "c" => "🟢 CREATE",
    "u" => "🟡 UPDATE",
    "d" => "🔴 DELETE",
    "r" => "🔵 READ",
    _ => "⚪"
};

app.Run();
```

### 4. NATS 通配符订阅

```csharp
using NATS.Client;
using NATS.Client.JetStream;

var opts = ConnectionFactory.GetDefaultOptions();
opts.Url = "nats://${NATS_HOST}:${NATS_PORT}";

using var conn = new ConnectionFactory().CreateConnection(opts);
var js = conn.CreateJetStreamContext();

// 订阅所有 cdc.* 主题
using var sub = js.SubscribeSync("cdc.>");

Console.WriteLine("=== CDC NATS Multi-Table Consumer ===");
Console.WriteLine("Subscribed to: cdc.>");
Console.WriteLine();

while (true)
{
    try
    {
        var msg = sub.NextMessage(5000);
        var json = Encoding.UTF8.GetString(msg.Data);
        var subject = msg.Subject; // cdc.dbo.Product
        
        // 从主题提取表名
        var table = subject.Split('.').Last();
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📨 {table}");
        
        // 处理...
    }
    catch (NATSTimeoutException) { }
}
```

---

## 消费者程序

### 项目结构

```
E:\github\ProActor\demo\demo\CdcService\
├── CdcHttpServer/           # HTTP 消费者
│   ├── Program.cs
│   ├── CdcHttpServer.csproj
│   └── README.md
├── CdcRedisConsumer/        # Redis 消费者（示例）
│   └── Program.cs
├── CdcNatsConsumer/         # NATS 消费者（示例）
│   └── Program.cs
└── README-CDC.md            # 本文档
```

### 运行消费者

```powershell
# HTTP
cd CdcHttpServer
& 'C:\Program Files\dotnet\dotnet.exe' run

# Redis（需要 StackExchange.Redis 包）
cd CdcRedisConsumer
& 'C:\Program Files\dotnet\dotnet.exe' run

# NATS（需要 NATS.Client 包）
cd CdcNatsConsumer
& 'C:\Program Files\dotnet\dotnet.exe' run
```

---

## 测试指南

### 1. 准备测试数据

```sql
USE ${DATABASE_NAME};
GO

-- 创建测试表（如果没有）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Product')
BEGIN
    CREATE TABLE dbo.Product (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Sku NVARCHAR(50) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        ProductName NVARCHAR(200),
        CustomerId INT
    );
END
GO
```

### 2. INSERT 测试

```sql
INSERT INTO dbo.Product (Sku, Price, ProductName, CustomerId) 
VALUES ('CDC-TEST-001', 99.99, 'CDC 测试商品', 2);
```

**预期输出**:
```
[22:00:15] 📨 Product - 🟢 CREATE
  SKU: CDC-TEST-001
  Price: ¥99.99
```

### 3. UPDATE 测试

```sql
UPDATE dbo.Product 
SET Price = 199.99 
WHERE Sku = 'CDC-TEST-001';
```

**预期输出**:
```
[22:00:20] 📨 Product - 🟡 UPDATE
  SKU: CDC-TEST-001
  Price: ¥199.99
```

### 4. DELETE 测试

```sql
DELETE FROM dbo.Product 
WHERE Sku = 'CDC-TEST-001';
```

**预期输出**:
```
[22:00:25] 📨 Product - 🔴 DELETE
  SKU: CDC-TEST-001
```

### 5. 批量测试

```sql
-- 批量插入
INSERT INTO dbo.Product (Sku, Price, ProductName) VALUES
('BULK-001', 10.00, '商品 1'),
('BULK-002', 20.00, '商品 2'),
('BULK-003', 30.00, '商品 3');

-- 批量更新
UPDATE dbo.Product 
SET Price = Price * 1.1 
WHERE Sku LIKE 'BULK-%';

-- 批量删除
DELETE FROM dbo.Product 
WHERE Sku LIKE 'BULK-%';
```

---

## 故障排查

### Debezium 没有捕获变更

**1. 检查 CDC 是否启用**

```sql
-- 检查数据库
SELECT name, is_cdc_enabled FROM sys.databases WHERE name = '';

-- 检查表
SELECT name, is_tracked_by_cdc FROM sys.tables WHERE name = 'Product';
```

**2. 启用 CDC**

```sql
USE ${DATABASE_NAME};
EXEC sys.sp_cdc_enable_db;

EXEC sys.sp_cdc_enable_table 
    @source_schema = 'dbo',
    @source_name = 'Product',
    @role_name = NULL;
```

**3. 检查 SQL Agent**

```sql
EXEC master.dbo.xp_servicecontrol 'QueryState', 'SQLAgent';
-- 如果未运行:
EXEC master.dbo.xp_servicecontrol 'Start', 'SQLAgent';
```

### 消费者没有收到消息

**HTTP**:
```powershell
# 检查端口
netstat -ano | findstr :8889

# 测试连通性
curl http://localhost:8889/health

# 检查 Docker 网络
docker exec debezium ping host.docker.internal
```

**Redis**:
```powershell
# 测试连接
redis-cli PING

# 订阅测试
redis-cli SUBSCRIBE cdc:dbo.Product
```

**NATS**:
```powershell
# 检查服务器
nats sub ">"

# 测试发布
nats pub cdc.dbo.Product "test"
```

### 重置 CDC

```powershell
# 停止服务
docker compose down

# 删除持久化数据
docker exec debezium rm -f /debezium/data/offsets.dat /debezium/data/schema_history.dat

# 重启
docker compose up -d
```

---

## 常用命令

```powershell
# 启动所有服务
docker compose up -d

# 停止所有服务
docker compose down

# 重启 Debezium
docker compose restart debezium

# 查看日志
docker logs debezium --tail 100 --follow

# 查看实时事件
Get-Content CdcHttpServer\cdc_events.log -Tail 50 -Wait

# 清理持久化数据
docker compose down
docker exec debezium rm -f /debezium/data/*
docker compose up -d
```

---

## 附录：完整配置模板

### HTTP Sink

```properties
debezium.source.connector.class=io.debezium.connector.sqlserver.SqlServerConnector
debezium.source.database.hostname=${SQL_SERVER_HOST}
debezium.source.database.port=1433
debezium.source.database.user=${DATABASE_USER}
debezium.source.database.password=${DATABASE_PASSWORD}
debezium.source.database.names=${DATABASE_PASSWORD}
debezium.source.database.server.name=product
debezium.source.topic.prefix=product
debezium.source.database.encrypt=true
debezium.source.database.trustServerCertificate=true
debezium.source.table.include.list=dbo.Product
debezium.source.snapshot.mode=initial
debezium.source.snapshot.locking.mode=none
debezium.source.schema.history.internal.file.filename=/debezium/data/schema_history.dat
debezium.source.offset.storage.file.filename=/debezium/data/offsets.dat
debezium.source.offset.flush.interval.ms=1000
debezium.sink.type=http
debezium.sink.http.url=http://host.docker.internal:8889/cdc
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

### Redis Sink

```properties
# ... SQL Server 配置同上 ...
debezium.sink.type=redis
debezium.sink.redis.server.urls=redis://${REDIS_HOST}:${REDIS_PORT}
debezium.sink.redis.topic.prefix=cdc
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

### NATS Sink

```properties
# ... SQL Server 配置同上 ...
debezium.sink.type=nats
debezium.sink.nats.serverUrls=nats://${NATS_HOST}:${NATS_PORT}
debezium.sink.nats.subjectPrefix=cdc
debezium.sink.nats.deliverySubject=cdc.dbo.Product
debezium.format.value=json
debezium.format.key=json
debezium.format.value.schemas.enable=false
debezium.format.key.schemas.enable=false
```

---

*文档生成时间: 2026-03-29 22:24*
