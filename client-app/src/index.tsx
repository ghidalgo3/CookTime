import React from 'react';
import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Navigate,
  Route,
  RouterProvider,
} from "react-router-dom";
import './index.css';
import reportWebVitals from './reportWebVitals';
import 'bootstrap/dist/css/bootstrap.min.css';
import './assets/css/site.css';
import './assets/css/all.css';
import { SignIn } from './pages/SignIn';
import { action as signinAction } from "./components/Authentication/SignUp"
import { AuthenticationProvider, IAuthenticationProvider } from './shared/AuthenticationProvider';
import { AuthenticationContext } from './components/Authentication/AuthenticationContext';
import DefaultLayout from './pages/DefaultLayout';
import { getCategories } from './shared/CookTime';
import '@smastrom/react-rating/style.css'

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

const router = createBrowserRouter(createRoutesFromElements(
  <>
    {/* Top level route defines layout */}
    <Route
      path="/"
      element={<DefaultLayout />}
      loader={() => getCategories()}
      >
      <Route path="Recipes/Details" element={<h1>We now at the recipe</h1>}>
      </Route>
    </Route>

    {/* Distinct signup, signin routes */}
    <Route path="/signin" element={<SignIn />} action={signinAction} />

    <Route
      path="*"
      element={<Navigate to="/" replace />} />
  </>
));

// This is the file that contains all the global state
root.render(
  <React.StrictMode>
    <AuthenticationContext>
      <RouterProvider router={router} />
    </AuthenticationContext>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
