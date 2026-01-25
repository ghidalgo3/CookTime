import { Button, Col, Row, Spinner } from "react-bootstrap";
import { UserDetails } from "src/shared/AuthenticationProvider";
import { MultiPartRecipe, generateRecipeImage } from "src/shared/CookTime";

export type RecipeEditButtonsProps = {
    user: UserDetails | null,
    recipe: MultiPartRecipe,
    edit: boolean,
    operationInProgress: boolean,
    onSave: () => void,
    onCancel: () => void,
    onDelete: () => void,
    onToggleEdit: () => void,
    onAddtoCard: () => void
}
export function RecipeEditButtons({ user, recipe, edit, operationInProgress, onSave, onCancel, onDelete, onToggleEdit, onAddtoCard }: RecipeEditButtonsProps) {
    let userSignedIn = user !== null;
    let canEdit = userSignedIn && user!.id === recipe.owner?.id || user?.roles.includes("Administrator");
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
                        <div data-bs-toggle="tooltip" data-bs-placement="bottom" title="Sign in to add recipes to your cart">
                            <Button
                                className="recipe-edit-buttons"
                                disabled={!userSignedIn}
                                onClick={(event) => onAddtoCard()}>
                                Add to Groceries
                            </Button>
                        </div>
                        :
                        <Button
                            className="recipe-edit-buttons"
                            disabled={!userSignedIn}
                            onClick={(event) => onAddtoCard()}>
                            Add to Groceries
                        </Button>}
                </Col>
            </Row>
        </Col>;
    return edit ?
        editButtons
        :
        defaultButtons;
}
