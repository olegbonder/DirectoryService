export interface PaginationResponse<T> extends PaginationRequest {
  items: T[];
  totalCount: number;
  totalPages: number;
}

export type DictionaryResponse = {
  items: DictionaryItemResponse[];
};

export type DictionaryItemResponse = {
  id: string;
  name: string;
};

export type OrderDirection = "asc" | "desc";

export interface PaginationRequest {
  page: number;
  pageSize: number;
}

export const PAGE_SIZE = 4;
