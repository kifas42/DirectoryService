import { Path } from "react-hook-form";
import { LocationFormValues } from "./model/types";

export const locationErrorMap: Record<string, Path<LocationFormValues>> = {
  "location.name.conflict": "name",
  "location.address.conflict": "_addressGroupError",
  "validation.timezone.invalid_format": "timezone",
  "validation.postal_code.invalid_format": "postalCode",
};
