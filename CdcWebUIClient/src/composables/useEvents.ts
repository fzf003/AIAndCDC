import { ref, computed, onMounted, onUnmounted } from 'vue';
import type { CdcEvent, FilterOptions, PagedResult, Stats } from '../types/cdc.ts';

const API_BASE = '/api';
const DEFAULT_POLL_INTERVAL = 3000;
const DEFAULT_PAGE_SIZE = 100;
const MAX_RETRIES = 3;

export function useEvents(pollInterval: number = DEFAULT_POLL_INTERVAL) {
  const events = ref<CdcEvent[]>([]);
  const stats = ref<Stats>({
    total: 0,
    totalEvents: 0,
    byOp: {},
    lastMinute: 0,
    lastHour: 0,
  });
  const loading = ref(false);
  const error = ref<string | null>(null);
  const filters = ref<FilterOptions>({ operation: 'ALL', tableSearch: '' });

  let pollTimer: number | null = null;
  let retryCount = 0;

  const filteredEvents = computed(() => {
    let result = events.value;

    if (filters.value.operation && filters.value.operation !== 'ALL') {
      result = result.filter((event) => event.op === filters.value.operation);
    }

    if (filters.value.tableSearch) {
      const search = filters.value.tableSearch.toLowerCase();
      result = result.filter((event) =>
        `${event.sourceSchema}.${event.sourceTable}`.toLowerCase().includes(search),
      );
    }

    return result;
  });

  async function fetchEvents(): Promise<void> {
    try {
      const response = await fetch(`${API_BASE}/events?page=1&size=${DEFAULT_PAGE_SIZE}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = (await response.json()) as PagedResult<CdcEvent>;
      events.value = data.items;
      error.value = null;
      retryCount = 0;
    } catch (err) {
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }

      const message = err instanceof Error ? err.message : '获取事件失败';
      error.value = message;
      console.error('获取事件失败:', err);

      if (retryCount < MAX_RETRIES) {
        retryCount += 1;
        window.setTimeout(fetchEvents, retryCount * 1000);
      }
    }
  }

  async function fetchStats(): Promise<void> {
    try {
      const response = await fetch(`${API_BASE}/stats`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      stats.value = (await response.json()) as Stats;
    } catch (err) {
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }

      console.error('获取统计信息失败:', err);
    }
  }

  async function fetchAll(): Promise<void> {
    loading.value = true;
    await Promise.all([fetchEvents(), fetchStats()]);
    loading.value = false;
  }

  function startPolling(): void {
    if (pollTimer) {
      return;
    }

    void fetchAll();
    pollTimer = window.setInterval(() => {
      void fetchAll();
    }, pollInterval);
  }

  function stopPolling(): void {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
  }

  function setFilters(newFilters: Partial<FilterOptions>): void {
    filters.value = { ...filters.value, ...newFilters };
  }

  function clearFilters(): void {
    filters.value = { operation: 'ALL', tableSearch: '' };
  }

  onMounted(() => {
    startPolling();
  });

  onUnmounted(() => {
    stopPolling();
  });

  return {
    events,
    stats,
    loading,
    error,
    filters,
    filteredEvents,
    fetchEvents,
    fetchStats,
    fetchAll,
    startPolling,
    stopPolling,
    setFilters,
    clearFilters,
  };
}
