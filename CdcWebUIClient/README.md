# CDC Web UI Client

基于 Deno + Vue 3 的 CDC 事件监控客户端。

## 技术栈

- **运行时**: Deno 2.x
- **框架**: Vue 3 (Composition API)
- **构建工具**: Vite
- **UI 组件库**: Naive UI
- **图标**: @vicons/ionicons5

## 功能特性

- 📊 实时统计面板（总事件数、最近1分钟、最近1小时）
- 📋 CDC 事件列表展示
- 🔍 按操作类型和表名筛选
- 📄 事件详情查看（JSON 格式化）
- 📋 一键复制事件数据
- 🌓 暗色/亮色主题切换
- 📱 响应式设计，支持移动端

## 快速开始

### 前置条件

- 安装 [Deno](https://deno.land/) 2.x
- 确保 CdcWebUI 后端运行在 http://localhost:8889

### 开发模式

```bash
# 进入项目目录
cd CdcWebUIClient

# 启动开发服务器
deno task dev
```

开发服务器将在 http://localhost:5173 启动，并代理 API 请求到后端。

### 生产构建

```bash
# 构建生产版本
deno task build

# 预览生产构建
deno task preview
```

## 项目结构

```
CdcWebUIClient/
├── deno.json              # Deno 配置
├── vite.config.ts         # Vite 配置
├── index.html             # HTML 入口
├── src/
│   ├── components/        # Vue 组件
│   │   ├── App.vue        # 根组件
│   │   ├── StatsPanel.vue # 统计面板
│   │   ├── FilterBar.vue  # 筛选栏
│   │   ├── EventCard.vue  # 事件卡片
│   │   ├── EventList.vue  # 事件列表
│   │   └── EventDetailModal.vue # 详情弹窗
│   ├── composables/       # 组合式函数
│   │   └── useEvents.ts   # 事件管理
│   ├── types/             # TypeScript 类型
│   │   └── cdc.ts         # CDC 类型定义
│   ├── styles/            # 样式文件
│   │   └── main.css       # 基础样式
│   └── main.ts            # 应用入口
```

## API 接口

客户端依赖 CdcWebUI 提供的以下接口：

| 接口 | 方法 | 描述 |
|------|------|------|
| `/api/events` | GET | 获取最近 50 条 CDC 事件 |
| `/api/stats` | GET | 获取事件统计信息 |

## 配置说明

### 代理配置

在 `vite.config.ts` 中配置代理：

```typescript
server: {
  proxy: {
    "/api": {
      target: "http://localhost:8889",
      changeOrigin: true,
    },
  },
}
```

### 轮询间隔

在 `src/composables/useEvents.ts` 中修改：

```typescript
const DEFAULT_POLL_INTERVAL = 3000; // 3秒
```

## 开发指南

### 添加新组件

1. 在 `src/components/` 创建 `.vue` 文件
2. 使用 `<script setup lang="ts">` 语法
3. 从 `naive-ui` 导入所需组件
4. 在 `App.vue` 中引入使用

### 类型定义

所有类型定义在 `src/types/cdc.ts`：

```typescript
interface CdcEvent {
  timestamp: string;
  database: string;
  table: string;
  op: 'CREATE' | 'UPDATE' | 'DELETE' | 'READ';
  data: string;
}
```

## 许可证

MIT
