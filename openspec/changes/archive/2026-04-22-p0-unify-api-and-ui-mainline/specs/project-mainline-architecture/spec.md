## ADDED Requirements

### Requirement: 项目必须有唯一推荐主线

项目 SHALL 只保留一套推荐的本地运行主线，避免重复 UI 方案长期并存。

#### Scenario: 开发者查看启动文档

- **WHEN** 开发者阅读项目文档
- **THEN** 文档只展示一套推荐启动路径

#### Scenario: 旧 UI 方案仍保留代码

- **WHEN** 旧方案暂未删除
- **THEN** 系统必须明确将其标记为非推荐入口或归档候选
