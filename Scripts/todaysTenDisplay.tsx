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
            <div className="todays-tens-container">
                {this.topTenImage("beans", d.hasBeans)}
                {this.topTenImage("whole-grains", d.hasGrains)}
                {this.topTenImage("greens", d.hasGreens)}
                {this.topTenImage("cruciferous", d.hasCruciferousVegetables)}
                {this.topTenImage("other-vegetables", d.hasVegetables)}
                {this.topTenImage("nuts", d.hasNutsAndSeeds)}
                {this.topTenImage("berries", d.hasBerries)}
                {this.topTenImage("other-fruit", d.hasFruits)}
                {this.topTenImage("flaxseed", d.hasFlaxseeds)}
                {this.topTenImage("spices", d.hasHerbsAndSpices)}
            </div>);
    }

    topTenImage(imageName : string, present : boolean) {
        return <img className={`todays-tens-symbols ${present ? "" : "absent"}`} alt={imageName} title={imageName} src={`/${imageName}.png`}></img>;
    }
}
