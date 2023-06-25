import React, { useEffect, useState } from "react";
import { Button, Card, Row, Col } from "react-bootstrap";
import { Rating } from "@smastrom/react-rating";
import { getReviews, Review } from "src/shared/CookTime";
import { useAuthentication } from "../Authentication/AuthenticationContext";

type RecipeReviewsProps = {
  recipeId: string
}

export function RecipeReviews({recipeId} : RecipeReviewsProps) {
  const [reviews, setReviews] = useState<Review[]>([])
  const { user } = useAuthentication();
  useEffect(() => {
    async function loadReviews() {
      setReviews(await getReviews(recipeId));
    }
    loadReviews();
  }, [recipeId]);

  function deleteReview() {
    fetch(`/api/MultiPartRecipe/${recipeId}/review`, {
      method: "DELETE",
      headers: {
        'Content-Type': 'application/json'
      }
    }).then(response => {
      if (response.ok) {
        // TODO this cannot be a location.reload()
        // eslint-disable-next-line no-restricted-globals
        // location.reload();
        setReviews(reviews.filter(review => review.owner.id !== user?.id));
    }});
  }

  return (
      <div>
        {reviews?.map((r, idx) => {
          return (
            <Card key={idx} className="review-card">
              <Row>
                <Col>
                  <Card.Link className="">
                    <Rating
                      style={{maxWidth: 100}}
                      value={r.rating}
                      readOnly />
                    <span className="margin-left-10">{r.owner.userName}</span>
                  </Card.Link>
                </Col>
                <Col>
                  {r.owner.id === user?.id ?
                    <Button
                      className="float-end"
                      variant="danger"
                      onClick={_ => deleteReview()}>
                      Delete
                    </Button>
                    : null}
                </Col>
              </Row>
              <Card.Body>
                {r.text}
              </Card.Body>
            </Card>
          )
        })}
      </div>
  );
}