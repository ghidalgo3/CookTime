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

type Ingredient = {
    name: string,
    id: string,
    isNew: boolean
}

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

type MultiPartRecipe = {
    id : string,
    name : string,
    duration : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    source: string,
    categories : {name: string, id: string, isNew: boolean}[],
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
    duration : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    source: string,
    ingredients : IngredientRequirement[] | undefined,
    steps : RecipeStep[] | undefined,
    categories : {name: string, id: string, isNew: boolean}[],
    staticImage : string
}
