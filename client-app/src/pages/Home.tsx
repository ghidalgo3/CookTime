import React, {useEffect, useState} from "react"
import { Col, Row } from "react-bootstrap";
import ReactPaginate from 'react-paginate';
import { getRecipeViews, Image, PagedResult, RecipeView } from "src/shared/CookTime"
import { Link, LoaderFunctionArgs, useLoaderData, useLocation, useSearchParams } from "react-router-dom";
import PaginatedList from "src/components/PaginatedList/PaginatedList";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import RecipeList from "src/components/RecipeList/RecipeList";

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
  const { user } = useAuthentication();
  return (
    <>
      {
        !query && 
        <>
          <RecipeList title="Featured Recipes" />
          <RecipeList title="New Recipes!" />
        </>
      }
      <RecipeList title="Recipes" />
    </>);
}