import React from "react";
import { IngredientRequirement, MeasureUnit } from "src/shared/CookTime";
import { UnitPreference, convertQuantity, formatNumber, formatUnitName } from "src/shared/units";

type IngredientDisplayProps = {
    ingredientRequirement: IngredientRequirement
    strikethrough?: boolean,
    showAlternatUnit?: boolean
    units?: MeasureUnit[]
    unitPreference?: UnitPreference
}
export class IngredientDisplay extends React.Component<IngredientDisplayProps, {}> {

    render() {
        const ingredient = this.props.ingredientRequirement.ingredient
        const unitPreference = this.props.unitPreference ?? "recipe";
        const conversion = convertQuantity({
            quantity: this.props.ingredientRequirement.quantity,
            unitName: this.props.ingredientRequirement.unit,
            units: this.props.units,
            preference: unitPreference,
        });

        const unitName = conversion.unitName === "count" ? "" : formatUnitName(conversion.unitName);
        const fraction = this.Fraction(conversion.quantity);
        const quantity = unitPreference === "metric"
            ? <>{formatNumber(conversion.quantity)}</>
            : <>{fraction}</>;

        // Show IR text, but if that's not available then show the ingredient canonical name.
        const ingredientName = (this.props.ingredientRequirement.text ?? ingredient.name.split(";").map(s => s.trim())[0]).toLowerCase()
        let text = <>{quantity} {unitName} {this.props.showAlternatUnit && unitPreference === "recipe" ? this.getAlternateUnit() : null} {ingredientName}
        </>
        if (this.props.strikethrough) {
            text = <s>{text}</s>
        }
        return (
            <div className="display-inline actual-display-inline">
                {text}
            </div>)
    }

    private Fraction(numb: number) {
        const decimal = numb % 1;
        const decimalStr = decimal.toFixed(4);
        const integral = Math.floor(numb)
        let quantity = <>{numb.toString()}</>
        if (decimalStr === "0.0625") {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>16</sub></>;
        } else if (decimalStr === "0.1250") {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>8</sub></>;
        }
        if (0 < decimal && decimal <= 0.0625) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>16</sub></>;
        } else if (0.0625 < decimal && decimal <= 0.1250) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>8</sub></>;
        } else if (0.1250 < decimal && decimal <= 0.1875) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>3</sup>&frasl;<sub>16</sub></>;
        } else if (0.1875 < decimal && decimal <= 0.2500) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}&frac14;</>;
        } else if (0.2500 < decimal && decimal <= 0.3125) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>16</sub></>;
        } else if (0.3125 < decimal && decimal <= 0.3333) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>3</sub></>;
        } else if (0.3333 < decimal && decimal <= 0.3750) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>6</sup>&frasl;<sub>16</sub></>;
        } else if (0.3750 < decimal && decimal <= 0.4167) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>12</sub></>;
        } else if (0.4167 < decimal && decimal <= 0.4375) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>7</sup>&frasl;<sub>16</sub></>;
        } else if (0.4375 < decimal && decimal <= 0.5000) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}&frac12;</>;
        } else if (0.5000 < decimal && decimal <= 0.5625) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>9</sup>&frasl;<sub>16</sub></>;
        } else if (0.5625 < decimal && decimal <= 0.6250) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>10</sup>&frasl;<sub>16</sub></>;
        } else if (0.6250 < decimal && decimal <= 0.6667) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>2</sup>&frasl;<sub>3</sub></>;
        } else if (0.6667 < decimal && decimal <= 0.6875) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>11</sup>&frasl;<sub>16</sub></>;
        } else if (0.6875 < decimal && decimal <= 0.7500) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>3</sup>&frasl;<sub>4</sub></>;
        } else if (0.7500 < decimal && decimal <= 0.8125) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>13</sup>&frasl;<sub>16</sub></>;
        } else if (0.8125 < decimal && decimal <= 0.8333) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>5</sup>&frasl;<sub>6</sub></>;
        } else if (0.8333 < decimal && decimal <= 0.8750) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>14</sup>&frasl;<sub>16</sub></>;
        } else if (0.8750 < decimal && decimal <= 0.9375) {
            quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>15</sup>&frasl;<sub>16</sub></>;
        } else if (0.9375 < decimal && decimal <= 0.9999) {
            quantity = <>{Math.round(numb)}</>;
        } else {
            quantity = <>{numb}</>;
        }
        return quantity;
    }

    getAlternateUnit(): any {
        const currentUnitType = this.props.units?.find(unit => unit.name === this.props.ingredientRequirement.unit)
        if (currentUnitType == null) {
            return "";
        }
        if (this.props.ingredientRequirement.ingredient.densityKgPerL == null) {
            return ""
        }
        if (currentUnitType?.siType == "volume") {
            const currentQuantityLiter = this.props.ingredientRequirement.quantity * currentUnitType.siValue;
            const grams = currentQuantityLiter * (this.props.ingredientRequirement.ingredient.densityKgPerL ?? 1.0) * 1000;
            return `(${Math.round(grams)} grams)`
        } else if (currentUnitType.siType == "weight") {
            const currentQuantityKg = this.props.ingredientRequirement.quantity * currentUnitType.siValue;
            const milliliters = currentQuantityKg / (this.props.ingredientRequirement.ingredient.densityKgPerL ?? 1.0) * 1000
            // 45 ml is approximately the cutoff of 3 tablespoons.
            if (milliliters <= 15) {
                const millisPerTeaspoon = 4.9289317406874
                return <>({this.Fraction(milliliters / millisPerTeaspoon)} Tsp)</>
            }
            else if (milliliters < 45) {
                const millisPerTablespoon = 14.7868
                return <>({this.Fraction(milliliters / millisPerTablespoon)} Tbsp)</>
                // 3785 ml is about one gallon, don't measure 
            } else if (milliliters < 3785) {
                const millisPerCup = 236.588
                return <>({this.Fraction(milliliters / millisPerCup)} cups)</>
            } else {
                return `(${Math.round(milliliters)} mL)`
            }
        } else {
            return ""
        }
    }
}
