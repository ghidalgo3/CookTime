import React from "react";
import Autosuggest from "react-autosuggest";

export type IngredientInputProps = {
    query: (value : string) => string,
    ingredient : Ingredient,
    onSelect: (ingredient : Ingredient) => void,
}

type IngredientInputState = {
    value: string,
    suggestions: Ingredient[],
    selection: Ingredient | null
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
                selection: props.ingredient
            };
        } else {
            this.state = {
                value: '',
                suggestions: [],
                selection: null
            };
        }
        this.fetchRequestCount = 0;
    }

    getSuggestionValue = (suggestion : Ingredient) => suggestion.name;

    // // Use your imagination to render suggestions.
    renderSuggestion = (suggestion : Ingredient) => (
        <div>
            {suggestion.name}
        </div>
    );

    onChange = (event : any, { newValue, method }) => {
        switch (method) {
            case 'enter':
                this.setState({
                    selection: null,
                    // selection: [...this.state.skillList, this.state.value],
                    value: ''
                });
                break;
            default:
                this.setState({
                    value: newValue
                });
                break;
        }
    };

    onSuggestionSelected = (event, {suggestion, suggestionValue}) => {
        this.props.onSelect(suggestion as Ingredient);
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

    render() {
        const { value, suggestions } = this.state;

        // Autosuggest will pass through all these props to the input.
        const inputProps = {
            placeholder: 'Ingredient',
            value,
            onChange: this.onChange,
            onKeyDown: this.onKeyDown,
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
                />
            </div>
        );
    }
}