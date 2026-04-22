## ADDED Requirements

### Requirement: 运行配置必须集中管理

系统 SHALL 通过统一配置体系管理运行参数，而不是在代码中分散硬编码。

#### Scenario: 修改 Redis 地址

- **WHEN** 运维或开发者修改配置文件或环境变量
- **THEN** 系统应读取新的 Redis 配置，无需修改源码

### Requirement: API 错误响应必须结构化

系统 SHALL 为失败请求返回统一的结构化错误信息。

#### Scenario: 非法分页参数

- **WHEN** 客户端提交非法查询参数
- **THEN** 系统返回可解析的结构化错误响应

### Requirement: 健康检查必须反映关键依赖状态

系统 SHALL 提供能反映应用和关键依赖状态的健康检查输出。

#### Scenario: Redis 不可用

- **WHEN** Redis 相关依赖不可连接
- **THEN** 健康检查结果应体现依赖异常，而不是始终返回简单成功文本
