import React, { useEffect, useState } from 'react';
import { Accordion, Alert, Col, Row, Spinner } from 'react-bootstrap';
import { RecipeCard } from '../RecipeCard/RecipeCard';
import { getRecipeRecommendations, RecipeRecommendation, RecommendationScoreBreakdown } from 'src/shared/CookTime';
import { useAuthentication } from '../Authentication/AuthenticationContext';
import './RecipeInsights.css';

function formatScore(score: number) {
  return score.toFixed(2);
}

function ScoreBreakdown({ scoreBreakdown }: { scoreBreakdown: RecommendationScoreBreakdown }) {
  const rows: Array<[string, number]> = [
    ['Ingredient similarity', scoreBreakdown.ingredientSimilarity],
    ['Favorite', scoreBreakdown.favoritedByUser],
    ['Novelty', scoreBreakdown.novelty],
    ['Diet match', scoreBreakdown.dietMatch],
  ];

  return (
    <div>
      {rows.map(([label, value]) => (
        <div className="score-breakdown-row" key={label}>
          <span>{label}</span>
          <span>{formatScore(value)}</span>
        </div>
      ))}
    </div>
  );
}

export function RecommendedRecipes({ recipeId }: { recipeId: string }) {
  const { user } = useAuthentication();
  const [recommendations, setRecommendations] = useState<RecipeRecommendation[]>([]);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);
  const showBreakdown = user?.roles.includes('Administrator') ?? false;

  useEffect(() => {
    let cancelled = false;

    async function loadRecommendations() {
      setLoading(true);
      setFailed(false);
      try {
        const nextRecommendations = await getRecipeRecommendations(recipeId);
        if (!cancelled) {
          setRecommendations(nextRecommendations);
        }
      } catch {
        if (!cancelled) {
          setFailed(true);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadRecommendations();

    return () => {
      cancelled = true;
    };
  }, [recipeId]);

  return (
    <section className="recipe-insight-section">
      <h2>Recommended Recipes</h2>
      {loading && (
        <div>
          <Spinner size="sm" /> Loading recommendations...
        </div>
      )}
      {!loading && failed && (
        <Alert variant="warning">Recommendations are not available right now.</Alert>
      )}
      {!loading && !failed && recommendations.length === 0 && (
        <p className="text-muted">No recommendations yet.</p>
      )}
      {!loading && !failed && recommendations.length > 0 && (
        <Row>
          {recommendations.map((recommendation) => (
            <Col xs={12} md={6} lg={4} key={recommendation.recipe.id}>
              <RecipeCard {...recommendation.recipe} />
              {showBreakdown && (
                <Accordion className="recommendation-details">
                  <Accordion.Item eventKey="0">
                    <Accordion.Header>
                      Score {formatScore(recommendation.score)}
                    </Accordion.Header>
                    <Accordion.Body>
                      {recommendation.reasons.length > 0 && (
                        <div className="recommendation-reasons">
                          {recommendation.reasons.map((reason) => (
                            <span className="recommendation-reason" key={reason}>
                              {reason}
                            </span>
                          ))}
                        </div>
                      )}
                      <ScoreBreakdown scoreBreakdown={recommendation.scoreBreakdown} />
                    </Accordion.Body>
                  </Accordion.Item>
                </Accordion>
              )}
            </Col>
          ))}
        </Row>
      )}
    </section>
  );
}
