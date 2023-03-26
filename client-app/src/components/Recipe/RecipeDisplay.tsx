import { Rating } from "@smastrom/react-rating";
import React, {useEffect, useState} from "react"
import { Button, Col, Form, Row, Stack } from "react-bootstrap";
import { DietDetails, getNutritionData, Image, MultiPartRecipe, RecipeComponent, RecipeNutritionFacts, toTitleCase } from "src/shared/CookTime";
import { useAuthentication } from "../Authentication/AuthenticationContext";
import { NutritionFacts } from "../NutritionFacts";
import { RecipeStructuredData } from "../RecipeStructuredData";
import { TodaysTenDisplay } from "../todaysTenDisplay";

export default function RecipeDisplay(props: {recipe : MultiPartRecipe, servings? : number}) {
  const { recipe } = props;
  const queryServings = props.servings ?? 0;
  const { user } = useAuthentication();
  const canEdit = user?.id === recipe.owner?.id;
  const [ nutritionData, setNutritionData ] = useState<RecipeNutritionFacts>();
  const [servings, setServings] = useState<number>(queryServings > 0 ? queryServings : recipe.servingsProduced);
  const [images, setImages] = useState<Image[]>([])

  useEffect(() => {
    async function loadImages() {
      const response = await fetch(`/api/multipartrecipe/${recipe.id}/images`)
      const images = await response.json() as Image[];
      setImages(images);
      // setImages((await loadImages()) as Image[]);
    }
    loadImages();
  }, [recipe])

  useEffect(() => {
    async function loadNutritionData() {
      const nutritionFacts = await getNutritionData(recipe.id)
      setNutritionData(nutritionFacts)
    }
    loadNutritionData();
  }, [recipe]);

  function ButtonGroup() {
    const editButton =
      <Button
        className="recipe-edit-buttons"
        disabled={!user || !canEdit}>
        Edit
      </Button>
    const addToGroceries =
      <Button
        className="recipe-edit-buttons"
        disabled={!user}>
        Add to Groceries
      </Button>
    return (
      <Row>
        <Col>
          {(!user || !canEdit) ?
            <div data-bs-toggle="tooltip" data-bs-placement="bottom" title="Sign in to modify your own recipes">
              {editButton}
            </div>
            :
            editButton
          }
        </Col>
        <Col>
          {!user ?
            <div data-bs-toggle="tooltip" data-bs-placement="bottom" title="Sign in to add recipes to your cart">
              {addToGroceries}
            </div>
            :
            addToGroceries
          }
        </Col>
      </Row>
    )
  }

  function RecipeImage() {
    // if (this.state.newImageSrc != null && this.state.newImageSrc != '') {
    //   return <img className="recipe-image" src={this.state.newImageSrc} />
    // } else
    if (images.length > 0) {
      return <img
        className="recipe-image"
        alt="Food"
        src={`/image/${images[0].id}`} />
    } else {
      return (recipe.staticImage === null) ?
        <img
          alt="Placeholder food"
          className="recipe-image"
          src={`/placeholder.jpg`} />
        :
        <img
          alt="Food"
          className="recipe-image"
          src={`/${recipe.staticImage}`} />;
    }
  }

  function TodaysTen() {
    const drwoz = nutritionData?.dietDetails.find(dd => dd.name === DietDetails.TODAYS_TEN)
    if (drwoz) {
      return <TodaysTenDisplay todaysTen={drwoz} />
    } else {
      return null;
    }
  }

  function caloriesPerServing() {
    let rightColContent = null
    if (recipe.caloriesPerServing === 0
      && (nutritionData?.recipe?.calories ?? 0) === 0) {
      return null;
    } else if (recipe.caloriesPerServing === 0
      && (nutritionData?.recipe?.calories ?? 0) !== 0) {
      // if the user did not provide a value and we computed one, use that
      (
        rightColContent = <div>
        {Math.round(nutritionData!.recipe.calories / recipe.servingsProduced)} kcal <i className="fas fa-solid fa-calculator"></i>
      </div>)
    } else if (recipe.caloriesPerServing !== 0) {
      return (
        rightColContent = <div>
          {recipe.caloriesPerServing}
        </div>
      )
    }
    return (
      <>
        <Col className="col-3 recipe-field-title">
          Calories per Serving
        </Col>
        <Col className="col d-flex align-items-center">
          {rightColContent}
        </Col>
      </>
    )
  }

  function servingsModifier() {
    return (<div className='serving-counter'>
      <Button
        variant="danger"
        className="minus-counter-button"
        onClick={(_) => {
          setServings((servings) => {
            return Math.max(0, servings - 1);
          })
        }}>
        <i className="fas fa-regular fa-minus"></i>
      </Button>
      <Form.Control
        onChange={(e) => {
          if (e.target.value === '') {
            setServings(0);
          }
          let newValue = parseFloat(e.target.value)
          if (!Number.isNaN(newValue) && newValue > 0) {
            setServings(newValue);
          }
        }}
        className="form-control count"
        value={servings} />
      <Button
        variant="success"
        className="plus-counter-button"
        onClick={(_) => {
          setServings((servings) => servings + 1)}
        }>
        <i className="fas fa-solid fa-plus"></i>
      </Button>
    </div>);
  }

  function TagsDisplay() {
    let categoryComponents = recipe.categories.map(category => {
      return toTitleCase(category.name)
    }).join(", ")
    return <div className="tag-style">{categoryComponents}</div>
  }

  function CookingTimeDisplay() {
    return (
      <>
        <Col className="col-3 recipe-field-title">
          Cook Time (Minutes)
        </Col>
        <Col className="col d-flex align-items-center">
          <div>{recipe.cooktimeMinutes}</div>
        </Col>
      </>
    )
  }

  function OriginalSourceDisplay() {
    return (
      <>
        <Col className="col-3 recipe-field-title">
          Link to Original Recipe
        </Col>
        <Col className="col d-flex align-items-center">
            <a href={(recipe.source)}>{recipe.source}</a>
        </Col>
      </>
    ) 
  }

  function nutritionFacts() {
    // return null;
    if ((nutritionData?.recipe ?? null) !== null) {
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
      } = nutritionData!.recipe;
      return (
        <NutritionFacts
          servings={servings}
          // servingSize={'1 cup (228g)'}
          // servingsPerContainer={2}
          calories={Math.round(calories / recipe.servingsProduced)}
          // totalFat={Math.round(monoUnsaturatedFats + polyUnsaturatedFats + saturatedFats)}
          saturatedFats={Math.round(saturatedFats / recipe.servingsProduced)}
          monoUnsaturatedFats={Math.round(monoUnsaturatedFats / recipe.servingsProduced)}
          polyUnsaturatedFats={Math.round(polyUnsaturatedFats / recipe.servingsProduced)}
          transFats={Math.round(transFats / recipe.servingsProduced)}
          // cholesterol={0}
          // sodium={0}
          carbohydrates={Math.round(carbohydrates / recipe.servingsProduced)}
          // dietaryFiber={0}
          sugars={Math.round(sugars / recipe.servingsProduced)}
          proteins={Math.round(proteins / recipe.servingsProduced)}
          potassium={Math.round(potassium / recipe.servingsProduced)}
          vitaminD={Math.round(vitaminD / recipe.servingsProduced)}
          calcium={Math.round(calcium / recipe.servingsProduced)}
          iron={Math.round(iron / recipe.servingsProduced)}
        />)
    } else {
      return null;
    }
  }

  function ingredientNutritionFacts() {
    if ((nutritionData?.recipe ?? null) !== null) {
      var lis = nutritionData?.ingredients.map((description, i) => {
        return (
          <div className="nbi-table-entry" key={i}>
            <div>{description.quantity} {description.unit === "Count" ? "" : description.unit.toLowerCase()} {description.name}</div>
            <div className="nbi-table-source">
              {description.quantity} {description.unit === "Count" ? "" : description.unit.toLowerCase()} {description.nutritionDatabaseId !== null ? <a target="_blank" href={`https://fdc.nal.usda.gov/fdc-app.html#/food-details/${description.nutritionDatabaseId}/nutrients`}>{description.nutritionDatabaseDescriptor}</a> : description.nutritionDatabaseDescriptor} | {Math.round(description.caloriesPerServing)} calories per serving</div>
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

  function ComponentDisplay({ component }: { component: RecipeComponent }) {
    return null;
  }

  return (<>
    <RecipeStructuredData recipe={recipe} images={[]} />
    <Row>
      <Col className="justify-content-md-left" xs={6}>
        <h1>{recipe.name}</h1>
        {
          recipe.reviewCount > 0 &&
          <Stack direction="horizontal" className="margin-bottom-8">
            <Rating
              style={{ maxWidth: 150 }}
              value={recipe.averageReviews}
              readOnly />{" "}({recipe.reviewCount})
          </Stack>
        }
        By {recipe.owner?.userName}
      </Col>
      <Col>
        <ButtonGroup />
      </Col>
    </Row>
    <Row>
      <Col>
        <RecipeImage />
      </Col>
    </Row>
    <Row>
      <Col>
        <TodaysTen />
      </Col>
    </Row>
    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
      {caloriesPerServing()}
    </Row>
    <Row>
      <Col className="col-3 recipe-field-title">
        Servings
      </Col>
      <Col className="col d-flex align-items-center">
        {servingsModifier()}
      </Col>
    </Row>
    <Row className="padding-right-0 d-flex align-items-center recipe-edit-row">
      {recipe.categories.length > 0 && <TagsDisplay />}
    </Row>
    <Row>
      {recipe.cooktimeMinutes! > 0 && <CookingTimeDisplay /> }
    </Row>
    <Row>
      {recipe.source && <OriginalSourceDisplay /> }
    </Row>
    {/* Recipe components and ingredients go here  */}
    {recipe.recipeComponents.map((component, idx, components) =>
      <ComponentDisplay component={component} key={idx}/>
    )}
    <div className="border-top-1 margin-top-10">
      <Row>
        <Col xs={3} className="nft-row">
          {nutritionFacts()}
        </Col>
        <Col>
          {ingredientNutritionFacts()}
        </Col>
      </Row>
    </div>
  </>
  )
}