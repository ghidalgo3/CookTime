import * as React from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay } from './IngredientDisplay';
import { parse, v4 as uuidv4 } from 'uuid';
import { TodaysTenDisplay } from './todaysTenDisplay';

type CartState = {
    cart : Cart
}
class ShoppingCart extends React.Component<{}, CartState> {
    constructor(props: {}) {
        super(props);
        this.state = {
            cart: {
                id: '',
                CreateAt: '',
                recipeRequirement: [],
                active: true,
                ingredientState: [],
                dietDetails: []
            },
        }
    }

    private todaysTen() {
        let todaysTen = this.state.cart.dietDetails.find(dd => dd.name === "TodaysTen")!
        if (todaysTen != null) {
            return <TodaysTenDisplay todaysTen={todaysTen} />
        } else {
            return null;
        }
    }

    componentDidMount() {
        fetch(`/api/cart`)
            .then(response => response.json())
            .then(
                result => {
                    let cart = result as Cart
                    cart.ingredientState = cart.ingredientState.filter(is => is.ingredient !== null)
                    this.setState({
                        cart
                    })
                }
            )
    }

    onDeleteRecipe = (idx : number) => {
        var newCart = {
            ...this.state.cart,
            recipeRequirement: this.state.cart.recipeRequirement.filter((r, i) => i !== idx),
        }
        this.setState({ cart: newCart })
        this.PutCart(newCart);
    }

    PutCart = (newCart: Cart) => {
        fetch(`api/Cart/${this.state.cart.id}`, {
            method: "PUT",
            body: JSON.stringify(newCart),
            headers: {
                'Content-Type': 'application/json'
            }
        });
    }

    render() {
        let recipes = this.state.cart?.recipeRequirement.map((r, rIndex) => {
            let recipe = r.recipe ?? r.multiPartRecipe;
            return (
                <Row key={rIndex} className="align-items-center padding-left-0 margin-top-10">
                    <Col className="col d-flex align-items-center">
                        <div className="serving-counter-in-cart">
                            <Button
                                variant="danger"
                                className="minus-counter-button"
                                onClick={(_) => {
                                    let qty = Array.from(this.state.cart.recipeRequirement)[rIndex].quantity;
                                    if (qty > 0) {
                                        this.addToRecipeRequirement(rIndex, -1)}
                                    }
                                }>
                                <i className="fas fa-regular fa-minus"></i>
                            </Button>
                            <Form.Control
                                onChange={(e) => {
                                    if (e.target.value === '') {
                                        this.setRecipeRequirement(rIndex, 0)
                                    }

                                    let newValue = parseFloat(e.target.value)
                                    if (newValue !== NaN && newValue > 0) {
                                        this.setRecipeRequirement(rIndex, newValue)
                                    }
                                }}
                                className="form-control count"
                                value={Math.round(r.quantity * recipe.servingsProduced)} />
                            <Button
                                variant="success"
                                className="plus-counter-button"
                                onClick={(_) => this.addToRecipeRequirement(rIndex, 1)}>
                                <i className="fas fa-solid fa-plus"></i>
                            </Button>
                        </div> 
                        <div id="cart-recipe-item" className="form-control input-field-style margin-left-20 margin-right-10 do-not-overflow-text" key={recipe.id}>
                            <a href={`/Recipes/Details?id=${recipe.id}&servings=${r.quantity * recipe.servingsProduced}`} >{recipe.name}</a> 
                        </div>
                        <Button 
                            className="float-end height-38" 
                            variant="danger"
                            onClick={(_) => this.onDeleteRecipe(rIndex)}>
                            <i className="fas fa-trash-alt"></i>
                        </Button>
                    </Col>
                </Row>
            )
        });
        let aggregateIngredients = this.getAggregateIngredients();
        return (
            <Form>
                <Row>
                    <Col className="justify-content-md-left" xs={6}>
                        <h1 className="margin-bottom-20">Groceries List</h1>
                    </Col>
                    <Col>
                        <Button variant="danger" className="float-end" onClick={_ => this.onClear()}>Clear Cart</Button>
                    </Col>
                </Row>
                {
                    this.todaysTen()
                }
                <div className="cart-header">
                    Servings
                </div>
                <div>
                    {recipes}
                </div>
                <div className="cart-header margin-top-15">
                    Ingredients
                </div>
                <div>
                    {aggregateIngredients}
                </div>
            </Form>
        )
    }

    addToRecipeRequirement(rIndex : number, arg1: number): void {
            var newRRequirements = Array.from(this.state.cart.recipeRequirement);
            let denominator = newRRequirements[rIndex].recipe?.servingsProduced ?? newRRequirements[rIndex].multiPartRecipe.servingsProduced;
            newRRequirements[rIndex].quantity += (arg1 / denominator);
            if (newRRequirements[rIndex].quantity > 0) {
                let newCart = {...this.state.cart, recipeRequirement: newRRequirements}
                this.setState({cart : newCart});
                this.PutCart(newCart);
            }
            else {
                console.log(newRRequirements[rIndex].quantity);
            }
    }

