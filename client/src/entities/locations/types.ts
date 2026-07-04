export interface GetLocationDto {
  id: string;
  name: string;
  officeNumber: string;
  buildingNumber: string;
  street: string;
  city: string;
  stateOrProvince?: string;
  country: string;
  postalCode?: string;
  timezone: string;
  isActive: boolean;
  createdAt: string; // ISO 8601 (например, "2026-05-08T14:30:00Z")
}

export interface GetLocationRequest {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string; // default: "name"
  sortOrder?: string; // default: "asc"
}

export interface AddressDto {
  officeNumber: string;
  buildingNumber: string;
  street: string;
  city: string;
  stateOrProvince?: string;
  country: string;
  postalCode?: string;
}

export interface CreateLocationRequest {
  name: string;
  address: AddressDto;
  timezone: string;
}

export interface EditLocationRequest extends CreateLocationRequest {
  id: string;
}
