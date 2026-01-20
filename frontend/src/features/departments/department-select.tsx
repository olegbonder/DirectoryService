import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useDepartmentDictionary } from "./model/use-department-dictionary";
import { MultiSelect } from "@/shared/components/ui/multi-select";

type DepartmentProps = {
  departmentIds: string[];
  onDepartmentChange: (departmentIds: string[]) => void;
};

export default function DepartmentSelect({
  departmentIds,
  onDepartmentChange,
}: DepartmentProps) {
  const { departments, isPending, isError, error } = useDepartmentDictionary();

  const handleChange = (value: string) => {
    if (value === "all") {
      onDepartmentChange([]);
    } else {
      onDepartmentChange([value]);
    }
  };

  return (
    <>
      {/*!isPending && !isError && departments && departments.length > 0 && (
        <MultiSelect
          options={departments.map((dept) => ({
            value: dept.id,
            label: dept.name,
          }))}
          onValueChange={onDepartmentChange}
          defaultValue={departmentIds}
        />
      )}*/}
      {!isPending && !isError && departments && departments.length > 0 && (
        <Select
          onValueChange={handleChange}
          value={departmentIds?.[0] ?? "all"}
        >
          <SelectTrigger>
            <SelectValue placeholder="Выберите подразделение" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Все</SelectItem>
            {!isPending &&
              departments &&
              departments.length > 0 &&
              departments.map((department) => (
                <SelectItem key={department.id} value={department.id}>
                  {department.name}
                </SelectItem>
              ))}
          </SelectContent>
        </Select>
      )}
    </>
  );
}
