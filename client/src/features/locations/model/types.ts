import { z } from "zod";

export const locationSchema = z.object({
  name: z
    .string()
    .min(1, "Имя обязательно")
    .min(3, "Минимум 3 символа")
    .max(200, "Не должно превышать 200 символов"),
  officeNumber: z.string().min(1, "Укажите номер офиса/помещения"),
  buildingNumber: z.string().min(1, "Укажите номер здания"),
  street: z.string().min(1, "Укажите улицу"),
  city: z.string().min(1, "Укажите город"),
  country: z.string().min(1, "Укажите страну"),
  timezone: z.string().min(1, "Выберите часовой пояс"),
  stateOrProvince: z.string().optional(),
  postalCode: z.string().optional(),
  _addressGroupError: z.string().optional(),
});

export type LocationFormValues = z.infer<typeof locationSchema>;

export interface DataTableFilterParams {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortOrder: "asc" | "desc";
  isActive?: boolean;
}
