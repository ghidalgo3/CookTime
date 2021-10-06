import * as React from 'react';
import { Button, Col, Form, FormControl, FormText, Row } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import * as ReactDOM from 'react-dom';


export class Ingredients extends React.Component<{}, {}>
{
    constructor(props) {
        super(props);
    }

    render() {
        return <div>Hello!</div>
    }
}

const recipeContainer = document.querySelector('#ingredients');
ReactDOM.render(
    <Ingredients />,
    recipeContainer);