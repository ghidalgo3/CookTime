import * as React from 'react';
import { Button, Col, Form, FormControl, FormText, Row } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientInput } from './IngredientInput';

type RecipeEditProps = {
    recipeId : string
}

type RecipeEditState = {
    recipe : Recipe,
    edit : boolean,
    units: string[]
}
class RecipeEdit extends React.Component<RecipeEditProps, RecipeEditState>
{
    constructor(props : RecipeEditProps) {
        super(props);
        this.state = {
            edit: false,
            units: [],
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
                    this.setState({recipe: result as Recipe})
                }
            )
        fetch(`/api/recipe/units`)
            .then(response => response.json())
            .then(
                result => {
                    this.setState({units: result as string[]});
                }
            )
    }

    ingredientEditGrid() {
        return (
            <Form>
                { this.state.recipe.ingredients.map((i, idx) => this.ingredientEditRow(i, idx)) }
                <Button onClick={_ => this.appendNewIngredientRequirementRow()}>New Ingredient</Button>
            </Form>
        )
    }

    appendNewIngredientRequirementRow(): void {
        var ir : IngredientRequirement = {
            ingredient: {name: '', id: uuidv4(), isNew: false},
            unit: 'Cup',
            quantity: 0,
            id: uuidv4()
        }
        var newIrs = Array.from(this.state.recipe.ingredients)
        newIrs.push(ir)
        this.setState({
            ...this.state,
            recipe: {
                ...this.state.recipe,
                ingredients: newIrs
            }
        })
    }

    ingredientEditRow(ir : IngredientRequirement, idx : number) {
        // this.updateIngredientQuantity(ir, 10)
        var id = ir.ingredient.id
        if (ir.ingredient.id === '' || ir.ingredient.id === '00000000-0000-0000-0000-000000000000') {
            id = idx.toString()
        }
        var unitOptions = this.state.units.map(unit => {
            return <option key={unit} value={unit}>{unit}</option>
        })
        return (
            <Row key={id}>
                <Col key={`${id}quantity`} xs={2}>
                    <Form.Control
                        type="number"
                        onChange={(e) => this.updateIngredientRequirement(ir, ir => { ir.quantity = parseInt(e.target.value); return ir; } ) }
                        value={ir.quantity}></Form.Control>
                </Col>
                <Col key={`${id}unit`} xs={2}>
                    <Form.Select
                        onChange={(e) => this.updateIngredientRequirement(ir, ir => {ir.unit = e.currentTarget.value; return ir; })}
                        value={ir.unit}>
                        {
                            unitOptions
                        }
                    </Form.Select>
                </Col>
                <Col key={`${id}name`} >
                    <IngredientInput
                        isNew={ir.ingredient.isNew}
                        query={text => `/api/recipe/ingredients?name=${text}`}
                        ingredient={ir.ingredient}
                        onSelect={(i, isNew) => this.updateIngredientRequirement(ir, ir => { ir.ingredient = i; ir.ingredient.isNew = isNew; return ir; })}/>
                    {/* <Form.Control
                        type="text"
                        onChange={(e) => this.updateIngredientRequirement(ir, x => { x.ingredient.name = e.target.value; return x;})}
                        value={ir.ingredient.name}
                        placeholder="Ingredient name"></Form.Control> */}
                </Col>
            </Row>
        )
    }

    updateIngredientRequirement(ir: IngredientRequirement, update : (ir : IngredientRequirement) => IngredientRequirement) {
        const idx = this.state.recipe.ingredients.findIndex(i => i.ingredient.id == ir.ingredient.id);
        const newIr = update(this.state.recipe.ingredients[idx])
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

    stepEdit(): React.ReactNode {
        return (
            <Form>
                { this.state.recipe.steps.map((i, idx) => this.stepEditRow(i, idx)) }
                <Button onClick={_ => this.appendNewStep()}>New Step</Button>
            </Form>
        )
    }

    stepEditRow(i: RecipeStep, idx: number): any {
        return (
            <FormControl
                key={idx}
                type="text"
                placeholder="Recipe step"
                value={i.text}
                onChange={e => {
                    let newSteps = Array.from(this.state.recipe.steps);
                    newSteps[idx].text = e.target.value;
                    this.setState({
                        ...this.state,
                        recipe: {
                            ...this.state.recipe,
                            steps: newSteps
                        }
                    })}
                }>
            </FormControl>
        )
    }

    appendNewStep(): void {
        var newSteps = Array.from(this.state.recipe.steps)
        newSteps.push({text: ''})
        this.setState({
            recipe: {
                ...this.state.recipe,
                steps : newSteps
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
                            {this.state.edit ? 
                                this.stepEdit() :
                                stepComponetns}
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