## ADDED Requirements

### Requirement: 统一 CDC 查询接口契约

系统 SHALL 为 CDC 事件查询提供统一且稳定的响应结构，供后端与前端共同使用。

#### Scenario: 事件列表返回分页对象

- **WHEN** 客户端请求 `GET /api/events`
- **THEN** 系统返回包含 `items`、`total`、`page`、`size` 的 JSON 对象

#### Scenario: 事件详情使用统一字段

- **WHEN** 客户端请求 `GET /api/events/{id}`
- **THEN** 系统返回与事件列表项兼容的统一事件字段结构

#### Scenario: 统计接口返回统一统计结构

- **WHEN** 客户端请求 `GET /api/stats`
- **THEN** 系统返回稳定的统计字段结构，不依赖前端推测字段名称

### Requirement: 接口操作类型保持 CDC 原始语义

系统 SHALL 在查询接口中使用 CDC 原始操作类型值 `c/u/d/r`。

#### Scenario: 返回创建事件

- **WHEN** 某条事件来自创建操作
- **THEN** 返回值中的 `op` 为 `c`

#### Scenario: 前端显示友好标签

- **WHEN** 前端需要展示中文或英文可读标签
- **THEN** 前端基于 `op` 派生展示文本，而不是要求后端返回第二套语义字段
