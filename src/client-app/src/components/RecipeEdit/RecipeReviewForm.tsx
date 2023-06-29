import React, { useState } from "react";
import { Button, Form, FormControl } from "react-bootstrap";
import { Rating } from "@smastrom/react-rating";
import { MultiPartRecipe } from "src/shared/CookTime";

type RecipeReviewFormProps = {
  recipe: MultiPartRecipe
}

type RecipeReviewFormState = {
  rating: number,
  text: string,
  validated: boolean
}

export function RecipeReviewForm({recipe} : RecipeReviewFormProps) {
  const [rating, setRating] = useState(0);
  const [text, setText] = useState("");
  const [validated, setValidated] = useState(false);

  function onSubmit(event : any) {
    const form = event.currentTarget;
    event.preventDefault();
    event.stopPropagation();
    if (form.checkValidity() === false || rating === 0) {
      setValidated(true);
      return;
    }

    fetch(`/api/MultiPartRecipe/${recipe.id}/review`, {
      method: "PUT",
      body: JSON.stringify({rating, text, validated}),
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

  return (
    <Form noValidate validated={validated} onSubmit={event => onSubmit(event)} className="margin-top-20">
      <Form.Group>
        <Rating
          style={{maxWidth: 200}}
          value={rating}
          onChange={setRating}
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
          className="margin-bottom-8 margin-top-8"
          type="text"
          placeholder="Rate and write a review for this recipe"
          value={text}
          onChange={e => {
            setText(e.target.value)
          }}>
        </Form.Control>
        <Form.Control.Feedback type="invalid">
          Please provide both a review and a rating.
        </Form.Control.Feedback>
      </Form.Group>
      <Button
        className="margin-bottom-20"
        variant="outline-primary"
        type="submit">
        Submit
      </Button>
    </Form>
  )
}