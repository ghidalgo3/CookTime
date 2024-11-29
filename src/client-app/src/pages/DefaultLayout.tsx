import React, { useEffect, useState } from "react"
import { Outlet, ScrollRestoration, useLoaderData, useLocation } from "react-router";
import { CookTimeBanner, NavigationBar } from "src/components";
import Footer from "src/components/Footer";
import { getCategories, getRecipeViews } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

export async function loader({ request }: { request: Request }) {
  const categories = await getCategories();
  return { categories }
}

export default function DefaultLayout() {
  const location = useLocation();
  let { categories } = useLoaderData() as { categories: string[] };

  useTitle();
  let theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  return (
    <>
      {/* Scroll restoration is bringing the user to the top of the page in every refresh, which is desirable in most pages.
    The location.pathname should allow us to keep previous scroll height on the recipe list but it's not working as intended right now. */}
      <ScrollRestoration
        getKey={(location, matches) => {
          return location.pathname === "/"
            ?
            location.pathname + location.search
            :
            location.key;
        }}
      />
      <NavigationBar categories={categories as string[]} />

      <main data-bs-theme={theme} role="main" className="pb-3">
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