import React, { useState } from 'react';
import Autosuggest from 'react-autosuggest';

interface NutritionAutosuggestProps {
    placeholder: string;
    value: string | number;
    onSelect: (ndbNumber: number | null, gtinUpc: string | null) => void;
    fieldType: 'ndb' | 'gtin';
}

interface NutritionSearchResult {
    id: string;
    name: string;
    ndbNumber: number | null;
    gtinUpc: string | null;
    dataset: string;
}

export default function NutritionAutosuggest({ placeholder, value, onSelect, fieldType }: NutritionAutosuggestProps) {
    const [inputValue, setInputValue] = useState<string>(
        fieldType === 'ndb'
            ? (typeof value === 'number' && value > 0 ? value.toString() : '')
            : (typeof value === 'string' ? value : '')
    );
    const [suggestions, setSuggestions] = useState<NutritionSearchResult[]>([]);

    const getSuggestionValue = (suggestion: NutritionSearchResult) => {
        return fieldType === 'ndb'
            ? (suggestion.ndbNumber?.toString() ?? '')
            : (suggestion.gtinUpc ?? '');
    };

    const renderSuggestion = (suggestion: NutritionSearchResult) => (
        <div>
            <div style={{ fontWeight: 'bold' }}>{suggestion.name}</div>
            <div style={{ fontSize: '0.8em', color: '#666' }}>
                {fieldType === 'ndb'
                    ? `NDB: ${suggestion.ndbNumber || 'N/A'}`
                    : `GTIN/UPC: ${suggestion.gtinUpc || 'N/A'}`
                }
                {' • '}{suggestion.dataset}
            </div>
        </div>
    );

    const onSuggestionsFetchRequested = async ({ value: query }: { value: string }) => {
        if (query.length < 2) {
            setSuggestions([]);
            return;
        }

        try {
            // Filter by dataset: NDB field shows sr_legacy, GTIN field shows branded
            const dataset = fieldType === 'ndb' ? 'usda_sr_legacy' : 'usda_branded';
            const response = await fetch(`/api/nutrition/search?query=${encodeURIComponent(query)}&dataset=${dataset}`);
            if (response.ok) {
                const results = await response.json() as NutritionSearchResult[];
                setSuggestions(results);
            }
        } catch (error) {
            console.error('Error searching nutrition facts:', error);
            setSuggestions([]);
        }
    };

    const onSuggestionsClearRequested = () => {
        setSuggestions([]);
    };

    const onSuggestionSelected = (
        _event: React.FormEvent,
        { suggestion }: { suggestion: NutritionSearchResult }
    ) => {
        if (fieldType === 'ndb') {
            onSelect(suggestion.ndbNumber, null);
            setInputValue(suggestion.ndbNumber?.toString() ?? '');
        } else {
            onSelect(null, suggestion.gtinUpc);
            setInputValue(suggestion.gtinUpc ?? '');
        }
    };

    const onChange = (_event: React.FormEvent, { newValue }: { newValue: string }) => {
        setInputValue(newValue);

        // If user is typing a number directly for NDB, update the parent field
        if (fieldType === 'ndb' && /^\d+$/.test(newValue)) {
            onSelect(parseInt(newValue), null);
        } else if (fieldType === 'gtin') {
            onSelect(null, newValue || null);
        }
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