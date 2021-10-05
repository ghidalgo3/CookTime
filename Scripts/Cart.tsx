import * as React from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import * as ReactDOM from 'react-dom';

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

    render() {
        let aggregateIngredients = this.getAggregateIngredients();
        let recipes = this.state.cart?.recipeRequirement.map(r => <Row key={r.recipe.id}>{r.recipe.name}</Row>)
        return (
            <Form>
                <Col>
                    {aggregateIngredients}
                    {recipes}
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
        var allIngredientRequirements = this.state.cart?.recipeRequirement.flatMap(recipeRequirement => {
            return recipeRequirement.recipe.ingredients;
        })
        return allIngredientRequirements?.map(ir => {
            return <Row key={ir.id}>{ir.quantity} {ir.unit} {ir.ingredient.name}</Row>
        })
    }
}
const recipeContainer = document.querySelector('#cart');
ReactDOM.render(
    <ShoppingCart />,
    recipeContainer);