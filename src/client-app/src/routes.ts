import type { RouteConfig } from "@react-router/dev/routes";
import { index, route, layout } from "@react-router/dev/routes";

export default [
    // Main layout with navigation
    layout("pages/DefaultLayout.tsx", { id: "defaultLayout" }, [
        index("pages/Home.tsx"),
        route("Recipes/:id", "pages/Recipe.tsx"),
        route("Recipes/Favorites", "pages/Favorites.tsx"),
        route("About", "pages/About.tsx"),
        route("Recipes/Mine", "pages/MyRecipes.tsx"),
        route("Recipes/Create", "pages/RecipeCreation.tsx"),
        route("Groceries", "pages/GroceriesList.tsx"),
        route("Admin/IngredientsView", "pages/IngredientsView.tsx"),
        route("Admin/IngredientNormalizer", "pages/IngredientNormalizer.tsx"),
    ]),

    // Plain layout for auth pages
    layout("pages/PlainLayout/PlainLayout.tsx", [
        route("signin", "pages/SignIn.tsx"),
        route("profile/setup", "pages/ProfileSetup.tsx"),
    ]),

    // Catch-all redirect
    route("*", "pages/CatchAll.tsx"),
] satisfies RouteConfig;
