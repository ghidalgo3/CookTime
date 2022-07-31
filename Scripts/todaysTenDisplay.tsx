import React from "react"

type TodaysTenDisplayProps = {
    todaysTen : DietDetail
}

export class TodaysTenDisplay extends React.Component<TodaysTenDisplayProps, {}> {

    details: TodaysTenDetails;

    constructor(props : TodaysTenDisplayProps) {
        super(props);
        this.details = props.todaysTen.details as TodaysTenDetails
    }

    render() {
        let d = this.details!
        return (
            <ul>
                <li>{d.hasFruits ? "Fruits" : null}</li>
                <li>{d.hasVegetables ? "Vegetables" : null}</li>
                <li>{d.hasCruciferousVegetables ? "Cruciferous vegetables" : null}</li>
                <li>{d.hasBeans ? "Beans" : null}</li>
                <li>{d.hasHerbsAndSpices ? "Herbs and Spices" : null}</li>
                <li>{d.hasNutsAndSeeds ? "Nuts and Seeds" : null}</li>
                <li>{d.hasGrains ? "Grains" : null}</li>
                <li>{d.hasFlaxseeds ? "Flaxseeds" : null}</li>
                <li>{d.hasBerries ? "Berries" : null}</li>
                <li>{d.hasGreens ? "Greens" : null}</li>
            </ul>);
    }
}
