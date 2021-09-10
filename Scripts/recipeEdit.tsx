import * as React from 'react';
import { Button, Form } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';

type RecipeEditProps = {
    recipeId : string
}

type IngredientRequirement = {
    ingredient : string,
    unit : string,
    quantity : number
}

type RecipeStep = {
    text : string
}

type RecipeEditState = {
    id : string,
    name : string,
    duration : number | undefined,
    caloriesPerServing : number,
    servings : number,
    ingredients : IngredientRequirement[],
    steps : RecipeStep[],
    categories : string[]
}


class RecipeEdit extends React.Component<RecipeEditProps, RecipeEditState>
{
    constructor(props : RecipeEditProps)
    {
        super(props);
        this.state = {
            id : '',
            name: '',
            duration: 5,
            caloriesPerServing: 100,
            servings: 2,
            ingredients: [],
            steps: [],
            categories: []
        }
    }
    componentDidMount() {
        fetch(`/api/recipe/${recipeId}`)
            .then(response => response.json())
            .then(
                result => {
                    console.log(result)
                    this.setState(result as RecipeEditState)
                }
            )
    }

    render() {
        let ingredientComponents = this.state.ingredients.map(ingredient => {
            return (<li key={ingredient.ingredient}>{ingredient.ingredient} {ingredient.quantity} {ingredient.unit}</li>)
        })
        let ingredientList = (
            <ul>{ingredientComponents}</ul>
        )
        return (
            <Form>
                <Form.Group
                    className="mb-3"
                    controlId="formBasicEmail">
                    <Form.Label>Name</Form.Label>
                    <Form.Control
                        type="textarea"
                        placeholder="Recipe name"
                        value={this.state.name} />
                </Form.Group>
                <Button variant="primary" type="submit">
                    Save
                </Button>
            </Form>
        )
    }
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id") as string;
ReactDOM.render(
    <RecipeEdit recipeId={recipeId} />,
    recipeContainer);