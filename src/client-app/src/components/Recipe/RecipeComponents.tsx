import React from 'react';
import { Button, Col, Form } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';
import { RecipeComponentEditor } from './RecipeComponentEditor';

export function RecipeComponents() {
  const { recipe, edit, appendComponent } = useRecipeContext();

  return (
    <div>
      {recipe.recipeComponents.map((component, index) => (
        <RecipeComponentEditor
          key={component.id}
          component={component}
          componentIndex={index}
        />
      ))}
      {edit && (
        <Form>
          <Col xs={12}>
            <Button
              variant="outline-primary"
              className="width-100"
              onClick={appendComponent}
            >
              New component
            </Button>
          </Col>
        </Form>
      )}
    </div>
  );
}
