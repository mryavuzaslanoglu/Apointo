import type { AxiosError } from "axios";

export function getErrorMessage(error: unknown, fallback = "Beklenmeyen bir hata olustu."): string {
  if (!error) {
    return fallback;
  }

  if (typeof error === "string") {
    return error;
  }

  if ((error as AxiosError)?.isAxiosError) {
    const axiosError = error as AxiosError<{ detail?: string; message?: string; errors?: string[] }>;
    const detail = axiosError.response?.data?.detail;
    const message = axiosError.response?.data?.message;
    const errors = axiosError.response?.data?.errors;

    if (detail) {
      return detail;
    }

    if (message) {
      return message;
    }

    if (Array.isArray(errors) && errors.length > 0) {
      return errors.join("\n");
    }

    return axiosError.message || fallback;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return fallback;
}