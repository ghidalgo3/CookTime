import * as React from 'react';
import { Button, Col, Form, FormControl, FormText, Row } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';
import { IngredientInput } from './IngredientInput';


export class Ingredients extends React.Component<{}, {}>
{
    constructor(props) {
        super(props);
    }

    render() {
        return <IngredientInput
        isNew={false}
        query={text => `/api/recipe/ingredients?name=${text}`}
        ingredient={null}
        onSelect={(i, isNew) => console.log(i)}
        />
    }
}

const recipeContainer = document.querySelector('#ingredients');
ReactDOM.render(
    <Ingredients />,
    recipeContainer);