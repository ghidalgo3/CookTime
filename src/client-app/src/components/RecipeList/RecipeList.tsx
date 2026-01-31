import React, { useCallback, useEffect, useRef, useState } from "react"
import { Col, Row } from "react-bootstrap";
import { RecipeCard } from "../RecipeCard/RecipeCard";
import { getFavoriteRecipeViews, getFeaturedRecipeViews, getMyRecipes, getNewRecipeViews, getRecipeViews, PagedResult, RecipeView, toPagedResult } from "src/shared/CookTime"
import { Link } from "react-router";
import PaginatedList from "../PaginatedList/PaginatedList";
import InfiniteScrollList from "../InfiniteScrollList/InfiniteScrollList";
import { useAuthentication } from "../Authentication/AuthenticationContext";
import "./RecipeList.css";

interface RecipeListProps {
  title: string
  type: "Featured" | "New" | "Query" | "Favorites" | "Mine"
  query?: URLSearchParams
  hideIfEmpty?: boolean
}

export default function RecipeList({ title, query, type, hideIfEmpty }: RecipeListProps) {
  const [recipes, setRecipes] = useState<PagedResult<RecipeView>>(toPagedResult([]));
  // Infinite scroll state for Query type
  const [allRecipes, setAllRecipes] = useState<RecipeView[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const searchQueryRef = useRef<string>("");

  // Load initial data or handle non-Query types
  useEffect(() => {
    async function loadData() {
      if (type === "Featured") {
        const featured = await getFeaturedRecipeViews();
        setRecipes(toPagedResult(featured));
      } else if (type === "New") {
        const news = await getNewRecipeViews();
        setRecipes(toPagedResult(news));
      } else if (type === "Query") {
        const search = query?.get("search") ?? "";
        // Reset state when search query changes
        if (search !== searchQueryRef.current) {
          searchQueryRef.current = search;
          setAllRecipes([]);
          setCurrentPage(1);
          setHasMore(true);
        }
        // Load first page
        setIsLoading(true);
        const result = await getRecipeViews({ search, page: 1 });
        setAllRecipes(result.results);
        setCurrentPage(1);
        setHasMore(result.currentPage < result.pageCount);
        setIsLoading(false);
      } else if (type === "Favorites") {
        const result = await getFavoriteRecipeViews();
        setRecipes(toPagedResult(result));
      } else if (type === "Mine") {
        const page = query?.get("page") ?? "1";
        const result = await getMyRecipes({ page: Number.parseInt(page) });
        setRecipes(result);
      }
    }
    loadData();
  }, [query, type]);

  // Load more function for infinite scroll
  const loadMore = useCallback(async () => {
    if (type !== "Query" || isLoading || !hasMore) return;

    setIsLoading(true);
    const search = query?.get("search") ?? "";
    const nextPage = currentPage + 1;

    const result = await getRecipeViews({ search, page: nextPage });
    setAllRecipes(prev => [...prev, ...result.results]);
    setCurrentPage(nextPage);
    setHasMore(result.currentPage < result.pageCount);
    setIsLoading(false);
  }, [type, isLoading, hasMore, query, currentPage]);
  const { user } = useAuthentication();

  // For Query type with infinite scroll, check allRecipes
  if (hideIfEmpty && type === "Query" && allRecipes.length === 0 && !isLoading) {
    return null;
  }
  // For other types, check recipes
  if (hideIfEmpty && type !== "Query" && (recipes === null || recipes.results.length === 0)) {
    return null;
  }

  return (
    <>
      <Row className="align-items-center mb-3">
        <Col>
          <h1>{title}</h1>
        </Col>
        {
          user && type === "Featured" &&
          <Col xs="auto" className="ms-auto">
            <Link to="/Recipes/Create" className="create-recipe-link">
              Create Recipe
            </Link>
          </Col>
        }
      </Row>
      {type === "Query" ? (
        <InfiniteScrollList
          items={allRecipes}
          element={(recipe: RecipeView) => <RecipeCard {...recipe} />}
          hasMore={hasMore}
          isLoading={isLoading}
          onLoadMore={loadMore}
        />
      ) : (
        <PaginatedList
          element={(recipe: RecipeView) => <RecipeCard {...recipe} />}
          inFeedAdIndex={4}
          items={recipes}
        />
      )}
    </>);
}