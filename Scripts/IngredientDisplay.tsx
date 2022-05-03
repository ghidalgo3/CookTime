import React from "react";

type IngredientDisplayProps = {
    ingredientRequirement: IngredientRequirement
    strikethrough?: boolean,
    showAlternatUnit?: boolean
    units? : MeasureUnit[]
}
export class IngredientDisplay extends React.Component<IngredientDisplayProps, {}> {
    constructor(props) {
        super(props);
    }


    render() {
        let ingredient = this.props.ingredientRequirement.ingredient
        // var unitName = (this.props.ingredientRequirement.unit == "Count" ? "" : this.props.ingredientRequirement.unit).toLowerCase()
        var unitName = ""
        switch (this.props.ingredientRequirement.unit) {
            case "Count":
                unitName = ""
                break;

            case "FluidOunce":
                unitName = "fluid ounce"
                break;
        
            default:
                unitName = this.props.ingredientRequirement.unit.toLowerCase()
                break;
        }
        var quantity = <>{this.props.ingredientRequirement.quantity.toString()}</>
        var integral = Math.floor(this.props.ingredientRequirement.quantity)
        let decimal = this.props.ingredientRequirement.quantity % 1
        let decimalStr = decimal.toFixed(4)
        if (decimalStr === "0.0625") {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>16</sub></>
        } else if (decimalStr === "0.1250") {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>8</sub></>
        }
        
        if (0 < decimal && decimal <= 0.0625) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>16</sub></>
        } else if (0.0625 < decimal && decimal <= 0.1250) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>8</sub></>
        } else if (0.1250 < decimal && decimal <= 0.1875) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>3</sup>&frasl;<sub>16</sub></>
        } else if (0.1875 < decimal && decimal <= 0.2500) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}&frac14;</>
        } else if (0.2500 < decimal && decimal <= 0.3125) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>16</sub></>
        } else if (0.3125 < decimal && decimal <= 0.3333) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>3</sub></>
        } else if (0.3333 < decimal && decimal <= 0.3750) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>6</sup>&frasl;<sub>16</sub></>
        } else if (0.3750 < decimal && decimal <= 0.4167) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>12</sub></>
        } else if (0.4167 < decimal && decimal <= 0.4375) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>7</sup>&frasl;<sub>16</sub></>
        } else if (0.4375 < decimal && decimal <= 0.5000) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}&frac12;</>
        } else if (0.5000 < decimal && decimal <= 0.5625) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>9</sup>&frasl;<sub>16</sub></>
        } else if (0.5625 < decimal && decimal <= 0.6250) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>10</sup>&frasl;<sub>16</sub></>
        } else if (0.6250 < decimal && decimal <= 0.6667) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>2</sup>&frasl;<sub>3</sub></>
        } else if (0.6667 < decimal && decimal <= 0.6875) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>11</sup>&frasl;<sub>16</sub></>
        } else if (0.6875 < decimal && decimal <= 0.7500) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>3</sup>&frasl;<sub>4</sub></>
        } else if (0.7500 < decimal && decimal <= 0.8125) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>13</sup>&frasl;<sub>16</sub></>
        } else if (0.8125 < decimal && decimal <= 0.8333) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>6</sub></>
        } else if (0.8333 < decimal && decimal <= 0.8750) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>14</sup>&frasl;<sub>16</sub></>
        } else if (0.8750 < decimal && decimal <= 0.9375) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>15</sup>&frasl;<sub>16</sub></>
        } else if (0.9375 < decimal && decimal <= 0.9999) {
            quantity = <>{Math.round(this.props.ingredientRequirement.quantity)}</>
        } else {
            quantity = <>{Math.round(this.props.ingredientRequirement.quantity)}</>
        }

        var ingredientName = (ingredient.name).toLowerCase()
        var text = <>{quantity} {unitName} {this.props.showAlternatUnit ? this.getAlternateUnit() : null} {ingredientName}
        </>
        if (this.props.strikethrough) {
            text = <s>{text}</s>
        }
        return (
        <div className="display-inline actual-display-inline">
            {text}
        </div>)
    }
    getAlternateUnit() : string {
        let currentUnitType = this.props.units?.find(unit => unit.name === this.props.ingredientRequirement.unit)
        if (currentUnitType == null) {
            return "";
        }
        if (this.props.ingredientRequirement.ingredient.densityKgPerL == null) {
            return ""
        }
        if (currentUnitType?.siType == "Volume") {
            let currentQuantityLiter = this.props.ingredientRequirement.quantity * currentUnitType.siValue;
            let grams = currentQuantityLiter * (this.props.ingredientRequirement.ingredient.densityKgPerL ?? 1.0) * 1000;
            return `(${Math.round(grams)} grams)`
        } else if (currentUnitType.siType == "Weight") {
            let currentQuantityKg = this.props.ingredientRequirement.quantity * currentUnitType.siValue;
            let milliliters = currentQuantityKg / (this.props.ingredientRequirement.ingredient.densityKgPerL ?? 1.0) * 1000
            return `(${Math.round(milliliters)} mL)`
        } else {
            return ""
        }
        return "";
    }
}