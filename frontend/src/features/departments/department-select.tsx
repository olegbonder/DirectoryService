import {
  SelectContent,
  Select,
  SelectTrigger,
  SelectValue,
  SelectItem,
} from "@/shared/components/ui/select";
import { Spinner } from "@/shared/components/ui/spinner";
import { DictionaryItemResponse, PaginationResponse } from "@/shared/api/types";
import useCursorRef from "@/shared/hooks/use-cursor-ref";
import {
  FetchNextPageOptions,
  InfiniteQueryObserverResult,
} from "@tanstack/react-query";

type DepartmentSelectProps = {
  departments: DictionaryItemResponse[];
  selectedId?: string;
  onChange: (selectedId?: string) => void;
  isPending: boolean;
  isError: boolean;
  error: Error | null;
  fetchNextPage: (
    options?: FetchNextPageOptions | undefined,
  ) => Promise<
    InfiniteQueryObserverResult<
      PaginationResponse<DictionaryItemResponse>,
      Error
    >
  >;
  isFetchingNextPage: boolean;
  hasNextPage: boolean;
};
export default function DepartmentSelect({
  departments,
  selectedId,
  onChange,
  isPending,
  isError,
  error,
  fetchNextPage,
  isFetchingNextPage,
  hasNextPage,
}: DepartmentSelectProps) {
  const cursorRef = useCursorRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return (
    <>
      {isPending && <Spinner />}
      {!isPending && isError && (
        <p className="text-red-500">
          Ошибка загрузки подразделений: {error?.message}
        </p>
      )}
      <Select
        onValueChange={(value) =>
          onChange(value === "none" ? undefined : value)
        }
        value={selectedId || "none"}
      >
        <SelectTrigger>
          <SelectValue placeholder="Выберите подразделение" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="none">Без подразделения</SelectItem>
          {departments?.map((department) => (
            <SelectItem key={department.id} value={department.id}>
              {department.name}
            </SelectItem>
          ))}
          <div ref={cursorRef} className="flex justify-center py-2">
            {isFetchingNextPage && <Spinner />}
          </div>
        </SelectContent>
      </Select>
    </>
  );
}
