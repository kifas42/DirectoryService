export interface GetPositionDto {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
}

export interface GetPositionRequest {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  cursor?: string;
  limit: number; // default: 10
  sortBy?: string; // default: "name"
  sortOrder?: string; // default: "asc"
}

export interface CreatePositionRequest {
  name: string;
}
