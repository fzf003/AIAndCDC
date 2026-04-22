## Why

在 P0 和 P1 完成后，项目主线、接口与运行治理会趋于稳定，这时再补齐“可持续维护能力”最划算：

- 事件存储能力需要从“仅够演示”走向“可扩展”。
- 自动化测试需要覆盖关键 CDC 解析与查询契约。
- 本地启动与排障文档需要变成真正能交接给别人的材料。

P2 的重点不是再做架构收口，而是提升长期维护能力。

## What Changes

- 评估并整理事件存储的演进方向。
- 为 CDC 解析、查询接口、Redis 消费补基础自动化测试。
- 完善中文启动文档、职责说明与排障说明。

## Dependencies

本变更依赖：

- `p0-unify-api-and-ui-mainline`
- `p1-runtime-hardening-and-service-boundaries`

因为测试与文档需要基于稳定主线、稳定接口和稳定运行方式。

## Capabilities

### New Capabilities

- `event-store-evolution`: 明确事件存储演进边界
- `automated-verification`: 为核心链路提供基础自动化验证
- `local-startup-doc`: 提供可交接的本地启动与排障文档

## Impact

- **后端代码**: 增加测试与可能的存储扩展接口。
- **项目文档**: 本地启动、职责边界、故障排查将更完整。
- **团队协作**: 新成员更容易理解项目并验证修改。
