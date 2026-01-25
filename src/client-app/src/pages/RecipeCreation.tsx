import React, { useState } from "react"
import { useNavigate } from "react-router";
import { Alert, Button, Col, Container, Form, Nav, Row, Spinner, Tab } from "react-bootstrap";
import { createRecipeWithName, generateRecipeFromImages, generateRecipeFromText, RecipeGenerationResult } from "src/shared/CookTime";
import { Path } from "./Recipe";
import { useTitle } from "src/shared/useTitle";
import "./RecipeCreation.css";

export const RECIPE_CREATE_PAGE_PATH = "Recipes/Create"

type GenerationMode = "images" | "text";

export default function RecipeCreation() {
  const navigate = useNavigate();
  useTitle("New Recipe")

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string>();
  const [recipeName, setRecipeName] = useState("");

  // Image upload state
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [imagePreviews, setImagePreviews] = useState<string[]>([]);

  // Text input state
  const [recipeText, setRecipeText] = useState("");

  // Generation result state
  const [generationResult, setGenerationResult] = useState<RecipeGenerationResult | null>(null);

  const handleSimpleCreate = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(undefined);

    const formData = new FormData(e.currentTarget);
    const name = formData.get("name")?.toString();

    if (!name) {
      setError("Recipe must have a name");
      setIsSubmitting(false);
      return;
    }

    const result = await createRecipeWithName({ name });

    if (result.ok) {
      const recipe = await result.json();
      navigate(Path(recipe.id));
    } else {
      setError("Something went wrong creating a recipe");
      setIsSubmitting(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    // Limit to 3 files
    const newFiles = [...selectedFiles, ...files].slice(0, 3);
    setSelectedFiles(newFiles);

    // Generate previews
    const previews: string[] = [];
    newFiles.forEach(file => {
      const reader = new FileReader();
      reader.onload = () => {
        previews.push(reader.result as string);
        if (previews.length === newFiles.length) {
          setImagePreviews([...previews]);
        }
      };
      reader.readAsDataURL(file);
    });
  };

  const removeImage = (index: number) => {
    const newFiles = selectedFiles.filter((_, i) => i !== index);
    const newPreviews = imagePreviews.filter((_, i) => i !== index);
    setSelectedFiles(newFiles);
    setImagePreviews(newPreviews);
  };

  const handleGenerateFromImages = async () => {
    if (selectedFiles.length === 0) {
      setError("Please select at least one image");
      return;
    }

    setIsSubmitting(true);
    setError(undefined);
    setGenerationResult(null);

    const result = await generateRecipeFromImages(selectedFiles);

    if (result.ok && result.data) {
      setGenerationResult(result.data);
      // Navigate to edit page - the backend already created the recipe
      navigate(Path(result.data.recipe.id), { state: { generatedRecipe: result.data } });
    } else {
      setError(result.error || "Failed to generate recipe from images");
    }

    setIsSubmitting(false);
  };

  const handleGenerateFromText = async () => {
    if (!recipeText.trim()) {
      setError("Please enter recipe text");
      return;
    }

    setIsSubmitting(true);
    setError(undefined);
    setGenerationResult(null);

    const result = await generateRecipeFromText(recipeText);

    if (result.ok && result.data) {
      setGenerationResult(result.data);
      // Navigate to edit page - the backend already created the recipe
      navigate(Path(result.data.recipe.id), { state: { generatedRecipe: result.data } });
    } else {
      setError(result.error || "Failed to generate recipe from text");
    }

    setIsSubmitting(false);
  };

  return (
    <Container>
      <Row className="justify-content-md-center">
        <Col style={{ maxWidth: "600px" }}>

          <h1>Create Recipe</h1>
          <br />

          {/* <div className="section-divider">
            <span>Choose a method</span>
          </div> */}

          {error && <Alert variant="danger" dismissible onClose={() => setError(undefined)}>{error}</Alert>}

          <Tab.Container defaultActiveKey="images">
            <Nav variant="tabs" className="mb-3">
              <Nav.Item>
                <Nav.Link eventKey="images">📷 From Images</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="text">📝 From Text</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="scratch">✏️ From Scratch</Nav.Link>
              </Nav.Item>
            </Nav>

            <Tab.Content>
              <Tab.Pane eventKey="images">
                <Form.Group controlId="formFileMultiple" className="mb-3">
                  <Form.Label>Let CookTime AI extract the recipe from your photo. Upload up to 3 images of a recipe.</Form.Label>
                  <Form.Control
                    type="file"
                    accept=".jpg,.jpeg,.png,.webp"
                    multiple
                    onChange={handleFileSelect}
                    disabled={selectedFiles.length >= 3}
                  />
                  <Form.Text className="text-muted">
                    Supported formats: JPEG, PNG, WebP (max 5MB each)
                  </Form.Text>
                </Form.Group>

                {imagePreviews.length > 0 && (
                  <div className="image-preview-container mb-3">
                    {imagePreviews.map((preview, index) => (
                      <div key={index} className="image-preview-item">
                        <img src={preview} alt={`Preview ${index + 1}`} />
                        <Button
                          variant="danger"
                          size="sm"
                          className="remove-image-btn"
                          onClick={() => removeImage(index)}
                        >
                          ×
                        </Button>
                      </div>
                    ))}
                  </div>
                )}

                <Button
                  variant="primary"
                  className="width-100"
                  onClick={handleGenerateFromImages}
                  disabled={isSubmitting || selectedFiles.length === 0}
                >
                  {isSubmitting ? (
                    <>
                      <Spinner size="sm" className="me-2" />
                      Analyzing images...
                    </>
                  ) : (
                    "Generate Recipe"
                  )}
                </Button>
              </Tab.Pane>

              <Tab.Pane eventKey="text">
                <Form.Group className="mb-3">
                  <Form.Label>Let CookTime AI extract the recipe from your text.</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={10}
                    placeholder="Paste a recipe here. Include ingredients, quantities, and instructions."
                    value={recipeText}
                    onChange={(e) => setRecipeText(e.target.value)}
                  />
                  {/* <Form.Text className="text-muted">
                    Include ingredient names, quantities, units, and cooking steps
                  </Form.Text> */}
                </Form.Group>

                <Button
                  variant="primary"
                  className="width-100"
                  onClick={handleGenerateFromText}
                  disabled={isSubmitting || !recipeText.trim()}
                >
                  {isSubmitting ? (
                    <>
                      <Spinner size="sm" className="me-2" />
                      Processing text...
                    </>
                  ) : (
                    "Generate Recipe"
                  )}
                </Button>
              </Tab.Pane>

              <Tab.Pane eventKey="scratch">
                <Form onSubmit={handleSimpleCreate}>
                  <Form.Group className="margin-bottom-8">
                    <Form.Label>Start by giving your recipe a name:</Form.Label>
                    <Form.Control
                      required
                      placeholder="Recipe name"
                      type="text"
                      name="name"
                      value={recipeName}
                      onChange={(e) => setRecipeName(e.target.value)}
                    />
                  </Form.Group>
                  <Form.Group>
                    <Button
                      className="width-100"
                      type="submit"
                      disabled={isSubmitting || !recipeName.trim()}>
                      {isSubmitting ? <Spinner size="sm" /> : "Continue to Recipe Editor"}
                    </Button>
                  </Form.Group>
                </Form>
              </Tab.Pane>

            </Tab.Content>
          </Tab.Container>

        </Col>
      </Row>
    </Container>
  );
}