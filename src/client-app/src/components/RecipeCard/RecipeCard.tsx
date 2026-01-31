import React, { useState } from "react"
import { Button, Card, Spinner, Stack } from "react-bootstrap";
import { Rating } from "@smastrom/react-rating";
import { Link } from "react-router";
import { Fa6RegularStar, Fa6SolidStar } from "../SVG";
import "./RecipeCard.css"
import { Image, RecipeView } from "src/shared/CookTime"
import imgs from "src/assets";
import { useAuthentication } from "../Authentication/AuthenticationContext";
import { useFavorites } from "../Favorites/FavoritesContext";

export function RecipeCard({
  categories,
  id,
  name,
  averageReviews,
  reviewCount,
  images }: RecipeView) {

  const { user } = useAuthentication();
  const { isFavorite, toggleFavorite } = useFavorites();
  const favorite = isFavorite(id);

  function FavoriteToggle() {
    const [submitting, setSubmitting] = useState<boolean>(false);
    var heartClass = "";
    if (favorite) {
      heartClass = "bi bi-heart-fill fs-4"
    } else {
      heartClass = "bi bi-heart fs-4 heart-outline"
    }
    const handleToggle = async () => {
      setSubmitting(true);
      await toggleFavorite(id);
      setSubmitting(false);
    }
    return (
      <>
        <Button
          disabled={submitting}
          onClick={handleToggle}
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
    let image = (images.length === 0 || !images[0].url) ?
      imgs.placeholder :
      images[0].url;
    return (
      <div className="cr-image-parent">
        <Link to={`/recipes/details?id=${id}`}>
          <img
            loading="lazy"
            className="card-img-top card-recipe-image"
            src={image}
            alt="Food">
          </img>
        </Link>
        {images.length > 1 && (
          <div className="image-count-dots">
            {images.slice(0, 5).map((_, index) => (
              <span key={index} className={`dot ${index === 0 ? 'active' : ''}`}></span>
            ))}
            {images.length > 5 && <span className="dot-more">+{images.length - 5}</span>}
          </div>
        )}
        {user && <FavoriteToggle />}
      </div>
    )
  }

  function ReviewDisplay() {
    return (
      <div className="height-32 margin-top-8">
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
        <Card.Title className="do-not-overflow-text">
          <Link to={`/recipes/details?id=${id}`}>{name}</Link>
        </Card.Title>
        <ReviewDisplay />
      </Card.Body>
    </Card>
  </>);
}