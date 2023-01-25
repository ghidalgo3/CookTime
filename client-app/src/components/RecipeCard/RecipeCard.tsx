import React, {useEffect, useState} from "react"
import { Button, Card, Spinner, Stack } from "react-bootstrap";
import { Rating } from "@smastrom/react-rating";
import { Link } from "react-router-dom";
import { Fa6RegularStar, Fa6SolidStar } from "../SVG";
import "./RecipeCard.css"
import { Image } from "src/shared/CookTime"

export interface RecipeCardProps {
  recipeId : string,
  recipeName : string,
  averageReviews: number,
  images: Image[],
  reviewCount: number,
  isFavorite: boolean | undefined,
  operationInProgress?: boolean | undefined,
  categories: string[]
}

export function RecipeCard({
  operationInProgress, 
  categories,
  isFavorite,
  recipeId,
  recipeName,
  averageReviews,
  reviewCount,
  images}: RecipeCardProps) {
  const [favorite, setFavorite] = useState(isFavorite);
  function button() {
        var heartClass = "";
        if (favorite) {
            heartClass = "fas fa-heart fa-2x"
        } else {
            heartClass = "far fa-heart fa-2x"
        }
        return (
        <Button className="favorite-button" >
                {operationInProgress ?
                    <Spinner
                        as="span"
                        animation="border"
                        
                        role="status"
                        aria-hidden="true"
                    />
                    : <i className={heartClass}></i>}
            </Button>
        )
    }
  function CardImage() {
    let image = (images.length == 0 || images[0].id == null || images[0].id == "null") ?
      "/placeholder.jpg" :
      `/image/${images[0].id}?width=300`;
    return (
      <div className="cr-image-parent">
        <a href={`/Recipes/Details?id=${recipeId}`}>
          <img
            className="card-img-top card-recipe-image"
            src={image}
            alt="Food image">
          </img>
        </a>
        {
          isFavorite ?
            button() :
            null
        }
      </div>
    )
  }

  function ReviewDisplay() {
    return (
      <div className="margin-top-8">
      <Stack direction="horizontal">
        <Rating
          value={averageReviews}
          className={"card-ratings"}
          readOnly
          /> {"   "} ({reviewCount})
      </Stack>
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
        {reviewCount > 0  && <ReviewDisplay />}
    </Card.Body>
  </Card>
  </>);
}