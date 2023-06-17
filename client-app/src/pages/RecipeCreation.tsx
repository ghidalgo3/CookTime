import React, {useEffect, useState} from "react"
import { ActionFunctionArgs, Form as RouterForm, redirect } from "react-router-dom";
import { Button, Col, Container, Form, Row } from "react-bootstrap";
import { createRecipeWithName, importRecipeFromImage, MultiPartRecipe } from "src/shared/CookTime";
import { Path } from "./Recipe";
import { useTitle } from "src/shared/useTitle";

export async function action(args : ActionFunctionArgs) {
  const formData = await args.request.formData();
  const formName = formData.get("intent")?.toString();
  const name = formData.get("name")?.toString();
  if (formName === SIMPLE_CREATE && name) {
    const result = await createRecipeWithName(name);
    if (result.ok) {
      var recipe = await result.json() as MultiPartRecipe;
      return redirect(Path(recipe.id))
    } else {
      return { errors: "Something went wrong" }
    }
  } else if (formName === IMPORT) {
    await importRecipeFromImage(formData)
    return null;
  }
  else {
    return {errors: "Name is required"}
  }
}

const SIMPLE_CREATE = "Simple create";
const IMPORT = "Import from image";

export default function RecipeCreation() {

  useTitle("New Recipe")

  const [image, setImage] = useState<Blob>();
  const [imageSrc, setImageSrc] = useState<string>();

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
                <Button className="width-100" type="submit" name="intent" value={SIMPLE_CREATE}>Create</Button>
              </Form.Group>
              {/* <Form.Control></Form.Control> */}
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
                <Button className="width-100" type="submit" name="intent" value={IMPORT}>Import</Button>
              </Form.Group>
            </RouterForm>
        </Col>
      </Row>
    </Container>
    </>
  );
}