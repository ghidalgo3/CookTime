import * as React from 'react';
import { Alert, Button, Col, Form, FormControl, FormText, Row, Spinner } from 'react-bootstrap';
import { stringify, v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay, IngredientInput } from './IngredientInput';
import { Step } from './RecipeStep';
import { IngredientRequirementList } from './IngredientRequirementList';
import { RecipeStepList } from './RecipeStepList';

type RecipeEditProps = {
    recipeId : string,
    multipart : boolean,
}

type RecipeEditState = {
    recipe : Recipe | MultiPartRecipe,
    newImage: Blob | undefined,
    newImageSrc : string | undefined,
    recipeImages: Image[],
    edit : boolean,
    units: string[],
    newServings: number,
    error: boolean,
    operationInProgress: boolean,
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
            operationInProgress: false,
        }
    }

    componentDidMount() {
        fetch(`/api/recipe/units`)
            .then(response => response.json())
            .then(
                result => {
                    this.setState({units: result as string[]});
                }
            )
        if (this.props.multipart) {
            fetch(`/api/multipartrecipe/${recipeId}`)
                .then(response => response.json())
                .then(
                    result => {
                        let r = result as MultiPartRecipe
                        r.recipeComponents.sort((a,b) => a.position - b.position);
                        let newServings = this.setServingsFromQueryParameters(r);
                        for (let i = 0; i < r.recipeComponents.length; i++) {
                            const element = r.recipeComponents[i];
                            element.ingredients?.sort((a,b) => a.position - b.position);
                        }
                        this.setState({
                            recipe: r,
                        })
                    }
                )
            fetch(`/api/MultiPartRecipe/${recipeId}/images`)
                .then(response => response.json())
                .then(
                    result => {
                        let r = result as Image[]
                        this.setState({
                            recipeImages: r
                        })
                    }
                )
        } else {
            fetch(`/api/recipe/${recipeId}`)
                .then(response => response.json())
                .then(
                    result => {
                        let r = result as Recipe
                        let newServings = this.setServingsFromQueryParameters(r);
                        r.ingredients?.sort((a,b) => a.position - b.position);
                        this.setState({
                            recipe: r,
                        })
                    }
                )
            fetch(`/api/recipe/${recipeId}/images`)
                .then(response => response.json())
                .then(
                    result => {
                        let r = result as Image[]
                        this.setState({
                            recipeImages: r
                        })
                    }
                )
        }

    }


    private setServingsFromQueryParameters(r: Recipe | MultiPartRecipe) {
        const urlParams = new URLSearchParams(window.location.search);
        const myParam = urlParams.get('servings');
        let newServings = r.servingsProduced;
        if (myParam != null) {
            newServings = parseInt(myParam);
        }
        this.setState({
            newServings: newServings
        });
    }

    appendNewIngredientRequirementRowForComponent(componentIndex : number, component : RecipeComponent): void {
        if (this.props.multipart) {
            var ir : IngredientRequirement = {
                ingredient: {name: '', id: uuidv4(), isNew: false},
                unit: 'Cup',
                quantity: 0,
                id: uuidv4(),
                position: component.ingredients?.length ?? 0
            }
            var newIrs = Array.from(component.ingredients ?? [])
            newIrs.push(ir)
            component.ingredients = newIrs;
            let newComponents = Array.from((this.state.recipe as MultiPartRecipe).recipeComponents)
            this.setState({
                ...this.state,
                recipe: {
                    ...this.state.recipe,
                    recipeComponents: newComponents
                }
            })
        }
    }

    appendNewIngredientRequiremendtRow(): void {
        if (!this.props.multipart) {
            var recipe = (this.state.recipe as Recipe)
            var ir : IngredientRequirement = {
                ingredient: {name: '', id: uuidv4(), isNew: false},
                unit: 'Cup',
                quantity: 0,
                id: uuidv4(),
                position: recipe.ingredients?.length ?? 0
            }
            var newIrs = Array.from(recipe.ingredients ?? [])
            newIrs.push(ir)
            this.setState({
                ...this.state,
                recipe: {
                    ...this.state.recipe,
                    ingredients: newIrs
                }
            })
        }
    }


    deleteIngredientRequirement(ir: IngredientRequirement) {
        if (!this.props.multipart) {
            var recipe = (this.state.recipe as Recipe)
            var newIrs = recipe.ingredients?.filter(i => i.id !== ir.id).map((e, i) => {
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
    }

    deleteIngredientRequirementForComponent(componentIndex : number, component : RecipeComponent, ir: IngredientRequirement) {
        if (this.props.multipart) {
            var newIrs = component.ingredients?.filter(i => i.id !== ir.id).map((e, i) => {
                e.position = i;
                return e;
            })
            // a particular component was updated
            component.ingredients = newIrs;
            let newComponents = Array.from((this.state.recipe as MultiPartRecipe).recipeComponents)
            newComponents[componentIndex] = component;
            this.setState({
                recipe: {
                    ...this.state.recipe,
                    recipeComponents: newComponents
                }
            })
        }
    }

    updateIngredientRequirement(ir: IngredientRequirement, update : (ir : IngredientRequirement) => IngredientRequirement) {
        if (!this.props.multipart) {
            var recipe = (this.state.recipe as Recipe)
            const idx = recipe.ingredients!.findIndex(i => i.ingredient.id == ir.ingredient.id);
            const newIr = update(recipe.ingredients![idx])
            let newIrs = Array.from(recipe.ingredients!)
            newIrs[idx] = newIr
            this.setState({
                ...this.state,
                recipe: {
                    ...recipe,
                    ingredients: newIrs
                }
            })
        }
    }

    updateIngredientRequirementForComponent(
        componentIndex : number,
        component : RecipeComponent,
        ir: IngredientRequirement,
        update : (ir : IngredientRequirement) => IngredientRequirement) {
        if (this.props.multipart) {
            // var recipe = (this.state.recipe as MultiPartRecipe)
            const idx = component.ingredients!.findIndex(i => i.ingredient.id == ir.ingredient.id);
            const newIr = update(component.ingredients![idx])
            let newIrs = Array.from(component.ingredients!)
            newIrs[idx] = newIr
            let newComponents = Array.from((this.state.recipe as MultiPartRecipe).recipeComponents);
            this.setState({
                ...this.state,
                recipe: {
                    ...this.state.recipe,
                    recipeComponents: newComponents
                }
            })
        }
    }

    appendNewStep(): void {
        if (!this.props.multipart) {
            var recipe = (this.state.recipe as Recipe)
            var newSteps = Array.from(recipe.steps ?? [])
            newSteps.push({text: ''})
            this.setState({
                recipe: {
                    ...this.state.recipe,
                    steps : newSteps
                }
            })
        }
    }

    appendNewStepForComponent(componentIndex : number, component : RecipeComponent): void {
        if (this.props.multipart) {
            var newSteps = Array.from(component.steps ?? [])
            newSteps.push({text: ''})
            component.steps = newSteps;
            let newComponents = Array.from((this.state.recipe as MultiPartRecipe).recipeComponents);
            this.setState({
                recipe: {
                    ...this.state.recipe,
                    recipeComponents: newComponents
                }
            })
        }
    }

    deleteStep(idx : number) {
        if (!this.props.multipart) {
            var recipe = (this.state.recipe as Recipe)
            this.setState({
                recipe: {
                    ...this.state.recipe,
                    steps: recipe.steps?.filter((s, i) => i !== idx) ?? [],
                }
            })
        }
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

                <div>
                    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row margin-top-20">
                        <Col className="col-3 recipe-field-title">
                            NAME
                        </Col>
                        <Col className="col d-flex align-items-center">
                            {this.state.edit ?
                                <Form.Control
                                    type="text"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, name: e.target.value}})}
                                    value={this.state.recipe.name}></Form.Control> :
                                <div>{this.state.recipe.name}</div> }
                        </Col>
                    </Row>
                    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                        <Col className="col-3 recipe-field-title">
                            CALORIES PER SERVING
                        </Col>
                        <Col className="col d-flex align-items-center">
                            {this.state.edit ?
                                <Form.Control
                                    type="number"
                                    min="0"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, caloriesPerServing: parseInt(e.target.value)}})}
                                    value={this.state.recipe.caloriesPerServing}></Form.Control> :
                                <div>{this.state.recipe.caloriesPerServing}</div> }
                        </Col>
                    </Row>
                    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                        <Col className="col-3 recipe-field-title">
                            SERVINGS
                        </Col>
                        <Col className="col d-flex align-items-center">
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
                        </Col>
                    </Row>
                {this.state.recipe.source == '' && !this.state.edit ? null :
                    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                        <Col className="col-3 recipe-field-title">
                            LINK TO ORIGINAL RECIPE
                        </Col>
                        <Col className="col d-flex align-items-center">
                            {this.state.edit ?
                                <Form.Control
                                    type="text"
                                    onChange={(e) => this.setState({recipe: {...this.state.recipe, source: e.target.value}})}
                                    value={this.state.recipe.source}></Form.Control> :
                                <div>{this.state.recipe.source}</div> }
                        </Col>
                    </Row>
                }
                    {this.state.edit ? 
                        <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                            <Col className="col-3 recipe-field-title">
                                IMAGE
                            </Col>
                            <Col className="col d-flex align-items-center">
                                <Form.Group controlId="formFile" className="image-selector">
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
                            </Col>
                        </Row> 
                        : 
                        null
                    }
                    {this.RecipeOrComponents()}
                </div>
            </div>
        );
    }

    private RecipeOrComponents() {
        if (this.props.multipart) {
            let editBlockComponents = (this.state.recipe as MultiPartRecipe).recipeComponents?.map((component, componentIndex) => {
                return this.EditBlockComponent(
                    componentIndex,
                    this.state.recipe as MultiPartRecipe,
                    component);
            })
            return (
                <div>
                    {editBlockComponents}
                    {this.state.edit ? 
                        <Form>
                            <Col xs={12}>
                                <Button
                                    variant="outline-primary"
                                    className="width-100"
                                    onClick={(_) => this.appendNewComponent()}>New component</Button>
                            </Col>
                        </Form>
                        : null}
                </div>);
        } else {
            return this.EditBlock(this.state.recipe as Recipe)
        }
    }

    private appendNewComponent() {
        let newComponents = Array.from((this.state.recipe as MultiPartRecipe).recipeComponents);
        newComponents.push({
            id: uuidv4(),
            name: "",
            ingredients: [],
            steps: [],
            position: newComponents.length
        })
        this.setState({
            recipe: { 
                ...this.state.recipe,
                recipeComponents: newComponents
            }
        })
    }

    private deleteComponent(recipe : MultiPartRecipe, componentIndex : number) {
        let newComponents = Array.from(recipe.recipeComponents.filter((c, i) => i !== componentIndex));
        this.setState({
            recipe: {
                ...this.state.recipe,
                recipeComponents: newComponents
            }
        })
    }

    private EditBlockComponent(
        componentIndex : number,
        recipe : MultiPartRecipe,
        component : RecipeComponent) {
        return (
            <div className="card margin-bottom-20 component-card">
                {recipe.recipeComponents.length > 1 ? 
                <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                    <Col className="col-3 recipe-field-title">
                        COMPONENT NAME
                    </Col>
                    <Col className="col d-flex align-items-center">
                        {this.state.edit ?
                            <Form.Control
                                type="text"
                                onChange={(e) => {
                                    let newComponents = Array.from(recipe.recipeComponents);
                                    newComponents[componentIndex].name = e.target.value
                                    this.setState({
                                        recipe: {
                                            ...recipe,
                                            recipeComponents: newComponents
                                        }
                                    })
                                }}
                                value={component.name}></Form.Control> :
                            <div>{component.name}</div> }
                    </Col>
                    <Col className="col-sm-1">
                        <Button className="float-end" variant="danger">
                            <i onClick={(_) => this.deleteComponent(recipe, componentIndex)}
                                className="fas fa-trash-alt"></i>
                        </Button>
                    </Col>
                </Row>
                : null
                }
                <Row className="padding-right-0 recipe-edit-row ">
                    <Col className="col-3 recipe-field-title margin-top-8">
                        INGREDIENTS
                    </Col>
                    <Col className="col d-flex align-items-center">
                        <div className="ingredient-list">
                            <IngredientRequirementList
                                ingredientRequirements={component.ingredients ?? []}
                                onDelete={(ir) => this.deleteIngredientRequirementForComponent(componentIndex, component, ir)}
                                onNewIngredientRequirement={() => this.appendNewIngredientRequirementRowForComponent(componentIndex, component)}
                                updateIngredientRequirement={(ir, u) => this.updateIngredientRequirementForComponent(componentIndex, component, ir, u)}
                                units={this.state.units}
                                edit={this.state.edit}
                                multiplier={this.state.newServings / this.state.recipe.servingsProduced} />
                        </div>
                    </Col>
                </Row>
                <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
                    <Col className="col-3 recipe-field-title">
                        DIRECTIONS
                    </Col>
                    <Col className="col d-flex align-items-center">
                        <div className="step-list">
                            <RecipeStepList
                                multipart={this.props.multipart}
                                recipe={recipe}
                                component={component}
                                newServings={this.state.newServings}
                                edit={this.state.edit}
                                onDelete={(idx) => this.deleteStep(idx)}
                                onChange={(newSteps) => {
                                    this.setState({
                                        ...this.state,
                                        recipe: {
                                            ...recipe,
                                            steps: newSteps
                                        }
                                    })
                                }
                                }
                                onNewStep={() => this.appendNewStepForComponent(componentIndex, component)} />
                        </div>
                    </Col>
                </Row>
            </div>
        )
    }

    private EditBlock(r : Recipe) {
        return (
            <div>
                <Row className="padding-right-0">
                    <dt className="col-sm-3 detail-header">
                        INGREDIENTS
                    </dt>
                    <dd className="col-sm-9">
                        <div className="ingredient-list">
                            <IngredientRequirementList
                                ingredientRequirements={r.ingredients ?? []}
                                onDelete={(ir) => this.deleteIngredientRequirement(ir)}
                                onNewIngredientRequirement={() => this.appendNewIngredientRequiremendtRow()}
                                updateIngredientRequirement={(ir, u) => this.updateIngredientRequirement(ir, u)}
                                units={this.state.units}
                                edit={this.state.edit}
                                multiplier={this.state.newServings / this.state.recipe.servingsProduced} />
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
                                multipart={this.props.multipart}
                                recipe={r}
                                newServings={this.state.newServings}
                                edit={this.state.edit}
                                onDelete={(idx) => this.deleteStep(idx)}
                                onChange={(newSteps) => {
                                    this.setState({
                                        ...this.state,
                                        recipe: {
                                            ...r,
                                            steps: newSteps
                                        }
                                    })
                                }
                                }
                                onNewStep={() => this.appendNewStep()} />
                        </div>
                    </dd>
                </Row>
            </div>)

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
                        <Button className="width-100" onClick={_ => this.onSave()}>
                            {
                                this.state.operationInProgress ?
                                    <Spinner
                                        as="span"
                                        animation="border"
                                        size="sm"
                                        role="status"
                                        aria-hidden="true"
                                        /> 
                                    : "Save"
                            }
                        </Button>
                    </Col>
                    <Col>
                        <Button variant="danger" className="width-100 margin-bottom-15" onClick={_ => this.onDelete()}>Delete</Button>
                    </Col>
                    {!this.props.multipart ?
                        <Col>
                            <Button variant="danger" className="width-100 margin-bottom-15" onClick={_ => this.onMigrate()}>Migrate</Button>
                        </Col>
                        :
                        null}
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
            if (response.redirected) {
                window.location.href = response.url
            }
            // TODO redirect to new page
        })
    }

    onDelete(): void {
        if (!this.props.multipart) {
            fetch(`/api/Recipe/${this.props.recipeId}`, {
                method: 'DELETE',
            }).then(response => {
                if (response.ok) {
                    window.location.href = "/"
                } else {
                    console.log(response.json())
                }
            })
        } else {
            fetch(`/api/MultiPartRecipe/${this.props.recipeId}`, {
                method: 'DELETE',
            }).then(response => {
                if (response.ok) {
                    window.location.href = "/"
                } else {
                    console.log(response.json())
                }
            })
        }
    }

    onSave() {
        this.setState({ operationInProgress: true });
        if (!this.props.multipart) {
            this.saveSimpleRecipe();
        } else {
            this.saveMultiPartRecipe();
        }
    }

    saveMultiPartRecipe() {
        var recipe = this.state.recipe as MultiPartRecipe;
        for (let i = 0; i < recipe.recipeComponents.length; i++) {
            const component = recipe.recipeComponents[i];
            component.steps = Array.from(component.steps ?? []).filter(step => step.text != null && step.text != '');
            component.ingredients = Array.from(component.ingredients ?? []).filter(ingredient => ingredient.ingredient.name != null && ingredient.ingredient.name != '');
        }

        fetch(`/api/MultiPartRecipe/${this.props.recipeId}`, {
            method: 'PUT',
            body: JSON.stringify(recipe),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => {
            if (response.ok) {
                response.json().then(r => {
                    this.setState({
                        ...this.state,
                        recipe: r as MultiPartRecipe,
                        edit: false,
                        operationInProgress: false
                    });
                });
            } else {
                this.setState({ error: true });
            }
        }
        ).then(() => {
            if (this.state.newImage != null) {
                let fd = new FormData();
                fd.append("files", this.state.newImage as Blob);
                fetch(`/api/MultiPartRecipe/${this.props.recipeId}/image`, {
                    method: 'PUT',
                    body: fd
                });
            }
        });
    }

    private saveSimpleRecipe() {
        var recipe = this.state.recipe as Recipe;
        var filteredSteps = Array.from(recipe.steps ?? []).filter(step => step.text != null && step.text != '');
        var filteredIngredients = Array.from(recipe.ingredients ?? []).filter(ingredient => ingredient.ingredient.name != null && ingredient.ingredient.name != '');
        let newState = {
            ...this.state,
            recipe: {
                ...this.state.recipe,
                steps: filteredSteps,
                ingredients: filteredIngredients
            }
        };


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
                        edit: false,
                        operationInProgress: false
                    });
                });
            } else {
                this.setState({ error: true });
            }
        }
        ).then(() => {
            if (this.state.newImage != null) {
                let fd = new FormData();
                fd.append("files", this.state.newImage as Blob);
                fetch(`/api/Recipe/${this.props.recipeId}/image`, {
                    method: 'PUT',
                    body: fd
                });
            }
        });
    }
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id") as string;
var multipart = recipeContainer?.getAttribute("data-multipart") as string === "True";
ReactDOM.render(
    <RecipeEdit recipeId={recipeId} multipart={multipart} />,
    recipeContainer);