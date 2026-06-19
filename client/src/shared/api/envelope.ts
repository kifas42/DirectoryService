import { ApiError } from "./error";

export type ApiEnvelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};
