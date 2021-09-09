import * as React from 'react';
import * as ReactDOM from 'react-dom';

console.log("Hello world!");

let reactComponent = (recipeId : string) =>
{
    let content = `Hello ${recipeId}`;
    return <p>{content}</p>
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id");
fetch(`/api/recipe/${recipeId}`).then(response => console.log(response))
ReactDOM.render(reactComponent(recipeId as string), recipeContainer);