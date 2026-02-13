import { useDepartmentDictionary } from "./model/use-department-dictionary";
import { PAGE_SIZE } from "@/shared/api/types";
import DepartmentSelect from "./department-select";

type ParentDepartmentSelectProps = {
  parentId: string | undefined;
  setParentId: (id: string | undefined) => void;
};
export default function ParentDepartmentSelect({
  parentId,
  setParentId,
}: ParentDepartmentSelectProps) {
  const {
    departments,
    isPending,
    isError,
    error,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useDepartmentDictionary({
    pageSize: PAGE_SIZE,
    showOnlyParents: true,
  });

  return (
    <DepartmentSelect
      departments={departments ?? []}
      selectedId={parentId}
      onChange={setParentId}
      isPending={isPending}
      isError={isError}
      error={error}
      fetchNextPage={fetchNextPage}
      isFetchingNextPage={isFetchingNextPage}
      hasNextPage={hasNextPage}
    />
  );
}
