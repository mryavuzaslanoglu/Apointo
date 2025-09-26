import { Navigate, Route, Routes } from "react-router-dom";
import { AuthLayout } from "../../components/layout/AuthLayout";
import { MainLayout } from "../../components/layout/MainLayout";
import { DashboardPage } from "../../features/dashboard/routes/DashboardPage";
import { ForgotPasswordPage } from "../../features/auth/routes/ForgotPasswordPage";
import { LoginPage } from "../../features/auth/routes/LoginPage";
import { RegisterPage } from "../../features/auth/routes/RegisterPage";
import { ResetPasswordPage } from "../../features/auth/routes/ResetPasswordPage";
import { ProtectedRoute } from "./ProtectedRoute";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/auth" element={<AuthLayout />}>
        <Route index element={<Navigate to="login" replace />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route path="reset-password" element={<ResetPasswordPage />} />
      </Route>

      <Route
        path="/"
        element={(
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        )}
      >
        <Route index element={<DashboardPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}