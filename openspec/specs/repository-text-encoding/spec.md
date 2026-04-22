## ADDED Requirements

### Requirement: 仓库文本文件统一使用 UTF-8

项目 SHALL 对源码、说明文档与过程文件统一使用 UTF-8 编码。

#### Scenario: 读取中文源码注释

- **WHEN** 开发者在编辑器或终端中查看源码
- **THEN** 中文内容应正确显示，不出现乱码

#### Scenario: 新增文档文件

- **WHEN** 新增 README、方案说明或任务文档
- **THEN** 文件应使用 UTF-8 编码保存

### Requirement: 文档和过程说明以中文为主

项目 SHALL 在说明文档、需求描述与变更过程记录中优先使用中文。

#### Scenario: 编写实施说明

- **WHEN** 记录变更方案、任务步骤或排障过程
- **THEN** 内容应以中文为主，必要时补充英文术语
