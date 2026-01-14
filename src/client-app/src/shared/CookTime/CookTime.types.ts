// export as namespace CookTime;

export type Autosuggestable = {
    name: string,
    id: string,
    isNew: boolean
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
    name: string,
    id: string
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
    steps: RecipeStep[] | undefined,
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
    reviewCount: number,
    isFavorite: boolean | undefined
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

export type IngredientReplacementRequest = {
    replacedId: string,
    name: string,
    usage: number,
    keptId: string,
}