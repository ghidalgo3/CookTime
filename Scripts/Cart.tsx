import * as React from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay } from './IngredientInput';
import { v4 as uuidv4 } from 'uuid';

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
                ingredientState: []
            },
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
            return (
                <Row key={rIndex} className="align-items-center padding-left-0 margin-top-10">
                    <Col className="recipe-counter-column">
                        <div className="serving-counter">
                            <i
                                onClick={(_) => this.addToRecipeRequirement(rIndex, 1)}
                                className="fas fa-plus-circle green-earth-color"></i>
                            <input
                                className="form-control count"
                                value={r.quantity * r.recipe.servingsProduced}></input>
                            <i
                                onClick={(_) => this.addToRecipeRequirement(rIndex, -1)}
                                className="fas fa-minus-circle red-dirt-color"></i>
                        </div> 
                    </Col>
                    <Col>
                        <div key={r.recipe.id}>
                            <a href={`/Recipes/Details?id=${r.recipe.id}`}>{r.recipe.name}</a> 
                            <i className="fas fa-trash deep-water-color padding-left-12" onClick={(_) => this.onDeleteRecipe(rIndex)}></i>
                        </div>
                    </Col>
                </Row>
            )
        });
        let aggregateIngredients = this.getAggregateIngredients();
        return (
            <Form>
                <Row>
                    <Col className="justify-content-md-left" xs={6}>
                        <h1 className="margin-bottom-20">Cart</h1>
                    </Col>
                    <Col>
                        <Button variant="danger" className="float-end" onClick={_ => this.onClear()}>Clear Cart</Button>
                    </Col>
                </Row>
                <div className="cart-header">
                    SERVINGS
                </div>
                <div>
                    {recipes}
                </div>
                <div className="cart-header margin-top-15">
                    INGREDIENTS
                </div>
                <div>
                    {aggregateIngredients}
                </div>
            </Form>
        )
    }

    addToRecipeRequirement(rIndex : number, arg1: number): void {
        var newRRequirements = Array.from(this.state.cart.recipeRequirement);
        newRRequirements[rIndex].quantity += (arg1 / newRRequirements[rIndex].recipe.servingsProduced);
        newRRequirements[rIndex].quantity = Math.round(newRRequirements[rIndex].quantity);
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
        var allIngredientRequirements : IngredientRequirement[] = allRecipeRequirements.flatMap((recipeRequirement, rrIndex) => {
            return recipeRequirement.recipe.ingredients.map((ir, irIndex) => {
                return { ...ir, quantity: ir.quantity * recipeRequirement.quantity};
            })
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
            return (
            <div className="cart-ingredients-list">
                {
                    unchecked ?
                      <i onClick={(_) => this.CheckIngredient(ir)}className="far fa-circle padding-right-10"></i> : 
                      <i onClick={(_) => this.UncheckIngredient(ir)} className="far fa-check-circle padding-right-10"></i>
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