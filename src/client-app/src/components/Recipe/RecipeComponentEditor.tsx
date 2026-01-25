import React from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';
import { RecipeComponent } from 'src/shared/CookTime';
import { IngredientRequirementList, IngredientRequirementEdit } from '../IngredientRequirements';
import { RecipeStepList } from '../RecipeEdit/RecipeStepList';

interface RecipeComponentEditorProps {
  component: RecipeComponent;
  componentIndex: number;
}

export function RecipeComponentEditor({ component, componentIndex }: RecipeComponentEditorProps) {
  const {
    recipe,
    edit,
    units,
    newServings,
    updateComponent,
    appendIngredientToComponent,
    deleteIngredientFromComponent,
    updateIngredientInComponent,
    appendStepToComponent,
    updateStepsInComponent,
    deleteStepFromComponent,
    deleteComponent,
  } = useRecipeContext();

  const hasMultipleComponents = recipe.recipeComponents.length > 1;

  return (
    <div className="border-top-1 margin-bottom-20">
      {hasMultipleComponents && (
        <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
          <Col className="col-3 recipe-field-title">Component Name</Col>
          <Col>
            <Row>
              <Col className="col d-flex align-items-center">
                {edit ? (
                  <Form.Control
                    type="text"
                    onChange={(e) =>
                      updateComponent(componentIndex, { name: e.target.value })
                    }
                    value={component.name}
                  />
                ) : (
                  <div className="component-name-field">{component.name}</div>
                )}
              </Col>
              {edit && (
                <Col xs={1}>
                  <Button
                    className="float-end"
                    variant="danger"
                    onClick={() => deleteComponent(componentIndex)}
                  >
                    <i className="bi bi-trash"></i>
                  </Button>
                </Col>
              )}
            </Row>
          </Col>
        </Row>
      )}

      <Row className="padding-right-0 recipe-edit-row">
        <Col className="col-3 recipe-field-title">Ingredients</Col>
        <Col className="col d-flex align-items-center">
          <div className="ingredient-list">
            {edit ? (
              <IngredientRequirementEdit
                ingredientRequirements={component.ingredients ?? []}
                units={units}
                onDelete={(ir) =>
                  deleteIngredientFromComponent(componentIndex, ir.id)
                }
                onNewIngredientRequirement={() =>
                  appendIngredientToComponent(componentIndex)
                }
                updateIngredientRequirement={(ir, u) =>
                  updateIngredientInComponent(componentIndex, ir.ingredient.id, u)
                }
              />
            ) : (
              <IngredientRequirementList
                ingredientRequirements={component.ingredients ?? []}
                units={units}
                multiplier={newServings / recipe.servingsProduced}
              />
            )}
          </div>
        </Col>
      </Row>

      <Row className="padding-right-0">
        <Col className="col-3 recipe-field-title">Steps</Col>
        <Col className="col d-flex align-items-center">
          <div className="step-list">
            <RecipeStepList
              multipart={true}
              recipe={recipe}
              component={component}
              newServings={newServings}
              edit={edit}
              onDeleteStep={(idx) => deleteStepFromComponent(componentIndex, idx)}
              onChange={(newSteps) => updateStepsInComponent(componentIndex, newSteps)}
              onNewStep={() => appendStepToComponent(componentIndex)}
            />
          </div>
        </Col>
      </Row>
    </div>
  );
}
