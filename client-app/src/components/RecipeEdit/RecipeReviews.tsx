import React from "react";
import { Button, Card, Row, Col, Form, FormControl, ListGroup } from "react-bootstrap";
import Rating from "react-rating";
import { Review } from "src/shared/CookTime";

type RecipeReviewsProps = {
  recipeId: string
}

export class RecipeReviews extends React.Component<RecipeReviewsProps, { reviews: Review[] }> {
  constructor(props: RecipeReviewsProps) {
    super(props);
    this.state = {
      reviews: []
    }
  }

  componentDidMount() {
    fetch(`/api/MultiPartRecipe/${this.props.recipeId}/reviews`)
      .then(response => response.json())
      .then(
        result => {
          let r = result as Review[]
          this.setState({
            reviews: r,
          })
        }
      )
  }

  render() {
    return (
      <div>
        {this.state.reviews?.map(r => {
          return (
            <Card className="review-card">
              <Row>
                <Col>
                  <Card.Link className="">
                    {/* <Rating
                      initialRating={r.rating}
                      emptySymbol="far fa-star"
                      fullSymbol="fas fa-star"
                      readonly /> */}
                    <span className="margin-left-10">{r.owner.userName}</span>
                  </Card.Link>
                </Col>
                <Col>
                  {/* {r.owner.id === getUserId() ?
                    <Button
                      className="float-end"
                      variant="danger"
                      onClick={_ => this.deleteRecipe()}>
                      Delete
                    </Button>
                    : null} */}
                </Col>
              </Row>
              <Card.Body>
                {r.text}
              </Card.Body>
            </Card>
          )
        })}
      </div>
    )
  }

  deleteRecipe() {
    fetch(`/api/MultiPartRecipe/${this.props.recipeId}/review`, {
      method: "DELETE",
      headers: {
        'Content-Type': 'application/json'
      }
    }).then(response => {
      if (response.ok) {
        // eslint-disable-next-line no-restricted-globals
        location.reload();
      }
    });
  }
}