## 项目背景

CdcWebUI 是一个简单的 .NET Web 应用，提供两个 API 端点：
- `GET /api/events` - 返回最近的 CDC 事件列表（最多 50 条）
- `GET /api/stats` - 返回事件统计信息（总数、最近1分钟、最近1小时）

当前前端是一个纯 HTML 文件，使用原生 JavaScript 轮询 API 获取数据。

## 目标与非目标

**目标：**
- 使用 Deno + Vue 3 创建现代化前端客户端
- 实现实时 CDC 事件展示
- 提供事件筛选和搜索功能
- 支持响应式布局

**非目标：**
- 不修改 CdcWebUI 后端代码
- 不实现用户认证（复用现有简单方案）
- 不持久化数据（仅展示内存中的事件）

## 设计决策

### 1. 技术栈选择
**决策**：Deno 2.x + Vue 3 + Vite

**理由**：
- Deno 原生支持 TypeScript，无需额外配置
- Vue 3 组合式 API 提供更好的代码组织
- Vite 提供快速的开发体验
- 使用 Deno 的 npm 兼容模式引入 Vue 生态

### 2. UI 组件库
**决策**：Element Plus 或 Naive UI

**理由**：
- 成熟的 Vue 3 组件库
- 提供表格、卡片、标签等适合展示事件的组件
- 支持暗色模式

### 3. 实时数据获取
**决策**：使用轮询（Polling）而非 WebSocket

**理由**：
- CdcWebUI 当前仅提供 HTTP API
- 轮询实现简单，满足实时性需求
- 可配置轮询间隔（默认 3 秒）

### 4. 项目结构
```
CdcWebUIClient/
├── deno.json          # Deno 配置
├── vite.config.ts     # Vite 配置
├── index.html         # 入口 HTML
├── src/
│   ├── main.ts        # 应用入口
│   ├── App.vue        # 根组件
│   ├── components/    # 组件
│   │   ├── EventList.vue
│   │   ├── EventCard.vue
│   │   ├── StatsPanel.vue
│   │   └── FilterBar.vue
│   ├── composables/   # 组合式函数
│   │   └── useEvents.ts
│   ├── types/         # TypeScript 类型
│   │   └── cdc.ts
│   └── styles/        # 样式
│       └── main.css
```

### 5. API 客户端设计
- 使用原生 `fetch` API
- 封装 composable `useEvents()` 管理状态
- 自动轮询和错误重试

## 风险与权衡

| 风险 | 缓解措施 |
|------|----------|
| Deno 生态相对较新 | 使用 npm 兼容模式，必要时使用 Node 包 |
| CORS 问题 | 配置 Vite 代理或确保同源部署 |
| 性能问题（大量事件） | 实现虚拟滚动或分页 |

## 迁移计划

**部署步骤**：
1. 开发完成后，构建生产版本
2. 可选择：
   - 独立部署：Deno 运行 dev 服务器
   - 集成部署：构建静态文件，由 CdcWebUI 托管

**回滚策略**：
- 删除 `CdcWebUIClient/` 目录即可回滚
- 保留原始 CdcWebUI 的 `wwwroot/index.html`

## 待确认问题

- 是否需要支持暗色模式切换？
- 事件详情展示需要哪些字段？
- 是否需要导出功能（CSV/JSON）？
