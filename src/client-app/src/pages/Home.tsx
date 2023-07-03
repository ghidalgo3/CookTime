import React, {useEffect, useState} from "react"
import { Col, Row } from "react-bootstrap";
import { getRecipeViews, Image, PagedResult, RecipeView } from "src/shared/CookTime"
import { Link, LoaderFunctionArgs, useLoaderData, useLocation, useSearchParams } from "react-router-dom";
import PaginatedList from "src/components/PaginatedList/PaginatedList";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import RecipeList from "src/components/RecipeList/RecipeList";
import { Helmet } from "react-helmet-async";

export async function loader(load : LoaderFunctionArgs) {
  const {request, params} = load;
  const url = new URL(request.url);
  const search = url.searchParams.get("search") ?? "";
  const page = Number.parseInt(url.searchParams.get("page") ?? "1");
  return await getRecipeViews({search, page});
}

export default function Home() {
  const recipes = useLoaderData() as PagedResult<RecipeView>;
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get("search");
  const page = searchParams.get("page") ?? "1";
  const { user } = useAuthentication();
  return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/`} />
      </Helmet>
      {
        !query && page === "1" &&
        <>
          <RecipeList
            title="Featured Recipes"
            type="Featured"
            hideIfEmpty/>
          <RecipeList
            title="New Recipes!"
            type="New"
            hideIfEmpty/>
        </>
      }
      {query && <p>Searching for '{query}'</p>}
      <RecipeList title="Recipes" type="Query" query={searchParams}/>
    </>);
}