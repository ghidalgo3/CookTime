import React from "react";
import Autosuggest from "react-autosuggest";
import { Badge } from "react-bootstrap";
import { v4 as uuidv4 } from 'uuid';
const theme = require('../wwwroot/css/site.css');

export type IngredientInputProps = {
    query: (value : string) => string,
    ingredient : Ingredient | null,
    isNew: boolean,
    onSelect: (ingredient : Ingredient, isNew: boolean) => void,
    className: string | null,
}

type IngredientInputState = {
    value: string,
    suggestions: Ingredient[],
    selection: Ingredient | null,
    newIngredient: boolean
}

export class IngredientInput extends React.Component<IngredientInputProps, IngredientInputState> {
    fetchRequestCount : number;
    constructor(props: IngredientInputProps) {
        super(props);

        // Autosuggest is a controlled component.
        // This means that you need to provide an input value
        // and an onChange handler that updates this value (see below).
        // Suggestions also need to be provided to the Autosuggest,
        // and they are initially empty because the Autosuggest is closed.
        if (props.ingredient !== null) {
            this.state = {
                value: props.ingredient.name,
                suggestions: [],
                selection: props.ingredient,
                newIngredient: this.props.isNew
                
            };
        } else {
            this.state = {
                value: '',
                suggestions: [],
                selection: null,
                newIngredient: this.props.isNew
            };
        }
        this.fetchRequestCount = 0;
    }

    getSuggestionValue = (suggestion : Ingredient) => suggestion.name;

    // // Use your imagination to render suggestions.
    renderSuggestion = (suggestion : Ingredient) => (
        <span>{suggestion.name}</span>
    );

    // called every time a key is pressed or when the user presses enter
    onChange = (event : any, { newValue, method }) => {
        switch (method) {
            case 'enter':
                break;
            default:
                // this.setState({
                //     value: newValue,
                // })

                var possibleSuggestions = this.state.suggestions.filter(suggestion => suggestion.name.toUpperCase().includes(newValue));
                if (possibleSuggestions.length == 1) {
                    // one ingredient matches
                    this.setState({
                        selection: possibleSuggestions[0],
                        value: newValue,
                    });
                    this.props.onSelect(possibleSuggestions[0], false)
                } else {
                    // whatever the user has typed may be a subset of an existing ingredient
                    // lol don't do that
                    // never before seen ingredient
                    // var newIngredient = {name: newValue, id: uuidv4()};
                    this.setState({
                        // selection: newIngredient,
                        value: newValue
                    });
                    // this.props.onSelect(newIngredient)
                }
                break;
        }
    };

    onSuggestionSelected = (event, {suggestion, suggestionValue}) => {
        this.props.onSelect(suggestion as Ingredient, false);
        this.setState({
            selection: suggestion as Ingredient,
            value: suggestion.name
        });
    }

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {
        // primitive primitive rate limiting, should do this server side too
        this.fetchRequestCount = (this.fetchRequestCount + 1) % 2; 
        if (this.fetchRequestCount === 0) {
            fetch(this.props.query(value as string))
                .then(response => response.json())
                .then(suggestions => {
                    this.setState({
                        suggestions: suggestions as Ingredient[]
                    })
                })
        }
    };

    // Autosuggest will call this function every time you need to clear suggestions.
    onSuggestionsClearRequested = () => {
        this.setState({
            suggestions: []
        });
    };

    onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
        switch (event.code) {
          case "Enter": { //ENTER key
            event.preventDefault();
            this.selectIngredient();
            // let {value} = this.state
            // let valueIsEmpty = value.trim() === ""
            // let valueExists = -1 !== skillList.findIndex((val) => {
            //     return value.toUpperCase() === val.toUpperCase()
            // });
            // if (!valueExists && !valueIsEmpty) {
            //     this.setState({
            //         skillList: [...skillList, value],
            //         value: ''
            //     });
            // }
          }
        }
      };

    selectIngredient = () => {
        var possibleSuggestions = this.state.suggestions.filter(suggestion => suggestion.name.toUpperCase() === this.state.value.toUpperCase());
        if (possibleSuggestions.length == 1) {
            // one ingredient matches
            this.setState({
                selection: possibleSuggestions[0],
            });
            this.props.onSelect(possibleSuggestions[0], false);
        } else if (possibleSuggestions.length == 0) {
            this.onNewIngredient();
        } else {

        }
    }

    onNewIngredient = () => {
        var newIngredient = { name: this.state.value, id: uuidv4(), isNew: true };
        this.setState({
            selection: newIngredient,
            newIngredient: true
        });
        this.props.onSelect(newIngredient, true);
    }

    render() {
        const { value, suggestions } = this.state;
        
        let badgeComponent = this.state.newIngredient ?
            <Badge className="new-curr-badge" bg="secondary">New</Badge> :
            null;

        // Autosuggest will pass through all these props to the input.
        const inputProps = {
            placeholder: 'Ingredient',
            value,
            onChange: this.onChange,
            onKeyDown: this.onKeyDown,
            onBlur: this.selectIngredient,
            className: `form-control ${this.props.className} ${this.state.newIngredient ? "padding-left-58" : "padding-left-12"}`,
        };

        // Finally, render it!
        return (
            <div>
                <Autosuggest
                    suggestions={suggestions}
                    onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
                    onSuggestionsClearRequested={this.onSuggestionsClearRequested}
                    getSuggestionValue={this.getSuggestionValue}
                    renderSuggestion={this.renderSuggestion}
                    inputProps={inputProps}
                    onSuggestionSelected={this.onSuggestionSelected}
                    // theme={theme}
                />
                {badgeComponent}
            </div>
        );
    }
}

export class IngredientDisplay extends React.Component<{ingredientRequirement: IngredientRequirement}, {}> {
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
        switch ((this.props.ingredientRequirement.quantity % 1).toFixed(4)) {
            case "0.0625":
                quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>16</sub></>
                break;
            case "0.1250":
                quantity = <>{integral != 0 ? `${integral} ` : ""}<sup>1</sup>&frasl;<sub>8</sub></>
                break;
            case "0.2500":
                quantity = <>{integral != 0 ? `${integral} ` : ""}&frac14;</>
                break;
            case "0.3333":
                quantity = <>{integral != 0 ? `${integral} ` : ""}&frac13;</>
                break;
            case "0.5000":
                quantity = <>{integral != 0 ? `${integral} ` : ""}&frac12;</>
                break;
            case "0.7500":
                quantity = <>{integral != 0 ? `${integral} ` : ""}&frac34;</>
                break;
            default:
                break;
        }
        var ingredientName = (ingredient.name).toLowerCase()
        return (<div className="display-inline actual-display-inline">{quantity} {unitName} {ingredientName}</div>)
    }
}