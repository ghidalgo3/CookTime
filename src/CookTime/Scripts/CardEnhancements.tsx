import React from "react";
import { Button, Spinner } from "react-bootstrap";
import ReactDOM from "react-dom";
import Rating from "react-rating";

type CardImageProps = {
    images: Image[],
    initialFavorite?: boolean | null
    recipeId: string
}

type CardImageState = {
    isFavorite?: boolean | undefined | null,
    operationInProgress: boolean
}


type ReviewDisplayProps = {
    averageReviews: number,
    reviewCount: number
}

class ReviewDisplay extends React.Component<ReviewDisplayProps, {}> {
    constructor(props: ReviewDisplayProps) {
        super(props);
    }

    render() {
        return (
            <div className="margin-top-8">
                <Rating
                    initialRating={this.props.averageReviews}
                    readonly
                    emptySymbol="bi bi-star"
                    fullSymbol="bi bi-star-fill"
                    fractions={2} />
                {"   "} ({this.props.reviewCount})
            </div>
        )
    }
}

/*
This adds interactivity to cards
*/
var recipeCards = document.querySelectorAll(".recipe-card")
recipeCards.forEach(recipeCard => {
    let averageReviews = Number.parseFloat(recipeCard.getAttribute("data-average-reviews") as string)
    let reviewCount = Number.parseInt(recipeCard.getAttribute("data-review-count") as string)
    let recipeId = recipeCard.getAttribute("data-recipe-id") as string
    let isFavorite = recipeCard.getAttribute("data-is-favorite") as string
    var ratingTarget = recipeCard.querySelector(`#rating-target-${recipeId}`)
    if (ratingTarget !== null) {
        ReactDOM.render(
            <ReviewDisplay
                averageReviews={averageReviews}
                reviewCount={reviewCount} />,
            ratingTarget)
    }

    var imageTarget = recipeCard.querySelector(`#image-target-${recipeId}`)
    if (imageTarget !== null) {
        var imageId = imageTarget.getAttribute("data-image-id") as string;
        ReactDOM.render(
            <CardImage
                recipeId={recipeId}
                initialFavorite={isFavorite == '' ? null : isFavorite == "True"}
                images={[{ id: imageId, name: "" }]} />,
            imageTarget)
    }
});