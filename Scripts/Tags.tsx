import * as React from "react";
import Autosuggest from "react-autosuggest";
import { stringify, v4 as uuidv4 } from 'uuid';
import { TagList } from "./TagList";

// const theme = require('../wwwroot/autosuggest.css');

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = (suggestion : Autosuggestable) => suggestion.name;

// Use your imagination to render suggestions.
const renderSuggestion = (suggestion : Autosuggestable) => (
    <div>
        {suggestion.name}
    </div>
);
type TagsState = {
    value : string
    suggestions : Autosuggestable[]
    tags : Autosuggestable[]
}
type TagsProps = {
    queryBuilder : (query: string) => string,
    tagsChanged : (newTags : Autosuggestable[]) => void,
    initialTags : Autosuggestable[],
}

export class Tags extends React.Component<TagsProps, TagsState> {

    fetchRequestCount : number;

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

    onChange = (event, { newValue, method }) => {
        switch (method) {
            case 'enter':
                let newTag = {
                    name: this.state.value,
                    id: uuidv4(),
                    isNew: false,
                }
                let newTags = [...this.state.tags, newTag];
                this.setState({
                    tags: newTags,
                    value: ''
                });
                this.props.tagsChanged(newTags);
                break;
        
            default:
                this.setState({
                    value: newValue
                });
                break;
        }
    };

    onSuggestionSelected = (event, {suggestion, suggestionValue}) => {
        console.log(suggestionValue)
        let newTag = {
            name: suggestionValue,
            id: uuidv4(),
            isNew: true,
        }
        let newTags = [...this.state.tags, newTag]
        this.setState({
            tags: newTags,
            value: ''
        });
        this.props.tagsChanged(newTags);
    }

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {
        // primitive primitive rate limiting, should do this server side too
        this.fetchRequestCount = (this.fetchRequestCount + 1) % 2; 
        if (this.fetchRequestCount === 0) {
            let url = this.props.queryBuilder(value);
            fetch(url)
                .then(response => response.json())
                .then(result => {
                    console.log(result);
                    let r = result as Autosuggestable[]
                    let mySuggestions = r.filter((s, i) => {return !this.state.tags.map(t => t.name).includes(s.name)});
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

    onDelete = (index) => {
        let newTags = this.state.tags.filter((_, i) => { return i !== index });
        this.setState({
            tags: newTags
        })
        this.props.tagsChanged(newTags);
        return true
    };

    onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
        switch (event.keyCode) {
          case 13: { //ENTER key
            event.preventDefault();
            let {value, tags } = this.state
            let valueIsEmpty = value.trim() === ""
            let valueExists = -1 !== tags.findIndex((val) => {
                return value.toUpperCase() === val.name.toUpperCase()
            });
            if (!valueExists && !valueIsEmpty) {
                let newTag = {
                    name: value,
                    id: uuidv4(),
                    isNew: true,
                }
                let newTags = [...tags, newTag];
                this.setState({
                    tags: newTags,
                    value: ''
                });
                // console.log("ENTER KEY PRESSED: ")
                console.log(newTag)
                this.props.tagsChanged(newTags);
            }
          }
        }
      };

    render() {
        const { value, suggestions, tags } = this.state;

        // Autosuggest will pass through all these props to the input.
        const inputProps = {
            placeholder: 'Tags',
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