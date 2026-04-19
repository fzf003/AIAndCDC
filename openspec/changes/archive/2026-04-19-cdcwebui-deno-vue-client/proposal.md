## 背景

当前 CdcWebUI 使用纯 HTML + JavaScript 实现，功能较为基础。为了提升用户体验和开发效率，需要一个现代化的前端客户端：

1. **更好的开发体验** - 使用 Vue 3 的组件化开发，代码更易维护
2. **类型安全** - Deno 提供原生 TypeScript 支持，减少运行时错误
3. **现代化 UI** - 使用 Vue 生态的 UI 组件库，提升界面美观度
4. **实时更新** - 实现 WebSocket 或 SSE 连接，实时显示 CDC 事件

## 变更内容

- 创建 Deno + Vue 3 前端项目 `CdcWebUIClient/`
- 实现 CDC 事件实时展示页面
- 集成 CdcWebUI 的 `/api/events` 和 `/api/stats` API
- 支持事件筛选、搜索、详情查看
- 添加响应式设计，支持移动端

## 能力定义

### 新增能力
- `cdcwebui-deno-vue-client`: Deno + Vue 3 前端客户端，用于可视化展示 CDC 事件

### 修改的能力
- （无现有 spec 需要修改）

## 影响范围

- **新增目录** `CdcWebUIClient/` - Deno + Vue 3 项目
- **API 依赖** - 依赖 CdcWebUI 的 `/api/events` 和 `/api/stats` 接口
- **无后端变更** - 不修改现有 CdcWebUI 代码
- **可选部署** - 可作为独立服务运行，或与 CdcWebUI 一起部署
