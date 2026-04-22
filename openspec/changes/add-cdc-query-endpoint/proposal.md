## Why

目前 CdcHttpServer 仅通过 POST /cdc 接收 CDC 事件并写入日志文件，没有提供任何查询接口。CDC Web UI 客户端和其他消费方无法获取历史事件数据，限制了平台的可观测性和实用性。需要新增查询端点，将接收到的 CDC 事件以结构化 JSON 返回，供前端展示和分析。

## What Changes

- 在 CdcHttpServer 中维护接收到的 CDC 事件内存缓存（最近 N 条，N 可配置，超出容量时采用 FIFO 淘汰策略）
- 新增 `GET /api/events` 端点，返回 CDC 事件列表（支持分页和基础过滤）
- 新增 `GET /api/events/{id}` 端点，返回单条事件详情
- 新增 `GET /api/stats` 端点，返回事件统计信息（总数量、按操作类型统计）
- 事件数据结构包含：来源库表、操作类型(c/u/d/r)、变更前后数据、时间戳
- 预留日志/持久化扩展接口，便于未来支持数据库或文件存储，防止服务重启数据丢失
- 明确接口的错误处理和返回码设计，如 404（事件不存在）、400（参数错误）等，返回统一的错误信息结构

## Capabilities

### New Capabilities
- `cdc-event-query`: CDC 事件查询能力，提供列表查询、详情查询和统计接口

### Modified Capabilities
- 无现有 spec 需要修改

## Impact

- **CdcHttpServer/Program.cs**: 新增内存存储逻辑（支持容量配置和淘汰策略）、查询端点映射、错误处理和返回码实现，预留持久化扩展接口
- **API 变更**: 新增 3 个 GET 端点（`/api/events`, `/api/events/{id}`, `/api/stats`），并规范错误返回结构
- **CdcWebUIClient**: 可直接消费新端点展示历史数据
- **无外部依赖变更**: 纯内存实现，无需数据库，未来可平滑扩展
