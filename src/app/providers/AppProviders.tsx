import { ReactNode } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

type AppProvidersProps = {
  children: ReactNode;
};

export function AppProviders({ children }: AppProvidersProps) {
  return (
    <BrowserRouter>
      {children}
      <ToastContainer position="top-right" autoClose={5000} newestOnTop pauseOnFocusLoss={false} />
    </BrowserRouter>
  );
}