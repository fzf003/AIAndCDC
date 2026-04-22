<template>
  <n-config-provider :theme="theme" :theme-overrides="themeOverrides">
    <n-loading-bar-provider>
      <n-message-provider>
        <n-layout class="layout">
          <n-layout-header bordered class="header">
            <n-space align="center" justify="space-between">
              <n-space align="center">
                <n-icon :component="PulseOutline" size="28" color="#18a058" />
                <n-h1 style="margin: 0; font-size: 20px">CDC 事件监控</n-h1>
              </n-space>
              <n-space>
                <n-tag :type="connectionStatus.type">
                  {{ connectionStatus.text }}
                </n-tag>
                <n-button circle size="small" @click="toggleTheme">
                  <template #icon>
                    <n-icon :component="isDark ? SunnyOutline : MoonOutline" />
                  </template>
                </n-button>
              </n-space>
            </n-space>
          </n-layout-header>

          <n-layout-content class="content">
            <n-grid :cols="24" :x-gap="16" responsive="screen">
              <n-grid-item :span="24" :md="6">
                <StatsPanel :stats="stats" />
                <FilterBar
                  :filters="filters"
                  @update:filters="setFilters"
                  @clear="clearFilters"
                />
              </n-grid-item>

              <n-grid-item :span="24" :md="18">
                <EventList
                  :events="filteredEvents"
                  :loading="loading"
                  @select="handleEventSelect"
                />
              </n-grid-item>
            </n-grid>
          </n-layout-content>
        </n-layout>

        <EventDetailModal
          :show="showDetail"
          :event="selectedEvent"
          @close="showDetail = false"
        />
      </n-message-provider>
    </n-loading-bar-provider>
  </n-config-provider>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  NButton,
  NConfigProvider,
  NGrid,
  NGridItem,
  NH1,
  NIcon,
  NLayout,
  NLayoutContent,
  NLayoutHeader,
  NLoadingBarProvider,
  NMessageProvider,
  NSpace,
  NTag,
  darkTheme,
  type GlobalThemeOverrides,
} from 'naive-ui';
import { MoonOutline, PulseOutline, SunnyOutline } from '@vicons/ionicons5';
import EventDetailModal from './EventDetailModal.vue';
import EventList from './EventList.vue';
import FilterBar from './FilterBar.vue';
import StatsPanel from './StatsPanel.vue';
import { useEvents } from '../composables/useEvents';
import type { CdcEvent } from '../types/cdc';

const isDark = ref(false);
const showDetail = ref(false);
const selectedEvent = ref<CdcEvent | null>(null);

const theme = computed(() => (isDark.value ? darkTheme : null));
const themeOverrides: GlobalThemeOverrides = {
  common: {
    primaryColor: '#18a058',
    primaryColorHover: '#36ad6a',
    primaryColorPressed: '#0c7a43',
  },
};

const {
  stats,
  loading,
  error,
  filters,
  filteredEvents,
  setFilters,
  clearFilters,
} = useEvents();

const connectionStatus = computed(() => {
  if (error.value) {
    return { type: 'error' as const, text: '连接失败' };
  }
  if (loading.value) {
    return { type: 'warning' as const, text: '加载中' };
  }
  return { type: 'success' as const, text: '已连接' };
});

function toggleTheme() {
  isDark.value = !isDark.value;
}

function handleEventSelect(event: CdcEvent) {
  selectedEvent.value = event;
  showDetail.value = true;
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
}

.layout {
  min-height: 100vh;
}

.header {
  padding: 16px 24px;
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--n-color);
}

.content {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

@media (max-width: 768px) {
  .header {
    padding: 12px 16px;
  }

  .content {
    padding: 16px;
  }
}
</style>
