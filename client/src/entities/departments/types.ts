export interface GetDepartmentDto {
  id: string;
  parentId: string | null;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  children: GetDepartmentDto[];
  hasMoreChildren: boolean;
}

export interface GetDepartmentsRequest {
  search?: string;
  isActive?: boolean;
  locationIds?: string[];
  excludeIds?: string[];
  parentId?: string;
  page?: number; // default: 1
  pageSize?: number; // default: 20
  sortBy?: string; // default: "name"
  sortOrder?: "asc" | "desc"; // default: "asc"
}
