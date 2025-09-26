import type { InputHTMLAttributes } from "react";
import type { FieldHookConfig } from "formik";
import { useField } from "formik";
import clsx from "clsx";

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
}

export function TextInput({ label, type = "text", ...props }: TextInputProps & FieldHookConfig<string>) {
  const [field, meta] = useField<string>(props);
  const hasError = meta.touched && Boolean(meta.error);

  return (
    <label className={clsx("form-field", hasError && "has-error")}>
      <span className="form-label">{label}</span>
      <input className="form-input" type={type} {...field} {...props} />
      {hasError ? <span className="form-error">{meta.error}</span> : null}
    </label>
  );
}