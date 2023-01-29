import React, {useEffect, useState} from "react"
import { Button, Card, Spinner, Stack } from "react-bootstrap";
import { Rating } from "@smastrom/react-rating";
import { Link, ActionFunctionArgs, useFetcher} from "react-router-dom";
import { Fa6RegularStar, Fa6SolidStar } from "../SVG";
import "./RecipeCard.css"
import { addToFavorites, Image, RecipeView, removeFromFavorites } from "src/shared/CookTime"
import imgs from "src/assets";
import { useAuthentication } from "../Authentication/AuthenticationContext";

export function RecipeCard({
  categories,
  isFavorite,
  id,
  name,
  averageReviews,
  reviewCount,
  images}: RecipeView) {

  const [favorite, setFavorite] = useState(isFavorite);
  const { user } = useAuthentication();

  function FavoriteToggle() {
    const [submitting, setSubmitting] = useState<boolean>(false);
    var heartClass = "";
    if (favorite) {
      heartClass = "fas fa-heart fa-2x"
    } else {
      heartClass = "far fa-heart fa-2x"
    }
    const toggleFavoriteState = async () => {
      setSubmitting(true);
      if (favorite) {
        await removeFromFavorites(id);
        setFavorite(false);
      } else {
        await addToFavorites(id);
        setFavorite(true);
      }
      setSubmitting(false);
    }
    return (
      <>
        <Button
          disabled={submitting}
          onClick={toggleFavoriteState}
          type="submit"
          className="favorite-button" >
          {submitting ?
            <Spinner
              as="span"
              animation="border"

              role="status"
              aria-hidden="true"
            />
            : <i className={heartClass}></i>}
        </Button>
      </>
    )
  }
  
  function CardImage() {
    let image = (images.length === 0 || images[0].id === "null") ?
      imgs.placeholder :
      `/image/${images[0].id}?width=300`;
    return (
      <div className="cr-image-parent">
        <Link to={`/Recipes/Details?id=${id}`}>
          <img
            className="card-img-top card-recipe-image"
            src={image}
            alt="Food">
          </img>
        </Link>
        {user && <FavoriteToggle />}
      </div>
    )
  }

  function ReviewDisplay() {
    return (
      <div className="margin-top-8">
        {reviewCount > 0 ?
          <Stack direction="horizontal">
            <Rating
              value={averageReviews}
              className={"card-ratings"}
              readOnly
            /> {"   "} ({reviewCount})
          </Stack>
          :
          <div className="card-ratings"></div>
        }
      </div>
    )
  }

  return (<>
    <Card className="recipe-card margin-bottom-20">
      <CardImage />
      <Card.Body>
        <p
          className="tag-style do-not-overflow-text margin-bottom-0">
          {categories.length ? categories.join(", ") : "Recipe"}
        </p>
        <Link to={`/recipes/details?id=${id}`}>{name}</Link>
        <ReviewDisplay />
      </Card.Body>
    </Card>
  </>);
}