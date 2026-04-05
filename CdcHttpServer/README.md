# CDC HTTP Server

Debezium CDC 事件接收器 - 通过 HTTP Webhook 接收数据库变更事件。

## 运行

```powershell
cd E:\github\ProActor\demo\demo\CdcService\CdcHttpServer
& 'C:\Program Files\dotnet\dotnet.exe' run
```

## 监听端点

- **URL**: `http://*:8889/cdc`
- **方法**: POST
- **Content-Type**: application/json

## 日志文件

运行时会在当前目录生成 `cdc_events.log`，记录所有接收到的 CDC 事件。

## Debezium 配置

确保 `debezium/application.properties` 中有：

```properties
debezium.sink.type=http
debezium.sink.http.url=http://host.docker.internal:8889/cdc
```

## 测试

```sql
INSERT INTO dbo.Product (Sku, Price, ProductName, CustomerId) 
VALUES ('TEST-001', 99.99, '测试商品', 2);
```

控制台应显示：
```
[22:00:00] 📨 CDC Event Received
  Table: dbo.Product
  Op: 🟢 CREATE
  Data:
    Sku: TEST-001
    Price: 99.99
    ...
```
