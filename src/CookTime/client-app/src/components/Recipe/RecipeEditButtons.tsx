import { useEffect, useState } from "react";
import { Button, Col, Dropdown, Row, Spinner } from "react-bootstrap";
import { UserDetails } from "src/shared/AuthenticationProvider";
import { MultiPartRecipe, RecipeList, generateRecipeImage, getLists } from "src/shared/CookTime";

export type RecipeEditButtonsProps = {
    user: UserDetails | null,
    recipe: MultiPartRecipe,
    edit: boolean,
    operationInProgress: boolean,
    onSave: () => void,
    onCancel: () => void,
    onDelete: () => void,
    onToggleEdit: () => void,
    onAddToList: (listName: string) => void
}
export function RecipeEditButtons({ user, recipe, edit, operationInProgress, onSave, onCancel, onDelete, onToggleEdit, onAddToList }: RecipeEditButtonsProps) {
    let userSignedIn = user !== null;
    let canEdit = userSignedIn && user!.id === recipe.owner?.id || user?.roles.includes("Administrator");
    const [lists, setLists] = useState<RecipeList[]>([]);

    useEffect(() => {
        if (userSignedIn) {
            getLists().then(setLists).catch(console.error);
        }
    }, [userSignedIn]);

    const editButtons =
        <Col>
            <Row>
                <Col>
                    <Button className="recipe-edit-buttons font-weight-600" onClick={_ => onSave()}>
                        {operationInProgress ?
                            <Spinner
                                as="span"
                                animation="border"
                                size="sm"
                                role="status"
                                aria-hidden="true" />
                            : "Save"}
                    </Button>
                </Col>
                <Col>
                    <Button className="recipe-edit-buttons margin-bottom-15" onClick={_ => onCancel()}>Cancel</Button>
                </Col>
                <Col>
                    <Button variant="danger" className="recipe-edit-buttons margin-bottom-15" onClick={_ => onDelete()}>Delete</Button>
                </Col>
                {user?.roles.includes("Administrator") &&
                    <Col>
                        <Button
                            className="recipe-edit-buttons margin-bottom-15"
                            onClick={_ => {
                                generateRecipeImage(recipe.id).then(_ => {
                                    alert("Image generated");
                                });
                            }}>
                            Generate Image
                        </Button>
                    </Col>
                }
            </Row>
        </Col>;

    // Filter out Favorites from the list dropdown, and ensure Groceries is first
    const sortedLists = lists
        .filter(l => l.name !== "Favorites")
        .sort((a, b) => {
            if (a.name === "Groceries") return -1;
            if (b.name === "Groceries") return 1;
            return a.name.localeCompare(b.name);
        });

    const defaultButtons =
        <Col>
            <Row>
                <Col>
                    {(!userSignedIn || !canEdit) ?
                        <div data-bs-toggle="tooltip" data-bs-placement="bottom" title="Sign in to modify your own recipes">
                            <Button
                                className="recipe-edit-buttons"
                                disabled={!userSignedIn || !canEdit}
                                onClick={(event) => onToggleEdit()}>
                                Edit
                            </Button>
                        </div>
                        :
                        <Button
                            className="recipe-edit-buttons"
                            disabled={!userSignedIn || !canEdit}
                            onClick={(event) => onToggleEdit()}>
                            Edit
                        </Button>}
                </Col>
                <Col>
                    {!userSignedIn ?
                        <div data-bs-toggle="tooltip" data-bs-placement="bottom" title="Sign in to add recipes to a list">
                            <Button
                                className="recipe-edit-buttons"
                                disabled={!userSignedIn}>
                                Add to List
                            </Button>
                        </div>
                        :
                        <Dropdown>
                            <Dropdown.Toggle
                                className="recipe-edit-buttons"
                                variant="primary"
                                id="add-to-list-dropdown"
                            >
                                Add to List
                            </Dropdown.Toggle>
                            <Dropdown.Menu>
                                {sortedLists.map(list => (
                                    <Dropdown.Item
                                        key={list.id}
                                        onClick={() => onAddToList(list.name)}
                                    >
                                        {list.name}
                                    </Dropdown.Item>
                                ))}
                                {sortedLists.length === 0 && (
                                    <Dropdown.Item disabled>No lists available</Dropdown.Item>
                                )}
                            </Dropdown.Menu>
                        </Dropdown>
                    }
                </Col>
            </Row>
        </Col>;
    return edit ?
        editButtons
        :
        defaultButtons;
}
