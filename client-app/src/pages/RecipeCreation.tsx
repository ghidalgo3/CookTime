import React, {useEffect, useState} from "react"
import { ActionFunctionArgs, Form as RouterForm, redirect } from "react-router-dom";
import { Button, Col, Container, Form, Row } from "react-bootstrap";
import { createRecipe, MultiPartRecipe } from "src/shared/CookTime";
import { Path } from "./Recipe";
import { useTitle } from "src/shared/useTitle";

export async function action(args : ActionFunctionArgs) {
  const formData = await args.request.formData();
  const name = formData.get("name")?.toString();
  if (name) {
    const result = await createRecipe(name);
    if (result.ok) {
      var recipe = await result.json() as MultiPartRecipe;
      return redirect(Path(recipe.id))
    } else {
      return { errors: "Something went wrong" }
    }
  } else {
    return {errors: "Name is required"}
  }
}

export default function RecipeCreation() {
  useTitle("New Recipe")
  return (
    <>
    <Container>
      <Row className="justify-content-md-center" >
        <Col style={{maxWidth: "540px"}}>
            <h1>Create recipe</h1>
            <RouterForm method="post">
              <Form.Group className="margin-bottom-8">
                <Form.Label>Recipe name</Form.Label>
                <Form.Control type="text" name="name"></Form.Control>
              </Form.Group>
              <Form.Group>
                <Button className="width-100" type="submit">Create</Button>
              </Form.Group>
              {/* <Form.Control></Form.Control> */}
            </RouterForm>
        </Col>
      </Row>
    </Container>
    </>
  );
}