import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { ISpinButtonStyles, IStackTokens, Label, PrimaryButton, SpinButton, Stack, TextField } from '@fluentui/react';
import { initializeIcons } from '@fluentui/react/lib/Icons';

initializeIcons(/* optional base url */);
console.log("Hello world!");
const stackTokens: IStackTokens = { childrenGap: 20 };
// By default the field grows to fit available width. Constrain the width instead.
const styles: Partial<ISpinButtonStyles> = { spinButtonWrapper: { width: 75 } };

let reactComponent = (recipeId : string) =>
{
    let content = `Hello ${recipeId}`;
    return (
        <Stack tokens={stackTokens}>
            <TextField
                label="Recipe name"
                placeholder="Name" />
            <SpinButton
                label="Duration (minutes)"
                defaultValue="5"
                min={5}
                max={120}
                step={5}
                incrementButtonAriaLabel="Increase value by 5"
                decrementButtonAriaLabel="Decrease value by 5"
                styles={styles}/>
        </Stack>
    )
}

const recipeContainer = document.querySelector('#recipeEdit');
var recipeId = recipeContainer?.getAttribute("data-recipe-id");
fetch(`/api/recipe/${recipeId}`).then(response => console.log(response))
ReactDOM.render(reactComponent(recipeId as string), recipeContainer);