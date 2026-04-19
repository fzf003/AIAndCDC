import { ref, computed, onMounted, onUnmounted } from 'vue';
import type { CdcEvent, Stats, FilterOptions, Operation } from '../types/cdc.ts';

const API_BASE = '/api';
const DEFAULT_POLL_INTERVAL = 3000;
const MAX_RETRIES = 3;

/**
 * CDC 事件管理组合式函数
 */
export function useEvents(pollInterval: number = DEFAULT_POLL_INTERVAL) {
  // 状态
  const events = ref<CdcEvent[]>([]);
  const stats = ref<Stats>({ total: 0, lastMinute: 0, lastHour: 0 });
  const loading = ref(false);
  const error = ref<string | null>(null);
  const filters = ref<FilterOptions>({ operation: 'ALL', tableSearch: '' });
  
  // 轮询控制
  let pollTimer: number | null = null;
  let retryCount = 0;

  // 筛选后的事件
  const filteredEvents = computed(() => {
    let result = events.value;
    
    // 按操作类型筛选
    if (filters.value.operation && filters.value.operation !== 'ALL') {
      result = result.filter((e: CdcEvent) => e.op === filters.value.operation);
    }
    
    // 按表名搜索
    if (filters.value.tableSearch) {
      const search = filters.value.tableSearch.toLowerCase();
      result = result.filter((e: CdcEvent) => e.table.toLowerCase().includes(search));
    }
    
    return result;
  });

  /**
   * 获取事件列表
   */
  async function fetchEvents(): Promise<void> {
    try {
      const response = await fetch(`${API_BASE}/events`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json() as CdcEvent[];
      events.value = data;
      error.value = null;
      retryCount = 0;
    } catch (err) {
      // 忽略请求取消错误（页面刷新或组件卸载时）
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }
      
      const message = err instanceof Error ? err.message : '获取事件失败';
      error.value = message;
      console.error('获取事件失败:', err);
      
      // 重试逻辑
      if (retryCount < MAX_RETRIES) {
        retryCount++;
        console.log(`将在 ${retryCount * 1000}ms 后重试...`);
        setTimeout(fetchEvents, retryCount * 1000);
      }
    }
  }

  /**
   * 获取统计信息
   */
  async function fetchStats(): Promise<void> {
    try {
      const response = await fetch(`${API_BASE}/stats`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json() as Stats;
      stats.value = data;
    } catch (err) {
      // 忽略请求取消错误
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }
      console.error('获取统计失败:', err);
    }
  }

  /**
   * 同时获取事件和统计
   */
  async function fetchAll(): Promise<void> {
    loading.value = true;
    await Promise.all([fetchEvents(), fetchStats()]);
    loading.value = false;
  }

  /**
   * 开始轮询
   */
  function startPolling(): void {
    if (pollTimer) return;
    
    fetchAll();
    pollTimer = window.setInterval(() => {
      fetchAll();
    }, pollInterval);
  }

  /**
   * 停止轮询
   */
  function stopPolling(): void {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
  }

  /**
   * 设置筛选条件
   */
  function setFilters(newFilters: Partial<FilterOptions>): void {
    filters.value = { ...filters.value, ...newFilters };
  }

  /**
   * 清除筛选
   */
  function clearFilters(): void {
    filters.value = { operation: 'ALL', tableSearch: '' };
  }

  // 生命周期
  onMounted(() => {
    startPolling();
  });

  onUnmounted(() => {
    stopPolling();
  });

  return {
    // 状态
    events,
    stats,
    loading,
    error,
    filters,
    filteredEvents,
    
    // 方法
    fetchEvents,
    fetchStats,
    fetchAll,
    startPolling,
    stopPolling,
    setFilters,
    clearFilters,
  };
}
