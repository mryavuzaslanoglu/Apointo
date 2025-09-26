import { Form, Formik } from "formik";
import * as Yup from "yup";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { TextInput } from "@/components/ui/TextInput";
import { useAuthActions } from "@/features/auth/hooks/useAuthActions";
import type { LoginPayload } from "@/features/auth/types";

const validationSchema = Yup.object().shape({
  email: Yup.string().email("Gecerli bir e-posta girin").required("E-posta zorunlu"),
  password: Yup.string().required("Sifre zorunlu"),
});

export function LoginPage() {
  const { login } = useAuthActions();

  const initialValues: LoginPayload = {
    email: "",
    password: "",
    device: "web",
  };

  const handleSubmit = async (
    values: LoginPayload,
    { setSubmitting }: { setSubmitting: (isSubmitting: boolean) => void }
  ) => {
    try {
      await login(values);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <h1>Giris Yap</h1>
      <Formik initialValues={initialValues} validationSchema={validationSchema} onSubmit={handleSubmit}>
        {({ isSubmitting }) => (
          <Form className="form">
            <TextInput name="email" label="E-posta" placeholder="ornek@domain.com" />
            <PasswordInput name="password" label="Sifre" placeholder="********" />
            <Button type="submit" isLoading={isSubmitting}>
              Giris Yap
            </Button>
            <div className="form-links">
              <Link to="/auth/forgot-password">Sifremi Unuttum</Link>
              <Link to="/auth/register">Hesabin yok mu? Kayit ol</Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
}