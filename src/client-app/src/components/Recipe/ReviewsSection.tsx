import React from 'react';
import { Col, Row } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';
import { AuthContext } from '../Authentication/AuthenticationContext';
import { RecipeReviewForm } from './RecipeReviewForm';
import { RecipeReviews } from './RecipeReviews';

interface ReviewsSectionProps {
  recipeId: string;
}

export function ReviewsSection({ recipeId }: ReviewsSectionProps) {
  const { recipe, edit } = useRecipeContext();

  if (edit) return null;

  return (
    <>
      <AuthContext.Consumer>
        {({ user }) =>
          user && (
            <div className="border-top-1 margin-top-30">
              <Row>
                <Col>
                  <RecipeReviewForm recipe={recipe} />
                </Col>
              </Row>
            </div>
          )
        }
      </AuthContext.Consumer>
      <div className="margin-top-10">
        <Row>
          <Col>
            <RecipeReviews recipeId={recipeId} />
          </Col>
        </Row>
      </div>
    </>
  );
}
