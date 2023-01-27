import React, {useEffect, useState} from "react"
import { Col, Row } from "react-bootstrap";
import ReactPaginate from 'react-paginate';
import { RecipeCard } from "../RecipeCard/RecipeCard";
import { getRecipeViews, Image, RecipeView } from "src/shared/CookTime"
import { LoaderFunctionArgs, useLoaderData, useLocation, useSearchParams } from "react-router-dom";

export async function loader(load : LoaderFunctionArgs) {
  const {request, params} = load;
  const url = new URL(request.url);
  const search = url.searchParams.get("search") ?? "";
  const page = Number.parseInt(url.searchParams.get("page") ?? "1");
  return await getRecipeViews({search, page});
}

export default function RecipeList() {
  const recipes = useLoaderData() as RecipeView[];
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get("search");
  return (
    <>
      {query && <p>Searching for '{query}'</p>}
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
    </>);
}