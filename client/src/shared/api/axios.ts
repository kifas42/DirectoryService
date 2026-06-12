import axios from "axios";
import { ApiEnvelope } from "./envelope";
import { EnvelopeError } from "./error";

export const apiClient = axios.create({
  baseURL: "http://localhost:8001/api/",
  headers: { "Content-Type": "application/json" },
  timeout: 15000,
});

apiClient.interceptors.response.use(
  (response) => {
    const data = response.data as ApiEnvelope;

    if (data.isError && data.error) {
    }
    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as ApiEnvelope;

      if (envelope?.isError && envelope.error) {
        throw new EnvelopeError(envelope.error);
      }
    }

    return Promise.reject(error);
  },
);
