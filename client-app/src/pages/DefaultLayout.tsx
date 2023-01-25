import React, {useEffect, useState} from"react"
import { Outlet, useLoaderData, useLocation } from "react-router-dom";
import { CookTimeBanner, NavigationBar, SignUp } from "src/components";
import Footer from "src/components/Footer";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";
import { Category } from "src/shared/CookTime";
import { getCategories } from "src/shared/CookTime.service";

export default function DefaultLayout() {
  const location = useLocation();
  let categories = useLoaderData();

  return (
    <>
    <NavigationBar categories={categories as string[]}/>
    {/* TODO don't render this always  */}
    {/* How to render the banner only at the home page */}
    {location.pathname === "/" && <CookTimeBanner />}
    <Outlet />
    <RecipeCard
      recipeId={"12"}
      recipeName={"Recipe"}
      averageReviews={2.7}
      reviewCount={2}
      isFavorite={false}
      categories={["a", "b"]}/>
    <Footer />
    </>
  );
}