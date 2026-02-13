import React from "react";
import { Button, Col, Form, FormControl, Row } from "react-bootstrap";
import { Step } from "./RecipeStep";
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, DragEndEvent } from "@dnd-kit/core";
import { SortableContext, sortableKeyboardCoordinates, useSortable, verticalListSortingStrategy, arrayMove } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { MeasureUnit, MultiPartRecipe, Recipe, RecipeComponent } from "src/shared/CookTime";
import { UnitPreference } from "src/shared/units";

type RecipeStepListProps = {
    recipe: Recipe | MultiPartRecipe,
    newServings: number,
    unitPreference: UnitPreference,
    units: MeasureUnit[],
    multipart: boolean,
    component?: RecipeComponent,
    onDeleteStep: (i: number, component?: RecipeComponent) => void,
    onChange: (newSteps: string[]) => void,
    onNewStep: () => void,
    edit: boolean
}

type SortableStepProps = {
    id: string,
    index: number,
    stepText: string,
    onTextChange: (index: number, value: string) => void,
    onDelete: (index: number) => void
}

function SortableStep({ id, index, stepText, onTextChange, onDelete }: SortableStepProps) {
    const { attributes, listeners, setNodeRef, transform, transition } = useSortable({ id });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
    };

    return (
        <div ref={setNodeRef} style={style}>
            <Row>
                <Col className="col d-flex align-items-center get-smaller">
                    <i
                        className="margin-right-10 drag-handle bi bi-grip-vertical"
                        style={{ cursor: 'grab' }}
                        {...attributes}
                        {...listeners}
                    />
                    <FormControl
                        as="textarea"
                        rows={4}
                        className="margin-bottom-8"
                        type="text"
                        placeholder="Recipe step"
                        value={stepText}
                        onChange={e => onTextChange(index, e.target.value)}
                    />
                </Col>
                <Col xs={1}>
                    <Button className="float-end" variant="danger">
                        <i onClick={() => onDelete(index)} className="bi bi-trash"></i>
                    </Button>
                </Col>
            </Row>
        </div>
    );
}

export function RecipeStepList(props: RecipeStepListProps) {
    const { recipe, multipart, component, newServings, unitPreference, units, edit, onChange, onDeleteStep, onNewStep } = props;

    const sensors = useSensors(
        useSensor(PointerSensor),
        useSensor(KeyboardSensor, {
            coordinateGetter: sortableKeyboardCoordinates,
        })
    );

    const steps = multipart ? (component?.steps ?? []) : ((recipe as Recipe).steps ?? []);

    // Create stable IDs for sortable items (using index as fallback since steps are strings)
    const itemIds = steps.map((_, idx) => `step-${idx}`);

    function handleDragEnd(event: DragEndEvent) {
        const { active, over } = event;
        if (over && active.id !== over.id) {
            const oldIndex = itemIds.indexOf(active.id as string);
            const newIndex = itemIds.indexOf(over.id as string);
            const newSteps = arrayMove([...steps], oldIndex, newIndex);
            onChange(newSteps);
        }
    }

    function handleTextChange(index: number, value: string) {
        const newSteps = [...steps];
        newSteps[index] = value;
        onChange(newSteps);
    }

    function handleDelete(index: number) {
        onDeleteStep(index, component);
    }

    if (edit) {
        return (
            <Form>
                <DndContext
                    sensors={sensors}
                    collisionDetection={closestCenter}
                    onDragEnd={handleDragEnd}
                >
                    <SortableContext items={itemIds} strategy={verticalListSortingStrategy}>
                        {steps.map((stepText, idx) => (
                            <SortableStep
                                key={itemIds[idx]}
                                id={itemIds[idx]}
                                index={idx}
                                stepText={stepText}
                                onTextChange={handleTextChange}
                                onDelete={handleDelete}
                            />
                        ))}
                    </SortableContext>
                </DndContext>
                <Col xs={12}>
                    <Button
                        variant="outline-primary"
                        className="width-100 margin-bottom-10"
                        onClick={() => onNewStep()}>New step</Button>
                </Col>
            </Form>
        );
    } else if (!multipart) {
        return (
            <>
                {(recipe as Recipe).steps?.map((step, index) => (
                    <Row key={index}>
                        <Col className="step-number">{index + 1}</Col>
                        <Col className="margin-bottom-12">
                            <Step
                                multipart={multipart}
                                recipe={recipe}
                                stepText={step}
                                unitPreference={unitPreference}
                                units={units}
                                newServings={newServings} />
                        </Col>
                    </Row>
                ))}
            </>
        );
    } else {
        return (
            <>
                {component?.steps?.map((step, index) => (
                    <Row key={index}>
                        <Col className="step-number">{index + 1}</Col>
                        <Col className="margin-bottom-12">
                            <Step
                                multipart={multipart}
                                recipe={recipe}
                                stepText={step}
                                component={component}
                                unitPreference={unitPreference}
                                units={units}
                                newServings={newServings} />
                        </Col>
                    </Row>
                ))}
            </>
        );
    }
}
