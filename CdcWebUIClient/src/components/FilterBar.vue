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
import { NButton, NCard, NIcon, NInput, NSelect, NSpace } from 'naive-ui';
import { CloseCircleOutline, SearchOutline } from '@vicons/ionicons5';
import type { FilterOptions, Operation } from '../types/cdc';

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
  { label: '新增', value: 'c' },
  { label: '更新', value: 'u' },
  { label: '删除', value: 'd' },
  { label: '读取', value: 'r' },
];

watch(
  () => props.filters,
  (newFilters) => {
    localOperation.value = newFilters.operation || 'ALL';
    localSearch.value = newFilters.tableSearch || '';
  },
  { deep: true },
);

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
