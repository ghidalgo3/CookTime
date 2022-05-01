import React from "react";
import { Button, Card, Form, FormControl, ListGroup } from "react-bootstrap";
import Rating from "react-rating";
import { getUserId } from "./AuthState";

type RecipeReviewsProps = {
    recipeId : string
}

export class RecipeReviews extends React.Component<RecipeReviewsProps, {reviews: Review[]}> {
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
                        <Card>
                            <Card.Subtitle>
                                By {r.owner.userName}
                            </Card.Subtitle>
                            <Card.Body>
                                {r.text}
                            </Card.Body>
                            <Card.Link>
                                <Rating
                                    initialRating={r.rating}
                                    emptySymbol="far fa-star fa-2x"
                                    fullSymbol="fas fa-star fa-2x"
                                    readonly />
                            </Card.Link>
                            {r.owner.id === getUserId() ?
                                <Button
                                    onClick={_ => this.deleteRecipe()}>
                                    Delete
                                </Button>
                            : null}
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
                location.reload();
            }
        });
    }
}