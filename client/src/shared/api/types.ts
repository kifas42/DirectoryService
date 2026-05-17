export type PaginationResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export interface ApiEnvelope<T = unknown> {
  result: T | null;
  error: string | null;
  isError: boolean;
  timeGenerated: string;
}
