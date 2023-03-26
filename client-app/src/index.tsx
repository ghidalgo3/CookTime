import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import React from 'react';
import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Navigate,
  Route,
  RouterProvider
} from "react-router-dom";
// import './index.css';
import '@smastrom/react-rating/style.css';
import 'bootstrap/dist/css/bootstrap.min.css'; // bootstrap
import './assets/css/all.css'; // fontawesome
import './assets/css/site.css'; // ours
import { AuthenticationContext, useAuthentication } from './components/Authentication/AuthenticationContext';
import DefaultLayout, { loader as categoryLoader } from './pages/DefaultLayout';
import { action as signInAction, SignIn } from './pages/SignIn';
import reportWebVitals from './reportWebVitals';
import { AuthenticationProvider, IAuthenticationProvider } from './shared/AuthenticationProvider';
import { getCategories } from './shared/CookTime';
// import RecipeList, {loader as recipeListLoader } from './components/RecipeList/RecipeList';
import Favorites from './pages/Favorites';
import GroceriesList from './pages/GroceriesList';
import Home, { loader as recipeListLoader } from './pages/Home';
import IngredientNormalizer from './pages/IngredientNormalizer';
import IngredientsView from './pages/IngredientsView';
import MyRecipes from './pages/MyRecipes';
import PlainLayout from './pages/PlainLayout/PlainLayout';
import Recipe, {loader as recipeLoader} from './pages/Recipe';
import RecipeCreation, { action as createRecipe } from './pages/RecipeCreation';
import Registration, { action as finishRegistration } from './pages/Registration';
import ResetPassword, { action as sendPasswordResetEmail } from './pages/ResetPassword';
import SignUp, { action as signUpAction } from './pages/SignUp';

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
        loader={categoryLoader}
      >
        <Route
          index
          path="/"
          loader={recipeListLoader}
          element={<Home />} />

        <Route path="Recipes/Details"
          loader={recipeLoader}
          element={<Recipe />} />
        <Route path="Recipes/Favorites" element={<Favorites />} />
        <Route path="Recipes/Mine" element={<MyRecipes />} />
        <Route
          path="Recipes/Create"
          element={<RecipeCreation />}
          action={createRecipe} />
        <Route path="Cart" element={<GroceriesList />} />
        <Route path="Admin/IngredientsView" element={<IngredientsView />} />
        <Route path="Admin/IngredientNormalizer" element={<IngredientNormalizer />} />
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

        <Route
          path="/resetPassword"
          action={async (actionArgs) => {
            return await (sendPasswordResetEmail(authProvider)(actionArgs))
          }}
          element={<ResetPassword />}></Route>

        <Route
          path="/registration"
          action={async (actionArgs) => {
            return await (finishRegistration(authProvider)(actionArgs))
          }}
          element={<Registration />}></Route>
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

const appInsights = new ApplicationInsights({ config: {
  connectionString: 'InstrumentationKey=b37afa75-076b-4438-a84d-79b9f4617d30;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/',
  enableAutoRouteTracking: true,
  enableCorsCorrelation: true,
  enableRequestHeaderTracking: true,
  enableResponseHeaderTracking: true,
  correlationHeaderExcludedDomains: ['*.queue.core.windows.net']
  /* ...Other Configuration Options... */
} });
appInsights.loadAppInsights();
appInsights.trackPageView(); // Manually call trackPageView to establish the current user/session/pageview

// This is the file that contains all the global state
root.render(
<AuthenticationContext>
  <App />
</AuthenticationContext>);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
