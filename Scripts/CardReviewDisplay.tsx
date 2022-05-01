import React from "react";
import ReactDOM from "react-dom";
import Rating from "react-rating";

type CardReviewDisplayProps = {
    averageReviews : number,
    reviewCount : number
}

class CardReviewDisplay extends React.Component<CardReviewDisplayProps, {}> {
    constructor(props : CardReviewDisplayProps) {
        super(props);
    }

    render() {
        return (
            <div>
                <Rating
                    initialRating={this.props.averageReviews}
                    readonly
                    emptySymbol="far fa-star"
                    fullSymbol="fas fa-star"
                    fractions={2} />
                ({this.props.reviewCount})
            </div>
        )
    }
}

var recipeCards = document.querySelectorAll(".recipe-card")
recipeCards.forEach(recipeCard => {
    let averageReviews = Number.parseFloat(recipeCard.getAttribute("data-average-reviews") as string)
    let reviewCount = Number.parseInt(recipeCard.getAttribute("data-review-count") as string)
    let recipeId = recipeCard.getAttribute("data-recipe-id") as string
    var target = recipeCard.querySelector(`#${recipeId}-rating-target`)
    if (target !== null) {
        ReactDOM.render(
            <CardReviewDisplay averageReviews={averageReviews} reviewCount={reviewCount} />,
            target)
    }
});