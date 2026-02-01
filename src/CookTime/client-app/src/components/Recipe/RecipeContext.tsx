import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { v4 as uuidv4 } from 'uuid';
import {
  MeasureUnit,
  MultiPartRecipe,
  Image,
  RecipeNutritionFacts,
  IngredientRequirement,
  RecipeComponent,
  RecipeGenerationResult,
  IngredientMatch,
  deleteRecipeImage,
  reorderRecipeImages,
  uploadRecipeImage,
  toRecipeUpdateDto,
  addToList,
} from 'src/shared/CookTime';

export type PendingImage = {
  id: string;
  file: File;
  previewUrl: string;
};

export type ImageOrderItem = {
  id: string;
  isPending: boolean;
};

interface RecipeContextState {
  recipe: MultiPartRecipe;
  recipeImages: Image[];
  pendingImages: PendingImage[];
  imageOrder: ImageOrderItem[];
  imageOperationInProgress: boolean;
  edit: boolean;
  units: MeasureUnit[];
  newServings: number;
  errorMessage: string | null;
  operationInProgress: boolean;
  nutritionFacts: RecipeNutritionFacts | undefined;
  showDeleteConfirm: boolean;
  ingredientMatches: IngredientMatch[];
  toastMessage: string | null;
}

interface RecipeContextActions {
  // Edit mode
  setEdit: (edit: boolean) => void;
  setShowDeleteConfirm: (show: boolean) => void;
  setErrorMessage: (message: string | null) => void;
  setNewServings: (servings: number) => void;
  setToastMessage: (message: string | null) => void;

  // Recipe updates
  updateRecipe: (updates: Partial<MultiPartRecipe>) => void;
  updateComponent: (componentIndex: number, updates: Partial<RecipeComponent>) => void;

  // Ingredient management
  appendIngredientToComponent: (componentIndex: number) => void;
  deleteIngredientFromComponent: (componentIndex: number, ingredientId: string) => void;
  updateIngredientInComponent: (
    componentIndex: number,
    ingredientId: string,
    updater: (ir: IngredientRequirement) => IngredientRequirement
  ) => void;

  // Step management
  appendStepToComponent: (componentIndex: number) => void;
  updateStepsInComponent: (componentIndex: number, newSteps: string[]) => void;
  deleteStepFromComponent: (componentIndex: number, stepIndex: number) => void;

  // Component management
  appendComponent: () => void;
  deleteComponent: (componentIndex: number) => void;

  // Image management
  handleAddImages: (files: FileList) => void;
  handleRemoveExistingImage: (imageId: string) => Promise<void>;
  handleRemovePendingImage: (imageId: string) => void;
  updateImageOrder: (newOrder: ImageOrderItem[]) => void;

  // Save/Delete/Cancel
  onSave: () => Promise<void>;
  onDelete: () => void;
  onConfirmDelete: () => Promise<void>;
  onCancel: () => void;
  onAddToList: (listName: string) => Promise<void>;
}

interface RecipeContextValue extends RecipeContextState, RecipeContextActions {}

const RecipeContext = createContext<RecipeContextValue | undefined>(undefined);

export function useRecipeContext() {
  const context = useContext(RecipeContext);
  if (!context) {
    throw new Error('useRecipeContext must be used within a RecipeProvider');
  }
  return context;
}

interface RecipeProviderProps {
  recipeId: string;
  generatedRecipe?: RecipeGenerationResult;
  children: React.ReactNode;
}

const defaultRecipe: MultiPartRecipe = {
  id: '',
  name: '',
  source: '',
  cooktimeMinutes: 5,
  caloriesPerServing: 100,
  servingsProduced: 2,
  categories: [],
  staticImage: '',
  owner: null,
  recipeComponents: [],
  reviewCount: 0,
  averageReviews: 4.0,
};

