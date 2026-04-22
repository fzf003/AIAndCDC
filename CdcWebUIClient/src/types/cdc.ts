export type Operation = 'c' | 'u' | 'd' | 'r';

export interface CdcEvent {
  id: string;
  sourceSchema: string;
  sourceTable: string;
  op: Operation;
  before: unknown | null;
  after: unknown | null;
  tsMs: number;
  receivedAt: string;
  rawJson: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  size: number;
}

export interface Stats {
  total: number;
  totalEvents: number;
  byOp: Record<string, number>;
  lastMinute: number;
  lastHour: number;
}

export interface FilterOptions {
  operation?: Operation | 'ALL';
  tableSearch?: string;
}

export const operationLabels: Record<Operation, string> = {
  c: '新增',
  u: '更新',
  d: '删除',
  r: '读取',
};

export function formatOperation(op: string): string {
  return operationLabels[op as Operation] ?? op.toUpperCase();
}

export function getEventPayload(event: CdcEvent): unknown {
  return event.after ?? event.before ?? {};
}
