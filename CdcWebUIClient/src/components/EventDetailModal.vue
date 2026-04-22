<template>
  <n-modal
    :show="show"
    preset="card"
    title="事件详情"
    style="width: 800px; max-width: 90vw"
    @close="$emit('close')"
  >
    <n-space v-if="event" vertical>
      <n-descriptions bordered :column="2">
        <n-descriptions-item label="操作类型">
          <n-tag :type="getOpType(event.op)">
            {{ formatOperation(event.op) }}
          </n-tag>
        </n-descriptions-item>
        <n-descriptions-item label="表名">
          {{ event.sourceSchema }}.{{ event.sourceTable }}
        </n-descriptions-item>
        <n-descriptions-item label="接收时间">
          {{ formatTime(event.receivedAt) }}
        </n-descriptions-item>
        <n-descriptions-item label="上游时间戳">
          {{ event.tsMs || '-' }}
        </n-descriptions-item>
      </n-descriptions>

      <n-divider />

      <n-space justify="space-between" align="center">
        <n-text strong>事件 JSON</n-text>
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
  NButton,
  NCode,
  NDescriptions,
  NDescriptionsItem,
  NDivider,
  NIcon,
  NModal,
  NSpace,
  NTag,
  NText,
  useMessage,
} from 'naive-ui';
import { CopyOutline } from '@vicons/ionicons5';
import { formatOperation, getEventPayload, type CdcEvent, type Operation } from '../types/cdc';

interface Props {
  show: boolean;
  event: CdcEvent | null;
}

const props = defineProps<Props>();
defineEmits<{
  (e: 'close'): void;
}>();

const message = useMessage();

const formattedData = computed(() => {
  if (!props.event) {
    return '{}';
  }

  try {
    return JSON.stringify(
      {
        ...props.event,
        payload: getEventPayload(props.event),
      },
      null,
      2,
    );
  } catch {
    return props.event.rawJson;
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
  return new Date(value).toLocaleString('zh-CN');
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
