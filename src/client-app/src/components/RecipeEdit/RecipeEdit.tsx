/* eslint-disable no-restricted-globals */
import * as React from 'react';
import { Alert, Button, Col, Form, Row, Spinner, Stack } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientRequirementList } from './IngredientRequirementList';
import { Rating } from "@smastrom/react-rating";
import { MeasureUnit, MultiPartRecipe, Image, RecipeNutritionFacts, Recipe, IngredientRequirement, RecipeComponent, toRecipeUpdateDto } from 'src/shared/CookTime';
import { RecipeStructuredData } from '../RecipeStructuredData';
import { RecipeStepList } from './RecipeStepList';
import { NutritionFacts } from '../NutritionFacts';
import { RecipeReviewForm } from './RecipeReviewForm';
import { RecipeReviews } from './RecipeReviews';
import { TodaysTenDisplay } from '../todaysTenDisplay';
import { Tags } from '../Tags/Tags';
import { AuthContext, AuthenticationContext } from '../Authentication/AuthenticationContext';
import { UserDetails } from 'src/shared/AuthenticationProvider';
import { GoogleInFeedAds } from '../GoogleInFeedAds';
import { RecipeEditButtons } from './RecipeEditButtons';

type RecipeEditProps = {
  recipeId: string,
  multipart: boolean,
}

type RecipeEditState = {
  recipe: MultiPartRecipe,
  newImage: Blob | undefined,
  newImageSrc: string | undefined,
  recipeImages: Image[],
  edit: boolean,
  units: MeasureUnit[],
  newServings: number,
  error: boolean,
  operationInProgress: boolean,
  nutritionFacts: RecipeNutritionFacts | undefined
}

