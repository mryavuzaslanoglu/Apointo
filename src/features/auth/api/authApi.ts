import { api } from "../../services/api/httpClient";
import type {
  AuthenticationResponse,
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
} from "../types";

const basePath = "/api/auth";

export const authApi = {
  login: (payload: LoginPayload) =>
    api.post<AuthenticationResponse>(`${basePath}/login`, {
      ...payload,
      device: payload.device ?? "web",
    }),
  register: (payload: RegisterPayload) =>
    api.post<AuthenticationResponse>(`${basePath}/register`, {
      ...payload,
      device: payload.device ?? "web",
    }),
  refresh: (userId: string, refreshToken: string) =>
    api.post<AuthenticationResponse>(`${basePath}/refresh`, {
      userId,
      refreshToken,
      device: "web",
    }),
  logout: (userId: string, refreshToken: string) =>
    api.post(`${basePath}/logout`, {
      userId,
      refreshToken,
    }),
  forgotPassword: (payload: ForgotPasswordPayload) => api.post(`${basePath}/forgot-password`, payload),
  resetPassword: (payload: ResetPasswordPayload) => api.post(`${basePath}/reset-password`, payload),
};