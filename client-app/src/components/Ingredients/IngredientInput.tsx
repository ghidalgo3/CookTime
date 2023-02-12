import React from "react";
import Autosuggest from "react-autosuggest";
import { Badge } from "react-bootstrap";
import { Ingredient, IngredientRequirement } from "src/shared/CookTime";
import { v4 as uuidv4 } from 'uuid';

export type IngredientInputProps = {
  query: (value: string) => string,
  ingredient: Ingredient | null,
  text: string,
  isNew: boolean,
  onSelect: (text: string, ingredient: Ingredient, isNew: boolean) => void,
  className: string | null,
  currentRequirements: IngredientRequirement[]
}

type IngredientInputState = {
  value: string,
  suggestions: Ingredient[],
  selection: Ingredient | null,
  newIngredient: boolean
}

export class IngredientInput extends React.Component<IngredientInputProps, IngredientInputState> {
  fetchRequestCount: number;
  constructor(props: IngredientInputProps) {
    super(props);

    // Autosuggest is a controlled component.
    // This means that you need to provide an input value
    // and an onChange handler that updates this value (see below).
    // Suggestions also need to be provided to the Autosuggest,
    // and they are initially empty because the Autosuggest is closed.
    if (props.ingredient !== null) {
      // console.log(props)
      this.state = {
        value: props.text ?? props.ingredient.name,
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

  getSuggestionValue = (suggestion: Ingredient) => suggestion.name;

  // // Use your imagination to render suggestions.
  renderSuggestion = (suggestion: Ingredient) => (
    <span>{suggestion.name}</span>
  );

  // called every time a key is pressed or when the user presses enter
  onChange = (event: any, { newValue, method } : {newValue: any, method: any}) => {
    switch (method) {
      case 'enter':
        // console.log(`Enter ${newValue}`)
        // TODO here
        break;
      default:
        // console.log(`Default ${newValue}`)
        var possibleSuggestions = this.state.suggestions.filter(suggestion =>
          suggestion.name.toUpperCase().includes(newValue));
        if (possibleSuggestions.length == 1) {
          this.setState({
            selection: possibleSuggestions[0],
            value: newValue,
          });
          this.props.onSelect(newValue, possibleSuggestions[0], false)
        } else {
          // whatever the user has typed may be a subset of an existing ingredient
          // lol don't do that
          // never before seen ingredient
          this.setState({
            value: newValue
          });
        }
        break;
    }
  };

  onSuggestionSelected = (event: any, { suggestion, suggestionValue }: {suggestion: any, suggestionValue: any}) => {
    this.props.onSelect(suggestionValue, suggestion as Ingredient, false);
    this.setState({
      selection: suggestion as Ingredient,
      value: suggestion.name
    });
  }

  // Autosuggest will call this function every time you need to update suggestions.
  // You already implemented this logic above, so just use it.
  onSuggestionsFetchRequested = ({ value } : {value : any}) => {
    fetch(this.props.query(value as string))
      .then(response => response.json())
      .then(suggestions => {
        var ingredients = suggestions as Ingredient[];
        // Do not suggest ingredients used in some other ingredient requirement
        // for this recipe component
        if (this.props.currentRequirements.length > 0) {
          ingredients = ingredients.filter(ingredient => {
            return !this.props.currentRequirements.some(ir => ir.ingredient.name === ingredient.name);
          })
        }
        this.setState({
          suggestions: ingredients
        })
      })
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
    console.log(`Selecting ingredient ${this.state.value}`)
    if (this.state.value === null || this.state.value === '') {
      return;
    }

    var possibleSuggestions = this.state.suggestions.filter(suggestion =>
      suggestion.name.toUpperCase() === this.state.value.trim().toUpperCase());
    console.log(possibleSuggestions);
    if (possibleSuggestions.length === 1) {
      // one ingredient matches
      this.setState({
        selection: possibleSuggestions[0],
      });
      let s = possibleSuggestions[0];
      this.props.onSelect(s.name, s, false);
    } else if (possibleSuggestions.length === 0) {
      this.onNewIngredient();
    } else {

    }
  }

  onNewIngredient = () => {
    var newIngredient = { name: this.state.value, id: uuidv4(), isNew: true, densityKgPerL: 1.0 };
    this.setState({
      selection: newIngredient,
      newIngredient: true
    });
    this.props.onSelect(this.state.value, newIngredient, true);
  }

  render() {
    const { value, suggestions } = this.state;

    let badgeComponent = this.state.newIngredient ?
      <Badge className="new-curr-badge" bg="">New</Badge> :
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
      <div className="ingredient-input">
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