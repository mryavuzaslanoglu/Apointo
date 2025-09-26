import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { authApi } from '@/features/auth/api/authApi';
import {
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
} from '@/features/auth/types';
import { useAuthStore } from '@/store/authStore';
import { getErrorMessage } from '@/utils/error';

export function useAuthActions() {
  const navigate = useNavigate();
  const setCredentials = useAuthStore((state) => state.setCredentials);
  const logoutStore = useAuthStore((state) => state.logout);
  const user = useAuthStore((state) => state.user);
  const refreshToken = useAuthStore((state) => state.refreshToken);

  const login = useCallback(
    async (payload: LoginPayload) => {
      try {
        const { data } = await authApi.login(payload);
        setCredentials(data);
        toast.success('Hoþ geldiniz!');
        navigate('/', { replace: true });
      } catch (error) {
        toast.error(getErrorMessage(error, 'Giriþ yapýlamadý.'));
        throw error;
      }
    },
    [navigate, setCredentials]
  );

  const register = useCallback(
    async (payload: RegisterPayload) => {
      try {
        const { data } = await authApi.register(payload);
        setCredentials(data);
        toast.success('Kayýt baþarýlý.');
        navigate('/', { replace: true });
      } catch (error) {
        toast.error(getErrorMessage(error, 'Kayýt iþlemi baþarýsýz.'));
        throw error;
      }
    },
    [navigate, setCredentials]
  );

  const forgotPassword = useCallback(async (payload: ForgotPasswordPayload) => {
    try {
      await authApi.forgotPassword(payload);
      toast.success('Þifre sýfýrlama talimatý e-postanýza gönderildi.');
    } catch (error) {
      toast.error(getErrorMessage(error, 'Þifre sýfýrlama isteði baþarýsýz.'));
      throw error;
    }
  }, []);

  const resetPassword = useCallback(async (payload: ResetPasswordPayload) => {
    try {
      await authApi.resetPassword(payload);
      toast.success('Þifreniz güncellendi.');
      navigate('/auth/login', { replace: true });
    } catch (error) {
      toast.error(getErrorMessage(error, 'Þifre güncellenemedi.'));
      throw error;
    }
  }, [navigate]);

  const logout = useCallback(async () => {
    try {
      if (user && refreshToken) {
        await authApi.logout(user.id, refreshToken);
      }
    } catch (error) {
      console.error('Logout isteði baþarýsýz:', error);
    } finally {
      logoutStore();
      navigate('/auth/login', { replace: true });
    }
  }, [logoutStore, navigate, refreshToken, user]);

  return {
    login,
    register,
    forgotPassword,
    resetPassword,
    logout,
  };
}