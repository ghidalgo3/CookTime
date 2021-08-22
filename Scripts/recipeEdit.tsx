import * as React from 'react';
import * as ReactDOM from 'react-dom';

console.log("Hello world!");

let reactComponent = () =>
{
    return <p>Hello world1</p>
}

const domContainer = document.querySelector('#react');
ReactDOM.render(reactComponent(), domContainer);