    setRecipeRequirement(rIndex : number, newQuantity : number) : void {
        var newRRequirements = Array.from(this.state.cart.recipeRequirement);
        let denominator = newRRequirements[rIndex].recipe?.servingsProduced ?? newRRequirements[rIndex].multiPartRecipe.servingsProduced;
        newRRequirements[rIndex].quantity = (newQuantity / denominator);
        let newCart = {...this.state.cart, recipeRequirement: newRRequirements}
        this.setState({cart : newCart});
        this.PutCart(newCart);
    }

    onClear() {
        fetch("/api/Cart/clear", {
            method: "POST"
        })
        .then(response => {
            this.setState({cart: {...this.state.cart, recipeRequirement: []}});
        });
    }

    getAggregateIngredients() {
        var allRecipeRequirements = this.state.cart?.recipeRequirement;
        // for each recipe, take their original ingredient requirement and multiply by the recipe requirement
        // for example, if recipeRequirement.quantity = 2 and ir.quantity = 2, then the new ir.quantity needs to be 4 = 2 * 2
        var allIngredientRequirements = allRecipeRequirements.flatMap((recipeRequirement, rrIndex) => {
            if (recipeRequirement.recipe !== null) {
                return recipeRequirement.recipe.ingredients!.map((ir, irIndex) => {
                    return { ...ir, quantity: ir.quantity * recipeRequirement.quantity};
                })
            } else {
                return recipeRequirement.multiPartRecipe.recipeComponents.flatMap(component => {
                    return component.ingredients!.map((ir, irIndex) => {
                        return { ...ir, quantity: ir.quantity * recipeRequirement.quantity};
                    })
                })
            }
        })
        var reducedIngredientRequirements : IngredientRequirement[] = []
        // now we need to add them all up
        allIngredientRequirements.forEach(ir => {
            // is there an element in reducedIrs with the same unit and ingredient id?
            var indexOfMatch = reducedIngredientRequirements.findIndex((currentIr, index) => {
                return currentIr.ingredient.id == ir.ingredient.id && currentIr.unit == ir.unit;
            })
            if (indexOfMatch === -1) {
                //no!
                reducedIngredientRequirements.push(ir);
            } else {
                //yes!
                reducedIngredientRequirements[indexOfMatch].quantity += ir.quantity;
            }
        });
        var uncheckedFn = ir => !this.state.cart.ingredientState.some(is => is.ingredient.id === ir.ingredient.id)
        reducedIngredientRequirements.sort((ir1, ir2) => {
            let x = uncheckedFn(ir1)
            let y = uncheckedFn(ir2)
            if (x === y)  {
                return 0
            } else if (x) {
                return -1
            } else {
                return 1
            }
        })
        // render an empty check mark unless the ingredient is present in ingredient state with checked == true
        return reducedIngredientRequirements?.map(ir => {
            var unchecked = uncheckedFn(ir);
            var onClickFn = unchecked ? () => this.CheckIngredient(ir) : () => this.UncheckIngredient(ir);
            return (
            <div onClick={(_) => onClickFn()} className="cart-ingredients-list">
                {
                    unchecked ?
                      <i className="far fa-circle padding-right-10"></i> : 
                      <i className="far fa-check-circle padding-right-10"></i>
                    }
                <IngredientDisplay ingredientRequirement={ir} strikethrough={!unchecked}/>
            </div>
            )
        })
    }

    UncheckIngredient(ir: IngredientRequirement): void {
        if (this.state.cart.ingredientState.some(is => is.ingredient.id === ir.ingredient.id)) {
            var newIngredientState = this.state.cart.ingredientState.filter(is => is.ingredient.id !== ir.ingredient.id)
            let newCart = {
                    ...this.state.cart,
                    ingredientState: newIngredientState
                }
            this.setState({ cart: newCart });
            this.PutCart(newCart);
        }
    }

    CheckIngredient(ir: IngredientRequirement): void {
        if (!this.state.cart.ingredientState.some(is => is.ingredient.id === ir.ingredient.id)) {
            var newIngredientState = Array.from(this.state.cart.ingredientState)
            // BABE TO DO: do not allow inserting duplicate ingredient states
            newIngredientState.push({
                id: uuidv4(),
                ingredient: ir.ingredient,
                checked: true
            })
            let newCart = {
                    ...this.state.cart,
                    ingredientState: newIngredientState
                }
            this.setState({ cart: newCart });
            this.PutCart(newCart);
        }
    }
}
const recipeContainer = document.querySelector('#cart');
ReactDOM.render(
    <ShoppingCart />,
    recipeContainer);