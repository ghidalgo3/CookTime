import React, {useEffect, useState} from "react"
import { Col, Row } from "react-bootstrap";
import { RecipeCard } from "../RecipeCard/RecipeCard";
import { getFavoriteRecipeViews, getFeaturedRecipeViews, getMyRecipes, getNewRecipeViews, getRecipeViews, Image, PagedResult, RecipeView, toPagedResult } from "src/shared/CookTime"
import { Link, LoaderFunctionArgs, useLoaderData, useLocation, useSearchParams } from "react-router-dom";
import PaginatedList from "../PaginatedList/PaginatedList";
import { useAuthentication } from "../Authentication/AuthenticationContext";

interface RecipeListProps {
  title: string
  type: "Featured" | "New" | "Query" | "Mine" | "Favorites"
  query?: URLSearchParams
  hideIfEmpty?: boolean
}

export default function RecipeList({title, query, type, hideIfEmpty} : RecipeListProps) {
  const [recipes, setRecipes] = useState<PagedResult<RecipeView>>(toPagedResult([]));
  useEffect(() => {
    async function loadData() {
      if (type === "Featured") {
        const featured = await getFeaturedRecipeViews();
        setRecipes(toPagedResult(featured));
      } else if (type === "New") {
        const news = await getNewRecipeViews();
        setRecipes(toPagedResult(news));
      } else if (type === "Query") {
        const page = query?.get("page") ?? "";
        const search = query?.get("search") ?? "";
        const result = await getRecipeViews({search, page: Number.parseInt(page)})
        setRecipes(result);
      } else if (type === "Favorites") {
        const result = await getFavoriteRecipeViews();
        setRecipes(toPagedResult(result));
      } else if (type === "Mine") {
        const result = await getMyRecipes();
        setRecipes(toPagedResult(result));
      }
    }
    loadData();
  }, [query, type])
  const { user } = useAuthentication();

  if (hideIfEmpty && (recipes === null || recipes.results.length === 0)) {
    return null;
  }

  return (
    <>
    <Row>
      <Col xs={10}>
        <h1>{title}</h1>
      </Col>
      {
        user && type === "Query" &&
        <Col className="margin-bottom-20 text-end" xs={2}>
          <Link to="/Recipes/Create">
            <i className="fas fa-plus-circle themePrimary-color fa-2x"></i>
          </Link>
        </Col>
      }
    </Row>
    <PaginatedList
      element={(recipe : RecipeView) => <RecipeCard {...recipe}/>}
      items={recipes}
      />
      {/* {query && <p>Searching for '{query}'</p>}
      <Row>
        <div className="col-10">
          <h1 className="margin-bottom-20">Recipes</h1>
        </div>
        {recipes?.results.map((recipe, idx) =>
          <Col key={idx} sm={4}>
            <RecipeCard
              key={idx}
              {...recipe}
            />
          </Col>
        )}
      </Row> */}
    </>);
}