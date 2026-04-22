## 1. 数据模型与内存存储

- [ ] 1.1 定义 `CdcEvent` 记录类，包含 Id、SourceSchema、SourceTable、Op、Before、After、TsMs、RawJson 属性
- [ ] 1.2 创建 `InMemoryEventStore` 类，使用线程安全的集合维护最近 1000 条事件，提供 Add、GetAll、GetById、GetStats 方法
- [ ] 1.3 修改 `POST /cdc` 处理逻辑，在接收事件时解析并存储到 `InMemoryEventStore`

## 2. 查询端点实现

- [ ] 2.1 实现 `GET /api/events` 端点，支持 `page`、`size`、`op` 查询参数，返回分页事件列表
- [ ] 2.2 实现 `GET /api/events/{id}` 端点，根据 ID 返回单条事件详情，不存在时返回 404
- [ ] 2.3 实现 `GET /api/stats` 端点，返回 `totalEvents` 和按 `op` 分组的统计信息

## 3. 集成与验证

- [ ] 3.1 在 CdcHttpServer 启动时注册 `InMemoryEventStore` 为单例服务
- [ ] 3.2 编译并运行 CdcHttpServer，使用 curl/Postman 测试 POST /cdc 接收和 GET /api/events 查询流程
- [ ] 3.3 验证分页、按 op 过滤、stats 统计功能正确
- [ ] 3.4 验证单条查询 404 场景
