import React, {useEffect, useState} from "react"
import { ActionFunctionArgs, Form as RouterForm, redirect, useRouteError } from "react-router-dom";
import { Button, Col, Container, Form, Row, Spinner } from "react-bootstrap";
import { createRecipeWithName, importRecipeFromImage, MultiPartRecipe } from "src/shared/CookTime";
import { Path } from "./Recipe";
import { useTitle } from "src/shared/useTitle";

export async function action(args : ActionFunctionArgs) {
  const formData = await args.request.formData();
  const formName = formData.get("intent")?.toString();
  const name = formData.get("name")?.toString();
  if (formName === SIMPLE_CREATE && name) {
    const body = formData.get("body")?.toString();
    const result = await createRecipeWithName({name, body});
    if (result.ok) {
      var recipe = await result.json() as MultiPartRecipe;
      return redirect(Path(recipe.id))
    } else {
      return { errors: "Something went wrong creating a recipe" }
    }
  } else if (formName === IMPORT) {
    const result = await importRecipeFromImage(formData)
    if (result.ok) {
      const recipe = await result.json() as MultiPartRecipe;
      return redirect(Path(recipe.id))
    } else {
      return { errors: "Something went wrong importing from an image" }
    }
  }
  else {
    return {errors: "Name is required"}
  }
}

const SIMPLE_CREATE = "Simple create";
const IMPORT = "Import from image";
export const RECIPE_CREATE_PAGE_PATH = "Recipes/Create"
export default function RecipeCreation() {

  useTitle("New Recipe")

  const [image, setImage] = useState<Blob>();
  const [imageSrc, setImageSrc] = useState<string>();
  const [importInProgress, setImportInProgress] = useState<boolean>();
  if (useRouteError())
  {
    setImportInProgress(false);
  }

  return (
    <>
    <Container>
      <Row className="justify-content-md-center" >
        <Col style={{maxWidth: "540px"}}>

            <h1>Create recipe</h1>
            <RouterForm method="post">
              <Form.Group className="margin-bottom-8">
                <Form.Control placeholder="Name" type="text" name="name"></Form.Control>
              </Form.Group>
              <Form.Group className="margin-bottom-8">
                <Form.Label>
                  Tips for recipe writing:
                  <ul>
                    <li> List your ingredients first, then the steps. </li>
                    <li> Write ingredients like this: quantity unit name (example: 2 tablespoons salt)</li>
                    <li> Write steps one line at a time </li>
                  </ul>
                  The AI may get it wrong. It's ok! You can edit it later.
                </Form.Label>
                <Form.Control as="textarea" rows={7} placeholder="Recipe text" type="textarea" name="body"></Form.Control>
              </Form.Group>
              <Form.Group>
                <Button
                  className="width-100"
                  type="submit"
                  name="intent"
                  onClick={() => setImportInProgress(true)}
                  value={SIMPLE_CREATE}>
                    {importInProgress ? <Spinner /> : "Create"}
                  </Button>
              </Form.Group>
            </RouterForm>

            <br />

            <h2>🪄 New! 🪄</h2>
            <p>Take a picture of a recipe and CookTime will use AI to import the content for you</p>

            
            <RouterForm method="post" encType="multipart/form-data">
              <Form.Group controlId="formFile" className="image-selector margin-bottom-8">
                <Form.Control
                  type="file"
                  accept=".jpg,.jpeg,.png"
                  multiple={false}
                  name="files"
                  onChange={e => {
                    let fileList = (e.target as HTMLInputElement).files! as FileList;
                    if (fileList.length != 1) {
                      return;
                    }

                    let reader = new FileReader();
                    reader.readAsDataURL(fileList.item(0)!);
                    reader.onload = (v) => {
                      setImage(fileList.item(0)!);
                      setImageSrc(reader.result as string);
                    }
                  }} />
              </Form.Group>
              <Form.Group>
                <Button
                  className="width-100"
                  type="submit"
                  name="intent"
                  onClick={() => setImportInProgress(true)}
                  value={IMPORT}>{importInProgress ? <Spinner /> : "Import"}</Button>
              </Form.Group>
            </RouterForm>
        </Col>
      </Row>
    </Container>
    </>
  );
}