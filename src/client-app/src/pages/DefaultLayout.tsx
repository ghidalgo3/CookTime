import React, { useState } from "react"
import { Outlet, ScrollRestoration, useLocation, useRouteLoaderData } from "react-router";
import type { Route } from "./+types/DefaultLayout";
import { CookTimeBanner, NavigationBar } from "src/components";
import Footer from "src/components/Footer";
import { getCategories } from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";

export async function clientLoader() {
  const categories = await getCategories();
  return { categories };
}

export default function DefaultLayout({ loaderData }: Route.ComponentProps) {
  const location = useLocation();
  const { categories } = loaderData;
  const [theme] = useState<string>(() => {
    // Check theme preference only on client
    if (typeof window !== 'undefined') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    return 'light';
  });

  useTitle();
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
      <NavigationBar categories={categories} />

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