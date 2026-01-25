import React from 'react';
import { Col, Row } from 'react-bootstrap';
import { useRecipeContext } from './RecipeContext';
import { NutritionFacts } from '../NutritionFacts';
import { TodaysTenDisplay } from '../todaysTenDisplay';

export function NutritionSection() {
  const { recipe, edit, newServings, nutritionFacts } = useRecipeContext();

  if (edit) return null;

  const todaysTen = nutritionFacts?.dietDetails.find((dd) => dd.name === 'TodaysTen');

  const renderNutritionFacts = () => {
    if ((nutritionFacts?.recipe ?? null) === null) return null;

    const {
      calories,
      carbohydrates,
      proteins,
      polyUnsaturatedFats,
      monoUnsaturatedFats,
      saturatedFats,
      sugars,
      transFats,
      iron,
      vitaminD,
      calcium,
      potassium,
    } = nutritionFacts!.recipe;

    return (
      <NutritionFacts
        calories={Math.round(calories / recipe.servingsProduced)}
        saturatedFats={Math.round(saturatedFats / recipe.servingsProduced)}
        monoUnsaturatedFats={Math.round(monoUnsaturatedFats / recipe.servingsProduced)}
        polyUnsaturatedFats={Math.round(polyUnsaturatedFats / recipe.servingsProduced)}
        transFats={Math.round(transFats / recipe.servingsProduced)}
        carbohydrates={Math.round(carbohydrates / recipe.servingsProduced)}
        sugars={Math.round(sugars / recipe.servingsProduced)}
        proteins={Math.round(proteins / recipe.servingsProduced)}
        servings={newServings}
        potassium={Math.round(potassium / recipe.servingsProduced)}
        vitaminD={Math.round(vitaminD / recipe.servingsProduced)}
        calcium={Math.round(calcium / recipe.servingsProduced)}
        iron={Math.round(iron / recipe.servingsProduced)}
      />
    );
  };

  const renderIngredientNutritionFacts = () => {
    if ((nutritionFacts?.recipe ?? null) === null) return null;

    const items = nutritionFacts?.ingredients.map((description, i) => (
      <div className="nbi-table-entry" key={i}>
        <div>
          {description.quantity}{' '}
          {description.unit === 'count' ? '' : description.unit.toLowerCase()}{' '}
          {description.name}
        </div>
        <div className="nbi-table-source">
          {description.quantity}{' '}
          {description.unit === 'count' ? '' : description.unit.toLowerCase()}{' '}
          {description.nutritionDatabaseId !== null ? (
            <a
              target="_blank"
              rel="noreferrer"
              href={`https://fdc.nal.usda.gov/fdc-app.html#/food-details/${description.nutritionDatabaseId}/nutrients`}
            >
              {description.nutritionDatabaseDescriptor}
            </a>
          ) : (
            description.nutritionDatabaseDescriptor
          )}{' '}
          | {Math.round(description.caloriesPerServing)} calories per serving
        </div>
      </div>
    ));

    return (
      <div className="nbi-table">
        <h1 className="performance-facts__title padding-8">Nutrition by Ingredient</h1>
        <div>{items}</div>
      </div>
    );
  };

  return (
    <>
      {todaysTen && <TodaysTenDisplay todaysTen={todaysTen} />}
      <div className="border-top-1 margin-top-10">
        <Row>
          <Col xs={3} className="nft-row">
            {renderNutritionFacts()}
          </Col>
          <Col>{renderIngredientNutritionFacts()}</Col>
        </Row>
      </div>
    </>
  );
}
