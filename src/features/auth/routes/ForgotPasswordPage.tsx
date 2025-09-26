import { Form, Formik } from "formik";
import * as Yup from "yup";
import { Link } from "react-router-dom";
import { Button } from "../../../components/ui/Button";
import { TextInput } from "../../../components/ui/TextInput";
import { useAuthActions } from "../hooks/useAuthActions";
import type { ForgotPasswordPayload } from "../types";

const validationSchema = Yup.object().shape({
  email: Yup.string().email("Gecerli bir e-posta girin").required("E-posta zorunlu"),
});

export function ForgotPasswordPage() {
  const { forgotPassword } = useAuthActions();

  const initialValues: ForgotPasswordPayload = {
    email: "",
    clientBaseUrl: typeof window !== "undefined" ? window.location.origin : undefined,
  };

  const handleSubmit = async (
    values: ForgotPasswordPayload,
    { setSubmitting }: { setSubmitting: (isSubmitting: boolean) => void }
  ) => {
    try {
      await forgotPassword(values);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <h1>Sifremi Unuttum</h1>
      <p className="form-subtitle">E-posta adresinizi girin, size sifre sifirlama baglantisi gonderelim.</p>
      <Formik initialValues={initialValues} enableReinitialize validationSchema={validationSchema} onSubmit={handleSubmit}>
        {({ isSubmitting }) => (
          <Form className="form">
            <TextInput name="email" label="E-posta" placeholder="ornek@domain.com" />
            <Button type="submit" isLoading={isSubmitting}>
              Baglanti Gonder
            </Button>
            <div className="form-links single">
              <Link to="/auth/login">Girise geri don</Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
}