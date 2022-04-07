import React from "react";

export class NutritionFacts extends React.Component<NutritionFactVector, {}>
{
    constructor(props: NutritionFactVector) {
        super(props);
    }

    render() {
        let {
            calories,
            carbohydrates,
            proteins,
            polyUnsaturatedFats,
            monoUnsaturatedFats,
            saturatedFats,
            sugars,
            transFats
        } = this.props;
        return (
            <div className="nf-body performance-facts">
                <header className="performance-facts__header">
                    <h1 className="performance-facts__title">Nutrition Facts</h1>
                    {/* <p className="nf-p">Serving Size 1/2 cup (about 82g)</p>
                        <p className="nf-p">Serving Per Container 8</p> */}
                </header>
                <table className="performance-facts__table">
                    <thead>
                        <tr>
                            <th colSpan={3} className="small-info">
                                Amount Per Serving
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <th colSpan={2}>
                                <b>Calories </b>
                                {calories} 
                            </th>
                            <td>
                                Calories from Fat {Math.round(9 * (monoUnsaturatedFats + polyUnsaturatedFats + saturatedFats))}
                            </td>
                        </tr>
                        <tr className="thick-row">
                            <td colSpan={3} className="thick-row small-info">
                                <b>% Daily Value*</b>
                            </td>
                        </tr>
                        <tr>
                            <th colSpan={2}>
                                <b>Total Fat </b>
                                {monoUnsaturatedFats + polyUnsaturatedFats + saturatedFats}g
                            </th>
                            <td>
                                <b>{Math.round(monoUnsaturatedFats + polyUnsaturatedFats + saturatedFats / 65)}%</b>
                            </td>
                        </tr>
                        <tr>
                            <td className="blank-cell">
                            </td>
                            <th>
                                Saturated Fat {saturatedFats}g
                            </th>
                            <td>
                                <b>{Math.round(saturatedFats / 65)}%</b>
                            </td>
                        </tr>
                        <tr>
                            <td className="blank-cell">
                            </td>
                            <th>
                                Trans Fat {transFats}g
                            </th>
                            <td>
                                <b>{Math.round(transFats / 65)}%</b>
                            </td>
                        </tr>
                        <tr>
                            {/* <th colSpan={2}>
                                <b>Cholesterol </b>
                                55mg
                            </th>
                            <td>
                                <b>18%</b>
                            </td> */}
                        </tr>
                        <tr>
                            {/* <th colSpan={2}>
                                <b>Sodium </b>
                                40mg
                            </th>
                            <td>
                                <b>2%</b>
                            </td> */}
                        </tr>
                        <tr>
                            <th colSpan={2}>
                                <b>Total Carbohydrate </b>
                                {carbohydrates}g
                            </th>
                            <td>
                                <b>{Math.round(carbohydrates / 300)}%</b>
                            </td>
                        </tr>
                        <tr>
                            {/* <td className="blank-cell">
                            </td>
                            <th>
                                Dietary Fiber 
                                1g
                            </th>
                            <td>
                                <b>4%</b>
                            </td> */}
                        </tr>
                        <tr>
                            <td className="blank-cell">
                            </td>
                            <th>
                                Sugars {sugars}g
                            </th>
                            <td>
                            </td>
                        </tr>
                        <tr className="thick-end">
                            <th colSpan={2}>
                                <b>Protein </b>
                                {proteins}g
                            </th>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>

                {/* <table className="performance-facts__table performance-facts__table__grid">
                    <tbody>
                        <tr>
                            <td colSpan={2}>
                                Vitamin A 
                                10%
                            </td>
                            <td className="nf-td-lastchild" style={{textAlign: "left"}}>
                                Vitamin C 
                                0%
                            </td>
                        </tr>
                        <tr className="thin-end">
                            <td colSpan={2}>
                                Calcium 
                                10%
                            </td>
                            <td className="nf-td-lastchild" style={{textAlign: "left"}}>
                                Iron 
                                6%
                            </td>
                        </tr>
                    </tbody>
                </table> */}

                <p className="nf-p small-info">* Percent Daily Values are based on a 2,000 calorie diet. Your daily values may be higher or lower depending on your calorie needs:</p>

                <table className="performance-facts__table performance-facts__table__small small-info">
                    <thead>
                        <tr className="nf-thead-tr">
                            <td className="nf-small-td-lastchild" style={{border: 0, padding: 0}} colSpan={2}></td>
                            <th className="nf-small-th-and-td" style={{border: 0, padding: 0}}>Calories:</th>
                            <th className="nf-small-th-and-td" style={{border: 0, padding: 0}}>2,000</th>
                            <th className="nf-small-th-and-td" style={{border: 0, padding: 0}}>2,500</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <th className="nf-small-th-and-td" colSpan={2} style={{border: 0, padding: 0}}>Total Fat</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}}>Less than</td>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}}>65g</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild" style={{border: 0, padding: 0, textAlign: 'left'}}>80g</td>
                        </tr>
                        <tr >
                            <td className="nf-small-th-and-td blank-cell" style={{border: 0, padding: 0}} ></td>
                            <th className="nf-small-th-and-td" style={{border: 0, padding: 0}} >Saturated Fat</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >Less than</td>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >20g</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild" style={{border: 0, padding: 0, textAlign: 'left'}} >25g</td>
                        </tr>
                        <tr>
                            <th className="nf-small-th-and-td" style={{border: 0, padding: 0}} colSpan={2}>Cholesterol</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >Less than</td>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >300mg</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild" style={{border: 0, padding: 0, textAlign: 'left'}} >300 mg</td>
                        </tr>
                        <tr>
                            <th className="nf-small-th-and-td" colSpan={2} style={{border: 0, padding: 0}} >Sodium</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >Less than</td>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >2,400mg</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild" style={{border: 0, padding: 0, textAlign: 'left'}} >2,400mg</td>
                        </tr>
                        <tr>
                            <th className="nf-small-th-and-td" colSpan={3} style={{border: 0, padding: 0}} >Total Carbohydrate</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >300g</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild " style={{border: 0, padding: 0, textAlign: 'left'}} >375g</td>
                        </tr>
                        <tr>
                            <td className="nf-small-th-and-td blank-cell" style={{border: 0, padding: 0}} ></td>
                            <th className="nf-small-th-and-td" colSpan={2} style={{border: 0, padding: 0}} >Dietary Fiber</th>
                            <td className="nf-small-th-and-td" style={{border: 0, padding: 0}} >25g</td>
                            <td className="nf-small-th-and-td nf-small-td-lastchild" style={{border: 0, padding: 0, textAlign: 'left'}} >30g</td>
                        </tr>
                    </tbody>
                </table>

                <p className="nf-p small-info">
                    Calories per gram:
                </p>
                <p className="nf-p small-info nf-text-center">
                    Fat 9
                    &bull;
                    Carbohydrate 4
                    &bull;
                    Protein 4
                </p>
            
            </div>
        )
    }
}