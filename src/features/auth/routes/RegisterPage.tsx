import { Form, Formik } from "formik";
import * as Yup from "yup";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { TextInput } from "@/components/ui/TextInput";
import { useAuthActions } from "@/features/auth/hooks/useAuthActions";
import type { RegisterPayload } from "@/features/auth/types";

interface RegisterFormValues extends RegisterPayload {
  confirmPassword: string;
}

const validationSchema = Yup.object().shape({
  firstName: Yup.string().required("Ad zorunlu"),
  lastName: Yup.string().required("Soyad zorunlu"),
  email: Yup.string().email("Gecerli bir e-posta girin").required("E-posta zorunlu"),
  password: Yup.string().min(8, "En az 8 karakter olmali").required("Sifre zorunlu"),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref("password")], "Sifreler eslesmiyor")
    .required("Sifre tekrari zorunlu"),
});

export function RegisterPage() {
  const { register } = useAuthActions();

  const initialValues: RegisterFormValues = {
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
    role: "Customer",
    device: "web",
  };

  const handleSubmit = async (
    values: RegisterFormValues,
    { setSubmitting }: { setSubmitting: (isSubmitting: boolean) => void }
  ) => {
    try {
      const { confirmPassword, ...payload } = values;
      await register(payload);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <h1>Kayit Ol</h1>
      <Formik initialValues={initialValues} validationSchema={validationSchema} onSubmit={handleSubmit}>
        {({ isSubmitting }) => (
          <Form className="form">
            <TextInput name="firstName" label="Ad" placeholder="Adiniz" />
            <TextInput name="lastName" label="Soyad" placeholder="Soyadiniz" />
            <TextInput name="email" label="E-posta" placeholder="ornek@domain.com" />
            <PasswordInput name="password" label="Sifre" placeholder="********" />
            <PasswordInput name="confirmPassword" label="Sifre Tekrari" placeholder="********" />
            <Button type="submit" isLoading={isSubmitting}>
              Kayit Ol
            </Button>
            <div className="form-links">
              <Link to="/auth/login">Zaten hesabin var mi? Giris yap</Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
}