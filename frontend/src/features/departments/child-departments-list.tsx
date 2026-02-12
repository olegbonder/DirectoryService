import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentChildrenInfinity } from "@/features/departments/model/use-department-children-infinity";
import DepartmentCard from "@/features/departments/department-card";
import { Building2 } from "lucide-react";

type ChildDepartmentsListProps = {
  parentId: string;
};

export default function ChildDepartmentsList({
  parentId,
}: ChildDepartmentsListProps) {
  const {
    childDepartments,
    isPending,
    isError,
    error,
    cursorRef,
    isFetchingNextPage,
  } = useDepartmentChildrenInfinity(parentId);

  if (isPending) {
    return (
      <div className="flex justify-center items-center py-4">
        <Spinner />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
        <p className="font-semibold">Ошибка загрузки дочерних подразделений</p>
        <p className="text-sm mt-1">{error?.message}</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-slate-900 flex items-center gap-2">
          <Building2 className="h-5 w-5 text-blue-600" />
          Дочерние подразделения ({childDepartments?.length || 0})
        </h2>
      </div>

      {!childDepartments || childDepartments.length === 0 ? (
        <div className="text-center py-4 text-slate-500">
          Нет дочерних подразделений
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
          {childDepartments.map((department) => (
            <DepartmentCard key={department.id} department={department} />
          ))}
        </div>
      )}
      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