export class RecipeEdit extends React.Component<RecipeEditProps, RecipeEditState> {
  constructor(props: RecipeEditProps) {
    super(props);
    this.state = {
      error: false,
      edit: false,
      units: [],
      newImage: undefined,
      newImageSrc: undefined,
      nutritionFacts: undefined,
      recipeImages: [],
      recipe: {
        id: '',
        name: '',
        source: '',
        cooktimeMinutes: 5,
        caloriesPerServing: 100,
        servingsProduced: 2,
        categories: [],
        staticImage: '',
        owner: null,
        recipeComponents: [],
        reviewCount: 0,
        averageReviews: 4.0
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
          this.setState({ units: result as MeasureUnit[] });
        }
      )
    if (this.props.multipart) {
      fetch(`/api/multipartrecipe/${this.props.recipeId}`)
        .then(response => response.json())
        .then(
          result => {
            let r = result as MultiPartRecipe
            document.title = r.name + " - CookTime"
            r.recipeComponents.sort((a, b) => a.position - b.position);
            let newServings = this.setServingsFromQueryParameters(r);
            for (let i = 0; i < r.recipeComponents.length; i++) {
              const element = r.recipeComponents[i];
              element.ingredients?.sort((a, b) => a.position - b.position);
            }
            this.setState({
              recipe: r,
            })
          }
        )
      fetch(`/api/MultiPartRecipe/${this.props.recipeId}/images`)
        .then(response => response.json())
        .then(
          result => {
            let r = result as Image[]
            this.setState({
              recipeImages: r
            })
          }
        )
      this.getNutritionData();
    } else {
      fetch(`/api/recipe/${this.props.recipeId}`)
        .then(response => response.json())
        .then(
          result => {
            let r = result as MultiPartRecipe
            let newServings = this.setServingsFromQueryParameters(r);
            this.setState({
              recipe: r,
            })
          }
        )
      fetch(`/api/recipe/${this.props.recipeId}/images`)
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


  private getNutritionData() {
    fetch(`/api/MultiPartRecipe/${this.props.recipeId}/nutritionData`)
      .then(response => response.json())
      .then(
        result => {
          let r = result as RecipeNutritionFacts;
          this.setState({
            nutritionFacts: r
          });
        }
      );
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

  appendNewIngredientRequirementRowForComponent(componentIndex: number, component: RecipeComponent): void {
    if (this.props.multipart) {
      var ir: IngredientRequirement = {
        ingredient: { name: '', id: uuidv4(), isNew: false, densityKgPerL: 1 },
        unit: 'Count',
        quantity: 0,
        id: uuidv4(),
        text: "",
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

  deleteIngredientRequirementForComponent(componentIndex: number, component: RecipeComponent, ir: IngredientRequirement) {
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

  updateIngredientRequirementForComponent(
    componentIndex: number,
    component: RecipeComponent,
    ir: IngredientRequirement,
    update: (ir: IngredientRequirement) => IngredientRequirement) {
    if (this.props.multipart) {
      // var recipe = (this.state.recipe as MultiPartRecipe)
      const idx = component.ingredients!.findIndex(i => i.ingredient.id === ir.ingredient.id);
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

  appendNewStepForComponent(componentIndex: number, component: RecipeComponent): void {
    if (this.props.multipart) {
      var newSteps = Array.from(component.steps ?? [])
      newSteps.push('')
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

  changeOrReorder(componentIndex: number, component: RecipeComponent, newSteps: string[]): void {
    if (this.props.multipart) {
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

  deleteStep(idx: number, component?: RecipeComponent) {
    if (!this.props.multipart) {
      // let recipe = (this.state.recipe as Recipe)
      // this.setState({
      //     recipe: {
      //         ...this.state.recipe,
      //         steps: recipe.steps?.filter((s, i) => i !== idx) ?? [],
      //     }
      // })
    } else {
      let mpRecipe = (this.state.recipe as MultiPartRecipe)
      let newComponents = Array.from(mpRecipe.recipeComponents);
      let modifiedIndex = newComponents.findIndex(c => c.id === component!.id);
      let newSteps = component?.steps?.filter((_, i) => i !== idx)
      newComponents[modifiedIndex].steps = newSteps
      this.setState({
        recipe: {
          ...mpRecipe,
          recipeComponents: newComponents,
        }
      })
    }
  }

  render() {
    return (
      <div>
        <RecipeStructuredData recipe={this.state.recipe} images={this.state.recipeImages} />
        <Row>
          <>
            <Col className="justify-content-md-left" xs={6}>
              {this.state.edit ?
                <div className="recipe-name-input">
                  <Form.Control
                    type="text"
                    onChange={(e) => this.setState({ recipe: { ...this.state.recipe, name: e.target.value } })}
                    value={this.state.recipe.name}></Form.Control>
                </div> :
                <h1>{this.state.recipe.name}</h1>}
              {this.state.recipe.reviewCount > 0 ?
                <Stack direction="horizontal" className="margin-bottom-8">
                  <Rating
                    style={{ maxWidth: 150 }}
                    value={this.state.recipe.averageReviews}
                    readOnly />{" "}({this.state.recipe.reviewCount})
                </Stack>
                :
                null
              }
              By {this.state.recipe.owner?.userName}
            </Col>
            <AuthContext.Consumer>
              {({ user }) =>
                <RecipeEditButtons
                  user={user}
                  recipe={this.state.recipe}
                  edit={this.state.edit}
                  operationInProgress={this.state.operationInProgress}
                  onSave={() => this.onSave()}
                  onCancel={() => this.onCancel()}
                  onDelete={() => this.onDelete()}
                  onToggleEdit={() => this.setState({ edit: !this.state.edit })}
                  onAddtoCard={() => this.onAddtoCard()}
                />}
            </AuthContext.Consumer>
          </>
        </Row>

        {this.state.error ?
          <Alert variant="danger" onClose={() => this.setState({ error: false })} dismissible>
            <Alert.Heading>Could not save recipe.</Alert.Heading>
            <p>
            </p>
          </Alert>
          :
          null}
        {this.image()}
        {
          this.todaysTen()
        }
        <div>
          {this.caloriesPerServingComponent()}
          <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
            <Col className="col-3 recipe-field-title">
              Servings
            </Col>
            <Col className="col d-flex align-items-center">
              {this.state.edit ?
                <Form.Control
                  type="number"
                  min="1"
                  onChange={(e) => this.setState({ recipe: { ...this.state.recipe, servingsProduced: parseInt(e.target.value) } })}
                  value={this.state.recipe.servingsProduced}></Form.Control> :
                <div className='serving-counter'>
                  <Button
                    variant="danger"
                    className="minus-counter-button"
                    onClick={(_) => {
                      if (this.state.newServings > 0) {
                        this.setState({ newServings: this.state.newServings - 1 })
                      }
                    }}>
                    <i className="bi bi-dash"></i>
                  </Button>
                  <Form.Control
                    onChange={(e) => {
                      if (e.target.value === '') {
                        this.setState({ newServings: 0 })
                      }
                      let newValue = parseFloat(e.target.value)
                      if (!Number.isNaN(newValue) && newValue > 0) {
                        this.setState({ newServings: newValue })
                      }
                    }}
                    className="form-control count"
                    value={this.state.newServings} />
                  <Button
                    variant="success"
                    className="plus-counter-button"
                    onClick={(_) => this.setState({ newServings: this.state.newServings + 1 })}>
                    <i className="bi bi-plus"></i>
                  </Button>
                </div>
              }
            </Col>
          </Row>

          {!this.state.edit && (this.state.recipe.categories.length === 0) ? null :
            <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
              <Col className="col-3 recipe-field-title">
                Categories
              </Col>
              <Col className="col d-flex align-items-center">
                {this.tagsComponent()}
              </Col>
            </Row>
          }

          {!this.state.edit && ((this.state.recipe.cooktimeMinutes == 0) || (this.state.recipe.cooktimeMinutes == null)) ? null :
            <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
              <Col className="col-3 recipe-field-title">
                Cook Time (Minutes)
              </Col>
              <Col className="col d-flex align-items-center">
                {this.state.edit ?
                  <Form.Control
                    type="number"
                    onChange={(e) => this.setState({ ...this.state, recipe: { ...this.state.recipe, cooktimeMinutes: parseInt(e.target.value) } })}
                    value={this.state.recipe.cooktimeMinutes}></Form.Control> :
                  <div>{this.state.recipe.cooktimeMinutes}</div>
                }
              </Col>
            </Row>
          }

          {(this.state.recipe.source == '' || this.state.recipe.source == null) && !this.state.edit ? null :
            <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
              <Col className="col-3 recipe-field-title">
                Link to Original Recipe
              </Col>
              <Col className="col d-flex align-items-center">
                {this.state.edit ?
                  <Form.Control
                    type="text"
                    onChange={(e) => this.setState({ recipe: { ...this.state.recipe, source: e.target.value } })}
                    value={this.state.recipe.source}></Form.Control> :
                  <a href={(this.state.recipe.source)}>{this.state.recipe.source}</a>}
              </Col>
            </Row>
          }
          {this.state.edit ?
            <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
              <Col className="col-3 recipe-field-title">
                Image
              </Col>
              <Col className="col d-flex align-items-center">
                <Form.Group controlId="formFile" className="image-selector">
                  <Form.Control
                    type="file"
                    accept=".jpg,.jpeg,.png"
                    multiple={false}
                    onChange={e => {
                      let fileList = (e.target as HTMLInputElement).files! as FileList;
                      if (fileList.length != 1) {
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
                    }} />
                </Form.Group>
              </Col>
            </Row>
            :
            null
          }
          {this.RecipeOrComponents()}
        </div>
        {
          !this.state.edit ?
            <div className="border-top-1 margin-top-10">
              <Row>
                <Col xs={3} className="nft-row">
                  {this.nutritionFacts()}
                </Col>
                <Col>
                  {this.ingredientNutritionFacts()}
                </Col>
              </Row>
            </div>
            :
            null
        }
        {
          <AuthContext.Consumer>
            {({ user }) => {
              return user && !this.state.edit &&
                <div className="border-top-1 margin-top-30">
                  <Row>
                    <Col>
                      <RecipeReviewForm recipe={this.state.recipe} />
                    </Col>
                  </Row>
                </div>
            }}
          </AuthContext.Consumer>
        }
        {
          !this.state.edit ?
            <div className="margin-top-10">
              <Row>
                <Col>
                  <RecipeReviews recipeId={this.props.recipeId} />
                </Col>
              </Row>
            </div>
            :
            null
        }
      </div>
    );
  }

  private todaysTen() {
    let todaysTen = this.state.nutritionFacts?.dietDetails.find(dd => dd.name === "TodaysTen")!
    if (!this.state.edit && todaysTen != null) {
      return <TodaysTenDisplay todaysTen={todaysTen} />
    } else {
      return null;
    }
  }

  tagsComponent() {
    if (this.state.edit) {
      return (
        <Tags
          queryBuilder={value => `/api/recipe/tags?query=${value}`}
          initialTags={this.state.recipe.categories}
          tagsChanged={(newTags) => {
            this.setState({
              ...this.state,
              recipe: {
                ...this.state.recipe,
                categories: newTags
              }
            })
          }} />);
    } else {
      let categoryComponents = this.state.recipe.categories.map(category => {
        return this.toTitleCase(category.name)
      }).join(", ")
      return <div className="tag-style">{categoryComponents}</div>
    }
  }

  toTitleCase(str: string) {
    return str.replace(
      /\w\S*/g,
      function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
      }
    );
  }

  ingredientNutritionFacts() {
    if ((this.state.nutritionFacts?.recipe ?? null) !== null) {
      var lis = this.state.nutritionFacts?.ingredients.map((description, i) => {
        return (
          <div className="nbi-table-entry" key={i}>
            <div>{description.quantity} {description.unit == "count" ? "" : description.unit.toLowerCase()} {description.name}</div>
            <div className="nbi-table-source">
              {description.quantity} {description.unit == "count" ? "" : description.unit.toLowerCase()} {description.nutritionDatabaseId !== null ? <a target="_blank" href={`https://fdc.nal.usda.gov/fdc-app.html#/food-details/${description.nutritionDatabaseId}/nutrients`}>{description.nutritionDatabaseDescriptor}</a> : description.nutritionDatabaseDescriptor} | {Math.round(description.caloriesPerServing)} calories per serving</div>
          </div>);
      })
      return (
        <div className="nbi-table">
          <h1 className="performance-facts__title padding-8">Nutrition by Ingredient</h1>
          <div>
            {lis}
          </div>
        </div>);
    } else {
      return null;
    }
  }

  nutritionFacts() {
    // return null;
    if ((this.state.nutritionFacts?.recipe ?? null) !== null) {
      let {
        calories,
        carbohydrates,
        proteins,
        polyUnsaturatedFats,
        monoUnsaturatedFats,
        saturatedFats,
        sugars,
        transFats,
        iron,
        vitaminD,
        calcium,
        potassium
      } = this.state.nutritionFacts!.recipe;
      return (
        <>
          {/* <GoogleInFeedAds /> */}
          <NutritionFacts
            // servingSize={'1 cup (228g)'}
            // servingsPerContainer={2}
            calories={Math.round(calories / this.state.recipe.servingsProduced)}
            // totalFat={Math.round(monoUnsaturatedFats + polyUnsaturatedFats + saturatedFats)}
            saturatedFats={Math.round(saturatedFats / this.state.recipe.servingsProduced)}
            monoUnsaturatedFats={Math.round(monoUnsaturatedFats / this.state.recipe.servingsProduced)}
            polyUnsaturatedFats={Math.round(polyUnsaturatedFats / this.state.recipe.servingsProduced)}
            transFats={Math.round(transFats / this.state.recipe.servingsProduced)}
            // cholesterol={0}
            // sodium={0}
            carbohydrates={Math.round(carbohydrates / this.state.recipe.servingsProduced)}
            // dietaryFiber={0}
            sugars={Math.round(sugars / this.state.recipe.servingsProduced)}
            proteins={Math.round(proteins / this.state.recipe.servingsProduced)}
            servings={this.state.newServings}
            potassium={Math.round(potassium / this.state.recipe.servingsProduced)}
            vitaminD={Math.round(vitaminD / this.state.recipe.servingsProduced)}
            calcium={Math.round(calcium / this.state.recipe.servingsProduced)}
            iron={Math.round(iron / this.state.recipe.servingsProduced)}
          />
        </>
      )
    } else {
      return null;
    }
  }

  private caloriesPerServingComponent() {
    var rightColContents: any = null;
    if (!this.state.edit
      && this.state.recipe.caloriesPerServing === 0
      && (this.state.nutritionFacts?.recipe?.calories ?? 0) === 0) {
      // if there is no static calories per serving and if cannot compute a non-0 calories per serving, don't render anything
      return null
    } else if (!this.state.edit
      && this.state.recipe.caloriesPerServing === 0
      && (this.state.nutritionFacts?.recipe?.calories ?? 0) !== 0) {
      // if the user did not provide a value and we computed one, use that
      let { saturatedFats, monoUnsaturatedFats, polyUnsaturatedFats } = this.state.nutritionFacts!.recipe;
      let allFats = saturatedFats + monoUnsaturatedFats + polyUnsaturatedFats;
      rightColContents =
        <div>
          {Math.round(this.state.nutritionFacts!.recipe.calories / this.state.recipe.servingsProduced)} kcal <i className="bi bi-calculator"></i>
        </div>
    } else if (!this.state.edit
      && this.state.recipe.caloriesPerServing !== 0) {
      rightColContents =
        <div>
          {this.state.recipe.caloriesPerServing}
        </div>
    } else if (this.state.edit) {
      rightColContents =
        <Form.Control
          type="number"
          min="0"
          onChange={(e) => this.setState({ recipe: { ...this.state.recipe, caloriesPerServing: parseInt(e.target.value) } })}
          value={this.state.recipe.caloriesPerServing}></Form.Control>
    }

    return (
      <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
        <Col className="col-3 recipe-field-title">
          Calories per Serving
        </Col>
        <Col className="col d-flex align-items-center">
          {rightColContents}
        </Col>
      </Row>
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
      return null;
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

  private deleteComponent(recipe: MultiPartRecipe, componentIndex: number) {
    let newComponents = Array.from(recipe.recipeComponents.filter((c, i) => i !== componentIndex));
    this.setState({
      recipe: {
        ...this.state.recipe,
        recipeComponents: newComponents
      }
    })
  }

  private EditBlockComponent(
    componentIndex: number,
    recipe: MultiPartRecipe,
    component: RecipeComponent) {
    return (
      <div className="border-top-1 margin-bottom-20">
        {recipe.recipeComponents.length > 1 ?
          <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
            <Col className="col-3 recipe-field-title">
              Component Name
            </Col>
            <Col>
              <Row>
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
                    <div className="component-name-field">{component.name}</div>}
                </Col>
                {this.state.edit ?
                  <Col xs={1}>
                    <Button
                      className="float-end"
                      variant="danger"
                      onClick={(_) => this.deleteComponent(recipe, componentIndex)}>
                      <i className="bi bi-trash"></i>
                    </Button>
                  </Col>
                  : null
                }
              </Row>
            </Col>
          </Row>
          : null
        }
        <Row className="padding-right-0 recipe-edit-row">
          <Col className="col-3 recipe-field-title">
            Ingredients
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
        <Row className="padding-right-0">
          <Col className="col-3 recipe-field-title">
            Steps
          </Col>
          <Col className="col d-flex align-items-center">
            <div className="step-list">
              <RecipeStepList
                multipart={this.props.multipart}
                recipe={recipe}
                component={component}
                newServings={this.state.newServings}
                edit={this.state.edit}
                onDeleteStep={(idx) => this.deleteStep(idx, component)}
                onChange={(newSteps) => {
                  console.log(newSteps);
                  this.changeOrReorder(componentIndex, component, newSteps);
                  // this.setState({
                  //     ...this.state,
                  //     recipe: {
                  //         ...recipe,

                  //         // todo TODO wtf?
                  //     }
                  // })
                }
                }
                onNewStep={() => this.appendNewStepForComponent(componentIndex, component)} />
            </div>
          </Col>
        </Row>
      </div>
    )
  }

  private image() {
    if (this.state.newImageSrc != null && this.state.newImageSrc != '') {
      return <img className="recipe-image" src={this.state.newImageSrc} />
    } else if (this.state.recipeImages.length > 0 && this.state.recipeImages[0].url) {
      return <img
        className="recipe-image"
        src={this.state.recipeImages[0].url} />
    } else {
      return (this.state.recipe.staticImage === null) ?
        <img className="recipe-image" src={`/placeholder.jpg`} />
        :
        <img className="recipe-image" src={`/${this.state.recipe.staticImage}`} />;
    }
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

  onAddtoCard(): void {
    fetch(`/api/Cart?recipeId=${this.props.recipeId}`, {
      method: 'POST',
    }).then(response => {
      if (response.redirected) {
        window.location.href = response.url
      }
    })
  }

  onCancel(): void {
    location.reload();
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
      // this.saveSimpleRecipe();
    } else {
      this.saveMultiPartRecipe();
    }
  }

  saveMultiPartRecipe() {
    var recipe = this.state.recipe as MultiPartRecipe;
    for (let i = 0; i < recipe.recipeComponents.length; i++) {
      const component = recipe.recipeComponents[i];
      component.steps = Array.from(component.steps ?? []).filter(step => step != null && step.trim() !== '');
      component.ingredients = Array.from(component.ingredients ?? []).filter(ingredient => ingredient.ingredient.name != null && ingredient.ingredient.name != '');
    }

    if (recipe.caloriesPerServing === null || isNaN(recipe.caloriesPerServing)) {
      recipe.caloriesPerServing = 0.0
    }

    if (recipe.servingsProduced === null || isNaN(recipe.servingsProduced)) {
      recipe.servingsProduced = 1
    }

    const updateDto = toRecipeUpdateDto(recipe);

    fetch(`/api/MultiPartRecipe/${this.props.recipeId}`, {
      method: 'PUT',
      body: JSON.stringify(updateDto),
      headers: {
        'Content-Type': 'application/json'
      }
    }).then(response => {
      if (response.ok) {
        if (this.state.newImage != null) {
          let fd = new FormData();
          fd.append("files", this.state.newImage as Blob);
          fetch(`/api/MultiPartRecipe/${this.props.recipeId}/image`, {
            method: 'PUT',
            body: fd
          }).then(_ => {
            location.reload();
          });
        } else {
          location.reload();
        }
      } else {
        this.setState({ error: true, operationInProgress: false });
      }
    })
  }
}