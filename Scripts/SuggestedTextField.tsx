class SkillList extends React.Component<{skills: string[]}, SkillListState> {
    fetchRequestCount : number;
    constructor(props: {skills: string[]}) {
        super(props);

        // Autosuggest is a controlled component.
        // This means that you need to provide an input value
        // and an onChange handler that updates this value (see below).
        // Suggestions also need to be provided to the Autosuggest,
        // and they are initially empty because the Autosuggest is closed.
        this.state = {
            value: '',
            suggestions: [],
            skillList: this.props.skills
        };
        this.fetchRequestCount = 0;
    }

    onChange = (event, { newValue, method }) => {
        switch (method) {
            case 'enter':
                this.setState({
                    skillList: [...this.state.skillList, this.state.value],
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
        this.setState({
            skillList: [...this.state.skillList, suggestionValue],
            value: ''
        });
    }

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {
        // primitive primitive rate limiting, should do this server side too
        this.fetchRequestCount = (this.fetchRequestCount + 1) % 2; 
        if (this.fetchRequestCount === 0) {
            $.ajax({
                url: `/Project/Skills?input=${value}`,
                type: "GET",
                success: (result) => {
                    console.log(result);
                    let mySuggestions = result.filter((s, i) => {return !this.state.skillList.includes(s)});
                    this.setState({
                        suggestions: mySuggestions
                    })
                }
            });
        }
    };

    // Autosuggest will call this function every time you need to clear suggestions.
    onSuggestionsClearRequested = () => {
        this.setState({
            suggestions: []
        });
    };

    onDelete = (index) => {
        let newSkills = this.state.skillList.filter((_, i) => { return i !== index });
        this.setState({
            skillList: newSkills
        })
        return true
    };

    onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
        switch (event.keyCode) {
          case 13: { //ENTER key
            event.preventDefault();
            let {value, skillList} = this.state
            let valueIsEmpty = value.trim() === ""
            let valueExists = -1 !== skillList.findIndex((val) => {
                return value.toUpperCase() === val.toUpperCase()
            });
            if (!valueExists && !valueIsEmpty) {
                this.setState({
                    skillList: [...skillList, value],
                    value: ''
                });
            }
          }
        }
      };

    render() {
        const { value, suggestions, skillList } = this.state;

        // Autosuggest will pass through all these props to the input.
        const inputProps = {
            placeholder: 'Skill',
            value,
            onChange: this.onChange,
            onKeyDown: this.onKeyDown,
        };

        // Finally, render it!
        return (
            <div>
                <TagList
                    data={skillList}
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