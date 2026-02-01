import React, { useCallback, useEffect, useState } from "react";
import { Button, Col, Modal, Row, Spinner } from "react-bootstrap";
import { Link, useParams } from "react-router";
import { Helmet } from "react-helmet-async";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import {
  RecipeListWithRecipes,
  getList,
  removeFromList,
} from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";
import { RecipeCard } from "src/components/RecipeCard/RecipeCard";

const PAGE_SIZE = 12;

export default function ListView() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuthentication();

  const [list, setList] = useState<RecipeListWithRecipes | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Infinite scroll state
  const [displayedCount, setDisplayedCount] = useState(PAGE_SIZE);
  
  // Clear list confirmation modal state
  const [showClearConfirm, setShowClearConfirm] = useState(false);

  useTitle(list?.name ?? "List");

  // Determine if user is the owner of the list
  const isOwner = user && list && list.ownerId === user.id;

  const fetchList = useCallback(async () => {
    if (!slug) return;

    try {
      const fetchedList = await getList(slug);
      setList(fetchedList);
      setError(null);
    } catch (err) {
      setError("Failed to load list");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [slug]);

  useEffect(() => {
    fetchList();
  }, [fetchList]);

  const handleDeleteRecipe = useCallback(async (recipeId: string) => {
    if (!list) return;

    await removeFromList(list.name, recipeId);

    setList(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        recipes: prev.recipes.filter(item => item.recipe.id !== recipeId)
      };
    });
  }, [list]);

  const handleClearList = useCallback(async () => {
    if (!list) return;

    await Promise.all(list.recipes.map(item => removeFromList(list.name, item.recipe.id)));
    setList(prev => prev ? { ...prev, recipes: [] } : prev);
    setShowClearConfirm(false);
  }, [list]);

  // Infinite scroll: load more items
  const loadMore = useCallback(() => {
    setDisplayedCount(prev => prev + PAGE_SIZE);
  }, []);

  // Check if there are more items to load
  const hasMore = list ? displayedCount < list.recipes.length : false;
  const displayedRecipes = list?.recipes.slice(0, displayedCount) ?? [];

  // Intersection observer for infinite scroll
  useEffect(() => {
    const sentinel = document.getElementById('list-scroll-sentinel');
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (entry.isIntersecting && hasMore) {
          loadMore();
        }
      },
      { root: null, rootMargin: "200px", threshold: 0 }
    );

    observer.observe(sentinel);

    return () => observer.disconnect();
  }, [hasMore, loadMore]);

  if (loading) {
    return (
      <div className="text-center padding-top-20">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (error || !list) {
    return (
      <div>
        <h1 className="margin-bottom-20">List Not Found</h1>
        <p className="text-muted">
          This list doesn't exist or you don't have permission to view it.
        </p>
        {user && (
          <Link to="/lists">
            <Button variant="primary">Back to Lists</Button>
          </Link>
        )}
      </div>
    );
  }

  // Read-only view for public lists when user is not the owner
  if (!isOwner) {
    return (
      <>
        <Helmet>
          <link rel="canonical" href={`${origin}/lists/${list.slug}`} />
        </Helmet>
        <Row className="align-items-center mb-3">
          <Col>
            <h1>{list.name}</h1>
            {list.description && (
              <p className="text-muted">{list.description}</p>
            )}
          </Col>
        </Row>

        <Row>
          {displayedRecipes.map((item) => (
            <Col sm="4" key={item.recipe.id}>
              <RecipeCard {...item.recipe} />
            </Col>
          ))}
        </Row>

        {list.recipes.length === 0 && (
          <p className="text-muted">No recipes in this list.</p>
        )}

        <div id="list-scroll-sentinel">
          {hasMore && (
            <div className="text-center py-4">
              <Spinner animation="border" role="status">
                <span className="visually-hidden">Loading...</span>
              </Spinner>
            </div>
          )}
        </div>

        {!hasMore && list.recipes.length > 0 && (
          <div className="text-center py-4">
            <p className="text-muted">You've reached the end!</p>
          </div>
        )}
      </>
    );
  }

  return (
    <>
      <Helmet>
        <link rel="canonical" href={`${origin}/lists/${list.slug}`} />
      </Helmet>
      <Row className="align-items-center mb-3">
        <Col>
          <h1>{list.name}</h1>
          {list.description && (
            <p className="text-muted">{list.description}</p>
          )}
        </Col>
        <Col xs="auto">
          <Button variant="danger" className="me-2" onClick={() => setShowClearConfirm(true)}>
            Clear List
          </Button>
          <Link to="/lists">
            <Button variant="outline-secondary">
              Manage Lists
            </Button>
          </Link>
        </Col>
      </Row>

      <Row>
        {displayedRecipes.map((item) => (
          <Col sm="4" key={item.recipe.id}>
            <RecipeCard {...item.recipe} onRemove={handleDeleteRecipe} />
          </Col>
        ))}
      </Row>

      {list.recipes.length === 0 && (
        <p className="text-muted">No recipes in this list. Add recipes from the recipe details page.</p>
      )}

      <div id="list-scroll-sentinel">
        {hasMore && (
          <div className="text-center py-4">
            <Spinner animation="border" role="status">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
          </div>
        )}
      </div>

      {!hasMore && list.recipes.length > 0 && (
        <div className="text-center py-4">
          <p className="text-muted">You've reached the end!</p>
        </div>
      )}

      <Modal show={showClearConfirm} onHide={() => setShowClearConfirm(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Clear List</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to remove all {list.recipes.length} recipes from "{list.name}"? This action cannot be undone!
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowClearConfirm(false)}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleClearList}>
            Clear List
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}
