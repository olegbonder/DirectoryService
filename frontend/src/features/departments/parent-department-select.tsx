import {
  SelectContent,
  Select,
  SelectTrigger,
  SelectValue,
  SelectItem,
} from "@/shared/components/ui/select";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDepartmentDictionary } from "./model/use-department-dictionary";
import { PAGE_SIZE } from "@/shared/api/types";

type ParentDepartmentSelectProps = {
  parentId: string | undefined;
  setParentId: (id: string | undefined) => void;
};
export default function ParentDepartmentSelect({
  parentId,
  setParentId,
}: ParentDepartmentSelectProps) {
  const { departments, isPending, isError, error } = useDepartmentDictionary({
    pageSize: PAGE_SIZE,
    showOnlyParents: true,
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
          setParentId(value === "none" ? undefined : value)
        }
        value={parentId}
      >
        <SelectTrigger>
          <SelectValue placeholder="Выберите родительское подразделение" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="none">Без родительского подразделения</SelectItem>
          {departments?.map((department) => (
            <SelectItem key={department.id} value={department.id}>
              {department.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </>
  );
}
