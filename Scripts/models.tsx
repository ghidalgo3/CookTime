type Cart = {
    id: string,
    recipeRequirement: RecipeRequirement[],
    CreateAt: string,
    active : boolean
}

type RecipeRequirement = {
    recipe : Recipe,
    quantity : number,
    id: string
}

type IngredientRequirement = {
    ingredient : Ingredient,
    unit : string,
    quantity : number,
    id: string
}

type Ingredient = {
    name: string,
    id: string,
    isNew: boolean
}

type RecipeStep = {
    text : string
}

type Recipe = {
    id : string,
    name : string,
    duration : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    ingredients : IngredientRequirement[],
    steps : RecipeStep[],
    categories : {name: string, id: string, isNew: boolean}[]
}
