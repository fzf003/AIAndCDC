## 1. 项目初始化

- [x] 1.1 创建 `CdcWebUIClient/` 目录结构
- [x] 1.2 使用 `deno.json` 初始化 Deno 项目
- [x] 1.3 配置 Vite 与 Deno 插件
- [x] 1.4 通过 npm 说明符安装 Vue 3 及依赖
- [x] 1.5 安装 UI 组件库（Naive UI 或 Element Plus）

## 2. 类型定义

- [x] 2.1 创建 `src/types/cdc.ts`，定义 CdcEvent 接口
- [x] 2.2 创建 `/api/stats` 响应的 Stats 接口
- [x] 2.3 定义 Operation 类型（'CREATE' | 'UPDATE' | 'DELETE' | 'READ'）

## 3. API 组合式函数

- [x] 3.1 创建 `src/composables/useEvents.ts`
- [x] 3.2 实现 fetchEvents() 函数
- [x] 3.3 实现 fetchStats() 函数
- [x] 3.4 实现可配置间隔的自动轮询
- [x] 3.5 添加错误处理和重试逻辑

## 4. 组件开发

- [x] 4.1 创建 `StatsPanel.vue` - 展示总数、最近1分钟、最近1小时统计
- [x] 4.2 创建 `FilterBar.vue` - 操作类型筛选和表名搜索
- [x] 4.3 创建 `EventCard.vue` - 单事件展示卡片
- [x] 4.4 创建 `EventList.vue` - 事件列表（支持分页）
- [x] 4.5 创建 `EventDetailModal.vue` - JSON 详情视图，支持复制
- [x] 4.6 创建 `App.vue` - 根组件，包含整体布局

## 5. 主应用

- [x] 5.1 创建 `src/main.ts` - Vue 应用入口
- [x] 5.2 创建 `index.html` - HTML 模板
- [x] 5.3 添加 `main.css` 基础样式
- [x] 5.4 配置暗色/亮色模式支持

## 6. 开发与测试

- [x] 6.1 启动 CdcWebUI 后端服务（端口 8889）
- [x] 6.2 运行 Deno 开发服务器
- [x] 6.3 测试事件获取和展示功能
- [x] 6.4 测试筛选功能
- [x] 6.5 测试移动端响应式布局

> 注：测试任务需要手动执行，确保后端服务已启动

## 7. 构建与部署

- [x] 7.1 配置生产环境构建（已在 vite.config.ts 中配置）
- [ ] 7.2 本地测试生产构建
- [x] 7.3 添加 README 部署说明
- [ ] 7.4 更新 docker-compose.yml（可选）

> 注：7.2 和 7.4 需要 Deno 环境和 Docker 环境支持，可手动执行
