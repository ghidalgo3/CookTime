type Autosuggestable = {
    name: string,
    id: string,
    isNew: boolean
}

type MeasureUnit = {
   name: string,
   siType : string
   siValue: number
}

type Cart = {
    id: string,
    recipeRequirement: RecipeRequirement[],
    CreateAt: string,
    active : boolean
    ingredientState : CartIngredient[]
}

type RecipeRequirement = {
    recipe : Recipe,
    multiPartRecipe : MultiPartRecipe,
    quantity : number,
    id: string
}

type IngredientRequirement = {
    ingredient : Ingredient,
    unit : string,
    quantity : number,
    id: string
    position : number
}

type Ingredient = Autosuggestable

type CartIngredient = {
    id: string,
    ingredient: Ingredient,
    checked: boolean
}

type RecipeStep = {
    text : string
}

type Image = {
    name : string,
    id : string
}
type Category = Autosuggestable

type MultiPartRecipe = {
    id : string,
    name : string,
    cooktimeMinutes : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    source: string,
    categories : Category[],
    staticImage : string
    recipeComponents: RecipeComponent[],
}

type RecipeComponent = {
    id : string,
    name : string,
    ingredients : IngredientRequirement[] | undefined,
    steps : RecipeStep[] | undefined,
    position : number
}

type Recipe = {
    id : string,
    name : string,
    cooktimeMinutes : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    source: string,
    ingredients : IngredientRequirement[] | undefined,
    steps : RecipeStep[] | undefined,
    categories : {name: string, id: string, isNew: boolean}[],
    staticImage : string
}

type NutritionFactVector = {
    calories : number,
    carbohydrates : number,
    saturatedFats : number,
    transFats : number,
    monoUnsaturatedFats : number,
    polyUnsaturatedFats : number,
    proteins : number,
    sugars : number
}

type RecipeNutritionFacts = {
    recipe : NutritionFactVector,
    components : NutritionFactVector[]
    ingredients: IngredientNutritionDescription[]
}

type IngredientNutritionDescription = {
    nutritionDatabaseId : string,
    nutritionDatabaseDescriptor : string,
    name : string,
    unit : string,
    modifier : string,
    quantity : number,
    caloriesPerServing : number,
}