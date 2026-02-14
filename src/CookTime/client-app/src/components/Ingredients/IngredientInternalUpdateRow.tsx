import React, { useEffect, useState } from "react"
import { Accordion, Spinner, Form, Row, Col, Button } from "react-bootstrap";
import { IngredientInternalUpdate } from "src/shared/CookTime";
import NutritionAutosuggest from "./NutritionAutosuggest";

interface IngredientInternalUpdateRowProps extends IngredientInternalUpdate {
  eventKey: string;
}

export default function IngredientInternalUpdateRow(props: IngredientInternalUpdateRowProps) {
  const {
    ingredientId,
    ingredientNames,
    gtinUpc,
    ndbNumber,
    countRegex,
    expectedUnitMass,
    nutritionDescription,
    eventKey
  } = props;
  const [update, setUpdate] = useState(props);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [failed, setFailed] = useState(false);

  // Check if the row should have warning styling (missing nutrition data)
  const hasNoNutritionData = (!update.ndbNumber || update.ndbNumber === 0) &&
    (!update.gtinUpc || update.gtinUpc.trim() === '');

  const handleSave = async () => {
    setIsSubmitting(true);
    const response = await fetch("/api/ingredient/internalupdate", {
      method: "post",
      body: JSON.stringify(update),
      headers: {
        "Content-Type": "application/json"
      }
    })
    setIsSubmitting(false);
    if (response.ok) {
      setUpdate(await response.json());
      setFailed(false);
    } else {
      setFailed(true);
    }
  };

  return (
    <Accordion.Item eventKey={eventKey} className={hasNoNutritionData ? 'border-warning' : ''}>
      <Accordion.Header>
        <div className="d-flex justify-content-between w-100 me-3">
          <span className={hasNoNutritionData ? 'text-warning fw-bold' : ''}>
            {update.ingredientNames || 'Unnamed ingredient'}
          </span>
          <small className="text-muted">
            {nutritionDescription || 'No nutrition data'}
          </small>
        </div>
      </Accordion.Header>
      <Accordion.Body>
        <Form>
          <Form.Group className="mb-3">
            <Form.Label>Ingredient ID</Form.Label>
            <Form.Control type="text" value={ingredientId} disabled />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Ingredient Names (semi-colon separated)</Form.Label>
            <Form.Control
              type="text"
              placeholder="Cooktime ingredient name (semi-colon separated)"
              onChange={e => setUpdate({ ...update, ingredientNames: e.target.value })}
              value={update.ingredientNames}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Nutrition Description</Form.Label>
            <Form.Control type="text" value={nutritionDescription || ''} disabled />
          </Form.Group>

          <Row className="mb-3">
            <Col md={6}>
              <Form.Group>
                <Form.Label>NDB Number</Form.Label>
                <NutritionAutosuggest
                  placeholder="Type ingredient name or NDB Number"
                  value={update.ndbNumber}
                  onSelect={(ndbNumber, gtinUpc) => {
                    if (ndbNumber !== null) {
                      setUpdate({ ...update, ndbNumber });
                    }
                  }}
                  fieldType="ndb"
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group>
                <Form.Label>GTIN/UPC Number</Form.Label>
                <NutritionAutosuggest
                  placeholder="Type ingredient name or GTIN/UPC"
                  value={update.gtinUpc}
                  onSelect={(ndbNumber, gtinUpc) => {
                    if (gtinUpc !== null) {
                      setUpdate({ ...update, gtinUpc });
                    }
                  }}
                  fieldType="gtin"
                />
              </Form.Group>
            </Col>
          </Row>

          <Row className="mb-3">
            <Col md={6}>
              <Form.Group>
                <Form.Label>Count RegEx</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Count RegEx"
                  onChange={e => setUpdate({ ...update, countRegex: e.target.value })}
                  value={update.countRegex}
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group>
                <Form.Label>Unit Mass (kg)</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Expected unit mass"
                  onChange={e => {
                    let unitMass = Number.parseFloat(e.target.value);
                    if (e.target.value === "0.") {
                      setUpdate({ ...update, expectedUnitMass: "0." });
                    } else {
                      if (isNaN(unitMass)) {
                        unitMass = 0.1;
                      }
                      setUpdate({ ...update, expectedUnitMass: unitMass.toString() })
                    }
                  }}
                  value={update.expectedUnitMass}
                />
              </Form.Group>
            </Col>
          </Row>

          <div className="d-flex gap-2">
            {isSubmitting ? (
              <Spinner animation="border" size="sm" />
            ) : (
              <Button
                variant={failed ? "danger" : "outline-success"}
                onClick={handleSave}
              >
                {failed ? "Failed - Retry" : "Save"}
              </Button>
            )}
          </div>
        </Form>
      </Accordion.Body>
    </Accordion.Item>
  );
}