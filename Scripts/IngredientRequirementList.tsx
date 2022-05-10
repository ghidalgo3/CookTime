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
            var printable = ""
            switch (unit.name) {
                case "Ounce":
                    printable = "oz";
                    break;
                case "Pound":
                    printable = "lb";
                    break;
                case "Milligram":
                    printable = "mg";
                    break;
                case "Gram":
                    printable = "g";
                    break;
                case "Kilogram":
                    printable = "kg";
                    break;
            }
            return <option key={unit.name} value={unit.name}>{printable}</option>
        })
        var volumeOptions = this.props.units.filter(u => u.siType === "Volume").map(unit => {
            var printable = ""
            switch (unit.name) {
                case "Tablespoon":
                    printable = "Tbps";
                    break;
                case "Teaspoon":
                    printable = "tsp";
                    break;
                case "Milliliter":
                    printable = "mL";
                    break;
                case "Cup":
                    printable = "cup";
                    break;
                case "FluidOunce":
                    printable = "fl oz";
                    break;
                case "Pint":
                    printable = "pint";
                    break;
                case "Quart":
                    printable = "quart";
                    break;
                case "Gallon":
                    printable = "gallon";
                    break;
                case "Liter":
                    printable = "L";
                    break;
            }
            return <option key={unit.name} value={unit.name}>{printable}</option>
        })
        var countOptions = this.props.units.filter(u => u.siType === "Count").map(unit => {
            var printable = "";
            if (unit.name == "Count") {
                printable = "unit";
            } 
            return <option key={unit.name} value={unit.name}>{printable}</option>
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
                <Col key={`${id}quantity`} xs={2} className="ingredient-col-left">
                    <Form.Control
                        type="number"
                        min="0"
                        onChange={(e) => this.props.updateIngredientRequirement(ir, ir => { ir.quantity = parseFloat(e.target.value); return ir; } ) }
                        placeholder={"0"}
                        value={ir.quantity === 0.0 ? undefined : ir.quantity}></Form.Control>
                </Col>
                <Col key={`${id}unit`} xs={3} className="ingredient-col-middle">
                    <Form.Select
                        className="border-0"
                        onChange={(e) => this.props.updateIngredientRequirement(ir, ir => {ir.unit = e.currentTarget.value; return ir; })}
                        value={ir.unit}>
                        { innerSelect }
                    </Form.Select>
                </Col>
                <Col key={`${id}name`} className="ingredient-col-right get-smaller">
                    <IngredientInput
                        isNew={ir.ingredient.isNew}
                        query={text => `/api/recipe/ingredients?name=${text}`}
                        ingredient={ir.ingredient}
                        className=""
                        currentRequirements={this.props.ingredientRequirements}
                        onSelect={(text, i, isNew) => this.props.updateIngredientRequirement(ir, ir => {
                            ir.ingredient = i
                            ir.ingredient.isNew = isNew
                            ir.text = text
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
