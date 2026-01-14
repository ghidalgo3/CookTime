import React from "react";
import { Button, Col, Form, FormControl, Row } from "react-bootstrap";
import { Step } from "./RecipeStep";
import ReactSortable from "react-sortablejs";
import { MultiPartRecipe, Recipe, RecipeComponent, RecipeStep } from "src/shared/CookTime";

type RecipeStepListProps = {
    recipe: Recipe | MultiPartRecipe,
    newServings: number,
    multipart: boolean,
    component?: RecipeComponent,
    onDeleteStep: (i: number, component?: RecipeComponent) => void,
    onChange: (newSteps: RecipeStep[]) => void,
    onNewStep: () => void,
    edit: boolean
}

export class RecipeStepList extends React.Component<RecipeStepListProps, {}> {
    constructor(props: RecipeStepListProps) {
        super(props);
    }

    stepEditRow(i: RecipeStep, idx: number): any {
        return (
            <Row>
                <Col className="col d-flex align-items-center get-smaller">
                    <i className="margin-right-10 drag-handle fas fa-solid fa-grip-vertical"></i>
                    <FormControl
                        as="textarea"
                        rows={4}
                        className="margin-bottom-8"
                        key={idx}
                        type="text"
                        placeholder="Recipe step"
                        value={i.text}
                        onChange={e => {
                            if (!this.props.multipart) {
                                let newSteps = Array.from((this.props.recipe as Recipe).steps ?? []);
                                newSteps[idx].text = e.target.value;
                                this.props.onChange(newSteps);
                            } else {
                                let newSteps = Array.from((this.props.component!).steps ?? []);
                                newSteps[idx].text = e.target.value;
                                this.props.onChange(newSteps);
                            }
                        }}>
                    </FormControl>
                </Col>
                <Col xs={1}>
                    <Button className="float-end" variant="danger">
                        <i onClick={(_) => this.props.onDeleteStep(idx, this.props.component)} className="fas fa-trash-alt"></i>
                    </Button>
                </Col>
            </Row>
        )
    }

    stepEdit(): React.ReactNode {
        return (
            <Form>

                <ReactSortable
                    list={this.props.component!.steps!}
                    setList={newSteps => {
                        this.props.onChange(newSteps);
                        // this.props.component!.steps! = newSteps
                    }}
                    handle=".drag-handle"
                >
                    {this.props.multipart ?
                        this.props.component!.steps?.map((i, idx) => this.stepEditRow(i, idx)) :
                        (this.props.recipe as Recipe).steps?.map((i, idx) => this.stepEditRow(i, idx))}
                </ReactSortable>
                <Col xs={12}>
                    <Button
                        variant="outline-primary"
                        className="width-100 margin-bottom-10"
                        onClick={(_) => this.props.onNewStep()}>New step</Button>
                </Col>
            </Form>
        )
    }

    render() {
        if (this.props.edit) {
            return this.stepEdit();
        } else if (!this.props.multipart) {
            return (this.props.recipe as Recipe).steps?.map((step, index) => {
                return (
                    <Row>
                        <Col className="step-number">{index + 1}</Col>
                        <Col className="margin-bottom-12" key={index}>
                            <Step
                                multipart={this.props.multipart}
                                recipe={this.props.recipe}
                                stepText={step}
                                newServings={this.props.newServings} />
                        </Col>
                    </Row>
                )
            });
        } else {
            return (this.props.component?.steps?.map((step, index) => {
                return (
                    <Row>
                        <Col className="step-number">{index + 1}</Col>
                        <Col className="margin-bottom-12" key={index}>
                            <Step
                                multipart={this.props.multipart}
                                recipe={this.props.recipe}
                                stepText={step}
                                component={this.props.component}
                                newServings={this.props.newServings} />
                        </Col>
                    </Row>
                )
            }));
        }
    }
}