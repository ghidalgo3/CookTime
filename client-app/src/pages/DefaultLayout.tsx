import React, {useEffect, useState} from "react"
import { Outlet, useLoaderData, useLocation } from "react-router-dom";
import { CookTimeBanner, NavigationBar } from "src/components";
import Footer from "src/components/Footer";
import { getCategories, getRecipeViews } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

export async function loader({request } : {request : Request}) {
  const categories = await getCategories();
  return {categories}
}

export default function DefaultLayout() {
  const location = useLocation();
  let {categories} = useLoaderData() as {categories: string[]};

  useTitle();
  return (
    <>
    <NavigationBar categories={categories as string[]}/>

    <main role="main" className="pb-3">
      <div className="container margin-top-30">
          {/* TODO don't render this always  */}
          {location.pathname === "/" && location.search === "" && 
            <CookTimeBanner /> }
          <Outlet />
      </div>
    </main>
    <Footer />
    </>
  );
}