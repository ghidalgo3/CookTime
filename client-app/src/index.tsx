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
import { action as signInAction, SignIn } from './pages/SignIn';
import { AuthenticationProvider, IAuthenticationProvider } from './shared/AuthenticationProvider';
import { AuthenticationContext, useAuthentication } from './components/Authentication/AuthenticationContext';
import DefaultLayout, {loader as recipeLoader} from './pages/DefaultLayout';
import { getCategories } from './shared/CookTime';
import '@smastrom/react-rating/style.css'
import RecipeList, {loader as recipeListLoader } from './components/RecipeList/RecipeList';
import Recipe from './pages/Recipe';
import GroceriesList from './pages/GroceriesList';
import PlainLayout from './pages/PlainLayout/PlainLayout';
import SignUp, { action as signUpAction } from './pages/SignUp';
import RecipeCreation, {action as createRecipe} from './pages/RecipeCreation';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
function App() {
  const authProvider = useAuthentication();
  const router = createBrowserRouter(createRoutesFromElements(
    <>
      {/* Top level route defines layout */}
      <Route
        element={<DefaultLayout />}
        loader={recipeLoader}
      >
        <Route
          index
          path="/"
          loader={recipeListLoader}
          element={<RecipeList />} />

        <Route path="Recipes/Details" element={<Recipe />} />
        <Route
          path="Recipes/Create"
          element={<RecipeCreation />}
          action={createRecipe} />
        <Route path="Cart" element={<GroceriesList />} />
      </Route>

      {/* Distinct signup, signin routes */}
      <Route
        element={<PlainLayout />}>
        <Route
          path="/signin"
          action={async (actionArgs) => {
            return await (signInAction(authProvider)(actionArgs));
          }}
          element={<SignIn />}></Route>

        <Route
          path="/signup"
          action={async (actionArgs) => {
            return await (signUpAction(authProvider)(actionArgs));
          }}
          element={<SignUp />}></Route>
      </Route>

      <Route
        path="*"
        element={<Navigate to="/" replace />} />
    </>
  ));
  return (
    <React.StrictMode>
      <RouterProvider router={router} />
    </React.StrictMode>
  )
}

// This is the file that contains all the global state
root.render(
<AuthenticationContext>
  <App />
</AuthenticationContext>);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