export function RecipeProvider({ recipeId, generatedRecipe, children }: RecipeProviderProps) {
  const [recipe, setRecipe] = useState<MultiPartRecipe>(defaultRecipe);
  const [recipeImages, setRecipeImages] = useState<Image[]>([]);
  const [pendingImages, setPendingImages] = useState<PendingImage[]>([]);
  const [imageOrder, setImageOrder] = useState<ImageOrderItem[]>([]);
  const [imageOperationInProgress, setImageOperationInProgress] = useState(false);
  const [edit, setEdit] = useState(false);
  const [units, setUnits] = useState<MeasureUnit[]>([]);
  const [newServings, setNewServings] = useState(1);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [operationInProgress, setOperationInProgress] = useState(false);
  const [nutritionFacts, setNutritionFacts] = useState<RecipeNutritionFacts | undefined>();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [ingredientMatches, setIngredientMatches] = useState<IngredientMatch[]>([]);
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  // Apply generated recipe data
  const applyGeneratedRecipe = useCallback(
    (baseRecipe: MultiPartRecipe, generated: RecipeGenerationResult): MultiPartRecipe => {
      const { recipe: genRecipe } = generated;

      const recipeComponents: RecipeComponent[] = genRecipe.components.map((component, componentIndex) => {
        const ingredients: IngredientRequirement[] = component.ingredients.map((genIng, ingIndex) => ({
          id: genIng.id || uuidv4(),
          ingredient: genIng.ingredient,
          quantity: genIng.quantity,
          unit: genIng.unit ?? 'count',
          text: genIng.text ?? '',
          position: genIng.position ?? ingIndex,
        }));

        return {
          id: uuidv4(),
          name: component.name ?? '',
          position: component.position ?? componentIndex,
          steps: component.steps,
          ingredients,
        };
      });

      return {
        ...baseRecipe,
        name: genRecipe.name || baseRecipe.name,
        servingsProduced: genRecipe.servings ?? baseRecipe.servingsProduced,
        cooktimeMinutes: genRecipe.cookingMinutes ?? baseRecipe.cooktimeMinutes,
        source: genRecipe.source ?? baseRecipe.source,
        recipeComponents: recipeComponents.length > 0 ? recipeComponents : baseRecipe.recipeComponents,
      };
    },
    []
  );

  // Load initial data
  useEffect(() => {
    // Fetch units
    fetch('/api/recipe/units')
      .then((res) => res.json())
      .then((result) => setUnits(result as MeasureUnit[]));

    // Fetch recipe
    fetch(`/api/multipartrecipe/${recipeId}`)
      .then((res) => res.json())
      .then((result) => {
        let r = result as MultiPartRecipe;
        document.title = `${r.name} - CookTime`;
        r.recipeComponents.sort((a, b) => a.position - b.position);
        r.recipeComponents.forEach((comp) => {
          comp.ingredients?.sort((a, b) => a.position - b.position);
        });

        // Check for servings query param
        const urlParams = new URLSearchParams(window.location.search);
        const servingsParam = urlParams.get('servings');
        const initialServings = servingsParam ? parseInt(servingsParam) : r.servingsProduced;
        setNewServings(initialServings);

        // Apply generated recipe if present
        if (generatedRecipe) {
          r = applyGeneratedRecipe(r, generatedRecipe);
          setRecipe(r);
          setEdit(true);
          setIngredientMatches(generatedRecipe.ingredientMatches);
        } else {
          setRecipe(r);
        }
      });

    // Fetch images
    fetch(`/api/MultiPartRecipe/${recipeId}/images`)
      .then((res) => res.json())
      .then((result) => {
        const images = result as Image[];
        setRecipeImages(images);
        setImageOrder(images.map((img) => ({ id: img.id, isPending: false })));
      });

    // Fetch nutrition data
    fetch(`/api/MultiPartRecipe/${recipeId}/nutritionData`)
      .then((res) => res.json())
      .then((result) => setNutritionFacts(result as RecipeNutritionFacts));
  }, [recipeId, generatedRecipe, applyGeneratedRecipe]);

  // Recipe updates
  const updateRecipe = useCallback((updates: Partial<MultiPartRecipe>) => {
    setRecipe((prev) => ({ ...prev, ...updates }));
  }, []);

  const updateComponent = useCallback((componentIndex: number, updates: Partial<RecipeComponent>) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      newComponents[componentIndex] = { ...newComponents[componentIndex], ...updates };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  // Ingredient management
  const appendIngredientToComponent = useCallback((componentIndex: number) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      const component = newComponents[componentIndex];
      const newIngredient: IngredientRequirement = {
        ingredient: { name: '', id: uuidv4(), isNew: false, densityKgPerL: 1 },
        unit: 'count',
        quantity: 0,
        id: uuidv4(),
        text: '',
        position: component.ingredients?.length ?? 0,
      };
      newComponents[componentIndex] = {
        ...component,
        ingredients: [...(component.ingredients ?? []), newIngredient],
      };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  const deleteIngredientFromComponent = useCallback((componentIndex: number, ingredientId: string) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      const component = newComponents[componentIndex];
      const newIngredients = (component.ingredients ?? [])
        .filter((i) => i.id !== ingredientId)
        .map((e, i) => ({ ...e, position: i }));
      newComponents[componentIndex] = { ...component, ingredients: newIngredients };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  const updateIngredientInComponent = useCallback(
    (
      componentIndex: number,
      ingredientId: string,
      updater: (ir: IngredientRequirement) => IngredientRequirement
    ) => {
      setRecipe((prev) => {
        const newComponents = [...prev.recipeComponents];
        const component = newComponents[componentIndex];
        const ingredients = component.ingredients ?? [];
        const idx = ingredients.findIndex((i) => i.ingredient.id === ingredientId);
        if (idx >= 0) {
          const newIngredients = [...ingredients];
          newIngredients[idx] = updater(newIngredients[idx]);
          newComponents[componentIndex] = { ...component, ingredients: newIngredients };
        }
        return { ...prev, recipeComponents: newComponents };
      });
    },
    []
  );

  // Step management
  const appendStepToComponent = useCallback((componentIndex: number) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      const component = newComponents[componentIndex];
      newComponents[componentIndex] = {
        ...component,
        steps: [...(component.steps ?? []), ''],
      };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  const updateStepsInComponent = useCallback((componentIndex: number, newSteps: string[]) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      newComponents[componentIndex] = { ...newComponents[componentIndex], steps: newSteps };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  const deleteStepFromComponent = useCallback((componentIndex: number, stepIndex: number) => {
    setRecipe((prev) => {
      const newComponents = [...prev.recipeComponents];
      const component = newComponents[componentIndex];
      const newSteps = component.steps?.filter((_, i) => i !== stepIndex);
      newComponents[componentIndex] = { ...component, steps: newSteps };
      return { ...prev, recipeComponents: newComponents };
    });
  }, []);

  // Component management
  const appendComponent = useCallback(() => {
    setRecipe((prev) => ({
      ...prev,
      recipeComponents: [
        ...prev.recipeComponents,
        {
          id: uuidv4(),
          name: '',
          ingredients: [],
          steps: [],
          position: prev.recipeComponents.length,
        },
      ],
    }));
  }, []);

  const deleteComponent = useCallback((componentIndex: number) => {
    setRecipe((prev) => ({
      ...prev,
      recipeComponents: prev.recipeComponents.filter((_, i) => i !== componentIndex),
    }));
  }, []);

  // Image management
  const handleAddImages = useCallback((files: FileList) => {
    const currentTotal = recipeImages.length + pendingImages.length;
    const maxToAdd = 10 - currentTotal;

    if (maxToAdd <= 0) return;

    const filesToAdd = Array.from(files).slice(0, maxToAdd);
    const newPending: PendingImage[] = [];
    const newOrderItems: ImageOrderItem[] = [];

    filesToAdd.forEach((file) => {
      const reader = new FileReader();
      reader.onload = () => {
        const pendingImage: PendingImage = {
          id: uuidv4(),
          file,
          previewUrl: reader.result as string,
        };
        newPending.push(pendingImage);
        newOrderItems.push({ id: pendingImage.id, isPending: true });

        if (newPending.length === filesToAdd.length) {
          setPendingImages((prev) => [...prev, ...newPending]);
          setImageOrder((prev) => [...prev, ...newOrderItems]);
        }
      };
      reader.readAsDataURL(file);
    });
  }, [recipeImages.length, pendingImages.length]);

  const handleRemoveExistingImage = useCallback(
    async (imageId: string) => {
      setImageOperationInProgress(true);

      const result = await deleteRecipeImage(recipeId, imageId);

      if (result.ok) {
        setRecipeImages((prev) => prev.filter((img) => img.id !== imageId));
        setImageOrder((prev) => prev.filter((item) => item.id !== imageId));
        setImageOperationInProgress(false);
      } else {
        console.error('Failed to delete image:', result.error);
        setErrorMessage(result.error || 'Failed to delete image');
        setImageOperationInProgress(false);
      }
    },
    [recipeId]
  );

  const handleRemovePendingImage = useCallback((imageId: string) => {
    setPendingImages((prev) => prev.filter((img) => img.id !== imageId));
    setImageOrder((prev) => prev.filter((item) => item.id !== imageId));
  }, []);

  const updateImageOrder = useCallback((newOrder: ImageOrderItem[]) => {
    setImageOrder(newOrder);
  }, []);

  // Save
  const onSave = useCallback(async () => {
    setOperationInProgress(true);

    // Clean up empty steps and ingredients
    const cleanedRecipe = { ...recipe };
    for (const component of cleanedRecipe.recipeComponents) {
      component.steps = (component.steps ?? []).filter(
        (step) => step != null && step.trim() !== ''
      );
      component.ingredients = (component.ingredients ?? []).filter(
        (ingredient) => ingredient.ingredient.name != null && ingredient.ingredient.name !== ''
      );
    }

    if (cleanedRecipe.caloriesPerServing === null || isNaN(cleanedRecipe.caloriesPerServing)) {
      cleanedRecipe.caloriesPerServing = 0.0;
    }

    if (cleanedRecipe.servingsProduced === null || isNaN(cleanedRecipe.servingsProduced)) {
      cleanedRecipe.servingsProduced = 1;
    }

    const updateDto = toRecipeUpdateDto(cleanedRecipe);

    try {
      const response = await fetch(`/api/MultiPartRecipe/${recipeId}`, {
        method: 'PUT',
        body: JSON.stringify(updateDto),
        headers: { 'Content-Type': 'application/json' },
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ error: 'Failed to save recipe' }));
        setErrorMessage(errorData.error || 'Failed to save recipe');
        setOperationInProgress(false);
        return;
      }

      // Upload pending images and track ID mapping
      const pendingIdToRealId = new Map<string, string>();
      for (const pending of pendingImages) {
        const result = await uploadRecipeImage(recipeId, pending.file);
        if (!result.ok || !result.data) {
          console.error('Failed to upload image:', result.error);
          setErrorMessage(result.error || 'Failed to upload image');
          setOperationInProgress(false);
          return;
        }
        pendingIdToRealId.set(pending.id, result.data.id);
      }

      // Build final order
      const finalOrderIds: string[] = imageOrder
        .map((item) => (item.isPending ? pendingIdToRealId.get(item.id) : item.id))
        .filter((id): id is string => id !== undefined);

      // Save the final order
      if (finalOrderIds.length > 0) {
        await reorderRecipeImages(recipeId, finalOrderIds);
      }

      location.reload();
    } catch (error) {
      console.error('Error saving recipe:', error);
      setErrorMessage(
        error instanceof Error ? error.message : 'An unexpected error occurred'
      );
      setOperationInProgress(false);
    }
  }, [recipe, recipeId, pendingImages, imageOrder]);

  // Delete
  const onDelete = useCallback(() => {
    setShowDeleteConfirm(true);
  }, []);

  const onConfirmDelete = useCallback(async () => {
    setShowDeleteConfirm(false);

    const response = await fetch(`/api/MultiPartRecipe/${recipeId}`, {
      method: 'DELETE',
    });

    if (response.ok) {
      window.location.href = '/';
    } else {
      console.log(await response.json());
    }
  }, [recipeId]);

  // Cancel
  const onCancel = useCallback(() => {
    location.reload();
  }, []);

  // Add to list
  const onAddToList = useCallback(async (listName: string) => {
    await addToList(listName, recipeId, 1);
    if (listName === "Groceries") {
      window.location.href = '/Groceries';
    } else {
      // For custom lists, show a toast notification
      setToastMessage(`Added to ${listName}!`);
    }
  }, [recipeId]);

  const value: RecipeContextValue = {
    // State
    recipe,
    recipeImages,
    pendingImages,
    imageOrder,
    imageOperationInProgress,
    edit,
    units,
    newServings,
    errorMessage,
    operationInProgress,
    nutritionFacts,
    showDeleteConfirm,
    ingredientMatches,
    toastMessage,
    // Actions
    setEdit,
    setShowDeleteConfirm,
    setErrorMessage,
    setNewServings,
    setToastMessage,
    updateRecipe,
    updateComponent,
    appendIngredientToComponent,
    deleteIngredientFromComponent,
    updateIngredientInComponent,
    appendStepToComponent,
    updateStepsInComponent,
    deleteStepFromComponent,
    appendComponent,
    deleteComponent,
    handleAddImages,
    handleRemoveExistingImage,
    handleRemovePendingImage,
    updateImageOrder,
    onSave,
    onDelete,
    onConfirmDelete,
    onCancel,
    onAddToList,
  };

  return <RecipeContext.Provider value={value}>{children}</RecipeContext.Provider>;
}
