import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";

const statuses = [
  { value: "all", label: "Все" },
  { value: "active", label: "Активные" },
  { value: "deleted", label: "Удаленные" },
];

type StatusSelectProps = {
  isActive: boolean | undefined;
  onIsActiveChange: (isActive: boolean | undefined) => void;
};
export default function StatusSelect({
  isActive,
  onIsActiveChange,
}: StatusSelectProps) {
  const handleIsActiveChange = (value: string) => {
    if (value === "all") {
      onIsActiveChange(undefined);
    } else if (value === "active") {
      onIsActiveChange(true);
    } else {
      onIsActiveChange(false);
    }
  };
  return (
    <Select
      onValueChange={(value) => handleIsActiveChange(value)}
      value={isActive === undefined ? "all" : isActive ? "active" : "deleted"}
    >
      <SelectTrigger>
        <SelectValue placeholder="Статус" />
      </SelectTrigger>
      <SelectContent>
        {statuses.map((status) => (
          <SelectItem key={status.value} value={status.value}>
            {status.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
