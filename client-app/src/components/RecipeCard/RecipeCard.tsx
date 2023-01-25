import React, {useEffect, useState} from "react"
import { Card } from "react-bootstrap";
import Rating from "react-rating";
import { Link } from "react-router-dom";

export interface RecipeCardProps {
  recipeId : string,
  recipeName : string,
  averageReviews: number,
  reviewCount: number,
  isFavorite: boolean,
  categories: string[]
}

export function RecipeCard({categories, recipeName, averageReviews, reviewCount }: RecipeCardProps) {
  function ReviewDisplay() {
    return (
      <div className="margin-top-8">
        <Rating
          initialRating={averageReviews}
          readonly
          emptySymbol="far fa-star"
          fullSymbol="fas fa-star"
          fractions={2} />
        {"   "} ({reviewCount})
      </div>
    )
  }

  return (<>
  <Card className="recipe-card">
    <div data-image-id="@imageId" id="@imageTarget"></div>
    <Card.Body>
        <p
          className="tag-style do-not-overflow-text margin-bottom-0">{categories.join(", ")}</p>
        <Link to="/recipes/details">{recipeName}</Link>
    </Card.Body>
  </Card>
  </>);
}