import React, { useState } from "react"
import { Accordion, Spinner, Form, Row, Col, Button, Badge } from "react-bootstrap";
import { EMPTY_GUID, IngredientUnified } from "src/shared/CookTime";
import NutritionAutosuggest from "./NutritionAutosuggest";
import IngredientAutosuggest from "./IngredientAutosuggest";

interface IngredientUnifiedRowProps extends IngredientUnified {
    eventKey: string;
    onError: (error: string) => void;
    onDeleted: () => void;
}

export default function IngredientUnifiedRow(props: IngredientUnifiedRowProps) {
    const {
        ingredientId,
        ingredientNames,
        gtinUpc,
        ndbNumber,
        countRegex,
        expectedUnitMass,
        nutritionDescription,
        usage,
        hasNutrition,
        eventKey,
        onError,
        onDeleted
    } = props;

    const [update, setUpdate] = useState(props);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [saveStatus, setSaveStatus] = useState<'idle' | 'success' | 'failed'>('idle');
    const [mergeTargetId, setMergeTargetId] = useState<string>(EMPTY_GUID);

    // Check if the row should have warning styling (missing nutrition data)
    const hasNoNutritionData = (!update.ndbNumber || update.ndbNumber === 0) &&
        (!update.gtinUpc || update.gtinUpc.trim() === '');

    const handleSave = async () => {
        setIsSubmitting(true);
        setSaveStatus('idle');
        try {
            const response = await fetch("/api/ingredient/internalupdate", {
                method: "post",
                body: JSON.stringify({
                    ingredientId: update.ingredientId,
                    ingredientNames: update.ingredientNames,
                    ndbNumber: update.ndbNumber,
                    gtinUpc: update.gtinUpc,
                    countRegex: update.countRegex,
                    expectedUnitMass: update.expectedUnitMass,
                    nutritionDescription: update.nutritionDescription
                }),
                headers: {
                    "Content-Type": "application/json"
                }
            });
            if (response.ok) {
                const result = await response.json();
                setUpdate({ ...update, ...result });
                setSaveStatus('success');
            } else {
                const errorText = await response.text();
                onError(`Failed to save: ${errorText}`);
                setSaveStatus('failed');
            }
        } catch (err) {
            onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
            setSaveStatus('failed');
        }
        setIsSubmitting(false);
    };

    const handleMerge = async () => {
        if (!mergeTargetId || mergeTargetId === EMPTY_GUID) return;

        setIsSubmitting(true);
        try {
            const response = await fetch("/api/ingredient/replace", {
                method: "post",
                body: JSON.stringify({
                    fromIngredientId: ingredientId,
                    toIngredientId: mergeTargetId
                }),
                headers: {
                    "Content-Type": "application/json"
                }
            });
            if (response.ok) {
                onDeleted();
            } else {
                const errorText = await response.text();
                onError(`Failed to merge: ${errorText}`);
            }
        } catch (err) {
            onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
        }
        setIsSubmitting(false);
    };

    const handleDelete = async () => {
        setIsSubmitting(true);
        try {
            const response = await fetch(`/api/ingredient/${ingredientId}`, {
                method: "delete",
            });
            if (response.ok) {
                onDeleted();
            } else {
                const errorText = await response.text();
                onError(`Failed to delete: ${errorText}`);
            }
        } catch (err) {
            onError(`Network error: ${err instanceof Error ? err.message : 'Unknown error'}`);
        }
        setIsSubmitting(false);
    };

    return (
        <Accordion.Item eventKey={eventKey} className={hasNoNutritionData ? 'border-warning' : ''}>
            <Accordion.Header>
                <div className="d-flex justify-content-between align-items-center w-100 me-3">
                    <span className={hasNoNutritionData ? 'text-warning fw-bold' : ''}>
                        {update.ingredientNames || 'Unnamed ingredient'}
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
                        <Row className="mb-3">
                            <Col md={8}>
                                <Form.Group>
                                    <Form.Label>Ingredient ID</Form.Label>
                                    <Form.Control type="text" value={ingredientId} disabled />
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group>
                                    <Form.Label>Recipe Usage</Form.Label>
                                    <Form.Control type="text" value={`${usage} recipe${usage !== 1 ? 's' : ''}`} disabled />
                                </Form.Group>
                            </Col>
                        </Row>

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
                            <Form.Control type="text" value={nutritionDescription || 'No nutrition data linked'} disabled />
                        </Form.Group>

                        <Row className="mb-3">
                            <Col md={6}>
                                <Form.Group>
                                    <Form.Label>NDB Number (SR Legacy)</Form.Label>
                                    <NutritionAutosuggest
                                        placeholder="Type ingredient name or NDB Number"
                                        value={update.ndbNumber}
                                        onSelect={(ndbNumber, _gtinUpc) => {
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
                                    <Form.Label>GTIN/UPC Number (Branded)</Form.Label>
                                    <NutritionAutosuggest
                                        placeholder="Type ingredient name or GTIN/UPC"
                                        value={update.gtinUpc}
                                        onSelect={(_ndbNumber, gtinUpc) => {
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
                                                setUpdate({ ...update, expectedUnitMass: unitMass.toString() });
                                            }
                                        }}
                                        value={update.expectedUnitMass}
                                    />
                                </Form.Group>
                            </Col>
                        </Row>

                        <div className="d-flex gap-2 mb-4">
                            {isSubmitting ? (
                                <Spinner animation="border" size="sm" />
                            ) : (
                                <Button
                                    variant={saveStatus === 'failed' ? "danger" : saveStatus === 'success' ? "success" : "outline-success"}
                                    onClick={handleSave}
                                >
                                    {saveStatus === 'failed' ? "Failed - Retry" : saveStatus === 'success' ? "Saved ✓" : "Save Changes"}
                                </Button>
                            )}
                        </div>

                        <hr />

                        <h6 className="text-muted mb-3">Merge or Delete</h6>

                        <Form.Group className="mb-3">
                            <Form.Label>Merge Into Another Ingredient</Form.Label>
                            <IngredientAutosuggest
                                placeholder="Search for ingredient to merge into..."
                                value=""
                                onSelect={(ingredientId, _ingredientName) => setMergeTargetId(ingredientId)}
                                excludeId={ingredientId}
                            />
                            <Form.Text className="text-muted">
                                This will move all recipe references to the target ingredient and delete this one
                            </Form.Text>
                        </Form.Group>

                        <div className="d-flex gap-2">
                            <Button
                                variant="outline-warning"
                                onClick={handleMerge}
                                disabled={isSubmitting || !mergeTargetId || mergeTargetId === EMPTY_GUID}
                            >
                                Merge & Delete
                            </Button>
                            {usage === 0 && (
                                <Button
                                    variant="outline-danger"
                                    onClick={handleDelete}
                                    disabled={isSubmitting}
                                >
                                    Delete Ingredient
                                </Button>
                            )}
                        </div>
                    </Form>
                </div>
            </Accordion.Collapse>
        </Accordion.Item>
    );
}
