import React from 'react';
import { Alert, Row } from 'react-bootstrap';
import { RecipeProvider, useRecipeContext } from './RecipeContext';
import { RecipeHeader } from './RecipeHeader';
import { RecipeFields } from './RecipeFields';
import { RecipeComponents } from './RecipeComponents';
import { NutritionSection } from './NutritionSection';
import { ReviewsSection } from './ReviewsSection';
import { DeleteConfirmModal } from './DeleteConfirmModal';
import { RecipeStructuredData } from '../RecipeStructuredData';
import { ImageCarousel } from '../ImageCarousel';
import { RecipeGenerationResult } from 'src/shared/CookTime';
import imgs from 'src/assets';

interface RecipePageProps {
  recipeId: string;
  generatedRecipe?: RecipeGenerationResult;
}

function RecipePageContent({ recipeId }: { recipeId: string }) {
  const { recipe, recipeImages, errorMessage, setErrorMessage } = useRecipeContext();

  const fallbackImage = recipe.staticImage
    ? `/${recipe.staticImage}`
    : imgs.placeholder;

  return (
    <div>
      <DeleteConfirmModal />
      <RecipeStructuredData recipe={recipe} images={recipeImages} />
      <Row>
        <RecipeHeader />
      </Row>

      {errorMessage && (
        <Alert
          variant="danger"
          onClose={() => setErrorMessage(null)}
          dismissible
        >
          <Alert.Heading>Error</Alert.Heading>
          <p>{errorMessage}</p>
        </Alert>
      )}

      <ImageCarousel
        images={recipeImages}
        fallbackImage={fallbackImage}
        className="recipe-image"
      />

      <div>
        <RecipeFields />
        <RecipeComponents />
      </div>

      <NutritionSection />
      <ReviewsSection recipeId={recipeId} />
    </div>
  );
}

export function RecipePage({ recipeId, generatedRecipe }: RecipePageProps) {
  return (
    <RecipeProvider recipeId={recipeId} generatedRecipe={generatedRecipe}>
      <RecipePageContent recipeId={recipeId} />
    </RecipeProvider>
  );
}

// Export for backward compatibility with existing routing
export { RecipePage as RecipeEdit };
