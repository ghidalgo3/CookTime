import * as React from 'react';
import { Button, Col, Form, FormControl, FormText, Row } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay, IngredientInput } from './IngredientInput';

type RecipeEditProps = {
    recipeId : string
}

type RecipeEditState = {
    recipe : Recipe,
    edit : boolean,
    units: string[],
    newServings: number
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
                categories: [],
                staticImage: ''
            },
            newServings: 1,
        }
    }

    componentDidMount() {
        fetch(`/api/recipe/${recipeId}`)
            .then(response => response.json())
            .then(
                result => {
                    let r = result as Recipe
                    this.setState({
                        recipe: result as Recipe,
                        newServings: r.servingsProduced
                    })
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
                <Col xs={11}>
                    <Button variant="outline-primary" className="width-100" onClick={_ => this.appendNewIngredientRequirementRow()}>New Ingredient</Button>
                </Col>
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
            <Row key={id} className="margin-bottom-8">
                <Col key={`${id}quantity`} xs={2}>
                    <Form.Control
                        type="number"
                        onChange={(e) => this.updateIngredientRequirement(ir, ir => { ir.quantity = parseFloat(e.target.value); return ir; } ) }
                        value={ir.quantity}></Form.Control>
                </Col>
                <Col key={`${id}unit`} xs={3}>
                    <Form.Select
                        onChange={(e) => this.updateIngredientRequirement(ir, ir => {ir.unit = e.currentTarget.value; return ir; })}
                        value={ir.unit}>
                        {
                            unitOptions
                        }
                    </Form.Select>
                </Col>
                <Col key={`${id}name`}>
                    <IngredientInput
                        isNew={ir.ingredient.isNew}
                        query={text => `/api/recipe/ingredients?name=${text}`}
                        ingredient={ir.ingredient}
                        className=""
                        onSelect={(i, isNew) => this.updateIngredientRequirement(ir, ir => {
                            ir.ingredient = i
                            ir.ingredient.isNew = isNew
                            if (isNew) {
                                ir.id = uuidv4()
                            }
                            return ir
                        })}/>
                    {/* <Form.Control
                        type="text"
                        onChange={(e) => this.updateIngredientRequirement(ir, x => { x.ingredient.name = e.target.value; return x;})}
                        value={ir.ingredient.name}
                        placeholder="Ingredient name"></Form.Control> */}
                </Col>
                <Col key={`${id}delete`} xs={1} className="d-flex align-items-center">
                    <i onClick={(_) => this.deleteIngredientRequirement(ir)} className="fas fa-trash-alt"></i>
                </Col>
            </Row>
        )
    }

    deleteIngredientRequirement(ir: IngredientRequirement) {
        this.setState({
            recipe: {
                ...this.state.recipe,
                ingredients: this.state.recipe.ingredients.filter(i => i.id !== ir.id),
            }
        })
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
                <Col xs={11}>
                    <Button variant="outline-primary" className="width-100" onClick={_ => this.appendNewStep()}>New Step</Button>
                </Col>
            </Form>
        )
    }

    stepEditRow(i: RecipeStep, idx: number): any {
        return (
            <Row>
                <Col>
                    <FormControl
                        as="textarea" 
                        rows={4}
                        className="margin-bottom-8"
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
                </Col>
                <Col xs={1}>
                    <i onClick={(_) => this.setState({
                        recipe: {
                            ...this.state.recipe,
                            steps: this.state.recipe.steps.filter((s,i) => i !== idx),
                        }
                    })} className="fas fa-trash-alt"></i>
                </Col>
            </Row>
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
            let newQuantity = ingredient.quantity * this.state.newServings / this.state.recipe.servingsProduced;
            return <Row className="ingredient-item"><IngredientDisplay ingredientRequirement={{...ingredient, quantity: newQuantity}} /></Row>
        });

        let stepComponetns = this.state.recipe.steps.map((step, index) => {
            return (
                <Row>
                    <Col className="step-number">{index + 1}</Col>
                    <Col className="margin-bottom-20" key={step.text}>{step.text}</Col>
                </Row>
            )
        });

        return (
            <div>
                <Row>
                    <Col className="justify-content-md-left" xs={6}>
                        <h1 className="margin-bottom-20">Recipe</h1>
                    </Col>
                    {this.editButtons()}
                </Row>

                {(this.state.recipe.staticImage === null) ?
                    <img className="recipe-image" src={`/placeholder.jpg`} />
                    :
                    <img className="recipe-image" src={`/${this.state.recipe.staticImage}`} />
                }

                <dl className="row">
                    <Row className="padding-right-0">
                        <dt className="col-sm-3">
                            NAME
                        </dt>
                        <dd className="col-sm-9">
                            {this.state.edit ?
                                <Form.Control
                                    type="text"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, name: e.target.value}})}
                                    value={this.state.recipe.name}></Form.Control> :
                                <div>{this.state.recipe.name}</div> }
                        </dd>
                    </Row>
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            CALORIES
                        </dt>
                        <dd className="col-sm-9">
                            {this.state.edit ?
                                <Form.Control
                                    type="number"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, caloriesPerServing: parseInt(e.target.value)}})}
                                    value={this.state.recipe.caloriesPerServing}></Form.Control> :
                                <div>{this.state.recipe.caloriesPerServing}</div> }
                        </dd>
                    </Row>
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            SERVINGS
                        </dt>
                        <dd className="col-sm-9">
                            {this.state.edit ?
                                <Form.Control
                                    type="number"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, servingsProduced: parseInt(e.target.value)}})}
                                    value={this.state.recipe.servingsProduced}></Form.Control> :
                                <div className="serving-counter">
                                    {/* BABE TO DO: MAKE THE COUNTER WORK AND MULTIPLY THE RENDERED INGREDIENT QTYS */}
                                    <i
                                        onClick={(_) => this.setState({newServings: this.state.newServings + 1})}
                                        className="fas fa-plus-circle deep-water-color"></i>
                                    <input className="form-control count" value={this.state.newServings}></input>
                                    <i
                                        onClick={(_) => this.setState({newServings: this.state.newServings - 1})}
                                        className="fas fa-minus-circle deep-water-color"></i>
                                </div> 
                            }
                        </dd>
                    </Row>
                    {this.state.edit ? 
                        <Row className="padding-right-0">
                            <dt className="col-sm-3 detail-header">
                                STATIC IMAGE
                            </dt>
                            <dd className="col-sm-9">
                                <Form.Control
                                    type="text"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, staticImage: e.target.value}})}
                                    value={this.state.recipe.staticImage}></Form.Control>
                            </dd>
                        </Row> 
                        : 
                        null
                    }
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            INGREDIENTS
                        </dt>
                        <dd className="col-sm-9">
                            <div className="ingredient-list">
                            {this.state.edit ?
                                this.ingredientEditGrid() : 
                                ingredientComponents}
                            </div>
                        </dd>
                    </Row>
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            DIRECTIONS
                        </dt>
                        <dd className="col-sm-9">
                            <div className="step-list">
                                {this.state.edit ? 
                                    this.stepEdit() :
                                    stepComponetns}
                            </div>
                        </dd>
                    </Row>
                </dl>
            </div>
        );
    }
    private editButtons(): string | number | boolean | {} | React.ReactElement<any, string | React.JSXElementConstructor<any>> | React.ReactNodeArray | React.ReactPortal | null | undefined {
        return this.state.edit ?
            <Col>
                <Row>
                    <Col>
                        <Button className="width-100" onClick={_ => this.onSave()}>Save</Button>
                    </Col>
                    <Col>
                        <Button variant="danger" className="width-100" onClick={_ => this.onDelete()}>Delete</Button>
                    </Col>
                </Row>
            </Col>
            :
            <Col>
                <Button className="float-end" onClick={(event) => this.setState({ edit: !this.state.edit })}>Edit</Button>
            </Col>;
    }

    onDelete(): void {
        fetch(`/api/Recipe/${this.props.recipeId}`, {
            method: 'DELETE',
        }).then(response => {
            if (response.ok) {
                window.location.href = "/"
            } else {
                console.log(response.json())
            }
        })
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