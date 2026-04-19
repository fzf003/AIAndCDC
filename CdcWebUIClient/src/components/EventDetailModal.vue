<template>
  <n-modal
    :show="show"
    preset="card"
    title="事件详情"
    style="width: 800px; max-width: 90vw"
    @close="$emit('close')"
  >
    <n-space vertical>
      <n-descriptions bordered :column="2">
        <n-descriptions-item label="操作类型">
          <n-tag :type="getOpType(event.op)">
            {{ getOpLabel(event.op) }}
          </n-tag>
        </n-descriptions-item>
        <n-descriptions-item label="表名">
          {{ event.table }}
        </n-descriptions-item>
        <n-descriptions-item label="数据库">
          {{ event.database }}
        </n-descriptions-item>
        <n-descriptions-item label="时间">
          {{ formatTime(event.timestamp) }}
        </n-descriptions-item>
      </n-descriptions>

      <n-divider />

      <n-space justify="space-between" align="center">
        <n-text strong>数据内容</n-text>
        <n-button size="small" @click="handleCopy">
          <template #icon>
            <n-icon :component="CopyOutline" />
          </template>
          复制 JSON
        </n-button>
      </n-space>

      <n-code
        :code="formattedData"
        language="json"
        show-line-numbers
        style="max-height: 400px; overflow: auto"
      />
    </n-space>
  </n-modal>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import {
  NModal,
  NCard,
  NSpace,
  NDescriptions,
  NDescriptionsItem,
  NTag,
  NText,
  NDivider,
  NButton,
  NIcon,
  NCode,
  useMessage,
} from 'naive-ui';
import { CopyOutline } from '@vicons/ionicons5';
import type { CdcEvent, Operation } from '../types/cdc';

interface Props {
  show: boolean;
  event: CdcEvent;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: 'close'): void;
}>();

const message = useMessage();

const formattedData = computed(() => {
  try {
    const data = JSON.parse(props.event.data);
    return JSON.stringify(data, null, 2);
  } catch {
    return props.event.data;
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
  return date.toLocaleString('zh-CN');
}

async function handleCopy() {
  try {
    await navigator.clipboard.writeText(formattedData.value);
    message.success('已复制到剪贴板');
  } catch {
    message.error('复制失败');
  }
}
</script>
