type IngredientRequirement = {
    ingredient : {name : string, id: string},
    unit : string,
    quantity : number
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
    categories : {name: string, id: string}[]
}
