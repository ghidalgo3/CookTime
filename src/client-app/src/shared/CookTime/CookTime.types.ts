// export as namespace CookTime;

export type Autosuggestable = {
    name: string,
    id: string,
    isNew: boolean,
    slug?: string
}

export type DietDetail = {
    name: string,
    opinion: string
    details: TodaysTenDetails | any
}

export type TodaysTenDetails = {
    hasFruits: boolean,
    hasVegetables: boolean,
    hasCruciferousVegetables: boolean,
    hasBeans: boolean,
    hasHerbsAndSpices: boolean,
    hasNutsAndSeeds: boolean,
    hasGrains: boolean,
    hasFlaxseeds: boolean,
    hasBerries: boolean,
    hasGreens: boolean,
}

export type Review = {
    id: string,
    createdAt: string,
    owner: Owner,
    rating: number,
    text: string
}

export type MeasureUnit = {
    name: string,
    siType: string
    siValue: number
}

export type Cart = {
    id: string,
    recipeRequirement: RecipeRequirement[],
    CreateAt: string,
    active: boolean
    ingredientState: CartIngredient[],
    dietDetails: DietDetail[]
}

export type RecipeRequirement = {
    recipe: Recipe,
    multiPartRecipe: MultiPartRecipe,
    quantity: number,
    id: string
}

export type IngredientRequirement = {
    ingredient: Ingredient,
    text: string,
    unit: string,
    quantity: number,
    id: string
    position: number
}

export type Ingredient = Autosuggestable & {
    densityKgPerL: number | undefined
}

export type CartIngredient = {
    id: string,
    ingredient: Ingredient,
    checked: boolean
}

export type Image = {
    id: string,
    url: string
}

export type Category = Autosuggestable

export type Owner = {
    userName: string,
    id: string
}

export type MultiPartRecipe = {
    id: string,
    name: string,
    owner: Owner | null,
    cooktimeMinutes: number | undefined,
    caloriesPerServing: number,
    servingsProduced: number,
    source: string,
    categories: Category[],
    staticImage: string
    recipeComponents: RecipeComponent[],
    reviewCount: number,
    averageReviews: number
}

export type RecipeComponent = {
    id: string,
    name: string,
    ingredients: IngredientRequirement[] | undefined,
    steps: string[] | undefined,
    position: number
}

export type Recipe = {
    id: string,
    name: string,
    cooktimeMinutes: number | undefined,
    caloriesPerServing: number,
    servingsProduced: number,
    source: string,
    ingredients: IngredientRequirement[] | undefined,
    steps: string[] | undefined,
    categories: { name: string, id: string, isNew: boolean }[],
    staticImage: string
}

export type NutritionFactVector = {
    calories: number,
    carbohydrates: number,
    saturatedFats: number,
    transFats: number,
    monoUnsaturatedFats: number,
    polyUnsaturatedFats: number,
    proteins: number,
    sugars: number,
    iron: number,
    vitaminD: number,
    calcium: number,
    potassium: number,
}

export type RecipeNutritionFacts = {
    recipe: NutritionFactVector,
    components: NutritionFactVector[]
    ingredients: IngredientNutritionDescription[]
    dietDetails: DietDetail[]
}

export type IngredientNutritionDescription = {
    nutritionDatabaseId: string,
    nutritionDatabaseDescriptor: string,
    name: string,
    unit: string,
    modifier: string,
    quantity: number,
    caloriesPerServing: number,
}

// TODO rename this to RecipeSummary to match server
export type RecipeView = {
    name: string,
    id: string,
    images: Image[],
    categories: string[],
    averageReviews: number,
    reviewCount: number
}

export type RecipeList = {
    id: string,
    name: string,
    description: string | null,
    creationDate: string,
    isPublic: boolean,
    recipeCount: number
}

export type RecipeListItem = {
    recipe: RecipeView,
    quantity: number
}

export type RecipeListWithRecipes = {
    id: string,
    name: string,
    description: string | null,
    creationDate: string,
    isPublic: boolean,
    ownerId: string,
    recipes: RecipeListItem[],
    selectedIngredients: string[]
}

export type AggregatedIngredient = {
    ingredient: Ingredient,
    quantity: number,
    unit: string,
    selected: boolean
}

export type PagedResult<T> = {
    results: T[],
    currentPage: number,
    pageCount: number,
    pageSize: number,
    rowCount: number,
    firstRowOnPage: number,
    lastRowOnPage: number
}

export type IngredientInternalUpdate = {
    ingredientId: string,
    ndbNumber: number,
    ingredientNames: string,
    gtinUpc: string,
    countRegex: string,
    expectedUnitMass: string,
    nutritionDescription: string
}

// Unified ingredient type combining InternalUpdate and ReplacementRequest
export type IngredientUnified = {
    ingredientId: string,
    ingredientNames: string,
    ndbNumber: number,
    gtinUpc: string,
    countRegex: string,
    expectedUnitMass: string,
    nutritionDescription: string,
    usage: number,
    hasNutrition: boolean
}

export type IngredientReplacementRequest = {
    replacedId: string,
    name: string,
    usage: number,
    hasNutrition: boolean,
    keptId: string,
}

// DTOs for create/update operations (match backend RecipeCreateDto/RecipeUpdateDto)
export type IngredientRequirementCreateDto = {
    id: string,
    ingredient: Ingredient,
    quantity: number,
    unit: string | null,
    position: number,
    text: string | null
}

export type ComponentCreateDto = {
    name: string | null,
    position: number,
    steps: string[],
    ingredients: IngredientRequirementCreateDto[]
}

export type RecipeCreateDto = {
    name: string,
    ownerId: string,
    prepMinutes: number | null,
    cookingMinutes: number | null,
    servings: number | null,
    calories: number | null,
    description: string | null,
    source: string | null,
    components: ComponentCreateDto[],
    categoryIds: string[]
}

export type RecipeUpdateDto = RecipeCreateDto & {
    id: string
}

// Conversion function from MultiPartRecipe to RecipeUpdateDto
export function toRecipeUpdateDto(recipe: MultiPartRecipe): RecipeUpdateDto {
    return {
        id: recipe.id,
        name: recipe.name,
        ownerId: recipe.owner?.id ?? '',
        prepMinutes: null,
        cookingMinutes: recipe.cooktimeMinutes ?? null,
        servings: recipe.servingsProduced ?? null,
        calories: recipe.caloriesPerServing ?? null,
        description: null,
        source: recipe.source ?? null,
        components: recipe.recipeComponents.map((component, index) => ({
            name: component.name ?? null,
            position: component.position ?? index,
            steps: (component.steps ?? []).filter(s => s != null && s.trim() !== ''),
            ingredients: (component.ingredients ?? [])
                .filter(i => i.ingredient?.id)
                .map((ing, ingIndex) => ({
                    id: ing.id,
                    ingredient: ing.ingredient,
                    quantity: ing.quantity ?? 0,
                    unit: ing.unit ?? null,
                    position: ing.position ?? ingIndex,
                    text: ing.text ?? null
                }))
        })),
        categoryIds: recipe.categories
            .filter(c => c?.id != null)
            .map(c => c.id)
    };
}

// AI Recipe Generation types
export type RecipeGenerationResult = {
    recipe: RecipeCreateDto;
    ingredientMatches: IngredientMatch[];
}

export type IngredientMatch = {
    originalText: string;
    matchedIngredientId: string | null;
    matchedIngredientName: string | null;
    confidence: number | null;
    candidates: IngredientCandidate[];
}

export type IngredientCandidate = {
    id: string;
    name: string;
    confidence: number;
}