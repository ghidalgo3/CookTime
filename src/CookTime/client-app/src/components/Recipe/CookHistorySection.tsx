import React, { useEffect, useMemo, useState } from 'react';
import { Button, Spinner } from 'react-bootstrap';
import { useAuthentication } from '../Authentication/AuthenticationContext';
import { useRecipeContext } from './RecipeContext';
import {
  CookHistoryEvent,
  getCookHistory,
  logCookHistory,
} from 'src/shared/CookTime';
import './RecipeInsights.css';

function formatDate(date: string) {
  return new Date(`${date}T00:00:00`).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

export function CookHistorySection({ recipeId }: { recipeId: string }) {
  const { user } = useAuthentication();
  const { setToastMessage, setErrorMessage } = useRecipeContext();
  const [history, setHistory] = useState<CookHistoryEvent[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadHistory() {
      if (!user) {
        if (!cancelled) {
          setHistory([]);
        }
        return;
      }

      try {
        setLoading(true);
        setLoadError(false);
        const events = await getCookHistory(recipeId);
        if (!cancelled) {
          setHistory(events);
        }
      } catch {
        if (!cancelled) {
          setLoadError(true);
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
  }, [recipeId, user]);

  const mostRecent = useMemo(() => history[0], [history]);

  if (!user) return null;

  const handleCookedToday = async () => {
    setSubmitting(true);
    try {
      const event = await logCookHistory(recipeId);
      if (event) {
        const nextHistory = [event, ...history].sort((a, b) => b.cookedAt.localeCompare(a.cookedAt));
        setHistory(nextHistory);
        setToastMessage('Logged that you cooked this recipe.');
      } else {
        setErrorMessage('Could not log cook history.');
      }
    } catch {
      setErrorMessage('Could not log cook history.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="recipe-insight-section">
      <div className="cook-history-summary">
        <div>
          <h2 className="h4 margin-bottom-0">Cooked</h2>
          {loading ? (
            <span className="text-muted">Loading history...</span>
          ) : loadError ? (
            <span className="text-muted">Could not load cook history.</span>
          ) : history.length === 0 ? (
            <span className="text-muted">No cook history yet.</span>
          ) : (
            <span className="text-muted">
              {history.length} {history.length === 1 ? 'time' : 'times'}
              {mostRecent ? `, last on ${formatDate(mostRecent.cookedAt)}` : ''}
            </span>
          )}
        </div>
        <Button onClick={handleCookedToday} disabled={submitting}>
          {submitting ? <Spinner size="sm" /> : 'I cooked this'}
        </Button>
      </div>
    </section>
  );
}
