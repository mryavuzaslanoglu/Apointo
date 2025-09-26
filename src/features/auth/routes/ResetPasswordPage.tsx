import { Form, Formik } from "formik";
import * as Yup from "yup";
import { Link, useSearchParams } from "react-router-dom";
import { Button } from "../../../components/ui/Button";
import { PasswordInput } from "../../../components/ui/PasswordInput";
import { useAuthActions } from "../hooks/useAuthActions";
import type { ResetPasswordPayload } from "../types";

interface ResetPasswordFormValues {
  newPassword: string;
  confirmPassword: string;
}

const validationSchema = Yup.object().shape({
  newPassword: Yup.string().min(8, "En az 8 karakter olmali").required("Sifre zorunlu"),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref("newPassword")], "Sifreler eslesmiyor")
    .required("Sifre tekrari zorunlu"),
});

export function ResetPasswordPage() {
  const { resetPassword } = useAuthActions();
  const [searchParams] = useSearchParams();

  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  if (!userId || !token) {
    return (
      <div className="auth-page">
        <h1>Gecersiz baglanti</h1>
        <p>Sifre sifirlama baglantisi gecersiz veya suresi dolmus.</p>
        <div className="form-links single">
          <Link to="/auth/forgot-password">Yeni baglanti iste</Link>
        </div>
      </div>
    );
  }

  const initialValues: ResetPasswordFormValues = {
    newPassword: "",
    confirmPassword: "",
  };

  const handleSubmit = async (
    values: ResetPasswordFormValues,
    { setSubmitting }: { setSubmitting: (isSubmitting: boolean) => void }
  ) => {
    const payload: ResetPasswordPayload = {
      userId,
      token,
      newPassword: values.newPassword,
      confirmPassword: values.confirmPassword,
    };

    try {
      await resetPassword(payload);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <h1>Sifreyi Sifirla</h1>
      <Formik initialValues={initialValues} validationSchema={validationSchema} onSubmit={handleSubmit}>
        {({ isSubmitting }) => (
          <Form className="form">
            <PasswordInput name="newPassword" label="Yeni Sifre" placeholder="********" />
            <PasswordInput name="confirmPassword" label="Sifre Tekrari" placeholder="********" />
            <Button type="submit" isLoading={isSubmitting}>
              Sifreyi Guncelle
            </Button>
            <div className="form-links single">
              <Link to="/auth/login">Giris sayfasina don</Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
}