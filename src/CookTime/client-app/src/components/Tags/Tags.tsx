import * as React from "react";
import Autosuggest from "react-autosuggest";
import { Autosuggestable } from "src/shared/CookTime";
import { TagList } from "./TagList";

// const theme = require('../wwwroot/autosuggest.css');

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = (suggestion: Autosuggestable) => suggestion.name;

// Use your imagination to render suggestions.
const renderSuggestion = (suggestion: Autosuggestable) => (
  <div>
    {suggestion.name}
  </div>
);
type TagsState = {
  value: string
  suggestions: Autosuggestable[]
  tags: Autosuggestable[]
}
type TagsProps = {
  queryBuilder: (query: string) => string,
  tagsChanged: (newTags: Autosuggestable[]) => void,
  initialTags: Autosuggestable[],
}

export class Tags extends React.Component<TagsProps, TagsState> {

  fetchRequestCount: number;

  constructor(props: TagsProps) {
    super(props);

    // Autosuggest is a controlled component.
    // This means that you need to provide an input value
    // and an onChange handler that updates this value (see below).
    // Suggestions also need to be provided to the Autosuggest,
    // and they are initially empty because the Autosuggest is closed.
    this.state = {
      value: '',
      suggestions: [],
      tags: this.props.initialTags
    };
    this.fetchRequestCount = 0;
  }

  onChange = (event: any, { newValue, method }: any) => {
    // Only allow typing to filter suggestions, not creating new tags
    if (method !== 'enter') {
      this.setState({
        value: newValue
      });
    }
  };

  onSuggestionSelected = (event: any, { suggestion, suggestionValue }: any) => {
    console.log('Tag selected:', suggestionValue, 'ID:', suggestion.id)
    const newTag = {
      name: suggestionValue,
      id: suggestion.id,
      isNew: false,
    }
    const newTags = [...this.state.tags, newTag]
    console.log('All tags after selection:', newTags);
    this.setState({
      tags: newTags,
      value: ''
    });
    this.props.tagsChanged(newTags);
  }

  // Autosuggest will call this function every time you need to update suggestions.
  // You already implemented this logic above, so just use it.
  onSuggestionsFetchRequested = ({ value }: any) => {
    // primitive primitive rate limiting, should do this server side too
    this.fetchRequestCount = (this.fetchRequestCount + 1) % 2;
    if (this.fetchRequestCount === 0) {
      const url = this.props.queryBuilder(value);
      fetch(url)
        .then(response => response.json())
        .then(result => {
          console.log(result);
          const r = result as Autosuggestable[]
          const mySuggestions = r.filter((s, i) => { return !this.state.tags.map(t => t.name).includes(s.name) });
          this.setState({
            suggestions: mySuggestions
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

  onDelete = (index: number) => {
    const newTags = this.state.tags.filter((_, i) => { return i !== index });
    this.setState({
      tags: newTags
    })
    this.props.tagsChanged(newTags);
    return true
  };

  onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    // Prevent Enter key from doing anything - users must select from suggestions
    if (event.keyCode === 13) {
      event.preventDefault();
    }
  };

  render() {
    const { value, suggestions, tags } = this.state;

    // Autosuggest will pass through all these props to the input.
    const inputProps = {
      placeholder: 'Categories',
      value,
      onChange: this.onChange,
      onKeyDown: this.onKeyDown,
      className: 'form-control width-100',
    };

    // Finally, render it!
    return (
      <div className="width-100">
        <TagList
          data={tags.map(t => t.name)}
          onDelete={this.onDelete} />
        <Autosuggest
          suggestions={suggestions}
          // theme={theme}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
          getSuggestionValue={getSuggestionValue}
          renderSuggestion={renderSuggestion}
          inputProps={inputProps}
          onSuggestionSelected={this.onSuggestionSelected}
        />
      </div>
    );
  }
}