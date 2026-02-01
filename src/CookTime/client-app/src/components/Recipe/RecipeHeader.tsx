import React from 'react';
import { Col, Form, Stack } from 'react-bootstrap';
import { Rating } from '@smastrom/react-rating';
import { useRecipeContext } from './RecipeContext';
import { AuthContext } from '../Authentication/AuthenticationContext';
import { RecipeEditButtons } from './RecipeEditButtons';

export function RecipeHeader() {
  const {
    recipe,
    edit,
    operationInProgress,
    setEdit,
    updateRecipe,
    onSave,
    onCancel,
    onDelete,
    onAddToList,
  } = useRecipeContext();

  return (
    <>
      <Col className="justify-content-md-left" xs={6}>
        {edit ? (
          <div className="recipe-name-input">
            <Form.Control
              type="text"
              onChange={(e) => updateRecipe({ name: e.target.value })}
              value={recipe.name}
            />
          </div>
        ) : (
          <h1>{recipe.name}</h1>
        )}
        {recipe.reviewCount > 0 && (
          <Stack direction="horizontal" className="margin-bottom-8">
            <Rating style={{ maxWidth: 150 }} value={recipe.averageReviews} readOnly /> (
            {recipe.reviewCount})
          </Stack>
        )}
        By {recipe.owner?.userName}
      </Col>
      <AuthContext.Consumer>
        {({ user }) => (
          <RecipeEditButtons
            user={user}
            recipe={recipe}
            edit={edit}
            operationInProgress={operationInProgress}
            onSave={() => onSave()}
            onCancel={onCancel}
            onDelete={onDelete}
            onToggleEdit={() => setEdit(!edit)}
            onAddToList={(listName) => onAddToList(listName)}
          />
        )}
      </AuthContext.Consumer>
    </>
  );
}
