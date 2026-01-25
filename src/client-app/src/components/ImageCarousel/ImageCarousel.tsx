import React, { useState } from "react";
import { Carousel } from "react-bootstrap";
import { Image } from "src/shared/CookTime";
import imgs from "src/assets";
import "./ImageCarousel.css";

interface ImageCarouselProps {
  images: Image[];
  fallbackImage?: string;
  className?: string;
  showControls?: boolean;
  showIndicators?: boolean;
}

export function ImageCarousel({
  images,
  fallbackImage = imgs.placeholder,
  className = "",
  showControls = true,
  showIndicators = true,
}: ImageCarouselProps) {
  const [index, setIndex] = useState(0);

  const handleSelect = (selectedIndex: number) => {
    setIndex(selectedIndex);
  };

  // If no images, show fallback
  if (images.length === 0) {
    return (
      <div className={`image-carousel ${className}`}>
        <img
          loading="lazy"
          className="carousel-image"
          src={fallbackImage}
          alt="Placeholder"
        />
      </div>
    );
  }

  // If only one image, show it without carousel controls
  if (images.length === 1) {
    const imageUrl = images[0].url || fallbackImage;
    return (
      <div className={`image-carousel ${className}`}>
        <img
          loading="lazy"
          className="carousel-image"
          src={imageUrl}
          alt="Recipe"
        />
      </div>
    );
  }

  // Multiple images - show carousel
  return (
    <Carousel
      activeIndex={index}
      onSelect={handleSelect}
      controls={showControls}
      indicators={showIndicators}
      className={`image-carousel ${className}`}
      interval={null}
    >
      {images.map((image, i) => (
        <Carousel.Item key={image.id || i}>
          <img
            loading="lazy"
            className="carousel-image d-block w-100"
            src={image.url || fallbackImage}
            alt={`Recipe image ${i + 1}`}
          />
        </Carousel.Item>
      ))}
    </Carousel>
  );
}

export default ImageCarousel;
