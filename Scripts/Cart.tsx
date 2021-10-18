import * as React from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';
import { IngredientDisplay } from './IngredientInput';

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
                active: true
            }
        }
    }

    componentDidMount() {
        fetch(`/api/cart`)
            .then(response => response.json())
            .then(
                result => {
                    this.setState({cart: result as Cart})
                }
            )
    }

    onDeleteRecipe = (idx : number) => {
        var newCart = {
            ...this.state.cart,
            recipeRequirement: this.state.cart.recipeRequirement.filter((r, i) => i !== idx),
        }
        this.setState({ cart: newCart })
        fetch(`api/Cart/${this.state.cart.id}`, {
            method: "PUT",
            body: JSON.stringify(newCart),
            headers: {
                'Content-Type': 'application/json'
            }
        })
    }

    render() {
        let aggregateIngredients = this.getAggregateIngredients();
        let recipes = this.state.cart?.recipeRequirement.map(r => {
            return (
                <Row className="align-items-center">
                    <Col className="recipe-counter-column">
                        <div className="serving-counter">
                            {/* BABE TO DO: MAKE THE COUNTER WORK AND MULTIPLY THE RENDERED INGREDIENT QTYS */}
                            <i className="fas fa-plus-circle deep-water-color"></i>
                            <input className="form-control count" value={r.quantity}></input>
                            <i className="fas fa-minus-circle deep-water-color"></i>
                        </div> 
                    </Col>
                    <Col>
                        <div key={r.recipe.id}>
                            <a href={`/Recipes/Details?id=${r.recipe.id}`}>{r.recipe.name}</a> x {r.quantity}
                        </div>
                    </Col>
                </Row>
            )
        });
        return (
            <Form>
                <Col>
                    <Row>
                        {recipes}
                    </Row>
                    <Row className="cart-header">
                        INGREDIENTS
                    </Row>
                    <Row>
                        {aggregateIngredients}
                    </Row>
                    <Row>
                        <Button variant="danger" className="width-100" onClick={_ => this.onClear()}>Clear Cart</Button>
                    </Row>
                </Col>
            </Form>
        )
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
        var allIngredientRequirements : IngredientRequirement[] = allRecipeRequirements.flatMap(recipeRequirement => {
            return recipeRequirement.recipe.ingredients.map(ir => {
                ir.quantity = ir.quantity * recipeRequirement.quantity;
                return ir;
            })
        })
        var reducedIngredientRequirements : IngredientRequirement[] = []
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
        reducedIngredientRequirements.sort((ir1, ir2) => ir1.ingredient.name.localeCompare(ir2.ingredient.name));
        return reducedIngredientRequirements?.map(ir => {
            return (
            <div className="cart-ingredient-item" key={ir.id}>
                <IngredientDisplay ingredientRequirement={ir}/>
            </div>)
        })
    }
}
const recipeContainer = document.querySelector('#cart');
ReactDOM.render(
    <ShoppingCart />,
    recipeContainer);