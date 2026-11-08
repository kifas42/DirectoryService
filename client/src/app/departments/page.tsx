"use client";

import { GetDepartmentDto } from "@/entities/departments/types";
import DepartmentSelect from "@/features/department-select/department-select";
import { useState } from "react";

export default function Departments() {
  const [singleSelected, setSingleSelected] = useState<GetDepartmentDto[]>([]);
  const [multiSelected, setMultiSelected] = useState<GetDepartmentDto[]>([]);
  return (
    <>
      <span>Departments Single</span>
      <DepartmentSelect
        selectedDepartments={singleSelected}
        onChange={setSingleSelected}
        mode="single"
        excludeIds={[
          "506c7a6e-13a7-4d0b-b6f0-005746637682",
          "7aedc924-e97c-4833-a1fd-b2198db862f1",
        ]}
      />

      <span className="block mt-6">Departments Multiplie</span>
      <DepartmentSelect
        selectedDepartments={multiSelected}
        onChange={setMultiSelected}
        mode="multiple"
        excludeIds={[]}
      />
    </>
  );
}
