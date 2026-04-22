<template>
  <n-card title="事件列表" class="event-list">
    <n-empty v-if="events.length === 0" description="暂无事件" />
    <n-scrollbar v-else style="max-height: 600px">
      <EventCard
        v-for="event in paginatedEvents"
        :key="event.id"
        :event="event"
        @click="handleEventClick"
      />
    </n-scrollbar>
    <n-pagination
      v-if="showPagination"
      v-model:page="currentPage"
      :page-size="pageSize"
      :item-count="events.length"
      class="pagination"
    />
  </n-card>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { NCard, NEmpty, NPagination, NScrollbar } from 'naive-ui';
import EventCard from './EventCard.vue';
import type { CdcEvent } from '../types/cdc';

interface Props {
  events: CdcEvent[];
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const emit = defineEmits<{
  (e: 'select', event: CdcEvent): void;
}>();

const currentPage = ref(1);
const pageSize = ref(20);

const paginatedEvents = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return props.events.slice(start, start + pageSize.value);
});

const showPagination = computed(() => props.events.length > pageSize.value);

watch(
  () => props.events,
  () => {
    currentPage.value = 1;
  },
  { deep: true },
);

function handleEventClick(event: CdcEvent) {
  emit('select', event);
}
</script>

<style scoped>
.event-list {
  min-height: 400px;
}

.pagination {
  margin-top: 16px;
  justify-content: center;
}
</style>
