## Context

CdcHttpServer 当前仅作为 CDC 事件的接收端，通过 POST /cdc 接收 Debezium 推送的变更事件，并将原始 JSON 追加写入本地日志文件。该服务运行在 `http://*:8889`，是 CDC 数据流的第一站。

现有痛点：
- 事件数据仅存储在文本日志中，无法结构化查询
- CdcWebUIClient（Deno+Vue 前端）需要展示历史事件，但无 API 可用
- 没有统计视图来观察数据变更趋势

## Goals / Non-Goals

**Goals:**
- 提供结构化的 CDC 事件查询 API，支持列表查询和单条详情
- 提供事件统计端点，展示总量和操作类型分布
- 纯内存实现，零外部依赖，保持现有架构轻量
- 兼容现有 CdcWebUIClient 前端数据格式

**Non-Goals:**
- 不引入数据库或 Redis 持久化（日志文件已存在，查询走内存）
- 不实现复杂过滤（如全文搜索、时间范围查询）
- 不实现事件回放或流式推送（SSE/WebSocket）
- 不修改 POST /cdc 接收逻辑的核心行为

## Decisions

**1. 内存环形缓冲区存储事件**
- 使用 `ConcurrentQueue<>` 或 `List<>` 维护最近 1000 条事件
- 理由：实现简单，无需依赖；CDC 场景通常关注近期事件；1000 条足够前端分页展示
- 替代方案：SQLite 本地存储（ rejected — 增加部署复杂度和文件锁问题）

**2. 事件模型使用强类型 DTO**
- 定义 `CdcEvent` 记录类，包含 Id、Source、Op、Before、After、TsMs、RawJson
- 理由：比直接返回原始 JsonDocument 更可控，便于前端消费和后续扩展
- 接收时从 JsonDocument 提取字段并构造 DTO

**3. 查询端点 RESTful 设计**
- `GET /api/events?page=1&size=20&op=` 返回分页列表
- `GET /api/events/{id}` 返回单条详情（id 为接收顺序索引或 GUID）
- `GET /api/stats` 返回聚合统计
- 理由：与现有 CdcWebUIClient 前端调用模式一致

**4. 不持久化查询数据**
- 服务重启后事件列表清空，仅从重启后接收的事件开始积累
- 理由：符合 CDC HTTP Server 作为轻量级中转层的定位；历史数据由日志文件保留

## Risks / Trade-offs

- **[Risk] 服务重启数据丢失** → 缓解：日志文件保留完整原始 JSON，关键分析可离线进行；未来如需持久化可升级到 SQLite
- **[Risk] 内存溢出（高并发场景）** → 缓解：环形缓冲区限制 1000 条；单条事件通常 < 10KB，总内存占用 < 10MB
- **[Trade-off] 不支持跨重启查询** → 接受：当前定位为实时观测层，非数据仓库
