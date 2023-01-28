import React, {useEffect, useState} from "react"
import { Outlet, useLoaderData, useLocation } from "react-router-dom";
import { CookTimeBanner, NavigationBar } from "src/components";
import Footer from "src/components/Footer";
import { getCategories, getRecipeViews } from "src/shared/CookTime";

export async function loader({request } : {request : Request}) {
  const categories = await getCategories();
  const recipes = await getRecipeViews({
    search: "",
    page: 1
  })
  return {categories, recipes}
}

export default function DefaultLayout() {
  const location = useLocation();
  let {categories, recipes} = useLoaderData() as {recipes : any[], categories: string[]};

  return (
    <>
    <NavigationBar categories={categories as string[]}/>

    <main role="main" className="pb-3">
      <div className="container margin-top-30">
          {/* TODO don't render this always  */}
          {location.pathname === "/" && location.search === "" && 
            <CookTimeBanner />}
          <Outlet />
      </div>
    </main>
    <Footer />
    </>
  );
}