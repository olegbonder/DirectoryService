import axios from "axios";
import { Envelope } from "./envelope";
import { ApiError, EnvelopeError } from "./errors";

export const apiClient = axios.create({
  baseURL: "http://localhost:5158/api",
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.response.use(
  (response) => {
    const data = response.data as Envelope;

    if (data.isError && data.errorList) {
      throw new EnvelopeError(data.errorList);
    }
    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as Envelope;
      if (envelope.isError && envelope.errorList) {
        const errorList = envelope.errorList as ApiError[];
        throw new EnvelopeError(errorList);
      }
    }
    return Promise.reject(error);
  }
);
