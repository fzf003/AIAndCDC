# CdcWebUIClient

基于 Deno + Vue 3 的 CDC 事件监控前端。

## 项目定位

- 推荐主线：`CdcHttpServer + CdcWebUIClient`
- 当前职责：展示 CDC 事件列表、统计信息与事件详情
- 文档约定：中文为主，文本文件统一使用 UTF-8

## 技术栈

- 运行时：Deno 2.x
- 框架：Vue 3
- 构建工具：Vite
- UI 组件：Naive UI

## 开发启动

先启动后端：

```powershell
cd CdcHttpServer
dotnet run
```

再启动前端：

```powershell
cd CdcWebUIClient
deno task dev
```

默认开发地址：

- 前端：`http://localhost:5173`
- 后端：`http://localhost:8889`

## 接口契约

客户端依赖以下接口：

- `GET /api/events?page=1&size=100`
  - 返回分页对象：`items`、`total`、`page`、`size`
- `GET /api/events/{id}`
  - 返回单条 CDC 事件
- `GET /api/stats`
  - 返回统计信息

事件字段主结构：

- `id`
- `sourceSchema`
- `sourceTable`
- `op`
- `before`
- `after`
- `tsMs`
- `receivedAt`
- `rawJson`

其中 `op` 使用 Debezium 原始值：`c / u / d / r`。

## 说明

`CdcWebUI` 目录中的旧版单体页面不再作为推荐入口，后续以前后端分离方案为主。
