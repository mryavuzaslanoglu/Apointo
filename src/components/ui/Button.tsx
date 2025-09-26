import type { ButtonHTMLAttributes, PropsWithChildren } from "react";
import clsx from "clsx";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary";
  isLoading?: boolean;
}

export function Button({
  children,
  className,
  variant = "primary",
  isLoading,
  disabled,
  ...props
}: PropsWithChildren<ButtonProps>) {
  return (
    <button
      className={clsx("btn", `btn-${variant}`, className)}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? "Isleniyor..." : children}
    </button>
  );
}