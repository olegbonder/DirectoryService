export type PaginationResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type DictionaryResponse = {
  items: DictionaryItemResponse[];
};

export type DictionaryItemResponse = {
  id: string;
  name: string;
};
