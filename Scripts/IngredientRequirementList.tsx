import React from "react";
import { Button, Col, Form, Row, ToggleButton, ToggleButtonGroup } from "react-bootstrap";
import { stringify, v4 as uuidv4 } from 'uuid';
import { IngredientDisplay } from "./IngredientDisplay";
import { IngredientInput } from "./IngredientInput";

type IngredientRequirementListProps = {
    ingredientRequirements: IngredientRequirement[],
    onDelete : (ir : IngredientRequirement) => void,
    onNewIngredientRequirement : () => void,
    updateIngredientRequirement : (ir: IngredientRequirement, update : (ir : IngredientRequirement) => IngredientRequirement) => void,
    units : MeasureUnit[],
    edit: boolean
    multiplier : number
}

type IngredientRequirementListState = {
    unitPreference : null | "Imperial" | "Metric"
}

export class IngredientRequirementList extends React.Component<IngredientRequirementListProps, IngredientRequirementListState> {
    constructor(props : IngredientRequirementListProps) {
        super(props);
        this.state = {
            unitPreference: null
        }
    }

    ingredientEditRow(ir : IngredientRequirement, idx : number) {
        // this.updateIngredientQuantity(ir, 10)
        var id = ir.ingredient.id
        if (ir.ingredient.id === '' || ir.ingredient.id === '00000000-0000-0000-0000-000000000000') {
            id = idx.toString()
        }
        var massOptions = this.props.units.filter(u => u.siType === "Weight").map(unit => {
            return <option key={unit.name} value={unit.name}>{unit.name}</option>
        })
        var volumeOptions = this.props.units.filter(u => u.siType === "Volume").map(unit => {
            return <option key={unit.name} value={unit.name}>{unit.name}</option>
        })
        var countOptions = this.props.units.filter(u => u.siType === "Count").map(unit => {
            return <option key={unit.name} value={unit.name}>{unit.name}</option>
        })
        var innerSelect = ( [
            {group: "Count", options: countOptions},
            {group: "Weight", options: massOptions},
            {group: "Volume", options: volumeOptions}
        ].map(x => {
            return (<optgroup label={x.group}>{x.options}</optgroup>)
        }))
        return (
            <Row key={id} className="margin-bottom-8">
                <Col key={`${id}quantity`} xs={2}>
                    <Form.Control
                        type="number"
                        min="0"
                        onChange={(e) => this.props.updateIngredientRequirement(ir, ir => { ir.quantity = parseFloat(e.target.value); return ir; } ) }
                        placeholder={"0"}
                        value={ir.quantity === 0.0 ? undefined : ir.quantity}></Form.Control>
                </Col>
                <Col key={`${id}unit`} xs={3}>
                    <Form.Select
                        onChange={(e) => this.props.updateIngredientRequirement(ir, ir => {ir.unit = e.currentTarget.value; return ir; })}
                        value={ir.unit}>
                        { innerSelect }
                    </Form.Select>
                </Col>
                <Col className="get-smaller" key={`${id}name`} >
                    <IngredientInput
                        isNew={ir.ingredient.isNew}
                        query={text => `/api/recipe/ingredients?name=${text}`}
                        ingredient={ir.ingredient}
                        className=""
                        onSelect={(i, isNew) => this.props.updateIngredientRequirement(ir, ir => {
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
                <Col key={`${id}delete`} xs={1} className="">
                    <Button className="float-end" variant="danger">
                        <i onClick={(_) => this.props.onDelete(ir)} className="fas fa-trash-alt"></i>
                    </Button>
                </Col>
            </Row>
        )
    }

    render() {
        if (!this.props.edit) {
            let rows = this.props.ingredientRequirements.map(ingredient => {
                let newQuantity = ingredient.quantity * this.props.multiplier;
                return (
                    <Row className="ingredient-item">
                        <IngredientDisplay
                            showAlternatUnit={true}
                            units={this.props.units}
                            ingredientRequirement={{...ingredient, quantity: newQuantity}} />
                    </Row>
                )
            });
            return rows;
            // return (
            //     <div>
            //             <ToggleButtonGroup
            //                 name="options"
            //                 type="radio"
            //                 value={this.state.unitPreference}
            //                 onChange={e => {
            //                     if (e[0] == "null") {
            //                         this.setState({unitPreference: null})
            //                     } else if (e[0] == "Imperial") {
            //                         this.setState({unitPreference: "Imperial"})
            //                     } else if (e[0] == "Metric") {
            //                         this.setState({unitPreference: "Metric"})
            //                     }
            //                 }}>
            //                 <ToggleButton id="tbg-btn-1" value={"Imperial"}>
            //                     Imperial
            //                 </ToggleButton>
            //                 <ToggleButton id="tbg-btn-2" value={"null"}>
            //                     As is
            //                 </ToggleButton>
            //                 <ToggleButton id="tbg-btn-3" value={"Metric"}>
            //                     Metric
            //                 </ToggleButton>
            //             </ToggleButtonGroup>
            //     </div>
            // )
        } else {
            return (
                <Form>
                    { this.props.ingredientRequirements?.map((i, idx) => this.ingredientEditRow(i, idx)) }
                    <Col xs={12}>
                        <Button variant="outline-primary" className="width-100" onClick={_ => this.props.onNewIngredientRequirement()}>New ingredient</Button>
                    </Col>
                </Form>
            )
        }
    }
}
