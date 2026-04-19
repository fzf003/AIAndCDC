<template>
  <n-card
    :class="['event-card', `op-${event.op.toLowerCase()}`]"
    hoverable
    @click="$emit('click', event)"
  >
    <n-space align="center" justify="space-between">
      <n-space align="center">
        <n-tag :type="getOpType(event.op)" size="small">
          {{ getOpLabel(event.op) }}
        </n-tag>
        <n-text strong>{{ event.table }}</n-text>
        <n-text type="info" class="database">{{ event.database }}</n-text>
      </n-space>
      <n-text type="info" class="timestamp">
        {{ formatTime(event.timestamp) }}
      </n-text>
    </n-space>
    <n-code
      :code="previewData"
      language="json"
      :show-line-numbers="false"
      class="data-preview"
    />
  </n-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { NCard, NSpace, NTag, NText, NCode } from 'naive-ui';
import type { CdcEvent, Operation } from '../types/cdc';

interface Props {
  event: CdcEvent;
}

const props = defineProps<Props>();
defineEmits<{
  (e: 'click', event: CdcEvent): void;
}>();

const previewData = computed(() => {
  try {
    const data = JSON.parse(props.event.data);
    return JSON.stringify(data, null, 2).slice(0, 200) + '...';
  } catch {
    return props.event.data.slice(0, 200);
  }
});

function getOpType(op: Operation): 'success' | 'warning' | 'error' | 'info' | 'default' {
  const typeMap: Record<Operation, 'success' | 'warning' | 'error' | 'info'> = {
    CREATE: 'success',
    UPDATE: 'warning',
    DELETE: 'error',
    READ: 'info',
  };
  return typeMap[op] || 'default';
}

function getOpLabel(op: Operation): string {
  const labelMap: Record<Operation, string> = {
    CREATE: '创建',
    UPDATE: '更新',
    DELETE: '删除',
    READ: '读取',
  };
  return labelMap[op] || op;
}

function formatTime(timestamp: string): string {
  const date = new Date(timestamp);
  return date.toLocaleTimeString('zh-CN');
}
</script>

<style scoped>
.event-card {
  margin-bottom: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.event-card:hover {
  transform: translateX(4px);
}

.op-create {
  border-left: 4px solid #18a058;
}

.op-update {
  border-left: 4px solid #f0a020;
}

.op-delete {
  border-left: 4px solid #d03050;
}

.op-read {
  border-left: 4px solid #2080f0;
}

.database {
  font-size: 12px;
}

.timestamp {
  font-size: 12px;
  font-family: monospace;
}

.data-preview {
  margin-top: 8px;
  font-size: 12px;
}

.data-preview :deep(pre) {
  margin: 0;
  padding: 8px;
  background: #f5f5f5;
  border-radius: 4px;
}
</style>
