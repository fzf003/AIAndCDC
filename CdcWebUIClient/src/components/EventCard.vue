<template>
  <n-card
    :class="['event-card', `op-${event.op}`]"
    hoverable
    @click="$emit('click', event)"
  >
    <n-space align="center" justify="space-between">
      <n-space align="center">
        <n-tag :type="getOpType(event.op)" size="small">
          {{ formatOperation(event.op) }}
        </n-tag>
        <n-text strong>{{ event.sourceTable }}</n-text>
        <n-text type="info" class="database">{{ event.sourceSchema }}</n-text>
      </n-space>
      <n-text type="info" class="timestamp">
        {{ formatTime(event.receivedAt) }}
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
import { NCard, NCode, NSpace, NTag, NText } from 'naive-ui';
import { formatOperation, getEventPayload, type CdcEvent, type Operation } from '../types/cdc';

interface Props {
  event: CdcEvent;
}

const props = defineProps<Props>();
defineEmits<{
  (e: 'click', event: CdcEvent): void;
}>();

const previewData = computed(() => {
  try {
    const text = JSON.stringify(getEventPayload(props.event), null, 2);
    return text.length > 200 ? `${text.slice(0, 200)}...` : text;
  } catch {
    return props.event.rawJson.slice(0, 200);
  }
});

function getOpType(op: Operation): 'success' | 'warning' | 'error' | 'info' {
  const typeMap: Record<Operation, 'success' | 'warning' | 'error' | 'info'> = {
    c: 'success',
    u: 'warning',
    d: 'error',
    r: 'info',
  };

  return typeMap[op];
}

function formatTime(value: string): string {
  return new Date(value).toLocaleTimeString('zh-CN');
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

.op-c {
  border-left: 4px solid #18a058;
}

.op-u {
  border-left: 4px solid #f0a020;
}

.op-d {
  border-left: 4px solid #d03050;
}

.op-r {
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
