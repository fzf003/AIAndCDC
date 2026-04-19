/**
 * CDC 事件操作类型
 */
export type Operation = 'CREATE' | 'UPDATE' | 'DELETE' | 'READ';

/**
 * CDC 事件数据结构
 */
export interface CdcEvent {
  /** 事件时间戳 */
  timestamp: string;
  /** 数据库名称 */
  database: string;
  /** 表名 */
  table: string;
  /** 操作类型 */
  op: Operation;
  /** 事件数据（JSON 字符串） */
  data: string;
}

/**
 * 统计信息响应
 */
export interface Stats {
  /** 总事件数 */
  total: number;
  /** 最近1分钟事件数 */
  lastMinute: number;
  /** 最近1小时事件数 */
  lastHour: number;
}

/**
 * 筛选条件
 */
export interface FilterOptions {
  /** 操作类型筛选 */
  operation?: Operation | 'ALL';
  /** 表名搜索关键词 */
  tableSearch?: string;
}
