import * as React from 'react';
import { Alert, Button, Col, Form, FormControl, FormText, Row } from 'react-bootstrap';
import { stringify, v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay, IngredientInput } from './IngredientInput';
import { Step } from './RecipeStep';
import { IngredientRequirementList } from './IngredientRequirementList';
import { RecipeStepList } from './RecipeStepList';

type RecipeEditProps = {
    recipeId : string
}

type RecipeEditState = {
    recipe : Recipe,
    newImage: Blob | undefined,
    newImageSrc : string | undefined,
    recipeImages: Image[],
    edit : boolean,
    units: string[],
    newServings: number,
    error: boolean
}

class RecipeEdit extends React.Component<RecipeEditProps, RecipeEditState>
{
    constructor(props : RecipeEditProps) {
        super(props);
        this.state = {
            error: false,
            edit: false,
            units: [],
            newImage: undefined,
            newImageSrc: undefined,
            recipeImages: [],
            recipe: {
                id : '',
                name: '',
                source: '',
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
                    const urlParams = new URLSearchParams(window.location.search);
                    const myParam =  urlParams.get('servings');
                    let newServings = r.servingsProduced;
                    if (myParam != null) {
                        newServings = parseInt(myParam);
                    }
                    r.ingredients?.sort((a,b) => a.position - b.position);
                    this.setState({
                        recipe: r,
                        newServings: newServings
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
        fetch(`/api/recipe/${recipeId}/images`)
                .then(response => response.json())
                .then(
                    result => {
                        let r = result as Image[]
                        this.setState({
                            recipeImages : r
                        })
                    }
                )
    }


    appendNewIngredientRequirementRow(): void {
        var ir : IngredientRequirement = {
            ingredient: {name: '', id: uuidv4(), isNew: false},
            unit: 'Cup',
            quantity: 0,
            id: uuidv4(),
            position: this.state.recipe.ingredients?.length ?? 0
        }
        var newIrs = Array.from(this.state.recipe.ingredients ?? [])
        newIrs.push(ir)
        this.setState({
            ...this.state,
            recipe: {
                ...this.state.recipe,
                ingredients: newIrs
            }
        })
    }


    deleteIngredientRequirement(ir: IngredientRequirement) {
        var newIrs = this.state.recipe.ingredients?.filter(i => i.id !== ir.id).map((e, i) => {
            e.position = i;
            return e;
        })

        this.setState({
            recipe: {
                ...this.state.recipe,
                ingredients: newIrs,
            }
        })
    }

    updateIngredientRequirement(ir: IngredientRequirement, update : (ir : IngredientRequirement) => IngredientRequirement) {
        const idx = this.state.recipe.ingredients!.findIndex(i => i.ingredient.id == ir.ingredient.id);
        const newIr = update(this.state.recipe.ingredients![idx])
        let newIrs = Array.from(this.state.recipe.ingredients!)
        newIrs[idx] = newIr
        this.setState({
            ...this.state,
            recipe: {
                ...this.state.recipe,
                ingredients: newIrs
            }
        })
    }



    appendNewStep(): void {
        var newSteps = Array.from(this.state.recipe.steps ?? [])
        newSteps.push({text: ''})
        this.setState({
            recipe: {
                ...this.state.recipe,
                steps : newSteps
            }
        })
    }
    deleteStep(idx : number) {
        this.setState({
            recipe: {
                ...this.state.recipe,
                steps: this.state.recipe.steps?.filter((s, i) => i !== idx) ?? [],
            }
        })
    }

    render() {

        return (
            <div>
                <Row>
                    <Col className="justify-content-md-left" xs={6}>
                        <h1 className="margin-bottom-20">Recipe</h1>
                    </Col>
                    {this.editButtons()}
                </Row>
                {/* { this.state.error ? 
                    <Alert variant="danger" onClose={() => this.setState({error: false})} dismissible>
                        <Alert.Heading>Could not save recipe.</Alert.Heading>
                        <p>
                        </p>
                    </Alert>
                :
                null} */}
                { this.image() }

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
                            CALORIES PER SERVING
                        </dt>
                        <dd className="col-sm-9">
                            {this.state.edit ?
                                <Form.Control
                                    type="number"
                                    min="0"
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
                                    min="1"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, servingsProduced: parseInt(e.target.value)}})}
                                    value={this.state.recipe.servingsProduced}></Form.Control> :
                                <div className="serving-counter">
                                    <i
                                        onClick={(_) => {
                                            if (this.state.newServings > 1) {
                                                this.setState({newServings: this.state.newServings - 1})
                                            }
                                        }}
                                        className="fas fa-minus-circle red-dirt-color"></i>
                                    <input className="form-control count" value={this.state.newServings}></input>
                                    <i
                                        onClick={(_) => this.setState({newServings: this.state.newServings + 1})}
                                        className="fas fa-plus-circle green-earth-color"></i>
                                    
                                </div> 
                            }
                        </dd>
                    </Row>
                {this.state.recipe.source == '' && !this.state.edit ? null :
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            LINK TO ORIGINAL RECIPE
                        </dt>
                        <dd className="col-sm-9">
                            {this.state.edit ?
                                <Form.Control
                                    type="text"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, source: e.target.value}})}
                                    value={this.state.recipe.source}></Form.Control> :
                                <div>{this.state.recipe.source}</div> }
                        </dd>
                    </Row>
                }
                    {this.state.edit ? 
                        <Row className="padding-right-0">
                            <dt className="col-sm-3 detail-header">
                                IMAGE
                            </dt>
                            <dd className="col-sm-9">
                                <Form.Group controlId="formFile" className="mb-3">
                                    <Form.Control
                                        type="file"
                                        accept=".jpg,.jpeg,.png"
                                        multiple={false}
                                        onChange={e =>
                                        {
                                            let fileList = (e.target as HTMLInputElement).files! as FileList;
                                            if (fileList.length != 1)
                                            {
                                                return;
                                            }

                                            let reader = new FileReader();
                                            reader.readAsDataURL(fileList.item(0)!);
                                            reader.onload = (v) => {
                                                this.setState({
                                                    newImage: fileList.item(0)!,
                                                    newImageSrc: reader.result as string
                                                });
                                            }
                                        }}/>
                                </Form.Group>
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
                            <IngredientRequirementList
                                ingredientRequirements={this.state.recipe.ingredients ?? []}
                                onDelete={(ir) => this.deleteIngredientRequirement(ir)}
                                onNewIngredientRequirement={() => this.appendNewIngredientRequirementRow()}
                                updateIngredientRequirement={(ir, u) => this.updateIngredientRequirement(ir, u)}
                                units={this.state.units}
                                edit={this.state.edit}
                                multiplier={this.state.newServings / this.state.recipe.servingsProduced}/>
                            </div>
                        </dd>
                    </Row>
                    <Row className="padding-right-0">
                        <dt className="col-sm-3 detail-header">
                            DIRECTIONS
                        </dt>
                        <dd className="col-sm-9">
                            <div className="step-list">
                                <RecipeStepList
                                 recipe={this.state.recipe}
                                 newServings={this.state.newServings}
                                 edit={this.state.edit}
                                 onDelete={(idx) => this.deleteStep(idx)}
                                 onChange={(newSteps) => {
                                    this.setState({
                                        ...this.state,
                                        recipe: {
                                            ...this.state.recipe,
                                            steps: newSteps
                                        }
                                    })}
                                 }
                                 onNewStep={() => this.appendNewStep()}/>
                            </div>
                        </dd>
                    </Row>
                </dl>
            </div>
        );
    }
    private image() {
        if (this.state.newImageSrc != null && this.state.newImageSrc != '') {
            return <img className="recipe-image" src={this.state.newImageSrc} />
        } else if (this.state.recipeImages.length > 0) {
            return <img
                className="recipe-image"
                src={`/image/${this.state.recipeImages[0].id}`} />
        } else {
            return (this.state.recipe.staticImage === null) ?
                <img className="recipe-image" src={`/placeholder.jpg`} />
                :
                <img className="recipe-image" src={`/${this.state.recipe.staticImage}`} />;
        }
    }

    private editButtons(): string | number | boolean | {} | React.ReactElement<any, string | React.JSXElementConstructor<any>> | React.ReactNodeArray | React.ReactPortal | null | undefined {
        return this.state.edit ?
            <Col>
                <Row>
                    <Col>
                        <Button className="width-100" onClick={_ => this.onSave()}>Save</Button>
                    </Col>
                    <Col>
                        <Button variant="danger" className="width-100 margin-bottom-15" onClick={_ => this.onDelete()}>Delete</Button>
                    </Col>
                    <Col>
                        <Button variant="danger" className="width-100 margin-bottom-15" onClick={_ => this.onMigrate()}>Migrate</Button>
                    </Col>
                </Row>
            </Col>
            :
            <Col>
                <Button className="float-end" onClick={(event) => this.setState({ edit: !this.state.edit })}>Edit</Button>
            </Col>;
    }
    onMigrate(): void {
        fetch(`/api/Recipe/${this.props.recipeId}/migrate`, {
            method: 'POST',
        }).then(response => {
            console.log(response.json())
        })
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
        var filteredSteps = Array.from(this.state.recipe.steps ?? []).filter(step => step.text != null && step.text != '');
        var filteredIngredients = Array.from(this.state.recipe.ingredients ?? []).filter(ingredient => ingredient.ingredient.name != null && ingredient.ingredient.name != '');
        let newState = {
            ...this.state,
            recipe: {
                ...this.state.recipe,
                steps: filteredSteps,
                ingredients: filteredIngredients
            }};


        fetch(`/api/Recipe/${this.props.recipeId}`, {
            method: 'PUT',
            body: JSON.stringify(newState.recipe),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => {
            if (response.ok) {
                response.json().then(r => {
                    this.setState({
                        ...this.state,
                        recipe: r as Recipe,
                        edit: false
                    })
                })
            } else {
                this.setState({error: true})
            }
        }
        ).then(() =>  {
            if (this.state.newImage != null) {
                let fd = new FormData();
                fd.append("files", this.state.newImage as Blob);
                fetch(`/api/Recipe/${this.props.recipeId}/image`, {
                    method: 'PUT',
                    body: fd
                })
            }
        })
    }
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id") as string;
ReactDOM.render(
    <RecipeEdit recipeId={recipeId} />,
    recipeContainer);