import React, { useEffect, useState } from "react";
import { Alert, Button, Spinner } from "react-bootstrap";
import { Link } from "react-router";
import { useAuthentication } from "src/components/Authentication/AuthenticationContext";
import {
  CookHistoryEventWithRecipe,
  deleteCookHistory,
  getUserCookHistory,
} from "src/shared/CookTime";
import { useTitle } from "src/shared/useTitle";
import "src/components/Recipe/RecipeInsights.css";

function formatDate(date: string) {
  return new Date(`${date}T00:00:00`).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export default function History() {
  const { user } = useAuthentication();
  const [history, setHistory] = useState<CookHistoryEventWithRecipe[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useTitle("History");

  useEffect(() => {
    let cancelled = false;

    async function loadHistory() {
      if (!user) {
        if (!cancelled) {
          setHistory([]);
          setLoading(false);
        }
        return;
      }

      try {
        setLoading(true);
        setError(null);
        const events = await getUserCookHistory();
        if (!cancelled) {
          setHistory(events);
        }
      } catch {
        if (!cancelled) {
          setError("Could not load cook history.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadHistory();

    return () => {
      cancelled = true;
    };
  }, [user]);

  const handleDelete = async (eventId: string) => {
    setDeletingId(eventId);
    setError(null);

    const deleted = await deleteCookHistory(eventId);
    if (deleted) {
      setHistory((prev) => prev.filter((event) => event.id !== eventId));
    } else {
      setError("Could not delete cook event.");
    }

    setDeletingId(null);
  };

  if (!user) {
    return (
      <Alert variant="warning">
        Sign in to view your recipe history.
      </Alert>
    );
  }

  return (
    <>
      <h1>History</h1>
      {error && (
        <Alert variant="danger" dismissible onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {loading ? (
        <div>
          <Spinner size="sm" /> Loading history...
        </div>
      ) : history.length === 0 ? (
        <p className="text-muted">No cook history yet.</p>
      ) : (
        <div className="cook-history-page-list">
          {history.map((event) => (
            <div className="cook-history-page-row" key={event.id}>
              <div>
                <Link to={`/recipes/details?id=${event.recipe.id}`}>
                  {event.recipe.name}
                </Link>
                <div className="text-muted">{formatDate(event.cookedAt)}</div>
              </div>
              <Button
                variant="outline-danger"
                disabled={deletingId === event.id}
                onClick={() => handleDelete(event.id)}
              >
                {deletingId === event.id ? <Spinner size="sm" /> : "Delete"}
              </Button>
            </div>
          ))}
        </div>
      )}
    </>
  );
}
