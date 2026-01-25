import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";

export type OrderDirection = "asc" | "desc";

const orderByOptions = [
  { value: "asc", label: "По возрастанию" },
  { value: "desc", label: "По убыванию" },
];

type SortSelectProps = {
  order: OrderDirection;
  setOrder: (order: OrderDirection) => void;
};
export default function SortSelect({ order, setOrder }: SortSelectProps) {
  return (
    <Select
      onValueChange={(value: OrderDirection) => setOrder(value)}
      value={order}
    >
      <SelectTrigger>
        <SelectValue placeholder="Cортировка" />
      </SelectTrigger>
      <SelectContent>
        {orderByOptions.map((order) => (
          <SelectItem key={order.value} value={order.value}>
            {order.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
