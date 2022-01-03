type Tag = {
    id: string,
    name: string
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

type Recipe = {
    id : string,
    name : string,
    duration : number | undefined,
    caloriesPerServing : number,
    servingsProduced : number,
    ingredients : IngredientRequirement[],
    steps : RecipeStep[],
    categories : {name: string, id: string, isNew: boolean}[],
    staticImage : string
    tags: Tag[]
}
