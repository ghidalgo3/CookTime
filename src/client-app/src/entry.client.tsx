import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router";
import { HelmetProvider } from 'react-helmet-async';
import { AuthenticationContext } from './components/Authentication/AuthenticationContext';

// Import layouts
import DefaultLayout from "./pages/DefaultLayout";
import PlainLayout from "./pages/PlainLayout/PlainLayout";

// Import pages
import Home from "./pages/Home";
import Recipe from "./pages/Recipe";
import Favorites from "./pages/Favorites";
import About from "./pages/About";
import MyRecipes from "./pages/MyRecipes";
import RecipeCreation from "./pages/RecipeCreation";
import GroceriesList from "./pages/GroceriesList";
import IngredientsView from "./pages/IngredientsView";
import IngredientNormalizer from "./pages/IngredientNormalizer";
import SignIn from "./pages/SignIn";
import SignUp from "./pages/SignUp";
import ResetPassword from "./pages/ResetPassword";
import Registration from "./pages/Registration";
import CatchAll from "./pages/CatchAll";

// Import styles
import '@smastrom/react-rating/style.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import './assets/css/all.css';
import './assets/css/site.css';



const router = createBrowserRouter([
    {
        element: <DefaultLayout />,
        children: [
            { index: true, element: <Home /> },
            { path: "Recipes/:id", element: <Recipe /> },
            { path: "Recipes/Favorites", element: <Favorites /> },
            { path: "About", element: <About /> },
            { path: "Recipes/Mine", element: <MyRecipes /> },
            { path: "Recipes/Create", element: <RecipeCreation /> },
            { path: "Groceries", element: <GroceriesList /> },
            { path: "Admin/IngredientsView", element: <IngredientsView /> },
            { path: "Admin/IngredientNormalizer", element: <IngredientNormalizer /> },
        ],
    },
    {
        element: <PlainLayout />,
        children: [
            { path: "signin", element: <SignIn /> },
            { path: "signup", element: <SignUp /> },
            { path: "resetpassword", element: <ResetPassword /> },
            { path: "registration", element: <Registration /> },
        ],
    },
    { path: "*", element: <CatchAll /> },
]);



createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <HelmetProvider>
            <AuthenticationContext>
                <RouterProvider router={router} />
            </AuthenticationContext>
        </HelmetProvider>
    </StrictMode>
);


