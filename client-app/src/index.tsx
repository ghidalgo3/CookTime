import React from 'react';
import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";
import './index.css';
import reportWebVitals from './reportWebVitals';
import 'bootstrap/dist/css/bootstrap.min.css';
import './assets/css/site.css';
import './assets/css/all.css';
import App from './App';
import { SignIn } from './pages/SignIn';
import { action as signinAction } from "./components/Authentication/SignUp"
import { AuthenticationProvider, IAuthenticationProvider } from './shared/AuthenticationProvider';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
  },
  {
    path: "/signin",
    action: signinAction,
    element: <SignIn />,
  },
]);

export let AuthContext = React.createContext<IAuthenticationProvider>(null!);

function AuthProvider({ children }: { children: React.ReactNode }) {
  let value = AuthenticationProvider;
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
// This is the file that contains all the global state
root.render(
  <React.StrictMode>
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
