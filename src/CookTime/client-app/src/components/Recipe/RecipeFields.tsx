import React from 'react';
import { Button, Col, DropdownButton, Dropdown, Form, Row } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';
import { ImageEditor } from './ImageEditor';
import { Tags } from '../Tags/Tags';
import { UnitPreference } from 'src/shared/units';

export function RecipeFields() {
  const {
    recipe,
    edit,
    newServings,
    nutritionFacts,
    recipeImages,
    pendingImages,
    imageOrder,
    imageOperationInProgress,
    unitPreference,
    setNewServings,
    updateRecipe,
    setUnitPreference,
    handleAddImages,
    handleRemoveExistingImage,
    handleRemovePendingImage,
    updateImageOrder,
  } = useRecipeContext();

  const toTitleCase = (str: string) => {
    return str.replace(/\w\S*/g, (txt) => {
      return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
  };

  // Calories per serving logic
  const renderCaloriesPerServing = () => {
    let rightColContents: React.ReactNode = null;

    if (
      !edit &&
      recipe.caloriesPerServing === 0 &&
      (nutritionFacts?.recipe?.calories ?? 0) === 0
    ) {
      return null;
    } else if (
      !edit &&
      recipe.caloriesPerServing === 0 &&
      (nutritionFacts?.recipe?.calories ?? 0) !== 0
    ) {
      rightColContents = (
        <div>
          {Math.round(nutritionFacts!.recipe.calories / recipe.servingsProduced)} kcal{' '}
          <i className="bi bi-calculator"></i>
        </div>
      );
    } else if (!edit && recipe.caloriesPerServing !== 0) {
      rightColContents = <div>{recipe.caloriesPerServing}</div>;
    } else if (edit) {
      rightColContents = (
        <Form.Control
          type="number"
          min="0"
          onChange={(e) =>
            updateRecipe({ caloriesPerServing: parseInt(e.target.value) })
          }
          value={recipe.caloriesPerServing}
        />
      );
    }

    return (
      <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
        <Col className="col-3 recipe-field-title">Calories per Serving</Col>
        <Col className="col d-flex align-items-center">{rightColContents}</Col>
      </Row>
    );
  };

  // Servings field
  const renderServings = () => (
    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
      <Col className="col-3 recipe-field-title">Servings</Col>
      <Col className="col d-flex align-items-center">
        {edit ? (
          <Form.Control
            type="number"
            min="1"
            onChange={(e) =>
              updateRecipe({ servingsProduced: parseInt(e.target.value) })
            }
            value={recipe.servingsProduced}
          />
        ) : (
          <div className="serving-counter">
            <Button
              variant="danger"
              className="minus-counter-button"
              onClick={() => {
                if (newServings > 0) {
                  setNewServings(newServings - 1);
                }
              }}
            >
              <i className="bi bi-dash"></i>
            </Button>
            <Form.Control
              onChange={(e) => {
                if (e.target.value === '') {
                  setNewServings(0);
                }
                const newValue = parseFloat(e.target.value);
                if (!Number.isNaN(newValue) && newValue > 0) {
                  setNewServings(newValue);
                }
              }}
              className="form-control count"
              value={newServings}
            />
            <Button
              variant="success"
              className="plus-counter-button"
              onClick={() => setNewServings(newServings + 1)}
            >
              <i className="bi bi-plus"></i>
            </Button>
          </div>
        )}
      </Col>
    </Row>
  );

  // Categories field
  const renderCategories = () => {
    if (!edit && recipe.categories.length === 0) return null;

    return (
      <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
        <Col className="col-3 recipe-field-title">Categories</Col>
        <Col className="col d-flex align-items-center">
          {edit ? (
            <Tags
              queryBuilder={(value) => `/api/recipe/tags?query=${value}`}
              initialTags={recipe.categories}
              tagsChanged={(newTags) => updateRecipe({ categories: newTags })}
            />
          ) : (
            <div className="tag-style">
              {recipe.categories.map((cat) => toTitleCase(cat.name)).join(', ')}
            </div>
          )}
        </Col>
      </Row>
    );
  };

  // Cook time field
  const renderCookTime = () => {
    if (!edit && (recipe.cooktimeMinutes === 0 || recipe.cooktimeMinutes == null)) {
      return null;
    }

    return (
      <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
        <Col className="col-3 recipe-field-title">Cook Time (Minutes)</Col>
        <Col className="col d-flex align-items-center">
          {edit ? (
            <Form.Control
              type="number"
              onChange={(e) =>
                updateRecipe({ cooktimeMinutes: parseInt(e.target.value) })
              }
              value={recipe.cooktimeMinutes}
            />
          ) : (
            <div>{recipe.cooktimeMinutes}</div>
          )}
        </Col>
      </Row>
    );
  };

  // Source link field
  const renderSource = () => {
    if ((recipe.source === '' || recipe.source == null) && !edit) {
      return null;
    }

    return (
      <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
        <Col className="col-3 recipe-field-title">Link to Original Recipe</Col>
        <Col className="col d-flex align-items-center">
          {edit ? (
            <Form.Control
              type="text"
              onChange={(e) => updateRecipe({ source: e.target.value })}
              value={recipe.source}
            />
          ) : (
            <a href={recipe.source}>{recipe.source}</a>
          )}
        </Col>
      </Row>
    );
  };

  // Images field (edit mode only)
  const renderImages = () => {
    if (!edit) return null;

    return (
      <Row className="padding-right-0 recipe-edit-row">
        <Col className="col-3 recipe-field-title">Images</Col>
        <Col className="col">
          <ImageEditor
            images={recipeImages}
            pendingImages={pendingImages}
            imageOrder={imageOrder}
            onReorder={updateImageOrder}
            onRemoveExisting={(imageId) => handleRemoveExistingImage(imageId)}
            onRemovePending={handleRemovePendingImage}
            onAddImages={handleAddImages}
            disabled={imageOperationInProgress}
            maxImages={10}
          />
        </Col>
      </Row>
    );
  };

  const renderUnitPreference = () => (
    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
      <Col className="col-3 recipe-field-title">Units</Col>
      <Col className="col d-flex align-items-center">
        <DropdownButton
          variant="secondary"
          title={
            unitPreference === 'imperial'
              ? 'Imperial'
              : unitPreference === 'metric'
                ? 'Metric'
                : 'Recipe'
          }
        >
          <Dropdown.Item
            active={unitPreference === 'recipe'}
            onClick={() => setUnitPreference('recipe' as UnitPreference)}
          >
            Recipe
          </Dropdown.Item>
          <Dropdown.Item
            active={unitPreference === 'imperial'}
            onClick={() => setUnitPreference('imperial' as UnitPreference)}
          >
            Imperial
          </Dropdown.Item>
          <Dropdown.Item
            active={unitPreference === 'metric'}
            onClick={() => setUnitPreference('metric' as UnitPreference)}
          >
            Metric
          </Dropdown.Item>
        </DropdownButton>
      </Col>
    </Row>
  );

  return (
    <div>
      {renderCaloriesPerServing()}
      {renderServings()}
      {renderUnitPreference()}
      {renderCategories()}
      {renderCookTime()}
      {renderSource()}
      {renderImages()}
    </div>
  );
}
