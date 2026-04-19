<template>
  <n-card class="filter-bar">
    <n-space align="center">
      <n-select
        v-model:value="localOperation"
        :options="operationOptions"
        placeholder="操作类型"
        clearable
        style="width: 140px"
        @update:value="handleOperationChange"
      />
      <n-input
        v-model:value="localSearch"
        placeholder="搜索表名..."
        clearable
        style="width: 200px"
        @update:value="handleSearchChange"
      >
        <template #prefix>
          <n-icon :component="SearchOutline" />
        </template>
      </n-input>
      <n-button @click="handleClear">
        <template #icon>
          <n-icon :component="CloseCircleOutline" />
        </template>
        清除筛选
      </n-button>
    </n-space>
  </n-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { NCard, NSelect, NInput, NButton, NSpace, NIcon } from 'naive-ui';
import { SearchOutline, CloseCircleOutline } from '@vicons/ionicons5';
import type { Operation, FilterOptions } from '../types/cdc';

interface Props {
  filters: FilterOptions;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: 'update:filters', filters: FilterOptions): void;
  (e: 'clear'): void;
}>();

const localOperation = ref<Operation | 'ALL' | null>(props.filters.operation || 'ALL');
const localSearch = ref(props.filters.tableSearch || '');

const operationOptions = [
  { label: '全部', value: 'ALL' },
  { label: '🟢 创建', value: 'CREATE' },
  { label: '🟡 更新', value: 'UPDATE' },
  { label: '🔴 删除', value: 'DELETE' },
  { label: '🔵 读取', value: 'READ' },
];

watch(() => props.filters, (newFilters) => {
  localOperation.value = newFilters.operation || 'ALL';
  localSearch.value = newFilters.tableSearch || '';
}, { deep: true });

function handleOperationChange(value: Operation | 'ALL' | null) {
  emit('update:filters', {
    ...props.filters,
    operation: value || 'ALL',
  });
}

function handleSearchChange(value: string) {
  emit('update:filters', {
    ...props.filters,
    tableSearch: value,
  });
}

function handleClear() {
  localOperation.value = 'ALL';
  localSearch.value = '';
  emit('clear');
}
</script>

<style scoped>
.filter-bar {
  margin-bottom: 16px;
}
</style>
