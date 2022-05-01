import React from "react";
import { Button, Form, FormControl } from "react-bootstrap";
import Rating from "react-rating";

type RecipeReviewFormProps = {
    recipe: MultiPartRecipe
}

type RecipeReviewFormState = {
    rating : number,
    text : string,
    validated: boolean
}

export class RecipeReviewForm extends React.Component<RecipeReviewFormProps, RecipeReviewFormState> {
    constructor(props: RecipeReviewFormProps) {
        super(props);
        this.state = {
            rating: 0,
            text: "",
            validated: false
        }
    }

    render() {
        return (
            <Form noValidate validated={this.state.validated} onSubmit={event => this.onSubmit(event)}>
                <Form.Group>
                    <Rating
                        {...this.props}
                        emptySymbol="far fa-star fa-2x"
                        fullSymbol="fas fa-star fa-2x"
                        initialRating={this.state.rating}
                        onClick={value => this.setState({ rating: value })}
                    />
                    <Form.Control.Feedback type="invalid">
                        Please provide a rating.
                    </Form.Control.Feedback>
                </Form.Group>

                <Form.Group>
                    <Form.Control
                        required
                        as="textarea"
                        rows={4}
                        className="margin-bottom-8"
                        type="text"
                        placeholder="Rate and write a review for this recipe"
                        value={this.state.text}
                        onChange={e => {
                            this.setState({ text: e.target.value })
                        }}>
                    </Form.Control>
                    <Form.Control.Feedback type="invalid">
                        Please provide a review.
                    </Form.Control.Feedback>
                </Form.Group>
                <Button
                    variant="outline-primary"
                    type="submit">
                        Submit
                </Button>
            </Form>
        )
    }

    private onSubmit(event) {
        const form = event.currentTarget;
        event.preventDefault();
        event.stopPropagation();
        if (form.checkValidity() === false || this.state.rating === 0) {
            return;
        }

        fetch(`/api/MultiPartRecipe/${this.props.recipe.id}/review`, {
            method: "PUT",
            body: JSON.stringify(this.state),
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