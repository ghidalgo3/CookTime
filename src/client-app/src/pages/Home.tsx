import React, { useEffect, useState } from "react"
import { Col, Row } from "react-bootstrap";
import { getRecipeViews, Image, PagedResult, RecipeView } from "src/shared/CookTime"
import { Link, useLocation, useSearchParams } from "react-router";
import PaginatedList from "src/components/PaginatedList/PaginatedList";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import RecipeList from "src/components/RecipeList/RecipeList";
import { Helmet } from "react-helmet-async";

export default function Home() {
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get("search");
  const page = searchParams.get("page") ?? "1";
  const { user } = useAuthentication();

  // Only access window.location.origin on the client
  const canonicalUrl = typeof window !== 'undefined' ? `${window.location.origin}/` : '/';

  return (
    <>
      <Helmet>
        <link rel="canonical" href={canonicalUrl} />
      </Helmet>
      {
        !query && page === "1" &&
        <>
          <RecipeList
            title="Featured Recipes"
            type="Featured"
            hideIfEmpty />
          <RecipeList
            title="New Recipes!"
            type="New"
            hideIfEmpty />
        </>
      }
      {query && <p>Searching for '{query}'</p>}
      <RecipeList title="Recipes" type="Query" query={searchParams} />
    </>);
}