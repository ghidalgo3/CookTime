import React, { useCallback, useEffect, useRef } from "react";
import { Col, Row, Spinner } from "react-bootstrap";
import "./InfiniteScrollList.css";

interface InfiniteScrollListProps<T> {
  items: T[];
  element: (item: T) => React.ReactNode;
  hasMore: boolean;
  isLoading: boolean;
  onLoadMore: () => void;
  colClassName?: string;
}

export default function InfiniteScrollList<T>({
  items,
  element,
  hasMore,
  isLoading,
  onLoadMore,
  colClassName,
}: InfiniteScrollListProps<T>) {
  const sentinelRef = useRef<HTMLDivElement>(null);

  const handleIntersection = useCallback(
    (entries: IntersectionObserverEntry[]) => {
      const [entry] = entries;
      if (entry.isIntersecting && hasMore && !isLoading) {
        onLoadMore();
      }
    },
    [hasMore, isLoading, onLoadMore]
  );

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(handleIntersection, {
      root: null,
      rootMargin: "200px",
      threshold: 0,
    });

    observer.observe(sentinel);

    return () => {
      observer.disconnect();
    };
  }, [handleIntersection]);

  return (
    <>
      <Row>
        {items.map((item, idx) => (
          <Col sm="4" className={colClassName} key={idx}>
            {element(item)}
          </Col>
        ))}
      </Row>

      <div ref={sentinelRef} className="infinite-scroll-sentinel">
        {isLoading && (
          <div className="infinite-scroll-loading">
            <Spinner animation="border" role="status">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
          </div>
        )}
      </div>

      {!hasMore && items.length > 0 && (
        <div className="infinite-scroll-end">
          <p className="text-muted">You've reached the end!</p>
        </div>
      )}
    </>
  );
}
