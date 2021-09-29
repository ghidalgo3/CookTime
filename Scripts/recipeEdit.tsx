import * as React from 'react';
import { Button, Col, Form, FormText, Row } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';

type RecipeEditProps = {
    recipeId : string
}

type RecipeEditState = {
    recipe : Recipe,
    edit : boolean
}
class RecipeEdit extends React.Component<RecipeEditProps, RecipeEditState>
{
    constructor(props : RecipeEditProps) {
        super(props);
        this.state = {
            edit: false,
            recipe: {
                id : '',
                name: '',
                duration: 5,
                caloriesPerServing: 100,
                servingsProduced: 2,
                ingredients: [],
                steps: [],
                categories: []
            }
        }
    }

    componentDidMount() {
        fetch(`/api/recipe/${recipeId}`)
            .then(response => response.json())
            .then(
                result => {
                    console.log(result)
                    this.setState({recipe: result as Recipe})
                }
            )
    }

    ingredientEditGrid() {
        return (
            <Form>
                { this.state.recipe.ingredients.map(i => this.ingredientEditRow(i)) }
            </Form>
        )
    }

    ingredientEditRow(ir : IngredientRequirement) {
        // this.updateIngredientQuantity(ir, 10)
        return (
            <Row key={ir.ingredient.id}>
                <Col xs={2}>
                    <Form.Control
                        type="number"
                        onChange={(e) => this.updateIngredientQuantity(ir, parseInt(e.target.value)) }
                        value={ir.quantity}></Form.Control>
                </Col>
                <Col xs={2}>
                    {ir.unit}
                </Col>
                <Col>
                    {ir.ingredient.name}
                </Col>
            </Row>
        )
    }

    updateIngredientQuantity(ir: IngredientRequirement, n : number): void {
        const idx = this.state.recipe.ingredients.findIndex(i => i.ingredient.id == ir.ingredient.id);
        const newIr = { ...this.state.recipe.ingredients[idx], quantity: n}
        let newIrs = Array.from(this.state.recipe.ingredients)
        newIrs[idx] = newIr
        this.setState({
            ...this.state,
            recipe: {
                ...this.state.recipe,
                ingredients: newIrs
            }
        })
    }

    render() {
        let ingredientComponents = this.state.recipe.ingredients.map(ingredient => {
            return (<li key={ingredient.ingredient.name}>{ingredient.ingredient.name} {ingredient.quantity} {ingredient.unit}</li>)
        });

        let stepComponetns = this.state.recipe.steps.map(step => {
            return (<li key={step.text}>{step.text}</li>)
        });

        return (
            <div>
                {/* <i onClick={} className="fas fa-edit"></i> */}
                <dl className="row">
                    <dt className="col-sm-3">
                        Name
                    </dt>
                    <dd className="col-sm-9">
                        {this.state.edit ?
                            <Form.Control
                                type="text"
                                onChange={(e) => this.setState({recipe: {...this.state.recipe, name: e.target.value}})}
                                value={this.state.recipe.name}></Form.Control> :
                            <div>{this.state.recipe.name}</div> }
                    </dd>
                    <dt className="col-sm-3">
                        Calories per Serving
                    </dt>
                    <dd className="col-sm-9">
                        {this.state.edit ?
                            <Form.Control
                                type="number"
                                onChange={(e) => this.setState({recipe: {...this.state.recipe, caloriesPerServing: parseInt(e.target.value)}})}
                                value={this.state.recipe.caloriesPerServing}></Form.Control> :
                            <div>{this.state.recipe.caloriesPerServing}</div> }
                    </dd>
                    <dt className="col-sm-3">
                        Servings Produced
                    </dt>
                    <dd className="col-sm-9">
                        {this.state.edit ?
                            <Form.Control
                                type="number"
                                onChange={(e) => this.setState({recipe: {...this.state.recipe, servingsProduced: parseInt(e.target.value)}})}
                                value={this.state.recipe.servingsProduced}></Form.Control> :
                            <div>{this.state.recipe.servingsProduced}</div> }
                    </dd>
                    <dt className="col-sm-3">
                        Ingredients
                    </dt>
                    <dd className="col-sm-9">
                        <ul>
                        {this.state.edit ?
                            this.ingredientEditGrid() : 
                            ingredientComponents}
                        </ul>
                    </dd>
                    <dt className="col-sm-3">
                        Directions
                    </dt>
                    <dd className="col-sm-9">
                        <ul>
                            {stepComponetns}
                        </ul>
                    </dd>
                </dl>
                {this.state.edit ?
                    <Button onClick={_ => this.onSave()}>Save</Button> : 
                    <Button onClick={(event) => this.setState({edit: !this.state.edit})}>Edit</Button>}
            </div>
        );
    }

    onSave() {
        fetch(`/api/Recipe/${this.props.recipeId}`, {
            method: 'PUT',
            body: JSON.stringify(this.state.recipe),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => this.setState({edit: !this.state.edit}))
    }
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id") as string;
ReactDOM.render(
    <RecipeEdit recipeId={recipeId} />,
    recipeContainer);