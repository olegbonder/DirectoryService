import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/shared/components/ui/breadcrumb";
import React from "react";

type DepartmentPathProps = {
  path: string;
  itemClassName?: string;
};
const delimiter = ".";
export default function DepartmentPath({
  path,
  itemClassName,
}: DepartmentPathProps) {
  return (
    <Breadcrumb>
      <BreadcrumbList className="flex-wrap">
        {path
          .split(delimiter)
          .filter((part) => part.trim() !== "")
          .map((part, index, array) => (
            <React.Fragment key={index}>
              <BreadcrumbItem key={index}>
                <BreadcrumbPage className={itemClassName}>
                  {part.trim()}
                </BreadcrumbPage>
              </BreadcrumbItem>
              {index < array.length - 1 && <BreadcrumbSeparator />}
            </React.Fragment>
          ))}
      </BreadcrumbList>
    </Breadcrumb>
  );
}
