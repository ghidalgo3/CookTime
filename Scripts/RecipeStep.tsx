import React from "react";

export class Step extends React.Component<{recipe: Recipe, recipeStep: RecipeStep}, {}>
{
    constructor(props) {
        super(props);
    }

    render() {
        return <p>{this.props.recipeStep.text}</p>
    }
}