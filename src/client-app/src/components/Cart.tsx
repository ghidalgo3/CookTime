import { useState, useEffect, useCallback } from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import { Link, useLocation } from 'react-router';
import {
  AggregatedIngredient,
  RecipeListItem,
  createGroceryList,
  getGroceryList,
  getGroceryListIngredients,
  removeFromGroceries,
  updateGroceryRecipeQuantity,
  toggleGroceryIngredientSelected,
  clearGrocerySelectedIngredients
} from 'src/shared/CookTime';
import { useAuthentication } from 'src/components/Authentication/AuthenticationContext';
import { IngredientDisplay } from './Ingredients/IngredientDisplay';

export function ShoppingCart() {
  const { user } = useAuthentication();
  const location = useLocation();
  const [recipes, setRecipes] = useState<RecipeListItem[]>([]);
  const [ingredients, setIngredients] = useState<AggregatedIngredient[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchData = useCallback(async () => {
    // Create the grocery list if it doesn't exist
    await createGroceryList();

    const [groceryList, ingredientsList] = await Promise.all([
      getGroceryList(),
      getGroceryListIngredients()
    ]);

    if (groceryList) {
      setRecipes(groceryList.recipes);
    }
    setIngredients(ingredientsList);
    setLoading(false);
  }, []);

  useEffect(() => {
    if (user) {
      fetchData();
    }
  }, [fetchData, user]);

  const handleQuantityChange = useCallback(async (recipeId: string, currentQuantity: number, delta: number) => {
    const newQuantity = currentQuantity + delta;
    if (newQuantity < 1) return; // Don't allow quantity below 1

    await updateGroceryRecipeQuantity(recipeId, newQuantity);

    // Update local state
    setRecipes(prev => prev.map(item =>
      item.recipe.id === recipeId
        ? { ...item, quantity: newQuantity }
        : item
    ));

    // Refresh ingredients since quantities changed
    const updatedIngredients = await getGroceryListIngredients();
    setIngredients(updatedIngredients);
  }, []);

  const handleSetQuantity = useCallback(async (recipeId: string, newQuantity: number) => {
    if (newQuantity < 1) return;

    await updateGroceryRecipeQuantity(recipeId, newQuantity);

    setRecipes(prev => prev.map(item =>
      item.recipe.id === recipeId
        ? { ...item, quantity: newQuantity }
        : item
    ));

    const updatedIngredients = await getGroceryListIngredients();
    setIngredients(updatedIngredients);
  }, []);

  const handleDeleteRecipe = useCallback(async (recipeId: string) => {
    await removeFromGroceries(recipeId);

    setRecipes(prev => prev.filter(item => item.recipe.id !== recipeId));

    const updatedIngredients = await getGroceryListIngredients();
    setIngredients(updatedIngredients);
  }, []);

  const handleClearList = useCallback(async () => {
    // Remove all recipes from the groceries list
    await Promise.all(recipes.map(item => removeFromGroceries(item.recipe.id)));
    setRecipes([]);
    setIngredients([]);
  }, [recipes]);

  const handleToggleIngredient = useCallback(async (ingredientId: string) => {
    const isNowSelected = await toggleGroceryIngredientSelected(ingredientId);

    setIngredients(prev => prev.map(ing =>
      ing.ingredient.id === ingredientId
        ? { ...ing, selected: isNowSelected }
        : ing
    ));
  }, []);

  const handleClearSelectedIngredients = useCallback(async () => {
    await clearGrocerySelectedIngredients();

    setIngredients(prev => prev.map(ing => ({ ...ing, selected: false })));
  }, []);

  // Sort ingredients: unselected first, then selected
  const sortedIngredients = [...ingredients].sort((a, b) => {
    if (a.selected === b.selected) return 0;
    return a.selected ? 1 : -1;
  });

  if (!user) {
    return (
      <div>
        <h1 className="margin-bottom-20">Groceries List</h1>
        <p className="text-muted">
          <Link to="/signin" state={{ redirectTo: location.pathname }}>Sign in</Link> to track your groceries.
        </p>
      </div>
    );
  }

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <Form>
      <Row>
        <Col className="justify-content-md-left" xs={6}>
          <h1 className="margin-bottom-20">Groceries List</h1>
        </Col>
        <Col>
          <Button variant="danger" className="float-end" onClick={handleClearList}>
            Clear List
          </Button>
        </Col>
      </Row>

      <div className="cart-header">Recipes</div>
      <div>
        {recipes.map((item) => {
          const recipe = item.recipe;
          const servings = item.quantity * (recipe.servingsProduced ?? 1);
          return (
            <Row key={recipe.id} className="align-items-center padding-left-0 margin-top-10">
              <Col className="col d-flex align-items-center">
                <div className="serving-counter-in-cart">
                  <Button
                    variant="danger"
                    className="minus-counter-button"
                    disabled={item.quantity <= 1}
                    onClick={() => handleQuantityChange(recipe.id, item.quantity, -1)}
                  >
                    <i className="bi bi-dash"></i>
                  </Button>
                  <Form.Control
                    onChange={(e) => {
                      const newValue = parseFloat(e.target.value);
                      if (!Number.isNaN(newValue) && newValue >= 1) {
                        handleSetQuantity(recipe.id, newValue);
                      }
                    }}
                    className="form-control count"
                    value={Math.round(servings)}
                  />
                  <Button
                    variant="success"
                    className="plus-counter-button"
                    onClick={() => handleQuantityChange(recipe.id, item.quantity, 1)}
                  >
                    <i className="bi bi-plus"></i>
                  </Button>
                </div>
                <div
                  className="form-control input-field-style margin-left-20 margin-right-10 do-not-overflow-text"
                >
                  <a href={`/recipes/details?id=${recipe.id}&servings=${servings}`}>
                    {recipe.name}
                  </a>
                </div>
                <Button
                  className="float-end height-38"
                  variant="danger"
                  onClick={() => handleDeleteRecipe(recipe.id)}
                >
                  <i className="bi bi-trash"></i>
                </Button>
              </Col>
            </Row>
          );
        })}
        {recipes.length === 0 && (
          <p className="text-muted">No recipes in your grocery list. Add recipes from the recipe details page.</p>
        )}
      </div>

      <div className="cart-header margin-top-15">
        Ingredients
        {ingredients.some(i => i.selected) && (
          <Button
            variant="outline-secondary"
            size="sm"
            className="float-end"
            onClick={handleClearSelectedIngredients}
          >
            Clear checked
          </Button>
        )}
      </div>
      <div>
        {sortedIngredients.map((ing) => {
          const isSelected = ing.selected;
          return (
            <div
              key={`${ing.ingredient.id}-${ing.unit}`}
              onClick={() => handleToggleIngredient(ing.ingredient.id)}
              className="cart-ingredients-list"
              style={{ cursor: 'pointer' }}
            >
              {isSelected ? (
                <i className="bi bi-check-circle padding-right-10"></i>
              ) : (
                <i className="bi bi-circle padding-right-10"></i>
              )}
              <IngredientDisplay
                ingredientRequirement={{
                  ingredient: ing.ingredient,
                  quantity: ing.quantity,
                  unit: ing.unit,
                  id: ing.ingredient.id,
                  text: ing.ingredient.name.split(';')[0].trim(),
                  position: 0
                }}
                strikethrough={isSelected}
              />
            </div>
          );
        })}
        {ingredients.length === 0 && recipes.length > 0 && (
          <p className="text-muted">No ingredients found for the recipes in your list.</p>
        )}
      </div>
    </Form>
  );
}