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
    operationInProgress : boolean
}

class CardImage extends React.Component<CardImageProps, CardImageState> {
    constructor(props: CardImageProps) {
        super(props);
        this.state = {
            isFavorite: this.props.initialFavorite,
            operationInProgress: false
        }
    }

    render() {
        let image = (this.props.images.length == 0 || this.props.images[0].id == null || this.props.images[0].id == "null") ?
            "/placeholder.jpg" :
            `/image/${this.props.images[0].id}?width=300`;
        return (
            <div className="cr-image-parent">
                <a href={`/Recipes/Details?id=${this.props.recipeId}`}>
                    <img
                        className="card-img-top card-recipe-image"
                        src={image}
                        alt="Food image">
                    </img>
                </a>
                {
                    this.state.isFavorite != null ?
                        this.button() :
                        null
                }
            </div>
        )
    }
    private button() {
        var heartClass = "";
        if (this.state.isFavorite) {
            heartClass = "fas fa-heart fa-2x"
        } else {
            heartClass = "far fa-heart fa-2x"
        }
        return (
        <Button className="favorite-button" onClick={e => this.toggleFavorite()}>
                {this.state.operationInProgress ?
                    <Spinner
                        as="span"
                        animation="border"
                        
                        role="status"
                        aria-hidden="true"
                    />
                    : <i className={heartClass}></i>}
            </Button>
        )
    }

    toggleFavorite(): void {
        this.setState({operationInProgress: true});
        if (this.state.isFavorite) {
            fetch(`/api/MultiPartRecipe/${this.props.recipeId}/favorite`,
            {
                method: "DELETE",
            }).then(response => {
                if (response.ok) {
                    this.setState({isFavorite: false, operationInProgress: false})
                }
            })
        } else {
            fetch(`/api/MultiPartRecipe/${this.props.recipeId}/favorite`,
            {
                method: "PUT",
            }).then(response => {
                if (response.ok) {
                    this.setState({isFavorite: true, operationInProgress: false})
                }
            })
        }
    }
}

type ReviewDisplayProps = {
    averageReviews : number,
    reviewCount : number
}

class ReviewDisplay extends React.Component<ReviewDisplayProps, {}> {
    constructor(props : ReviewDisplayProps) {
        super(props);
    }

    render() {
        return (
            <div className="margin-top-8">
                <Rating
                    initialRating={this.props.averageReviews}
                    readonly
                    emptySymbol="far fa-star"
                    fullSymbol="fas fa-star"
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
                images={[{id: imageId, name: ""}]} />,
            imageTarget)
    }
});