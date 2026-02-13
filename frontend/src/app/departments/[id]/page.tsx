import DepartmentDetail from "@/features/departments/department-detail";

type Props = {
  params: Promise<{ id: string }>;
};

export default async function DepartmentPage({ params }: Props) {
  const { id } = await params;

  return <DepartmentDetail id={id} />;
}
