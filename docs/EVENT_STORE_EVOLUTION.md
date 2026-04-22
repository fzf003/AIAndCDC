# 事件存储演进说明

## 当前定位

当前默认实现是 [`InMemoryEventStore`](/d:/HarnessSpecProject/AIAndCDC/CdcHttpServer/Services/InMemoryEventStore.cs)，适合以下场景：

- 本地开发
- CDC 界面展示
- 最近事件查询
- 无需跨重启保留历史数据的演示环境

## 当前限制

- 数据仅保存在内存中，服务重启后会丢失
- 不适合长期历史归档
- 不适合复杂筛选、全文检索或大规模查询

## 保留策略

- 采用固定容量策略
- 超过容量后按 FIFO 丢弃最早事件
- 当前容量默认值由 `EventStore:Capacity` 控制

## 为什么现在继续保留内存实现

- 对当前推荐主线足够轻量
- 接口已经通过 `IEventStore` 做了抽象
- 在 P0/P1 完成后，先补测试和文档比立即引入持久化更划算

## 后续扩展方向

如需持久化，可在不破坏当前 API 的前提下新增实现，例如：

- SQLite
- SQL Server
- 文件型事件归档

建议扩展时保持以下边界稳定：

- `GET /api/events`
- `GET /api/events/{id}`
- `GET /api/stats`
- `IEventStore` 的核心职责

## 推荐演进顺序

1. 先保持 `InMemoryEventStore` 作为默认实现
2. 为持久化实现增加独立配置开关
3. 在不改动前端契约的前提下切换底层存储
