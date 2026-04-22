<template>
  <n-card title="统计信息" class="stats-panel">
    <n-grid :cols="3" :x-gap="12">
      <n-grid-item>
        <n-statistic label="总事件数" :value="stats.total">
          <template #prefix>
            <n-icon :component="AppsOutline" />
          </template>
        </n-statistic>
      </n-grid-item>
      <n-grid-item>
        <n-statistic label="最近 1 分钟" :value="stats.lastMinute">
          <template #prefix>
            <n-icon :component="TimeOutline" />
          </template>
        </n-statistic>
      </n-grid-item>
      <n-grid-item>
        <n-statistic label="最近 1 小时" :value="stats.lastHour">
          <template #prefix>
            <n-icon :component="HourglassOutline" />
          </template>
        </n-statistic>
      </n-grid-item>
    </n-grid>
  </n-card>
</template>

<script setup lang="ts">
import { NCard, NGrid, NGridItem, NIcon, NStatistic } from 'naive-ui';
import { AppsOutline, HourglassOutline, TimeOutline } from '@vicons/ionicons5';
import type { Stats } from '../types/cdc';

interface Props {
  stats: Stats;
}

defineProps<Props>();
</script>

<style scoped>
.stats-panel {
  margin-bottom: 16px;
}
</style>
