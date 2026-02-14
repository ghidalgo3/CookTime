import React, { useState } from "react"
import { Accordion, Form, Button, Row, Col, Badge } from "react-bootstrap";
import { EMPTY_GUID, IngredientReplacementRequest } from "src/shared/CookTime";
import IngredientAutosuggest from "./IngredientAutosuggest";

interface IngredientNormalizerRowProps extends IngredientReplacementRequest {
  onError: (error: string) => void;
  eventKey: string;
}

export default function IngredientNormalizerRow(props: IngredientNormalizerRowProps) {
  const {
    replacedId,
    name,
    usage,
    hasNutrition,
    keptId,
    onError,
    eventKey
  } = props;
  const [replacement, setReplacement] = useState(props);

  const handleReplace = async () => {
    try {
      const requestBody = {
        fromIngredientId: replacedId,
        toIngredientId: replacement.keptId
      };
      const response = await fetch("/api/ingredient/replace", {
        method: "post",
        body: JSON.stringify(requestBody),
        headers: {
          "Content-Type": "application/json"
        }
      })
      if (!response.ok) {
        const errorText = await response.text();
        onError(`Failed to replace ingredient: ${errorText}`);
        return;
      }

      location.reload();
    } catch (err) {
      onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
    }
  };

  const handleDelete = async () => {
    try {
      const response = await fetch(`/api/ingredient/${replacedId}`, {
        method: "delete",
      })
      if (!response.ok) {
        const errorText = await response.text();
        onError(`Failed to delete ingredient: ${errorText}`);
        return;
      }

      location.reload();
    } catch (err) {
      onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
    }
  };

  return (
    <Accordion.Item eventKey={eventKey} className={!hasNutrition ? 'border-warning' : ''}>
      <Accordion.Header>
        <div className="d-flex justify-content-between align-items-center w-100 me-3">
          <span className={!hasNutrition ? 'text-warning fw-bold' : ''}>
            {name}
          </span>
          <div className="d-flex gap-2 align-items-center">
            <Badge bg={usage > 0 ? "primary" : "secondary"}>
              {usage} recipe{usage !== 1 ? 's' : ''}
            </Badge>
            <span style={{ fontSize: '18px' }}>
              {hasNutrition ? '✓' : '✗'}
            </span>
          </div>
        </div>
      </Accordion.Header>
      <Accordion.Collapse eventKey={eventKey} mountOnEnter>
        <div className="accordion-body">
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Ingredient ID</Form.Label>
              <Form.Control type="text" value={replacedId} disabled />
              <Form.Text className="text-muted">
                Copy this ID to merge this ingredient into another
              </Form.Text>
            </Form.Group>

            <Row className="mb-3">
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Recipes Using</Form.Label>
                  <Form.Control type="text" value={usage} disabled />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Has Nutrition Data</Form.Label>
                  <Form.Control type="text" value={hasNutrition ? 'Yes' : 'No'} disabled />
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3">
              <Form.Label>Replace With (search by ingredient name)</Form.Label>
              <IngredientAutosuggest
                placeholder="Type ingredient name to search"
                value=""
                onSelect={(ingredientId, ingredientName) => setReplacement({ ...replacement, keptId: ingredientId })}
                excludeId={replacedId}
              />
              <Form.Text className="text-muted">
                This will merge all recipe references to the target ingredient
              </Form.Text>
            </Form.Group>

            <div className="d-flex gap-2">
              <Button
                variant="outline-success"
                onClick={handleReplace}
                disabled={!replacement.keptId || replacement.keptId === EMPTY_GUID}
              >
                Replace & Merge
              </Button>
              {usage === 0 && (
                <Button
                  variant="outline-danger"
                  onClick={handleDelete}
                >
                  Delete
                </Button>
              )}
            </div>
          </Form>
        </div>
      </Accordion.Collapse>
    </Accordion.Item>
  );
}