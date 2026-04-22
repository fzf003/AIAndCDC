## Why

当前项目在 P0 层面存在三个直接影响后续开发的问题：

- 后端 API 契约与前端类型不一致，`CdcHttpServer` 与 `CdcWebUIClient` 无法稳定协作。
- `CdcWebUI` 与 `CdcWebUIClient + CdcHttpServer` 两条 UI 主线并存，导致启动方式、文档说明与数据模型重复。
- 仓库存在编码乱码，已经影响源码注释、日志输出与文档可读性。

如果不先解决这三项，后续的配置治理、日志治理、测试补齐都会反复返工。

## What Changes

- 选定一条主线架构作为推荐实现与后续维护对象。
- 统一 CDC 查询接口的响应结构、字段命名与分页契约。
- 统一前端 `TypeScript` 类型定义与接口消费方式。
- 修复当前仓库中的中文乱码文件，并建立 UTF-8 约定。
- 将未选中的 UI 方案标记为归档候选，避免继续扩散。

## Recommended Mainline

本变更推荐保留：

- `CdcHttpServer + CdcWebUIClient`

本变更推荐归档候选：

- `CdcWebUI`

推荐理由：

- `CdcHttpServer` 已经承担了 CDC 接收与查询职责，适合继续作为后端主线。
- `CdcWebUIClient` 的前后端边界更清晰，后续更容易演进。
- `CdcWebUI` 当前更像早期原型，保留会增加重复维护成本。

## Capabilities

### New Capabilities

- `cdc-api-contract`: 统一 CDC 查询接口契约
- `project-mainline-architecture`: 明确项目推荐主线与归档边界
- `repository-text-encoding`: 统一文本编码与中文文档约定

## Impact

- **CdcHttpServer**: API 输出模型、分页结构、错误响应会收敛。
- **CdcWebUIClient**: 类型定义与接口读取方式会同步调整。
- **CdcWebUI**: 将被标记为归档候选或停止作为推荐入口。
- **仓库文档**: README、变更文档、过程说明改为中文为主，并修复乱码。
