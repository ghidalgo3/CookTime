import React, { useState } from 'react';
import Autosuggest from 'react-autosuggest';

interface IngredientAutosuggestProps {
    placeholder: string;
    value: string;
    onSelect: (ingredientId: string, ingredientName: string) => void;
    excludeId?: string; // ID to exclude from results (e.g., the current ingredient)
}

// Matches IngredientAutosuggestDto from backend
interface IngredientSearchResult {
    id: string;
    name: string;
    isNew: boolean;
}

export default function IngredientAutosuggest({ placeholder, value, onSelect, excludeId }: IngredientAutosuggestProps) {
    const [inputValue, setInputValue] = useState<string>(value || '');
    const [suggestions, setSuggestions] = useState<IngredientSearchResult[]>([]);

    const getSuggestionValue = (suggestion: IngredientSearchResult) => {
        return suggestion.name;
    };

    const renderSuggestion = (suggestion: IngredientSearchResult) => (
        <div>
            <div style={{ fontWeight: 'bold' }}>{suggestion.name}</div>
        </div>
    );

    const onSuggestionsFetchRequested = async ({ value: query }: { value: string }) => {
        if (query.length < 2) {
            setSuggestions([]);
            return;
        }

        try {
            const response = await fetch(`/api/recipe/ingredients?name=${encodeURIComponent(query)}`);
            if (response.ok) {
                let results = await response.json() as IngredientSearchResult[];
                // Exclude the current ingredient from results
                if (excludeId) {
                    results = results.filter(r => r.id !== excludeId);
                }
                setSuggestions(results);
            }
        } catch (error) {
            console.error('Error searching ingredients:', error);
            setSuggestions([]);
        }
    };

    const onSuggestionsClearRequested = () => {
        setSuggestions([]);
    };

    const onSuggestionSelected = (
        _event: React.FormEvent,
        { suggestion }: { suggestion: IngredientSearchResult }
    ) => {
        onSelect(suggestion.id, suggestion.name);
        setInputValue(suggestion.name);
    };

    const onChange = (_event: React.FormEvent, { newValue }: { newValue: string }) => {
        setInputValue(newValue);
    };

    const inputProps = {
        placeholder,
        value: inputValue,
        onChange,
        className: 'form-control',
    };

    return (
        <Autosuggest
            suggestions={suggestions}
            onSuggestionsFetchRequested={onSuggestionsFetchRequested}
            onSuggestionsClearRequested={onSuggestionsClearRequested}
            getSuggestionValue={getSuggestionValue}
            renderSuggestion={renderSuggestion}
            inputProps={inputProps}
            onSuggestionSelected={onSuggestionSelected}
        />
    );
}
