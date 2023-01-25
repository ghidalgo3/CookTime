import React, {useEffect, useState} from"react"
import { Col, Row } from "react-bootstrap";
import { Outlet, useLoaderData, useLocation } from "react-router-dom";
import { CookTimeBanner, NavigationBar, SignUp } from "src/components";
import Footer from "src/components/Footer";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";
import { getCategories, getRecipeCards } from "src/shared/CookTime";

export async function loader({request } : {request : Request}) {
  const categories = await getCategories();
  const recipes = await getRecipeCards({
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
          {/* How to render the banner only at the home page */}
          {location.pathname === "/" && <CookTimeBanner />}
          <Outlet />
          <Row>
            {recipes?.map((recipe, idx) =>
              <Col key={idx} sm={4}>
                <RecipeCard
                  key={idx}
                  {...recipe}
                />
              </Col>
            )}
          </Row>
      </div>
    </main>
    <Footer />
    </>
  );
}